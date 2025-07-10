using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	sealed class AdditionalReferenceTypeNationalProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "AIS.Expected_AdditionalReferenceNational";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalReferenceNational();
	}
}
