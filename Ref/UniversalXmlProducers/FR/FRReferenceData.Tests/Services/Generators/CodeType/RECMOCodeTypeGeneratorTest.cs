using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeType
{
	class RECMOCodeTypeGeneratorTest : RefCusCodeTypeGeneratorTest<RECMOCodeTypeGenerator>
	{
		protected override string OutputXMLFileName => "DIE Motivation for Rectification Request Code Type.xml";
	}
}
