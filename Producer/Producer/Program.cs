using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                    if (streamValue.Equals("boom"))
                    {
                       
                        for (var x = 0; x < 500; x++)
                        {
                            db.StreamAddAsync(redisOptions.StreamName, "Message", $"Message {x}");
                        }
                    }
                    else if (streamValue.Equals("clear"))
                    {
                        var results = db.StreamRead(redisOptions.StreamName, "0-0");
                        db.StreamDelete(redisOptions.StreamName, results.Select(x => x.Id).ToArray());
                    }
                    else
                    {
                        var id = await db.StreamAddAsync(redisOptions.StreamName, "Message", streamValue);
                        Console.WriteLine($"Wrote id {id} to stream {redisOptions.StreamName}");

                    }
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