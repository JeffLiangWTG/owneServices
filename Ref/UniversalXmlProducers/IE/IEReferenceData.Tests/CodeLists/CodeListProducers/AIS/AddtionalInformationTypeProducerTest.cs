using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	sealed class AddtionalInformationTypeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "AIS.Expected_AddtionalInformation";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalInformation();
	}
}
