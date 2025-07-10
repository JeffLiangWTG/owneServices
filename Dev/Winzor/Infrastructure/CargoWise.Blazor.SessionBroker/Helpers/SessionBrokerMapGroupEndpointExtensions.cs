using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.StaticFiles;
using CargoWiseNext.Infrastructure.Installations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using WTG.Logging.Extensions;

namespace CargoWise.Blazor.SessionBroker.Helpers
{
	/// <summary>
	/// Extension methods for configuring routes under /_sessionbroker
	/// </summary>
	public static class SessionBrokerMapGroupEndpointExtensions
	{
		const string MapGroupPrefix = "/_sessionbroker";

		/// <summary>
		/// Hooks up routes for installers and health checks under /_sessionbroker
		/// Any unrecognised paths return a 404
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseSessionBrokerMapGroupEndpoints(this IApplicationBuilder app)
		{
			ArgumentNullException.ThrowIfNull(app);

			return app.UseEndpoints(endpoints =>
			{
				_ = endpoints.MapGroup(MapGroupPrefix)
					.MapSetupInformationEndpoint()
					.MapHealthCheckEndPoint()
					.MapAllOthersAsNotFound();
			});
		}

		static RouteGroupBuilder MapSetupInformationEndpoint(this RouteGroupBuilder group)
		{
			_ = group.MapGet(
				Arguments.WinzorClientSetupInformationEndpoint[MapGroupPrefix.Length..],
				async (
					HttpContext cxt,
					IAppInstallerHandler appInstallerHandler,
					IInstallerPaths installerPaths,
					IUrlHandlerProvider urlHandlerProvider) =>
			{
				cxt.Response.Headers.CacheControl = "no-cache";

				var scheme = cxt.Request.Scheme;
				var host = cxt.Request.Host.Value;
				var installerInfo = appInstallerHandler.GetAppInstallerInfo();
				var version = installerInfo?.Version ?? new Version();
				var packageName = installerInfo?.PackageName ?? Constants.MsixClientPackageName;
				var setupInformation = CreateMsixSetupInformation(scheme, host, version, packageName);

				var jsonOption = new JsonSerializerOptions()
				{
					Converters = { new JsonStringEnumConverter() },
				};
				setupInformation.ClientAppLaunchUri = $"{urlHandlerProvider.GetUrlHandler()}:{scheme}://{host}/";
				await cxt.Response.WriteAsJsonAsync(setupInformation, options: jsonOption);
			});
			return group;
		}

		static WinzorClientSetupInformation CreateMsixSetupInformation(string scheme, string host, Version version, string packageName)
		{
			return new WinzorClientSetupInformation()
			{
				RequiredVersion = version.ToString(),
				MsixPackageName = packageName,
				MsixAppInstallerUrl = $"{scheme}://{host}{Arguments.ClientAppInstallerDownloadUrl}",
				MsixPackageUrl = $"{scheme}://{host}{Arguments.ClientMsixPackageDownloadUrl}",
				InstallType = WinzorClientInstallType.MSIX,
			};
		}

		static RouteGroupBuilder MapHealthCheckEndPoint(this RouteGroupBuilder group)
		{
			group.MapGet("/health", async (HttpContext ctx, IAffinityCookieValidator affinityCookieValidator, IRegistryAccessor registryAccessor) =>
			{
				var dbVersion = registryAccessor.GetDBSchemaVersion();
				var assemblyVersionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>();
				var appVersion = assemblyVersionAttribute?.Version ?? "0.0.0.0";

				var healthCheckResp = new HealthCheckResponse
				{
					AppServerCookieIsValid = affinityCookieValidator.Validate(ctx),
					CargoWiseVersion = $"App={appVersion}, DB={dbVersion}"
				};

				await ctx.Response.WriteAsJsonAsync(healthCheckResp);
			});
			return group;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static RouteGroupBuilder MapAllOthersAsNotFound(this RouteGroupBuilder group)
		{
			group.Map("/{*catchall}", async ctx =>
			{
				var logger = ctx.RequestServices.GetService<ILogger<Startup>>();
				logger.Enrich()
					.WithAlert(LogEventCategory.IntrusionDetection)
					.LogWarning("No endpoint is defined for request {Method} {Path}", ctx.Request.Method, ctx.Request.Path);

				ctx.Response.StatusCode = StatusCodes.Status404NotFound;
				await ctx.Response.WriteAsync($"[SessionBroker] No endpoint is defined for request {ctx.Request.Method} {ctx.Request.Path}");
			});
			return group;
		}

		internal record HealthCheckResponse
		{
			public bool AppServerCookieIsValid { get; init; }
			public string CargoWiseVersion { get; init; }
		}
	}
}
