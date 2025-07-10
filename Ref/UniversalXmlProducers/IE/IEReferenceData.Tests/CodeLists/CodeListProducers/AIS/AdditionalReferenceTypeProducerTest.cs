using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	sealed class AdditionalReferenceTypeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "AIS.Expected_AdditionalReference";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalReference();
	}
}
