using System;
using System.Net.Http;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public sealed class HttpClientWrapper : IHttpClient
	{
		public HttpClientWrapper(HttpClient httpClient)
		{
			this.httpClient = httpClient;
		}

		readonly HttpClient httpClient;

		string IHttpClient.Post(string requestUri, HttpContent content)
		{
			var response = httpClient.PostAsync(new Uri(requestUri), content)?.Result;
			return response.Content.ReadAsStringAsync()?.Result;
		}

		string IHttpClient.Get(string requestUri)
		{
			var response = httpClient.GetAsync(new Uri(requestUri))?.Result;
			return response.Content.ReadAsStringAsync()?.Result;
		}
	}
}
