using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeList;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList;

class ENSUBCodeListGeneratorTest : BaseCodeListGeneratorTest<ENSUBCodeListGenerator>
{
	protected override string OutputXMLFileName => "FR - ENSUB Code Lists.xml";
}
