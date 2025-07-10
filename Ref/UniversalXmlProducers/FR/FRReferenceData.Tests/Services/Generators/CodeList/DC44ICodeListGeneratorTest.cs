using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList
{
	class DC44ICodeListGeneratorTest : BaseCodeListGeneratorTest<DC44ICodeListGenerator>
	{
		protected override string OutputXMLFileName => "DIE - DC44I Code Lists.xml";
	}
}
