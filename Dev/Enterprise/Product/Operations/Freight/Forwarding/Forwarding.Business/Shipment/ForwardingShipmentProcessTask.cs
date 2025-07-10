using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentProcessTask : RoutingSupportProcessTask, Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask
	{
		public ForwardingShipmentProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : RoutingSupportProcessTask.Schema
		{
			public const string LateCargoReportReason = "LateCargoReportReason";
			public const string LateCargoReportText = "LateCargoReportText";
		}

		protected override Type ParentType
		{
			get { return typeof(ForwardingShipment); }
		}

		public new ForwardingShipment Parent
		{
			get { return (ForwardingShipment)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get
			{
				if (Parent != null && Parent.JS_IsForwardRegistered)
				{
					return ControllerIDs.JobShipment;
				}
				else
				{
					return null;
				}
			}
		}

		protected override bool MatchesParentForMessageTriggerAction(ProcessTask relatedTrigger)
		{
			bool result = base.MatchesParentForMessageTriggerAction(relatedTrigger);
			if (!result &&
				ProcessTaskNotifications.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback) &&
				relatedTrigger.ProcessTaskNotifications.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback))
			{
				JobShipmentPreplanning relatedPreadvice = relatedTrigger.Parent as JobShipmentPreplanning;
				CommonConsol relatedConsol = relatedTrigger.Parent as CommonConsol;

				result = result || relatedPreadvice != null && Parent.PreAdvice == relatedPreadvice;
				result = result || relatedConsol != null && Parent.Consols.Contains(relatedConsol);
			}
			return result;
		}

		#region Transport Legs

		protected override Type TransportType
		{
			get { return typeof(Transport); }
		}

		protected override Type TransportParentType
		{
			get { return typeof(ForwardingShipment); }
		}

		protected override bool IsTransportDepartureEvent(ZString eventCode)
		{
			return
				base.IsTransportDepartureEvent(eventCode) ||
				eventCode == Events.GateOut.Code;
		}

		protected override bool IsTransportArrivalEvent(ZString eventCode)
		{
			return
				base.IsTransportArrivalEvent(eventCode) ||
				eventCode == Events.GateIn.Code;
		}

		protected override Transport GetTransportLegToAttach()
		{
			if (P9_SE_NKMilestoneEvent == Events.Departure.Code || P9_SE_NKMilestoneEvent == Events.GateIn.Code || P9_SE_NKMilestoneEvent == Events.CutOffDate.Code)
			{
				foreach (Transport transport in TransportLegsInMovementOrder)
				{
					if (transport.JW_RL_NKLoadPort == Parent.JS_RL_NKOrigin)
					{
						return transport;
					}
				}
			}

			if (P9_SE_NKMilestoneEvent == Events.Arrival.Code)
			{
				foreach (Transport transport in TransportLegsInMovementOrder)
				{
					if (transport.JW_RL_NKDiscPort == Parent.JS_RL_NKDestination)
					{
						return transport;
					}
				}
			}

			return null;
		}

		#endregion

		#region Pulling Cargo Available from Shipment

		protected override void OnSetActualDateCore(ZDateTimeOffset value)
		{
			base.OnSetActualDateCore(value);

			if (IsMilestoneOrWorkflowTrigger)
			{
				if (P9_SE_NKMilestoneEvent == Events.PickupCartageAdvised.Code)
				{
					Parent.DocsAndCartage.JP_PickupCartageAdvised = value.ToZDateTime();
				}
				else if (P9_SE_NKMilestoneEvent == Events.DeliveryCartageAdvised.Code)
				{
					Parent.DocsAndCartage.JP_DeliveryCartageAdvised = value.ToZDateTime();
				}
			}
		}

		protected override bool P9_ActualDate_ReadOnly
		{
			get { return base.P9_ActualDate_ReadOnly || P9_SE_NKMilestoneEvent == Events.CargoAvailable.Code; }
		}

		#endregion

		#region Closing Order AllImportDocumentsReceived Exceptions

		public override ZString P9_Status
		{
			get { return base.P9_Status; }
			set
			{
				if (base.P9_Status != value)
				{
					base.P9_Status = value;
					if (IsException &&
						P9_SE_NKMilestoneEvent == Events.AllImportDocumentsReceived.Code &&
						IsExceptionActioned)
					{
						ClosePreadviceAllImportDocumentsReceivedExceptions();
					}
				}
			}
		}

		void ClosePreadviceAllImportDocumentsReceivedExceptions()
		{
			foreach (JobShipmentPreplanning preadvice in Factory.Load<JobShipmentPreplanning>(new ZQuery(JobShipmentPreplanningSchema.EF_JS, Parent.PK)))
			{
				foreach (ProcessTask exception in preadvice.WorkflowItems.Exceptions)
				{
					if (P9_SE_NKMilestoneEvent == Events.AllImportDocumentsReceived.Code &&
						!exception.IsExceptionActioned)
					{
						exception.IsExceptionActioned = true;
					}
				}
			}
		}

		#endregion
		#region BusinessObject Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			if (IsException && IsCargoReportAcceptedProcessTask && !LateCargoReportReason.IsEmpty)
			{
				if (LateCargoReportText.IsEmpty && LateCargoReportingReasonCodes.ContainsCode(LateCargoReportReason))
				{
					LateCargoReportText = LateCargoReportingReasonCodes.GetDescriptionFromCode(LateCargoReportReason);
				}
				if (!LateCargoReportText.Contains((NoResString)"Submitted by:", StringComparison.Ordinal)) // Hard-coded constant
				{
					LateCargoReportText = Res.GetString("323de499-9dd8-4bf7-bf48-da8c02fbdbc0", "{0} (Submitted by: {1} at", LateCargoReportText, GlbStaff.CurrentUser.GS_FullName) + " " + ZDateTime.Now.ToString() + ")";
				}
			}
		}

		protected override ProcessTasksValidation GetNewValidation()
		{
			ProcessTasksValidation result;
			if (IsException && IsCargoReportAcceptedProcessTask)
			{
				result = new CargoReportAcceptedExceptionValidation(this);
			}
			else
			{
				result = base.GetNewValidation();
			}
			return result;
		}

		#endregion

		#region ProcessTask Property Overrides

		protected override ProcessTaskNotificationCollection GetNewProcessTaskNotificationCollection()
		{
			return new ForwardingShipmentProcessTaskNotificationCollection(this);
		}

		#region ETA

		public override ZDateTime ETA
		{
			get { return Parent == null ? ZDateTime.Empty : Parent.JS_E_ARV; }
		}

		#endregion

		#region ETD

		public override ZDateTime ETD
		{
			get { return Parent == null ? ZDateTime.Empty : Parent.JS_E_DEP; }
		}

		#endregion

		#region Load/Origin Port

		public override ZString LoadOrOriginPort
		{
			get { return Parent == null ? ZString.Empty : Parent.JS_RL_NKOrigin; }
		}

		#endregion

		#region Discharge/Destination Port

		public override ZString DischargeOrDestinationPort
		{
			get { return Parent == null ? ZString.Empty : Parent.JS_RL_NKDestination; }
		}

		#endregion

		#endregion

		#region Late Cargo Reporting

		[List("LateCargoReportingReasonCodes")]
		[MaxLength(5)]
		public ZString LateCargoReportReason
		{
			get
			{
				return notesBlob.blobReason;
			}
			set
			{
				if (notesBlob.blobReason != value)
				{
					CheckMaximumLength(LateCargoReportReasonInfo, value);
					notesBlob.blobReason = value;
					SetPropertyValue(P9_NotesInfo, notesBlob.ToBlob());
					if (!IsValidationSuspended && Validation.GetType() == typeof(CargoReportAcceptedExceptionValidation))
					{
						((CargoReportAcceptedExceptionValidation)Validation).ValidateLateCargoReportReason();
					}
					LateCargoReportReasonInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo LateCargoReportReasonInfo
		{
			get { return GetZPropertyInfo(Schema.LateCargoReportReason); }
		}

		[MaxLength(2048)]
		public ZString LateCargoReportText
		{
			get
			{
				return notesBlob.blobText;
			}
			set
			{
				if (notesBlob.blobText != value)
				{
					CheckMaximumLength(LateCargoReportTextInfo, value);
					notesBlob.blobText = value;
					SetPropertyValue(P9_NotesInfo, notesBlob.ToBlob());
					if (!IsValidationSuspended && Validation.GetType() == typeof(CargoReportAcceptedExceptionValidation))
					{
						((CargoReportAcceptedExceptionValidation)Validation).ValidateLateCargoReportText();
					}
					LateCargoReportTextInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo LateCargoReportTextInfo
		{
			get { return GetZPropertyInfo(Schema.LateCargoReportText); }
		}

		NotesBlob notesBlob
		{
			get
			{
				if (cachedNotesBlob == null)
				{
					cachedNotesBlob = new NotesBlob(P9_Notes);
				}
				return cachedNotesBlob;
			}
		}
		NotesBlob cachedNotesBlob;

		class NotesBlob
		{
			public NotesBlob(ZBlob notes)
			{
				ZString blobAscii = notes.ToAscii();
				if (blobAscii.Length > 5 && blobAscii.Substring(5, 1) == ":")
				{
					blobReason = blobAscii.Substring(0, 5);
					blobText = blobAscii.Substring(6);
				}
				else
				{
					blobReason = ZString.Empty;
					blobText = blobAscii;
				}
			}
			public ZString blobReason;
			public ZString blobText;

			public ZBlob ToBlob()
			{
				return ZBlob.FromAscii(blobReason.PadRight(5) + ":" + blobText);
			}
		}

		public CodeDescriptionPairList LateCargoReportingReasonCodes
		{
			get { return Factory.GetCachedValue<LateCargoReportingReasons>(); }
		}

		public bool IsCargoReportAcceptedProcessTask
		{
			get
			{
				return Parent != null && P9_SE_NKMilestoneEvent == ((IParentForCargoReporter)Parent).CargoReportAcceptedEvent.Code;
			}
		}

		protected bool P9_Notes_ReadOnly
		{
			get { return IsException && IsExceptionActioned && !P9_Notes.IsEmpty && IsCargoReportAcceptedProcessTask; }
		}

		protected bool IsExceptionActioned_ReadOnly
		{
			get { return IsException && IsExceptionActioned && IsCargoReportAcceptedProcessTask; }
		}

		public static ProcessTask[] CargoReportAcceptedMilestones(BusinessObject parent)
		{
			ZQuery milestoneQuery = new ZQuery(ProcessTasksSchema.P9_ParentID, parent.PK);
			milestoneQuery.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);
			milestoneQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, ((IParentForCargoReporter)parent).CargoReportAcceptedEvent.Code);
			return parent.Factory.Load<ProcessTask>(milestoneQuery);
		}

		public static ForwardingShipmentProcessTask UnactionedExceptionForMiletsone(ProcessTask mileStone)
		{
			if (!mileStone.IsClosed && mileStone.P9_ActualDate.IsEmpty)
			{
				ForwardingShipmentProcessTask cachedMilestoneException = (ForwardingShipmentProcessTask)mileStone.MilestoneException;
				if (cachedMilestoneException != null && !cachedMilestoneException.IsExceptionActioned)
				{
					return cachedMilestoneException;
				}
			}
			return null;
		}

		public override bool CanDelete
		{
			get
			{
				return (!IsException || !IsCargoReportAcceptedProcessTask) && base.CanDelete;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return !(!IsException || !IsCargoReportAcceptedProcessTask) ? ResString.GetMultilingualString("92dc5ddb-9270-47e8-a289-a1577a3bbabb", "This Exception may not be deleted because it contains Late Cargo Reporting Information, which must be retained.")
				: base.ReasonForNotAbleToDelete;
			}
		}

		#endregion
	}
}
