using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class FunctionalErrorCodeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_FunctionalErrorCode";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new FunctionalErrorCode();
	}
}
