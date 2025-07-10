using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.Tests.CodeLists.CodeListProducers.AIS
{
	class CountryCodeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_CountryCodes";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new CountryCodeDetails();
	}
}
