using Asp.Versioning;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace eServices.Dms.Core.ServiceDefaults;

public static class DmsDefaults
{
	public static ApiVersion DefaultApiVersion => ApiVersion_0_1;
	public static ApiVersion ApiVersion_0_1 { get; } = new ApiVersion(0.1);

	public static IServiceCollection AddDmsApiSwaggerGen(this IServiceCollection services, string title)
	{
		services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = DefaultApiVersion;
			options.AssumeDefaultVersionWhenUnspecified = true;
		});

		services.AddEndpointsApiExplorer();

		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Version = DefaultApiVersion.ToString(),
				Title = title
			});
			options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
			{
				Description = "Basic auth added to authorization header",
				Name = "Authorization",
				In = ParameterLocation.Header,
				Scheme = "basic",
				Type = SecuritySchemeType.Http
			});
			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
					},
					new List<string>()
				}
			});
			options.ResolveConflictingActions(apiDescriptions =>
			{
				var descriptionVersions = apiDescriptions
					.SelectMany(a => a.ActionDescriptor.EndpointMetadata.OfType<ApiVersionMetadata>().First()
						.Map(ApiVersionMapping.Explicit).DeclaredApiVersions.Select(v => new { description = a, version = v }))
					.ToDictionary(dv => dv.version, dv => dv.description);
				return descriptionVersions[DefaultApiVersion] ?? descriptionVersions.MaxBy(dv => dv.Key).Value;
			});
		});

		return services;
	}

	public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder)
		=> builder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler<AppUser>>(BasicDefaults.AuthenticationScheme, default);

	public static WebApplication MapWtgHealthChecks(this WebApplication app)
	{
		app.MapHealthChecks("/wtg/status").AllowAnonymous();

		app.MapHealthChecks("/wtg/alive", new HealthCheckOptions
		{
			Predicate = r => r.Tags.Contains("live")
		}).AllowAnonymous();

		return app;
	}
}
