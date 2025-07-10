using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeList;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	class AHCONCodeListGeneratorTest : BaseCodeListGeneratorTest<AHCONCodeListGenerator>
	{
		protected override string OutputXMLFileName => "DIE - AHCON Code Lists.xml";
	}
}
