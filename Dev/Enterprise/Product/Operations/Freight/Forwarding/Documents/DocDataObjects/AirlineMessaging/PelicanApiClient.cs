using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging
{
	public class PelicanApiClient : IDisposable
	{
		readonly IApiClient apiClient;

		public PelicanApiClient(Uri uri)
		{
			apiClient = ObjectFactory.Get<IApiClient>("HttpClient", uri.ToString());
			apiClient.Accept = new List<string> { "application/xml" };
			apiClient.AccessToken = ((NoResString)"Basic", Environment.Env.CurrentCompany.GetEHubAuthValue());
			apiClient.ContentSerializer = new SimpleStringContentSerializer();
			apiClient.MediaType = "application/xml";
			apiClient.RetryCount = FreightDataRegistry.Instance.PelicanApiRetryAttempts.Value;
			apiClient.Timeout = TimeSpan.FromSeconds(FreightDataRegistry.Instance.PelicanApiTimeout.Value);
		}

		public IApiResponse<string> Post(string content, CancellationToken token)
		{
			return apiClient.PostAsync<string>(string.Empty, content, token).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public void Dispose()
		{
			apiClient.Dispose();
		}
	}
}
