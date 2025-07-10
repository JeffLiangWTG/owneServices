using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	public class RefCusCodeListSchema
	{
		public string CodeType { get; set; } = string.Empty;
		public string Code { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTime StartDate { get; set; } = DateTime.MinValue;
		public DateTime EndDate { get; set; } = DateTime.MinValue;
		public string Country { get; set; } = string.Empty;
		public IEnumerable<RefCusCodeListAttributeSchema> Attributes { get; set; }
	}
}
