using System.Threading.Tasks;
using CargoWiseNext.Infrastructure.Installations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Blazor.SessionBroker.StaticFiles
{
	public static class SessionBrokerStaticFilesExtensions
	{
		const string InstallerFilesPrefix = "/_sessionbroker/installer";
		const string PublicEndpointPrefix = "/_clientapi/installer";

		public static IServiceCollection AddStaticFilesForInstallers(this IServiceCollection services)
		{
			services.AddSingleton<IInstallerPaths, InstallerPaths>();
			services.AddSingleton<IAppInstallerHandler, AppInstallerHandler>();
			services.AddSingleton<IFileResponseHeaderResolver, FileResponseHeaderResolver>();
			services.AddSingleton<SessionBrokerStaticFileProvider>();
			services.AddSingleton<CustomContentTypeProvider>();
			return services;
		}

		public static IApplicationBuilder UseSessionBrokerStaticFiles(this IApplicationBuilder app)
		{
			return app.UseWhen(context => context.Request.Path.StartsWithSegments(InstallerFilesPrefix) ||
										context.Request.Path.StartsWithSegments(PublicEndpointPrefix),
				staticFileAppBuilder =>
				{
					staticFileAppBuilder.UseStaticFilesForPath(InstallerFilesPrefix);
					staticFileAppBuilder.UseStaticFilesForPath(PublicEndpointPrefix);
				}
			);
		}

		static IApplicationBuilder UseStaticFilesForPath(this IApplicationBuilder app, string requestPath)
		{
			var sessionBrokerStaticFileProvider = app.ApplicationServices.GetRequiredService<SessionBrokerStaticFileProvider>();
			var customContentTypeProvider = app.ApplicationServices.GetRequiredService<CustomContentTypeProvider>();

			return app.UseStaticFiles(new StaticFileOptions()
			{
				ContentTypeProvider = customContentTypeProvider,
				FileProvider = sessionBrokerStaticFileProvider,
				RequestPath = requestPath,
				OnPrepareResponseAsync = ctx => CustomPrepareResponseAsync(ctx),
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static Task CustomPrepareResponseAsync(StaticFileResponseContext ctx)
		{
			// Cache assets in proxy caches, but REQUIRE revalidation by any cache when served.
			ctx.Context.Response.Headers.CacheControl = "public, no-cache, must-revalidate";
			return Task.CompletedTask;
		}
	}
}
