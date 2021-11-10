using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Producer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var redis = scope.ServiceProvider.GetRequiredService<ConnectionMultiplexer>();
                var redisOptions = scope.ServiceProvider.GetRequiredService<RedisOptions>();
                var db = redis.GetDatabase();

                while (true)
                {
                    Console.WriteLine("Enter a message:");
                    var streamValue = Console.ReadLine();
                    var id = await db.StreamAddAsync(redisOptions.StreamName, "Message", streamValue);
                    Console.WriteLine($"Wrote id {id} to stream {redisOptions.StreamName}");
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
            
            return serviceCollection.BuildServiceProvider();
        }
    }
}