using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeList;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	class AHIPCCodeListGeneratorTest : BaseCodeListGeneratorTest<AHIPCCodeListGenerator>
	{
		protected override string OutputXMLFileName => "DIE - AHIPC Code Lists.xml";
	}
}
