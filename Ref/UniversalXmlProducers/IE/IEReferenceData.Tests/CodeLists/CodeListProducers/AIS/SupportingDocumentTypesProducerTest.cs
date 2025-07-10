using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class SupportingDocumentTypesProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_SupportingDocumentTypes";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new SupportingDocumentTypesDetails();
	}
}
