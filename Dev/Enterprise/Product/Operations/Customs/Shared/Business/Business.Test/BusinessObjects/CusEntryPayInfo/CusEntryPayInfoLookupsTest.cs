using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryPayInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransactionTypeList()
		{
			var header = Factory.New<CusEntryPayInfo>();
			AssertEquals(0, header.Lookups.TransactionTypeList.Count);
		}

		public void TestPaymentPartyList()
		{
			var header = Factory.New<CusEntryPayInfo>();
			AssertEquals(Factory.GetCachedValue<PaidByCodeList>(), header.Lookups.PaymentPartyList);
		}

		public void TestPaymentStatusList()
		{
			var header = Factory.New<CusEntryPayInfo>();
			AssertEquals(Factory.GetCachedValue<CusEntryPayInfoStatusList>(), header.Lookups.PaymentStatusList);
		}
	}
}
