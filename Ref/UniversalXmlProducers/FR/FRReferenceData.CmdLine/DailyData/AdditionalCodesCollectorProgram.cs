using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Business;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class AdditionalCodesCollectorProgram : ZipDataCollectorProgram
	{
		public override string CommandArgument => Constants.ProgramFunctions.AdditionalCodes;

		protected override IEnumerable<IUniversalReferenceDataFileGenerator> GetGenerators()
		{
			yield return new DocumentNatureUniversalReferenceDataFileGenerator();
			yield return new DocumentTypeUniversalReferenceDataFileGenerator();
			yield return new SpecialMentionUniversalReferenceDataFileGenerator();
			yield return new AdditionalCodesUniversalReferenceDataFileGenerator();
			yield return new VATAdditionalCodesUniversalReferenceDataFileGenerator();
			yield return new ConditionTypeUniversalReferenceDataFileGenerator();
		}
	}
}
