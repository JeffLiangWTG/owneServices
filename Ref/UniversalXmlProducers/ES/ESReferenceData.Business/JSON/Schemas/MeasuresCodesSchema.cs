using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class MeasuresCodesSchema : JsonlSchema
{
	public string additional_code_id { get; set; } = string.Empty;
	public IEnumerable<Description> descriptions { get; set; }

	public class Description
	{
		public string lang { get; set; } = string.Empty;
		public string text { get; set; } = string.Empty;
	}
}
