using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models
{
	public class Rate
	{
		public string RateQualifier { get; set; } = string.Empty;
		public string RateType { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string FormulaCode { get; set; } = string.Empty;
		public string Countries { get; set; } = string.Empty;
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		#region Calculated Fields
		public string Preference { get; set; } = string.Empty;
		public List<string> CountryCodes { get; set; } = new List<string>();
		public string Formula { get; set; } = string.Empty;
		public string UnitOfMeasureOriginal { get; set; } = string.Empty;
		public string UnitOfMeasureConverted { get; set; } = string.Empty;
		public string Key => CountryCodes.Count > 0 ? string.Join(",", CountryCodes) : RateType;
		#endregion

		public IEnumerable<string> GetTradeGroups(TariffData tariff)
		{
			var results = new List<string>();
			if (tariff.Schedule == SARSSchedule.S1P1)
			{
				if (CountryCodes.Any())
				{
					results = CountryCodes;
				}
				else
				{
					results.Add(SARSMappingHelper.GetTradeGroupFromRateType(RateType));
				}
			}
			else if (tariff.Schedule.HasImportCountry)
			{
				results.AddRange(tariff.ImportCountries);
			}
			else if (tariff.Schedule.HasNoTradeGroup)
			{
				results.Add(null);
			}

			return results;
		}

		public void ProcessUpdates(TariffData tariff, Dictionary<string, string> countryCodes, ILogger logger)
		{
			Preference = SARSMappingHelper.GetPreferenceFromScheduleAndRate(tariff.Schedule.ScheduleType, RateType);

			SetupCountryCodes(countryCodes);

			var matched = SARSFormulaHelper.PopulateFormulaData(tariff, this);

			if (!matched)
			{
				logger.LogError($"Line: [{tariff.LineNumber}] Tariff: [{tariff.TariffCode}] Formula: [{FormulaCode}:{Description}] could not be matched");
			}
		}

		public bool IsValid(TariffData tariff)
		{
			var isValid = !string.IsNullOrWhiteSpace(Formula);

			if (isValid)
			{
				if (RateType == RateTypes.Standard)
				{
					isValid = tariff.Schedule.IsValidForStandardRate;
				}
				else
				{
					isValid = tariff.Schedule == SARSSchedule.S1P1;
				}
			}

			return isValid;
		}

		void SetupCountryCodes(Dictionary<string, string> countryCodes)
		{
			CountryCodes = (Countries ?? string.Empty).ToUpperInvariant()
				.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(country => countryCodes.GetValueOrDefault(country.Trim()))
				.Where(countryCode => !string.IsNullOrEmpty(countryCode))
				.OrderBy(countryCode => countryCode)
				.ToList();
		}
	}
}
