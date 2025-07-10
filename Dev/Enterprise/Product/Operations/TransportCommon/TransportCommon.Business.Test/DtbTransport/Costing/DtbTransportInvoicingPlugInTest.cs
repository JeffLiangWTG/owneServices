using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInvoicingPlugInTest : TestCaseWithFactory
	{
		#region IJobInvoicingPlugIn Members

		public void TestIJobInvoicingPlugIn()
		{
			var transport = GetNewDtbTransportJob();
			IJobInvoicingPlugIn invoicingPlugIn = GetNewDtbTransportInvoicingPlugIn(transport);
			AssertEquals(ExpectedInvoicingSupporterType, invoicingPlugIn.InvoicingSupporter.GetType());
		}

		protected abstract Type ExpectedInvoicingSupporterType { get; }

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent()
		{
			var transport = GetNewDtbTransportJob();
			IJobHeaderParent parent = GetNewDtbTransportInvoicingPlugIn(transport);
			AssertEquals(true, parent.AllowInvoiceDeletion);
			AssertEquals("", parent.JobNumber);
			AssertNoExceptionThrown(() => parent.OnJobCreating(null));
			AssertNoExceptionThrown(() => parent.OnJobDeleting(null));

			parent.SetJobNumberFieldOnSaving();
			AssertNotEquals("", parent.JobNumber);
		}

		#endregion

		#region IJobHeaderParentCore Members

		public void TestIJobHeaderParentCore()
		{
			var transport = GetNewDtbTransportJob();
			IJobHeaderParentCore parentCore = GetNewDtbTransportInvoicingPlugIn(transport);
			AssertEquals(transport.PK, parentCore.PK);
			AssertEquals(transport.TableName, parentCore.TableName);
			AssertEquals(false, parentCore.IsInDatabase);
			AssertEquals(transport.Factory, parentCore.Factory);

			Factory.Save();
			AssertEquals(true, parentCore.IsInDatabase);
		}

		#endregion

		#region IJobNumber Members

		public void TestIJobNumber()
		{
			var transport = GetNewDtbTransportJob();
			IJobNumber jobNumber = GetNewDtbTransportInvoicingPlugIn(transport);
			Factory.Save();
			AssertEquals(false, transport.KM_JobID.IsEmpty);
			AssertEquals(transport.KM_JobID, jobNumber.JobNumber);
		}

		#endregion

		#region Implementation

		protected abstract DtbTransport GetNewDtbTransportJob();
		protected abstract DtbTransportInvoicingPlugIn GetNewDtbTransportInvoicingPlugIn(DtbTransport transport);

		#endregion
	}
}
