using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class TransportChargesProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_TransportCharges";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new TransportCharges();
	}
}
