using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Business
{
	public static class HttpDpsWebHelper
	{
		const string RefComplianceUrlParameter = "/api/v4.4/compliance-lists";

		public static IEnumerable<RefComplianceListResponse> GetRefComplianceList(string baseUrl, string token)
		{
			return Get<IEnumerable<RefComplianceListResponse>>(baseUrl + RefComplianceUrlParameter, token);
		}

		static TResponse Get<TResponse>(string urlParameter, string token)
		{
			return RetryHandler(() =>
			{
				using (var client = new HttpClient())
				{
					AppendLicenceAndAuthorizationToHeader(client, token);
					return client.GetFromJsonAsync<TResponse>(urlParameter).Result;
				}
			});
		}

		static TResponse RetryHandler<TResponse>(Func<TResponse> action)
		{
			var tryCount = 3;

			do
			{
				try
				{
					return action();
				}
				catch (Exception ex) when (ex is HttpRequestException || ex is AggregateException aggregateEx && aggregateEx.InnerException is HttpRequestException)
				{
					if (--tryCount <= 0)
					{
						throw;
					}
				}
			}
			while (true);
		}

		static void AppendLicenceAndAuthorizationToHeader(HttpClient request, string token)
		{
			request.DefaultRequestHeaders.Add("LicenceCode", "WTG-XML-REFQ3EEdkdyjWsHhEpFsK+a+WiNbSdB/ruG3AO+KxMpf0c=");
			request.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
		}
	}
}
