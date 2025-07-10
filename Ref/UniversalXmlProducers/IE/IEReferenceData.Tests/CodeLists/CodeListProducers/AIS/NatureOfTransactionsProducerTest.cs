using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class NatureOfTransactionsProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_NatureOfTransactions";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new NatureOfTransactionsDetails();
	}
}
