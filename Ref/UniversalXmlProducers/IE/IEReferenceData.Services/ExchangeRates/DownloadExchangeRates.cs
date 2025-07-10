using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CsvHelper;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Services
{
	public static class DownloadExchangeRates
	{
		public static (Uri ExchangeRateUri, DateTime ExchangeRateDate) GetLatestExchangeRateURL(string exchangeRateIndexUrl)
		{
			Uri exchangeRateIndexUri = null;
			string indexPageText = null;
			try
			{
				exchangeRateIndexUri = new Uri(exchangeRateIndexUrl);
				using (var loader = TextLoader.New(exchangeRateIndexUri, ApplicationConfig.Instance.DefaultHttpUserAgent))
				{
					indexPageText = loader.LoadAsync().Result;
				}
			}
			catch (Exception ex)
			{
				throw new ExchangeRatesException($"Unable to access Exchange Rate index page {exchangeRateIndexUri}: {ex.Message}", ex);
			}
			if (!string.IsNullOrWhiteSpace(indexPageText))
			{
				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(indexPageText);
				var allExchangeRateLinks = htmlDocument.DocumentNode.Descendants("a").Select(linkNode => linkNode.Attributes["href"].Value);

				var latestMatchGroup = allExchangeRateLinks
					.Where(link => RatesLinkRegex.IsMatch(link))
					.Select(link => RatesLinkRegex.Match(link))
					.OrderByDescending(match => MonthsDictionary[match.Groups["month"].Value.ToUpperInvariant()])
					.FirstOrDefault();

				if (latestMatchGroup != null)
				{
					var host = latestMatchGroup.Groups["host"].Value.IfEmpty(() => exchangeRateIndexUri.Host);
					var path = latestMatchGroup.Groups["path"].Value;
					var year = latestMatchGroup.Groups["year"].Value;
					var monthText = latestMatchGroup.Groups["month"].Value;
					var month = MonthsDictionary[monthText.ToUpperInvariant()];
					var month_suffix = latestMatchGroup.Groups["month_suffix"]?.Value ?? string.Empty;
					var extension = latestMatchGroup.Groups["extension"];

					var urlBuilder = new UriBuilder
					{
						Host = host,
						Port = exchangeRateIndexUri.Port,
						Scheme = exchangeRateIndexUri.Scheme,
						Path = latestMatchGroup.Groups[0].Value
					};

					return (urlBuilder.Uri, new DateTime(int.Parse(year, CultureInfo.InvariantCulture), month, 1));
				}
			}
			throw new ExchangeRatesException($"Unable to retrieve the exchange URL from the index page: {exchangeRateIndexUrl}. The data source schema may have changed.");
		}

		public static Dictionary<string, decimal> GetExchangeRatesData(string latestExchangeRateUrl, HtmlDocument htmlDocument)
		{
			try
			{
				var descriptionColumnIndex = htmlDocument.DocumentNode.SelectSingleNode("//table/thead/tr").ChildNodes.Count - 2;
				return htmlDocument.DocumentNode.SelectNodes("//table/tbody/tr").ToDictionary(
					tr => WebUtility.HtmlDecode(tr.Descendants("td").ElementAtOrDefault(descriptionColumnIndex).InnerText).Trim(),
					tr => Convert.ToDecimal(string.Concat(WebUtility.HtmlDecode(tr.Descendants("td").Last().InnerText).Where(x => x == '.' || char.IsDigit(x))).Trim('.'), CultureInfo.InvariantCulture));
			}
			catch (Exception ex)
			{
				throw new ExchangeRatesException($"Unable to generate exchange rate table from URL: {latestExchangeRateUrl} \r\n Exception Message: {ex.Message}");
			}
		}

		public static string GetMonthAndYearRatesRelateTo(HtmlDocument htmlDocument)
		{
			var result = string.Empty;
			var headerColumnsCount = htmlDocument.DocumentNode.SelectSingleNode("//table/thead/tr").ChildNodes.Count;
			var rateColumnRegex = string.Format(CultureInfo.InvariantCulture, "//table/thead/tr/th[{0}]", headerColumnsCount);
			var latestExchangeRateDate = htmlDocument.DocumentNode.SelectSingleNode(rateColumnRegex);
			var match = Regex.Match(latestExchangeRateDate.InnerText, @"\d{2}/\d{2}/\d{2,4}");
			if (match.Success)
			{
				result = match.Groups[0].Value;
			}
			return result;
		}

		public static Dictionary<string, decimal> GetExchangeRatesDataFromCsv(string csvUri)
		{
			var result = new Dictionary<string, decimal>();

			using (var loader = TextLoader.New(new Uri(csvUri), ApplicationConfig.Instance.DefaultHttpUserAgent))
			using (var csvReader = new CsvReader(new StringReader(loader.LoadAsync().Result)))
			{
				csvReader.Read();
				csvReader.ReadHeader();

				while (csvReader.Read())
				{
					var currencyCode = csvReader.GetField<string>("Currency_Code");
					var rate = csvReader.GetField<decimal>("Rate");
					result.Add(currencyCode, rate);
				}
			}

			return result;
		}

		static readonly ImmutableDictionary<string, int> MonthsDictionary = new Dictionary<string, int>()
						{
							{ "JANUARY", 1 }, { "FEBRUARY", 2 }, { "MARCH", 3 }, { "APRIL", 4 },
							{ "MAY", 5 }, { "JUNE", 6 }, { "JULY", 7 }, { "AUGUST", 8 },
							{ "SEPTEMBER", 9 }, { "OCTOBER", 10 }, { "NOVEMBER", 11 }, { "DECEMBER", 12 },
							{ "JAN", 1 }, { "FEB", 2 }, { "MAR", 3 }, { "APR", 4 },
							{ "JUN", 6 }, { "JUL", 7 }, { "AUG", 8 }, { "SEP", 9 },
							{ "OCT", 10 }, { "NOV", 11 }, { "DEC", 12 }
						}.ToImmutableDictionary();

		static readonly Regex RatesLinkRegex = new Regex(@"(?<host>[\w\-.]+revenue.ie)?(?<path>\/?([\w\-]+\/)+)(?<year>20\d{2})/(?<month>[a-zA-Z]+)(?<month_suffix>[^\.]*)(?<extension>\.\w+)");
	}
}
