using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportBookings.Business.Testing
{
	class DtbBookingInvoicingPlugInTest : TestCaseWithFactory
	{
		Type ExpectedInvoicingSupporterType
		{
			get { return typeof(DtbBookingInvoicingSupporter); }
		}

		public void TestIJobInvoicingPlugIn()
		{
			var booking = GetNewDtbBookingJob();
			IJobInvoicingPlugIn invoicingPlugIn = GetNewDtbBookingInvoicingPlugIn(booking);
			AssertEquals(ExpectedInvoicingSupporterType, invoicingPlugIn.InvoicingSupporter.GetType());
		}

		public void TestIJobHeaderParent()
		{
			var booking = GetNewDtbBookingJob();
			IJobHeaderParent parent = GetNewDtbBookingInvoicingPlugIn(booking);
			AssertEquals(true, parent.AllowInvoiceDeletion);
			AssertEquals("", parent.JobNumber);
			AssertNoExceptionThrown(() => parent.OnJobCreating(null));
			AssertNoExceptionThrown(() => parent.OnJobDeleting(null));

			parent.SetJobNumberFieldOnSaving();
			AssertNotEquals("", parent.JobNumber);
		}

		public void TestIJobHeaderParentCore()
		{
			var booking = GetNewDtbBookingJob();
			IJobHeaderParentCore parentCore = GetNewDtbBookingInvoicingPlugIn(booking);
			AssertEquals(booking.PK, parentCore.PK);
			AssertEquals(booking.TableName, parentCore.TableName);
			AssertEquals(false, parentCore.IsInDatabase);
			AssertEquals(booking.Factory, parentCore.Factory);

			Factory.Save();
			AssertEquals(true, parentCore.IsInDatabase);
		}

		public void TestIJobNumber()
		{
			var booking = GetNewDtbBookingJob();
			IJobNumber jobNumber = GetNewDtbBookingInvoicingPlugIn(booking);
			Factory.Save();
			AssertEquals(false, booking.KM_JobID.IsEmpty);
			AssertEquals(booking.KM_JobID, jobNumber.JobNumber);
		}

		DtbBookingInvoicingPlugIn GetNewDtbBookingInvoicingPlugIn(DtbBooking booking)
		{
			return new DtbBookingInvoicingPlugIn(booking);
		}

		DtbBooking GetNewDtbBookingJob()
		{
			return Helper.CreateBooking();
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
