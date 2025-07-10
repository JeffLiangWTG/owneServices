using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectInvoicingSupporter))]
	public class ProjectInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestConsumerType()
		{
			var project = Factory.New<Project>();
			IJobInvoicingPlugIn job = project;
			AssertEquals(JobInvoicingConsumerTypes.Project, job.InvoicingSupporter.ConsumerType);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var project = Factory.New<Project>();
			return project;
		}

		public void TestClient()
		{
			var project = Factory.New<Project>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			project.WKP_OA_ClientAddress = org.MainAddress.PK;
			AssertEquals("Client", org, ((IJobInvoicingPlugIn)project).InvoicingSupporter.Consignee);
			AssertEquals("Client", org, ((IJobInvoicingPlugIn)project).InvoicingSupporter.Consignor);
		}
	}
}
