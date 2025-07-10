using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.CNReferenceData.Services;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class CNExchangeRateParser
	{
		#region Fields

		readonly string sourceUrl;

		#endregion

		#region Constants
		const string ExchangeRateTableRegexPattern = @"\<table cellpadding=""0"" align=""left"" cellspacing=""0"" width=""100%""\>.+?\</table\>";
		#endregion

		DateTime Today { get; set; }

		public CNExchangeRateParser(string sourceUrl, DateTime? today = null)
		{
			Argument.NotNull(sourceUrl, nameof(sourceUrl));

			this.sourceUrl = sourceUrl;
			Today = today ?? DateTime.UtcNow.ToChinaStandardTime().Date;
		}

		async Task<string> GetSearchResult(IHttpHandler httpHandler)
		{
			var response = await httpHandler.GetAsync(sourceUrl);
			return await response.Content.ReadAsStringAsync();
		}

		public void ExportToXMLFile(string outputFilePath, IHttpHandler httpHandler)
		{
			Argument.NotNull(httpHandler, nameof(httpHandler));

			var requiredCurrencies = GlobalOption.Instance.Setting.CurrencyInChineseMap;

			var xmlDoc = new XmlDocument();
			var exchangeRates = new List<RefExchangeRateZZ>();
			DateTime latestPublishDate = DateTime.MinValue;

			Match match = null;

			var retryCount = 0;
			while (true)
			{
				try
				{
					match = ExtractRateTableFromHtml(GetSearchResult(httpHandler)?.Result.FilterHtml());
					break;
				}
				catch (AggregateException)
				{
					if (++retryCount >= 3)
					{
						throw;
					}

					Thread.Sleep(RetryInterval);
				}
			}

			if (match?.Groups == null || match.Groups.Count == 0)
			{
				throw new NotSupportedException("Exchange rate table is not found, Not Found element.");
			}

			try
			{
				xmlDoc.LoadXml(match.Groups[0].Value);

				var rateNodes = xmlDoc.SelectNodes("table/tr");
				var headerArray = rateNodes.Cast<XmlElement>().FirstOrDefault();
				if (headerArray == null)
				{
					throw new NotSupportedException("Exchange rate table is not found, Not Found element.");
				}

				var headerTitles = headerArray.ChildNodes.Cast<XmlElement>().Select(x => x.InnerText).ToArray();
				var currencyNameIndex = Array.IndexOf(headerTitles, "货币名称");
				var averageBuySellPriceIndex = Array.IndexOf(headerTitles, "中行折算价");
				var publishDateIndex = Array.IndexOf(headerTitles, "发布日期");

				if (currencyNameIndex < 0 || averageBuySellPriceIndex < 0 || publishDateIndex < 0)
				{
					throw new NotSupportedException("Could not find '货币名称' or '中行折算价' or '发布日期' in table.");
				}

				foreach (var node in rateNodes.Cast<XmlElement>().Skip(1).ToArray())
				{
					var currency = node.ChildNodes[currencyNameIndex]?.InnerText;
					if (string.IsNullOrEmpty(currency) || !requiredCurrencies.ContainsKey(currency))
					{
						continue;
					}

					var averageBuySellPrice = node.ChildNodes[averageBuySellPriceIndex]?.InnerText;
					if (string.IsNullOrEmpty(averageBuySellPrice))
					{
						continue;
					}

					var publishDate = Convert.ToDateTime(node.ChildNodes[publishDateIndex]?.InnerText, CultureInfo.InvariantCulture);
					if (publishDate.CompareTo(latestPublishDate) > 0)
					{
						latestPublishDate = publishDate;
					}
					if (publishDate.Date != Today)
					{
						throw new NotSupportedException($"Publish Date {publishDate:yyyy-MM-dd HH:mm:ss} is not today.");
					}

					var rate = decimal.Parse(averageBuySellPrice, CultureInfo.InvariantCulture) / 100;
					var rateScale = rate.Scale();
					var rateElement = new RefExchangeRateZZ
					{
						ZZN_RX_NKExCurrency = requiredCurrencies[currency],
						ZZN_Rate = Utils.FormatCurrency(rate, rateScale),
					};

					exchangeRates.Add(rateElement);
				}
			}
			catch (XmlException ex)
			{
				throw new NotSupportedException("Exchange rate table cannot be parsed to xml", new InvalidOperationException(match.Groups[0].Value, ex));
			}

			var currencies = exchangeRates.Select(r => r.ZZN_RX_NKExCurrency).ToArray();
			var currenciesNotFound = requiredCurrencies.Where(x => !currencies.Contains(x.Value)).OrderBy(x => x.Value).Select(x => x.Key).ToArray();
			if (currenciesNotFound.Length > 0)
			{
				throw new NotSupportedException($"Could not find rates for {string.Join(",", currenciesNotFound)}.");
			}

			var nextMonth = Today.AddMonths(1);
			var firstDayOfMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);
			var startDate = firstDayOfMonth;
			var endDate = firstDayOfMonth.AddMonths(1).AddDays(-1);

			Helper.ExportToXMLFile("Ref Exchange Rate CN", outputFilePath, Helper.GetRefExchangeRateZZConfiguration(startDate, endDate), latestPublishDate, exchangeRates, UpdateType.Partial);
		}

		static Match ExtractRateTableFromHtml(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return null;
			}
			var match = Regex.Match(html, ExchangeRateTableRegexPattern, RegexOptions.Singleline);
			if (match.Groups.Count != 1)
			{
				return null;
			}

			return match;
		}

		public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMinutes(3);
	}
}
