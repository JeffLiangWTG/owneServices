using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Hawking.CSI.Forward.Services;
using Hawking.CSI.Forward.Services.ApplicationClients;
using Hawking.CSI.Forward.Services.ApplicationRegistryClients;
using Hawking.CSI.Forward.Services.ForwardHeartbeatClients;
using Hawking.CSI.Forward.Services.ForwardQueueClients;
using Hawking.CSI.Forward.Services.ForwardQueueLockClients;
using Hawking.CSI.Tracking.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Hawking.CSI.Forward
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var serilogConfig = new LoggerConfiguration().ReadFrom.Configuration(Configuration);
            serilogConfig.WriteTo.Console(outputTemplate: "{Timestamp:s} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: SystemConsoleTheme.Literate);
            Log.Logger = serilogConfig.CreateLogger();
            services.AddLogging(config => config.AddSerilog());

            services.AddMessageEventClient();

            services.AddSingleton<IApplicationRegistryClient, JsonApplicationRegistryClient>();
            services.AddSingleton<IForwardQueueClient, MongoForwardQueueClient>();
            services.AddSingleton<IForwardEnqueueService, ForwardEnqueueService>();
            services.AddSingleton<IForwardHeartbeatClient, RedisForwardHeartbeatClient>();
            services.AddSingleton<IForwardQueueLockClient, RedisForwardQueueLockClient>();
            services.AddHttpClient<IApplicationClient, HttpApplicationClient>()
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(60))
                .AddTransientHttpErrorPolicy(config => config.RetryForeverAsync());

            services.AddHostedService<ForwardService>();

            var cert = new X509Certificate2(File.ReadAllBytes("self-signed.pfx"), "3hubRock$", X509KeyStorageFlags.MachineKeySet);
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(@"./keys/")).ProtectKeysWithCertificate(cert);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
