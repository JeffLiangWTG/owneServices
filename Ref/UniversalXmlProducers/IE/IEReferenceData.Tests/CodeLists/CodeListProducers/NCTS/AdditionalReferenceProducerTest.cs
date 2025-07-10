using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class AdditionalReferenceCodeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_AdditionalReference";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalReference();
	}
}
