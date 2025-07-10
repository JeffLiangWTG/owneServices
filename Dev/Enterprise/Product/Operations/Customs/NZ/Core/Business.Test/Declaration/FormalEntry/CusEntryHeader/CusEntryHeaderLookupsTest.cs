namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry.Testing
{
	using CargoWise.EntityFramework.Testing;

	class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCH_EntryStatusList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertSame(Factory.GetCachedValue<FormalEntryStatusList>(), entryHeader.Lookups.CH_EntryStatusList);
		}
	}
}
