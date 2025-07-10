using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Business;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeList;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class DeltaIECodesCollectorProgram : ZipDataCollectorProgram
	{
		public override string CommandArgument => Constants.ProgramFunctions.DeltaIECodes;

		public override bool IsUCC6 => true;

		protected override IEnumerable<IUniversalReferenceDataFileGenerator> GetGenerators()
		{
			yield return new AHCONCodeListGenerator();
			yield return new AHIPCCodeListGenerator();
			yield return new AI44ICodeListGenerator();
			yield return new AR44ICodeListGenerator();
			yield return new CL716CodeListGenerator();
			yield return new CL790CodeListGenerator();
			yield return new CL047CodeListGenerator();
			yield return new CL740CodeListGenerator();
			yield return new CL214IMCodeListGenerator();
			yield return new DC44ICodeListGenerator();
			yield return new ENSUBCodeListGenerator();
			yield return new InvalidationMotivationCodeListGenerator();
			yield return new RECMOCodeListGenerator();
			yield return new TD44ICodeListGenerator();
			yield return new TRNATCodeListGenerator();
			yield return new TAXCodeListGenerator();
		}
	}
}
