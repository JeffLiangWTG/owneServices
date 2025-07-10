using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class AuthorisationTypeDepartureProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_AuthorisationTypeDeparture";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new AuthorisationTypeDeparture();
	}
}
