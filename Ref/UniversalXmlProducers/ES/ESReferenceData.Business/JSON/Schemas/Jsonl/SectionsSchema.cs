using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class SectionsSchema : JsonlSchema
	{
		public int section_id { get; set; }
		public IEnumerable<Description> descriptions { get; set; }
		public IEnumerable<int> chapters { get; set; }

		public class Description
		{
			public string lang { get; set; } = string.Empty;
			public string text { get; set; } = string.Empty;
		}
	}
}
