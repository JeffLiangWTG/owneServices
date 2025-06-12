using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Hawking.eHub.Api.Controllers;
using Hawking.eHub.Api.Exceptions;
using Hawking.eHub.Mapping.Config.eHubTransactions;
using Hawking.eHub.Mapping.Config.Services;
using Hawking.eHub.Model.eHubTransactions;
using Hawking.eHub.Model.Services;
using Hawking.eHub.Transform.Helper.Wrapper.Cargowise.eHub.DataAccess.Sql;
using Hawking.Unity;
using Hawking.Xslt.ExtensionObjects.Interfaces;
using Hawking.Xslt.Legacy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;
using Unity;
using Unity.Lifetime;

namespace Hawking.eHub.Api
{
    public class Startup : IDisposable
    {
        const string LegacyXsltAssembliesPathConfig = "Hawking.Xslt.Legacy.Path";

        #region Constructors

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion
        
        #region IDisposable

        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeInternal();
            }

            disposed = true;
        }

        void DisposeInternal()
        {
            DependencyFactory.Instance?.Dispose();
        }

        #endregion

        #region Properties

        public static IConfiguration Configuration { get; private set; }

        #endregion

        #region Configuration

        // This method gets called by the runtime. Use this method to configure the unity container.
        public void ConfigureContainer(IUnityContainer unityContainer)
        {
            unityContainer.RegisterSingleton<ITransformAccessor, TransformAccessor>();
            unityContainer.RegisterSingleton<IeHubStoredProc, eHubStoredProc>();
            unityContainer.RegisterSingleton<IeHubTransactionsContext, eHubTransactionsContext>();
            unityContainer.RegisterInstance<ILogger>(CreateLogger(), new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<XsltController>();
            unityContainer.RegisterSingleton<IMappingConfigService, MappingConfigService>();
            unityContainer.RegisterType<IeHubMappingDataContext, eHubMappingJsonDataContext>();

            ConfigLegacyXsltAssemblies(unityContainer);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // using IUnityContainer instead
            //Log.Logger = CreateLogger();
            //services.AddLogging(config => config.AddSerilog());
            //services.AddSingleton<ITransformAccessor, TransformAccessor>();
            //services.AddSingleton<IeHubStoredProc, eHubStoredProc>();
            //services.AddSingleton<IeHubTransactionsContext, eHubTransactionsContext>();
            //services.AddSingleton<ILogger>(CreateLogger());
            //services.AddSingleton<IMappingConfigService, MappingConfigService>();
            //services.AddSingleton<IeHubMappingDataContext, eHubMappingJsonDataContext>();

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseMvc()
                .UseMiddleware<ExceptionHandlersMiddleware>();
                //.UseExceptionHandler(options =>
                //{
                //    options.Run(
                //        async context =>
                //        {
                //            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                //            context.Response.ContentType = "text/html";

                //            var exception = context.Features.Get<IExceptionHandlerFeature>();
                //            if (exception != null)
                //            {
                //                var message = $"<h1>Error: {exception.Error.Message}</h1>{exception.Error.StackTrace}";
                //                await context.Response.WriteAsync(message).ConfigureAwait(false);
                //            }
                //        });
                //});
        }

        static ILogger CreateLogger()
        {
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext();

            return loggerConfig.CreateLogger();
        }

        #endregion

        #region Helpers

        void ConfigLegacyXsltAssemblies(IUnityContainer unitContainer)
        {
            var configuration = unitContainer.Resolve<IConfiguration>();

            string xsltLibFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration.GetSection("AppSettings")["Hawking.Xslt.Legacy.Path"]);
            string[] xsltLibFiles = Directory.GetFiles(xsltLibFilesPath, "*.dll");

            var pluginMappings = new Dictionary<Type, Type>();
            foreach (string fileName in xsltLibFiles)
            {
                var xsltAssembly = Assembly.LoadFile(fileName);
                var xsltAssemblyTypes = xsltAssembly.GetTypes().Where(x => typeof(IXsltLegacy).IsAssignableFrom(x));
                if (xsltAssemblyTypes.Any())
                {
                    foreach (var xsltType in xsltAssemblyTypes)
                    {
                        unitContainer.RegisterType(xsltType);
                    }
                }
            }
        }

        #endregion
    }
}
