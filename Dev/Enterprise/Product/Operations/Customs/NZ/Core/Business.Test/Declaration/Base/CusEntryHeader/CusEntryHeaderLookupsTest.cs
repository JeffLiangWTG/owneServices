using Enterprise.Customs.NZ.TradeSingleWindow;
namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTSWMessageStatusList()
		{
			AssertSame(Factory.GetCachedValue<StatusList>(), header.Lookups.TSWMessageStatusList);
		}

		public void TestMessageModeList()
		{
			AssertSame(Factory.GetCachedValue<JobApplicationCodeList>(), header.Lookups.MessageModeList);
		}

		public void TestCH_MessageTypeList()
		{
			AssertSame(Factory.GetCachedValue<EntryMessageTypeList>(), header.Lookups.CH_MessageTypeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusEntryHeader>();
		}
		CusEntryHeader header;
	}
}
