using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCH_MessageTypeList()
		{
			var header = Factory.New<CusEntryHeader>();
			AssertEquals(Factory.GetCachedValue<JobMessageTypeList>(), header.Lookups.CH_MessageTypeList);
		}

		public void TestWarehouseTransactionStatusList()
		{
			var header = Factory.New<CusEntryHeader>();
			AssertEquals(Factory.GetCachedValue<WarehouseTransactionStatusList>(), header.Lookups.WarehouseTransactionStatusList);
		}

		public void TestCH_EntryStatusList()
		{
			var header = Factory.New<CusEntryHeader>();
			AssertEquals(0, header.Lookups.CH_EntryStatusList.Count);
			var dec = Factory.New<BaseJobDeclaration>();
			header.CH_JE = dec.PK;
			var list = dec.Lookups.EntryStatusList;
			var entryStatusList = header.Lookups.CH_EntryStatusList;
			AssertEquals(list.Count, entryStatusList.Count);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Description, entryStatusList.GetDescriptionFromCode(pair.Code));
			}
		}

		public void TestMessageStatusList()
		{
			var header = Factory.New<CusEntryHeader>();
			AssertEquals(0, header.Lookups.MessageStatusList.Count);
			var dec = Factory.New<BaseJobDeclaration>();
			header.CH_JE = dec.PK;
			var list = dec.Lookups.MessageStatusList;
			var messageStatusList = header.Lookups.MessageStatusList;
			AssertEquals(list.Count, messageStatusList.Count);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Description, messageStatusList.GetDescriptionFromCode(pair.Code));
			}
		}
	}
}
