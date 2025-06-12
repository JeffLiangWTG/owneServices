using Polly;
using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Hawking.Apps.CCCCCC.Inbound.Services;
using Hawking.Tracking.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Hawking.Apps.CCCCCC.Inbound
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
            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Environment.CurrentDirectory);
                    config.AddJsonFile("appsettings.json", optional: false);
                    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostContext, config) =>
                {
                    config.AddConfiguration(hostContext.Configuration);
                    var serilogConfig = new LoggerConfiguration().ReadFrom.Configuration(hostContext.Configuration);
                    serilogConfig.WriteTo.Console(outputTemplate: "{Timestamp:s} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
                    Log.Logger = serilogConfig.CreateLogger();
                    config.AddSerilog();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTrackingMessageEventClient(hostContext.Configuration);

                    services.AddHostedService<PollingService>();
                    services.AddHttpClient<IPollingHttpClient, PollingHttpClient>()
                        .AddTransientHttpErrorPolicy(config => config.RetryForeverAsync());
                    services.AddHttpClient<IMessageCacheHttpClient, MessageCacheHttpClient>()
                        .AddTransientHttpErrorPolicy(config => config.RetryForeverAsync());
                });

            await hostBuilder.RunConsoleAsync(stopping.Token);
        }
    }
}
