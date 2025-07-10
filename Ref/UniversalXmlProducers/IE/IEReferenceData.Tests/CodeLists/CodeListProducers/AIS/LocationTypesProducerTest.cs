using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class LocationTypesProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_LocationTypes";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new LocationTypesDetails();
	}
}
