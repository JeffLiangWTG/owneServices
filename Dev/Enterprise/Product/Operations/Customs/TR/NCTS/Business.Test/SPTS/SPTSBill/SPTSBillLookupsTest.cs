using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class SPTSBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeclarationTypeList()
		{
			var bill = Factory.New<SPTSBill>();
			var lookups = bill.Lookups;

			AssertEquals("H, HD, HT, K, T, TD, TR, X, XD", lookups.DeclarationTypeList.CodesAsString);
		}

		public void TestStatusList()
		{
			var bill = Factory.New<SPTSBill>();
			var lookups = bill.Lookups;

			AssertEquals("N, Y", lookups.YesNoList.CodesAsString);
		}
	}
}
