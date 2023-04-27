import React, { useEffect } from 'react';
import logo from './logo.svg';
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

function App() {
    // Configure the tracer to export to a collector
    const collectorOptions = {
      url: 'http://127.0.0.1:4318/v1/traces'
    };
    
    const provider = new WebTracerProvider({
      resource: new Resource({
        [SemanticResourceAttributes.SERVICE_NAME]: "SPA frontend"
    }),

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
                'https://localhost:7234/weatherforecast/withlogging',
              ],
        
            }),
        ],
    });

    callApi();

return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.tsx</code> and save to reload.
        </p>
        <a
          className="App-link"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Hello my dude!
        </a>
      </header>
    </div>
  );
}

async function callApi() {
    const apiUrl = "https://localhost:7234/weatherforecast/withlogging";

    try {
        const response = await axios.get(apiUrl);
        console.log(response.data);
    } catch (error) {
        console.error(error);
        console.error('The trace ID is:' + opentelemetry.trace.getActiveSpan()?.spanContext()?.traceId);
    }
}

export default App;
