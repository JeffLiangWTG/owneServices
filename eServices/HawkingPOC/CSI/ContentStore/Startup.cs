using System.IO;
using System.Security.Cryptography.X509Certificates;
using Hawking.CSI.ContentStore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Hawking.CSI.ContentStore
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
            serilogConfig.WriteTo.Logger(config => config
                .WriteTo.Console(outputTemplate: "{Timestamp:s} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            );
            Log.Logger = serilogConfig.CreateLogger();
            services.AddLogging(config => config.AddSerilog());

            services.AddSingleton<IObjectStoreClient, GridFsObjectStoreClient>();

            var cert = new X509Certificate2(File.ReadAllBytes("self-signed.pfx"), "3hubRock$", X509KeyStorageFlags.MachineKeySet);
            services.AddDataProtection()
               .PersistKeysToFileSystem(new DirectoryInfo(@"./keys/"))
               .ProtectKeysWithCertificate(cert);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
