using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models
{
	public class TariffData
	{
		public string LineNumber { get; set; }
		public string ItemNumber { get; set; }
		public string Heading { get; set; }
		public string Code { get; set; }
		public string SubHeading { get; set; }
		public string CheckDigit { get; set; }

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public string StatisticalUnitOriginal { get; set; }
		public string Description { get; set; }
		public string ImportedFrom { get; set; }
		public string GovernmentGazetteNoticeNumber { get; set; }
		public string ScheduleTypeCode { get; set; }

		public List<Rate> Rates { get; set; } = new List<Rate>();

		#region Calculated Fields
		public List<string> ImportCountries { get; set; } = new List<string>();
		public string TariffCode { get; set; }
		public string RelationshipTariffCode { get; set; }

		public string StatisticalUnitConverted { get; set; }

		public SARSSchedule Schedule { get; set; }
		public short UniqueId { get; set; } = -1;
		public bool TariffKeyExists { get; set; }
		public Dictionary<string, string> AdditionalUOMs { get; set; } = new Dictionary<string, string>();
		public Dictionary<string, string> AdditionalAttributes { get; set; } = new Dictionary<string, string>();

		public bool IsHeading => (!string.IsNullOrWhiteSpace(SubHeading) || (ItemNumber?.Length == 6 && string.IsNullOrWhiteSpace(Code))) && string.IsNullOrWhiteSpace(CheckDigit);
		public bool AllowForExpiration => Schedule?.ScheduleType == Schedules.S1P1;
		public bool IsAddedForExpiration { get; set; }
		#endregion

		public DateTime CalcStartDate() => CommonHelper.CalcMinDate(StartDate);
		public DateTime CalcEndDate() => CommonHelper.CalcMaxDate(EndDate);

		public bool IsValidTariff(Header header, ILogger logger)
		{
			var errorBuilder = new StringBuilder();

			var isValid = Validate(Schedule?.Schedule > 0, errorBuilder, "Invalid schedule") &&
						  Validate(!string.IsNullOrWhiteSpace(TariffCode), errorBuilder, "Tariff code required") &&
						  Validate(!string.IsNullOrWhiteSpace(Description), errorBuilder, "Description is required");

			if (isValid)
			{
				if (string.IsNullOrWhiteSpace(ItemNumber))
				{
					isValid = !string.IsNullOrWhiteSpace(SubHeading); // "SubHeading is required";
				}
				else if (ItemNumber.Length < 6) // nnn.nn
				{
					isValid = Validate(false, errorBuilder, "Invalid item number");
				}
				else if (ItemNumber.Length == 6 && string.IsNullOrWhiteSpace(Code))
				{
					isValid = Rates.Any(); // Heading/SubHeading
				}
			}

			if (isValid && !IsAddedForExpiration && string.IsNullOrWhiteSpace(CheckDigit) && header.TransactionType != TransactionType.Deletion)
			{
				isValid = Rates.Any(); // Heading/SubHeading - No error
			}

			if (isValid && Schedule.ScheduleType == Schedules.S1P1)
			{
				isValid = Validate(!string.IsNullOrEmpty(CheckDigit), errorBuilder, "CheckDigit required for 1P1 tariff");
			}

			if (isValid && !IsAddedForExpiration && !string.IsNullOrWhiteSpace(CheckDigit) && header.TransactionType != TransactionType.Deletion)
			{
				isValid = Validate(Rates.Any(), errorBuilder, "No rates");
			}

			if (isValid)
			{
				isValid = Validate((CalcEndDate() - CalcStartDate()).TotalSeconds > 60, errorBuilder, "End date should be at least a minute after start date"); // Ensure at least 1 minute difference for smalldatetime
			}

			if (!isValid && errorBuilder.Length > 0)
			{
				logger.LogError($"Line: [{LineNumber}] Tariff: [{TariffCode}] Validation error: {errorBuilder}");
			}

			return isValid;
		}

		static bool Validate(bool expression, StringBuilder errorBuilder, string errorMessage)
		{
			if (!expression)
			{
				errorBuilder.Append(errorMessage);
			}
			return expression;
		}

		public void ProcessUpdates(Dictionary<string, string> countryCodes, ITariffHelper tariffHelper, ILogger logger)
		{
			SetupSchedule();
			SetupTariffCode(tariffHelper);
			SetupImportCountries(countryCodes);

			StatisticalUnitConverted = SARSMappingHelper.GetUnitOfMeasure(StatisticalUnitOriginal);

			Rates.ForEach(r => r.ProcessUpdates(this, countryCodes, logger));

			var duplicateRates = FindDuplicateRates();
			if (duplicateRates.Count > 0)
			{
				RemoveDuplicateRates(duplicateRates);
				ExpireExistingRates(tariffHelper, duplicateRates);
			}

			if (string.IsNullOrEmpty(StatisticalUnitConverted) && Rates.Count == 1 && !string.IsNullOrEmpty(Rates[0].UnitOfMeasureConverted))
			{
				StatisticalUnitConverted = Rates[0].UnitOfMeasureConverted;
			}

			ApplyRules(tariffHelper);
		}

		List<Rate> FindDuplicateRates()
		{
			var standardRateFormula = Rates.FirstOrDefault(x => x.RateType == RateTypes.Standard)?.Formula ?? string.Empty;
			return Rates.Where(x => x.RateType != RateTypes.Standard && (Schedule.ScheduleType != Schedules.S1P1 || x.Formula == standardRateFormula)).ToList();
		}

		void RemoveDuplicateRates(List<Rate> duplicateRates)
		{
			Rates = Rates.Except(duplicateRates).ToList();
		}

		void ExpireExistingRates(ITariffHelper tariffHelper, List<Rate> ratesToExpire)
		{
			var startDate = CalcStartDate().Date;
			var endDate = CalcEndDate().Date;
			var existingRates = tariffHelper.GetRates(TariffCode, startDate, endDate);
			var existingValidRates = existingRates.Where(r => r.StartDate <= startDate && r.EndDate >= endDate && !string.IsNullOrEmpty(r.Key)).ToDictionary(r => r.Key);
			foreach (var rateToExpire in ratesToExpire)
			{
				if (existingValidRates.TryGetValue(rateToExpire.Key, out var matchingRate))
				{
					matchingRate.EndDate = CommonHelper.CalcMaxDate(startDate.AddDays(-1));
					Rates.Add(matchingRate);
				}
			}
		}

		void SetupSchedule()
		{
			Schedule = SARSSchedule.Get(ScheduleTypeCode, ItemNumber);
		}

		void SetupTariffCode(ITariffHelper tariffHelper)
		{
			var relationCode = (string.IsNullOrEmpty(SubHeading) ? Heading ?? string.Empty : SubHeading).Replace(".", "");

			if (string.IsNullOrEmpty(ItemNumber))
			{
				TariffCode = relationCode;
				relationCode = "";
			}
			else
			{
				TariffCode = $"{ItemNumber}{Code ?? string.Empty}".Replace(".", "");
			}

			RelationshipTariffCode = RemoveTrailingZeros(relationCode);

			var result = tariffHelper.GetUniqueId(TariffCode, CheckDigit, RelationshipTariffCode);

			UniqueId = result.UniqueId;
			TariffKeyExists = result.Exists;
		}

		static string RemoveTrailingZeros(string tariffCode)
		{
			var result = tariffCode ?? string.Empty;
			while (result.Length >= 2 && result.Substring(result.Length - 2) == "00")
			{
				result = result.Substring(0, result.Length - 2);
			}
			return result == "0" ? string.Empty : result;
		}

		void SetupImportCountries(Dictionary<string, string> countryCodes)
		{
			var countries = (ImportedFrom ?? string.Empty).ToUpperInvariant().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			ImportCountries = new List<string>();

			foreach (var name in countries)
			{
				if (countryCodes.TryGetValue(name.Trim(), out string countryCode))
				{
					ImportCountries.Add(countryCode);
				}
			}
		}

		protected void ApplyRules(ITariffHelper tariffHelper)
		{
			var rulesToApply = tariffHelper.GetRules().Where(x => x.IsMatch(TariffCode, Schedule.ScheduleType, CheckDigit)).ToList();

			rulesToApply.ForEach(r =>
			{
				ApplyAttributes(r.Attributes);
				ApplyUnitsOfMeasure(r.UnitsOfMeasure);
				ApplyRates(r.Rates);
			});
		}

		void ApplyAttributes(IEnumerable<RuleTariffAttribute> attributes)
		{
			foreach (var attribute in attributes)
			{
				AdditionalAttributes[attribute.AttributeName] = attribute.AttributeValue;
			}
		}

		void ApplyUnitsOfMeasure(IEnumerable<RuleTariffUOM> unitsOfMeasure)
		{
			foreach (var unit in unitsOfMeasure)
			{
				AdditionalUOMs[unit.UOMType] = unit.UOMValue;
			}
		}

		void ApplyRates(IEnumerable<RuleRate> rates)
		{
			var rateToUpdate = Rates.FirstOrDefault(x => x.RateType == RateTypes.Standard);
			bool matchEmptySelector = Schedule.HasNoTradeGroup;

			foreach (var rate in rates)
			{
				var rateType = RuleMapping.GetRateTypeFromSelector(rate.SelectorFormula);

				if (rateToUpdate != null && (rateType == rateToUpdate.RateType || (matchEmptySelector && string.IsNullOrWhiteSpace(rateType))))
				{
					rateToUpdate.Formula = rate.RateFormula;
					rateToUpdate.Description = rate.Description;
				}
				else if (!string.IsNullOrWhiteSpace(rateType))
				{
					var newRate = new Rate
					{
						RateType = rateType,
						Description = rate.Description,
						Formula = rate.RateFormula,
						Preference = SARSMappingHelper.GetPreferenceFromScheduleAndRate(Schedule.ScheduleType, rateType)
					};

					Rates.Add(newRate);
				}
			}
		}
	}
}
