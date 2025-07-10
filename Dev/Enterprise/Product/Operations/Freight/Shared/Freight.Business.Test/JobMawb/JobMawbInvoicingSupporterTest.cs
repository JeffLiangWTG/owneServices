using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobMawbInvoicingSupporter))]
	sealed class JobMawbInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			JobMawb jobMawb = Factory.NewWithValidTestData<JobMawb>();
			return jobMawb;
		}
	}
}
