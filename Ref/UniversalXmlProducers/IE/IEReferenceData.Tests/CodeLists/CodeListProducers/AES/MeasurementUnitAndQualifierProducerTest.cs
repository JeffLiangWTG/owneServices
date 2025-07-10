using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class MeasurementUnitAndQualifierProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_MeasurementUnitAndQualifier";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new MeasurementUnitAndQualifier();
	}
}
