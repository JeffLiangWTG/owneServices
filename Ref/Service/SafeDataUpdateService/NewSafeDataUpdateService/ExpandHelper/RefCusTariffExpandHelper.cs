using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public static class RefCusTariffExpandHelper
	{
		public static void ExpandTariff(this IReferenceDataRepository repo, RefCusTariff[] tariffsChunk, IExpandClauseWrapper expandClause)
		{
			Argument.NotNull(repo, nameof(repo));
			Argument.NotNull(tariffsChunk, nameof(tariffsChunk));
			Argument.NotNull(expandClause, nameof(expandClause));

			var dataSetPKs = tariffsChunk.Select(x => x.ZZ1_PK).ToList();
			var expandTypeList = new List<Type>();
			SetExpandTypeList(expandTypeList, expandClause);

			var rateUOMsChunk = repo.Get<RefCusRateUOM>().GetChunk(dataSetPKs, expandTypeList);
			var ratesChunk = repo.Get<RefCusRate>().GetChunk(dataSetPKs, expandTypeList);
			var tariffAttributesChunk = repo.Get<RefCusTariffAttribute>().GetChunk(dataSetPKs, expandTypeList);
			var uomsChunk = repo.Get<RefCusTariffUOM>().GetChunk(dataSetPKs, expandTypeList);
			var tariffRelationshipChunk = repo.Get<RefCusTariffRelationship>().GetChunk(dataSetPKs, expandTypeList);
			var nationalCodesChunk = repo.Get<RefCusTariffNationalCode>().GetChunk(dataSetPKs, expandTypeList);
			var bRCharacteristicChunk = repo.Get<RefCusTariffBRCharacteristic>().GetChunk(dataSetPKs, expandTypeList);
			var bRCharacteristicAttributeChunk = repo.Get<RefCusTariffBRCharacteristicAttribute>().GetChunk(dataSetPKs, expandTypeList);
			var bRCharacteristicValueChunk = repo.Get<RefCusTariffBRCharacteristicValue>().GetChunk(dataSetPKs, expandTypeList);
			var additionalCodeLanguagesChunk = repo.Get<RefCusTariffAdditionalCodeLanguage>().GetChunk(dataSetPKs, expandTypeList);
			var additionalCodesChunk = repo.Get<RefCusTariffAdditionalCode>().GetChunk(dataSetPKs, expandTypeList);
			var conditionsChunk = repo.Get<RefCusCondition>().GetChunk(dataSetPKs, expandTypeList);
			var conditionValuesChunk = repo.Get<RefCusConditionValue>().GetChunk(dataSetPKs, expandTypeList);
			var conditionLanguagesChunk = repo.Get<RefCusConditionLanguage>().GetChunk(dataSetPKs, expandTypeList);
			var applicabilitiesChunk = repo.Get<RefCusApplicability>().GetChunk(dataSetPKs, expandTypeList);
			var vatApplicabilitiesChunk = repo.Get<RefCusVATApplicability>().GetChunk(dataSetPKs, expandTypeList);
			var excludedTradesChunk = repo.Get<RefCusExcludedTradeGroup>().GetChunk(dataSetPKs, expandTypeList);
			var languageChunk = repo.Get<RefCusTariffLanguage>().GetChunk(dataSetPKs, expandTypeList);

			var rateIdx = 0;
			var rateUomIdx = 0;
			var tariffAttributeIdx = 0;
			var uomIdx = 0;
			var tariffRelationshipIdx = 0;
			var tariffNationalCodeIdx = 0;
			var conditionIdx = 0;
			var conditionValueIdx = 0;
			var conditionLanguageIdx = 0;
			var applicabilityIdx = 0;
			var excludedTradeIdx = 0;
			var vatApplicabilityIdx = 0;
			var tariffLanguageIdx = 0;
			var additionalCodeLanguageIdx = 0;
			var additionalCodeIdx = 0;
			var characteristicIdx = 0;
			var characteristicAttributeIdx = 0;
			var characteristicValueIdx = 0;

			for (int i = 0; i < tariffsChunk.Length; i++)
			{
				var result = tariffsChunk[i];
				var dataSetPK = tariffsChunk[i].ZZ1_PK;

				var uoms = uomsChunk.GetSetData(x => x.ZZ8_DataSetPK, dataSetPK, ref uomIdx).GroupBy(x => x.ZZ8_ZZ1_Tariff ?? x.ZZ8_ZZW_TariffNationalCode);
				result.RefCusTariffUOMs = uoms.FilterToArray(dataSetPK);

				var rateUOMs = rateUOMsChunk.GetSetData(x => x.ZXG_DataSetPK, dataSetPK, ref rateUomIdx).GroupBy(x => x.ZXG_ZZ2_Rate);
				var excludedTrades = excludedTradesChunk.GetSetData(x => x.ZZC_DataSetPK, dataSetPK, ref excludedTradeIdx).GroupBy(x => x.ZZC_ZZT_Applicability);
				var applicabilities = applicabilitiesChunk.GetSetData(x => x.ZZT_DataSetPK, dataSetPK, ref applicabilityIdx).GroupBy(x => x.ZZT_ZX1_Conditions ?? x.ZZT_ZZ2_Rate ?? x.ZZT_ZY2_AdditionalCode, x =>
				{
					var model = x;
					model.RefCusExcludedTradeGroups = excludedTrades.FilterToArray(model.ZZT_PK);
					return model;
				});
				var rates = ratesChunk.GetSetData(x => x.ZZ2_DataSetPK, dataSetPK, ref rateIdx).GroupBy(x => x.ZZ2_ZZ1_Tariff ?? x.ZZ2_ZZW_TariffNationalCode, x =>
				{
					var model = x;
					model.RefCusRateUOMs = rateUOMs.FilterToArray(model.ZZ2_PK);
					model.RefCusApplicabilities = applicabilities.FilterToArray(model.ZZ2_PK);
					return model;
				});
				result.RefCusRates = rates.FilterToArray(dataSetPK);

				var conditionValues = conditionValuesChunk.GetSetData(x => x.ZX3_DataSetPK, dataSetPK, ref conditionValueIdx).GroupBy(x => x.ZX3_ZX1_Condition);
				var conditionLanguages = conditionLanguagesChunk.GetSetData(x => x.ZXJ_DataSetPK, dataSetPK, ref conditionLanguageIdx).GroupBy(x => x.ZXJ_ZX1_Condition);
				result.RefCusConditions = conditionsChunk.GetSetData(x => x.ZX1_DataSetPK, dataSetPK, ref conditionIdx).Select(x =>
				{
					var model = x;
					model.RefCusApplicabilities = applicabilities.FilterToArray(model.ZX1_PK);
					model.RefCusConditionValues = conditionValues.FilterToArray(model.ZX1_PK);
					model.RefCusConditionLanguages = conditionLanguages.FilterToArray(model.ZX1_PK);
					return model;
				}).ToArray();

				var vats = vatApplicabilitiesChunk.GetSetData(x => x.ZX5_DataSetPK, dataSetPK, ref vatApplicabilityIdx).GroupBy(x => x.ZX5_ZZ1_Tariff ?? x.ZX5_ZZW_TariffNationalCode);
				result.RefCusVATApplicabilities = vats.FilterToArray(dataSetPK);

				var additionalCodeLanguages = additionalCodeLanguagesChunk.GetSetData(x => x.ZY4_DataSetPK, dataSetPK, ref additionalCodeLanguageIdx).GroupBy(x => x.ZY4_ZY2_TariffAdditionalCode);
				var additionalCodes = additionalCodesChunk.GetSetData(x => x.ZY2_DataSetPK, dataSetPK, ref additionalCodeIdx).GroupBy(x => x.ZY2_ZZ1_Tariff ?? x.ZY2_ZZW_NationalCode, x =>
				{
					var model = x;
					model.RefCusTariffAdditionalCodeLanguages = additionalCodeLanguages.FilterToArray(model.ZY2_PK);
					model.RefCusApplicabilities = applicabilities.FilterToArray(model.ZY2_PK);
					return model;
				});
				result.RefCusTariffAdditionalCodes = additionalCodes.FilterToArray(dataSetPK);

				var tariffRelationships = tariffRelationshipChunk.GetSetData(x => x.ZZH_DataSetPK, dataSetPK, ref tariffRelationshipIdx).GroupBy(x => x.ZZH_ZZ1_Tariff, x =>
				{
					var model = x;
					model.RefCusApplicabilities = applicabilities.FilterToArray(model.ZZH_PK);
					return model;
				});
				result.RefCusTariffRelationships = tariffRelationships.FilterToArray(dataSetPK);

				var tariffAttributes = tariffAttributesChunk.GetSetData(x => x.ZZ3_DataSetPK, dataSetPK, ref tariffAttributeIdx).GroupBy(x => x.ZZ3_ZZ1_Tariff ?? x.ZZ3_ZZW_TariffNationalCode);
				result.RefCusTariffAttributes = tariffAttributes.FilterToArray(dataSetPK);

				result.RefCusTariffNationalCodes = nationalCodesChunk.GetSetData(x => x.ZZW_ZZ1_Tariff, dataSetPK, ref tariffNationalCodeIdx).Select(x =>
				{
					var model = x;
					model.RefCusRates = rates.FilterToArray(model.ZZW_PK);
					model.RefCusTariffAttributes = tariffAttributes.FilterToArray(model.ZZW_PK);
					model.RefCusVATApplicabilities = vats.FilterToArray(model.ZZW_PK);
					model.RefCusTariffUOMs = uoms.FilterToArray(model.ZZW_PK);
					model.RefCusTariffAdditionalCodes = additionalCodes.FilterToArray(model.ZZW_PK);
					return model;
				}).ToArray();

				result.RefCusTariffLanguages = languageChunk.GetSetData(x => x.ZX7_ZZ1_Tariff, dataSetPK, ref tariffLanguageIdx).ToArray();
				var brAttributes = bRCharacteristicAttributeChunk.GetSetData(a => a.ZB3_DataSetPK, dataSetPK, ref characteristicAttributeIdx).GroupBy(x => x.ZB3_ZB1_Characteristic);
				var brValues = bRCharacteristicValueChunk.GetSetData(a => a.ZB2_DataSetPK, dataSetPK, ref characteristicValueIdx).GroupBy(x => x.ZB2_ZB1_Characteristic);
				result.RefCusTariffBRCharacteristics = bRCharacteristicChunk.GetSetData(x => x.ZB1_DataSetPK, dataSetPK, ref characteristicIdx).Select(x =>
				{
					var model = x;
					model.RefCusTariffBRCharacteristicAttributes = brAttributes.FilterToArray(model.ZB1_PK);
					model.RefCusTariffBRCharacteristicValues = brValues.FilterToArray(model.ZB1_PK);
					return model;
				}).ToArray();
			}
		}

		//Will improve the performance further in WI00620352
		//for example: expand RefCusApplicabilty for REfCusRate only without retrieving REfCusapplicability for RefCusCondition
		static void SetExpandTypeList(List<Type> list, IExpandClauseWrapper expandClause)
		{
			Argument.NotNull(list, nameof(list));
			Argument.NotNull(expandClause, nameof(expandClause));

			var expandItems = expandClause.GetExpandClauseWrapper();
			foreach (var item in expandItems)
			{
				var expandType = item.GetExpandType();
				if (!list.Contains(expandType))
				{
					list.Add(expandType);
				}

				var clause = item.GetExpandClause();
				if (clause != null)
				{
					SetExpandTypeList(list, clause);
				}
			}
		}
	}
}
