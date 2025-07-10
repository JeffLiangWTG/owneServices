using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class AdditionalReferenceProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_AdditionalReference";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalReference();
	}
}
