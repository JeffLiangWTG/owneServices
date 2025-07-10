using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequestInvoicingSupporter))]
	public class WorkRequestInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestConsumerType()
		{
			var request = Factory.New<WorkRequest>();
			IJobInvoicingPlugIn job = request;
			AssertEquals(JobInvoicingConsumerTypes.WorkRequest, job.InvoicingSupporter.ConsumerType);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var request = Factory.New<WorkRequest>();
			return request;
		}
	}
}
