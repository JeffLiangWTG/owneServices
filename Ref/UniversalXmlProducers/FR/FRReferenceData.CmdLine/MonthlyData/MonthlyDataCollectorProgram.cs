using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Business;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class MonthlyDataCollectorProgram : ZipDataCollectorProgram
	{
		public override string CommandArgument => Constants.ProgramFunctions.MonthlyData;

		protected override IEnumerable<IUniversalReferenceDataFileGenerator> GetGenerators()
		{
			//Add here generators that are not using Drop.Zip archive
			yield break;
		}
	}
}
