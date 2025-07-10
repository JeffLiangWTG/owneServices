using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class UNDangerousGoodsCodeProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_UNDangerousGoodsCode";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new UNDangerousGoodsCode();
	}
}
