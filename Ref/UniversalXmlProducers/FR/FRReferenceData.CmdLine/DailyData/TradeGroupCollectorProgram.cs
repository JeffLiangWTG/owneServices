using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Business;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class TradeGroupCollectorProgram : ZipDataCollectorProgram
	{
		public override string CommandArgument => Constants.ProgramFunctions.TradeGroup;

		protected override IEnumerable<IUniversalReferenceDataFileGenerator> GetGenerators()
		{
			yield return new TradeGroupReferenceDataFileGenerator();
		}
	}
}
