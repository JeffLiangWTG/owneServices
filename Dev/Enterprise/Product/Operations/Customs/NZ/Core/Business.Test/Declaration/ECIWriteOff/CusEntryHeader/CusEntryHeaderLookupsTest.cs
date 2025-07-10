namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Testing
{
	using CargoWise.EntityFramework.Testing;

	class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCH_EntryStatusList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertSame(Factory.GetCachedValue<LowValueManifestStatusList>(), entryHeader.Lookups.CH_EntryStatusList);
		}
	}
}
