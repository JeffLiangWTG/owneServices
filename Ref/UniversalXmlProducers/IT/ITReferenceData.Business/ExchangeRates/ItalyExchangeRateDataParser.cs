using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public class ItalyExchangeRateDataParser : IExchangeRateDataParser
	{
		readonly ExchangeRateMetaData _metaData;
		readonly ExchangeRateData _data;

		public ItalyExchangeRateDataParser(ExchangeRateMetaData metaData, ExchangeRateData data)
		{
			_metaData = metaData;
			_data = data;
		}

		public ExchangeRateData Parse()
		{
			var rows = new List<RefExchangeRateZZ>();
			foreach (var page in _data.UnProcessedData)
			{
				rows.AddRange(ExtractOnePage(_metaData, page));
			}

			return new ExchangeRateData(_data.UnProcessedData, rows);
		}

		static IEnumerable<RefExchangeRateZZ> ExtractOnePage(ExchangeRateMetaData meta, string page)
		{
			Argument.NotNull(page, nameof(page));

			var currencyRateList = new List<RefExchangeRateZZ>();
			var matches = Regex.Matches(page, DefaultCurrencyContentRegexString, RegexOptions.Singleline).Cast<Match>();
			if (!matches.Any())
			{
				return Enumerable.Empty<RefExchangeRateZZ>();
			}

			foreach (Match match in matches)
			{
				var value = match.Value;
				if (string.IsNullOrEmpty(value.Trim()))
				{
					continue;
				}

				var groups = match.Groups;
				var currency = groups[CurrencyGroup].Value;
				if (!string.IsNullOrEmpty(currency) && decimal.TryParse(groups[RateGroup].Value, out var rate))
				{
					var refExchangeRate = new RefExchangeRateZZ()
					{
						ZZN_RN_NKCountry = Constants.CountryISO,
						ZZN_ExRateType = Constants.RateType,
						ZZN_Rate = rate,
						ZZN_RX_NKExCurrency = currency,
						ZZN_StartDate = Utils.GetFirstDayInMonthDate(meta.Version),
						ZZN_EndDate = Utils.CreateDateTime(meta.Version)
					};
					currencyRateList.Add(refExchangeRate);
				}
			}

			return currencyRateList;
		}

		const string DefaultCurrencyContentRegexString = @"(?<code>[A-Z]{3})\s+[ \t\w]+\s+(?<rate>(\d*\.?\d+))";
		const string CurrencyGroup = "code";
		const string RateGroup = "rate";
	}
}
