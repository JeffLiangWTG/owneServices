using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public sealed class ItalyExchangeRateMetaDataParser : IExchangeRateMetaDataParser
	{
		readonly ExchangeRateMetaData _metaData;

		public ItalyExchangeRateMetaDataParser(ExchangeRateMetaData metaData)
		{
			_metaData = metaData;
		}

		ExchangeRateMetaData IExchangeRateMetaDataParser.Parse()
		{
			_metaData.DataLocation = ExtractDataLocation();
			var sourceUri = new Uri(_metaData.Source);
			if (!_metaData.DataLocation.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				_metaData.DataLocation = sourceUri.Scheme + "://" + sourceUri.Authority + _metaData.DataLocation;
			}
			_metaData.Version = ExtractDataVersion();
			return _metaData;
		}

		const string DataLocationRegexString = @"<a\s+aria-label=""Scarica il documento.*""\s*href\s*=\s*""([^""]*)""[^>]*>";
		const string DataLocationNotFoundErrorMsg = "Link to 'Monthly Foreign Currency Exchange Rate' is not found";
		const string DataVersionYearOrMonthNotFoundErrorMsg = "year or month not found in content";
		const string DataVersionYearAndMonth = @"<a\s+aria-label=""Scarica il documento.*""\s*href\s*=\s*"".*""[^>]*>(?<month>[a-zA-Z]{0,}?)\s(?<year>[0-9]*?)<\/a>";

		string ExtractDataLocation()
		{
			var match = new Regex(DataLocationRegexString).Match(_metaData.Content);
			if (match.Groups.Count != 2)
			{
				throw new InvalidOperationException(DataLocationNotFoundErrorMsg);
			}
			return match.Groups[1].Value;
		}

		string ExtractDataVersion()
		{
			var match = new Regex(DataVersionYearAndMonth).Match(_metaData.Content);
			if (match.Groups.Count != 3)
			{
				throw new InvalidOperationException(DataVersionYearOrMonthNotFoundErrorMsg);
			}

			var month = match.Groups["month"].Value;
			var year = match.Groups["year"].Value;

			return Utils.GetLastDayInMonthDateString(int.Parse(year, CultureInfo.InvariantCulture), _italianMonthToNumberMapping[month.ToUpperInvariant()]);
		}

		readonly Dictionary<string, int> _italianMonthToNumberMapping = new Dictionary<string, int>()
		{
			{ "DICEMBRE", 12 },
			{ "NOVEMBRE", 11 },
			{ "OTTOBRE", 10 },
			{ "SETTEMBRE", 9 },
			{ "AGOSTO", 8 },
			{ "LUGLIO", 7 },
			{ "GIUGNO", 6 },
			{ "MAGGIO", 5 },
			{ "APRILE", 4 },
			{ "MARZO", 3 },
			{ "FEBBRAIO", 2 },
			{ "GENNAIO", 1 }
		};
	}
}
