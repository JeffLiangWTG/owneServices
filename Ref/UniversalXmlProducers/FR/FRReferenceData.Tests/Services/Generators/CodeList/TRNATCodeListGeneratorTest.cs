using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators.CodeList;

sealed class TRNATCodeListGeneratorTest : BaseCodeListGeneratorTest<TRNATCodeListGenerator>
{
	protected override string OutputXMLFileName => "DIE - TRNAT Code Lists.xml";
}
