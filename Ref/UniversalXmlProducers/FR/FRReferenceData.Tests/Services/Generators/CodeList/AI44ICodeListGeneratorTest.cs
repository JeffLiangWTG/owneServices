using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	class AI44ICodeListGeneratorTest : BaseCodeListGeneratorTest<AI44ICodeListGenerator>
	{
		protected override string OutputXMLFileName => "DIE - AI44I Code Lists.xml";
	}
}
