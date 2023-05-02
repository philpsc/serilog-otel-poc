import React, { useState } from "react";
import './App.css';
import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { W3CTraceContextPropagator } from "@opentelemetry/core";
import { Resource } from '@opentelemetry/resources';
import { SemanticResourceAttributes  } from '@opentelemetry/semantic-conventions'

import { SimpleSpanProcessor, ConsoleSpanExporter } from '@opentelemetry/sdk-trace-web';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-proto';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
import { UserInteractionInstrumentation } from '@opentelemetry/instrumentation-user-interaction';
import { XMLHttpRequestInstrumentation } from '@opentelemetry/instrumentation-xml-http-request';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { ZoneContextManager } from '@opentelemetry/context-zone';
import opentelemetry from "@opentelemetry/api";

import axios from 'axios';

const BACKEND_URL = 'https://localhost:7234/weatherforecast/withTracing'
const OTEL_COLLECTOR_URL = 'http://127.0.0.1:4318/v1/traces'

function App() {
  // Boilerplate code to display a button and call the backend on button clicks
  const [data, setData] = useState("");
  const handleClick = async () => {
    const response = await axios.get(BACKEND_URL);
    setData(JSON.stringify(response.data));
  };

  // Configure the tracer to export to a collector
  const collectorOptions = {
    url: OTEL_COLLECTOR_URL
  };
  
  const provider = new WebTracerProvider({
    resource: new Resource({
      [SemanticResourceAttributes.SERVICE_NAME]: 'SPA frontend'
    })
  });

  provider.addSpanProcessor(new SimpleSpanProcessor(new OTLPTraceExporter(collectorOptions)));
  provider.addSpanProcessor(new SimpleSpanProcessor(new ConsoleSpanExporter()));
  provider.register({
    contextManager: new ZoneContextManager(),
    propagator: new W3CTraceContextPropagator(),
  });    

  // Registering instrumentations
  registerInstrumentations({
      instrumentations: [
          new UserInteractionInstrumentation(),
          new DocumentLoadInstrumentation(),
          new XMLHttpRequestInstrumentation({
            propagateTraceHeaderCorsUrls: [
              BACKEND_URL,
            ],
      
          }),
      ],
  });

return (
    <div className="App">
      <div >
        <button className="my-button" onClick={handleClick}>Trigger http request</button>
        {data && <div>{data}</div>}
      </div>
    </div>
  );
}

export default App;
