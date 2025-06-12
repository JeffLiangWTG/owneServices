using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace eServices.HealthCheck.Tests;

public class TestApp
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddWtgHealthChecks()
			.AddCheck("Ready", () => builder.Configuration["HEALTHCHECK_READY"] switch
			{
				"H" => HealthCheckResult.Healthy(),
				"D" => HealthCheckResult.Degraded(),
				"U" => HealthCheckResult.Unhealthy(),
				_ => HealthCheckResult.Healthy()
			}, ["ready"])
			.AddCheck("Check1", () => builder.Configuration["HEALTHCHECK_CHECK1"] switch
			{
				"H" => HealthCheckResult.Healthy(),
				"D" => HealthCheckResult.Degraded(),
				"U" => HealthCheckResult.Unhealthy(),
				_ => HealthCheckResult.Healthy()
			})
			.AddCheck("Check2", () =>
			{
				var check2 = builder.Configuration["HEALTHCHECK_CHECK2"] ?? "";
				var msg = $"""
				Check2 state is
				'{check2}'
				""";
				return new HealthCheckResult(check2 switch
				{
					"H" => HealthStatus.Healthy,
					"D" => HealthStatus.Degraded,
					"U" => HealthStatus.Unhealthy,
					_ => HealthStatus.Healthy
				},
				msg
				, check2 is "U" ? new InvalidOperationException() : null
				, new Dictionary<string, object> {
					{ "STATE", check2 },
					{ "Message", msg }
				});

			})
			.AddCheck("Check3", () => builder.Configuration["HEALTHCHECK_CHECK3"] switch
			{
				"H" => HealthCheckResult.Healthy(),
				"D" => HealthCheckResult.Degraded(),
				"U" => throw new InvalidOperationException("Check3 has failed."),
				_ => HealthCheckResult.Healthy()
			});
		var app = builder.Build();
		app.MapWtgHealthChecks();
		app.Run();
	}
}