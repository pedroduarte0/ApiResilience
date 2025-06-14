
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;

namespace ApiClient
{
    // References:
    //  https://learn.microsoft.com/en-us/dotnet/core/resilience/?tabs=dotnet-cli
    //  https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli
    //  https://www.pollydocs.org/strategies/

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var httpClientBuilder = builder.Services.AddHttpClient<WeatherApiClient>();
            httpClientBuilder.AddStandardResilienceHandler(
                options =>
                {
                    //options.Retry.DisableFor(HttpMethod.Post, HttpMethod.Delete);
                    
                    options.Retry.DisableForUnsafeHttpMethods();    // the DisableForUnsafeHttpMethods(HttpRetryStrategyOptions) disables retries for POST, PATCH, PUT, DELETE, and CONNECT requests.
                });

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        
        // Alternative to the standard resilience where the resilience strategies are explicitely set.
        public static void MainWithSelectiveResilience(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var httpClientBuilder = builder.Services.AddHttpClient<WeatherApiClient>();

            // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli#add-custom-resilience-handlers
            httpClientBuilder.AddResilienceHandler(
                "CustomPipeline",

                static builder =>
                {
                    // See: https://www.pollydocs.org/strategies/retry.html
                    var retryOptions = new HttpRetryStrategyOptions
                    {
                        // Customize and configure the retry logic.
                        BackoffType = DelayBackoffType.Exponential,
                        MaxRetryAttempts = 5,
                        UseJitter = true
                    };

                    retryOptions.DisableForUnsafeHttpMethods();
                    //retryOptions.DisableFor(HttpMethod.Post, HttpMethod.Delete);

                    builder.AddRetry(retryOptions);

                    // See: https://www.pollydocs.org/strategies/circuit-breaker.html
                    builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                    {
                        // Customize and configure the circuit breaker logic.
                        SamplingDuration = TimeSpan.FromSeconds(10),
                        FailureRatio = 0.2,
                        MinimumThroughput = 3,
                        ShouldHandle = static args =>
                        {
                            return ValueTask.FromResult(args is
                            {
                                Outcome.Result.StatusCode:
                                HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
                            });
                        }
                    });

                    // See: https://www.pollydocs.org/strategies/timeout.html
                    builder.AddTimeout(TimeSpan.FromSeconds(5));
                });

            builder.Services.AddControllers();
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
