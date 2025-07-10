using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public class ExchangeRateDownloader
	{
		public string DownloadData(DateTime currencyDate)
		{
			var retryCount = 0;
			while (true)
			{
				try
				{
					return DownloadDataCore(currencyDate);
				}
				catch (Exception ex)
				{
					if (++retryCount >= AppConfig.ExchangeRate.MaxRetry)
					{
						throw new UnhandledApplicationException($"Failed to load exchange rate from the source after {retryCount} retries", ex);
					}

					Thread.Sleep(SleepInterval);
				}
			}
		}

		string DownloadDataCore(DateTime currencyDate)
		{
			using (var request = new HttpRequestMessage())
			using (var client = GetHttpClient())
			{
				var requestUri = new Uri(AppConfig.ExchangeRate.Url);
				var param = new Dictionary<string, string>
				{
					{ "currencyCode", "ALL" },
					{ "currencyDate", currencyDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) }
				};

				request.Method = HttpMethod.Post;
				request.Headers.Host = requestUri.Host;
				request.RequestUri = requestUri;
				request.Content = new StringContent(JsonSerializer.Serialize(param), Encoding.UTF8, "application/json");
				request.Content.Headers.ContentLength = 50;

				var response = client.SendAsync(request).Result;
				Helper.Assume(response.IsSuccessStatusCode,
					$"Failed to load exchange rate from the source, State Code:{response.StatusCode}");

				var responseContent = response.Content.ReadAsStringAsync().Result;
				Helper.Assume(!string.IsNullOrEmpty(responseContent),
					"Failed to load exchange rate from the source, Response is empty");

				return responseContent;
			}
		}

		protected virtual HttpClient GetHttpClient() => new HttpClient();

		protected virtual int SleepInterval => AppConfig.ExchangeRate.SleepInterval;
	}
}
