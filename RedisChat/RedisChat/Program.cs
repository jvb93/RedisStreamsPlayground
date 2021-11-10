using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace RedisChat
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var serviceProvider = BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(scope.ServiceProvider));
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
