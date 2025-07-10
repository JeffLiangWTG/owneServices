using System.Collections.Generic;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Tariff
{
	class PLRefCusRateReader
	{
		public static ExtractedMeasureData GetRefCusRateFromIsztarHistoryResponse(measure measureData)
		{
			var retv = new ExtractedMeasureData
			{
				DataType = MeasureDataType.REF_CUS_RATE,
				TariffCode = measureData.goodsNomenclature.goodsNomenclatureItemId,
				StartDate = measureData.validityStartDate,
				RateFormula = RateFormulaGenerator.GenerateRateFormulaFromMeasure(measureData),
				MeasureGeneratingRegulationId = measureData.measureGeneratingRegulationId,
				RegulationRoleType = measureData.regulationRoleType?.regulationRoleTypeId,
				ValidityEndDate = measureData.validityEndDate,
				RateNkTradeGroup = measureData.geographicalArea?.geographicalAreaId
			};

			if (measureData.additionalCode != null)
			{
				retv.AdditionalCode = $"{measureData.additionalCode.additionalCodeType.additionalCodeTypeId}{measureData.additionalCode.additionalCodeCode}";
			}

			if (measureData.measureComponent != null)
			{
				retv.UnitCodes = new List<string>();
				foreach (var measureComponent in measureData.measureComponent)
				{
					var unitCode = RateFormulaGenerator.GetFullMeasurementUnitCode(measureComponent);
					if (!string.IsNullOrEmpty(unitCode))
					{
						retv.UnitCodes.Add(unitCode);
					}
				}
			}

			return retv;
		}
	}
}

