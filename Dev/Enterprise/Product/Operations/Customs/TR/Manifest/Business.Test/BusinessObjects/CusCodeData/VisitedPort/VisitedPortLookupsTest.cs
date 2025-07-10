using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public partial class VisitedPortLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVisitedPortList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("PORT", "Port");
			helper.CreateCusCodeList("TR", "PORT", "TRMER-001", yesterday, tomorrow);
			helper.CreateCusCodeList("TR", "PORT", "DEHAM", yesterday, tomorrow);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.CustomsLoadingPortList as BusinessObjectCollection;
			list.Load();
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "TRMER-001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "DEHAM"));
		}

		public void TestLookups()
		{
			var number = Factory.New<VisitedPort>();
			AssertEquals("Visited Port Lookups", typeof(VisitedPortLookups), number.Lookups.GetType());
		}
	}
}
