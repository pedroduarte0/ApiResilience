using ApiClient.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ApiClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiWithResilienceController : ControllerBase
    {
        private readonly WeatherApiClient _weatherApiClient;

        public ApiWithResilienceController(WeatherApiClient weatherApiClient)
        {
            _weatherApiClient = weatherApiClient;
        }

        [HttpGet(Name = "GetWeatherAsync")]
        [ProducesResponseType<IEnumerable<WeatherDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WeatherDto>>> GetWeatherAsync()
        {
            // Normallly would call the API client on a service not from the controller directly.
            var result = await _weatherApiClient.GetWeatherAsync();
            return Ok(result);
        }
    }
}
