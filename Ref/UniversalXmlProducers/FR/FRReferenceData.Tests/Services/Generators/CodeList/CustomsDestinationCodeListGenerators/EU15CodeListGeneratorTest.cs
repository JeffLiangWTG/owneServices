using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	class EU15CodeListGeneratorTest : BaseCodeListGeneratorTest<EU15CodeListGenerator>
	{
		protected override string OutputXMLFileName => "FR - EU15 Code Lists.xml";
	}
}
