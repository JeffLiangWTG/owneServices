using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLineFee()
		{
			CusEntryLineFee parent = Factory.New<CusEntryLineFee>();
			AssertEquals(parent.Lookups.EntryLineFee, parent);
		}
	}
}
