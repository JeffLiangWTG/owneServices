using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Hawking.CSI.Tracking.Services;
using Hawking.CSI.Tracking.Services.EventStreamClients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Hawking.CSI.Tracking
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

            var cert = new X509Certificate2(File.ReadAllBytes("self-signed.pfx"), "3hubRock$", X509KeyStorageFlags.MachineKeySet);
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(@"./keys/")).ProtectKeysWithCertificate(cert);

            services.AddSingleton<IEventStreamClient, StanEventStreamClient>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
