using System.Text.Json;
using DotnetJsOTProcessor;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        .WithOrigins("http://localhost:8080")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

var app = builder.Build();

app.UseCors("CorsPolicy");

app.MapGet("/", () => "Hello World!");

app.MapPost("/traces", async (context) =>
{
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var request = JsonSerializer.Deserialize<TracesData>(requestBody, options);
    if (request?.ResourceSpans != null)
    {
        HandleTrace.Process(request.ResourceSpans);
    }
});


app.Run();