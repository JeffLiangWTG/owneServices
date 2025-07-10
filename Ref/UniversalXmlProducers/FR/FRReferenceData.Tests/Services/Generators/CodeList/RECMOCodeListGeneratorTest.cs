using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	class RECMOCodeListGeneratorTest : BaseCodeListGeneratorTest<RECMOCodeListGenerator>
	{
		protected override string OutputXMLFileName => "DIE - RECMO Code Lists.xml";
	}
}
