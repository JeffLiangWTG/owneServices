using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ProvisionalPaymentAmountCodeDataLookupTest : TestCaseWithFactory
	{
		public void TestCodeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var entryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var provisionalPayment = entryLine.ProvisionalPayments.AddNew();
			AssertEquals(8, provisionalPayment.Lookups.CY_CodeList.Count);
			AssertEquals(provisionalPayment.Lookups.ProvisionalPaymentTypes, provisionalPayment.Lookups.CY_CodeList);
			dec.JE_MessageType = "EXP";
			AssertEquals(1, provisionalPayment.Lookups.CY_CodeList.Count);
			AssertEquals(provisionalPayment.Lookups.ProvisionalPaymentTypes, provisionalPayment.Lookups.CY_CodeList);
		}
	}
}
