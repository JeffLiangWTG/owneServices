using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class LocationIdentificationQualifiersProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_LocationIdentificationQualifiers";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new LocationIdentificationQualifiersDetails();
	}
}
