using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class TransportChargesMethodOfPaymentProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_TransportChargesMethodOfPayment";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new TransportChargesMethodOfPayment();
	}
}
