using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentInvoicingPlugInTest : TestCaseWithFactory
	{
		#region TestInvoicingSupporter

		public void TestIJobInvoicingPlugIn()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			IJobInvoicingPlugIn invoicingPlugIn = new DtbConsignmentInvoicingPlugIn(consignment);
			AssertEquals(typeof(DtbConsignmentInvoicingSupporter), invoicingPlugIn.InvoicingSupporter.GetType());
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			IJobHeaderParent parent = new DtbConsignmentInvoicingPlugIn(consignment);
			AssertEquals(true, parent.AllowInvoiceDeletion);
			AssertEquals("LTC001", parent.JobNumber);
			AssertNoExceptionThrown(() => parent.OnJobCreating(null));
			AssertNoExceptionThrown(() => parent.OnJobDeleting(null));

			parent.SetJobNumberFieldOnSaving();
			AssertNotEquals("", parent.JobNumber);
		}

		#endregion

		#region IJobHeaderParentCore Members

		public void TestIJobHeaderParentCore()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			IJobHeaderParentCore parentCore = new DtbConsignmentInvoicingPlugIn(consignment);
			AssertEquals(consignment.PK, parentCore.PK);
			AssertEquals(consignment.TableName, parentCore.TableName);
			AssertEquals(false, parentCore.IsInDatabase);
			AssertEquals(consignment.Factory, parentCore.Factory);

			consignment.Factory.Save();
			AssertEquals(true, parentCore.IsInDatabase);
		}

		#endregion

		#region IJobNumber Members

		public void TestIJobNumber()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			IJobNumber jobNumber = new DtbConsignmentInvoicingPlugIn(consignment);
			Factory.Save();

			AssertEquals(false, consignment.LTC_JobID.IsEmpty);
			AssertEquals(consignment.LTC_JobID, jobNumber.JobNumber);
		}

		#endregion

		#region Implementation

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion

		#endregion
	}
}
