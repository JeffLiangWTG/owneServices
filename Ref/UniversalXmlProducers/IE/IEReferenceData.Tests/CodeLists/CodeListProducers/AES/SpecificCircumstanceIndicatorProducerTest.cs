using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class SpecificCircumstanceIndicatorProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_SpecificCircumstanceIndicator";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new SpecificCircumstanceIndicator();
	}
}
