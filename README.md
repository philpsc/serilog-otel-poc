# serilog-otel-poc
Proof of Concept application and configuration for instrumenting C# code to emit logs, metrics and traces to observability backends using OpenTelemetry. Metrics and traces are created using the OpenTelemetry SDK. Logs are structured and enriched using Serilog, then mapped to the OpenTelemetry log format and emitted and sent using the serilog OpenTelemetry sink (see https://github.com/serilog/serilog-sinks-opentelemetry).

## React frontend
The `serilog-otel-poc/frontend` directory contains a React app instrumented to send telemetry data to an OpenTelemetry Collector. It will trace
- User interactions with the app, such as clicks,
- Document load times,
- HTTP/S requests. It will also propagate the context of its current trace to the recipient of the request.

## .NET Core Web API
The `serilog-otel-poc/rest-web-api` directory contains a basic .NET Core Web API project instrumented to send telemetry data to an OpenTelemetry Collector. The telemetry data supported includes logs, metrics and traces. To test each of the instrumentations, there are the following three endpoints:
- `GET /weatherforecast/withlogging`: this endpoint will always produce a 400 status code while also emitting an error log.
- `GET /weatherforecast/withmetrics`: this endpoint will always produce a 200 status code while also counting up a custom metric and emitting the updated count.
- `GET /weatherforecast/withtracing`: this endpoint will always produce a 200 status code while also creating a custom span inside the request's trace.

The app is also instrumented to automatically trace HTTPS requests, to emit metrics generated by the ASP.NET Core framework and to correlate any logs with traces using the ´TracingLogEventEnricher´.

## .NET Core gRPC service
The `serilog-otel-poc/grpc-web-api` directory contains a basic .NET Core gRPC service instrumented to send telemetry data to an OpenTelemetry Collector. The service is instrumented to automatically trace gRPC requests.

## OpenTelementry Collector
The `otel-collector` directory contains a docker environment including an OpenTelemetry Collector as well as a Jaeger tracing backend instance. The Otel collector is also configured to send any telemetry data to a Datadog instance.

## Getting Started
- Enter your Datadog API key. If you don't have one, remove Datadog from the `exporters` config inside `otel-collector-config.yml`.
- Run the docker-compose environment from the otel-collector directory using `docker compose up`.
- Start the React frontend from using `npm start`
- Start the .NET Core Web API from your IDE or CLI.
- Start the .NET Core gRPC service from your IDE or CLI.
- Navigate to `localhost:3000` and interact with the web app. You should be able to see traces in Jaeger. If you have Datadog configured, you should be able to see traces, logs and metrics in Datadog.
- To clean up, shutdown the docker-compose environment using `docker compose down --volumes`.
