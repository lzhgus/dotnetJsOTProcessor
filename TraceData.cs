using System.Text.Json.Serialization;

namespace DotnetJsOTProcessor;

public class TracesData
{
    [JsonPropertyName("resourceSpans")]
    public List<ResourceSpans>? ResourceSpans { get; set; }
}

public class ResourceSpans
{
    public Resource? Resource { get; set; }
    public List<ScopeSpans>? ScopeSpans { get; set; }
}

public class Resource
{
    public List<KeyValue>? Attributes { get; set; }
    public int DroppedAttributesCount { get; set; }
}

public class KeyValue
{
    public string? Key { get; set; }
    public Value? Value { get; set; }
}

public class Value
{
    public string? StringValue { get; set; }
}

public class ScopeSpans
{
    public InstrumentationScope? Scope { get; set; }
    public List<Span>? Spans { get; set; }
}

public class InstrumentationScope
{
    public string? Name { get; set; }
}

public class Span
{
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }
    public string? ParentSpanId { get; set; }
    public string? TraceState { get; set; }
    public string? Name { get; set; }
    public int Kind { get; set; }
    public long StartTimeUnixNano { get; set; }
    public long EndTimeUnixNano { get; set; }
    public List<KeyValue>? Attributes { get; set; }
    public int DroppedAttributesCount { get; set; }
    public List<Event>? Events { get; set; }
    public int DroppedEventsCount { get; set; }
    public Status? Status { get; set; }
    public List<Link>? Links { get; set; }
    public int DroppedLinksCount { get; set; }
}

public class Event
{
    public List<KeyValue>? Attributes { get; set; }
    public string? Name { get; set; }
    public ulong TimeUnixNano { get; set; }
    public int DroppedAttributesCount { get; set; }
}

public class Link
{
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }
    public string? TraceState { get; set; }
    public List<KeyValue>? Attributes { get; set; }
    public int DroppedAttributesCount { get; set; }
}

public class Status
{
    public int Code { get; set; }
}
