using Microsoft.AspNetCore.Mvc;

namespace serilog_otel_poc.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet("withLogging")]
    public ActionResult<IEnumerable<WeatherForecast>> GetWithLogging()
    {
        _logger.LogError("Yeah that didn't work.");
        return BadRequest();
    }
    
    [HttpGet("withTracing")]
    public ActionResult<IEnumerable<WeatherForecast>> GetWithTracing()
    {
        using var activity = DiagnosticsConfig.ActivitySource.StartActivity("Weather forecast request with tracing");
        activity?.SetTag("foo", 1);
        activity?.SetTag("bar", "Hello, World!");
        activity?.SetTag("baz", new int[] { 1, 2, 3 });

        return Ok(Get());
    }
    
    [HttpGet("withMetrics")]
    public ActionResult<IEnumerable<WeatherForecast>> GetWithMetrics()
    {
        DiagnosticsConfig.RequestCounter.Add(1);
        
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
}