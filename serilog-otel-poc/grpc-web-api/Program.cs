using System.Diagnostics;
using System.Diagnostics.Metrics;
using grpc_web_api.Services;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();


builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource(DiagnosticsConfig.ActivitySource.Name)
            .ConfigureResource(resource => resource
                .AddService(DiagnosticsConfig.ServiceName))
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://127.0.0.1:4318/v1/traces");
                opt.Protocol = OtlpExportProtocol.HttpProtobuf;
            }))
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


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

public static class DiagnosticsConfig
{
    public const string ServiceName = "ASP.NET Grp Server";
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);
    
    public static readonly Meter Meter = new(ServiceName);
    public static readonly Counter<long> RequestCounter =
        Meter.CreateCounter<long>("app.grpc_request_counter");

}
