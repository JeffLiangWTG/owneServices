using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class AuthorisationCodeTypeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_AuthorisationCodeTypes";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AuthorisationCodeTypesDetails();
	}
}
