using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.ExchangeRates
{
	public static class DownloadExchangeRates
	{
		public static async Task<DownloadResult> Download(string kursartValue, DateTime startDate, DateTime endDate, HttpClient client, IDateTimeProvider dateTimeProvider)
		{
			client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
			client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
			client.DefaultRequestHeaders.Add("Accept-Language", "de-DE,de;q=0.9,en-US;q=0.8,en;q=0.7,en-DE;q=0.6,es;q=0.5,fr;q=0.4");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36");

			var uriBuilder = new UriBuilder(ApplicationConfig.ExchangeRatesStartURL);
			var paramValues = HttpUtility.ParseQueryString(uriBuilder.Query);
			paramValues.Add(Kursart, kursartValue);

			paramValues.Add(DayOfStartDate, $"{startDate.Day:00}");
			paramValues.Add(MonthOfStartDate, $"{startDate.Month:00}");
			paramValues.Add(YearOfStartDate, $"{startDate.Year}");

			paramValues.Add(DayOfEndDate, $"{endDate.Day:00}");
			paramValues.Add(MonthOfEndDate, $"{endDate.Month:00}");
			paramValues.Add(YearOfEndDate, $"{endDate.Year}");

			paramValues.Add(Sort, SortDefaultValue);
			paramValues.Add(Spalte, SpalteDefaultValue);
			uriBuilder.Query = paramValues.ToString();

			var xmlResponse = await RetryHelper.RetryWithDelayAsync(async() => await client.GetAsync(uriBuilder.Uri));
			var kurseXML = await xmlResponse.Content.ReadAsByteArrayAsync();

			var encoding = DownloadHelper.GetEncoding(kurseXML);
			var result = new DownloadResult
			{
				LastModified = xmlResponse.Content.Headers.LastModified ?? dateTimeProvider.CurrentLocalDateTimeOffset,
				Content = encoding.GetString(kurseXML)
			};
			return result;
		}

		public const string ListedKursartValue = "1";
		public const string UnListedKursartValue = "2";
		public const string IATAKursartValue = "3";

		const string Kursart = "kursart";
		const string DayOfStartDate = "startdatum_tag2";
		const string MonthOfStartDate = "startdatum_monat2";
		const string YearOfStartDate = "startdatum_jahr2";
		const string DayOfEndDate = "enddatum_tag2";
		const string MonthOfEndDate = "enddatum_monat2";
		const string YearOfEndDate = "enddatum_jahr2";
		const string Sort = "sort";
		const string SortDefaultValue = "asc";
		const string Spalte = "spalte";
		const string SpalteDefaultValue = "gueltigkeit";
	}
}
