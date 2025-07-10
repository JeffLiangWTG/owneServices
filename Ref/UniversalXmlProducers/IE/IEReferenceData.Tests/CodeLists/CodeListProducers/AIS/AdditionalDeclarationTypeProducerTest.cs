using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class AdditionalDeclarationTypeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_AdditionalDeclarationTypes";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalDeclarationTypesDetails();
	}
}
