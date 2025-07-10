using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PaymentTypeListTest : TestCaseWithFactory
	{
		public void TestIsPaidByBroker()
		{
			AssertEquals(true, PaymentTypeList.IsPaidByBroker(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			AssertEquals(false, PaymentTypeList.IsPaidByBroker(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			AssertEquals(false, PaymentTypeList.IsPaidByBroker(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));
			AssertEquals(true, PaymentTypeList.IsPaidByBroker(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode));
			AssertEquals(false, PaymentTypeList.IsPaidByBroker(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter));
			AssertEquals(false, PaymentTypeList.IsPaidByBroker(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes));
			AssertEquals(false, PaymentTypeList.IsPaidByBroker(PaymentTypeList.Codes.IndividualBasis));
		}

		public void TestIsPaidByImporter()
		{
			AssertEquals(false, PaymentTypeList.IsPaidByImporter(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			AssertEquals(true, PaymentTypeList.IsPaidByImporter(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			AssertEquals(true, PaymentTypeList.IsPaidByImporter(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));
			AssertEquals(false, PaymentTypeList.IsPaidByImporter(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode));
			AssertEquals(true, PaymentTypeList.IsPaidByImporter(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter));
			AssertEquals(true, PaymentTypeList.IsPaidByImporter(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes));
			AssertEquals(false, PaymentTypeList.IsPaidByImporter(PaymentTypeList.Codes.IndividualBasis));
		}

		public void TestIsPeriodicPayment()
		{
			AssertEquals(true, PaymentTypeList.IsPeriodicPayment(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			AssertEquals(true, PaymentTypeList.IsPeriodicPayment(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			AssertEquals(true, PaymentTypeList.IsPeriodicPayment(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));
			AssertEquals(false, PaymentTypeList.IsPeriodicPayment(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode));
			AssertEquals(false, PaymentTypeList.IsPeriodicPayment(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter));
			AssertEquals(false, PaymentTypeList.IsPeriodicPayment(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes));
			AssertEquals(false, PaymentTypeList.IsPeriodicPayment(PaymentTypeList.Codes.IndividualBasis));
		}

		public void TestIsDailyPayment()
		{
			AssertEquals(false, PaymentTypeList.IsDailyPayment(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			AssertEquals(false, PaymentTypeList.IsDailyPayment(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			AssertEquals(false, PaymentTypeList.IsDailyPayment(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));
			AssertEquals(true, PaymentTypeList.IsDailyPayment(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode));
			AssertEquals(true, PaymentTypeList.IsDailyPayment(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter));
			AssertEquals(true, PaymentTypeList.IsDailyPayment(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes));
			AssertEquals(false, PaymentTypeList.IsDailyPayment(PaymentTypeList.Codes.IndividualBasis));
		}

		public void TestIsPaymentTypeRequiredAccountNo()
		{
			AssertEquals(false, PaymentTypeList.DoesPaymentTypeRequireAccountNo(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			AssertEquals(true, PaymentTypeList.DoesPaymentTypeRequireAccountNo(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			AssertEquals(false, PaymentTypeList.DoesPaymentTypeRequireAccountNo(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));
			AssertEquals(false, PaymentTypeList.DoesPaymentTypeRequireAccountNo(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode));
			AssertEquals(true, PaymentTypeList.DoesPaymentTypeRequireAccountNo(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter));
			AssertEquals(false, PaymentTypeList.DoesPaymentTypeRequireAccountNo(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes));
			AssertEquals(false, PaymentTypeList.DoesPaymentTypeRequireAccountNo(PaymentTypeList.Codes.IndividualBasis));
		}

		public void TestGetCachedReconPaymentTypeList()
		{
			PaymentTypeList list1 = PaymentTypeList.GetCachedReconPaymentTypeList(Factory);
			PaymentTypeList list2 = PaymentTypeList.GetCachedReconPaymentTypeList(Factory);
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			PaymentTypeList list3 = PaymentTypeList.GetCachedReconPaymentTypeList(new BusinessObjectFactory());
			AssertEquals(false, object.ReferenceEquals(list1, list3));

			Assert(!list1.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			Assert(!list1.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			Assert(!list1.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));

			AssertEquals("sorted", PaymentTypeList.Codes.IndividualBasis, list3[0].Code);
		}

		public void TestGetNewReconPaymentTypeList()
		{
			PaymentTypeList list = PaymentTypeList.GetCachedReconPaymentTypeList(Factory);

			Assert(!list.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			Assert(!list.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			Assert(!list.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));

			AssertEquals("sorted", PaymentTypeList.Codes.IndividualBasis, list[0].Code);
		}
	}
}
