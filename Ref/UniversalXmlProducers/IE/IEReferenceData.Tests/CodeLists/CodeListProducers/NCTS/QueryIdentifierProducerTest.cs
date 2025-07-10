using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class QueryIdentifierProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_QueryIdentifier";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new QueryIdentifier();
	}
}
