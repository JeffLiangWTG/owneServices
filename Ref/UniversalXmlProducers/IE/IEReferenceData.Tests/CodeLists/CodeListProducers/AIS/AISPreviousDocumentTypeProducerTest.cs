using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class AISPreviousDocumentTypeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_AISPreviousDocumentTypes";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new PreviousDocumentTypesDetails();
	}
}
