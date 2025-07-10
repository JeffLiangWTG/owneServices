using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class NomenclaturesSchema : JsonlSchema
	{
		public string suffix { get; set; } = string.Empty;
		public string goods_id { get; set; } = string.Empty;
		public string scope { get; set; } = string.Empty;
		public IEnumerable<Description> descriptions { get; set; }
		public DateTime? start_date { get; set; } = DateTime.MinValue;
		public DateTime? end_date { get; set; } = DateTime.MaxValue;
		public string hierarchy_position { get; set; } = string.Empty;
		public string indent { get; set; } = string.Empty;

		public class Description
		{
			public string lang { get; set; } = string.Empty;
			public string text { get; set; } = string.Empty;
		}
	}
}
