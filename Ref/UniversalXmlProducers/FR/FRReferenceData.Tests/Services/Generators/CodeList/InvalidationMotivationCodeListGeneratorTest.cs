using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	class InvalidationMotivationCodeListGeneratorTest : BaseCodeListGeneratorTest<InvalidationMotivationCodeListGenerator>
	{
		protected override string OutputXMLFileName => "DIE - INVMO Code Lists.xml";
	}
}
