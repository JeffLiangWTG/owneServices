using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CargoWise.Blazor.Client.Integration.DependencyInjection;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWise.Blazor.SessionBroker.Middleware;
using CargoWise.Blazor.SessionBroker.StaticFiles;
using CargoWise.DataProtection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.SessionAffinity;
using Yarp.ReverseProxy.Transforms;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace CargoWise.Blazor.SessionBroker
{
	public class Startup
	{
		readonly IConfiguration configuration;

		public Startup(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public void ConfigureServices(IServiceCollection services)
		{
			var routes = new[]
			{
				new RouteConfig
				{
					RouteId = "route1",
					ClusterId = "cluster1",
					Match = new RouteMatch
					{
						Path = "{**catch-all}"
					},
				}.WithTransformXForwarded(
					xFor: ForwardedTransformActions.Append,
					xHost: ForwardedTransformActions.Append,
					xProto: ForwardedTransformActions.Append
				),
			};

			var clusters = new[]
			{
				new ClusterConfig
				{
					ClusterId = "cluster1",
					SessionAffinity = new SessionAffinityConfig
					{
						Enabled = true,
						Policy = SessionAffinityConstants.Policies.Cookie,
						FailurePolicy = AuthenticatingAffinityFailurePolicy.PolicyName,
						AffinityKeyName = AffinityCookieOptionsProvider.DefaultCookieName
					},
					HealthCheck = new HealthCheckConfig
					{
						Active = new ActiveHealthCheckConfig
						{
							Enabled = true,
							Path = "/health",
							Policy = "ValidateProcessId"
						},
					},
					Destinations = new Dictionary<string, DestinationConfig>(),
				},
			};
			services.Configure<CargoWiseOptions>(configuration.GetSection(nameof(CargoWiseOptions)));
			services.PostConfigure<CargoWiseOptions>(c =>
			{
				c.SessionBrokerProcessCorrelationId ??= Guid.NewGuid();
			});

			services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardLimit = null;
				options.KnownNetworks.Add(new IPNetwork(IPAddress.Any, 0));
				options.ForwardedHeaders = ForwardedHeaders.All;
			});

			services.Configure<ShutdownOptions>(configuration.GetSection(nameof(ShutdownOptions)));
			services.AddSingleton<RequestTracker>();
			services.AddReverseProxy().LoadFromMemory(routes, clusters);
			services.AddSingleton<AppInstanceService>();
			services.AddSingleton<ITokenValidatorWrapper, TokenValidatorWrapper>();
			services.AddSingleton<IAffinityFailurePolicy, AuthenticatingAffinityFailurePolicy>();
			services.AddSingleton<IActiveHealthCheckPolicy, ValidateProcessIdHealthCheckPolicy>();
			services.AddSingleton<ISecureSecretGenerator, SecureSecretGenerator>();
			services.AddSingleton<SessionSecretStore>();
			services.AddTransient<AppServerProcess>();
			services.AddCargoWiseClient();
			services.AddStaticFilesForInstallers();
			services.AddRazorPages();
			services.AddMemoryCache();
			services.AddScoped<HtmlRenderer>();

			services.AddHttpClient();
			services.AddTransient<VersionBrokerRegistration>();
			services.AddLogging();
			services.AddHealthChecks();
			services.AddHostedService<IdleWatcherService>();
			services.AddHttpContextAccessor();

			services.AddSingleton<Microsoft.Extensions.Logging.ILogger, Microsoft.Extensions.Logging.Logger<Startup>>();
			services.AddSingleton<ICargoWiseAuthenticator, CargoWiseAuthenticator>();
			services.AddSingleton<IDatabaseAccessor, DatabaseAccessor>();
			services.AddSingleton<IAuthenticationDatabaseAccessor, AuthenticationDatabaseAccessor>();
			services.AddSingleton<IRegistryAccessor, RegistryAccessor>();
			services.AddSingleton<IAuthenticationConfigAccessor, AuthenticationConfigAccessor>();
			services.AddSingleton<IAffinityCookieValidator, AffinityCookieValidator>();
			services.AddSingleton<ISiteOfflineChecker, SiteOfflineChecker>();

			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Web);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopmentOrIntegrationTest())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler(new ExceptionHandlerOptions()
				{
					ExceptionHandler = async ctx =>
					{
						var feature = ctx.Features.Get<IExceptionHandlerFeature>();
						Log.Warning($"{ctx.Request.Method} {ctx.Request.Path} Error.", feature.Error);
						ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
						await ctx.Response.WriteAsync("SessionBroker Internal Error.");
					}
				});
			}

			app.UseForwardedHeaders();
			app.UseSessionBrokerStaticFiles();

			const string healthPath = "/health";
			app.UseWhen(
				context => new Uri(context.Request.Path, UriKind.Relative) != new Uri(healthPath, UriKind.Relative),
				subApp =>
				{
					subApp.UseMiddleware<RequestTrackerMiddleware>();
					subApp.UseUserAgentCheck();
				});

			app.UseStaticFiles();

			app.UseRouting();
			app.UseSerilogRequestLogging();
			app.UseSessionBrokerMapGroupEndpoints();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages(); // do pages first to ensure that people are directed straight to the "login" page
				endpoints.MapHealthChecks(healthPath);
				endpoints.MapReverseProxy(proxyPipeline =>
				{
					proxyPipeline.UseMiddleware<CustomSessionAffinityMiddleware>();
					proxyPipeline.UseLoadBalancing();
				});
			});
		}
	}
}
