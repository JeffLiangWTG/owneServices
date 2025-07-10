using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeListAttributeName;

sealed class TAXCodeListAttributeNameGeneratorTest : BaseCodeListAttributeNameGeneratorTest<TAXCodeListAttributeNameGenerator>
{
	protected override string OutputXMLFileName => "FR TAX Attribute Name.xml";
}
