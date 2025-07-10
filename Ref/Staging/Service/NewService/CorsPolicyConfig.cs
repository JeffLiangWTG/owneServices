using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public static class CorsPolicyConfig
	{
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
		}
	}
}
