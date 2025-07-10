using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate
{
	public static class ParserHelper
	{
		public static DateTime GetValidPublishDate(DateTime today, DateTime? publishDateFromUrl, DateTime? publishDateFromFile)
		{
			var result = new[] { publishDateFromUrl, publishDateFromFile }.Where(x => x.HasValue).OrderBy(x => Math.Abs(today.Subtract(x.Value).Days)).FirstOrDefault();

			if (result != null)
			{
				var isValid = Math.Abs(today.Subtract(result.Value).Days) < ApplicationConfig.Instance.ExchangeRateDelayDays;
				if (isValid)
				{
					return result.Value;
				}
				else
				{
					throw new InvalidOperationException("Can not find a valid publish date from page or file.");
				}
			}
			else
			{
				throw new InvalidOperationException("Can not find the publish date from page and file.");
			}
		}

		public static bool IsUrlForExchangeRateExcelFile(HtmlNode htmlNode)
		{
			var result = false;
			if (htmlNode.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
			{
				var href = htmlNode.Attributes["href"]?.Value?.Trim().ToUpper(CultureInfo.InvariantCulture);
				result = !string.IsNullOrEmpty(href) && href.EndsWith(@"XLSX", StringComparison.Ordinal);
			}
			return result;
		}
	}
}
