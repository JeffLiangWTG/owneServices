using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business
{
	[TestedType(typeof(AgencyBookingInvoicingSupporter))]
	internal class AgencyBookingInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestInvoicingSecurityCheckPoints()
		{
			IJobInvoicingPlugIn invoicing = Factory.New<AgencyBooking>();
			AssertEquals("AuditSecurity", Env.Security.AgencyBookingAuditBilling, invoicing.InvoicingSupporter.AuditSecurity);
			AssertEquals("JobInvoicing", Env.Security.AgencyBookingJobInvoicing, invoicing.InvoicingSupporter.JobInvoicingSecurity);
		}

		#region Implementation
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AgencyBooking>();
		}
		#endregion
	}
}
