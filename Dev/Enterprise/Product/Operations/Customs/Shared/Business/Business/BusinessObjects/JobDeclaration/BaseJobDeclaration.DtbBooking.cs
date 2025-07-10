using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public partial class BaseJobDeclaration : IDtbBookingParent
	{
		public string TransportBookingTransportMode
		{
			get { return TransportBookingTransportModeCore; }
		}

		#region IDtbBookingParent

		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob
		{
			get { return this; }
		}

		ZString IDtbBookingParent.JobType
		{
			get { return ((IWorkflowProvider)this).WorkflowType; }
		}

		ZString IDtbBookingParent.JobTypeDescription
		{
			get { return HumanReadableNameWithoutID; }
		}

		DtbBookingDirection[] IDtbBookingParent.GetSupportedDirections()
		{
			return GetSupportedDirectionsCore();
		}

		protected virtual DtbBookingDirection[] GetSupportedDirectionsCore()
		{
			return new[] { IsExport ? DtbBookingDirection.PIC : DtbBookingDirection.DLV };
		}

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
		}

		ZBool IDtbBookingParent.IsSupportsDirectSchedule
		{
			get { return false; }
		}

		bool IDtbBookingParent.RequiresMultiContainerBooking => true;

		public ZQuery TransportBookingTemplateFilters => new ZQuery();

		public bool CanCreateTransportBooking => true;

		public ZGuid BookingParentPK => PK;

		public string BookingParentTablePrefix => TablePrefix;

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		#endregion // IDtbBookingParent

		#region IRelatedJob

		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableName; }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return JE_DeclarationReference; }
		}

		ZString IRelatedJob.JobStatus
		{
			get { return JE_EntryStatusDescription; }
		}

		#endregion

		#region Implementation

		protected virtual string TransportBookingTransportModeCore
		{
			get
			{
				return IsAir ? Constants.TransportModes.Air
					 : IsSea ? Constants.TransportModes.Sea
					 : IsRoad ? Constants.TransportModes.Road
					 : IsRail ? Constants.TransportModes.Rail
					 : IsPost ? Constants.TransportModes.Mail
					 : "";
			}
		}

		void IBuyerSupplierRelationshipConsumer.RestoreImportBrokerFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultStatus)
		{
		}

		#endregion // Implementation
	}
}
