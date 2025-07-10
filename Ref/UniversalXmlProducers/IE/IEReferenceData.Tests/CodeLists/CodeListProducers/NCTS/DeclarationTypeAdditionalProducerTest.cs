using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class DeclarationTypeAdditionalProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_DeclarationTypeAdditional";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new DeclarationTypeAdditional();
	}
}
