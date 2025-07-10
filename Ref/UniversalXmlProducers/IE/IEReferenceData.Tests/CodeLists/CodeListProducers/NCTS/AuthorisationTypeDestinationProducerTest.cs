using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class AuthorisationTypeDestinationProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_AuthorisationTypeDestination";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new AuthorisationTypeDestination();
	}
}
