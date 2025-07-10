
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.Shared
{
	public interface IDtbBookingParent : IRelatedJob
	{
		ZGuid PK { get; }
		string TablePrefix { get; }
		bool HasChanges { get; }
		ZString HumanReadableName { get; }
		bool IsInDatabase { get; }
		BusinessObjectFactory Factory { get; }

		ZQuery TransportBookingTemplateFilters { get; }

		ZBool IsSupportsDirectSchedule { get; }
		ZString JobType { get; }
		ZString JobTypeDescription { get; }
		bool CanCreateTransportBooking { get; }

		bool RequiresMultiContainerBooking { get; }

		DtbBookingDirection[] GetSupportedDirections();

		IJobInvoicingPlugIn InvoicingJob { get; }

		void TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings = null);

		ZGuid BookingParentPK { get; }

		string BookingParentTablePrefix { get; }

		(bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking();
	}
}
