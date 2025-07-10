using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingConsolidationJobInvoicingPlugIn : IJobInvoicingPlugIn, IBusinesssObjectProviderForDocumentWrapper
	{
		public DtbBookingConsolidationJobInvoicingPlugIn(IJobInvoicingPlugIn parent, bool supporterShouldUseParent = false)
		{
			Argument.NotNull(parent, "parent");

			Parent = parent;
			SupporterShouldUseParent = supporterShouldUseParent;
		}

#if DEBUG
		public
#endif
		readonly IJobInvoicingPlugIn Parent;
		readonly bool SupporterShouldUseParent;

		public List<DtbBooking> Bookings
		{
			get { return bookings ?? (bookings = new List<DtbBooking>()); }
		}

		List<DtbBooking> bookings;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new DtbBookingConsolidationJobInvoicingSupporter(this, Parent.InvoicingSupporter, shouldUseParentSupporter: SupporterShouldUseParent)); }
		}

		string IJobNumber.JobNumber
		{
			get { return Parent.JobNumber; }
		}

		IJobInvoicingSupporter invoicingSupporter;

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return Parent.Factory; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			Parent.OnJobCreating(job);
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

		ZGuid IJobHeaderParentCore.PK
		{
			get { return Parent.PK; }
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			Parent.SetJobNumberFieldOnSaving();
		}

		string IJobHeaderParentCore.TableName
		{
			get { return Parent.TableName; }
		}

		bool IJobHeaderParentCore.IsInDatabase
		{
			get { return Parent.IsInDatabase; }
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		bool IJobHeaderParent.IsDeleted
		{
			get { return Parent.IsDeleted; }
		}

		BusinessObject IBusinesssObjectProviderForDocumentWrapper.BusinessObjectForDocumentWrapper => Parent as BusinessObject;
	}
}
