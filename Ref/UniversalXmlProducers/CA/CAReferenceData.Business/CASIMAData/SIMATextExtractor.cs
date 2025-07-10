using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData
{
	public class SIMATextExtractor : ISIMATextExtractor
	{
		public SIMATextExtractor(IHttpClientHelper helper, string entityMatcherUri)
		{
			Argument.NotNull(helper, nameof(helper));
			Argument.NotNullOrEmpty(entityMatcherUri, nameof(entityMatcherUri));

			this.helper = helper;
			this.entityMatcherCountryUri = string.Format(CultureInfo.InvariantCulture, entityMatcherUri, Constants.EntityMatcherCOUNTRYUri);
			this.entityMatcherCurrencyUri = string.Format(CultureInfo.InvariantCulture, entityMatcherUri, Constants.EntityMatcherCURRENCYUri);
			this.entityMatcherUnitCodeUri = string.Format(CultureInfo.InvariantCulture, entityMatcherUri, Constants.EntityMatcherUNITCODEUri);
		}

		readonly IHttpClientHelper helper;
		readonly string entityMatcherCountryUri;
		readonly string entityMatcherCurrencyUri;
		readonly string entityMatcherUnitCodeUri;

		public string[] ExtractClassificationNumbers(string classificationText)
		{
			Argument.NotNullOrEmpty(classificationText, nameof(classificationText));
			var regex = new Regex(@"[0-9]{4}\.[0-9]{2}\.[0-9]{2}\.[0-9]{2}");
			var matches = regex.Matches(classificationText);
			return matches.Cast<Match>().Select(x => x.Value).Distinct().ToArray();
		}

		public IEnumerable<string> ExtractDutyTypes(string dutyText)
		{
			Argument.NotNullOrEmpty(dutyText, nameof(dutyText));
			var dutyTax = dutyText.ToLowerInvariant().Replace('‑', '-');
			if (dutyTax.Contains("anti-dumping"))
			{
				yield return Constants.AntiDumping;
			}
			if (dutyTax.Contains("countervailing"))
			{
				yield return Constants.Countervailing;
			}
		}

		public string ExtractEffectiveDate(string dutyText)
		{
			Argument.NotNullOrEmpty(dutyText, nameof(dutyText));
			var regex = new Regex("or after ([0-9]{4}-[0-9]{2}-[0-9]{2})");
			var match = regex.Match(dutyText);
			if (match.Success)
			{
				return match.Groups[1].Value;
			}
			regex = new Regex(@"(or after|commencing) ([^\s]*\s+[0-9]{1,2}\,\s*[0-9]{4})");
			match = regex.Match(dutyText);
			if (match.Success)
			{
				return match.Groups[2].Value;
			}
			return null;
		}

		public string ExtractDeterminationDate(string dutyText)
		{
			Argument.NotNullOrEmpty(dutyText, nameof(dutyText));
			var lines = dutyText.Split(new[] { '\r', '\n' }).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
			for (int i = lines.Length - 2; i >= 0; i--)
			{
				var line = lines[i];
				if (!string.IsNullOrEmpty(line))
				{
					if (line.ToLowerInvariant().Contains("final determination"))
					{
						return lines[i + 1];
					}
					else if (line.ToLowerInvariant().Contains("preliminary determination"))
					{
						return lines[i + 1];
					}
					else if (line.ToLowerInvariant().Contains("preliminary decisions"))
					{
						return lines[i + 1];
					}
				}
			}
			return string.Empty;
		}

		public Tuple<string, string> ExtractDutyRate(string dutyText)
		{
			Argument.NotNullOrEmpty(dutyText, nameof(dutyText));
			var regex = new Regex(@"\s+([0-9][0-9,\.,\,]*[0-9])%\s*"); // 101% of export price
			var match = regex.Match(dutyText);
			var unitText = string.Empty;
			var currency = string.Empty;
			if (match.Success)
			{
				var value = decimal.Parse(match.Groups[1].Value) / 100;
				return Tuple.Create(value + " * VFD", string.Empty);
			}
			regex = new Regex(@"\s+([0-9][0-9,\.,\,]*[0-9])\s*(.*)\s+per\s+(.*?)[\,\.]"); // 150 rupees per kilogram
			match = regex.Match(dutyText);
			if (match.Success)
			{
				unitText = match.Groups[3].Value;
				currency = match.Groups[2].Value;
				return Tuple.Create(decimal.Parse(match.Groups[1].Value) + " * " + ConvertToUnitCode(unitText), ConvertToCurrency(currency));
			}
			regex = new Regex(@"\s+([0-9][0-9,\.,\,]*[0-9])\s+(.*)/(.*)"); // 16 TL/kg
			var matches = regex.Matches(dutyText);
			match = matches.Cast<Match>().Where(m => m.Groups[3].Value.Length < 15).FirstOrDefault();
			if (matches.Count > 0 && match != null)
			{
				unitText = match.Groups[3].Value;
				currency = match.Groups[2].Value;
				return Tuple.Create(decimal.Parse(match.Groups[1].Value) + " * " + ConvertToUnitCode(unitText), ConvertToCurrency(currency));
			}
			if (dutyText.Contains("Amount of Subsidy per Kilogram")) // special case
			{
				var idx = dutyText.IndexOf("Amount of Subsidy per Kilogram");
				regex = new Regex(@"\s+([0-9][0-9,\.,\,]*[0-9])\s+(.*)");
				match = regex.Match(dutyText, idx);
				if (match.Success)
				{
					currency = match.Groups[2].Value;
					return Tuple.Create(decimal.Parse(match.Groups[1].Value) + " * [KGM]", ConvertToCurrency(currency));
				}
			}
			if (dutyText.Contains("Amount of Subsidy per Metric Tonne")) // special case
			{
				var idx = dutyText.IndexOf("Amount of Subsidy per Metric Tonne");
				regex = new Regex(@"\s+([0-9][0-9,\.,\,]*[0-9])\s+(.*)");
				match = regex.Match(dutyText, idx);
				if (match.Success)
				{
					currency = match.Groups[2].Value;
					return Tuple.Create(decimal.Parse(match.Groups[1].Value) + " * [TNE]", ConvertToCurrency(currency));
				}
			}

			if (dutyText.ToLower().Contains("all exporters")) // special case on grid. 
			{
				var idx = dutyText.ToLower().IndexOf("all exporters");
				var afterIdx = dutyText.ToLower().Substring(idx);
				regex = new Regex(@"\s+([0-9][0-9,\.,\,]*[0-9])\s+(.*)");

				match = regex.Match(dutyText, idx);
				if (match.Success)
				{
					if (match.Groups.Count > 2 && !match.Groups[2].Value.Contains("%"))
					{
						currency = match.Groups[1].Value;
						var uom = "NMB";
						regex = new Regex(@"^([A-Z]{3}\/[A-Z]{3})");

						if (dutyText.ToLower().Contains("amount of subsidy") && dutyText.Contains("/"))
						{
							uom = dutyText.Substring(dutyText.IndexOf("/") + 1, 3);
						}
						return Tuple.Create(decimal.Parse(match.Groups[1].Value) + " * [" + uom + "]", ConvertToCurrency(currency));
					}
					else
					{
						var value = decimal.Parse(match.Groups[1].Value) / 100;
						return Tuple.Create(value + " * VFD", string.Empty);
					}
				}
			}

			if (dutyText.ToLower().Contains("all other exporters")) // special case on grid. 
			{
				var idx = dutyText.ToLower().IndexOf("all other exporters");
				var afterIdx = dutyText.Substring(idx);
				regex = new Regex(@"\s+([0-9][0-9,\.,\,]*[0-9])\s+(.*)");

				match = regex.Match(dutyText, idx);
				if (match.Success)
				{
					if (match.Groups.Count > 2 && !match.Groups[2].Value.Contains("%"))
					{
						currency = match.Groups[2].Value;
						return Tuple.Create(decimal.Parse(match.Groups[1].Value) + " * [NMB]", ConvertToCurrency(currency));
					}
					else
					{
						var value = decimal.Parse(match.Groups[1].Value) / 100;
						return Tuple.Create(value + " * VFD", string.Empty);
					}
				}
			}
			return Tuple.Create(Constants.DefaultValues.Undefined, string.Empty);
		}

		string ConvertToCurrency(string currency)
		{
			Argument.NotNullOrEmpty(currency, nameof(currency));
			currency = currency.Replace("&nbsp;", string.Empty);
			currency = currency.ToLowerInvariant();
			currency = Regex.Replace(currency, "[^a-z0-9 ]", "");
			currency = currency?.Trim();

			var resultFromWebService = Task.Run(async () => await helper.GetMatchedEntityCodesAsync(entityMatcherCurrencyUri, new string[] { currency }))?.Result?.FirstOrDefault();
			if (resultFromWebService != null)
			{
				return resultFromWebService;
			}
			return currency.Length <= 3 ? currency.ToUpper() : currency.Substring(0, 3).ToUpper();
		}

		string ConvertToUnitCode(string unitText)
		{
			Argument.NotNullOrEmpty(unitText, nameof(unitText));
			unitText = unitText.Replace("&nbsp;", string.Empty);
			unitText = unitText.ToLowerInvariant();
			unitText = Regex.Replace(unitText, "[^a-z0-9 ]", "");
			unitText = unitText?.Trim();

			var resultFromWebService = Task.Run(async () => await helper.GetMatchedEntityCodesAsync(entityMatcherUnitCodeUri, new string[] { unitText }))?.Result?.FirstOrDefault();
			if (resultFromWebService != null)
			{
				return "[" + resultFromWebService + "]";
			}
			throw new NotSupportedException($"Unit of Measure is not supported: {unitText}");
		}

		public string[] ExtractCountryOfOriginOrExport(string[] countryNames)
		{
			Argument.NotNull(countryNames, nameof(countryNames));
			var sanitizedCountryNames = countryNames.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
			var results = Task.Run(async () => await helper.GetMatchedEntityCodesAsync(entityMatcherCountryUri, sanitizedCountryNames))?
				.Result;
			for (int i = 0; i < results.Length; i++)
			{
				if (results[i] == "EU")
				{
					results[i] = "31";
				}
			}
			return results;
		}

		public string ExtractDumpingCase(string referenceNumberText)
		{
			Argument.NotNullOrEmpty(referenceNumberText, nameof(referenceNumberText));
			var regex = new Regex("/[A-Z]+[0-9]*/");
			var match = regex.Match(referenceNumberText.ToUpperInvariant());
			return match.Success ? match.Groups[0].Value.Trim('/') : throw new NotSupportedException($"Cannot extract reference number from {referenceNumberText}");
		}
	}
}
