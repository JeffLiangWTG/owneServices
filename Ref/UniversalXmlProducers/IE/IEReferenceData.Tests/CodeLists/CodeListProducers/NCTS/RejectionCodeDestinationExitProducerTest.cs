using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class RejectionCodeDestinationExitProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_RejectionCodeDestinationExit";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new RejectionCodeDestinationExit();
	}
}
