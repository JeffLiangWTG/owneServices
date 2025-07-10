using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Business;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class DeltaIECustomsProcedureCollectorProgram : ZipDataCollectorProgram
	{
		public override string CommandArgument => Constants.ProgramFunctions.UCC6CustomsProcedure;

		public override bool IsUCC6 => true;

		protected override IEnumerable<IUniversalReferenceDataFileGenerator> GetGenerators()
		{
			yield return new DeltaIECustomsProcedureUniversalReferenceDataFileGenerator();
		}
	}
}
