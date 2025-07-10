using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class PreviousDocumentProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_PreviousDocument";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new PreviousDocument();
	}
}
