using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class TOMeasuresSchema : JsonlSchema
	{
		public string scope { get; set; } = string.Empty;
		public string goods_id { get; set; } = string.Empty;
		public string geographical_area_id { get; set; } = string.Empty;
		public string measure_type_id { get; set; } = string.Empty;
		public IEnumerable<TaxCode> tax_codes { get; set; }
		public IEnumerable<Component> components { get; set; }
		public DateTime? start_date { get; set; }
		public DateTime? end_date { get; set; }

		public class TaxCode
		{
			public string tax_code_id { get; set; } = string.Empty;
			public string scope { get; set; } = string.Empty;
		}

		public class Component
		{
#nullable enable
			public decimal? duty_amount { get; set; } = -1;
			public string? duty_expression_id { get; set; } = string.Empty;
			public string? measurement_unit_id { get; set; } = string.Empty;
			public string? monetary_unit_id { get; set; } = string.Empty;
			public string? measurement_unit_qualifier_id { get; set; } = string.Empty;
#nullable disable
		}
	}
}
