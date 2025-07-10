using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	public class RefCusApplicabilitySchema
	{
		public string TradeGroup { get; set; } = string.Empty;
		public DateTime StartDate { get; set; } = DateTime.MinValue;
		public DateTime EndDate { get; set; } = DateTime.MinValue;
		public IEnumerable<string> Exclusions { get; set; }
	}
}
