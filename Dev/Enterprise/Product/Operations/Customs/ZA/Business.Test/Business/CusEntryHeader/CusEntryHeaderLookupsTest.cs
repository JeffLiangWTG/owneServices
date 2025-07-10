using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentMethodCodeList()
		{
			CombineAssertions(() =>
			{
				var entryHeader = Factory.New<CusEntryHeader>();
				var testList = entryHeader.Lookups.PaymentMethodCodeList;
				AssertNotNull(testList);
				AssertEquals("Count", 4, testList.Count);
				Assert("Code C", testList.ContainsCode("C"));
				Assert("Code D", testList.ContainsCode("D"));
				Assert("Code V", testList.ContainsCode("V"));
				Assert("Code F", testList.ContainsCode("F"));
			});
		}

		public void TestCH_PaymentMethodCodeList()
		{
			var testHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var lookup = testHeader.Lookups;
			var testList = lookup.CH_PaymentMethodCodeList;
			AssertNotNull(testList);
			AssertEquals(4, testList.Count);
			Assert("Code C", testList.ContainsCode("C"));
			Assert("Code D", testList.ContainsCode("D"));
			Assert("Code F", testList.ContainsCode("F"));
			Assert("Code V", testList.ContainsCode("V"));
		}
	}
}
