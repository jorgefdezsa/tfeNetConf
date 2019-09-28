﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Weather
{
    public class WeatherWorker : BackgroundService
    {
        private ILogger<WeatherWorker> _logger;
        private IConfiguration _configuration;
        private IHttpClientFactory _httpClientFactory;
        private IMemoryCache _cache;
        private const string LocationId = "340247";

        public WeatherWorker(ILogger<WeatherWorker> logger,
                            IConfiguration configuration,
                            IHttpClientFactory httpClientFactory,
                            IMemoryCache cache)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();

                    var response = await client.GetAsync($"{_configuration["weather:uri"]}/{LocationId}");
                    var model = await JsonSerializer.DeserializeAsync<Forecast[]>();

                    _cache.Set("WeatherCache", model.First());

                    await Task.Delay(TimeSpan.FromMinutes(10));
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unexpected error fetching weather data: {ex}");
                }
            }
        }
    }
}
