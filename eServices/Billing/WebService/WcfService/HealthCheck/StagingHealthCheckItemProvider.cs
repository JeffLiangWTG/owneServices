using System;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.eServices.Billing.DataAccess;
using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace CargoWise.eServices.Billing.WcfService
{
	public class StagingHealthCheckItemProvider : IHealthCheckItemProvider
	{
		public string Name => "Staging";

		public Task<HealthCheckItem> CheckHealthAsync()
		{
			try
			{
				if (IsStagingBehind(thresholdInHours, out string message))
				{
					return Task.FromResult(HealthCheckItem.Error($"Staging is behind by more than {thresholdInHours} hours. {message}"));
				}

				return Task.FromResult(HealthCheckItem.Info($"Healthy. {message}"));
			}
			catch (Exception ex)
			{
				var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check. Message: {1}", ex.GetType().FullName, ex.Message);
				return Task.FromResult(HealthCheckItem.Error(errorDescription));
			}
		}

		bool IsStagingBehind(int numberOfHours, out string message)
		{
			message = string.Empty;
			var oldest = BillingRepository.OldestSystemCreateUTCInStaging();

			if (!oldest.HasValue)
			{
				message = "Staging is empty.";
				return false;
			}

			var oldestUTC = oldest.Value;
			var checkTimeUTC = UtcNow();
			var timeDifferenceInHours = (checkTimeUTC - oldestUTC).TotalHours;

			if (timeDifferenceInHours < numberOfHours)
			{
				message = $"The time difference in hours '{timeDifferenceInHours:F2}' between the oldest Staging record created time UTC '{oldestUTC.ToString("dd-MMM-yyyy hh:mm:ss tt")}' and check time UTC '{checkTimeUTC.ToString("dd-MMM-yyyy hh:mm:ss tt")}' is less than {numberOfHours}.";
				return false;
			}
			else
			{
				message = $"The time difference in hours '{timeDifferenceInHours:F2}' between the oldest Staging record created time UTC '{oldestUTC.ToString("dd-MMM-yyyy hh:mm:ss tt")}' and check time UTC '{checkTimeUTC.ToString("dd-MMM-yyyy hh:mm:ss tt")}' is greater than or equal to {numberOfHours}.";
				return true;
			}
		}

		internal virtual IBillingRepository BillingRepository { get; } = new BillingRepository();
		internal virtual DateTime UtcNow() => DateTime.UtcNow;
		const int thresholdInHours = 2;
	}
}
