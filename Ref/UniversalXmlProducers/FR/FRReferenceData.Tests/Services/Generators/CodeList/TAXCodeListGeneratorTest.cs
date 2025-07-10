using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList;

sealed class TAXCodeListGeneratorTest : BaseCodeListGeneratorTest<TAXCodeListGenerator>
{
	protected override string OutputXMLFileName => "FR - TAX Code Lists.xml";
}
