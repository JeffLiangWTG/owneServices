using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.Business.Testing
{
	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	public class DummyWithDtbBooking : DummyWithWorkflow, IDtbBookingParent, IJobInvoicingPlugIn, IDocManagerSupport
	{
		public DummyWithDtbBooking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CanCreateTransportBooking = true;
			BookingParentPK = PK;
			BookingParentTablePrefix = TablePrefix;
		}

		public ZDateTime LastPickupCompleted;

		public string LastDeliverySignedBy;
		public ZDateTime LastDeliveryCompleted;

		public OrgHeader Consignee
		{
			get
			{
				OrgHeader result = null;
				if (ConsigneeDocAddress != null && !ConsigneeDocAddress.E2_AddressOverride)
				{
					result = ConsigneeDocAddress.Organisation;
				}
				return result;
			}
		}

		public JobDocAddress ConsigneeDocAddress
		{
			get { return consigneeDocAddress; }
			set { consigneeDocAddress = value; }
		}

		JobDocAddress consigneeDocAddress;

		public virtual bool RequiresMultiContainerBooking => true;

		ZString IRelatedJob.JobDescription
		{
			get { return "Dummy 123"; }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return JobNumber; }
		}

		public ZString JobType
		{
			get { return "DUM"; }
		}

		public ZString JobTypeDescription
		{
			get { return "Dummy"; }
		}

		public ZString Waybill
		{
			get { return "H012345"; }
		}

		public ZString ClientServiceLevel
		{
			get { return "STD"; }
		}

		public ZBool IsSupportsDirectSchedule
		{
			get { return false; }
		}

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

		public void TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookingsFromResult)
		{
			if (TransportBookingCreatedOrUpdated_ForTesting != null)
			{
				TransportBookingCreatedOrUpdated_ForTesting();
			}
		}

		public Action TransportBookingCreatedOrUpdated_ForTesting;

		public ZQuery TransportBookingTemplateFilters { get; set; }

		public bool CanCreateTransportBooking { get; set; }

		public ZGuid BookingParentPK { get; set; }

		public string BookingParentTablePrefix { get; set; }

		public virtual IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new DummyWithDtbBookingJobInvoicingSupporter()); }
		}

		protected IJobInvoicingSupporter invoicingSupporter;

		string IJobNumber.JobNumber
		{
			get { return JobNumber; }
		}

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return Factory; }
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

		ZGuid IJobHeaderParentCore.PK
		{
			get { return PK; }
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
		}

		public virtual (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		string IJobHeaderParentCore.TableName
		{
			get { return TableName; }
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return new DocManagerInfo(this, "DUM"); }
		}
	}
	public class DummyWithDtbBookingAndConfirmMessage : DummyWithDtbBooking
	{
		public DummyWithDtbBookingAndConfirmMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (true, "Test caption", "Test confrim message", "Test confirmation");
	}
}
