using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ForzaListner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ForzaConfig>(hostContext.Configuration.GetSection("Telemetry"));

                    var statsConfig = new StatsDConfig();
                    hostContext.Configuration.GetSection("StatsD").Bind(statsConfig);
                    services.AddSingleton<IMetrics>(new StatsDService(statsConfig));
                    services.AddHostedService<Worker>();
                });
    }
}
