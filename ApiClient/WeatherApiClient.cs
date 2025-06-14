using ApiClient.Dtos;
using System.Text.Json;

namespace ApiClient
{
    public class WeatherApiClient
    {
        private readonly HttpClient _httpClient;

        public WeatherApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5098");     // Point to ApiServer (see its launchsettings.json)
        }

        public async Task<IEnumerable<WeatherDto>> GetWeatherAsync()
        {
            var response = await _httpClient.GetAsync("/WeatherForecast");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<WeatherDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Deserialization failed");
        }
    }
}
