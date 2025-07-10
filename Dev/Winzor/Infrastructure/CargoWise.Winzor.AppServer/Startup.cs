using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BArchitecture;
using CargoWise.Blazor.Client.Integration.DependencyInjection;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.HostedAppRuntime;
using CargoWise.EntityFramework;
using CargoWise.Main.Navigation;
using CargoWise.Winzor.AppServer.Diagnostics;
using CargoWise.Winzor.AppServer.Helpers;
using CargoWise.Winzor.AppServer.Hosting;
using CargoWise.Winzor.AppServer.Middleware;
using CargoWise.Winzor.AppServer.Telemetry;
using CargoWise.Winzor.Telemetry;
using CargoWiseNext.Blazor.Components;
using Enterprise.DocumentScanning.Business;
using Enterprise.Winzor.Architecture;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WTG.Logging.Extensions;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms.Builder;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace CargoWise.Winzor.AppServer
{
	public class Startup
	{
		const int AssetCacheDuration = 60 * 60 * 24; // Cache for a day
		const int MaxHubMessageBufferSize = 1024 * 96; // max signalR message or buffer size
		static Collection<string> CachedFileTypes { get; } = new Collection<string>
		{
			".png",
			".woff",
			".woff2",
			".ico",
			".svg",
			".bmp",
			".cur"
		};

		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			Configuration = configuration;
			HostEnvironment = env;
		}

		IConfiguration Configuration { get; }

		IWebHostEnvironment HostEnvironment { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public void ConfigureServices(IServiceCollection services)
		{
			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
			services.AddHttpContextAccessor();
			services.Configure<CargoWiseAuthOptions>(Configuration.GetSection(nameof(CargoWiseAuthOptions)));
			services.Configure<CargoWiseOptions>(Configuration.GetSection(nameof(CargoWiseOptions)));
			services.AddOptions<EntryPointPoolOptions>()
				.Bind(Configuration.GetSection(nameof(EntryPointPoolOptions)))
				.ValidateDataAnnotations();
			var cwOptions = new CargoWiseOptions();
			Configuration.Bind(nameof(CargoWiseOptions), cwOptions);
			if (cwOptions.AppServerProcessCorrelationId is null)
			{
				cwOptions.AppServerProcessCorrelationId = Guid.NewGuid();
				services.PostConfigure<CargoWiseOptions>(c =>
				{
					c.AppServerProcessCorrelationId ??= cwOptions.AppServerProcessCorrelationId;
				});
			}
			AddTelemetry(services, cwOptions);
			services.Configure<ForwardedHeadersOptions>(options =>
			{
				// The ASP.NET Core defaults only allow 1 proxy server on the localhost.
				// Our solution requires 3 proxy servers including the HAProxy on a different host.
				// If publicly hosted (e.g. WiseCloud), this service must be hosted behind a
				// trusted frontend proxy which strips spoofed forwarded headers.
				// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer
				// These headers are important so that NavigationManager can calculate the app's absolute URL.
				options.ForwardLimit = null;
				options.KnownNetworks.Add(new IPNetwork(IPAddress.Any, 0));
				options.ForwardedHeaders = ForwardedHeaders.All;
			});
			services.AddHostedService<BackchannelHostedService>();
			services.AddCargoWiseClient();
			services.AddControllers();
			services.AddRazorPages();
			services.AddSignalR(e => e.MaximumReceiveMessageSize = MaxHubMessageBufferSize);
			services.AddRazorComponents()
				.AddInteractiveServerComponents(options =>
				{
					if (HostEnvironment.IsDevelopmentOrIntegrationTest())
					{
						options.DetailedErrors = true;
					}
					options.DisconnectedCircuitRetentionPeriod = cwOptions.DisconnectedCircuitRetentionPeriod;
				})
				.AddHubOptions(options =>
				{
					options.DisableImplicitFromServicesParameters = true;
				});

			services.AddResponseCompression(options =>
			{
				// Compressing dynamic content over HTTPS would expose us to CRIME and BREACH attacks, so we need to disable the compression for HTTPS requests
				// However, the middleware by default will always compress static web assets over HTTPS and any response over HTTP as these are safe to compress
				options.EnableForHttps = false;
				options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
			});

			services.AddSingleton<PoolingFormOpener>();
			services.AddSingleton<FormOpener>();
			services.AddSingleton<IFormOpener>(s =>
			{
				var options = s.GetRequiredService<IOptions<EntryPointPoolOptions>>();

				if (options.Value.Enabled)
				{
					return s.GetRequiredService<PoolingFormOpener>();
				}

				return s.GetRequiredService<FormOpener>();
			});
			services.AddSingleton<IFormInstanceRegister, EnterpriseRegisteredFormInstances>();
			services.AddSingleton(s => new WinzorDispatcher(s.GetRequiredService<IFormOpener>(), s.GetRequiredService<IFormInstanceRegister>()));
			services.AddHttpClient();
			services.AddHttpForwarder();
			services.AddScoped<IJSRuntimeWithMonitor, JSRuntimeWithMonitor>();
			services.AddSingleton<IFileVersionHash, FileVersionHash>();
			services.AddSingleton<ITokenValidatorWrapper, TokenValidatorWrapper>();
			services.AddSingleton<ICargoWiseAuthStateProvider, CargoWiseAuthStateProvider>();
			services.AddSingleton<IWinzorCargoWiseLoginHandler, WinzorCargoWiseLoginHandler>();
			services.AddSingleton<IOpeningFormQueue, OpeningFormQueue>();
			services.AddSingleton<EntryPointPool>();
			services.AddCargoWiseRuntime();
			services.AddSingleton<Application>();
			services.AddSingleton<GlobalExceptionHandler>();
			services.AddSingleton<IShutDownCircuitHandler, ShutDownCircuitHandler>();
			services.AddSingleton<CircuitHandler, ShutDownCircuitHandler>(provider => (ShutDownCircuitHandler)provider.GetRequiredService<IShutDownCircuitHandler>());
			services.AddScoped<ICircuitIdProvider, CircuitIdProvider>();
			services.AddScoped<CircuitHandler, CircuitIdProvider>(provider => (CircuitIdProvider)provider.GetRequiredService<ICircuitIdProvider>());
			services.AddTransient<IProcessStartNotifier, ProcessStartNotifier>();
			services.AddHealthChecks().AddCheck<HealthCheckWithProcessId>("health", HealthStatus.Unhealthy, null);
			services.Decorate<IWindowService, WinzorWindowService>();
			services.AddSingleton<IDownloadObjectManager, DownloadObjectManager>();
			services.AddTransient<IFileService, FileService>();
			services.AddTransient<IClientEventService, ClientEventService>();
			services.AddSingleton<DocumentFactoryProvider>();
			services.AddSingleton<UserMonitorRegistry>();
			services.AddTransient(provider => provider.GetRequiredService<DocumentFactoryProvider>().GetFactory(provider.GetRequiredService<Lazy<BusinessObjectFactory>>().Value));
			services.AddHttpContextAccessor();
			RegisterJSInteropServices(services);
			services.AddSingleton<IImageCacher, ImageCacher>();

			services.Configure<StaticFileOptions>(options =>
			{
				options.OnPrepareResponse = PrepareStaticFileResponse;
			});

			services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
		}

		void PrepareStaticFileResponse(StaticFileResponseContext ctx)
		{
			if (ctx.File.Name == null)
			{
				return;
			}

			var filename = ctx.File.Name.ToLower();

			if (CachedFileTypes.Any(ext => filename.EndsWith(ext)))
			{
				ctx.Context.Response.Headers.Append("cache-control", $"public, max-age={AssetCacheDuration}");
				return;
			}

			if (filename.EndsWith(".js") || filename.EndsWith(".css"))
			{
				using (var stream = ctx.File.CreateReadStream())
				{
					var fileVersionHash = ctx.Context.RequestServices.GetRequiredService<IFileVersionHash>();
					var version = fileVersionHash.GetHash(stream);
					if (!string.IsNullOrEmpty(version))
					{
						var v = ctx.Context.Request.Query["v"];
						if (v.Count == 1 && v[0] == version)
						{
							ctx.Context.Response.Headers.Append("cache-control", $"public, max-age={AssetCacheDuration}");
						}
						else
						{
							ctx.Context.Response.Headers.Append("cache-control", $"private, max-age={AssetCacheDuration}");
							var query = QueryString.Create((NoResString)"v", version);
							ctx.Context.Response.Redirect(ctx.Context.Request.Path + query);
						}
					}
				}
			}
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<CargoWiseAuthOptions> cargoWiseAuthOptions, IHttpForwarder forwarder, ITransformBuilder transformBuilder, ILogger<Startup> logger, UserMonitorRegistry userMonitorRegistry)
		{
			_ = app.ApplicationServices.GetRequiredService<GlobalExceptionHandler>();
			var reporter = new BaseExceptionReporter
			{
				SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions = true,
			};
			Action<Exception> reportHandler = (ex) => _ = Task.Run(() => reporter.ReportDeveloperException(ex.Message, ex));
			Trace.Listeners.Add(new ExceptionReporterTraceListener(reportHandler, propagateError: env.IsDevelopment()));

			app.UseForwardedHeaders();
			app.UseResponseCompression();

			if (env.IsDevelopmentOrIntegrationTest())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				//app.UseHsts();
			}

			//app.UseHttpsRedirection();
			const string healthCheckPath = "/health";
			app.UseStaticFiles();

			app.UseSerilogRequestLogging();

			app.UseRouting();
			app.UseAntiforgery();

			if (env.IsProduction() || cargoWiseAuthOptions.Value.SessionToken is not null)
			{
				if (string.IsNullOrWhiteSpace(cargoWiseAuthOptions.Value.SessionToken))
				{
					logger.Enrich()
						.WithAlert(LogEventCategory.Session, LogEventOutcome.Failure)
						.LogWarning($"{nameof(CargoWiseAuthOptions.SessionToken)} is required but was not provided.");

					throw new Exception($"{nameof(CargoWiseAuthOptions.SessionToken)} is required but was not provided.");
				}

				app.UseWhen(
					httpContext => new Uri(httpContext.Request.Path, UriKind.Relative) != new Uri(healthCheckPath, UriKind.Relative),
					subApp => subApp.UseSessionTokenMiddleware());
			}

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapUserMonitoringEndpoint(forwarder, transformBuilder, logger, userMonitorRegistry);
				endpoints.MapRazorComponents<App>()
					.AddInteractiveServerRenderMode()
					// This is a workaround to be able to configure the HttpConnectionDispatcherOptions of SignalR which was previously available
					// Hopefully Microsoft will expose these options again in the future - see https://github.com/dotnet/aspnetcore/issues/54080
					.Add(convention =>
					{
						var options = convention.Metadata.OfType<HttpConnectionDispatcherOptions>().FirstOrDefault();
						if (options is not null)
						{
							// Long-polling produces such a bad user experience that we can't support it
							options.Transports = HttpTransportType.WebSockets;
						}
					});
				endpoints.MapControllers();
				endpoints.MapHealthChecks(healthCheckPath, new HealthCheckOptions { ResponseWriter = WriteHealthCheckResponseAsync });
				endpoints.MapImageCacherEndpoint();
			});
		}

		// adapted from docs at https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-5.0#customize-output
		static Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport result)
		{
			context.Response.ContentType = (NoResString)"application/json; charset=utf-8";

			var options = new JsonWriterOptions
			{
				Indented = true,
			};

			using (var stream = new MemoryStream())
			{
				using (var writer = new Utf8JsonWriter(stream, options))
				{
					writer.WriteStartObject();
					writer.WriteString((NoResString)"status", result.Status.ToString());
					var entry = result.Entries.Single();
					writer.WriteStartObject((NoResString)"data");

					foreach (var item in entry.Value.Data)
					{
						writer.WritePropertyName(item.Key);
						JsonSerializer.Serialize(
							writer,
							item.Value,
							item.Value?.GetType() ?? typeof(object));
					}

					writer.WriteEndObject();
					writer.WriteEndObject();
				}

				var json = Encoding.UTF8.GetString(stream.ToArray());

				return context.Response.WriteAsync(json);
			}
		}

		public void AddTelemetry(IServiceCollection services, CargoWiseOptions cwOptions)
		{
			var telemetryOptions = Configuration.GetSection(nameof(TelemetryOptions)).Get<TelemetryOptions>();
			if (telemetryOptions != null)
			{
				telemetryOptions.ServiceVersion = ReleaseInfo.Instance.VersionNumber.ToString();
				telemetryOptions.CargoWiseDbName = cwOptions.DatabaseName;
				telemetryOptions.ServiceInstanceId = cwOptions.AppServerProcessCorrelationId.ToString();

				services.AddSingleton(Options.Create(telemetryOptions));
				services.AddTelemetry(telemetryOptions, HostEnvironment.EnvironmentName);
			}

			services.RegisterTelemetryServices();
			services.Configure<HubOptions>(o =>
			{
				o.AddFilter<TelemetryHubFilter>();
			});
		}

		public static void RegisterJSInteropServices(IServiceCollection services)
		{
			WinzorFramework.JSInterop.Services.AddJSInteropServices(services);
			Aga.Controls.JSInterop.Services.AddJSInteropServices(services);
			CargoWise.GUI.TileBar.JsInterop.Services.AddJSInteropServices(services);
			Enterprise.ZArchitecture.GUI.JSInterop.Services.AddJSInteropServices(services);
			CargoWise.NetworkVisualisation.GUI.JSInterop.Services.AddJSInteropServices(services);
			Enterprise.DocumentEngine.GUI.JSInterop.Services.AddJSInteropServices(services);
			Enterprise.DocumentScanning.GUI.JSInterop.Services.AddJSInteropServices(services);
			CargoWise.Windows.UI.JSInterop.Services.AddJSInteropServices(services);
			services
				.AddCargoWiseMainNavigation()
				.AddCargoWiseBlazorComponents();
		}
	}
}
