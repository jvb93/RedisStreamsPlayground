using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Consumer
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

                string lastReceived = "";
                Console.WriteLine("Catching up!");
                var results = await db.StreamReadAsync(redisOptions.StreamName, "0-0");
                WriteStreamContent(results);
                lastReceived = results.Last().Id;
                var lastReadTime = DateTime.Now;
                    
                while (true)
                {
                    results = await db.StreamReadAsync(redisOptions.StreamName, lastReceived);
                    if (results.Any())
                    {
                        WriteStreamContent(results);
                        lastReceived = results.Last().Id;
                    }
                    else
                    {
                        Console.WriteLine($"Nothing new since {lastReadTime:T}");
                    }

                    lastReadTime = DateTime.Now;
                    await Task.Delay(1000);
                }
            }
        }

        static void WriteStreamContent(StreamEntry[] streamEntries)
        {
            foreach (var streamEntry in streamEntries)
            {
                Console.WriteLine($"{streamEntry.Id} - {streamEntry.Values.First().Name}: {streamEntry.Values.First().Value}");
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
