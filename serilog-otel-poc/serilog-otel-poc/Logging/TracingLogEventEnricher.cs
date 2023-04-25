using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace serilog_otel_poc.Logging;

public class TracingLogEventEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("traceId", Activity.Current?.Id));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("spanId", Activity.Current?.SpanId));
    }
}