using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Business;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class PNTSCodeListsCollectorProgram : ExcelDataCollectorProgram
	{
		protected override IEnumerable<IUniversalReferenceDataFileGenerator> GetGenerators()
		{
			yield return new AR44TCodeListGenerator();
			yield return new AI44TCodeListGenerator();
			yield return new DC44TCodeListGenerator();
		}

		public override string CommandArgument => Constants.ProgramFunctions.PNTSCodeLists;
	}
}
