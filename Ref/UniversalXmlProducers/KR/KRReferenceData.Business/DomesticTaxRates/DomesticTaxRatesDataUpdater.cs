using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;
using static CargoWise.RefDbRepo.KRReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class DomesticTaxRatesDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{
		public DomesticTaxRatesDataUpdater(DateTime publicationDate)
		{
			PublicationDate = publicationDate;
		}

		public static void UpdateAdditionally(RefCusTariff dataEntity, IRow row, EntityConfiguration configuration)
		{
			var rateDetails = GetDomesticTaxDetails(configuration, row);

			dataEntity.ZZ1_TariffCode = string.IsNullOrEmpty(rateDetails.AdditionalCode) ? rateDetails.TariffCode : $"{rateDetails.TariffCode}-{rateDetails.AdditionalCode}";
			HandleRefCusTariffAttribute(dataEntity, rateDetails);
			HandleRefCusRateDetails(dataEntity, rateDetails, configuration.Rules);
		}

		static DomesticTaxDetails GetDomesticTaxDetails(EntityConfiguration configuration, IRow row)
		{
			var result = new DomesticTaxDetails();
			foreach (var property in configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties).Where(x => x.IsNonPersistent))
			{
				var cell = row.GetCell(property.ExcelColumn);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;

				switch (property.Name)
				{
					case NonPersistentNames.UnitQuantity:
						result.UnitQuantity = cellValue;
						break;
					case NonPersistentNames.TaxRate:
						if (decimal.TryParse(cellValue, out var taxRate))
						{
							result.TaxRate = decimal.Parse(taxRate.ToString("0.##", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
						}
						break;
					case NonPersistentNames.BasePrice:
						if (decimal.TryParse(cellValue, out var basePrice))
						{
							result.BasePrice = basePrice;
						}
						break;
					case NonPersistentNames.TariffCode:
						result.TariffCode = cellValue;
						break;
					case NonPersistentNames.AdditionalCode:
						result.AdditionalCode = cellValue;
						result.TaxClassification = GetTaxClassification(cellValue);
						break;
					case NonPersistentNames.AgricultureTaxBApplies:
						result.AgricultureTaxBApplies = cellValue;
						break;
				}
			}
			return result;
		}
		static string GetTaxClassification(string additionalCode)
		{
			var taxClassification = string.Empty;
			switch (additionalCode)
			{
				case DomesticTaxClassificationCode.A:
				case DomesticTaxClassificationCode.B:
					taxClassification = CodeListAttributeValues.SpecialConsumptionTax;
					break;
				case DomesticTaxClassificationCode.D:
					taxClassification = CodeListAttributeValues.LiquorTax;
					break;
				case DomesticTaxClassificationCode.E:
				case DomesticTaxClassificationCode.F:
					taxClassification = CodeListAttributeValues.TransportationTax;
					break;
			}
			return taxClassification;
		}

		static void HandleRefCusTariffAttribute(RefCusTariff dataEntity, DomesticTaxDetails domesticTaxDetails)
		{
			var tariffAttributes = new List<RefCusTariffAttribute>
			{
				new RefCusTariffAttribute
				{
					ZZ3_Name = NonPersistentNames.TaxClassification,
					ZZ3_Value = domesticTaxDetails.TaxClassification
				}
			};
			if (domesticTaxDetails.AgricultureTaxBApplies == YesNo.Yes)
			{
				tariffAttributes.Add(new RefCusTariffAttribute
				{
					ZZ3_Name = NonPersistentNames.AgricultureTaxBApplies,
					ZZ3_Value = domesticTaxDetails.AgricultureTaxBApplies
				});
			}

			dataEntity.RefCusTariffAttributes = tariffAttributes.ToArray();
		}

		static void HandleRefCusRateDetails(RefCusTariff tariff, DomesticTaxDetails domesticTaxDetails, Rule[] rules)
		{
			var taxRate = domesticTaxDetails.TaxRate;
			var unitQuantity = domesticTaxDetails.UnitQuantity;
			var basePrice = domesticTaxDetails.BasePrice;
			var taxRateNumeric = string.IsNullOrEmpty(unitQuantity) ? taxRate / 100 : taxRate;

			var refCusRate = tariff.RefCusRates.Last();
			refCusRate.ZZ2_ZY1_NKRateCode = domesticTaxDetails.TaxClassification;
			refCusRate.ZZ2_RateFormulaDerivedFrom = $"{taxRate}";

			var rule = rules.FirstOrDefault(x => x.Name == RuleID.QuantityUnitCodes);
			var ruleValue = rule.RuleValues.FirstOrDefault(x => x.Value == domesticTaxDetails.TariffCode);
			if (ruleValue != null)
			{
				refCusRate.ZZ2_RateFormula = string.Format(CultureInfo.InvariantCulture, ruleValue.Formula, basePrice, taxRateNumeric);

				var refCusTariffUOMs = ruleValue.QuantityUnit.Split(',');
				if (refCusTariffUOMs.Any())
				{
					AddRefCusTariffUOMs(refCusTariffUOMs);
				}
			}
			else if (!string.IsNullOrEmpty(unitQuantity) && basePrice == 0 && taxRate > 0)
			{
				refCusRate.ZZ2_RateFormula = $"[{unitQuantity}]*{taxRate}";
				AddRefCusTariffUOMs(new string[] { unitQuantity });
			}
			else if (string.IsNullOrEmpty(unitQuantity) && taxRate > 0)
			{
				if (basePrice > 0)
				{
					refCusRate.ZZ2_RateFormula = $"(VFD - {basePrice}*[{Unit}])*{taxRateNumeric}";
					AddRefCusTariffUOMs(new string[] { Unit });
				}
				else
				{
					refCusRate.ZZ2_RateFormula = $"VFD*{taxRateNumeric}";
				}
			}

			AddEducationTax(domesticTaxDetails.TaxClassification);

			void AddEducationTax(string taxClassification)
			{
				switch (taxClassification)
				{
					case CodeListAttributeValues.SpecialConsumptionTax:
						var exemptionRule_SCT = rules.FirstOrDefault(x => x.Name == RuleID.EducationTaxExemptionOfSCTTariffs);
						if (exemptionRule_SCT.RuleValues.Any(x => x.Value == domesticTaxDetails.TariffCode))
						{
							break;
						}
						else
						{
							var reductionRule_SCT = rules.FirstOrDefault(x => x.Name == RuleID.EducationTaxReductionOfSCTTariffs);
							var educationRate_SCT = CreateEducationRate();
							if (reductionRule_SCT.RuleValues.Any(x => x.Value == domesticTaxDetails.TariffCode))
							{
								SetEducationTaxRateFormula(educationRate_SCT, EducationTaxFifteen);
							}
							else
							{
								SetEducationTaxRateFormula(educationRate_SCT, EducationTaxThirty);
							}
							tariff.RefCusRates = tariff.RefCusRates.Append(educationRate_SCT).ToArray();
						}
						break;
					case CodeListAttributeValues.LiquorTax:
						var exemptionRule_LQT = rules.FirstOrDefault(x => x.Name == RuleID.EducationTaxExemptionOfLQTTariffs);
						if (exemptionRule_LQT.RuleValues.Any(x => x.Value == domesticTaxDetails.TariffCode))
						{
							break;
						}
						else
						{
							var AdditionalRule_LQT = rules.FirstOrDefault(x => x.Name == RuleID.AdditionalEducationTaxOfLQTTariffs);
							var educationRate_LQT = CreateEducationRate();
							if (AdditionalRule_LQT.RuleValues.Any(x => x.Value == domesticTaxDetails.TariffCode) || domesticTaxDetails.TaxRate > AdditionalTaxStandard)
							{
								SetEducationTaxRateFormula(educationRate_LQT, EducationTaxThirty);
							}
							else
							{
								SetEducationTaxRateFormula(educationRate_LQT, EducationTaxTen);
							}
							tariff.RefCusRates = tariff.RefCusRates.Append(educationRate_LQT).ToArray();
						}
						break;
					case CodeListAttributeValues.TransportationTax:
						var educationRate_TRT = CreateEducationRate();
						SetEducationTaxRateFormula(educationRate_TRT, EducationTaxFifteen);
						tariff.RefCusRates = tariff.RefCusRates.Append(educationRate_TRT).ToArray();
						break;
				}
			}

			RefCusRate CreateEducationRate()
			{
				return new RefCusRate
				{
					ZZ2_ZY1_NKRateCode = CodeListAttributeValues.EducationTax,
					ZZ2_ZY1_ZZR_NKRateType = CodeListAttributeValues.EducationTax,
					ZZ2_StartDate = refCusRate.ZZ2_StartDate,
					ZZ2_EndDate = refCusRate.ZZ2_EndDate
				};
			}

			void AddRefCusTariffUOMs(string[] newUOMs)
			{
				var list = new List<RefCusTariffUOM>(tariff.RefCusTariffUOMs ?? new RefCusTariffUOM[0]);
				var cuLength = list.Count;
				var sorted = newUOMs.Except(list.Select(x => x.ZZ8_UOM)).Order();
				foreach (var uom in sorted)
				{
					list.Add(new RefCusTariffUOM() { ZZ8_Type = "CU" + (++cuLength), ZZ8_UOM = uom });
				}
				tariff.RefCusTariffUOMs = list.ToArray();
			}

			void SetEducationTaxRateFormula(RefCusRate rate, decimal derivedFromValue)
			{
				rate.ZZ2_RateFormula = $"VFD*{derivedFromValue / 100}";
				rate.ZZ2_RateFormulaDerivedFrom = derivedFromValue.ToString("0", CultureInfo.InvariantCulture);
			}
		}

		const string Unit = "U";
		const decimal AdditionalTaxStandard = 70m;
		const decimal EducationTaxTen = 10m;
		const decimal EducationTaxFifteen = 15m;
		const decimal EducationTaxThirty = 30m;

		struct DomesticTaxDetails
		{
			public decimal TaxRate;
			public string UnitQuantity;
			public decimal BasePrice;
			public string TariffCode;
			public string AdditionalCode;
			public string TaxClassification;
			public string AgricultureTaxBApplies;
		}

		DateTime PublicationDate { get; }
		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff tariff, IRow row, EntityConfiguration configuration) => UpdateAdditionally(tariff, row, configuration);

		public static bool IsDataRowValid() => true;
		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid();
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }

		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff dataEntity, Rule rule) { }

		class RuleID
		{
			public const string AdditionalEducationTaxOfLQTTariffs = "Additional Education Tax Of Liquor Tax Tariffs";
			public const string EducationTaxExemptionOfLQTTariffs = "Education Tax Exemption Of Liquor Tax Tariffs";
			public const string EducationTaxExemptionOfSCTTariffs = "Education Tax Exemption Of Special Consumption Tax Tariffs";
			public const string EducationTaxReductionOfSCTTariffs = "Education Tax Reduction Of Special Consumption Tax Tariffs";
			public const string QuantityUnitCodes = "Quantity Unit Codes";
		}
		static class NonPersistentNames
		{
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
			public const string AgricultureTaxBApplies = "AgricultureTaxBApplies";
			public const string AdditionalCode = "AdditionalCode";
			public const string Description = "Description";
			public const string TariffCode = "TariffCode";
			public const string TaxClassification = "TaxClassification1";
			public const string TaxRate = "TaxRate";
			public const string UnitQuantity = "UnitQuantity";
			public const string BasePrice = "BasePrice";
		}
	}
}
