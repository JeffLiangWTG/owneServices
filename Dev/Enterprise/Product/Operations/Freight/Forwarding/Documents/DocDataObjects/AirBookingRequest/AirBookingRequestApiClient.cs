using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class AirBookingRequestApiClient : IDisposable
	{
		public AirBookingRequestApiClient(Uri uri)
		{
			apiClient = ObjectFactory.Get<IApiClient>("HttpClient", uri.ToString());
			apiClient.Accept = new List<string> { "application/xml" };
			apiClient.MediaType = "application/xml";
			if (FreightDataRegistry.Instance.IncludeAuthorizationWhenSendingEBookings.Value)
			{
				apiClient.AccessToken = ((NoResString)"Basic", Environment.Env.CurrentCompany.GetEHubAuthValue());
			}
			apiClient.Timeout = GetRequestTimeoutInSeconds();
			apiClient.RetryCount = 0;
			apiClient.ContentSerializer = new SimpleStringContentSerializer();
		}

		public AirBookingRequestApiClient(IApiClient apiClient)
		{
			this.apiClient = apiClient;
		}

		readonly IApiClient apiClient;

		public IHttpContentSerializer Serializer
		{
			get => apiClient.ContentSerializer;
			set => apiClient.ContentSerializer = value;
		}

		TimeSpan GetRequestTimeoutInSeconds()
		{
			var res = FreightDataRegistry.Instance.EBookingApiTimeoutInSeconds.Value;

			if (res <= 0
				|| res == int.MaxValue)
			{
				res = 5;
			}

			return TimeSpan.FromSeconds(res);
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
