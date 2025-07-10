using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CargoWise.Winzor.AppServer
{
	public class HealthCheckWithProcessId : IHealthCheck
	{
		public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			var result = new Dictionary<string, object>
			{
				{ "ProcessId", System.Environment.ProcessId },
			};
			return Task.FromResult(HealthCheckResult.Healthy(null, result));
		}
	}
}
