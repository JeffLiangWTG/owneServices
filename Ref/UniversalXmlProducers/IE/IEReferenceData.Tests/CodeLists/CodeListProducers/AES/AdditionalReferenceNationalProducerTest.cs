using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class AdditionalReferenceNationalProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_AdditionalReferenceNational";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalReferenceNational();
	}
}
