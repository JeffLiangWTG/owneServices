using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class Measure
	{
		public Measure() { }

		public string AdditionalCodeId { get; set; }

		public string SIDAdditionalCode { get; set; }

		public string AdditionalCodeType { get; set; }

		public string GeographicalAreaId { get; set; }

		public string GoodsNomenclatureCode { get; set; }

		public string SIDGoodsNomenclature { get; set; }

		public string MeasureType { get; set; }

		public string National { get; set; }

		public string RegulationId { get; set; }

		public string RegulationRoleType { get; set; }

		public string SID { get; set; }

		public string SIDGeographicalArea { get; set; }

		public DateTime? DateStart { get; set; }

		public string StoppedFlag { get; set; }

		public string ChangeType { get; set; }

		public string Expression { get; set; }

		public IEnumerable<MeasureComponent> Components { get; set; }

		public string CleanId { get; set; }

		public string Formula { get; set; }

		public string AdditionalCode { get; set; }

		public string TaxOrFeeCode { get; set; }

		public IEnumerable<string> UnitOfMeasure { get; set; }

		public string RateCode { get; set; }

		public string RateType { get; set; }

		public DateTime? DateEnd { get; set; }
	}

	public class MeasureComponent
	{
		public decimal? DutyAmount { get; set; }
		public string DutyExpressionId { get; set; }
		public string MeasurementUnitCode { get; set; }
		public string MonetaryUnitCode { get; set; }
		public string National { get; set; }
	}

	public class MeasureCondition
	{
		public string ConditionCode { get; set; }
		public string National { get; set; }
		public DateTime? DateStart { get; set; }
		public string Type { get; set; }
		public string ChangeType { get; set; }
		public IEnumerable<MeasureConditionDescription> Descriptions { get; set; }
	}

	public class MeasureConditionDescription
	{
		public string Description { get; set; }
		public string Language { get; set; }
		public string National { get; set; }
	}
}
