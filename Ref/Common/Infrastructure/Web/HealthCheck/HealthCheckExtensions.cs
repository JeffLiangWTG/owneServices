using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Common.Web
{
	public static class HealthCheckExtensions
	{
		const string WtgStatusPath = "/wtg/status";
		const string WtgHealthPathForQuartz = "/quartz/wtg/health";

		public static IApplicationBuilder UseWtgStatusHealthChecks(this IApplicationBuilder app)
		{
			return UseWtgHealthChecks(app, WtgStatusPath);
		}

		public static IApplicationBuilder UseWtgHealthChecksForQuartz(this IApplicationBuilder app)
		{
			return UseWtgHealthChecks(app, WtgHealthPathForQuartz);
		}

		static IApplicationBuilder UseWtgHealthChecks(IApplicationBuilder app, string wtgHealthCheckPath)
		{
			return app.UseHealthChecks(wtgHealthCheckPath,
				new HealthCheckOptions
				{
					AllowCachingResponses = false,
					ResponseWriter = WtgStatusResponseHelper.WriteWtgStatusResponse
				});
		}

		public static IHealthChecksBuilder AddWtgStatusHealthChecks(this IServiceCollection services, ILogWrapper logWrapper, string name, string connectionStringOrEntityConnectionString)
		{
			return services.AddHealthChecks().AddTypeActivatedCheck<WtgStatusHealthCheck>(name, new object[] { logWrapper, connectionStringOrEntityConnectionString });
		}
	}
}
