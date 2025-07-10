using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CargoWise.RefDbRepo.Common.Web
{
	public class WtgStatusHealthCheck : IHealthCheck
	{
		readonly ILog log;
		readonly string connectionString;

		public WtgStatusHealthCheck(ILogWrapper logWrapper, string connectionString)
		{
			log = logWrapper.GetLog<WtgStatusHealthCheck>();
			this.connectionString = connectionString;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:Do not catch general exception types")]
		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(connectionString))
				{
					await sqlConnection.OpenAsync(cancellationToken);
					using (var command = sqlConnection.CreateCommand())
					{
						command.CommandText = "SELECT 1";
						await command.ExecuteNonQueryAsync(cancellationToken);
					}
				}

				log.Info("Service is OK");
				return HealthCheckResult.Healthy("Service is OK");
			}
			catch (Exception ex)
			{
				log.Error(ex);
				return HealthCheckResult.Unhealthy($"Service is unavailable: {ex}");
			}
		}
	}
}
