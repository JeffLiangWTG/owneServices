using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business.Testing;

namespace Enterprise.TransportBookings.Business.Test
{
	public class DtbBookingConsolidationJobInvoicingPlugInTest : DtbBookingTestCaseWithFactory
	{
		public void TestIJobInvoicingPlugIn()
		{
			IJobInvoicingPlugIn dummy = Factory.New<DummyWithDtbBooking>();
			IJobInvoicingPlugIn jobInvoicingPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy);

			AssertEquals(dummy.Factory, jobInvoicingPlugIn.Factory);
			AssertEquals(dummy.PK, jobInvoicingPlugIn.PK);
			AssertEquals(dummy.TableName, jobInvoicingPlugIn.TableName);
			AssertEquals(dummy.JobNumber, jobInvoicingPlugIn.JobNumber);
			AssertEquals(false, jobInvoicingPlugIn.IsInDatabase);
			Assert(jobInvoicingPlugIn.AllowInvoiceDeletion);

			Factory.Save();
			AssertEquals(true, jobInvoicingPlugIn.IsInDatabase);

			var supporter = (DtbBookingConsolidationJobInvoicingSupporter)jobInvoicingPlugIn.InvoicingSupporter;
			AssertEquals(jobInvoicingPlugIn, supporter.Parent);
			AssertEquals(dummy.InvoicingSupporter, supporter.ParentSupporter);
		}
	}
}
