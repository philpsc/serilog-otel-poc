using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using serilog_otel_poc.Logging;
using Serilog.Events;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(DiagnosticsConfig.ServiceName)
            .ConfigureResource(resource => resource
                .AddService(DiagnosticsConfig.ServiceName))

            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://127.0.0.1:4318/v1/traces");
                opt.Protocol = OtlpExportProtocol.HttpProtobuf;
            })
            .AddConsoleExporter())
    
            .WithMetrics(metricsProviderBuilder =>
                metricsProviderBuilder
                    .AddMeter(DiagnosticsConfig.Meter.Name)
                    .ConfigureResource(resource => resource
                        .AddService(DiagnosticsConfig.ServiceName))
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(opt =>
                    {
                        opt.Endpoint = new Uri("http://127.0.0.1:4318/v1/metrics");
                        opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                    }));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Host.UseSerilog((_, services, config) => config
    .ReadFrom.Services(services)
    .Enrich.FromLogContext() // Append every entry from the IDiagnosticContext to the log
    .Enrich.With(new TracingLogEventEnricher()) // Append tracing information using the custom Enricher
    .WriteTo.Console(new RenderedCompactJsonFormatter()) // Render logs as JSON
    .WriteTo.OpenTelemetry()
    .MinimumLevel.Debug() // Set minimum log level to Debug
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // Set exception for log level for framework logs
);
    
    



var app = builder.Build();
app.UseSerilogRequestLogging(options =>
{
    // Customize the message template
    options.MessageTemplate = "Handled {RequestPath} {RequestHost} {RequestScheme}";
    
    // Emit debug-level events instead of the defaults
    options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;
    
    // Attach additional properties to the request completion event
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
    };
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();

public static class DiagnosticsConfig
{
    public const string ServiceName = "ASP.NET WebApi";
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);
    
    public static readonly Meter Meter = new(ServiceName);
    public static readonly Counter<long> RequestCounter =
        Meter.CreateCounter<long>("app.rest_request_counter");

}