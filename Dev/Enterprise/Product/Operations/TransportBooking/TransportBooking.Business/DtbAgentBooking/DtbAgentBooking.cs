using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	[HiddenBookingParent]
	public class DtbAgentBooking : AutoDtbAgentBooking, IDtbAgentBooking, IDtbBookingParent, IJobNumber, IJobInvoicingPlugIn
	{
		public DtbAgentBooking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static DtbAgentBooking NewFromBooking(DtbBooking booking)
		{
			var agentBooking = booking.Factory.New<DtbAgentBooking>();

			agentBooking.LTB_KM_TransportBooking = booking.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = agentBooking.TablePrefix;
			booking.ConsolidationSingleJob.KB_ParentID = agentBooking.PK;

			return agentBooking;
		}

		public static bool IsAuthorisedCarrierBookingAgent(ZString cba)
		{
			return AuthorisedCarrierBookingAgents.Contains(cba);
		}

		static IEnumerable<ZString> AuthorisedCarrierBookingAgents
		{
			get
			{
				yield return "3GTMS_EAD";
				yield return "SMARTFREIGHT_EAD";
				yield return "TEKNOWLOGI_EAD";
				yield return "PIERBRIDGE_EAD";
				yield return "TRINIUM_EAD";
				yield return "SAASTRANS_EAD";
				yield return "BLUME_EAD";
				yield return ContainerTransportOptimizationCBA;
			}
		}

		public const string ContainerTransportOptimizationCBA = "CONTAINER_TRANSPORT_OPTIMIZATION";

		public DtbBooking Booking => Factory.Load<DtbBooking>(LTB_KM_TransportBooking);

		[RelatedBusinessObject(nameof(Booking))]
		public override ZGuid LTB_KM_TransportBooking { get => base.LTB_KM_TransportBooking; }

		public override void OnSaving()
		{
			PopulateLTB_JobID();
			base.OnSaving();
		}

		void PopulateLTB_JobID()
		{
			if (!IsInDatabase)
			{
				LTB_JobID = Env.NumberFountains.DtbAgentBookingID.GetNextFormatted(Factory);
			}
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new DtbAgentBookingFountainUniqueIndexFailureHandler(this)); }
		}

		class DtbAgentBookingFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public DtbAgentBookingFountainUniqueIndexFailureHandler(DtbAgentBooking agentBooking)
				: base(DtbAgentBookingSchema.Constants.Indexes.NR_UC__LTB_JobID, agentBooking)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix => Env.NumberFountains.DtbAgentBookingID;
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		// interfaces

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookingsFromResult)
		{
			throw new NotSupportedException();
		}

		ZBool IDtbBookingParent.IsSupportsDirectSchedule => false;

		ZString IDtbBookingParent.JobType => ZString.Empty;

		ZString IDtbBookingParent.JobTypeDescription => ZString.Empty;

		DtbBookingDirection[] IDtbBookingParent.GetSupportedDirections() => Array.Empty<DtbBookingDirection>();

		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob => this;

		ZString IRelatedJob.JobDescription => ZString.Empty;

		ZString IRelatedJob.JobStatus => ZString.Empty;

		ControllerID IControllerIDProvider.ControllerID => null;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		ZString IRelatedJob.JobNumber => LTB_JobID;

		public ZQuery TransportBookingTemplateFilters => new ZQuery();

		public bool RequiresMultiContainerBooking => true;

		public bool CanCreateTransportBooking => true;

		public ZGuid BookingParentPK => PK;

		public string BookingParentTablePrefix => TablePrefix;

		string IJobNumber.JobNumber => Booking?.KM_JobID;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => new DtbAgentBookingInvoicingSupporter(this);

		bool IJobHeaderParent.AllowInvoiceDeletion => true;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateLTB_JobID();
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			var attachEvenIfInDatabase = IsAdoptingJobHeader;
			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromChildPortTransportJob() and TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildPortTransportJob()
			CartageHelper.AttachCartageJobsToParentJob(job, PK, attachEvenIfInDatabase);
			// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildLandTransportConsignment()
			ConsignmentJobHelper.AttachLTConsignmentJobsToParentJob(job, PK, attachEvenIfInDatabase);
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

		public DisposableAction TurnOnAdoptingJobHeaderMode()
		{
			var initAction = new Action(() =>
			{
				AdoptingJobHeaderModeCalls++;
			});
			var disposeAction = new Action(() =>
			{
				AdoptingJobHeaderModeCalls--;
			});
			return new DisposableAction(initAction, disposeAction);
		}

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		public bool IsAdoptingJobHeader => AdoptingJobHeaderModeCalls > 0;
		int AdoptingJobHeaderModeCalls;
	}
}
