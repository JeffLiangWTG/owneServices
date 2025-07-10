using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions.HR;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;

namespace Enterprise.HRM.Common.ServiceTasks
{
	public class HierarchyProvider : IReviewHierarchyProvider
	{
		public IEnumerable<ReviewHierarchyNode> DetermineHierarchy(Guid process)
			=> GetHierarchyFromGlow(process).GetAwaiter().GetResult();

		async Task<IEnumerable<ReviewHierarchyNode>> GetHierarchyFromGlow(Guid reviewProcessPk)
		{
			var url = $"api/reviewprocess/{reviewProcessPk}/hierarchy";
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
				return JsonConvert.DeserializeObject<ReviewHierarchyNode[]>(responseAsString);
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
}
