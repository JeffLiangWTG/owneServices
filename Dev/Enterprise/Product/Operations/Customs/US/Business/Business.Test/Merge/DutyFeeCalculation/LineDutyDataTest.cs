using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryLineDutyDataTest : TestCaseWithFactory
	{
		public void TestSpecialProgramsIndicatorSecondary()
		{
			var mock = new Mock<IEntryLineOrInvoiceLineDutyData>();
			mock.Setup(m => m.SpecialProgramsIndicatorSecondary).Returns("X");
			mock.Setup(m => m.CustomsValue).Returns(0m);
			LineDutyData dutyData = new LineDutyData(mock.Object, 200m);
			AssertEquals("X", dutyData.SpecialProgramsIndicatorSecondary);

			mock.Setup(m => m.SpecialProgramsIndicatorSecondary).Returns("Y");
			AssertEquals("Y", dutyData.SpecialProgramsIndicatorSecondary);
			AssertEquals(200m, dutyData.CustomsValue);
		}

		public void TestCalculateException()
		{
			JobComInvoiceLine invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			invoiceLine.TariffCalculateExceptionMessage = "Test error1";
			DDPDisbursementDutyDataBase dutyDatyBase = new DDPDisbursementLineDutyData(invoiceLine, null, null, true);
			LineDutyData dutyData = new LineDutyData(dutyDatyBase, 200m);
			AssertEquals("Test error1", ((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException);

			((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException = "Test error2";
			AssertEquals("Test error2", ((IEntryLineOrInvoiceLineDutyData)dutyDatyBase).CalculateException);
		}
	}
}
