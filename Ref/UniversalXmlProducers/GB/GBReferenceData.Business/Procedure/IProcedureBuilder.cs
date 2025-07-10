using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Procedure
{
	public interface IProcedureBuilder
	{
		void BuildXml(DateTime publicationDate, IEnumerable<ProcedureCodeData> data, IEnumerable<CategoryProcedureMapping> categoryProcedureMapping, string outputPath);
	}
}
