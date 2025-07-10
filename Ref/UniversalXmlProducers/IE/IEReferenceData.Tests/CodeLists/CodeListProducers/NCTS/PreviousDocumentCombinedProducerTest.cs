using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class PreviousDocumentCombinedProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_PreviousDocumentCombined";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new PreviousDocument();
		protected override IRevenueCodeListDetails GetCombinedCodeListDetails() => new PreviousDocumentExport();
	}
}
