using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class RefCusTariffSchema
	{
		public string TariffCode { get; set; } = string.Empty;
		public string Description { get; set; }
		public string Country { get; set; } = string.Empty;
		public string TariffType { get; set; } = string.Empty;
		public DateTime StartDate { get; set; } = DateTime.MinValue;
		public DateTime EndDate { get; set; } = DateTime.MinValue;
		public IEnumerable<RefCusTariffAttributeSchema> Attributes { get; set; }
		public IEnumerable<string> ParentTariffs { get; set; }
		public IEnumerable<RefCusRateSchema> Rates { get; set; }
		public IEnumerable<RefCusConditionSchema> Conditions { get; set; }
	}
}
