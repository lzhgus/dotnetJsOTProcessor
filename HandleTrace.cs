using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DotnetJsOTProcessor;

using System.Collections.Concurrent;
using System.Diagnostics;

public static class HandleTrace
{
    private static ConcurrentDictionary<ActivitySpanId, (ActivitySpanId, ActivityTraceId, ActivityTraceId)> _spanMap =
        new ConcurrentDictionary<ActivitySpanId, (ActivitySpanId, ActivityTraceId, ActivityTraceId)>();
    public static void Process(List<ResourceSpans> resourceSpansList)
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MySample"))
            .AddSource("unknown_service")
            .AddConsoleExporter()
            .AddOtlpExporter(o=> o.Endpoint = new Uri("http://localhost:4317"))
            .Build();
        
        foreach (var resourceSpans in resourceSpansList)
        {
            var source = resourceSpans.Resource!.Attributes!.Where(x => x.Key == "service.name")
                .Select(x => x.Value!.StringValue).FirstOrDefault() ?? "UI.Web";

            foreach (var scopeSpan in resourceSpans.ScopeSpans!)
            {
                foreach (var span in scopeSpan.Spans!)
                {
                    var name = span.Name ?? "UI-Activity";
                    var activitySource = new ActivitySource(source);
                    var traceId = ActivityTraceId.CreateFromString(span.TraceId.AsSpan());
                    var spanId = ActivitySpanId.CreateFromString(span.SpanId.AsSpan());
                    var parentSpanId = span.ParentSpanId == null
                        ? default
                        : ActivitySpanId.CreateFromString(span.ParentSpanId.AsSpan());

                    var originalActivity = Activity.Current;
                    Activity.Current = null;

                    Activity? activity;
                    if (parentSpanId != default && _spanMap.TryGetValue(parentSpanId, out var parentSpan))
                    {
                        var parentContext = new ActivityContext(parentSpan.Item3, parentSpan.Item1, ActivityTraceFlags.Recorded);
                        activity = activitySource.StartActivity(name, ActivityKind.Internal, parentContext);
                    }
                    else
                    {
                        activity = activitySource.StartActivity(name, ActivityKind.Internal);
                    }

                    if (activity == null)
                    {
                        continue;
                    }

                    _spanMap.TryAdd(spanId, (activity.SpanId, traceId, activity.TraceId));

                    activity.SetIdFormat(ActivityIdFormat.W3C);
                    activity.SetEndTime(DateTime.UtcNow +
                                        TimeSpan.FromMilliseconds((span.EndTimeUnixNano - span.StartTimeUnixNano) /
                                                                  1_000_000));
                    activity.AddTag("status_code", span.Status!.Code.ToString());

                    foreach (var attribute in span.Attributes!)
                    {
                        activity.AddTag(attribute.Key!, attribute.Value?.StringValue);
                    }

                    // Add all the span's events to the activity.
                    if (span.Events != null)
                    {
                        foreach (var otlpEvent in span.Events!)
                        {
                            var timestamp = UnixNanoToDateTimeOffset(otlpEvent.TimeUnixNano);
                            var tags = new ActivityTagsCollection(otlpEvent.Attributes?
                                .Select(a => new KeyValuePair<string, object?>(a.Key!, a.Value?.StringValue))!);
                            activity.AddEvent(new ActivityEvent(otlpEvent.Name!, timestamp, tags));
                        }
                    }

                    activity.Stop();
                    Activity.Current = originalActivity;
                }
            }
        }
    }

    private static DateTimeOffset UnixNanoToDateTimeOffset(ulong unixNano)
    {
        var unixTime = unixNano / 1_000_000_000.0;
        return DateTimeOffset.FromUnixTimeSeconds((long)unixTime);
    }
}
