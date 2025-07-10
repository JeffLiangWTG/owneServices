using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class LocationOfGoodsProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_LocationOfGoods";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new LocationOfGoodsDetails();
	}
}
