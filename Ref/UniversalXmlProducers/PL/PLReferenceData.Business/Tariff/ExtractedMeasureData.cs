using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Tariff
{
	public enum MeasureDataType
	{
		REF_CUS_VAT_APPLICABILITY,
		REF_CUS_CONDITION,
		REF_CUS_RATE
	}

	public class ExtractedMeasureData
	{
		public MeasureDataType DataType { get; set; }

		public string TariffCode { get; set; }
		public string NKTaxOrFeeCode { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string AdditionalCode { get; set; }

		public string MeasureGeneratingRegulationId { get; set; }
		public string RegulationRoleType { get; set; }
		public DateTime ValidityEndDate { get; set; }

		// RefCusCondition
		public string MeasureTypeId { get; set; }
		public string ConditionNkTradeGroup { get; set; }
		public List<RefCusCondition> RefCusConditions { get; set; }

		// RefCusRate
		public string RateNkTradeGroup { get; set; }
		public string RateFormula { get; set; }
		public List<string> UnitCodes { get; set; }
	}
}
