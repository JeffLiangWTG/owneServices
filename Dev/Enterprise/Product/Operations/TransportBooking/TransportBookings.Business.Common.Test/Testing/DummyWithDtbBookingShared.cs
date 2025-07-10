using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Shared.Testing
{
	public class DummyWithDtbBookingShared : DummyBusinessObject, IDtbBookingParent
	{
		public DummyWithDtbBookingShared(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableName; }
		}

		public ZString JobNumber
		{
			get { return "D00000123"; }
		}

		public ZString JobType
		{
			get { return ""; }
		}

		public ZString JobTypeDescription
		{
			get { return "Dummy"; }
		}

		public ZString JobStatus
		{
			get { return ""; }
		}

		public ZString Waybill
		{
			get { return "H012345"; }
		}

		public ZString ClientServiceLevel
		{
			get { return "STD"; }
		}

		public DtbBookingDirection[] GetSupportedDirections()
		{
			return Array.Empty<DtbBookingDirection>();
		}

		public ZGuid LocalClientAddressPK
		{
			get { return ZGuid.Empty; }
		}

		public IJobInvoicingPlugIn InvoicingJob
		{
			get { return null; }
		}

		public ControllerID ControllerID
		{
			get { return ControllerIDs.JobShipment; }
		}

		public Guid BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		public void TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
		}

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		public ZBool IsSupportsDirectSchedule
		{
			get { return false; }
		}

		public ZQuery TransportBookingTemplateFilters
		{
			get { return new ZQuery(); }
		}

		public bool RequiresMultiContainerBooking => true;

		public bool CanCreateTransportBooking => true;

		public ZGuid BookingParentPK => PK;

		public string BookingParentTablePrefix => TablePrefix;
	}
}
