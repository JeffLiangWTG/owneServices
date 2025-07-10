using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Business
{
	public static class HttpBWHelper
	{
		const string RefComplianceUrlParameter = "/api/v3/compliance/alert-details";
		public const string BorderWiseAPIKey = "NjAyMDRmN2MtMjA1OC00YjY4LWI1ZTEtZmQ0NDY5ZGE3ZTA2";


		public static IEnumerable<ComplianceCountryAlertDetailsResponseModel> GetRefComplianceCommodityAlertList(string baseUrl)
		{
			return Get<IEnumerable<ComplianceCountryAlertDetailsResponseModel>>(baseUrl + RefComplianceUrlParameter);
		}

		static TResponse Get<TResponse>(string urlParameter)
		{
			return RetryHandler(() =>
			{
				using (var client = new HttpClient())
				{
					var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(BorderWiseAPIKey));
					var licenseCode = "WTG-XML-REFQ3EEdkdyjWsHhEpFsK+a+WiNbSdB/ruG3AO+KxMpf0c=";

					var request = new HttpRequestMessage(HttpMethod.Get, urlParameter);
					request.Headers.Add("Authentication", ComputeSha256Hash(decodedString + licenseCode));
					request.Headers.Add("LicenseCode", licenseCode);

					var result = client.SendAsync(request).Result;
					if (result.IsSuccessStatusCode)
					{
						return result.Content.ReadFromJsonAsync<TResponse>().Result;
					}
					throw new HttpRequestException($"Failed to get data from {urlParameter}. Status code: {result.StatusCode}");
				}
			});
		}

		static string ComputeSha256Hash(string rawData)
		{
			using (var sha256Hash = SHA256.Create())
			{
				return Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData)));
			}
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
	}
}
