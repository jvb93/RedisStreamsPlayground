using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var redisStreamConsumer = scope.ServiceProvider.GetRequiredService<IRedisStreamConsumer>();
                var settings = scope.ServiceProvider.GetRequiredService<RedisOptions>();
                if (!string.IsNullOrWhiteSpace(settings.ConsumerGroupName))
                {
                    redisStreamConsumer.BeginReadWithConsumerGroup();
                }
                else
                {
                    redisStreamConsumer.BeginRead();
                }

            }
        }

       

        static IServiceProvider BuildServiceProvider()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var serviceCollection = new ServiceCollection();
            var localOptions = new RedisOptions();
            config.GetSection("Redis").Bind(localOptions);

            serviceCollection.AddSingleton(localOptions);
            serviceCollection.AddSingleton(ConnectionMultiplexer.Connect($"{localOptions.Host}:{localOptions.Port}"));
            serviceCollection.AddScoped<IRedisStreamConsumer, RedisStreamConsumer>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}
