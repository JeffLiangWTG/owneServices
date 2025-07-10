using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsManifestsToOpenLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTRWarehouseList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TRCWH", "TRCWH");

			helper.CreateCusCodeList("TR", "TRCWH", "A0002", "EKOL ULUSŞLARARASI TİC. A.Ş.", yesterday, tomorrow);
			helper.CreateCusCodeList("TR", "TRCWH", "A0004", "İNT.İNTERNAS NAK.TURİZM A.Ş.", yesterday, tomorrow);

			Factory.Save();
			var header = Factory.NewWithValidTestData<NctsManifestsToOpen>();
			var list = header.Lookups.TRWarehouseList;
			list.Load();
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A0002"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A0004"));
		}
		public void TestLookups()
		{
			var number = Factory.New<NctsManifestsToOpen>();
			AssertEquals("Visited Port Lookups", typeof(NctsManifestsToOpenLookups), number.Lookups.GetType());
		}
	}
}
