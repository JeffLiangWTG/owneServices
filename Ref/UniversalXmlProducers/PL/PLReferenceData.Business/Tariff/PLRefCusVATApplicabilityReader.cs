using System;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Tariff
{
	class PLRefCusVATApplicabilityReader
	{
		public static ExtractedMeasureData GetRefCusVATApplicabilityFromIsztarHistoryResponse(measure measureData)
		{
			var retv = new ExtractedMeasureData
			{
				DataType = MeasureDataType.REF_CUS_VAT_APPLICABILITY,
				TariffCode = measureData.goodsNomenclature.goodsNomenclatureItemId,
				StartDate = measureData.validityStartDate,
				NKTaxOrFeeCode = GetNKTaxOrFeeCodeBasedOnDutyAmound(measureData),
				MeasureGeneratingRegulationId = measureData.measureGeneratingRegulationId,
				RegulationRoleType = measureData.regulationRoleType?.regulationRoleTypeId,
				ValidityEndDate = measureData.validityEndDate,
			};

			if (measureData.additionalCode != null) {
				retv.AdditionalCode = $"{measureData.additionalCode.additionalCodeType.additionalCodeTypeId}{measureData.additionalCode.additionalCodeCode}";
			}

			return retv;
		}

		protected static string GetNKTaxOrFeeCodeBasedOnDutyAmound(measure measureData)
		{
			if (measureData.measureComponent == null || measureData.measureComponent[0] == null) {
				throw new Exception("Measure component for specified measure data not found.");
			}

			switch (measureData.measureComponent[0].dutyAmount)
			{
				case 0.0m:
					return Constants.DutyCode.Ese;
				case 3.0m:
					return Constants.DutyCode.Spe;
				case 5.0m:
					return Constants.DutyCode.Min;
				case 8.0m:
					return Constants.DutyCode.Rid;
				case 23.0m:
					return Constants.DutyCode.Ord;
			}

			return string.Empty; // most likely data where endDate is before date defined in Constants.LatestVatChangeDate - we should not be worried about that if thats the case
		}
	}
}
