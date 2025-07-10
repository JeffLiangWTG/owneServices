using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.Foundation.Http;

namespace Enterprise.Freight.AIS
{
	public class AisWebApiClient : IAisWebApiClient
	{
		readonly IApiClient apiClient;

		public AisWebApiClient()
		{
			var resilientDelegatingHandler = new ResilientDelegatingHandler()
			{
				InnerHandler = new AisAuthorizationDelegatingHandler
				{
					InnerHandler = new HttpClientHandler()
				}
			};

			var httpClient = ObjectFactory.Get<IHttpClientFactory>().CreateNew(resilientDelegatingHandler, TimeSpan.FromSeconds(10));
			httpClient.BaseAddress = new Uri(FreightDataRegistry.Instance.RouteVisualizerApiUrl.Value);

			apiClient = ObjectFactory.Get<IApiClient>("HttpClient", httpClient);
		}

		public async Task<PortCallPage> PortCallsAsync(string vesselImo, int? pageIndex, int? pageSize, string timeFrom, string timeTo, string carrierCode, string voyageNumber, string departurePortUnloco, string arrivalPortUnloco, CancellationToken cancellationToken)
		{
			if (vesselImo == null)
			{
				throw new ArgumentNullException(nameof(vesselImo));
			}

			var urlBuilder = new StringBuilder();
			urlBuilder.AppendFormat((NoResString)"v1/vessels/{0}/portCalls?", Uri.EscapeDataString(vesselImo));

			if (pageIndex != null)
			{
				urlBuilder.AppendFormat("pageIndex={0}&", Uri.EscapeDataString(pageIndex.ToString()));
			}

			if (pageSize != null)
			{
				urlBuilder.AppendFormat("pageSize={0}&", Uri.EscapeDataString(pageSize.ToString()));
			}

			if (timeFrom != null)
			{
				urlBuilder.AppendFormat("timeFrom={0}&", Uri.EscapeDataString(timeFrom));
			}

			if (timeTo != null)
			{
				urlBuilder.AppendFormat("timeTo={0}&", Uri.EscapeDataString(timeTo));
			}

			if (carrierCode != null)
			{
				urlBuilder.AppendFormat("carrierCode={0}&", Uri.EscapeDataString(carrierCode));
			}

			if (voyageNumber != null)
			{
				urlBuilder.AppendFormat("voyageNumber={0}&", Uri.EscapeDataString(voyageNumber));
			}

			if (departurePortUnloco != null)
			{
				urlBuilder.AppendFormat("departurePortUnloco={0}&", Uri.EscapeDataString(departurePortUnloco));
			}

			if (arrivalPortUnloco != null)
			{
				urlBuilder.AppendFormat("arrivalPortUnloco={0}&", Uri.EscapeDataString(arrivalPortUnloco));
			}

			urlBuilder.Length--;

			var response = apiClient.GetAsync<PortCallPage>(urlBuilder.ToString(), cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
			await response.EnsureSuccessStatusCodeAsync(true);

			if (!response.IsSuccessStatusCode)
			{
				throw new AisWebApiException();
			}

			return response.Content;
		}

		public void Dispose()
		{
			apiClient.Dispose();
		}
	}
}
