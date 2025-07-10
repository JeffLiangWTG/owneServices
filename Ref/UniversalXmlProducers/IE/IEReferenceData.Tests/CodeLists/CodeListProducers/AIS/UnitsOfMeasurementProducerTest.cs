using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class UnitsOfMeasurementProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "AIS.Expected_UnitsOfMeasurement";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new UnitsOfMeasurementDetailsForTesting();
	}
}
