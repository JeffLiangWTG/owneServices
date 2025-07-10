using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	class IM15CodeListGeneratorTest : BaseCodeListGeneratorTest<IM15CodeListGenerator>
	{
		protected override string OutputXMLFileName => "FR - IM15 Code Lists.xml";
	}
}
