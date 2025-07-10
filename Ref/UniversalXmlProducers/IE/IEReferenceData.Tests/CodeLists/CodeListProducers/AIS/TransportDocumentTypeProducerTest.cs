using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class TransportDocumentTypeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_TransportDocumentTypes";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new TransportDocumentTypeDetails();
	}
}
