using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Monitoring.Services;
using Hawking.CSI.Monitoring.Services.EventStreamClients;
using Hawking.CSI.Monitoring.Services.MetricsCacheClients;
using Hawking.CSI.Monitoring.Services.MetricsStoreClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Hawking.CSI.Monitoring
{
    class Program
    {
        private static readonly CancellationTokenSource stopping = new CancellationTokenSource();

        static Program()
        {
            AssemblyLoadContext.Default.Unloading += OnUnloading;
        }

        private static void OnUnloading(AssemblyLoadContext obj)
        {
            stopping.Cancel();
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting...");

            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Environment.CurrentDirectory);
                    config.AddJsonFile("appsettings.json", optional: false);
                    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostContext, config) =>
                {
                    var serilogConfig = new LoggerConfiguration().ReadFrom.Configuration(hostContext.Configuration);
                    serilogConfig.WriteTo.Console(outputTemplate: "{Timestamp:s} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
                    Log.Logger = serilogConfig.CreateLogger();
                    config.AddSerilog();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<MetricsConsumerService>();
                    services.AddSingleton<IEventStreamClient, StanEventStreamClient>();
                    services.AddSingleton<IMetricsCacheClient, InMemoryMetricsCacheClient>();
                    services.AddSingleton<IMetricsStoreClient, ElasticsearchMetricsStoreClient>();
                })
            ;

            await hostBuilder.RunConsoleAsync(stopping.Token);
        }
    }
}
