namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusDecHouseBillLookupsTest : Customs.Business.Testing.CusDecHouseBillLookupsTest
	{
		public void TestHouseBill()
		{
			Bill parent = Factory.New<Bill>();
			AssertEquals(parent.Lookups.HouseBill, parent);
		}
	}
}
