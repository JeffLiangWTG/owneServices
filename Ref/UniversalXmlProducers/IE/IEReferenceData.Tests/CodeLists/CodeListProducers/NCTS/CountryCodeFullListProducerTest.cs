using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class CountryCodeFullListProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_CountryCodeFullList";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new CountryCodeFullList();
	}
}
