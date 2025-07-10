using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;

namespace Enterprise.HRM.Common.ServiceTasks
{
	public interface IReviewAllocationProvider
	{
		IEnumerable<BudgetAllocation> LoadBudgetAllocations(Guid reviewProcessPk);
	}

	public class BudgetAllocation
	{
		public decimal BudgetPercent { get; set; }
		public decimal MeritBudgetPercent { get; set; }
		public string EntitlementCode { get; set; } = string.Empty;
		public Guid StaffPK { get; set; }
	}

	public class ReviewAllocationProvider : IReviewAllocationProvider
	{
		public IEnumerable<BudgetAllocation> LoadBudgetAllocations(Guid reviewProcessPk)
			=> LoadBudgetAllocationsFromGlow(reviewProcessPk).GetAwaiter().GetResult();

		async Task<IEnumerable<BudgetAllocation>> LoadBudgetAllocationsFromGlow(Guid reviewProcessPk)
		{
			var url = $"api/rem/allocation?reviewProcessPk={reviewProcessPk}";
			using (var response = await Client.GetAsync(url).ConfigureAwait(false))
			{
				if (response == null || !response.IsSuccessStatusCode)
				{
					var content = response?.Content != null
						? await response.Content.ReadAsStringAsync().ConfigureAwait(false)
						: string.Empty;

					throw new HttpRequestException($"Error getting response from {url}... Code: {(int?)response?.StatusCode ?? -1}, Phrase: {response?.ReasonPhrase}, Content: {content}");
				}

				var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				return JsonConvert.DeserializeObject<BudgetAllocation[]>(responseAsString);
			}
		}

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

	public static class ReviewProcessTypes
	{
		public const string Remuneration = "REM";
		public const string Classification = "CLA";
		public const string Performance = "PER";
		public const string CombinedClassificationAndPerformance = "C&P";
	}
}
