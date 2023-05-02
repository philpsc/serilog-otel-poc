using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using rest_web_api;

namespace serilog_otel_poc.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private const string GrpcUrl = "https://localhost:7188";

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet("withLogging")]
    public ActionResult<IEnumerable<WeatherForecast>> GetWithLogging()
    {
        _logger.LogInformation("WithLogging endpoint called.");
        _logger.LogError("Yeah that didn't work.");
        return BadRequest();
    }
    
    [HttpGet("withTracing")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetWithTracing()
    {
        _logger.LogInformation("WithTracing endpoint called.");

        using var activity = DiagnosticsConfig.ActivitySource.StartActivity("Weather forecast request with tracing");
        activity?.SetTag("foo", 1);
        activity?.SetTag("bar", "Hello, World!");
        activity?.SetTag("baz", new int[] { 1, 2, 3 });
        
        await CallGrpcService();
        
        return Ok(Get());
    }
    
    [HttpGet("withMetrics")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetWithMetrics()
    {
        _logger.LogInformation("WithMetrics endpoint called.");
        DiagnosticsConfig.RequestCounter.Add(1);
        await CallGrpcService();
        
        return Ok(Get());
    }

    private IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = "Mild"
            })
            .ToArray();
    }

    private async Task CallGrpcService()
    {
        using var channel = GrpcChannel.ForAddress(GrpcUrl);
        var client = new Greeter.GreeterClient(channel);
        var reply = await client.SayHelloAsync(
            new HelloRequest { Name = "GreeterClient" });
        Console.WriteLine("Greeting: " + reply.Message);
    }
}