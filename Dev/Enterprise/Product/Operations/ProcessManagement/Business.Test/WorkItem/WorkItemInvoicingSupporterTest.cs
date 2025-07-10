using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemInvoicingSupporter))]
	class WorkItemInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestConsumerType()
		{
			var workItem = Factory.New<WorkItem>();
			IJobInvoicingPlugIn testJob = workItem;
			AssertEquals(JobInvoicingConsumerTypes.WorkItem, testJob.InvoicingSupporter.ConsumerType);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var workItem = Factory.New<WorkItem>();
			return workItem;
		}
	}
}
