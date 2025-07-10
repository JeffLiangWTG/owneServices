using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInvoicingPlugIn : IJobInvoicingPlugIn
	{
		public DtbBookingInvoicingPlugIn(DtbBooking booking)
		{
			Booking = booking;
		}

		readonly DtbBooking Booking;

		DtbBookingInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new DtbBookingInvoicingSupporter(Booking);
		}

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = GetNewInvoicingSupporter()); }
		}

		IJobInvoicingSupporter invoicingSupporter;

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			Booking.PopulateUniqueIDIfNeeded();
		}

		bool IJobHeaderParent.IsDeleted
		{
			get { return Booking.IsDeleted; }
		}

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return Booking.Factory; }
		}

		ZGuid IJobHeaderParentCore.PK
		{
			get { return Booking.PK; }
		}

		string IJobHeaderParentCore.TableName
		{
			get { return Booking.TableName; }
		}

		bool IJobHeaderParentCore.IsInDatabase
		{
			get { return Booking.IsInDatabase; }
		}

		string IJobNumber.JobNumber
		{
			get { return Booking.KM_JobID; }
		}
	}
}
