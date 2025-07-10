using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobHeaderStatusListTest : TestCaseWithFactory
	{
		public void TestJobHeaderStatusList()
		{
			var jobHeaderStatusList = new JobHeaderStatusList();
			AssertEquals(13, jobHeaderStatusList.Count);
			AssertEquals(JobHeaderStatus.Working, jobHeaderStatusList[0]);
			AssertEquals(JobHeaderStatus.WorkOnHold, jobHeaderStatusList[1]);
			AssertEquals(JobHeaderStatus.InvoiceOnHold, jobHeaderStatusList[2]);
			AssertEquals(JobHeaderStatus.CustomsProcessActive, jobHeaderStatusList[3]);
			AssertEquals(JobHeaderStatus.JobReadyForRevenuePosting, jobHeaderStatusList[4]);
			AssertEquals(JobHeaderStatus.JobReadyForCostPosting, jobHeaderStatusList[5]);
			AssertEquals(JobHeaderStatus.JobReadyForRevenueAndCostPosting, jobHeaderStatusList[6]);
			AssertEquals(JobHeaderStatus.JobInvoiced, jobHeaderStatusList[7]);
			AssertEquals(JobHeaderStatus.JobReadyForDelivery, jobHeaderStatusList[8]);
			AssertEquals(JobHeaderStatus.Complete, jobHeaderStatusList[9]);
			AssertEquals(JobHeaderStatus.JobReadyForFinancialClosure, jobHeaderStatusList[10]);
			AssertEquals(JobHeaderStatus.Closed, jobHeaderStatusList[11]);
			AssertEquals(JobHeaderStatus.ScheduledForArchive, jobHeaderStatusList[12]);
		}
	}
}
