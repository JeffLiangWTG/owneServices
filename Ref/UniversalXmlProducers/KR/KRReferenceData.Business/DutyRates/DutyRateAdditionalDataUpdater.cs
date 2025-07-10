using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NPOI.SS.UserModel;
using static CargoWise.RefDbRepo.KRReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class DutyRateAdditionalDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{

		public DutyRateAdditionalDataUpdater(Dictionary<string, string> tradeGroups)
		{
			TradeGroups = tradeGroups;
		}

		Dictionary<string, string> TradeGroups { get; set; }

		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusTariff.ZZ1_TariffCode))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;
				result = cellValue.Length == Constants.TariffLength;
			}
			return result;
		}

		public void UpdateAdditionally(RefCusTariff tariff, IRow row, EntityConfiguration configuration)
		{
			var rateEntityType = configuration.EntityTypeExcelColumnMapping.EntityTypes.FirstOrDefault(x => x.Name == nameof(RefCusRate));

			var rateDetails = GetRateDetails(rateEntityType, row);
			var preference = rateDetails.Preference;
			var tariffAmountPerUnit = rateDetails.TariffAmountPerUnit;
			var basePrice = rateDetails.BasePrice;
			var adValoremRate = rateDetails.AdValoremRate;
			var tariffCode = tariff.ZZ1_TariffCode;
			var startDateValue = Helper.GetDateFromRow(row, rateEntityType, nameof(RefCusRate.ZZ2_StartDate));
			var endDateValue = Helper.GetDateFromRow(row, rateEntityType, nameof(RefCusRate.ZZ2_EndDate));
			endDateValue = endDateValue.AddHours(23).AddMinutes(59);

			var currentRefCusRate = tariff.RefCusRates.FirstOrDefault(x => x.ZZ2_ZZS_NKPreference == preference && x.ZZ2_StartDate == startDateValue);
			if (currentRefCusRate != null)
			{
				SetRefCusRate();
			}

			void SetRefCusRate()
			{
				if (IsAdValoremRate())
				{
					(string advaloremRateCode, string advaloremRateFormula, string adValoremRateformulaDerivedFrom) = GetAdvaloremRateData();
					currentRefCusRate.ZZ2_ZY1_NKRateCode = advaloremRateCode;
					currentRefCusRate.ZZ2_RateFormula = advaloremRateFormula;
					currentRefCusRate.ZZ2_RateFormulaDerivedFrom = adValoremRateformulaDerivedFrom; 
				}
				else if (IsSpecificRate())
				{
					(string specificRateCode, string specificRateFormula, string specificformulaDerivedFrom) = GetSpecificRateData();
					currentRefCusRate.ZZ2_ZY1_NKRateCode = specificRateCode;
					currentRefCusRate.ZZ2_RateFormula = specificRateFormula;
					currentRefCusRate.ZZ2_RateFormulaDerivedFrom = specificformulaDerivedFrom; 
				}
				else if (HasMaxMinRate())
				{
					(string advaloremRateCode, string advaloremRateFormula, string adValoremRateformulaDerivedFrom) = GetAdvaloremRateData();
					(string specificRateCode, string specificRateFormula, string specificformulaDerivedFrom) = GetSpecificRateData();

					currentRefCusRate.ZZ2_ZY1_NKRateCode = advaloremRateCode;
					currentRefCusRate.ZZ2_RateFormula = advaloremRateFormula;
					currentRefCusRate.ZZ2_RateFormulaDerivedFrom = adValoremRateformulaDerivedFrom;


					var addibleRefCusRate = new RefCusRate
					{
						ZZ2_StartDate = startDateValue,
						ZZ2_EndDate = endDateValue,
						ZZ2_ZZS_NKPreference = preference,
						ZZ2_ZY1_NKRateCode = specificRateCode,
						ZZ2_RateFormula = specificRateFormula,
						ZZ2_RateFormulaDerivedFrom = specificformulaDerivedFrom
					};

					if (IsRuleApplicable(preference, configuration.Rules.FirstOrDefault(x => x.Name == RuleID.PREFERENCES_HAVING_USER_SELECT_RULE)))
					{
						currentRefCusRate.RefCusApplicabilities = new RefCusApplicability[]
						{
							new RefCusApplicability {ZZT_StartDate = startDateValue, ZZT_EndDate = endDateValue, ZZT_AdditionalCode = DutyRateApplicableTypes.Min },
							new RefCusApplicability {ZZT_StartDate = startDateValue, ZZT_EndDate = endDateValue, ZZT_AdditionalCode = DutyRateApplicableTypes.Max }
						};
						addibleRefCusRate.RefCusApplicabilities = new RefCusApplicability[]
						{
							new RefCusApplicability {ZZT_StartDate = startDateValue, ZZT_EndDate = endDateValue, ZZT_AdditionalCode = DutyRateApplicableTypes.Min },
							new RefCusApplicability {ZZT_StartDate = startDateValue, ZZT_EndDate = endDateValue, ZZT_AdditionalCode = DutyRateApplicableTypes.Max }
						};
					}
					else
					{
						currentRefCusRate.RefCusApplicabilities = new RefCusApplicability[]
						{
						new RefCusApplicability {ZZT_StartDate = startDateValue, ZZT_EndDate = endDateValue, ZZT_AdditionalCode = DutyRateApplicableTypes.Max }
						};
						addibleRefCusRate.RefCusApplicabilities = new RefCusApplicability[]
						{
						new RefCusApplicability {ZZT_StartDate = startDateValue, ZZT_EndDate = endDateValue, ZZT_AdditionalCode = DutyRateApplicableTypes.Max }
						};
					}

					tariff.RefCusRates = tariff.RefCusRates.Append(addibleRefCusRate).ToArray();
					SetTradeGroup(addibleRefCusRate);
				}
				else if (HasBasePrice())
				{
					(string rateCode, string rateFormula, string rateformulaDerivedFrom) = GetBasePriceData();
					currentRefCusRate.ZZ2_RateFormula = rateFormula;
					currentRefCusRate.ZZ2_ZY1_NKRateCode = rateCode;
					currentRefCusRate.ZZ2_RateFormulaDerivedFrom = rateformulaDerivedFrom;
				}
				SetTradeGroup(currentRefCusRate);

				(string, string, string) GetAdvaloremRateData() => (Constants.RateCodes.AdValoremRate, adValoremRate == 0m ? "0" : $"VFD * {adValoremRate / 100m}", adValoremRate.ToString("0.##", CultureInfo.InvariantCulture));
				(string, string, string) GetSpecificRateData()
				{
					var uq = string.Empty;
					foreach (var rule in configuration.Rules)
					{
						uq = GetUQ(tariffCode, rule, startDateValue, endDateValue);
					}
					if (string.IsNullOrEmpty(uq))
					{
						uq = "KG";
					}
					return (Constants.RateCodes.SpecificRate, $"[{uq}] * {tariffAmountPerUnit}", tariffAmountPerUnit.ToString("0.##", CultureInfo.InvariantCulture));
				}
				(string, string, string) GetBasePriceData()
				{
					var rateCode = string.Empty;
					var normalDutyFormula = string.Empty;
					var formulaDerivedFrom = string.Empty;
					if (adValoremRate > 0)
					{
						(string advaloremRateCode, string advaloremRateFormula, string adValoremRateformulaDerivedFrom) = GetAdvaloremRateData();
						rateCode = advaloremRateCode;
						normalDutyFormula = advaloremRateFormula;
						formulaDerivedFrom = adValoremRateformulaDerivedFrom;
					}
					else
					{
						(string specificRateCode, string specificRateFormula, string specificformulaDerivedFrom) = GetSpecificRateData();
						rateCode = specificRateCode;
						normalDutyFormula = specificRateFormula;
						formulaDerivedFrom = specificformulaDerivedFrom;
					}

					var formulaF1 = string.Format(CultureInfo.CurrentCulture, "[KG] * {0} * 0.52 - VFD * 0.9", basePrice);
					var formulaF2 = string.Format(CultureInfo.CurrentCulture, "[KG] * {0} * 0.47 - VFD * 0.7", basePrice);
					var formulaF3 = string.Format(CultureInfo.CurrentCulture, "[KG] * {0} * 0.39 - VFD * 0.5", basePrice);
					var formulaF4 = string.Format(CultureInfo.CurrentCulture, "[KG] * {0} * 0.27 - VFD * 0.3", basePrice);
					var formulaBARR = string.Format(CultureInfo.CurrentCulture, "([KG] * {0} - VFD)/([KG] * {0}) * 100", basePrice);
					const string basePriceFormula = "{0} + IF({1} > 75, {2}, IF({1} > 60, {3}, IF({1} > 40, {4}, IF({1} > 10, {5}, 0))))";
					var rateFormula = string.Format(CultureInfo.CurrentCulture, basePriceFormula, normalDutyFormula, formulaBARR, formulaF1, formulaF2, formulaF3, formulaF4);
					return (rateCode, rateFormula, formulaDerivedFrom);
				}
				bool IsAdValoremRate() => tariffAmountPerUnit == 0m && basePrice == 0m;
				bool IsSpecificRate() => tariffAmountPerUnit > 0m && adValoremRate == 0m && basePrice == 0m;
				bool HasMaxMinRate() => tariffAmountPerUnit > 0m && adValoremRate > 0m && basePrice == 0m;
				bool HasBasePrice() => basePrice > 0m;
				void SetTradeGroup(RefCusRate refCusRate)
				{
					var refCusApplicabilities = refCusRate.RefCusApplicabilities ?? (refCusRate.RefCusApplicabilities = Array.Empty<RefCusApplicability>());
					if (refCusApplicabilities.Length == 0)
					{
						refCusRate.RefCusApplicabilities = refCusApplicabilities.Append(new RefCusApplicability() { ZZT_StartDate = startDateValue, ZZT_EndDate = endDateValue, ZZT_AdditionalCode = "" }).ToArray();
					}
					foreach (var item in refCusRate.RefCusApplicabilities)
					{
						item.ZZT_ZZA_NKTradeGroup = TradeGroups[preference];
					}
				}
			}
		}

		static RateDetails GetRateDetails(MappingEntityType rateEntityType, IRow row)
		{
			const int tariffRateDecimalPlace = 2;
			var preference = string.Empty;
			var tariffAmountPerUnit = 0m;
			var basePrice = 0m;
			var adValoremRate = 0m;

			foreach (var property in rateEntityType.Properties)
			{
				var cell = row.GetCell(property.ExcelColumn);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;

				if (property.Name == nameof(RefCusRate.ZZ2_ZZS_NKPreference))
				{
					preference = cellValue;
				}
				if (property.IsNonPersistent)
				{
					switch (property.Name)
					{
						case NonPersistentNames.TariffRate:
							var tariffRate = cellValue;
							_ = decimal.TryParse(tariffRate, out adValoremRate);
							adValoremRate = decimal.Round(adValoremRate, tariffRateDecimalPlace);
							break;
						case NonPersistentNames.TariffAmountPerUnit:
							tariffAmountPerUnit = decimal.TryParse(cellValue, out tariffAmountPerUnit) ? tariffAmountPerUnit : 0;
							break;
						case NonPersistentNames.BasePrice:
							basePrice = decimal.TryParse(cellValue, out basePrice) ? basePrice : 0;
							break;
					}
				}
			}
			return new RateDetails() { Preference = preference, AdValoremRate = adValoremRate, BasePrice = basePrice, TariffAmountPerUnit = tariffAmountPerUnit };
		}

		struct RateDetails
		{
			public string Preference;
			public decimal AdValoremRate;
			public decimal BasePrice;
			public decimal TariffAmountPerUnit;
		}

		public string GetUQ(string hsCode, Rule rule, DateTime rateStartDate, DateTime rateEndDate)
		{
			switch (rule.Name)
			{
				case RuleID.HS_UOM_CU1:
					return GetHSCode_UOM(hsCode, rule, rateStartDate, rateEndDate, Constants.UOMTypes.CU1);
				case RuleID.HS_UOM_CU3:
					return GetHSCode_UOM(hsCode, rule, rateStartDate, rateEndDate, Constants.UOMTypes.CU3);
			}
			return null;
		}

		string GetHSCode_UOM(string hsCode, Rule rule, DateTime rateStartDate, DateTime rateEndDate, string uomType)
		{
			if (rule.Relationship == Relationship.OR.ToString())
			{
				if (rule.RuleValues.Any(x => hsCode.StartsWith(x.Value, StringComparison.Ordinal)))
				{
					return GetUOM(hsCode, uomType, rateStartDate, rateEndDate);
				}
			}
			return null;
		}

		string GetUOM(string hsCode, string uomType, DateTime rateStartDate, DateTime rateEndDate)
		{
			var result = string.Empty;
			var tariffData = RefDataEntityLoader.GetTariffData(Constants.CountryCodes.KoreaSouth, hsCode, rateStartDate).FirstOrDefault();
			if (tariffData != null && tariffData.ZZ1_EndDate >= rateEndDate)
			{
				result = tariffData.RefCusTariffUOMs.Where(x => x.ZZ8_Type == uomType).FirstOrDefault()?.ZZ8_UOM;
			}
			return result;
		}

		static bool IsRuleApplicable(string compareValue, Rule rule)
		{
			if (rule != null)
			{
				foreach (var ruleValue in rule.RuleValues)
				{
					if (compareValue == ruleValue.Value)
					{
						return true;
					}
				}
			}
			return false;
		}

		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff dataEntity, IRow row, EntityConfiguration configuration) => UpdateAdditionally(dataEntity, row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff dataEntity, Rule rule) { }
		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
		protected virtual ISafeRepository SafeRepository => new SafeRepository(new Uri(ApplicationConfig.SafeDataUpdateUri));
		protected virtual RefDataEntityLoader RefDataEntityLoader => new RefDataEntityLoader(SafeRepository);
	}

	static class NonPersistentNames
	{
		public const string TariffRate = "TariffRate";
		public const string TariffAmountPerUnit = "TariffAmountPerUnit";
		public const string BasePrice = "BasePrice";
	}

	class RuleID
	{
		public const string HS_UOM_CU3 = "HS UOM CU3";
		public const string HS_UOM_CU1 = "HS UOM CU1";
		public const string HSCODE_HAVING_MIN_RULE = "HS Codes having MIN of two rates";
		public const string PREFERENCES_HAVING_MIN_RULE = "Preferences having MIN of two rates";
		public const string PREFERENCES_HAVING_USER_SELECT_RULE = "Preferences having users select MIN or MAX";
	}
}
