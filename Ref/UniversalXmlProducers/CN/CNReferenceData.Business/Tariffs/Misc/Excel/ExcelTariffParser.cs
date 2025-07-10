using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Columns = CargoWise.RefDbRepo.CNReferenceData.Business.ExcelReadConfiguration.ColumnConstants;
using RefCusCondition = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusCondition;
using RefCusConditionValue = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusConditionValue;
using RefCusExcludedTradeGroup = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusExcludedTradeGroup;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class ExcelTariffParser
	{
		DateTime EffectiveDate { get; }
		DateTime EndDate { get; }
		AdditionalElementHelper AdditionalElementHelper { get; }

		public ExcelTariffParser(DateTime? effectiveDate, DateTime endDate, AdditionalElementHelper additionalElementHelper)
		{
			EffectiveDate = effectiveDate ?? GlobalOption.Instance.FirstDayOfThisMonth;
			EndDate = endDate;
			AdditionalElementHelper = additionalElementHelper;
		}

		public IEnumerable<RefCusTariff> CreateTariffs(
			RowData rawTariff,
			IEnumerable<RowData> rawCiqs,
			RowData rawRate,
			RowData rawExcise,
			RowData rawExpDuty,
			IEnumerable<RowData> rawUsaDuties
		)
		{
			var hsnTariff = CreateHsnTariff(rawTariff);

			AddRates(hsnTariff, rawTariff, rawRate, rawExcise, rawExpDuty, rawUsaDuties);
			AddConditions(hsnTariff, rawTariff);

			var attributes = GetRequirements(rawTariff)
			.Union(GetTariffAttributeRepository(hsnTariff))
			.Union(GetAdditionalInfos(hsnTariff, rawTariff));
			hsnTariff.RefCusTariffAttributes = attributes.ToArray();

			hsnTariff.RefCusTariffUOMs = CreateTariffUOM(rawTariff).ToArray();

			yield return hsnTariff;

			if (rawCiqs.Any())
			{
				foreach (var ciqData in rawCiqs)
				{
					yield return CreateCIQTariff(rawTariff, ciqData);
				}
			}
			else
			{
				var ciqData = new RowData();
				ciqData.Properties.Add(Columns.CIQ.TariffCode.Name, rawTariff.Properties[Columns.Tariff.TariffCode.Name]);
				ciqData.Properties.Add(Columns.CIQ.CIQCode.Name, "999");
				ciqData.Properties.Add(Columns.CIQ.Description.Name, rawTariff.Properties[Columns.Tariff.Description.Name]);
				ciqData.Properties.Add(Columns.CIQ.EnglishDescription.Name, rawTariff.Properties[Columns.Tariff.EnglishDescription.Name]);

				yield return CreateCIQTariff(rawTariff, ciqData);
			}
		}

		RefCusTariff CreateHsnTariff(RowData rawTariff)
		{
			return CreateTariff(rawTariff, Constants.TariffTypes.HSN);
		}

		RefCusTariff CreateCIQTariff(RowData rawTariff, RowData rawCIQTariff)
		{
			var hsnTariffCode = rawTariff.Properties[Columns.Tariff.TariffCode.Name];
			var ciqTariffCode = hsnTariffCode + rawCIQTariff.Properties[Columns.CIQ.CIQCode.Name];
			rawCIQTariff.Properties[Columns.CIQ.TariffCode.Name] = ciqTariffCode;

			var refCusCIQTariff = CreateTariff(rawCIQTariff, Constants.TariffTypes.CIQ);
			refCusCIQTariff.RefCusTariffRelationships = new[] { new RefCusTariffRelationship { ZZH_TariffCode = hsnTariffCode } };
			return refCusCIQTariff;
		}

		RefCusTariff CreateTariff(RowData rawTariff, string tariffType = Constants.TariffTypes.HSN)
		{
			rawTariff.Exported = true;

			var isHsn = tariffType == Constants.TariffTypes.HSN;

			RefCusTariffLanguage[] refCusTariffLanguages = null;
			var englishDescription = rawTariff.Properties[isHsn ? Columns.Tariff.EnglishDescription.Name : Columns.CIQ.EnglishDescription.Name];
			if (!string.IsNullOrEmpty(englishDescription))
			{
				refCusTariffLanguages = new[]
				{
					new RefCusTariffLanguage
					{
						ZX7_ZX6_NKLanguage = "EN",
						ZX7_Description = englishDescription
					}
				};
			}

			return new RefCusTariff
			{
				ZZ1_TariffCode = rawTariff.Properties[isHsn ? Columns.Tariff.TariffCode.Name : Columns.CIQ.TariffCode.Name],
				ZZ1_Description = rawTariff.Properties[isHsn ? Columns.Tariff.Description.Name : Columns.CIQ.Description.Name],
				ZZ1_StartDate = EffectiveDate,
				ZZ1_EndDate = EndDate,
				ZZ1_ZZF_NKTaxOrFeeCode = isHsn && int.TryParse(rawTariff.Properties[Columns.Tariff.VATRate.Name], out var rate) ? TaxOrFeeHelper.Instance.GetCode(rate) : string.Empty,
				ZZ1_ZZI_NKTariffType = tariffType,
				RefCusTariffLanguages = refCusTariffLanguages
			};
		}

		#region Rates & RefCusApplicability

		void AddRates(RefCusTariff refCusTariff, RowData tariffData, RowData rateData, RowData exciseData, RowData expDutyData, IEnumerable<RowData> usaDutyDataList)
		{
			var rates = new List<RefCusRate>();

			var mfnRate = tariffData.Properties[Columns.Tariff.ProvRate.Name];
			if (string.IsNullOrEmpty(mfnRate))
			{
				mfnRate = tariffData.Properties[Columns.Tariff.MFNRate.Name];
			}

			rates.Add(CreateRate(refCusTariff, "MFN", "MFN", mfnRate));
			rates.Add(CreateRate(refCusTariff, "NORMAL", "STANDARD", tariffData.Properties[Columns.Tariff.NormalRate.Name]));

			if (rateData != null)
			{
				rateData.Exported = true;

				var ldcRates = new List<string>();
				foreach (KeyValuePair<string, string> pair in rateData.Properties)
				{
					if (pair.Key.StartsWith("LDC", StringComparison.Ordinal) && !string.IsNullOrEmpty(pair.Value))
					{
						ldcRates.Add(pair.Key);
					}
				}

				foreach (KeyValuePair<string, string> pair in rateData.Properties.Where(x => x.Key != "Tariff"))
				{
					var rate = pair.Value;
					if (!string.IsNullOrEmpty(rate))
					{
						rates.Add(CreateRate(refCusTariff, pair.Key.StartsWith("LDC", StringComparison.Ordinal) ? "LDC" : "FTA", pair.Key, rate, ldcRates));
					}
				}
			}

			if (exciseData != null)
			{
				exciseData.Exported = true;

				var exciseRate = exciseData.Properties[Columns.ExciseRate.Rate.Name].TrimEnd('%');
				if (string.IsNullOrEmpty(exciseRate))
				{
					exciseRate = exciseData.Properties[Columns.ExciseRate.SpecialRate.Name];
				}
				rates.Add(CreateRate(refCusTariff, "EXC", "STANDARD", exciseRate));
			}

			if (expDutyData != null)
			{
				expDutyData.Exported = true;

				var expDutyRate = expDutyData.Properties[Columns.ExciseRate.SpecialRate.Name];
				if (string.IsNullOrEmpty(expDutyRate))
				{
					expDutyRate = expDutyData.Properties[Columns.ExciseRate.Rate.Name];
				}
				rates.Add(CreateRate(refCusTariff, "EXP", "STANDARD", expDutyRate));
			}

			if (usaDutyDataList.Count() > 1)
			{
				GlobalOption.Instance.Log.Error($"Multi USA Additional Duty Rate for {tariffData.Key}");
			}

			foreach (var usaDutyData in usaDutyDataList)
			{
				usaDutyData.Exported = true;

				DateTime? startDate = ParseDate(usaDutyData.Properties["EffectiveDate"])?.Date;
				DateTime? endDate = ParseDate(usaDutyData.Properties["ExpiredDate"])?.Date.AddDays(1).AddMinutes(-1);

				if (usaDutyDataList.Count() > 1)
				{
					GlobalOption.Instance.Log.Info($"\tEffective From {startDate} to {endDate}");
				}
				rates.Add(CreateRate(refCusTariff, "ADL", "US", usaDutyData.Properties[Columns.UsaAddRate.Rate.Name], startDate: startDate, endDate: endDate));
			}

			refCusTariff.RefCusRates = rates.ToArray();
		}

		static DateTime? ParseDate(string dateAsString)
		{
			DateTime? date = null;
			if (!string.IsNullOrEmpty(dateAsString))
			{
				if (double.TryParse(dateAsString, out double dateAsDouble))
				{
					date = DateTime.FromOADate(dateAsDouble);
				}
				else if (DateTime.TryParse(dateAsString, out DateTime dateAsDate))
				{
					date = dateAsDate;
				}
				else
				{
					throw new FormatException($"Parse DateTime Error: {dateAsString}");
				}
			}
			return date;
		}

		RefCusRate CreateRate(RefCusTariff refCusTariff, string preference, string tradeGroupCode, string rate, List<string> ldcRates = null, DateTime? startDate = null, DateTime? endDate = null)
		{
			if (startDate == null)
			{
				startDate = EffectiveDate;
			}
			if (endDate == null)
			{
				endDate = EndDate;
			}

			var preferenceValue = IsDuty(preference) ? (preference == "ADL" ? "" : preference) : "";
			rate = RateFormulaParser.RemoveNewLines(rate);
			var refCusRate = new RefCusRate
			{
				ZZ2_RateFormula = RateFormulaParser.GetRateFormula(refCusTariff.ZZ1_TariffCode, preference, rate, GlobalOption.Instance.Log),
				ZZ2_RateFormulaDerivedFrom = rate,
				ZZ2_StartDate = startDate.Value,
				ZZ2_EndDate = endDate.Value,

				ZZ2_ZY1_NKRateCode = IsDuty(preference) ? (preference == "ADL" ? "ADL" : "DTY") : preference,
				ZZ2_ZY1_ZZR_NKRateType = IsDuty(preference) ? "DTY" : preference,
				ZZ2_ZZS_NKPreference = preferenceValue,
				ZZ2_ZZS_ZZZ_NKDataGrouping = string.IsNullOrEmpty(preferenceValue) ? "" : "CN",
				RefCusApplicabilities = new[] { CreateApplicability(tradeGroupCode, startDate.Value, endDate.Value, ldcRates) }
			};
			return refCusRate;
		}

		static RefCusApplicability CreateApplicability(string tradeGroupCode, DateTime startDate, DateTime endDate, List<string> ldcRates = null)
		{
			var applicability = new RefCusApplicability
			{
				ZZT_AdditionalCode = TradeGroupHelper.Instance.GetAdditionalCode(tradeGroupCode),
				ZZT_StartDate = startDate,
				ZZT_EndDate = endDate,
				ZZT_ZZA_NKTradeGroup = tradeGroupCode
			};

			var excludedTradeGroups = new List<RefCusExcludedTradeGroup>();
			if (ldcRates != null && TradeGroupHelper.Instance.GetExcludeTradeGroups(tradeGroupCode) is string[] excludeTradeGroup)
			{
				foreach (var excludedTradeGroup in excludeTradeGroup)
				{
					if (ldcRates.Contains(excludedTradeGroup))
					{
						excludedTradeGroups.Add(new RefCusExcludedTradeGroup { ZZC_ZZA_NKTradeGroup = excludedTradeGroup });
					}
				}
			}
			applicability.RefCusExcludedTradeGroups = excludedTradeGroups.ToArray();

			return applicability;
		}

		#endregion

		#region RefCusCondition & RefCusConditionValue

		void AddConditions(RefCusTariff refCusTariff, RowData rawTariff)
		{
			var refConsitions = new List<RefCusCondition>();

			var rawConditions = CustomsCondition.Extract(rawTariff.Properties[Columns.Tariff.CUSRequirements.Name]).Where(x => !x.IsIgnorable).ToList();

			foreach (var condition in rawConditions.Where(c => !c.IsLicense))
			{
				refConsitions.Add(CreateNoneLicenceCondition(condition));
			}
			var importLicenses = rawConditions.Where(x => x.IsLicense && x.IsImport);
			if (importLicenses.Any())
			{
				refConsitions.Add(CreateImportCondition(importLicenses));
			}

			var exportLicenses = rawConditions.Where(x => x.IsLicense && x.IsExport);
			if (exportLicenses.Any())
			{
				refConsitions.Add(CreateExportCondition(exportLicenses));
			}

			refCusTariff.RefCusConditions = refConsitions.ToArray();
		}

		RefCusCondition CreateNoneLicenceCondition(CustomsCondition condition)
		{
			var result = new RefCusCondition()
			{
				ZX1_Comment = $"{condition.Code.Last()}.{condition.Description}",
				ZX1_ConditionValueTrueMeansStop = condition.IsProhibit,
				ZX1_IsExport = condition.IsExport,
				ZX1_IsImport = condition.IsImport,
				ZX1_StartDate = EffectiveDate,
				ZX1_EndDate = EndDate,
				ZX1_ZX2_NKConditionType = condition.IsProhibit ? condition.IsImport ? "IMPPH" : "EXPPH" : "CNDOC",
			};
			if (!condition.IsProhibit)
			{
				result.RefCusConditionValues = new[] { CreateConditionValue(condition) };
			}

			return result;
		}

		RefCusCondition CreateImportCondition(IEnumerable<CustomsCondition> conditions)
		{
			var result = new RefCusCondition
			{
				ZX1_Comment = "相关进口许可证",
				ZX1_ConditionValueTrueMeansStop = false,
				ZX1_IsExport = false,
				ZX1_IsImport = true,
				ZX1_StartDate = EffectiveDate,
				ZX1_ZX2_NKConditionType = "CNDOC",
				RefCusConditionValues = conditions.Select(c => CreateConditionValue(c)).ToArray()
			};
			return result;
		}

		RefCusCondition CreateExportCondition(IEnumerable<CustomsCondition> conditions)
		{
			var result = new RefCusCondition
			{
				ZX1_Comment = "相关出口许可证",
				ZX1_ConditionValueTrueMeansStop = false,
				ZX1_IsExport = true,
				ZX1_IsImport = false,
				ZX1_StartDate = EffectiveDate,
				ZX1_ZX2_NKConditionType = "CNDOC",
				RefCusConditionValues = conditions.Select(c => CreateConditionValue(c)).ToArray()
			};
			return result;
		}

		static RefCusConditionValue CreateConditionValue(CustomsCondition condition)
		{
			return new RefCusConditionValue
			{
				ZX3_Value = condition.Code,
				ZX3_ZX4_NKValueType = "DOC"
			};
		}

		#endregion

		#region RefCusTariffAttribute

		static IEnumerable<RefCusTariffAttribute> GetRequirements(RowData rawTariff)
		{
			var cusReqs = CustomsCondition.Extract(rawTariff.Properties[Columns.Tariff.CUSRequirements.Name]).ToArray();
			foreach (var reqs in cusReqs.GroupBy(x => x.RequirementName).Where(x => !string.IsNullOrEmpty(x.Key)))
			{
				yield return new RefCusTariffAttribute { ZZ3_Name = reqs.Key, ZZ3_Value = new string(reqs.Select(x => x.CustomsCode).ToArray()) };
			}

			var ciqReqs = CIQRequirement.Extract(rawTariff.Properties[Columns.Tariff.CIQRequirements.Name]).ToArray();
			foreach (var reqs in ciqReqs.GroupBy(x => x.RequirementName).Where(x => !string.IsNullOrEmpty(x.Key)))
			{
				yield return new RefCusTariffAttribute { ZZ3_Name = reqs.Key, ZZ3_Value = new string(reqs.Select(x => x.Code).ToArray()) };
			}
		}

		static IEnumerable<RefCusTariffAttribute> GetTariffAttributeRepository(RefCusTariff refCusTariff)
		{
			var attributes = TariffAttributeRepository.Instance.GetByTariffCode(refCusTariff.ZZ1_TariffCode);
			foreach (var attr in attributes)
			{
				yield return new RefCusTariffAttribute { ZZ3_Name = attr.AttributeName, ZZ3_Value = attr.AttributeValue };
			}
		}

		IEnumerable<RefCusTariffAttribute> GetAdditionalInfos(RefCusTariff refCusTariff, RowData rawTariff)
		{
			yield return GetAdditionalInfo(1, "品名");

			foreach (var (index, description) in AdditionalElementHelper.Parse(refCusTariff.ZZ1_TariffCode, rawTariff.Properties[Columns.Tariff.AdditionalInfo.Name], GlobalOption.Instance.Log))
			{
				yield return GetAdditionalInfo(index, description);
			}
		}

		RefCusTariffAttribute GetAdditionalInfo(int index, string description)
		{
			return new RefCusTariffAttribute
			{
				ZZ3_Name = "AdditionalInfo" + index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
				ZZ3_Value = AdditionalElementHelper.GetAdditionElementCode(description),
			};
		}

		#endregion

		static IEnumerable<RefCusTariffUOM> CreateTariffUOM(RowData rawTariff)
		{
			var unit1 = rawTariff.Properties[Columns.Tariff.Unit1.Name];
			if (!string.IsNullOrWhiteSpace(unit1))
			{
				yield return new RefCusTariffUOM
				{
					ZZ8_Type = "CU1",
					ZZ8_UOM = UnitOfMeasurementHelper.Instance.GetValue(unit1)
				};
			}
			var unit2 = rawTariff.Properties[Columns.Tariff.Unit2.Name];
			if (!string.IsNullOrWhiteSpace(unit2))
			{
				yield return new RefCusTariffUOM
				{
					ZZ8_Type = "CU2",
					ZZ8_UOM = UnitOfMeasurementHelper.Instance.GetValue(rawTariff.Properties[Columns.Tariff.Unit2.Name])
				};
			}
		}

		static bool IsDuty(string preference)
		{
			return preference != "EXP" && preference != "EXC";
		}
	}
}
