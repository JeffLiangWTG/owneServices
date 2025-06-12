using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Hawking.CSI.Apps.YYYYYY.Send.Services;
using Hawking.CSI.Apps.YYYYYY.Send.Services.ContentStoreClients;
using Hawking.CSI.Apps.YYYYYY.Send.Services.ExternalClients;
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

namespace Hawking.CSI.Apps.YYYYYY.Send
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

            services.AddHttpClient<IContentStoreClient, HttpContentStoreClient>()
                .AddPolicyHandler(Policy.HandleResult<HttpResponseMessage>(r => 
                    r.StatusCode == HttpStatusCode.NotFound)
                        .WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(1)))
                .AddTransientHttpErrorPolicy(config => 
                    config.WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(1)));

            switch (Configuration["ExternalClient"])
            {
                case "FTP":
                    services.AddSingleton<IExternalClient, FtpExternalClient>();
                    break;
                case "NULL":
                    services.AddTransient<IExternalClient, NullExternalClient>();
                    break;
            }

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
