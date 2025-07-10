using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class SupportingDocumentTypeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_SupportingDocumentType";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new SupportingDocumentType();
	}
}
