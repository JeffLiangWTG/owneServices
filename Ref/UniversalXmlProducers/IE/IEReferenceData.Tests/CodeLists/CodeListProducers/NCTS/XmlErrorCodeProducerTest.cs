using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class XmlErrorCodeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_XmlErrorCodesCode";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new XmlErrorCodesCode();
	}
}
