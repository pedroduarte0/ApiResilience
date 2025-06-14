namespace ApiClient.Dtos
{
    public record WeatherDto(
        DateOnly Date,
        int TemperatureC,
        int TemperatureF,
        string? Summary);
}
