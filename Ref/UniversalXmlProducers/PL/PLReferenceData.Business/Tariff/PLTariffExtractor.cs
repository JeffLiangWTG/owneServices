using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Tariff
{
	class PLTariffExtractor
	{
		public static List<RefCusTariff> GeneratePLTariffData(IsztarHistoryResponse isztarData, IDateTimeProvider dateTimeProvider)
		{
			var tempDataList = ExtractMeasuresDataFromIsztarHistoryResponse(isztarData);

			FillExtractedMeasureDataWithValidEndDate(isztarData, tempDataList);

			return GetRefCusTariffFromPLTariff(tempDataList, dateTimeProvider);
		}

		static List<ExtractedMeasureData> ExtractMeasuresDataFromIsztarHistoryResponse(IsztarHistoryResponse isztarData)
		{
			var tempDataList = new List<ExtractedMeasureData>();

			for (var i = 0; i < isztarData.IsztarHistoryItem.Length; i++)
			{
				IsztarHistoryResponseIsztarHistoryItem baseIsztarHistoryItem = isztarData.IsztarHistoryItem[i];

				switch (baseIsztarHistoryItem.Item.ToString())
				{
					case Taric4Constants.GroupNodes.FindMeasureByDatesResponseHistory:
						{
							OnFindMeasureByDatesResponseHistory(tempDataList, baseIsztarHistoryItem);
						}
						break;
					default:
						break;
				}
			}

			return tempDataList;
		}

		static void FillExtractedMeasureDataWithValidEndDate(IsztarHistoryResponse isztarData, List<ExtractedMeasureData> extractedMeasureData)
		{
			for (var i = 0; i < isztarData.IsztarHistoryItem.Length; i++)
			{
				IsztarHistoryResponseIsztarHistoryItem baseIsztarHistoryItem = isztarData.IsztarHistoryItem[i];

				switch (baseIsztarHistoryItem.Item.ToString())
				{
					case Taric4Constants.GroupNodes.FindBaseRegulationByDatesResponseHistory:
						{
							OnFindBaseRegulationByDatesResponseHistory(extractedMeasureData, baseIsztarHistoryItem);
						}
						break;
					case Taric4Constants.GroupNodes.FindModificationRegulationByDatesResponseHistory:
						{
							OnFindModificationRegulationByDatesResponseHistory(extractedMeasureData, baseIsztarHistoryItem);
						}
						break;
					default:
						break;
				}
			}
		}

		protected static DateTime GetEndDateTime(DateTime regulationDateTime, DateTime measureDateTime)
		{
			if (regulationDateTime == DateTime.MinValue && measureDateTime == DateTime.MinValue)
			{
				return Constants.ConstantEndDate;
			}
			else if (regulationDateTime == DateTime.MinValue && measureDateTime != DateTime.MinValue)
			{
				return measureDateTime;
			}
			else if ((regulationDateTime != DateTime.MinValue) && ((measureDateTime == DateTime.MinValue) || ((measureDateTime > regulationDateTime))))
			{
				return regulationDateTime;
			}
			else if ((measureDateTime < regulationDateTime))
			{
				//throw new Exception($"Expected that DateTime:[{regulationDateTime}] < DateTime:[{measureDateTime}] - you might want to inform Polish PUESC about this error."); // please leave this, will be usefull for debugging in the future
				return PLReferenceData.Business.Constants.LatestVatChangeDate.AddYears(-1);
			}

			return Constants.ConstantEndDate;
		}

		static void OnFindModificationRegulationByDatesResponseHistory(List<ExtractedMeasureData> tempDataList, IsztarHistoryResponseIsztarHistoryItem baseIsztarHistoryItem)
		{
			var modificationRegulationList = ((findModificationRegulationByDatesResponseHistory)baseIsztarHistoryItem.Item).ModificationRegulation;
			if (modificationRegulationList == null)
			{
				return;
			}

			foreach (var singleModificationRegulation in modificationRegulationList)
			{
				var plTariffList = tempDataList.FindAll(x => x.MeasureGeneratingRegulationId == singleModificationRegulation.baseRegulation.baseRegulationId);
				if (plTariffList != null)
				{
					foreach (var item in plTariffList)
					{
						item.EndDate = GetEndDateTime(singleModificationRegulation.effectiveEndDate, item.ValidityEndDate);
					}
				}
			}
		}

		static void OnFindBaseRegulationByDatesResponseHistory(List<ExtractedMeasureData> tempDataList, IsztarHistoryResponseIsztarHistoryItem baseIsztarHistoryItem)
		{
			var baseRegulationList = ((findBaseRegulationByDatesResponseHistory)baseIsztarHistoryItem.Item).BaseRegulation;
			if (baseRegulationList == null)
			{
				return;
			}

			foreach (var singleBaseRegulation in baseRegulationList)
			{
				var baseRegulationId = singleBaseRegulation?.baseRegulationId;
				var regulationRoleTypeId = singleBaseRegulation?.regulationRoleType?.regulationRoleTypeId;
				var plTariffList = tempDataList.FindAll(x => x.MeasureGeneratingRegulationId == baseRegulationId && x.RegulationRoleType == regulationRoleTypeId);
				if (plTariffList != null)
				{
					foreach (var item in plTariffList)
					{
						var endDate = GetEndDateTime(singleBaseRegulation.effectiveEndDate, item.ValidityEndDate);
						if (item.EndDate < endDate)
						{
							item.EndDate = endDate;
						}
					}
				}
			}
		}

		static void OnFindMeasureByDatesResponseHistory(List<ExtractedMeasureData> tempDataList, IsztarHistoryResponseIsztarHistoryItem baseIsztarHistoryItem)
		{
			foreach (var singleMeasure in ((findMeasureByDatesResponseHistory)baseIsztarHistoryItem.Item).Measure)
			{
				if (singleMeasure.measureType.measureTypeId == Constants.MeasureType.MeasureVatVATApplicabilityTypeId)
				{
					OnMeasureVatVATApplicabilityTypeId(tempDataList, singleMeasure);
				}
				else if (singleMeasure.measureType.measureTypeId[0] == Constants.NationalMeasureStartingLetter)
				{
					OnNationalMeasureStartingLetter(tempDataList, singleMeasure);
				}
				else if (singleMeasure.measureType.measureTypeId == Constants.MeasureType.MeasureRateTypeId)
				{
					OnMeasureRateTypeId(tempDataList, singleMeasure);
					OnNationalMeasureStartingLetter(tempDataList, singleMeasure);
				}
			}
		}

		static void OnMeasureVatVATApplicabilityTypeId(List<ExtractedMeasureData> plTariffList, measure singleMeasure)
		{
			ExtractedMeasureData tempData;

			try
			{
				tempData = PLRefCusVATApplicabilityReader.GetRefCusVATApplicabilityFromIsztarHistoryResponse(singleMeasure);
				if (string.IsNullOrEmpty(tempData.NKTaxOrFeeCode))
				{
					return;
				}
				plTariffList.Add(tempData);
			}
			catch (Exception ex)
			{
				var errorMessage = $"{Constants.MeasureType.MeasureVatVATApplicabilityTypeId}:Measure ID [{singleMeasure.measureGeneratingRegulationId}]";
				Console.Error.WriteLine(errorMessage);
				Console.Error.WriteLine(ex);
				return;
			}
		}

		protected static void OnNationalMeasureStartingLetter(List<ExtractedMeasureData> plTariffList, measure singleMeasure)
		{
			if (singleMeasure.measureCondition != null &&
				!singleMeasure.measureCondition.Any(x => x.measureAction != null && (x.measureAction.actionCode == "05" || x.measureAction.actionCode == "25")))
			{
				try
				{
					plTariffList.Add(PLRefCusConditionReader.GetRefCusConditionFromIsztarHistoryResponse(singleMeasure));
				}
				catch (Exception ex)
				{
					var errorMessage = $"{Constants.NationalMeasureStartingLetter}:Measure ID [{singleMeasure.measureGeneratingRegulationId}]";
					Console.Error.WriteLine(errorMessage);
					Console.Error.WriteLine(ex);
					return;
				}
			}
		}

		static void OnMeasureRateTypeId(List<ExtractedMeasureData> plTariffList, measure singleMeasure)
		{
			try
			{
				plTariffList.Add(PLRefCusRateReader.GetRefCusRateFromIsztarHistoryResponse(singleMeasure));
			}
			catch (Exception ex)
			{
				var errorMessage = $"{Constants.MeasureType.MeasureRateTypeId}:Measure ID [{singleMeasure.measureGeneratingRegulationId}]";
				Console.Error.WriteLine(errorMessage);
				Console.Error.WriteLine(ex);
				return;
			}
		}

		protected static List<RefCusTariff> GetRefCusTariffFromPLTariff(List<ExtractedMeasureData> data, IDateTimeProvider dateTimeProvider)
		{
			var retv = new List<RefCusTariff>();

			data.Sort((a, b) => string.Compare(a.TariffCode, b.TariffCode, StringComparison.OrdinalIgnoreCase));

			foreach (var tariffGroup in data.GroupBy(d => d.TariffCode))
			{
				var applicabilitiesList = new List<RefCusVATApplicability>();
				var conditionList = new List<RefCusCondition>();
				var rateList = new List<RefCusRate>();

				foreach (var item in tariffGroup)
				{
					if (!IsExpiredData(item.EndDate, dateTimeProvider))
					{
						switch (item.DataType)
						{
							case MeasureDataType.REF_CUS_VAT_APPLICABILITY:
								applicabilitiesList.Add(GetRefCusVATApplicabilityFromPLTariff(item));
								break;
							case MeasureDataType.REF_CUS_CONDITION:
								conditionList.AddRange(GetRefCusConditionsFromPLTariff(item));
								break;
							case MeasureDataType.REF_CUS_RATE:
								rateList.Add(GetRefCusRateFromPLTariff(item));
								break;
							default:
								break;
						}
					}
				}

				if (applicabilitiesList.Count != 0 || conditionList.Count != 0 || rateList.Count != 0)
				{
					retv.Add(new RefCusTariff()
					{
						ZZ1_TariffCode = tariffGroup.Key,
						RefCusVATApplicabilities = applicabilitiesList.ToArray(),
						RefCusConditions = conditionList.ToArray(),
						RefCusRates = rateList.ToArray()
					});
				}
			}

			return retv;
		}

		protected static RefCusVATApplicability GetRefCusVATApplicabilityFromPLTariff(ExtractedMeasureData itemData)
		{
			return new RefCusVATApplicability()
			{
				ZX5_EndDate = itemData.EndDate.ToSmallDateTime(),
				ZX5_StartDate = itemData.StartDate,
				ZX5_ZZF_NKTaxOrFeeCode = itemData.NKTaxOrFeeCode,
				ZX5_AdditionalCode = itemData.AdditionalCode
			};
		}

		static List<RefCusCondition> GetRefCusConditionsFromPLTariff(ExtractedMeasureData itemData)
		{
			foreach (var refCusCondition in itemData.RefCusConditions)
			{
				GetRefCusConditionFromPLTariff(itemData, refCusCondition);
			}

			return itemData.RefCusConditions;
		}

		protected static RefCusCondition GetRefCusConditionFromPLTariff(ExtractedMeasureData itemData, RefCusCondition refCusCondition)
		{
			refCusCondition.ZX1_EndDate = itemData.EndDate.ToSmallDateTime();
			refCusCondition.ZX1_StartDate = itemData.StartDate;
			refCusCondition.ZX1_ZX2_NKConditionType = itemData.MeasureTypeId;
			refCusCondition.ZX1_Comment = GetCommentBasedOnMeasureTypeId(itemData.MeasureTypeId);
			refCusCondition.ZX1_IsExport = IsExport(itemData.MeasureTypeId);
			refCusCondition.ZX1_IsImport = !IsExport(itemData.MeasureTypeId);
			refCusCondition.RefCusApplicabilities = new RefCusApplicability[]
			{
				new RefCusApplicability()
				{
					ZZT_AdditionalCode = itemData.AdditionalCode,
					ZZT_EndDate = itemData.EndDate.ToSmallDateTime(),
					ZZT_StartDate = itemData.StartDate,
					ZZT_ZZA_NKTradeGroup = itemData.ConditionNkTradeGroup
				}
			};

			return refCusCondition;
		}

		static RefCusRate GetRefCusRateFromPLTariff(ExtractedMeasureData itemData)
		{
			return new RefCusRate()
			{
				ZZ2_ZY1_NKRateCode = "1A1",
				ZZ2_ZY1_ZZR_NKRateType = "EXC",
				ZZ2_StartDate = itemData.StartDate,
				ZZ2_EndDate = itemData.EndDate.ToSmallDateTime(),
				ZZ2_RateFormula = itemData.RateFormula,
				RefCusApplicabilities = new RefCusApplicability[]
				{
					new RefCusApplicability()
					{
						ZZT_StartDate = itemData.StartDate,
						ZZT_EndDate = itemData.EndDate.ToSmallDateTime(),
						ZZT_AdditionalCode = itemData.AdditionalCode,
						ZZT_ZZA_NKTradeGroup = itemData.RateNkTradeGroup
					}
				},
				RefCusRateUOMs = GetRefCusRateUOM(itemData.UnitCodes).DistinctBy(x => x.ZXG_UOM).ToArray()
			};
		}

		static IEnumerable<RefCusRateUOM> GetRefCusRateUOM(List<string> unitCodes)
		{
			if (unitCodes == null
				|| unitCodes.Count == 0)
			{
				yield break;
			}

			foreach (var unitCode in unitCodes)
			{
				yield return new RefCusRateUOM()
				{
					ZXG_UOM = unitCode
				};
			}
		}

		protected static string GetCommentBasedOnMeasureTypeId(string measureTypeId)
		{
			if (!Constants.PTypeMeasureCommentMeasureTypeDictionary.TryGetValue(measureTypeId, out string retv))
			{
				return null;
			}

			return retv;
		}

		protected static bool IsExport(string measureTypeId) => measureTypeId == Constants.MeasureType.P03;

		static bool IsExpiredData(DateTime dateTime, IDateTimeProvider dateTimeProvider) =>
			dateTime.Year <= (dateTimeProvider.CurrentLocalDateTime.Year - Constants.ExpiredYears)
			|| dateTime < PLReferenceData.Business.Constants.LatestVatChangeDate;
	}
}
