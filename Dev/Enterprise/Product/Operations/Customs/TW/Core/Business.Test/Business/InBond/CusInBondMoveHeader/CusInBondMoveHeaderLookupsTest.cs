using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondMoveHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryTypeList()
		{
			var entryTypeList = Lookups.EntryTypeList;
			CombineAssertions(() =>
			{
				AssertSame(Factory.GetCachedValue<EntryTypeList>(), entryTypeList);
				AssertEquals("The CodeAsString of EntryTypeList must be", "T1, T2, T4, T5, T6", entryTypeList.CodesAsString);
			});
		}

		public void TestTranshipmentTransportCodeList()
		{
			var transhipmentTransportCodeList = Lookups.TranshipmentTransportCodeList;
			CombineAssertions(() =>
			{
				AssertSame(Factory.GetCachedValue<TranshipmentTransportCodeList>(), transhipmentTransportCodeList);
				AssertEquals("The CodeAsString of TranshipmentTransportCodeList must be", "11, 12, 13, 14, 15, 16, 41, 42, 43, 44, 51, 52, 70, 71, 72, 99", transhipmentTransportCodeList.CodesAsString);
			});
		}

		CusInBondMoveHeaderLookups Lookups => new CusInBondMoveHeaderLookups(CusInBondMoveHeader);
		CusInBondMoveHeader CusInBondMoveHeader => fCusInBondMoveHeader ?? (fCusInBondMoveHeader = Factory.New<CusInBondMoveHeader>());
		CusInBondMoveHeader fCusInBondMoveHeader;
	}
}
