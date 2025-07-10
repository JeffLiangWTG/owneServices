
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Tariff
{
	class PLRefCusConditionReader
	{
		static readonly List<string> TrueMeansStopActionCodeList = new List<string>() { "4", "5", "6", "9" };

		public static ExtractedMeasureData GetRefCusConditionFromIsztarHistoryResponse(measure measureData)
		{
			var retv = new ExtractedMeasureData
			{
				DataType = MeasureDataType.REF_CUS_CONDITION,

				TariffCode = measureData.goodsNomenclature.goodsNomenclatureItemId,
				MeasureTypeId = measureData.measureType.measureTypeId,
				StartDate = measureData.validityStartDate,
				MeasureGeneratingRegulationId = measureData.measureGeneratingRegulationId,
				RegulationRoleType = measureData.regulationRoleType?.regulationRoleTypeId,
				ValidityEndDate = measureData.validityEndDate,
			};

			if (measureData.additionalCode != null)
			{
				retv.AdditionalCode = $"{measureData.additionalCode.additionalCodeType.additionalCodeTypeId}{measureData.additionalCode.additionalCodeCode}";
			}

			if (measureData.geographicalArea != null)
			{
				retv.ConditionNkTradeGroup = measureData.geographicalArea.geographicalAreaId;
			}

			retv.RefCusConditions = new List<RefCusCondition>();
			if (measureData.measureCondition != null)
			{
				foreach (var groupItem in measureData.measureCondition.Where(x => !string.IsNullOrEmpty(x?.certificate?.certificateCode)).GroupBy(x => x.measureAction?.actionCode))
				{
					var actionCode = groupItem.Key;
					if (!string.IsNullOrEmpty(actionCode))
					{
						var refCusConditionValues = new List<RefCusConditionValue>();
						foreach (var item in groupItem)
						{
							var zx3Value = $"{item.certificate.certificateType.certificateTypeCode}{item.certificate.certificateCode}";
							if (!refCusConditionValues.Any(x => x.ZX3_Value == zx3Value))
							{
								refCusConditionValues.Add(
									new RefCusConditionValue()
									{
										ZX3_LogicalORWithinGroup = 0,
										ZX3_Value = zx3Value,
										ZX3_ZX4_NKValueType = "SUP"
									});
							}
						}
						var refCusCondition = new RefCusCondition
						{
							ZX1_ConditionValueTrueMeansStop = TrueMeansStopActionCodeList.Contains(actionCode),
							RefCusConditionValues = refCusConditionValues.ToArray(),
						};
						retv.RefCusConditions.Add(refCusCondition);
					}
				}
			}

			return retv;
		}
	}
}
