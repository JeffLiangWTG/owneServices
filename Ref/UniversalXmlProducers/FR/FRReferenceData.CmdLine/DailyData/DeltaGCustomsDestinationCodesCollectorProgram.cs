using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Business;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class DeltaGCustomsDestinationCodesCollectorProgram : ZipDataCollectorProgram
	{
		public override string CommandArgument => Constants.ProgramFunctions.CustomsDestination;

		protected override IEnumerable<IUniversalReferenceDataFileGenerator> GetGenerators()
		{
			yield return new CO15CodeListGenerator();
			yield return new CO17CodeListGenerator();
			yield return new IM15CodeListGenerator();
			yield return new IM17CodeListGenerator();
			yield return new EU15CodeListGenerator();
			yield return new EU17CodeListGenerator();
			yield return new EX15CodeListGenerator();
			yield return new EX17CodeListGenerator();
		}
	}
}
