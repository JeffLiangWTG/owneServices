using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class TransportDocumentProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_TransportDocument";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new TransportDocument();
	}
}
