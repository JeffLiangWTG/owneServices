using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	class EU17CodeListGeneratorTest : BaseCodeListGeneratorTest<EU17CodeListGenerator>
	{
		protected override string OutputXMLFileName => "FR - EU17 Code Lists.xml";
	}
}
