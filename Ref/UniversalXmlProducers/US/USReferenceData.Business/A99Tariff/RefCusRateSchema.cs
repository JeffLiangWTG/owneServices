using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class RefCusRateSchema
	{
		public string RateType { get; set; } = string.Empty;
		public string RateCode { get; set; } = string.Empty;
		public string RateFormula { get; set; } = string.Empty;
		public DateTime StartDate { get; set; } = DateTime.MinValue;
		public DateTime EndDate { get; set; } = DateTime.MinValue;
		public IEnumerable<RefCusApplicabilitySchema> Applicabilities { get; set; }
	}
}
