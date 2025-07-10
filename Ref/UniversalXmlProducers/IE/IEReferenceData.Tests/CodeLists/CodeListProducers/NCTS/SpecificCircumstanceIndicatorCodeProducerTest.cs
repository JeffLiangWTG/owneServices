using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class SpecificCircumstanceIndicatorCodeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_SpecificCircumstanceIndicatorCode";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new SpecificCircumstanceIndicatorCode();
	}
}
