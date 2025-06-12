using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hawking.CSI.Tracking.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Hawking.CSI.Hold
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

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
