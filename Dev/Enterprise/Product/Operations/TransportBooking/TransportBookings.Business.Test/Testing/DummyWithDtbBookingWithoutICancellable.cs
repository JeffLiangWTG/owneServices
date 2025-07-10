using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.TransportBookings.Business.Testing
{
	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	public class DummyWithDtbBookingWithoutICancellable : ZArchitecture.Business.Testing.DummyEnterpriseBusinessObject, IDtbBookingParent, IJobInvoicingPlugIn, IDocManagerSupport
	{
		public DummyWithDtbBookingWithoutICancellable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		ZString IRelatedJob.JobDescription
		{
			get { return "Dummy 123"; }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return "Dummy Job No."; }
		}

		public ZString JobType
		{
			get { return "DUM"; }
		}

		public ZString JobTypeDescription
		{
			get { return "Dummy"; }
		}

		public ZBool IsSupportsDirectSchedule
		{
			get { return false; }
		}

		public bool RequiresMultiContainerBooking => true;

		public DtbBookingDirection[] SupportedDirections
		{
			get { return supportedDirections ?? (supportedDirections = new DtbBookingDirection[] { DtbBookingDirection.PIC }); }
			set { supportedDirections = value; }
		}

		public DtbBookingDirection[] GetSupportedDirections()
		{
			return SupportedDirections;
		}

		DtbBookingDirection[] supportedDirections;

		public IJobInvoicingPlugIn InvoicingJob
		{
			get { return this; }
		}

		public void TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
			if (TransportBookingCreatedOrUpdated_ForTesting != null)
			{
				TransportBookingCreatedOrUpdated_ForTesting();
			}
		}

		public Action TransportBookingCreatedOrUpdated_ForTesting;

		public ZString JobStatus
		{
			get { return "Dummy Job Status"; }
		}

		public ControllerID ControllerID
		{
			get { return DummyControllerIDs.Dummy; }
		}

		public Guid BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		public ZQuery TransportBookingTemplateFilters => new ZQuery();

		public bool CanCreateTransportBooking => true;

		public ZGuid BookingParentPK => PK;

		public string BookingParentTablePrefix => TablePrefix;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new DummyWithDtbBookingJobInvoicingSupporter()); }
		}

		IJobInvoicingSupporter invoicingSupporter;

		string IJobNumber.JobNumber
		{
			get { return Z0_Description; }
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
		}

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return new DocManagerInfo(this, "DUM"); }
		}
	}
}
