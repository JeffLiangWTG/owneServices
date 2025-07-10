using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.NewService
{
	public static class CorsPolicyConfig
	{
		public static void Register(WebApplicationBuilder builder)
		{
			if (!string.IsNullOrEmpty(ApplicationConfig.PortalUri))
			{
				builder.Services.AddCors(options =>
				{
					options.AddDefaultPolicy(
						policy =>
						{
							policy.AllowAnyHeader()
								.AllowAnyMethod()
								.WithOrigins(ApplicationConfig.PortalUri);
						});
				});
			}
		}
	}
}
