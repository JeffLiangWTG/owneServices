using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobHeaderStatusTest : TestCaseWithFactory
	{
		public void TestWorking()
			=> AssertCodeAndDescription(JobHeaderStatus.Working, "WRK", "Working");

		public void TestWorkOnHold()
			=> AssertCodeAndDescription(JobHeaderStatus.WorkOnHold, "WHL", "Work on Hold");

		public void TestInvoiceOnHold()
			=> AssertCodeAndDescription(JobHeaderStatus.InvoiceOnHold, "IHL", "Invoice On Hold");

		public void TestCustomsProcessActive()
			=> AssertCodeAndDescription(JobHeaderStatus.CustomsProcessActive, "CUS", "Customs Processing Active");

		public void TestJobInvoiced()
			=> AssertCodeAndDescription(JobHeaderStatus.JobInvoiced, "INV", "Job Invoiced");

		public void TestJobReadyForRevenuePosting()
			=> AssertCodeAndDescription(JobHeaderStatus.JobReadyForRevenuePosting, "JRB", "Job Ready for Revenue Posting");

		public void TestJobReadyForCostPosting()
			=> AssertCodeAndDescription(JobHeaderStatus.JobReadyForCostPosting, "JRC", "Job Ready for Cost Posting");

		public void TestJobReadyForRevenueAndCostPosting()
			=> AssertCodeAndDescription(JobHeaderStatus.JobReadyForRevenueAndCostPosting, "JRA", "Job Ready for Revenue and Cost Posting");

		public void TestJobReadyForDelivery()
			=> AssertCodeAndDescription(JobHeaderStatus.JobReadyForDelivery, "RDD", "Job Ready for Delivery");

		public void TestComplete()
			=> AssertCodeAndDescription(JobHeaderStatus.Complete, "CMP", "Complete");

		public void TestClosed()
			=> AssertCodeAndDescription(JobHeaderStatus.Closed, "CLS", "Closed");

		public void TestScheduledForArchive()
			=> AssertCodeAndDescription(JobHeaderStatus.ScheduledForArchive, "ARC", "Schedule for Archive");

		public void TestJobReadyForFinancialClosure()
			=> AssertCodeAndDescription(JobHeaderStatus.JobReadyForFinancialClosure, "JFC", "Job Ready for Financial Closure");

		void AssertCodeAndDescription(JobHeaderStatus jobHeaderStatus, string code, string description)
		{
			AssertEquals(code, jobHeaderStatus.Code);
			AssertEquals(description, jobHeaderStatus.Description);
		}

		public void TestJobHeaderStatusCodes()
		{
			AssertEquals("WRK", JobHeaderStatus.Codes.Working);
			AssertEquals("WHL", JobHeaderStatus.Codes.WorkOnHold);
			AssertEquals("IHL", JobHeaderStatus.Codes.InvoiceOnHold);
			AssertEquals("CUS", JobHeaderStatus.Codes.CustomsProcessActive);
			AssertEquals("INV", JobHeaderStatus.Codes.JobInvoiced);
			AssertEquals("JRB", JobHeaderStatus.Codes.JobReadyForRevenuePosting);
			AssertEquals("JRC", JobHeaderStatus.Codes.JobReadyForCostPosting);
			AssertEquals("JRA", JobHeaderStatus.Codes.JobReadyForRevenueAndCostPosting);
			AssertEquals("RDD", JobHeaderStatus.Codes.JobReadyForDelivery);
			AssertEquals("CMP", JobHeaderStatus.Codes.Complete);
			AssertEquals("CLS", JobHeaderStatus.Codes.Closed);
			AssertEquals("ARC", JobHeaderStatus.Codes.ScheduledForArchive);
			AssertEquals("JFC", JobHeaderStatus.Codes.JobReadyForFinancialClosure);
		}
	}
}
