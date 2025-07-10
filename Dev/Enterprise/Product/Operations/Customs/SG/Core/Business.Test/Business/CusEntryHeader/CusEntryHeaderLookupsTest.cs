using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeader()
		{
			CusEntryHeader parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Lookups.EntryHeader, parent);
		}
	}
}
