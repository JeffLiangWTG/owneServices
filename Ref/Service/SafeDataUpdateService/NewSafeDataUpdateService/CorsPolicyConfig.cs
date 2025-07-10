using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public static class CorsPolicyConfig
	{
		public const string StagingServiceCorsPolicy = "stagingServiceCorsPolicy";

		public static void Register(WebApplicationBuilder builder)
		{
			var portalUri = ConfigurationProvider.Configuration["PortalUri"];
			if (!string.IsNullOrEmpty(portalUri))
			{
				builder.Services.AddCors(options =>
				{
					options.AddDefaultPolicy(
						policy =>
						{
							policy.AllowAnyHeader()
								.AllowAnyMethod()
								.WithOrigins(portalUri);
						});
				});
			}
			var stagingServiceUri = ConfigurationProvider.Configuration["stagingServiceUri"];
			if (!string.IsNullOrEmpty(stagingServiceUri))
			{
				builder.Services.AddCors(options =>
				{
					options.AddPolicy(name: StagingServiceCorsPolicy,
						policy =>
						{
							policy.AllowAnyHeader()
								.AllowAnyMethod()
								.WithOrigins(stagingServiceUri.TrimEnd('/'));
						});
				});
			}
		}
	}
}
