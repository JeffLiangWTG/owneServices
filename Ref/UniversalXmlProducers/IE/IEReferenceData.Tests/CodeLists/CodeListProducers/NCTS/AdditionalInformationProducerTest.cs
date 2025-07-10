using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class AdditionalInformationProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_AdditionalInformation";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalInformation();
	}
}
