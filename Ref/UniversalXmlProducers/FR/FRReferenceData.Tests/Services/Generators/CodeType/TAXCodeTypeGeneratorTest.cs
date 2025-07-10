using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeType;
sealed class TAXCodeTypeGeneratorTest : RefCusCodeTypeGeneratorTest<TAXCodeTypeGenerator>
{
	protected override string OutputXMLFileName => "FR French National Tax Code Code Type.xml";
}
