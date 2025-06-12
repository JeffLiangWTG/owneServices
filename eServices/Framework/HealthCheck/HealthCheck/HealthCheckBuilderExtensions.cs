using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace eServices.HealthCheck;

public static class HealthCheckBuilderExtensions
{
	public static IHealthChecksBuilder AddWtgHealthChecks(this IServiceCollection services)
	{
		var healthCheckBuilder = services.AddHealthChecks()
			.AddCheck("Default", () => HealthCheckResult.Healthy(), ["ready"]);
		return healthCheckBuilder;
	}

	public static IEndpointRouteBuilder MapWtgHealthChecks(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapHealthChecks("/wtg/ready", new HealthCheckOptions
		{
			ResultStatusCodes =
			{
				[HealthStatus.Healthy] = StatusCodes.Status200OK,
				[HealthStatus.Degraded] = StatusCodes.Status200OK,
				[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
			},
			Predicate = r => r.Tags.Contains("ready")
		}).AllowAnonymous();

		endpoints.MapHealthChecks("/wtg/status", new HealthCheckOptions
		{
			ResultStatusCodes =
			{
				[HealthStatus.Healthy] = StatusCodes.Status200OK,
				[HealthStatus.Degraded] = StatusCodes.Status200OK,
				[HealthStatus.Unhealthy] = StatusCodes.Status200OK
			},
			ResponseWriter = (context, healthReport) =>
			{
				context.Response.ContentType = "text/plain; charset=utf-8";

				var result = new StringBuilder();

				foreach (var item in healthReport.Entries)
				{
					var line = new StringBuilder();

					line.Append(item.Value.Status switch
					{
						HealthStatus.Healthy => "INFO",
						HealthStatus.Degraded => "WARNING",
						HealthStatus.Unhealthy => "ERROR",
						_ => throw new NotImplementedException()
					}).Append($"({item.Key})");

					var details = new List<string>();

					if (item.Value.Description != null)
					{
						details.Add(item.Value.Description);
					}

					if (item.Value.Data != null)
					{
						foreach (var data in item.Value.Data)
						{
							details.Add($"{data.Key}: {data.Value}");
						}
					}

					if (item.Value.Exception != null)
					{
						details.Add($"{item.Value.Exception.GetType()}: {item.Value.Exception.Message}");
					}

					if (details.Count > 0)
					{
						line.Append(": ").Append(string.Join("|", details));
					}

					result.AppendLine(line.ToString().ReplaceLineEndings(" "));
				}

				return context.Response.WriteAsync(result.ToString());
			}
		}).AllowAnonymous();

		return endpoints;
	}
}
