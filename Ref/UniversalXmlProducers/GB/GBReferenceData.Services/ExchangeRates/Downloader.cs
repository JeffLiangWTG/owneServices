using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.ExchangeRates
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public class Downloader
	{
		public Downloader(IDateTimeProvider dateTimeProvider, IWebClientWrapper webClientWapper, StringBuilder errorCollector)
		{
			this.dateTimeProvider = dateTimeProvider;
			webClient = webClientWapper;
			ErrorCollector = errorCollector;
		}
		readonly IDateTimeProvider dateTimeProvider;
		readonly IWebClientWrapper webClient;
		StringBuilder ErrorCollector;

		public (DateTime PublishDate, IEnumerable<ExchangeRate> Rates) GetExchangeRates()
		{
			var publishDate = DateTime.MinValue;
			var exchangeRates = new List<ExchangeRate>();

			foreach (var url in GetDownloadURLs())
			{
				var data = ProcessUrl(url);
				if (data.Rates.Any())
				{
					if (data.PublishDate > publishDate)
					{
						publishDate = data.PublishDate;
					}
					exchangeRates.AddRange(data.Rates);
				}
			}

			return (publishDate, exchangeRates
									.OrderBy(x => x.StartDate)
									.ThenBy(x => x.Currency));
		}

		protected (DateTime PublishDate, IEnumerable<ExchangeRate> Rates) ProcessUrl(string url)
		{
			var exchangeRates = new List<ExchangeRate>();
			var datedContent = DownloadXml(url);
			var content = datedContent.Content;
			var publishDate = datedContent.LastModified;

			if (!string.IsNullOrWhiteSpace(content))
			{
				try
				{
					var xDoc = XDocument.Parse(content);

					var month = xDoc.Element(MonthElement);
					if (month != null)
					{
						var dates = ExtractFileDates(month);

						if (dates.Success)
						{
							exchangeRates.AddRange(month.Elements(ExchangeRateElement)
													.Select(e => ConvertXElementToModel(dates.StartDate, dates.EndDate, e))
													.Where(x => x.IsValid())
													.GroupBy(x => x.Currency)
													.Select(g => g.First()));
						}
					}
					else
					{
						ErrorCollector.AppendLine(CultureInfo.InvariantCulture, $"'{MonthElement}' element missing from XML for URL {url}");
					}
				}
				catch
				{
					ErrorCollector.AppendLine(CultureInfo.InvariantCulture, $"Failed to parse xml content from URL {url}");
				}
			}

			return (publishDate, exchangeRates);
		}

		protected IEnumerable<string> GetDownloadURLs()
		{
			var baseUrl = ConfigurationProvider.ExchangeRateBaseUrl;
			var monthFormat = ConfigurationProvider.ExchangeRateMonthFormat;
			var currentDate = dateTimeProvider.UTCDateTime;
			var date = new DateTime(currentDate.Year, currentDate.Month, 1);

			yield return string.Format(CultureInfo.CurrentCulture, baseUrl, date.ToString(monthFormat, CultureInfo.CurrentCulture));
			yield return string.Format(CultureInfo.CurrentCulture, baseUrl, date.AddMonths(1).ToString(monthFormat, CultureInfo.CurrentCulture));
		}

		protected (DateTime LastModified, string Content) DownloadXml(string url)
		{
			try
			{
				return webClient.GetDatedContent(url);
			}
			catch
			{
				ErrorCollector.AppendLine(CultureInfo.InvariantCulture, $"Failed to download xml from URL {url}");
				return (DateTime.Now, string.Empty);
			}
		}

		protected (DateTime StartDate, DateTime EndDate, bool Success) ExtractFileDates(XElement element)
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;
			var success = false;

			var periodAttrib = element.Attribute(PeriodAttribute);
			if (periodAttrib != null)
			{
				// Expecting "01/Jan/2021 to 31/Jan/2021"
				var regStr = new Regex(@"(\d){2}/[A-Za-z]{3}/(\d){4}");

				var matches = regStr.Matches(periodAttrib.Value);
				if (matches.Count == 2)
				{
					startDate = DateTime.ParseExact(matches[0].Value, "dd/MMM/yyyy", CultureInfo.InvariantCulture);
					endDate = DateTime.ParseExact(matches[1].Value, "dd/MMM/yyyy", CultureInfo.InvariantCulture);
					endDate = endDate.Add(new TimeSpan(23, 59, 0));
					success = true;
				}
				else
				{
					ErrorCollector.AppendLine(CultureInfo.InvariantCulture, $"Could not extract date range from period '{periodAttrib.Value}'");
				}
			}
			else
			{
				ErrorCollector.AppendLine("Could not extract date range as 'period' attribute is missing");
			}

			return (startDate, endDate, success);
		}

		protected static ExchangeRate ConvertXElementToModel(DateTime startdate, DateTime endDate, XElement element)
		{
			var rate = new ExchangeRate
			{
				StartDate = startdate,
				EndDate = endDate
			};

			rate.Currency = element.Element(CurrencyCodeElement)?.Value ?? string.Empty;
			if (decimal.TryParse(element.Element(RateElement)?.Value, out var newRate))
			{
				rate.Rate = newRate;
			}

			return rate;
		}

		const string MonthElement = "exchangeRateMonthList";
		const string ExchangeRateElement = "exchangeRate";
		const string PeriodAttribute = "Period";
		const string CurrencyCodeElement = "currencyCode";
		const string RateElement = "rateNew";
	}
}
