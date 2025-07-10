using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class PreviousDocumentExportProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_PreviousDocumentExport";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new PreviousDocumentExport();
	}
}
