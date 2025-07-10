using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BrokerPaymentTypeCodeDescriptionPairListTest : TestCase
	{
		public void TestList()
		{
			BrokerPaymentTypeCodeDescriptionPairList list = new BrokerPaymentTypeCodeDescriptionPairList();
			AssertEquals(7, list.Count);

			AssertEquals(true, list.ContainsCode(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode));
			AssertEquals(true, list.ContainsCode(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter));
			AssertEquals(true, list.ContainsCode(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes));

			AssertEquals(true, list.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			AssertEquals(true, list.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			AssertEquals(true, list.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));
		}
	}
}
