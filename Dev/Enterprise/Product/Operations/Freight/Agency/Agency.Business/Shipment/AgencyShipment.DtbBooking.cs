using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	partial class AgencyShipment : IDtbBookingParent
	{
		IEnumerable<BusinessObject> RelatedTransportBookingEvents
		{
			get { return TransportBookingLoader.GetRelatedTransportBookingEvents(this); }
		}

		#region IRelatedJob Members

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableName; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IRelatedJob.JobNumber
		{
			get { return JS_UniqueConsignRef; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IRelatedJob.JobStatus
		{
			get { return JS_ShipmentStatus; }
		}

		#endregion

		#region IDtbBookingParent Members

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IDtbBookingParent.JobType
		{
			get { return ((IWorkflowProvider)this).WorkflowType; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IDtbBookingParent.JobTypeDescription
		{
			get { return HumanReadableNameWithoutID; }
		}

		DtbBookingDirection[] IDtbBookingParent.GetSupportedDirections()
		{
			if (ShipmentStatusHelperMethods.IsBookingStage(JS_ShipmentStatus))
			{
				return new DtbBookingDirection[] { DtbBookingDirection.PIC };
			}
			else
			{
				return new DtbBookingDirection[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob
		{
			get { return this; }
		}

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZBool IDtbBookingParent.IsSupportsDirectSchedule
		{
			get { return false; }
		}

		public ZQuery TransportBookingTemplateFilters => new ZQuery();

		bool IDtbBookingParent.RequiresMultiContainerBooking => true;

		public bool CanCreateTransportBooking => true;

		public ZGuid BookingParentPK => PK;

		public string BookingParentTablePrefix => TablePrefix;

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		#region IControllerIDProvider Members

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ControllerID IControllerIDProvider.ControllerID
		{
			get { return GetDtbBookingParentControllerID(); }
		}

		protected virtual ControllerID GetDtbBookingParentControllerID()
		{
			throw new NotImplementedException("This method should only be accessed via instances of subclasses."); // Exception message
		}

		#endregion

		#endregion

		#region IJobHeaderParent members

		public override void OnJobCreating(JobHeader job)
		{
			base.OnJobCreating(job);

			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromChildPortTransportJob() and TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildPortTransportJob()
			CartageHelper.AttachCartageJobsToParentJob(job, PK);
			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildLandTransportConsignment()
			ConsignmentJobHelper.AttachLTConsignmentJobsToParentJob(job, PK);
		}

		ICartageHelper CartageHelper
		{
			get { return cartageHelper ?? (cartageHelper = ObjectFactory.Get<ICartageHelper>()); }
		}
		ICartageHelper cartageHelper;

		IConsignmentJobHelper ConsignmentJobHelper
		{
			get { return consignmentJobHelper ?? (consignmentJobHelper = ObjectFactory.Get<IConsignmentJobHelper>()); }
		}
		IConsignmentJobHelper consignmentJobHelper;

		#endregion
	}
}
