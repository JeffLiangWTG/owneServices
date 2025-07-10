using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeType
{
	class TD44ICodeTypeGeneratorTest : RefCusCodeTypeGeneratorTest<TD44ICodeTypeGenerator>
	{
		protected override string OutputXMLFileName => "DIE Import Transport Document Type Code Type.xml";
	}
}
