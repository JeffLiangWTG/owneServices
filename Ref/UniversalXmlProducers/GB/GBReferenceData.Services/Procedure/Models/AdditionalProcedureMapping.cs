using System.Collections.Generic;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models
{
	public class AdditionalProcedureMapping
	{
		public string ProcedureCode { get; set; }
		public List<string> AdditionalProcedureCodes { get; set; }
	}
}
