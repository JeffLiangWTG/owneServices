using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusEntryPayInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestTransactionTypeList()
		{
			var parent = Factory.New<CusEntryPayInfo>();
			var transactionTypeList = parent.Lookups.TransactionTypeList;
			NUnit.Framework.Assert.That(transactionTypeList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<EntryChargeTypeList>()));
		}

		[ExpectNoExceptions]
		public void TestReasonOfPaymentList()
		{
			var parent = Factory.New<CusEntryPayInfo>();
			var reasonOfPaymentList = parent.Lookups.ReasonOfPaymentList;
			NUnit.Framework.Assert.That(reasonOfPaymentList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<ReasonOfPaymentList>()));
		}
	}
}
