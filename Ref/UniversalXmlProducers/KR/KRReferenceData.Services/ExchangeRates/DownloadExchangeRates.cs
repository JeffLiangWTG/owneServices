using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.ExchangeRates.Services
{
	public static class DownloadExchangeRates
	{
		public static async Task<string> Download(ExchangeRateTypes rateType, DateTime requestDate, HttpClient client)
		{
			client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
			client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
			client.DefaultRequestHeaders.Add("Accept-Language", "ko,en;q=0.9,en-US;q=0.8");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36 Edg/106.0.1370.34");

			var uriBuilder = new UriBuilder(ApplicationConfig.ExchangeRatesStartURL);
			var paramValues = HttpUtility.ParseQueryString(uriBuilder.Query);
			paramValues.Add(WebServiceParameters.RequestDate, requestDate.ToString(WebServiceParameters.RequestDateFormat, null));
			paramValues.Add(WebServiceParameters.MessageType, ((int)rateType).ToString(null, null));

			uriBuilder.Query = paramValues.ToString();

			var xmlResponse = await client.GetAsync(uriBuilder.Uri);
			if (xmlResponse.StatusCode != System.Net.HttpStatusCode.OK)
			{
				return xmlResponse.ReasonPhrase;
			}
			else
			{
				var contentXML = await xmlResponse.Content.ReadAsByteArrayAsync();
				return System.Text.Encoding.UTF8.GetString(contentXML);
			}
		}

		static class WebServiceParameters
		{
			public const string RequestDate = "qryYymmDd";
			public const string MessageType = "imexTp";
			public const string RequestDateFormat = "yyyyMMdd";
		}
	}
	public enum ExchangeRateTypes
	{
		None = 0,
		Export = 1,
		Import = 2
	}
}
