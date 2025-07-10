using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ManifestToOpenPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWarehouseCodeList()
		{
			var yesterday = ZDateTime.Now.AddDays(-1);
			var tomorrow = ZDateTime.Now.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TRCWH", "Turkey Warehouse Codes");
			helper.CreateCusCodeList("TR", "TRCWH", "K61000008", yesterday, tomorrow);
			helper.CreateCusCodeList("TR", "TRCWH", "K65000006", yesterday, tomorrow);
			Factory.Save();

			var pack = Factory.New<ManifestToOpenPack>();
			var lookups = pack.Lookups;
			lookups.WarehouseCodeList.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "K61000008", "K65000006" }, lookups.WarehouseCodeList.Select(x => x.ZZD_Code));
		}
	}
}
