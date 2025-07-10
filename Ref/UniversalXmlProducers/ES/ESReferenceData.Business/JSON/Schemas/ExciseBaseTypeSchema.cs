using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class ExciseBaseTypeSchema : JsonlSchema
{
	public IEnumerable<string> pvp { get; set; }
}
