using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class DDPDisbursementDutyDataBaseTest : TestCaseWithFactory
	{
		public void TestCalculateException()
		{
			JobComInvoiceLine invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			invoiceLine.TariffCalculateExceptionMessage = "Test error1";
			DDPDisbursementDutyDataBase dutyData = GetDDPDisbursementDutyDataForTest(invoiceLine);
			AssertEquals("Test error1", ((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException);

			((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException = "Test error2";
			AssertEquals("Test error2", invoiceLine.TariffCalculateExceptionMessage);
		}

		internal abstract DDPDisbursementDutyDataBase GetDDPDisbursementDutyDataForTest(JobComInvoiceLine invoiceLine);
	}
}
