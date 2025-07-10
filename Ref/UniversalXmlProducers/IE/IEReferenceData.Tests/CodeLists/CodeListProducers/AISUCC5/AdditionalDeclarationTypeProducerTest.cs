using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.AISUCC5.Tests
{
	class AdditionalDeclarationTypeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string DataGrouping => Constants.DataGroupings.IEUCC5;

		protected override string ExpectedTestFileName => "AISUCC5.Expected_AdditionalDeclarationTypes";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalDeclarationTypesDetails();
	}
}
