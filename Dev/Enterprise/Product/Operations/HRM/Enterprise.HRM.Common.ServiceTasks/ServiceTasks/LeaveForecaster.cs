using System;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions.LeaveEngine;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;

namespace Enterprise.HRM.Common
{
	public class LeaveForecaster : ILeaveForecaster
	{
		async Task<LeaveForecasterResult> GetForcastResultAsync(string staffCode, DateTime accrualDate, DateTime? lastAccrualDate, bool? leaveOnAccrualDate, bool onlyIncludeRequestedSnapshots)
		{
			var url = $"api/leave/forecast?staffCode={staffCode}&accrualDate={accrualDate:yyyy-MM-dd}&onlyIncludeRequestedSnapshots={onlyIncludeRequestedSnapshots.ToString().ToLowerInvariant()}";
			if (lastAccrualDate != null)
			{
				url += $"&lastAccrualDate={lastAccrualDate:yyyy-MM-dd}";
			}

			if (leaveOnAccrualDate != null)
			{
				url += $"&leaveOnAccrualDate={leaveOnAccrualDate.ToString().ToLowerInvariant()}";
			}

			using var response = await Client.GetAsync(url).ConfigureAwait(false) ?? throw new HttpRequestException($"Error getting response from {url}.");
			var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				throw new HttpRequestException($"Error getting response from {url}... Code: {(int)response.StatusCode}, Phrase: {response.ReasonPhrase}, Content: {content}");
			}

			return JsonConvert.DeserializeObject<LeaveForecasterResult>(content);
		}

		public LeaveForecasterResult Forecast(string staffCode, DateTime accrualDate, DateTime? lastAccrualDate, bool? leaveOnAccrualDate, bool onlyIncludeRequestedSnapshots)
			=> GetForcastResultAsync(staffCode, accrualDate, lastAccrualDate, leaveOnAccrualDate, onlyIncludeRequestedSnapshots).GetAwaiter().GetResult();

		IGlowServiceClient GetGlowClient()
		{
			var serviceUri = GlowRegistry.Instance.GlowServiceUri;
			if (!string.IsNullOrEmpty(serviceUri) && serviceUri != "/")
			{
				var clientFactory = ObjectFactory.Get<IGlowServiceClientFactory>();

				return clientFactory.Create(new Uri(serviceUri));
			}

			throw new InvalidOperationException("Glow Service Uri is not set.");
		}

		IGlowServiceClient Client => client ?? (client = GetGlowClient());
		IGlowServiceClient client;
	}
}
