using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.CalendarArithmetic;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Schema;
using CargoWise.Shared;
using CargoWise.Tools.TextStandardizer;
using CargoWise.Types;
using CargoWise.Workflow;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(ProcessTask.Schema.P9_TaskID), DescriptionProperty(ProcessTask.Schema.P9_Description)]
	[DebuggerDisplay("{" + Schema.P9_Type + "}-{" + Schema.P9_TaskID + "}-{" + Schema.P9_Description + "}-{" + Schema.P9_SE_NKMilestoneEvent + "}")]
	[UniversalCopyWithExtendedEntities]
	[UniversalCopyIgnoreElement(ProcessTask.Schema.P9_TaskID)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[UniversalDataContext(DataContextType.WorkflowTask)]
	public partial class ProcessTask : ProcessTaskWithUTCAdapters,
		IProcessTask,
		IWorkflowTask,
		IWorkflowTrigger,
		ITemplateTrigger,
		ILineTriggerSupport,
		IAssignedWorkflowItem,
		IExternalReferencingTrigger,
		IProcessTaskInternals,
		IDocManagerSupport,
		ICustomTextTemplateContext,
		IRootTypeProvider,
		IDynamicRootProvider,
		ICanDelete,
		ITagable,
		ITagBindable,
		IProcessHandlingInfoProvider,
		INumberFountainConsumer,
		IJobNumber,
		IDefaultedFromDateProvider,
		IAntlrMacroContextProvider,
		IDataVersionLoggingSupported,
		IRegisterStatusChangeMode,
		IUniversalCopiedIdentifier
	{
		public ProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_Notes), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_SystemCreateTimeUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_SystemLastEditTimeUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_SystemLastEditUser), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_SystemCreateUser), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_TriggerFiredCountdown), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_FC_CurrentComponent), ConcurrencyPolicy.Ignore);

			if (IsTask)
			{
				WorkflowAfterOnSavingBOService.HookupFactory(Factory);
			}
		}

		#region Schema

		public new abstract class Schema : AutoProcessTasks.Schema
		{
			public const string P9_CompletedTime = Schema.P9_CompletedTimeUtc; // A temporary column
			public const string IsMilestone = "IsMilestone";
			public const string P9_SE_NKMilestoneEventDescription = "P9_SE_NKMilestoneEventDescription";
			public const string P9_SE_NKExceptionEventDescription = "P9_SE_NKExceptionEventDescription";
			public const string ReferenceCode = "ReferenceCode";
			public const string Status = "Status";
			public const string IsExceptionActioned = "IsExceptionActioned";
			public const string P9_Condition2ValueForBinding = "P9_Condition2ValueForBinding";
			public const string P9_OriginalScheduledDateLocal = "P9_OriginalScheduledDateLocal";
			public const string P9_ShareTasksForAllCompanies = "P9_ShareTasksForAllCompanies";
			public const string P9_TriggerConditionValue = "P9_TriggerConditionValue";
			public const string P9_ActualDateForBinding = "P9_ActualDateForBinding";
			public const string P9_ActualDateOffset = "P9_ActualDateOffset";
			public const string P9_ScheduledDateForBinding = "P9_ScheduledDateForBinding";
			public const string Milestone_P9_TriggerField = "Milestone_P9_TriggerField";
			public const string Trigger_P9_TriggerField = "Trigger_P9_TriggerField";
			public const string Milestone_P9_SE_NKMilestoneEvent = "Milestone_P9_SE_NKMilestoneEvent";
			public const string Trigger_P9_SE_NKMilestoneEvent = "Trigger_P9_SE_NKMilestoneEvent";
			public const string EffectiveTaskNudge = "EffectiveTaskNudge";
		}

		#endregion

		#region Constants

		public const string WorkflowEventTriggerJobQueueName = "WorkflowEventTrigger";
		public const string LastCompletedStatusCode = "LST";
		public const string NextToBeCompletedStatusCode = "NXT";
		public const int MacroTriggerConditionValueMaxLength = 2048;
		public const int TriggerConditionValueMaxLength = 1024;
		public const string ProcessHeaderSequence = "WorkflowSequence";

		#region MessageRecipientPartyTypeList-related members

		//this has a chance to be useless if MessagingTriggerParties will become useless/obsolete; should this be tested?
		public static MessageRecipientPartyType GetMessageRecipientPartyTypeFromCode(ZString code)
		{
			switch (code)
			{
				case (MessageRecipientPartyTypeList.Codes.Consignee):
					return MessageRecipientPartyType.Consignee;
				case (MessageRecipientPartyTypeList.Codes.Consignor):
					return MessageRecipientPartyType.Consignor;
				case (MessageRecipientPartyTypeList.Codes.Broker):
					return MessageRecipientPartyType.Broker;
				case (MessageRecipientPartyTypeList.Codes.ImportBroker):
					return MessageRecipientPartyType.ImportBroker;
				case (MessageRecipientPartyTypeList.Codes.ExportBroker):
					return MessageRecipientPartyType.ExportBroker;
				case (MessageRecipientPartyTypeList.Codes.BillToParty):
					return MessageRecipientPartyType.BillToParty;
				case (MessageRecipientPartyTypeList.Codes.PickupCartage):
					return MessageRecipientPartyType.PickupCartage;
				case (MessageRecipientPartyTypeList.Codes.DeliveryCartage):
					return MessageRecipientPartyType.DeliveryCartage;
				case (MessageRecipientPartyTypeList.Codes.SendingAgent):
					return MessageRecipientPartyType.SendingAgent;
				case (MessageRecipientPartyTypeList.Codes.ReceivingAgent):
					return MessageRecipientPartyType.ReceivingAgent;
				case (MessageRecipientPartyTypeList.Codes.ControllingAgent):
					return MessageRecipientPartyType.ControllingAgent;
				case (MessageRecipientPartyTypeList.Codes.ControllingCustomer):
					return MessageRecipientPartyType.ControllingCustomer;
				case (MessageRecipientPartyTypeList.Codes.OrgProxy):
					return MessageRecipientPartyType.OrgProxy;
				case (MessageRecipientPartyTypeList.Codes.Client):
					return MessageRecipientPartyType.Client;
				case (MessageRecipientPartyTypeList.Codes.Email):
					return MessageRecipientPartyType.Email;
				case (MessageRecipientPartyTypeList.Codes.Print):
					return MessageRecipientPartyType.Print;
				case (MessageRecipientPartyTypeList.Codes.TransportCo):
					return MessageRecipientPartyType.TransportCo;
				case (MessageRecipientPartyTypeList.Codes.Carrier):
					return MessageRecipientPartyType.Carrier;
				case (MessageRecipientPartyTypeList.Codes.CarrierBookingAgent):
					return MessageRecipientPartyType.CarrierBookingAgent;
				case (MessageRecipientPartyTypeList.Codes.NotifyParty):
					return MessageRecipientPartyType.NotifyParty;
				case (MessageRecipientPartyTypeList.Codes.DeliveryToParty):
					return MessageRecipientPartyType.DeliveryToParty;
				case (MessageRecipientPartyTypeList.Codes.PickupParty):
					return MessageRecipientPartyType.PickupParty;
				case (MessageRecipientPartyTypeList.Codes.EDICommunication):
					return MessageRecipientPartyType.EDICommunication;
				case (MessageRecipientPartyTypeList.Codes.InvoiceDebtor):
					return MessageRecipientPartyType.InvoiceDebtor;
				case (MessageRecipientPartyTypeList.Codes.CarrierMessagingDebtor):
					return MessageRecipientPartyType.CarrierMessagingDebtor;
				case (MessageRecipientPartyTypeList.Codes.DepartureCFS):
					return MessageRecipientPartyType.DepartureCFS;
				case (MessageRecipientPartyTypeList.Codes.ArrivalCFS):
					return MessageRecipientPartyType.ArrivalCFS;
				case (MessageRecipientPartyTypeList.Codes.ArrivalCarrier):
					return MessageRecipientPartyType.ArrivalCarrier;
				case (MessageRecipientPartyTypeList.Codes.DepartureCarrier):
					return MessageRecipientPartyType.DepartureCarrier;
				case (MessageRecipientPartyTypeList.Codes.Principal):
					return MessageRecipientPartyType.Principal;
				case (MessageRecipientPartyTypeList.Codes.DepartureCTO):
					return MessageRecipientPartyType.DepartureCTO;
				case (MessageRecipientPartyTypeList.Codes.ArrivalCTO):
					return MessageRecipientPartyType.ArrivalCTO;
				case (MessageRecipientPartyTypeList.Codes.DepartureContainerYard):
					return MessageRecipientPartyType.DepartureContainerYard;
				case (MessageRecipientPartyTypeList.Codes.ArrivalContainerYard):
					return MessageRecipientPartyType.ArrivalContainerYard;
				case (MessageRecipientPartyTypeList.Codes.WarehouseInwards):
					return MessageRecipientPartyType.WarehouseInwards;
				case (MessageRecipientPartyTypeList.Codes.WarehouseOutwards):
					return MessageRecipientPartyType.WarehouseOutwards;
				case (MessageRecipientPartyTypeList.Codes.BondedWhsChangeOfOwnership):
					return MessageRecipientPartyType.BondedWhsChangeOfOwnership;
				case (MessageRecipientPartyTypeList.Codes.BondedWarehouseInwards):
					return MessageRecipientPartyType.BondedWarehouseInwards;
				case (MessageRecipientPartyTypeList.Codes.BondedWarehouseOutwards):
					return MessageRecipientPartyType.BondedWarehouseOutwards;
				case (MessageRecipientPartyTypeList.Codes.HVLVAirClearanceAgent):
					return MessageRecipientPartyType.HVLVAirClearanceAgent;
				case (MessageRecipientPartyTypeList.Codes.ShippingManager):
					return MessageRecipientPartyType.ShippingManager;
				case (MessageRecipientPartyTypeList.Codes.BookingParty):
					return MessageRecipientPartyType.BookingParty;
				case (MessageRecipientPartyTypeList.Codes.DeConsolidator):
					return MessageRecipientPartyType.DeConsolidator;
				case (MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty):
					return MessageRecipientPartyType.AirCargoResponsibleParty;
				case (MessageRecipientPartyTypeList.Codes.PickupAgent):
					return MessageRecipientPartyType.PickupAgent;
				case (MessageRecipientPartyTypeList.Codes.DeliveryAgent):
					return MessageRecipientPartyType.DeliveryAgent;
				case (MessageRecipientPartyTypeList.Codes.HVLVSeaClearanceAgent):
					return MessageRecipientPartyType.HVLVSeaClearanceAgent;
				case (MessageRecipientPartyTypeList.Codes.Warehouse):
					return MessageRecipientPartyType.Warehouse;
				case (MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse):
					return MessageRecipientPartyType.DepartureTransitWarehouse;
				case (MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse):
					return MessageRecipientPartyType.ArrivalTransitWarehouse;
				case (MessageRecipientPartyTypeList.Codes.WiseNettingSystem):
					return MessageRecipientPartyType.NettingSystem;
				case (MessageRecipientPartyTypeList.Codes.ContainerYard):
					return MessageRecipientPartyType.ContainerYard;
				case (MessageRecipientPartyTypeList.Codes.CTO):
					return MessageRecipientPartyType.CTO;
				case MessageRecipientPartyTypeList.Codes.PersonalEmail:
					return MessageRecipientPartyType.PersonalEmail;
				case MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail:
					return MessageRecipientPartyType.PersonPrimaryWorkEmail;
				case MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail:
					return MessageRecipientPartyType.PersonalFallbackPrimaryWorkEmail;
				default:
					return MessageRecipientPartyType.None;
			}
		}

		#endregion

		#endregion

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => IsDataVersionsAutoLogged;
		protected virtual bool IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region BusinessObject Overrides

		public static readonly ProcessTaskTypeDecider TypeDecider = new ProcessTaskTypeDecider();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			P9_EstimateVariationFactor = 2;

			if (!(this is TemplateProcessTask))
			{
				P9_GC = GlbCompany.CurrentCompany.PK;
			}
		}

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || !IsNonPersistedRepresentationOfTemplateTrigger); }
		}

		public override bool HasChanges
		{
			get
			{
				return useHasChangesForTemplateApplication ? hasChangesForTemplateApplication : base.HasChanges;
			}
			set
			{
				base.HasChanges = value;
				if (!suspendSettingHasChangesForTemplateApplication)
				{
					hasChangesForTemplateApplication = value;
				}
			}
		}

		public static IDisposable UseHasChangesForTemplateApplication()
		{
			if (useHasChangesForTemplateApplication)
			{
				return null;
			}
			else
			{
				useHasChangesForTemplateApplication = true;
				return new DisposableAction(() => useHasChangesForTemplateApplication = false);
			}
		}

		[ThreadStatic]
		static bool useHasChangesForTemplateApplication;

		IDisposable SuspendSettingHasChangesForTemplateApplication()
		{
			if (suspendSettingHasChangesForTemplateApplication)
			{
				return null;
			}
			else
			{
				suspendSettingHasChangesForTemplateApplication = true;
				return new DisposableAction(() => suspendSettingHasChangesForTemplateApplication = false);
			}
		}

		bool suspendSettingHasChangesForTemplateApplication;
		bool hasChangesForTemplateApplication;

		public override void OnSaving()
		{
			ReportErrorIfGhostedTrigger();
			RaiseMilestoneExceptionForFutureActualDate();

			if (!IsInDatabase)
			{
				DefaultEstimateIfRequired();
				if (!P9_FH_ProcessHeader.IsEmpty && ProcessHeader == null)
				{
					P9_FH_ProcessHeader = ZGuid.Empty;
				}

				if (P9_ParentTemplateID.IsValid)
				{
					WorkflowAfterOnSavingBOService.TryHookupRaceConditionHandlerService(Factory, TemplateApplicationRaceHandlingConfig.GetConfig());
				}
			}

			if (!P9_FormFlowType.IsEmpty
				&& IsInDatabase
				&& P9_Status != ProcessTaskStatusCodeList.Codes.Closed
				&& P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled
				&& P9_GS_NKAssignedStaffMemberInfo.HasChanges)
			{
				var userSyncProcessorFactory = ObjectFactory.New<Warehouse.Integration.IProcessTasksSyncUserProcessorFactory>();
				var userSyncProcessor = userSyncProcessorFactory.GetUserSyncProcessor(P9_FormFlowType);
				userSyncProcessor?.SyncUser(Factory, this);
			}

			if (IsMilestone &&
				P9_MilestoneExceptionAdded.IsEmpty &&
				P9_ActualDate.IsEmpty &&
				!P9_SE_NKExceptionEvent.IsEmpty &&
				P9_ScheduledDateUtc < ZDateTime.UtcToday)
			{
				CreateMilestoneException();
			}

			if (IsException)
			{
				if (P9_ActualDate.IsEmpty)
				{
					P9_ActualDate = ZDateTime.Now;
				}

				if (P9_SE_NKMilestoneEvent.IsEmpty && !IsInDatabase)
				{
					ProcessTaskLogger.AddExceptionRaisedLog(ExceptionTypeCode);
				}
			}

			if (IsMilestone)
			{
				using (Suspender<string>.Suspend(ref suspender, Events.EstimatedDateChangedCode))
				{
					SetMilestoneStatuses();
				}
			}

			if ((HasChanges || !IsInDatabase) && ProcessHeader != null)
			{
				ProcessHeader.FH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}

			AddChangedStatusLog();
			AddAssignedLog();

			CheckForEstimateLoop();

			ProcessEstimateLogHelper.CreateProcessTaskEstimateLog(this);

			SetTemporaryPenetrationReset();

			base.OnSaving();

			if (IsMilestoneOrWorkflowTrigger)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_CardNote), ConcurrencyPolicy.Ignore);
			}
		}

		internal void AddChangedStatusLog()
		{
			if ((IsTask || IsException) && ProcessTaskWrapper.StatusChangeLogs.Any())
			{
				var taskStatusChangedEventParametersStrategy = ObjectFactory.Get<TaskStatusChangedEventParametersStrategy>("BMTaskStatusChangedEventParametersStrategy");
				var additionalParams = taskStatusChangedEventParametersStrategy.GetLogReferenceParameters(this).ToArray();

				foreach (var log in processTaskWrapper.StatusChangeLogs)
				{
					Logs.CreateOrRecreateEventLog(Events.StatusChange, EstimateActual.Actual, log.Time, log.BuildLogReference(additionalParams));
				}
			}
			ProcessTaskWrapper.StatusChangeLogs.Clear();
		}

		void AddAssignedLog()
		{
			var referenceBuilder = EventLogReferenceBuilder.New();

			if ((!IsInDatabase || P9_GS_NKAssignedStaffMemberInfo.HasChanges) && !P9_GS_NKAssignedStaffMember.IsEmpty)
			{
				referenceBuilder.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Staff, P9_GS_NKAssignedStaffMember);
			}

			if ((!IsInDatabase || P9_GG_AssignedGroupCodeInfo.HasChanges) && !P9_GG_AssignedGroupCode.IsEmpty)
			{
				referenceBuilder.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Group, P9_GG_AssignedGroupCode);
			}

			var reference = referenceBuilder.Build();

			if (!string.IsNullOrEmpty(reference))
			{
				Logs.CreateOrRecreateEventLog(Events.Assigned, EstimateActual.Actual, ZDateTimeOffset.Now, reference);
			}
		}

		void SetTemporaryPenetrationReset()
		{
			var processHeader = ProcessHeader;

			if (IsTaskPenetrationResettable(processHeader))
			{
				var releaseGroup = processHeader.ReleaseGroup;
				var currentComponent = processHeader.CurrentComponent;

				if (releaseGroup != null && currentComponent != null)
				{
					if (CanResetTaskPenetrationOutsideGroup(releaseGroup, currentComponent) || CanResetTaskPenetrationInsideGroup(releaseGroup, currentComponent))
					{
						P9_IsResetBeingAppliedToThisTask = true;
						processHeader.SetPenetrationResetDate();

						ProcessTaskLogger.AddTaskPenetrationResetLog();
					}
				}
			}
		}

		bool IsTaskPenetrationResettable(IProcessHeader processHeader)
		{
			if (IsTask && processHeader != null && processHeader.IsReleased
				&& (P9_GS_NKAssignedStaffMember != (ZString)P9_GS_NKAssignedStaffMemberInfo.OriginalValue || !IsInDatabase)
				&& ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled
				&& WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.Value)
			{
				var taskType = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskType(WorkflowType, P9_Type);
				var system = processHeader.BMSystem;

				return taskType != null && taskType.AllowTaskReset
					&& system != null && system.FS_IsLive;
			}

			return false;
		}

		bool CanResetTaskPenetrationOutsideGroup(IGlbGroup releaseGroup, IBMComponent currentComponent)
		{
			return (AssignedStaffMember != null && !releaseGroup.Staff.Contains(AssignedStaffMember))
				&& IsComponentReleaseGroupFlagSet(currentComponent, releaseGroup, (l) => l.FO_ResetTaskPenetrationOutsideGroup);
		}

		bool CanResetTaskPenetrationInsideGroup(IGlbGroup releaseGroup, IBMComponent currentComponent)
		{
			return (AssignedStaffMember != null && releaseGroup.Staff.Contains(AssignedStaffMember))
				&& IsComponentReleaseGroupFlagSet(currentComponent, releaseGroup, (l) => l.FO_ResetTaskPenetrationInsideGroup);
		}

		bool IsComponentReleaseGroupFlagSet(IBMComponent currentComponent, IGlbGroup releaseGroup, Func<IBMComponentReleaseGroupLink, bool> linkFunc)
		{
			return currentComponent.ReleaseGroupLinks.ToArray().Cast<IBMComponentReleaseGroupLink>().Where(w => w.ReleaseGroup.PK == releaseGroup.PK).Any(l => linkFunc(l));
		}

		void CheckForEstimateLoop()
		{
			if (IsMilestone || IsTask)
			{
				var estimate = CalculateDefaultEstimate();
				using (SetOffsetForUtcDates(estimate.Offset))
				{
					var asDate = estimate.ToZDateTime();
					if (asDate.IsValid && asDate != P9_ScheduledDate)
					{
						P9_RecalculateScheduledDate = false;
					}
				}
			}
		}

		public bool DefaultCapabilityAssigned { get; set; }

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!IsNonPersistedRepresentationOfTemplateTrigger)
			{
				DefaultEstimateIfRequired();
				CancelIfParentIsCancelled();
				CheckIfShouldBeSaved();
			}
		}

		void CheckIfShouldBeSaved()
		{
			if (!((IBusiness)this).HasChangesNotIncludingChildren)
			{
				var propertyInfo = P9_ActualDateInfo;
				if (propertyInfo.HasChanges)
				{
					HasChanges = true;
				}
			}
		}

		protected virtual void CancelIfParentIsCancelled()
		{
			if (IsParentCancelled())
			{
				MarkExceptionsAsActioned();

				if (P9_Status != ProcessTaskStatusCodeList.Codes.Closed && Lookups.Types.ContainsCode(P9_Type))
				{
					P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				}
				if (P9_Type == Core.Constants.Workflow.MilestoneType && P9_ActualDate.IsEmpty)
				{
					Delete();
				}
			}
		}

		bool IsParentCancelled()
		{
			var parent = Parent;

			if (parent != null && ((BusinessObject)parent).IsDeleted)
			{
				return true;
			}
			else
			{
				return parent is ICancellable cancellable && cancellable.IsCancelled;
			}
		}

		public void DefaultEstimateIfRequired()
		{
			if (ShouldDefaultEstimate && !Loader.SuppressCreatingWorkflowFromTemplate)
			{
				inDefaultEstimate = true;
				try
				{
					var estimate = CalculateDefaultEstimate();
					using (SetOffsetForUtcDates(estimate.Offset))
					{
						if (estimate.IsValid && P9_ScheduledDateForBinding != estimate)
						{
							IsDefaultingFromSameEvent = EstimateDefaultedFromPredecessor != null && EstimateDefaultedFromPredecessor.P9_SE_NKMilestoneEvent == P9_SE_NKMilestoneEvent;
							var cycled = IsCycledEventLog(estimate);
							if (cycled)
							{
								P9_RecalculateScheduledDate = false;
							}
							else
							{
								P9_ScheduledDateForBinding = estimate;
								if (P9_ScheduledDateForBinding == estimate) //ensure !SuspendedScheduledDateChange
								{
									var refBuilder = EventLogReferenceBuilder.New();
									ServiceTaskTrackingLogHelper.AddServiceTaskDetails(refBuilder);
									Logs.AddNew(Events.CalculatedEstimate, refBuilder.Build(), P9_ScheduledDateOffset);
								}
							}
						}
					}
				}
				finally
				{
					IsDefaultingFromSameEvent = false;
					inDefaultEstimate = false;
				}
			}
		}

		ZDateTimeOffset CalculateDefaultEstimate()
		{
			ZDateTimeOffset estimate = ZDateTimeOffset.Empty;
			var estimateDefaultedFromDate = EstimateDefaultedFromDate;
			if (estimateDefaultedFromDate.IsValid && estimateDefaultedFromDate.IsValidSmallDateTime)
			{
				var newDate = WorkflowDescriptor.AddTimespanToDateTime(estimateDefaultedFromDate, P9_EstimatedDefaultTimeDelta);
				var asDateTime = newDate.ToZDateTime();
				if (asDateTime.IsValidSmallDateTime)
				{
					var latestDate = ZDateTime.Now.AddYears(-TypeValidationLimits.Default.PastYearsBeforeError);
					var newestDate = ZDateTime.Now.AddYears(TypeValidationLimits.Default.FutureYearsBeforeError);
					if (asDateTime > latestDate && asDateTime <= newestDate)
					{
						estimate = new ZDateTimeOffset(asDateTime.ToSmallDateTimeFloor(), newDate.Offset);
					}
				}
			}
			return estimate;
		}

		protected virtual bool ShouldDefaultEstimate
		{
			get
			{
				return !inDefaultEstimate
						&& !(this is TemplateProcessTask)
						&& !IsDeleted
						&& (!P9_ScheduledDate.IsValid || P9_RecalculateScheduledDate && !P9_ActualDate.IsValid);
			}
		}

		bool inDefaultEstimate;
		protected bool IsDefaultingFromSameEvent { get; private set; }

		bool IsCycledEventLog(ZDateTimeOffset newDate)
		{
			if (IsInDatabase)
			{
				var loopLimit = WorkflowDataRegistry.Instance.EstimateCalculationCycleLimitBeforeDisable.Value;
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CalculatedEstimateCode)
				{
					OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC",
					MaximumRows = loopLimit
				}
				.AddToFilter(StmALogSchema.SL_IsEstimate, false)
				.AddToFilter(StmALogSchema.SL_Parent, PK)
				.AddToFilter(StmALogSchema.SL_Table, ProcessTasksSchema.Constants.TableName);

				query.TableIndexHints.Add(new TableIndexHint("NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime"));
				query.TableHints = TableHints.FORCESEEK;

				var lastSixLogs = Factory.Load<StmALog>(query)
					.Where(log => log.SL_PostedTimeUtc > ZDateTime.UtcNow.AddMinutes(-60))
					.Select(s => s.SL_EventTime).ToList();

				if (lastSixLogs.Count == loopLimit)
				{
					var logs = new[] { newDate.ToZDateTime() }.Concat(lastSixLogs);
					var spans = logs.SelectSequencedPairs((s1, s2) => Math.Abs((s2 - s1).TotalSeconds)).ToList();
					return spans.IsRepeatingPattern();
				}
			}
			return false;
		}

		internal ZQuery GetMarkExceptionsAsActionedQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(ProcessTasksSchema.P9_ParentID, P9_ParentID);
			query.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.ExceptionType);
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, P9_SE_NKMilestoneEvent);
			query.AddToFilter(ProcessTasksSchema.P9_Description, P9_Description);

			return query;
		}

		internal void MarkExceptionsAsActioned()
		{
			if (!P9_SE_NKMilestoneEvent.IsEmpty && IsMilestone)
			{
				var exceptions = Factory.Load<ProcessTask>(GetMarkExceptionsAsActionedQuery());

				foreach (ProcessTask exception in exceptions)
				{
					exception.IsExceptionActioned = true;
				}
			}
		}

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges => true;

		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges => true;

		public bool IsCreatedFromTemplateDuringSaving { get; set; }

		protected override ZString CustomLogReferenceSuffix
		{
			get { return IsTask ? (Res.GetString("2cf180f5-09e5-4437-9156-fdd0da46e6cf", "Task {0}", P9_TaskID)) : ""; }
		}

		protected override ProcessTasksValidation GetNewValidation()
		{
			if (IsTask)
			{
				return new ProcessTaskValidation(this);
			}
			else if (IsMilestone || IsWorkflowTrigger)
			{
				return new MilestoneOrTriggerValidation(this);
			}
			else if (IsException)
			{
				return new ExceptionValidation(this);
			}
			else
			{
				return null;
			}
		}

		public new ProcessTaskValidationBase Validation
		{
			get { return (ProcessTaskValidationBase)base.Validation; }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ProcessTaskFetchStrategy(this);
		}

		public bool ShouldAddMilestoneCompletionPivotFetchHintsForLoadChildEditableObjects { get; set; }

		#region LightValidation

		protected override bool EnableLightValidationIfAvailable
		{
			get { return Globals.IsUserInteractive; }
		}

		#endregion

		#region On Loaded

		public override void OnLoaded()
		{
			base.OnLoaded();

			OriginalP9_ScheduledDate = P9_ScheduledDate;
			OriginalP9_IsCalendarItem = P9_IsCalendarItem;
		}

		ZDateTime OriginalP9_ScheduledDate;
		ZBool OriginalP9_IsCalendarItem;

		#endregion

		#region Delete

		readonly Lazy<IProcessTaskHelper> lazyProcessTaskHelper = new Lazy<IProcessTaskHelper>(() => ObjectFactory.Get<IProcessTaskHelper>());
		IProcessTaskHelper ProcessTaskHelper => lazyProcessTaskHelper.Value;

		public override void Delete()
		{
			if (!ProcessTaskHelper.CanDelete(this))
			{
				throw new InvalidOperationException($"Trying to delete a task in service task that NEVER should delete tasks, PK:{PK}, Description:{P9_Description}");
			}

			WorkflowAfterOnSavingBOService.GetWorkflowItemsChangeLogService(Factory).ProcessTaskManualChangeLog(this, WorkflowItemsManualChangeLogService.WorkflowItemActionType.Deleted);
			OnDeleteAction?.Value?.Invoke(this);
			OnDelete?.Invoke(this, EventArgs.Empty);
			ProcessTaskNotificationsWithoutMultipleIndexes.DeleteAll();
			ProcessHeader?.UpdateLastEditTimeIfNotDeleting();

			ProcessTaskDeleteHandler.DeleteRelations(ProcessTaskWrapper);
			ProcessWorkflowException?.Delete();

			base.Delete();
		}

		internal event EventHandler OnDelete;

		[ThreadSafe]
		internal static ThreadLocal<Action<ProcessTask>> OnDeleteAction = new ThreadLocal<Action<ProcessTask>>();

		static partial void HookOnCreateTasksAndMilestonesFromTemplateIfRequired();

		#endregion

		#region ICanDelete Members

		static CanDeleteProvider<ProcessTask> GetCanDeleteBuilder()
		{
			return new CanDeleteProvider<ProcessTask>.Builder()
				.PreventDelete(t => !ProcessTaskSecurityMan.IsAllowDelete(t),
					ProcessTaskSecurityMan.GetPreventDeleteReason)
				.PreventDelete(t => !t.P9_FormFlowType.IsEmpty && t.Parent != null,
					t => ResString.GetMultilingualString("40563909-e33c-4823-97a8-79dc327baec1", "Cannot delete system maintained tasks linked to jobs."))
				.PreventDelete(t => t.P9_TaskCannotBeDeleted && t.Parent != null,
					t => ResString.GetMultilingualString("eb59c1f0-e677-4b98-9ec4-261883d76b60", "Mandatory tasks cannot be deleted."))
				.PreventDelete(t => t.IsNonPersistedRepresentationOfTemplateTrigger,
					GetCannotDeleteNonPersistedTriggersDescription)
				.PreventDelete(t => t.HasIterationLink,
					t => ResString.GetMultilingualString("11A86B16-00C3-48B2-B0D4-4032C4FAF183", "The Task cannot be deleted, because there is at least one Quality Iteration Link referencing it."))
				.Build();
		}

		static MultilingualString GetCannotDeleteNonPersistedTriggersDescription(ProcessTask task)
		{
			var message = ResString.GetMultilingualString("dc836a54-0360-44a1-be9d-a33da88436d0", "This trigger is defined on a Workflow Template and not copied onto this job. It cannot be deleted, but can be excluded using Template Conditions.");
			var template = task.TemplateVersion != null ? task.Factory.Load<ProcessTaskTemplate>(task.TemplateVersion.ParentID) : null;
			if (template != null)
			{
				return MultilingualString.Join(System.Environment.NewLine, message, ResString.GetMultilingualString("9bb2b14b-a3a0-4528-8444-1d1ee0afb80b", "Workflow Template: [{0}]", template.P0_Name));
			}
			else
			{
				return message;
			}
		}

		static readonly Lazy<CanDeleteProvider<ProcessTask>> canDeleteProvider = new Lazy<CanDeleteProvider<ProcessTask>>(GetCanDeleteBuilder, isThreadSafe: true);

		public override bool CanDelete => canDeleteProvider.Value.CanDelete(this);

		public override MultilingualString ReasonForNotAbleToDelete => canDeleteProvider.Value.GetReasonForNotAbleToDelete(this);

		#endregion

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public static ZQuery GetFilterForCurrentCompany()
			{
				ZQuery result = new ZQuery(ProcessTasksSchema.P9_Type,
					new string[]
					{
						Core.Constants.Workflow.MilestoneType,
						Core.Constants.Workflow.WorkflowTriggerType,
						Core.Constants.Workflow.ExceptionType
					});
				result.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_GC, GlbCompany.CurrentCompany.PK);
				return result;
			}

			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			#region CreateTasksAndMilestonesFromTemplateIfRequired

			public static IDisposable WithTextMacroCaching(BusinessObjectFactory factory)
			{
				var mcrCache = MacroEvaluationCache.Enable();
				if (factory.ServiceContainer.GetService<ITextMacroProcessingCachingService>() == null)
				{
					factory.ServiceContainer.AddService(ObjectFactory.Get<ITextMacroProcessingCachingService>(nameof(ITextMacroProcessingCachingService)));
					return new DisposableAction(() =>
					{
						factory.ServiceContainer.RemoveService<ITextMacroProcessingCachingService>();
						mcrCache?.Dispose();
					});
				}

				return mcrCache;
			}

			public static IDisposable SuppressTemplateApplication()
			{
				if (suppressCreatingWorkflowFromTemplate)
				{
					return null;
				}
				suppressCreatingWorkflowFromTemplate = true;
				return new DisposableAction(() => suppressCreatingWorkflowFromTemplate = false);
			}

			public static IDisposable SuppressTemplateApplication(bool suppress)
			{
				return suppress ? SuppressTemplateApplication() : null;
			}

			public static bool SuppressCreatingWorkflowFromTemplate => suppressCreatingWorkflowFromTemplate;

			[ThreadStatic]
			static bool suppressCreatingWorkflowFromTemplate;

			public ApplyWorkflowTemplateResult CreateTasksAndMilestonesFromTemplateIfRequired(IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters = null)
			{
				if (new TemplateApplicationHasChangesStrategy().JobHasChanges(workflowProvider, parameters))
				{
					using (WithTextMacroCaching(Factory))
					{
						if (WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType) == null)
						{
							if ((WorkflowDescriptors.ShouldIncludeWorkflowDescriptor().Invoke(workflowProvider.WorkflowType)))
							{
								var errorMessage = $"WorkflowProvider has no corresponding WorkflowDescriptor. WorkflowType: {workflowProvider.WorkflowType}, Type: {workflowProvider.GetType()}";
								ErrorReporter.ReportOnce("c29cbd64-9075-4bb8-b847-e34ed4cae3b3", errorMessage);
							}
							return ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.InvalidWorkflowProvider);
						}

						using (new ProcessTaskDeleteHook())
						{
							ProcessTask.HookOnCreateTasksAndMilestonesFromTemplateIfRequired();

							if (SuppressCreatingWorkflowFromTemplate)
							{
								return ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.TemplateApplicationSuspended);
							}

							if (!Env.IsValidLogon)
							{
								return ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.InvalidLogin);
							}

							if (workflowProvider is ISometimesWorkflowProvider swf && !swf.ShouldSupportWorkflowTemplateApplication)
							{
								return ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.UnsupportedISometimesWorkflowProvider);
							}

							using (PerformanceStatisticsCollector.StartMonitoring("CreateTasksAndMilestonesFromTemplateIfRequired"))
							using (WorkflowAfterOnSavingBOService.GetWorkflowItemsChangeLogService(Factory).SuppressWorkflowChangeLog())
							{
								var applicationParameters = (workflowProvider as IWorkflowTemplateParameterProvider)?.TemplateApplicationParameters ?? parameters ?? TemplateApplicationParameters.Default;
								return CreateTasksAndMilestonesFromTemplateIfRequiredCore(workflowProvider, applicationParameters);
							}
						}
					}
				}
				return ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.NoChangeMadeToBusinessObject);
			}

			ApplyWorkflowTemplateResult CreateTasksAndMilestonesFromTemplateIfRequiredCore(IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters)
			{
				var workFlowUserContextManager = Factory.ServiceContainer.GetService<WorkflowUserContextManager>();
				var workflowUserContextWrong = ErrorReportWrongUserContext(workFlowUserContextManager);

				var parent = (BusinessObject)workflowProvider;

				if (parent.IsDeleted)
				{
					return ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.JobDeleted);
				}

				if (parent is ICancellable cancellable && cancellable.IsCancelled)
				{
					return ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.JobCancelled);
				}

				var checkTemplatesForCurrentCompany = WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany.Value;
				var templatesToCheck = Array.Empty<ProcessTaskTemplate>();

				using (workflowUserContextWrong ? Env.Instance.SetTemporaryUserContext(workFlowUserContextManager.UserContext) : null)
				{
					if (checkTemplatesForCurrentCompany) //Only load templates for the current company if they will be used further on.
					{
						templatesToCheck = parameters.SpecificTemplatesToApply ?? LoadTemplateMatches(workflowProvider, Factory);
					}
					var templatesBeforeScopeEnforcer = templatesToCheck.Length;

					templatesToCheck = TemplatesWithScopeEnforcer(workflowProvider, templatesToCheck);

					if (!checkTemplatesForCurrentCompany || templatesToCheck.Any())
					{
						using ((parent as EnterpriseBusinessObject)?.CacheBusinessObjectsWithRelatedEvents())
						{
							if (parameters.JobAttributesMayHaveChangedSinceLastTemplateApplication)
							{
								ObjectFactory.Get<IUserDefinedConditionEvaluator>().ClearCache(parent);
							}
							workflowProvider.WorkflowItems.HasReloadedFromDb = false;

							return ApplyWorkflowTemplates(parent, workflowProvider, parameters, templatesToCheck, checkTemplatesForCurrentCompany);
						}
					}

					if (templatesBeforeScopeEnforcer > 0)
					{
						return ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.TemplateScopeEnforcerRestriction);
					}

					return ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.NoMatchingTemplate);
				}
			}

			static bool ErrorReportWrongUserContext(WorkflowUserContextManager workFlowUserContextManager)
			{
				if (workFlowUserContextManager?.UserContext != null && !workFlowUserContextManager.UserContext.Equals(Env.CurrentUserContext))
				{
					var error = new StringBuilder();

					error.Append(FormattableString.Invariant($@"Environment user context ""Company: {Env.CurrentUserContext.Company.Code}, Branch: {Env.CurrentUserContext.Branch.Code}, User: {Env.CurrentUserContext.User.LoginName}""
Workflow user context ""Company: {workFlowUserContextManager.UserContext.Company.Code}, Branch: {workFlowUserContextManager.UserContext.Branch.Code}, User: {workFlowUserContextManager.UserContext.User.LoginName}""

"));

					if (workFlowUserContextManager.Logger != null)
					{
						error.Append(string.Join("\r\n\r\n", workFlowUserContextManager.Logger.Logs.Select(x => x.ToString())));
					}

					ErrorReporter.ReportOnce(FormattableString.Invariant($"Workflow user context has been modified from its expected value.\r\n {error.ToString()}"));
					return true;
				}

				return false;
			}

			[Obsolete("This should never be used. Instead use TemplateApplicationParameters.jobAttributesMayHaveChangedSinceLastTemplateApplication. This method is here only for ZClientEDI which hacks around with Workflow internals :(")]
			public static void ClearUserDefinedConditionCache(IBusiness workflowParent)
			{
				ObjectFactory.Get<IUserDefinedConditionEvaluator>().ClearCache(workflowParent);
			}

			ProcessTaskTemplate[] TemplatesWithScopeEnforcer(IWorkflowProvider workflowProvider, IEnumerable<ProcessTaskTemplate> templates)
			{
				var scopeEnforcer = workflowProvider.GetWorkflowTemplateScopeEnforcer();
				return templates.Where(template => scopeEnforcer.ShouldApplyTemplate(workflowProvider, template, Factory)).ToArray();
			}

			#endregion

			#region Apply Templates

			ApplyWorkflowTemplateResult ApplyWorkflowTemplates(BusinessObject parent, IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters, IEnumerable<ProcessTaskTemplate> templates, bool checkTemplatesForCurrentCompany)
			{
				var result = new ApplyWorkflowTemplateResult.Builder(workflowProvider);
				var foundTemplateCandidates = false;
				if (checkTemplatesForCurrentCompany)
				{
					result.Add(ApplyWorkflowTemplatesCore(parent, workflowProvider, parameters, templates));
				}
				else
				{
					foreach (var context in TemplateApplicationUserContextProvider.GetTemplateUserContexts(workflowProvider, Factory))
					{
						using (Env.SetTemporaryUserContext(context))
						{
							templates = parameters.SpecificTemplatesToApply ?? LoadTemplateMatches(workflowProvider, Factory);
							foundTemplateCandidates = foundTemplateCandidates || templates.Any();

							var scopeEnforcer = workflowProvider.GetWorkflowTemplateScopeEnforcer();
							templates = TemplatesWithScopeEnforcer(workflowProvider, templates);

							if (templates.Any())
							{
								result.Add(ApplyWorkflowTemplatesCore(parent, workflowProvider, parameters, templates));
							}
						}
					}
				}
				return foundTemplateCandidates && result.ResultsCount == 0 ? ApplyWorkflowTemplateResult.Empty(TemplateApplicationResult.TemplateScopeEnforcerRestriction) : result.Build();
			}

			IEnumerable<CreateItemsFromTemplateResult> ApplyWorkflowTemplatesCore(BusinessObject parent, IWorkflowProvider workflowProvider, TemplateApplicationParameters parameters, IEnumerable<ProcessTaskTemplate> templates)
			{
				var applicators = GetWorkflowTemplateApplicators(parent, workflowProvider, templates, parameters).ToArray();
				if (applicators.Any())
				{
					return applicators.SelectMany(applicator => applicator.ApplyTemplates(templates, parameters)).ToArray();
				}
				else
				{
					return Enumerable.Empty<CreateItemsFromTemplateResult>();
				}
			}

			public IEnumerable<IWorkflowTemplateApplicator> GetWorkflowTemplateApplicators(BusinessObject parent, IWorkflowProvider workflowProvider, IEnumerable<ProcessTaskTemplate> templatesToCheck, TemplateApplicationParameters parameters)
			{
				foreach (var template in templatesToCheck)
				{
					Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, template.PK);
				}

				if (parameters.CanApply(TemplateEntityType.Tasks))
				{
					yield return workflowProvider.WorkflowItems.Tasks;
				}

				if (parameters.CanApply(TemplateEntityType.Tasks) || parameters.CanApply(TemplateEntityType.ReleaseGroupRules))
				{
					yield return new ReleaseGroupRulesTemplateApplicator(workflowProvider);
				}

				if (parameters.CanApply(TemplateEntityType.Milestones))
				{
					yield return workflowProvider.WorkflowItems.Milestones;
				}

				if (parameters.CanApply(TemplateEntityType.Triggers))
				{
					yield return workflowProvider.WorkflowItems.Triggers;
				}

				if (parameters.CanApply(TemplateEntityType.Workflows))
				{
					yield return ObjectFactory.Get<IWorkflowLinkTemplateApplicator>("IWorkflowLinkTemplateApplicator", workflowProvider);
				}
			}

			#endregion

			#region Load Matching Templates

			public static ProcessTaskTemplate[] LoadTemplateMatches(IWorkflowProvider workflowProvider, BusinessObjectFactory factory, bool includeUniversalTemplates = false, bool includeOnlyUniversalTemplates = false)
			{
#if DEBUG
				OnLoadTemplateMatches_ForTest.Value?.Invoke();
#endif
				return new ProcessTaskTemplate.Loader(factory).FindMatches(workflowProvider, includeUniversalTemplates: includeUniversalTemplates, includeOnlyUniversalTemplates: includeOnlyUniversalTemplates);
			}

			#endregion

			#region HasMilestones / HasExceptions

			public static bool ShouldCreateTasksFromTemplate(BusinessObject parent, IWorkflowProvider workflowProvider)
			{
				var areTasksCompanySpecific = workflowProvider.WorkflowItems.AreTasksCompanySpecific;

				foreach (var task in workflowProvider.WorkflowItems.Tasks.Cast<ProcessTask>())
				{
					if (areTasksCompanySpecific && (task.P9_GC != GlbCompany.CurrentCompany.PK))
					{
						continue;
					}

					if (!parent.IsInDatabase
						&& WorkflowDataRegistry.Instance.AlwaysApplyTasksFromTemplateWhenFirstSavingJob.Value
						&& task.P9_ParentTemplateID.IsEmpty)
					{
						continue;
					}

					return false;
				}

				return true;
			}

			bool HasMilestonesForCurrentCompany(IWorkflowProvider workflowProvider)
			{
				return workflowProvider.WorkflowItems.Milestones.Cast<ProcessTask>().Any(t => t.P9_GC == GlbCompany.CurrentCompany.PK && t.TriggerConditions.TriggerContextCode != TriggerUserContextList.Codes.Specified);
			}

			public static bool ShouldCreateTriggersFromTemplate(IWorkflowProvider workflowProvider)
			{
				return workflowProvider.WorkflowItems.Triggers.Cast<ProcessTask>().All(t => t.IsNonPersistedRepresentationOfTemplateTrigger || t.P9_GC != GlbCompany.CurrentCompany.PK || t.TriggerConditions.TriggerContextCode == TriggerUserContextList.Codes.Specified);
			}

			#endregion

			#region BusinessObject.Loader Overrides

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(ProcessTask);
			}

			#endregion

			#region Testing
#if DEBUG
			public static Overridable<Action> OnLoadTemplateMatches_ForTest { get; } = new Overridable<Action>();
#endif
			#endregion
		}

		#endregion

		#region Property Overrides

		#region P9_FC_CurrentComponent

		[ReadOnly(true)]
		public override ZGuid P9_FC_CurrentComponent
		{
			get => base.P9_FC_CurrentComponent;
			set => base.P9_FC_CurrentComponent = value;
		}

		#endregion

		#region P9_OriginalScheduledDateUtc

		[ReadOnlyMember(nameof(P9_OriginalScheduledDateUtc_ReadOnly))]
		public override ZDateTime P9_OriginalScheduledDateUtc
		{
			get => base.P9_OriginalScheduledDateUtc;
			set => base.P9_OriginalScheduledDateUtc = value;
		}

		protected bool P9_OriginalScheduledDateUtc_ReadOnly => DisallowCreateEventBySettingDates;

		#endregion

		#region P9_FH_ProcessHeader

		[List("Lookups.ProcessHeaders")]
		[RelatedBusinessObject("ProcessHeader")]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject ProcessHeader (IProcessHeader) is an interface")]
		[ReadOnlyMember(nameof(P9_FH_ProcessHeader_ReadOnly))]
		public override ZGuid P9_FH_ProcessHeader
		{
			get => base.P9_FH_ProcessHeader;
			set
			{
				if (value.IsValid && !IsTask)
				{
					throw new InvalidOperationException("Only tasks can belong to a workflow");
				}

				var originalValue = P9_FH_ProcessHeader;
				var hasChanged = originalValue != value;
				var originalProcessHeader = hasChanged ? ProcessHeader : null;

				if (originalProcessHeader != null)
				{
					originalProcessHeader.FH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
				}

				base.P9_FH_ProcessHeader = value;

				if (hasChanged)
				{
					if (originalProcessHeader != null)
					{
						Iteration = ZString.Empty;
					}

					UpdateIterationPivots();
				}

				if (!IsValidationSuspended)
				{
					if (IsOpen)
					{
						ProcessHeader?.Validation.ValidateFH_GG_ReleaseGroup();
					}
				}
			}
		}

		public bool P9_FH_ProcessHeader_ReadOnly => !IsTask;

		#endregion

		#region P9_OA
		[List("Lookups.Addresses")]
		public override ZGuid P9_OA
		{
			get { return base.P9_OA; }
			set { base.P9_OA = value; }
		}
		#endregion

		#region Exception View Model

		public ExceptionViewModel ExceptionProperties
		{
			get
			{
				if (exceptionViewModel == null)
				{
					if (!IsException)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Accessing exception properties for {0}", P9_Type));
					}

					exceptionViewModel = new ExceptionViewModel(this);
				}

				return exceptionViewModel;
			}
		}

		ExceptionViewModel exceptionViewModel;

		public class ExceptionViewModel
		{
			internal ExceptionViewModel(ProcessTask exception)
			{
				this.exception = exception;
			}

			readonly ProcessTask exception;

			public ZDateTime ActualDate
			{
				get { return exception.P9_ActualDate; }
				set { exception.P9_ActualDate = value; }
			}
		}

		#endregion

		#region Task View Model

		public TaskViewModel TaskProperties
		{
			get
			{
				if (taskViewModel == null)
				{
					if (!IsTask)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Accessing task properties for {0}", P9_Type));
					}

					taskViewModel = new TaskViewModel(this);
				}

				return taskViewModel;
			}
		}

		TaskViewModel taskViewModel;

		public class TaskViewModel
		{
			internal TaskViewModel(ProcessTask task)
			{
				this.task = task;
			}

			readonly ProcessTask task;

			public ZDateTimeOffset ActualDate
			{
				get { return task.P9_ActualDateInternal; }
				set { task.P9_ActualDateInternal = value; }
			}

			public ZDateTimeOffset ScheduledDate
			{
				get { return new ZDateTimeOffset(task.P9_ScheduledDate); }
				set { task.P9_ScheduledDate = value.ToZDateTime(); }
			}
		}

		#endregion

		#region Trigger Properties

		public TriggerViewModel TriggerProperties
		{
			get
			{
				if (triggerViewModel == null)
				{
					if (!IsMilestoneOrWorkflowTrigger)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Accessing wrong properties for {0}", P9_Type));
					}

					triggerViewModel = new TriggerViewModel(this);
				}

				return triggerViewModel;
			}
		}

		TriggerViewModel triggerViewModel;

		public class TriggerViewModel
		{
			internal TriggerViewModel(ProcessTask task)
			{
				this.task = task;
			}

			readonly ProcessTask task;

			public ZDateTimeOffset ActualDate
			{
				get { return new ZDateTimeOffset(task.P9_ActualDate); }
			}

			public ZDateTimeOffset ScheduledDate
			{
				get { return new ZDateTimeOffset(task.P9_ScheduledDate); }
			}
		}

		#endregion

		#region Trigger Conditions

		public TriggerConditionsViewModel TriggerConditions
		{
			get
			{
				if (triggerConditions == null)
				{
					triggerConditions = new ProcessTaskTriggerConditionsViewModel(this);
					RegisterEditableChildObject(triggerConditions);
				}

				return triggerConditions;
			}
		}

		TriggerConditionsViewModel triggerConditions;

		#region P9_TriggerCondition

		[ReadOnly(true)]
		[CustomisedControlExclude]
		[BusinessObjectTestExclude] // Can't set this property without using ITriggerConditions interface.
		public override ZString P9_TriggerCondition
		{
			get { return base.P9_TriggerCondition; }
			set
			{
				EnsureCanSetTriggerCondition(nameof(P9_TriggerCondition));

				if (base.P9_TriggerCondition != value)
				{
					base.P9_TriggerCondition = value;
				}
			}
		}

		#endregion

		#region P9_TriggerField

		[ReadOnly(true)]
		[CustomisedControlExclude]
		[BusinessObjectTestExclude] // Can't set this property without using ITriggerConditions interface.
		public override ZString P9_TriggerField
		{
			get { return base.P9_TriggerField; }
			set
			{
				EnsureCanSetTriggerCondition(nameof(P9_TriggerField));

				base.P9_TriggerField = value;
			}
		}

		#endregion

		#endregion

		#region Template Conditions

		public TemplateConditionsViewModel TemplateConditions
		{
			get
			{
				if (templateConditions == null)
				{
					templateConditions = new TemplateConditionsViewModel(this, Parent as ProcessTaskTemplate);
					RegisterEditableChildObject(templateConditions);
				}

				return templateConditions;
			}
		}

		TemplateConditionsViewModel templateConditions;

		[ReadOnly(true)]
		[CustomisedControlExclude]
		[BusinessObjectTestExclude] // Can't set this property without using ITemplateConditionalWorkflowItem interface.
		public override ZString P9_Condition1
		{
			get { return base.P9_Condition1; }
			set
			{
				EnsureCanSetTemplateCondition(nameof(P9_Condition1));

				base.P9_Condition1 = value;
			}
		}

		[ReadOnly(true)]
		[CustomisedControlExclude]
		[BusinessObjectTestExclude] // Can't set this property without using ITemplateConditionalWorkflowItem interface.
		public override ZString P9_Condition2
		{
			get { return base.P9_Condition2; }
			set
			{
				EnsureCanSetTemplateCondition(nameof(P9_Condition2));

				if (ProcessTasksLookups.IsMacroCondition(value) && !P9_Condition2Value.IsEmpty)
				{
					using (TemporarilyAllowSettingCondition(nameof(P9_Condition2Value)))
					{
						P9_Condition2Value = ZString.Empty;
					}
				}

				base.P9_Condition2 = value;
			}
		}

		#region P9_Condition2Value

		[ReadOnly(true)]
		[CustomisedControlExclude]
		[BusinessObjectTestExclude] // Can't set this property without using ITemplateConditionalWorkflowItem interface.
		public override ZString P9_Condition2Value
		{
			get
			{
				if (ProcessTasksLookups.IsMacroCondition(P9_Condition2))
				{
					return UdfConditionHiddenNote.Text;
				}
				else
				{
					return base.P9_Condition2Value;
				}
			}
			set
			{
				EnsureCanSetTemplateCondition(nameof(P9_Condition2Value));

				if (ProcessTasksLookups.IsMacroCondition(P9_Condition2))
				{
					if (value.IsEmpty)
					{
						if (UdfConditionHiddenNote.HasNote)
						{
							UdfConditionHiddenNote.Delete();
							base.P9_Condition2Value = ZString.Empty;
						}
					}
					else
					{
						UdfConditionHiddenNote.Text = value;
						base.P9_Condition2Value = ZString.Empty;
					}

					if (IsMilestoneOrWorkflowTrigger)
					{
						P9_Condition2ValueHash = CalculateUdfHash();
					}
				}
				else
				{
					base.P9_Condition2Value = value;
				}
			}
		}

		/// <summary>
		/// This property is used for merging tasks from different ProcessTaskTemplate.
		/// Instead of copying an StmNote containing the same UDF condition over and over
		/// We can instead compare these hash values. (For non->udf this can just be the value of P9_Condition2Value)
		/// Please Note: This is a foul abuse and reuse of an existing column for a nefarious end.
		/// </summary>
		public ZString P9_Condition2ValueHash
		{
			set
			{
				if (ShouldUseCondition2Hash)
				{
					P9_CardNote = value;
				}
				else
				{
					throw new InvalidOperationException("Because this is a foul reuse of an existing column you should not be setting this.");
				}
			}
			get
			{
				if (ShouldUseCondition2Hash)
				{
					var initialHash = P9_CardNote;
					if (!initialHash.IsEmpty)
					{
						return initialHash;
					}
					else
					{
						P9_CardNote = CalculateUdfHash();
						if (!IsTemplate && UdfConditionHiddenNote.HasNote) // Quietly clean up old data when we see it.
						{
							UdfConditionHiddenNote.Delete();
						}
						return P9_CardNote;
					}
				}
				else
				{
					throw new InvalidOperationException("Should only be checking the Hash of UDF type triggers and milestones");
				}
			}
		}

		internal bool ShouldUseCondition2Hash => ProcessTasksLookups.IsMacroCondition(P9_Condition2) && IsMilestoneOrWorkflowTrigger;

		string CalculateUdfHash()
		{
			var baseValue = P9_Condition2Value;
			var hash = Convert.ToInt32(Murmur3Hash.ComputeStringFast(baseValue));
			var len = Convert.ToInt32(baseValue.Length);
			return hash.ToString("D10", CultureInfo.InvariantCulture) + len.ToString("D10", CultureInfo.InvariantCulture);
		}

		internal HiddenUdfConditionNote UdfConditionHiddenNote
		{
			get { return udfConditionHiddenNote ?? (udfConditionHiddenNote = new HiddenUdfConditionNote(this)); }
		}
		HiddenUdfConditionNote udfConditionHiddenNote;

		internal class HiddenUdfConditionNote : HiddenTextNote, IDeletableItem
		{
			public HiddenUdfConditionNote(ProcessTask parent) : base(parent, true) { }

			protected override ZString Description
			{
				get { return (NoResString)"User Defined Condition"; }
			}
		}

		#endregion

		#endregion

		#region P9_TaskCanNotBeDeleted

		protected bool P9_TaskCannotBeDeleted_ReadOnly
		{
			get { return !(this is TemplateProcessTask); }
		}

		#endregion

		#region P9_Description

		[ReadOnlyMember(nameof(P9_Description_ReadOnly))]
		public override ZString P9_Description
		{
			get { return base.P9_Description; }
			set
			{
				base.P9_Description = value;
				if (IsTask && P9_Type.IsEmpty)
				{
					P9_Type = Core.Constants.Workflow.UndefinedTaskType;
				}
			}
		}

		public bool P9_Description_ReadOnly
		{
			get { return IsCompletionStatement; }
		}

		#endregion

		#region P9_CardNote

		[ReadOnlyMember(nameof(IsMilestoneOrWorkflowTrigger))]
		public override ZString P9_CardNote
		{
			get => base.P9_CardNote;
			set => base.P9_CardNote = value;
		}

		#endregion

		#region P9_Notes

		[CustomisedControlExclude]
		public override ZBlob P9_Notes
		{
			get => base.P9_Notes;
			set
			{
				// When current value is empty check if the new value contains only rtf style information
				if (!base.P9_Notes.IsEmpty || !string.IsNullOrEmpty(ConvertZBlobToPlainText(value)))
				{
					base.P9_Notes = value;
					p9_NotesAsPlainText = ZString.Empty;
				}
			}
		}

		string ConvertZBlobToPlainText(ZBlob rtf)
		{
			var rawString = BlobAsString(rtf);
			if (!rawString.StartsWith(@"{\rtf", StringComparison.OrdinalIgnoreCase))
			{
				return rawString;
			}

			return ORtfTextUtil.RtfToText(rawString);
		}

		public override ZPropertyInfo P9_NotesInfo
		{
			get
			{
				var info = base.P9_NotesInfo;
				info.ValueChanged -= P9_NotesValueChangedHandler;
				info.ValueChanged += P9_NotesValueChangedHandler;
				return info;
			}
		}

		public static string GetAsRtfFormatted(string input)
		{
			return ORtfTextUtil.TextToRtf(input);
		}

		static void P9_NotesValueChangedHandler(object sender, EventArgs args)
		{
			if (sender is ProcessTask task && task.IsTask && args is ConcurrencyValueChangedEventArgs valueChangedArgs)
			{
				const string conflictSymbol = ">>>>>";

				var currentHeader = GetAsRtfFormatted(conflictSymbol + Res.GetString("f6fb3d15-2b1e-417e-8447-0845be2a7ff0", "Concurrency Error Current Value"));
				var currentrtf = ORtfTextUtil.AppendRtfStrings(currentHeader, BlobAsString(new ZBlob(valueChangedArgs.CurrentValue)));

				var oldHeader = GetAsRtfFormatted(conflictSymbol + Res.GetString("8e97f0b3-3eb1-40bd-abf0-b413047434b3", "Concurrency Error Old Value"));
				var oldrtf = ORtfTextUtil.AppendRtfStrings(oldHeader, BlobAsString(new ZBlob(valueChangedArgs.OldValue)));

				task.P9_NotesAsString = ORtfTextUtil.AppendRtfStrings(currentrtf, oldrtf);
			}
		}

		#endregion

		#region P9_NotesAsPlainText

		[ResourceStringData("ProcessTask.P9_NotesAsPlainText", Caption = "Description")]

		ZString p9_NotesAsPlainText = ZString.Empty;

		public ZString P9_NotesAsPlainText
		{
			get {
				if (p9_NotesAsPlainText == ZString.Empty)
				{
					p9_NotesAsPlainText = ConvertZBlobToPlainText(P9_Notes);
				}
				return p9_NotesAsPlainText;
			}
		}

		#endregion

		#region P9_NotesAsString

		[CustomisedControlExclude]
		[ResourceStringData("ProcessTask.P9_NotesAsString", Caption = "Description")]
		public ZString P9_NotesAsString
		{
			get
			{
				return BlobAsString(P9_Notes);
			}
			set
			{
				using (var stream = new MemoryStream())
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(value);
					writer.Flush();
					P9_Notes = new ZBlob(stream.ToArray());
				}

				if (IsCompletionStatement)
				{
					if (value.Length > ProcessTasksSchema.P9_Description.MaxLength)
					{
						value = value.SubstringSafe(0, ProcessTasksSchema.P9_Description.MaxLength - 3) + "...";
					}

					P9_Description = value;
				}
			}
		}

		static string BlobAsString(ZBlob blob)
		{
			using (var stream = new MemoryStream(blob))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		public ZPropertyInfo P9_NotesAsStringInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(P9_NotesAsString), _ => P9_NotesInfo); } // This is a calculated property
		}

		#endregion

		#region P9_Status

		string temporaryStatusChangeMode;
		public IDisposable SetTemporaryStatusChangeMode(string statusChangeMode)
		{
			temporaryStatusChangeMode = statusChangeMode;
			return new DisposableAction(() => temporaryStatusChangeMode = null);
		}

		ProcessTaskStatusChangeModeTracker ProcessTaskStatusChangeModeTracker =>
			processTaskStatusChangeModeTracker ?? (processTaskStatusChangeModeTracker = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>());
		ProcessTaskStatusChangeModeTracker processTaskStatusChangeModeTracker;

		string StatusChangeMode => temporaryStatusChangeMode ?? ProcessTaskStatusChangeModeTracker.Current;

		static bool StatusEq(string status1, string status2) => StringComparer.OrdinalIgnoreCase.Equals(status1, status2);

		void ChangeStatus(ZString value, ref ZString oldStatus, ref ZString newStatus)
		{
			if ((!IsTask && !IsException) || IsTemplateTask)
			{
				SetStatusCore(value);
				return;
			}

			ProcessTaskStatusChanger.ChangeStatusAsync(ProcessTaskWrapper, newStatus, oldStatus, StatusChangeMode).GetAwaiter().GetResult();
		}

		internal ProcessTaskWrapper ProcessTaskWrapper => processTaskWrapper ?? (processTaskWrapper = new ProcessTaskWrapper(this));
		ProcessTaskWrapper processTaskWrapper;

		ProcessTaskStatusChanger<ProcessTaskWrapper> ProcessTaskStatusChanger =>
			processTaskStatusChanger ?? (processTaskStatusChanger = new ProcessTaskStatusChanger<ProcessTaskWrapper>(
				new ProcessTaskSecuritySettings(),
				new ProcessTaskDateTimeProvider(),
				new WorkingTaskProvider(),
				new ProcessTaskLogLoader()));

		ProcessTaskStatusChanger<ProcessTaskWrapper> processTaskStatusChanger;

		[List("Lookups.Statuses")]
		[ResourceStringData("ProcessTask.P9_Status", Caption = "Exception Status", ShortCaption = "Status", IsApplicableMember = nameof(IsException))]
		public override ZString P9_Status
		{
			get { return base.P9_Status; }
			set
			{
				var oldStatus = base.P9_Status;

				if (IsException && !isSettingExceptionActioned)
				{
					ErrorReporter.ReportOnce("ExceptionStatusSet", "Exceptions should not be actioned by setting P9_Status. Instead, use IsExceptionActioned.");
				}

				if (StatusChangeMode == ProcessTaskStatusChangeModeCodeList.Codes.TriggerOrOtherAutomation && value == ProcessTaskStatusCodes.Working && !P9_GS_NKAssignedStaffMember.IsEmpty)
				{
					var query = @$"FROM {ProcessTasksSchema.Constants.SqlSchemaName}.{ProcessTasksSchema.Constants.TableName}
WHERE {ProcessTasksSchema.Constants.P9_GS_NKAssignedStaffMember} = @StaffCode
AND {ProcessTasksSchema.Constants.P9_Status} = '{ProcessTaskStatusCodes.Working}'
AND {ProcessTasksSchema.Constants.PK} <> '{PK}'";

					if (Db.Connection.Exists(query,
						cmd => cmd.AddParameter("@StaffCode", ProcessTasksSchema.P9_GS_NKAssignedStaffMember.SqlDbType, P9_GS_NKAssignedStaffMember.ToString())))
					{
						value = ProcessTaskStatusCodes.Suspended;
					}
				}

				var newStatus = value;

				if (StatusEq(oldStatus, newStatus))
				{
					return;
				}

				ChangeStatus(value, ref oldStatus, ref newStatus);

				if (!IsValidationSuspended)
				{
					Validation.ValidateP9_EstDuration();
					Validation.ValidateP9_ActualDuration();

					if (IsException)
					{
						Validation.ValidateP9_GS_NKAssignedStaffMember();
						Validation.ValidateP9_GG_AssignedGroup();
						Validation.ValidateExceptionCausePK();
						Validation.ValidateExceptionResolutionPK();
					}
				}
			}
		}

		protected virtual bool P9_Status_ReadOnly
		{
			get { return IsMilestone || IsException; }
		}

		public void AppendNote(string note, bool addInitialsAndDateTime = true)
		{
			if (addInitialsAndDateTime)
			{
				note = EnvProxy.Instance.CurrentUser.InitialsAndDateTime + note;
			}

			P9_Notes = ORtfTextUtil.ConcatRtfString(P9_Notes, note);
		}

		void CalculateDurationFromStartChange(ZDateTimeOffset oldStart, ZDateTimeOffset newStart)
		{
			if (P9_ActualDuration.IsValid && oldStart.IsValid && newStart.IsValid)
			{
				var durationTracker = new DurationTracker<ProcessTaskLogger.StatusSpan>(WorkDaysHelper);
				if (newStart > oldStart)
				{
					var span = durationTracker.GetDuration(TimeSpan.Zero, new[] { new ProcessTaskLogger.StatusSpan("", oldStart, newStart) });
					P9_ActualDuration = TaskDurationCalculator.AddToDurationSafe(P9_ActualDuration, -span);
				}
				else
				{
					var span = durationTracker.GetDuration(TimeSpan.Zero, new[] { new ProcessTaskLogger.StatusSpan("", newStart, oldStart) });
					P9_ActualDuration = TaskDurationCalculator.AddToDurationSafe(P9_ActualDuration, span);
				}
			}
		}

		internal void OnTaskOwnerPasswordRequested(PasswordRequestEventArgs args)
		{
			if (TaskOwnerPasswordRequested != null)
			{
				TaskOwnerPasswordRequested(this, args);
			}
		}

		public class PasswordRequestEventArgs : EventArgs
		{
			public PasswordRequestEventArgs(string loginName)
			{
				this.loginName = loginName;
			}

			public readonly string loginName;
			public bool IsValidPassword;
		}

		public event EventHandler<PasswordRequestEventArgs> TaskOwnerPasswordRequested;

		bool suspendValidationOnTaskCancellation;

		public bool IsValidationOnTaskCancellationSuspended => suspendValidationOnTaskCancellation;

		public void CancelAndSuspendValidationOnTaskCancellation()
		{
			suspendValidationOnTaskCancellation = true;
			P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			if (P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
			{
				ResumeValidationOnTaskCancellation();
			}
		}

		internal void ResumeValidationOnTaskCancellation()
		{
			suspendValidationOnTaskCancellation = false;
		}

		#endregion

		#region Maybe Assign to current user

		protected internal virtual bool ShouldPromptToResumeSuspendedTasks
		{
			get { return true; }
		}

		[ZDateTimeDurationValue]
		public ZDateTime OverrideActualDuration
		{
			get { return fOverrideActualDuration; }
			set { SetNonPersistentPropertyValue(OverrideActualDurationInfo, ref fOverrideActualDuration, value.ConvertToDurationBasedDate(OverrideActualDurationInfo)); }
		}

		ZDateTime fOverrideActualDuration;

		public ZPropertyInfo OverrideActualDurationInfo
		{
			get { return GetZPropertyInfo(nameof(OverrideActualDuration)); }
		}

		protected internal void SetStatusCore(ZString status)
		{
			base.P9_Status = status.ToUpperInvariant();
		}

		ProcessTask GetTaskForStaff(GlbStaff staff, ZString code)
		{
			ZQuery query = new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff.GS_Code);
			query.AddToFilter(ProcessTasksSchema.P9_Status, code);
			query.AddToFilter(ProcessTasksSchema.PK, SQLComparisonOperator.NotEqual, PK);

			return Factory.LoadTop1<ProcessTask>(query);
		}

		public bool ContainmentBarrierStatusChangeResponderSupressed { get; private set; }

		public IDisposable SupressContainmentBarrierStatusChangeResponder()
		{
			ContainmentBarrierStatusChangeResponderSupressed = true;

			return new DisposableAction(() => ContainmentBarrierStatusChangeResponderSupressed = false);
		}

		internal virtual bool DoStatusChangeRespondersAllowStatusChange(ZString status)
		{
			foreach (IStatusChangeResponder responder in ObjectFactory.Get<System.Collections.IEnumerable>("TaskStatusChangeResponders"))
			{
				if (responder.RespondsToStatusChange(this, status))
				{
					var result = responder.RespondToChange(this, status);
					if (result == StatusChangeResult.ChangeNotHandled)
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		#region Is Estimate

		public ZBool ShouldTriggerOnEstimateEvents
		{
			get { return fShouldTriggerOnEstimateEvents; }
			set { SetNonPersistentPropertyValue(ShouldTriggerOnEstimateEventsInfo, ref fShouldTriggerOnEstimateEvents, value); }
		}

		ZBool fShouldTriggerOnEstimateEvents;

		public ZPropertyInfo ShouldTriggerOnEstimateEventsInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldTriggerOnEstimateEvents)); }
		}

		#endregion

		#region P9_TaskID

		[ReadOnly(true)]
		public override ZString P9_TaskID
		{
			get { return base.P9_TaskID; }
			set { base.P9_TaskID = value; }
		}

		#endregion

		#region P9_Type

		[List("Lookups.Types")]
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString P9_Type
		{
			get { return base.P9_Type; }
			set
			{
				if (P9_Type != value)
				{
					base.P9_Type = value;

					using (GetValidationSuspender())
					{
						if (!IsTask && !P9_FH_ProcessHeader.IsEmpty)
						{
							P9_FH_ProcessHeader = ZGuid.Empty;
						}

						if (Lookups.Types.ContainsCode(P9_Type))
						{
							P9_IsCalendarItem = Lookups.Types.GetBoolFromCode(P9_Type);
						}

						if (IsMilestone && P9_SE_NKExceptionEvent.IsEmpty)
						{
							P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily;
						}

						if (IsMilestone || IsWorkflowTrigger || IsException)
						{
							P9_EstDuration = ZDateTime.Empty;
						}
						else
						{
							TriggerConditions.TriggerEventCode = ZString.Empty;
							P9_SE_NKExceptionEvent = ZString.Empty;
						}

						if (IsTask && !P9_GS_NKAssignedStaffMember.IsEmpty && P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
						{
							P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
						}

						SetExceptionLocation();
					}

					SetDescriptionFromType();
					P9_ActualDateInfo.RefreshBinding();
				}
			}
		}

		protected virtual void SetExceptionLocation() { }

		protected virtual void SetDescriptionFromType()
		{
			if (P9_Description.IsEmpty && !TypeDescription.IsEmpty)
			{
				P9_Description = TypeDescription.Substring(0, P9_DescriptionInfo.MaxLength - 1);
			}
		}

		protected bool P9_Type_ReadOnly
		{
			get { return IsMilestone || IsException; }
		}

		#endregion

		#region P9_GS_NKAssignedStaffMember

		[List("Lookups.AssignedStaffMembers")]
		[ResourceStringData("ProcessTask.P9_GS_NKAssignedStaffMember", Caption = "Exception Assigned To", ShortCaption = "Staff", FullDescription = "The assigned or responsible staff member. When 'Reminder' is ticked, the calendar item is placed in the staff member's calendar.", IsApplicableMember = nameof(IsException))]
		public override ZString P9_GS_NKAssignedStaffMember
		{
			get { return base.P9_GS_NKAssignedStaffMember; }
			set
			{
				if (base.P9_GS_NKAssignedStaffMember != value)
				{
					var @continue = true;
					var newStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, value);
					if (IsTask && newStaff != null)
					{
						if (newStaff.GS_IsSystemAccount && !Globals.IsUserInteractive)
						{
							@continue = false;
						}
						else if (P9_Status == ProcessTaskStatusCodeList.Codes.Working && GetTaskForStaff(newStaff, ProcessTaskStatusCodeList.Codes.Working) != null)
						{
							@continue = ProcessTaskStatusChanger.CanContinueAfterSuspendingExistingWorkingTaskAsync(ProcessTaskWrapper, newStaff.GS_Code, StatusChangeMode).GetAwaiter().GetResult();
						}
					}

					if (@continue)
					{
						SetAssignedStaffMemberAndUpdateRelatedTasks(value);
					}

					if (IsException && exceptionStaffWasAutoAssigned)
					{
						exceptionStaffWasAutoAssigned = false;
					}
				}
			}
		}

		public bool AssignmentRestrictionValidationSuspended { get; private set; }

		public IDisposable SuspendAssignmentRestrictionValidation()
		{
			var workflowItems = Parent?.WorkflowItems;

			if (workflowItems == null)
			{
				return DisposableAction.NoAction;
			}

			var tasks = workflowItems.Where(t => t != this).ToArray();
			tasks.ForEach(t => t.AssignmentRestrictionValidationSuspended = true);

			return new DisposableAction(() => tasks.ForEach(t => t.AssignmentRestrictionValidationSuspended = false));
		}

		void SetAssignedStaffMemberAndUpdateRelatedTasks(ZString value)
		{
			SetP9_GS_NKAssignedStaffMemberCore(value);
			RelatedTaskPKsFromLastTaskAutoAssignment = TaskAssignmentHelper.PopulateRelatedTasksWithSameResourceAndReport(this);
		}

		internal void SetP9_GS_NKAssignedStaffMemberCore(ZString value)
		{
			base.P9_GS_NKAssignedStaffMember = value;

			if (IsTask && !P9_GS_NKAssignedStaffMember.IsEmpty &&
				(P9_Status == ProcessTaskStatusCodeList.Codes.Open || P9_Status == ProcessTaskStatusCodeList.Codes.Working || (P9_Status == ProcessTaskStatusCodeList.Codes.Closed && P9_GS_NKAssignedStaffMember != GlbStaff.CurrentUser.GS_Code))
				&& !ObjectFactory.Get<IWorkflowSetFieldResultsHelper>().IsSettingProperty(Factory, P9_StatusInfo))
			{
				P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			}

			Validation.ValidateP9_GG_AssignedGroup();

			if (IsException)
			{
				Validation.ValidateP9_Status();
			}
		}

		public IEnumerable<ZGuid> RelatedTaskPKsFromLastTaskAutoAssignment { get; private set; } = Enumerable.Empty<ZGuid>();

		#endregion

		#region P9_GG_AssignedGroup
		[List("Lookups.AssignedGroups")]
		public override ZGuid P9_GG_AssignedGroup
		{
			get { return base.P9_GG_AssignedGroup; }
			set
			{
				base.P9_GG_AssignedGroup = value;
				if (IsTask && !P9_GG_AssignedGroup.IsEmpty && P9_Status == ProcessTaskStatusCodeList.Codes.Open)
				{
					P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				}

				Validation.ValidateP9_GS_NKAssignedStaffMember();

				if (IsException)
				{
					Validation.ValidateP9_Status();
				}
			}
		}

		#endregion

		#region P9_G4_RequiredCapability

		[List("Lookups.RequiredCapabilities")]
		public override ZGuid P9_G4_RequiredCapability
		{
			get { return base.P9_G4_RequiredCapability; }
			set
			{
				base.P9_G4_RequiredCapability = value;
				DefaultCapabilityAssigned = false;

				Validation.ValidateP9_GS_NKAssignedStaffMember();

				if (value.IsValid && P9_Status == ProcessTaskStatusCodeList.Codes.Open)
				{
					P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				}
			}
		}

		protected bool P9_G4_RequiredCapability_ReadOnly => !IsTask;

		#endregion

		#region P9_GE_TriggerDepartment

		[ReadOnly(true)]
		public override ZGuid P9_GE_TriggerDepartment
		{
			get => base.P9_GE_TriggerDepartment;
			set => base.P9_GE_TriggerDepartment = value;
		}

		#endregion

		#region Delay Duration

		[ReadOnlyMember(nameof(P9_SuppressDuplicates_ReadOnly))]
		public override ZBool P9_SuppressDuplicates { get => base.P9_SuppressDuplicates; set => base.P9_SuppressDuplicates = value; }

		public ZBool P9_SuppressDuplicates_ReadOnly => IsNonPersistedRepresentationOfTemplateTrigger;

		[ReadOnlyMember(nameof(P9_DelayDuration_ReadOnly))]
		[ZDateTimeDurationValueCalculatedFromSeconds]
		public ZDateTime P9_DelayDuration
		{
			get => TimeSpan.FromSeconds(P9_DelayDurationSeconds);
			set
			{
				P9_DelayDurationSeconds = DurationAsDateTimeHelper.DateTimeToDuration(value);
			}
		}

		public ZBool P9_DelayDuration_ReadOnly => IsNonPersistedRepresentationOfTemplateTrigger;

		public ZPropertyInfo P9_DelayDurationInfo
		{
			get { return GetZPropertyInfo(nameof(P9_DelayDuration)); }
		}
		#endregion

		#region P9_ActualDate

		[ReadOnlyMember(nameof(P9_ActualDate_ReadOnly))]
		public new ZDateTime P9_ActualDate
		{
			get { return base.P9_ActualDate; }
			protected set
			{
				P9_ActualDateInternal = new ZDateTimeOffset(value);
			}
		}

		protected virtual void OnSetActualDateCore(ZDateTimeOffset value)
		{
		}

		void IProcessTaskInternals.SetActualDateWithoutFiringWorkflow(ZDateTimeOffset value)
		{
			base.P9_ActualDateOffset = value;
		}

		internal ZDateTimeOffset P9_ActualDateInternal
		{
			get => base.P9_ActualDateOffset;
			set
			{
				if (IsMilestoneOrWorkflowTrigger)
				{
					TrySetTriggerableActualDate(null, this.GetJob(), new ZDateTimeOffset(value));
				}
				else if (IsException)
				{
					base.P9_ActualDateOffset = value;
				}
				else // It is a task.
				{
					var oldValue = base.P9_ActualDateOffset;
					base.P9_ActualDateOffset = value;
					CalculateDurationFromStartChange(oldValue, value);
				}
				OnSetActualDateCore(new ZDateTimeOffset(value));
			}
		}

		public ZString TriggerEventSourceStaffCode { get; private set; }

		bool TrySetTriggerableActualDate(IWorkflowTriggerSource triggerEventSource, BusinessObject job, ZDateTimeOffset value)
		{
			using (SuspendSettingHasChangesForTemplateApplication())
			{
				if (IsNonPersistedRepresentationOfTemplateTrigger || IsTemplateTriggerOrMilestone || IsRecursiveDDAEvent_HACK(triggerEventSource))
				{
					return false;
				}
				if (value.IsValid && !value.IsValidSmallDateTime && !Globals.IsUserInteractive)
				{
					ErrorReporter.ReportOnce("StrangeEventTimeForProcessTasks", FormattableString.Invariant($"What is setting P9_ActualDate to such a weird value: [{value}]"));
				}

				var result = false;

				RelatedMilestoneEventDefinition ReferenceForActualEvent(string eventType)
				{
					var proxy = new WorkflowMilestoneProxy(this);
					var previousDate = proxy.ActualDate;
					var eventSource = triggerEventSource != null ? new EventSource(triggerEventSource) : null;
					return new RelatedMilestoneActualEventDefinition(proxy, eventType, newDate: value.ToDateTimeOffsetSafe(), previousDate: previousDate, originalDate: proxy.OriginalActualDate, triggerEventSource: eventSource);
				}

				void SetActualDate(ZDateTimeOffset newValue)
				{
					using (GetValidationSuspender())
					{
						base.P9_ActualDateOffset = ActualDateWithOffset_SetDuringSession = newValue;
					}
					SetFiredStatus();
					result = true;
					Validation.ValidateP9_ActualDate();
					P9_ActualDateForBindingInfo.RefreshBinding();
					TriggerEventSourceStaffCode = triggerEventSource?.StaffCode ?? ZString.Empty;

					if (IsMilestone && !newValue.IsEmpty)
					{
						MilestoneCompletionHelper.TriggerTaskMilestoneCompletion(this);
					}
				}

				StmALog CreateUpdateOrCancelLogForMilestone(EventValue eventValue)
				{
					if (ShouldUpdateEventLog(P9_SE_NKMilestoneEvent))
					{
						return ProcessTaskLogger.CreateRecreateOrUpdateEventLog(eventValue, true);
					}
					else
					{
						return null;
					}
				}

				/*
				* The six use-cases:
				* 1/ Firing this Milestone/Trigger from a matching event.
				* 2/ Firing this Milestone by setting P9_ActualDateForBinding
				* 3/ Firing this Trigger by setting P9_ActualDateForBinding (as a work-around, not officially supported)
				* 4/ Cancelling an event that matches this milestone
				* 5/ Cancelling this Milestone by setting P9_ActualDateForBinding
				* 6/ Updating P9_ActualDate on a Milestone from a matching event without firing
				*/
				if (P9_TriggerFiredCountdown < 1 && value.IsValid)
				{
					if (triggerEventSource is IStmALog)
					{
						var cacheUpdated = false;
						if (this.IsTriggerOrMilestoneThatCanFire())
						{
							cacheUpdated = this.TryUpdateWorkflowTriggerEventLogInCache(job, new EventSource(triggerEventSource), triggerEventSource as IStmALog, () => SetActualDate(value));
						}
						if (!cacheUpdated && this.ShouldUpdateActualDateWithoutFiring())
						{
							SetActualDate(value);
						}
					}
					return result;
				}

				if (triggerEventSource is IStmALog stmALog) // There is a matching event
				{
					if (stmALog.EventTime.IsValid)
					{
						if (value.IsValid)
						{
							if (this.IsTriggerOrMilestoneThatCanFire())
							{
								this.AddWorkflowTriggerEventLog(job, new EventSource(triggerEventSource), triggerEventSource as IStmALog, () => SetActualDate(value));
							}
							else
							{
								if (this.ShouldUpdateActualDateWithoutFiring())
								{
									SetActualDate(value);
								}
								// Track events that didn't fire the milestone so if the previous event is cancelled we can fire based on this event.
								WorkflowTriggerEventCache.TryAddWTELog(this, new EventSource(triggerEventSource), null);
							}
						}
						else if (base.P9_ActualDate.IsValid) // Optimization: Don't bother cancelling if this trigger/milestone is already cancelled.
						{
							this.UnfireTrigger((IStmALog)triggerEventSource, job, SetActualDate);
						}
					}
					else
					{
						ErrorReporter.ReportOnceWithAdditionalInfo("bf82524e-4e3e-43d1-8178-62573585e7b0", StmALog.LogMessages.InvalidEventTimeWithInfo(stmALog),
							StmALog.ErrorReportKeys.Category.WorkflowGun,
							StmALog.ErrorReportKeys.Category.DeletedStmALog);
					}
				}
				else if (IsWorkflowTrigger && triggerEventSource == null)
				{
					// Triggers should never have their actual date set directly, but rather be triggered by raising the appropriate event
					// So we simulate that behaviour here. Setting the value to empty is not supported.
					if (value.IsValid && !string.IsNullOrEmpty(P9_SE_NKMilestoneEvent))
					{
						var def = ReferenceForActualEvent(P9_SE_NKMilestoneEvent);
						// No checking the result here because we block the manual reference update
						TriggerActualDateSetter.TryGenerateReferenceForEvent(new[] { def }, TriggerConditionTypeConverter.Convert(P9_TriggerCondition), P9_TriggerConditionValue, out var eventDefinitionPairs);
						ProcessTaskLogger.CreateRecreateOrUpdateEventLog(eventDefinitionPairs.First().Value.ToEventValue());
						ErrorReporter.ReportOnce("TriggerActualDateSet", "The actual date for a trigger should never be set directly. Instead, raise the associated event.");
					}
				}
				else // The actual date was set directly for a milestone
				{
					var def = ReferenceForActualEvent(P9_SE_NKMilestoneEvent);
					if (value.IsValid)
					{
						if (TryGenerateReferenceForEvent(def, P9_TriggerCondition, P9_TriggerConditionValue, out var pair))
						{
							var shouldFire = this.IsTriggerOrMilestoneThatCanFire();
							var source = CreateUpdateOrCancelLogForMilestone(pair.Value.ToEventValue());
							var firingWorkflowIsntSuspendedOrSomething = source != null || triggerEventSource != null; // For when the date was set directly, but weird recursion happened so we didn't raise a new event.
																													   // If there is some bug like, "P9_ActualDate was set but where is the corresponding WTE" then this is probably the cause.

							if (shouldFire && firingWorkflowIsntSuspendedOrSomething)
							{
								var log = source ?? triggerEventSource;

								if (source?.IsDeleted ?? false)
								{
									AddRowError(Res.GetString("3d5b2c3f-6c24-411f-88cc-e44759ad3f30", "Cannot complete this milestone as there is a completion trigger action causing the {0} event to be canceled.", pair.Value.Code));
								}
								else
								{
									this.AddWorkflowTriggerEventLog(job, new EventSource(log), log as IStmALog, () => SetActualDate(value));
								}
							}
							else
							{
								SetActualDate(value);
							}
						}
					}
					else if (base.P9_ActualDate.IsValid) // Go cancel some matching event
					{
						if (TryGenerateReferenceForEvent(def, P9_TriggerCondition, P9_TriggerConditionValue, out var pair))
						{
							CreateUpdateOrCancelLogForMilestone(pair.Value.ToEventValue()); // This code recurs into into TrySetActualDate.
																							// There could be a subtle bug here if CreateUpdateOrCancelLogForMilestone
							SetActualDate(value);
						}
					}
					else
					{
						// Don't bother cancelling if this trigger/milestone is already cancelled.
					}
				}

				return result;
			}
		}

		MilestoneActualDateSetter TriggerActualDateSetter => triggerActualDateSetter ?? (triggerActualDateSetter = new MilestoneActualDateSetter(new MacroValueReplacementProvider(this)));
		internal MilestoneActualDateSetter MilestoneActualDateSetter => milestoneActualDateSetter ?? (milestoneActualDateSetter = new MilestoneActualDateSetter(new MacroValueReplacementProvider(this)));
		MilestoneActualDateSetter milestoneActualDateSetter;
		MilestoneActualDateSetter triggerActualDateSetter;

		bool TryGenerateReferenceForEvent(RelatedMilestoneEventDefinition[] logsToUpdate, ZString triggerConditionCode, ZString value, out List<EventValueStrategyPair> events)
		{
			var triggerCondition = TriggerConditionTypeConverter.Convert(triggerConditionCode);
			return MilestoneActualDateSetter.TryGenerateReferenceForEvent(logsToUpdate, triggerCondition, value, out events);
		}

		bool TryGenerateReferenceForEvent(RelatedMilestoneEventDefinition logToUpdate, ZString triggerConditionCode, ZString value, out EventValueStrategyPair eventDefinitionPair)
		{
			var triggerCondition = TriggerConditionTypeConverter.Convert(triggerConditionCode);
			var result = MilestoneActualDateSetter.TryGenerateReferenceForEvent(new[] { logToUpdate }, triggerCondition, value, out var eventDefinitionPairs);
			eventDefinitionPair = eventDefinitionPairs.FirstOrDefault();
			return result;
		}

		internal ZDateTimeOffset ActualDateWithOffset_SetDuringSession { get; set; }

		void SetFiredStatus()
		{
			if (IsMilestone)
			{
				SetMilestoneStatuses();
			}
			else if (IsWorkflowTrigger)
			{
				if (P9_ActualDate.IsValid)
				{
					if (P9_Status != ProcessTaskStatusCodeList.Codes.Closed)
					{
						P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}
				else
				{
					P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				}
			}
		}

		public bool TrySetActualDateForEvent(IWorkflowTriggerSource eventSource, BusinessObject job, ZDateTimeOffset actualDate)
		{
			// Trigger can keep firing, but milestone only fire once
			if (IsTemplateTask)
			{
				return false;
			}
			else
			{
				SetProcessJobTriggerLink();
				return TrySetTriggerableActualDate(eventSource, job, actualDate);
			}
		}

		void SetProcessJobTriggerLink()
		{
			if (processJobTriggerLink == null)
			{
				processJobTriggerLink = TemplateTrigger?.GetOrCreateJobVersionOfTrigger(this.GetJob(), false);

				if (processJobTriggerLink is IProcessJobTriggerLink link)
				{
					link.OnDeleted += (o, a) => processJobTriggerLink = IsDeleted ? null : TemplateTrigger?.GetOrCreateJobVersionOfTrigger(this.GetJob(), false);
				}
			}
		}

		bool IsRecursiveDDAEvent_HACK(IWorkflowTriggerSource sourceLog)
		{
			if (sourceLog == null)
			{
				return false;
			}
			else
			{
				var isDDATriggerWithEDCAction = P9_SE_NKMilestoneEvent == Events.DocumentAllocatedCode
				&& ProcessTaskNotifications.Any(a => a.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs);

				return isDDATriggerWithEDCAction && IsSourceLogDocumentEqualsToActionDocuments(sourceLog, ProcessTaskNotifications);
			}
		}

		bool IsSourceLogDocumentEqualsToActionDocuments(IWorkflowTriggerSource sourceLog, ProcessTaskNotificationCollection actions)
		{
			var documentCommandPKs = actions
				.Where(a => a.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs)
				.Select(a => a.PQ_SU_Document)
				.ToArray();
			if (documentCommandPKs.Any())
			{
				var guid = BaseStmALog.GetGuid(sourceLog.Reference);
				var sourceLogDocType = new ZString(sourceLog.Reference).SubstringSafe(0, 3);

				if (guid.IsValid)
				{
					var templatePivots = Factory.Load<IStmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, documentCommandPKs));
					foreach (var templatePivot in templatePivots)
					{
						var docType = Factory.Load<RefDocType>(templatePivot.SI_RT_DocType);
						if (docType != null && docType.RT_DocType == sourceLogDocType)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		void SetMilestoneStatuses()
		{
			var milestones = ParentTaskCollection?.Milestones;
			if (milestones != null)
			{
				WorkflowAfterOnSavingBOService.TryHookupMilestoneStatusService(Factory, milestones);
			}
		}

		public virtual void UpdateScheduledDate()
		{
		}

		protected void ClearP9_ActualDateWithoutDeletingLog()
		{
			base.P9_ActualDateOffset = ZDateTimeOffset.Empty;
		}

		#region ReadOnly for Actuals

		protected virtual bool P9_ActualDate_ReadOnly
		{
			get { return IsWorkflowTrigger || RestrictEditingTaskActualDateFields || !P9_LineTriggerType.IsEmpty || DisallowCreateEventBySettingDates; }
		}

		bool DisallowCreateEventBySettingDates => Events.ChangeLogCodes.Contains(P9_SE_NKMilestoneEvent);

		protected bool P9_CompletedTimeUtc_ReadOnly
		{
			get { return IsWorkflowTrigger || RestrictEditingTaskActualDateFields; }
		}

		protected bool P9_ActualDuration_ReadOnly
		{
			get { return RestrictEditingTaskActualDateFields; }
		}

		bool RestrictEditingTaskActualDateFields
		{
			get { return IsTask && (!Env.Security.WorkflowTasksEditActuals.IsAllowed || this is TemplateProcessTask); }
		}

		#endregion

		bool ShouldUpdateEventLog(string eventCode)
		{
			return Events.All[eventCode] != null
				&& !Suspender<string>.IsSuspended(ref suspender, eventCode)
				&& ShouldUpdateEventLog_IgnoringSuspend();
		}

		bool ShouldUpdateEventLog_IgnoringSuspend()
		{
			return IsMilestone
				&& Parent != null
				&& !IsDefaultingFromSameEvent;
		}

		IDisposable IProcessTaskInternals.SuspendUpdateEventLog()
		{
			return SuspendUpdateEventLog();
		}

		internal IDisposable SuspendUpdateEventLog()
		{
			return Suspender<string>.Suspend(ref suspender, P9_SE_NKMilestoneEvent);
		}

		public void RaiseMilestoneExceptionForFutureActualDate()
		{
			if (P9_ActualDateInfo.HasError(ProcessTaskValidationBase.ActualDateIsInTheFutureErrorMessage)
				|| (IsValidationSuspended && ProcessTaskValidationBase.IsMilestoneActualDateInTheFuture(this)))
			{
				// Note: This is called in OnSaving, it can only happen in a context where the save is ignoring validation errors or validation is suspended
				CreateMilestoneException(ProcessWorkflowExceptionType.ExceptionFutureEvent);
			}
		}

		#endregion

		#region P9_ActualDateForBinding

		[SkipForRefresh]
		public ZDateTimeOffset P9_ActualDateForBinding
		{
			get => P9_ActualDateInternal;
			set
			{
				P9_ActualDateInternal = value;
			}
		}

		public ZPropertyInfo P9_ActualDateForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.P9_ActualDateForBinding, _ => P9_ActualDateInfo); }
		}

		#endregion

		#region P9_ActualDateUpdateType

		[List("Lookups.ActualDateUpdateTypes")]
		[ReadOnlyMember(nameof(ActualDateUpdateType_ReadOnly))]
		public override ZString P9_ActualDateUpdateType { get => base.P9_ActualDateUpdateType; set => base.P9_ActualDateUpdateType = value; }

		public bool ActualDateUpdateType_ReadOnly => !IsTemplate && (P9_ParentTemplateID.IsValid || IsInDatabase);

		#endregion

		#region P9_ActualDuration

		[ZDateTimeDurationValue]
		public override ZDateTime P9_ActualDuration
		{
			get { return new ZDateTime(GetValueFromRowSafely(ProcessTasksSchema.P9_ActualDuration), DateTimeKind.Local); }
			set
			{
				var duration = RoundUpMinuteIfNeeded(value.ConvertToDurationBasedDate(P9_ActualDurationInfo));

				SetPropertyValue(P9_ActualDurationInfo, duration);
				if (!IsValidationSuspended)
				{
					Validation.ValidateP9_ActualDuration();
				}
			}
		}

		ZDateTime RoundUpMinuteIfNeeded(ZDateTime value)
		{
			if (!value.IsEmpty && value.IsValid)
			{
				var baseYear = ZDateTime.DefaultDurationEpoch;
				var difference = value - baseYear;

				if (difference.TotalMinutes > 0 && difference.TotalMinutes < 1)
				{
					return baseYear.AddMinutes(1);
				}
			}

			return value;
		}

		#endregion

		#region P9_ScheduledDateForBinding

		[SkipForRefresh]
		public ZDateTimeOffset P9_OriginalScheduledDateLocalForBinding
		{
			get { return new ZDateTimeOffset(P9_OriginalScheduledDateLocal); }
			set
			{
				using (SupressScheduledDateChangeErrorReport())
				{
					P9_OriginalScheduledDateLocal = value.ToZDateTime();
				}
			}
		}

		public ZPropertyInfo P9_OriginalScheduledDateLocalForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(P9_OriginalScheduledDateLocalForBinding), _ => P9_OriginalScheduledDateUtcInfo); }
		}

		[SkipForRefresh]
		public ZDateTimeOffset P9_OriginalScheduledDateUtcForBinding
		{
			get { return new ZDateTimeOffset(P9_OriginalScheduledDateUtc, DateTimeKind.Utc); }
			set
			{
				using (SupressScheduledDateChangeErrorReport())
				{
					P9_OriginalScheduledDateUtc = value.ToUtcZDateTime();
				}
			}
		}

		public ZPropertyInfo P9_OriginalScheduledDateUtcForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(P9_OriginalScheduledDateUtcForBinding), _ => P9_OriginalScheduledDateUtcInfo); }
		}

		[SkipForRefresh]
		public ZDateTimeOffset P9_ScheduledDateForBinding
		{
			get => P9_ScheduledDateOffset;
			set
			{
				P9_ScheduledDateOffset = value;
			}
		}

		public ZPropertyInfo P9_ScheduledDateForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.P9_ScheduledDateForBinding, _ => P9_ScheduledDateInfo); }
		}

		[ReadOnly(true)]
		[SkipForRefresh]
		public ZDateTime P9_ScheduledDateUtcForBinding
		{
			get { return P9_ScheduledDateUtc; }
		}

		[ReadOnly(true)]
		[SkipForRefresh]
		public ZDateTime P9_ActualDateUtcForBinding
		{
			get { return P9_ActualDateUtc; }
		}

		public ZPropertyInfo P9_ScheduledDateUtcForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(P9_ScheduledDateUtcForBinding), _ => P9_ScheduledDateInfo); }
		}

		public ZPropertyInfo P9_ActualDateUtcForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(P9_ActualDateUtcForBinding), _ => P9_ActualDateInfo); }
		}

		[ReadOnly(true)]
		[SkipForRefresh]
		public ZDateTime P9_ScheduledDateLocalForBinding
		{
			get { return P9_ScheduledDateUtc.ToLocalBranchTime(Factory); }
		}

		public ZPropertyInfo P9_ScheduledDateLocalForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(P9_ScheduledDateLocalForBinding), _ => P9_ScheduledDateInfo); }
		}

		[ReadOnly(true)]
		[SkipForRefresh]
		public ZDateTime P9_ActualDateLocalForBinding
		{
			get { return P9_ActualDateUtc.ToLocalBranchTime(Factory); }
		}

		public ZPropertyInfo P9_ActualDateLocalForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(P9_ActualDateLocalForBinding), _ => P9_ActualDateInfo); }
		}

		#endregion

		#region P9_CompletedTimeUtc

		public override ZDateTime P9_CompletedTimeUtc
		{
			get { return base.P9_CompletedTimeUtc; }
			set
			{
				base.P9_CompletedTimeUtc = value;
				CompletedTimeLocalInfo.RefreshBinding();
			}
		}

		public ZDateTime CompletedTimeLocal
		{
			get { return P9_CompletedTimeUtc.ToLocalBranchTime(); }
			set
			{
				P9_CompletedTimeUtc = value.ToUniversalBranchTime();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCompletedTimeLocal();
				}
			}
		}

		public virtual ZPropertyInfo CompletedTimeLocalInfo
		{
			get { return GetZPropertyInfo(nameof(CompletedTimeLocal)); }
		}

		#endregion

		#region P9_EstDuration

		[ZDateTimeDurationValue]
		public override ZDateTime P9_EstDuration
		{
			get { return base.P9_EstDuration; }
			set
			{
				base.P9_EstDuration = value.ConvertToDurationBasedDate(P9_EstDurationInfo);
				HighEstimatedDurationInfo.RefreshBinding();
			}
		}

		#endregion

		#region P9_EstimatedHandoverTime

		public override ZDateTime P9_EstimatedHandoverTimeUtc
		{
			get
			{
				return base.P9_EstimatedHandoverTimeUtc;
			}
			set
			{
				base.P9_EstimatedHandoverTimeUtc = value;
				EstimatedHandoverTimeLocalInfo.RefreshBinding();
			}
		}

		public ZDateTime EstimatedHandoverTimeLocal
		{
			get { return P9_EstimatedHandoverTimeUtc.ToLocalBranchTime(Factory); }
			set { P9_EstimatedHandoverTimeUtc = value.ToUniversalBranchTime(Factory); }
		}

		public ZPropertyInfo EstimatedHandoverTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(EstimatedHandoverTimeLocal), o => P9_EstimatedHandoverTimeUtcInfo); }
		}

		#endregion

		#region Elapsed Duration

		[ResourceStringData("2336aab1-d126-44af-ac39-2cd0aa2e78a4", Caption = "Elapsed Duration")]
		public ZDateTime ElapsedDuration => ProcessTaskStatusChanger.GetActualDurationAsync(ProcessTaskWrapper).GetAwaiter().GetResult();

		public ZPropertyInfo ElapsedDurationInfo
		{
			get { return GetZPropertyInfo(nameof(ElapsedDuration)); }
		}

		#endregion

		#region EventLogSuspendable

		Suspender<string> suspender;

		#endregion

		#region Suspended Duration

		[ResourceStringData("0749eaa7-0b83-4992-b916-5f5e1854dcea", Caption = "Suspended Duration")]
		public ZDateTime SuspendedDuration
		{
			get
			{
				ZDateTime result = P9_TotalSuspendedDuration;
				if (!result.IsValid)
				{
					result = ZDateTime.DefaultDurationEpoch;
				}

				if (P9_Status == ProcessTaskStatusCodeList.Codes.Suspended && P9_SuspendedAt.IsValid)
				{
					TimeSpan currentSuspendedTime = GetTimeDifferenceInUtcTime(P9_SuspendedAt, ZDateTime.Now);
					result = result.Add(currentSuspendedTime);
				}

				return result;
			}
		}

		public ZPropertyInfo SuspendedDurationInfo
		{
			get { return GetZPropertyInfo(nameof(SuspendedDuration)); }
		}

		#endregion

		#region P9_SE_NKExceptionEvent

		[ActionField(CollectionType = typeof(StmEventCodeDescriptionPairList), FieldType = ActionFieldType.Code)]
		public override ZString P9_SE_NKExceptionEvent
		{
			get { return base.P9_SE_NKExceptionEvent; }
			set
			{
				base.P9_SE_NKExceptionEvent = value;
			}
		}

		#endregion

		#region P9_RespondToCascadedEvents

		public override ZBool P9_RespondToCascadedEvents
		{
			get { return base.P9_RespondToCascadedEvents; }
			set
			{
				base.P9_RespondToCascadedEvents = value;
				if (!value)
				{
					P9_CascadedEventsContext = ZString.Empty;
				}
				P9_CascadedEventsContextInfo.RefreshBinding();
			}
		}

		#endregion

		#region P9_CascadedEventsContext

		protected bool P9_CascadedEventsContext_ReadOnly
		{
			get
			{
				var contextSteps = P9_RespondToCascadedEvents && WorkflowDescriptor != null ? WorkflowDescriptor.GetFollowingContextSteps(Enumerable.Empty<WorkflowEventContextPair>()) : null;
				return contextSteps == null || !contextSteps.Any();
			}
		}

		#endregion

		#region P9_LineTriggerType

		[List("Lookups.LineTriggerTypes")]
		[ResourceStringData("ea0c5746-2e9c-11e5-bf45-902b34dc814a", Caption = "Line Trigger Type", ShortCaption = "Line Trigger", FullDescription = "The type of the children this trigger belongs to, i.e. the trigger is common for all children of this type. The trigger will be displayed on the related child workflow.")]
		public override ZString P9_LineTriggerType
		{
			get { return base.P9_LineTriggerType; }
			set { base.P9_LineTriggerType = value; }
		}

		public bool P9_LineTriggerType_ReadOnly => IsInDatabase || Lookups.LineTriggerTypes.Count == 0; //Lookup is empty where line triggers are not supported

		#endregion

		#region P9_TriggerFiredCountDown

		[ReadOnly(true)]
		[CustomisedControlExclude]
		[BusinessObjectTestExclude] // Can't set this property without using ITriggerConditions interface.
		public override ZShort P9_TriggerFiredCountdown
		{
			get => base.P9_TriggerFiredCountdown;
			//
			set
			{
				if (value < 10)
				{
					ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_TriggerFiredCountdown), ConcurrencyPolicy.DefaultWithoutDatabaseMerge);
				}
				EnsureCanSetTriggerCondition(nameof(P9_TriggerFiredCountdown));
				base.P9_TriggerFiredCountdown = value;
			}
		}

		public override ZPropertyInfo P9_TriggerFiredCountdownInfo
		{
			get
			{
				var info = base.P9_TriggerFiredCountdownInfo;
				info.ConcurrencyMerged -= P9_TriggerFiredCountdownConcurrencyMergeHandler;
				info.ConcurrencyMerged += P9_TriggerFiredCountdownConcurrencyMergeHandler;
				return info;
			}
		}
#if DEBUG
		internal
#endif
		void P9_TriggerFiredCountdownConcurrencyMergeHandler(object sender, EventArgs args)
		{
			var concurrencyArgs = args as ConcurrencyValueChangedEventArgs;
			if (concurrencyArgs != null)
			{
				var originalValue = concurrencyArgs.OriginalValue == DBNull.Value ? 0 : concurrencyArgs.OriginalValue;
				var currentValue = concurrencyArgs.CurrentValue == DBNull.Value ? 0 : concurrencyArgs.CurrentValue;

				var currentTriggerFiredCount = Convert.ToInt16(originalValue) - Convert.ToInt16(currentValue);
				var excessTriggerFiredCount = (ZShort)ZDataType.ObjectToZType(typeof(ZShort), concurrencyArgs.OldValue) - currentTriggerFiredCount;
				if (excessTriggerFiredCount < 0)
				{
					this.DeleteWorkflowTriggerEventLogsByEventTime(Math.Abs(excessTriggerFiredCount));
					concurrencyArgs.FinalValue = 0;
				}
				else
				{
					concurrencyArgs.FinalValue = (short)excessTriggerFiredCount;
				}
			}
		}

		#endregion

		#region P9_thing

		[List("Lookups.CompletionMilestones")]
		[ReadOnlyMember(nameof(P9_MilestoneCompletionPivotKeyReadOnly))]
		public override ZString P9_MilestoneCompletionPivotKey
		{
			get
			{
				if (IsTask)
				{
					return base.P9_MilestoneCompletionPivotKey;
				}
				else
				{
					return new ZString(FormattableString.Invariant($"{P9_SE_NKMilestoneEvent} - {P9_Description}")).SubstringSafe(0, ProcessHeaderSchema.FH_MilestoneCompletionPivotKey.MaxLength);
				}
			}
			set => base.P9_MilestoneCompletionPivotKey = value;
		}

		public bool P9_MilestoneCompletionPivotKeyReadOnly => !IsTask;

		#endregion

		#region P9_IsResetBeingAppliedToThisTask

		[ReadOnly(true)]
		[ResourceStringData("618514ba-863b-429a-8cee-f171f99ef97d", Caption = "Is Penetration Reset")]
		public override ZBool P9_IsResetBeingAppliedToThisTask
		{
			get => base.P9_IsResetBeingAppliedToThisTask;
			set => base.P9_IsResetBeingAppliedToThisTask = value;
		}

		#endregion

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get
			{
				string result = HumanReadableNameWithoutID;
				if (!P9_TaskID.IsEmpty)
				{
					result += " " + P9_TaskID;
				}
				if (!P9_Description.IsEmpty)
				{
					result += " " + P9_Description;
				}
				return result;
			}
		}

		protected ZString HumanReadableNameWithoutID
		{
			get
			{
				return Res.GetString("201d2c6a-e57b-4455-a9f4-dc8374759332", "Task");
			}
		}

		#endregion

		#region P9_ParentTableCode

		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString P9_ParentTableCode
		{
			get => base.P9_ParentTableCode;
			set => base.P9_ParentTableCode = value;
		}

		#endregion

		#region Audit Fields

		[ReadOnly(true)]
		public override ZDateTime P9_SystemCreateTimeUtc
		{
			get => base.P9_SystemCreateTimeUtc;
			set => base.P9_SystemCreateTimeUtc = value;
		}

		public override ZPropertyInfo P9_SystemCreateTimeUtcInfo
		{
			get
			{
				var info = base.P9_SystemCreateTimeUtcInfo;
				return info;
			}
		}

		[ReadOnly(true)]
		public override ZDateTime P9_SystemLastEditTimeUtc
		{
			get => base.P9_SystemLastEditTimeUtc;
			set => base.P9_SystemLastEditTimeUtc = value;
		}

		public override ZPropertyInfo P9_SystemLastEditTimeUtcInfo
		{
			get
			{
				var info = base.P9_SystemLastEditTimeUtcInfo;
				return info;
			}
		}

		[ReadOnly(true)]
		public override ZString P9_SystemCreateUser
		{
			get => base.P9_SystemCreateUser;
			set => base.P9_SystemCreateUser = value;
		}

		public override ZPropertyInfo P9_SystemCreateUserInfo
		{
			get
			{
				var info = base.P9_SystemCreateUserInfo;
				return info;
			}
		}

		[ReadOnly(true)]
		public override ZString P9_SystemLastEditUser
		{
			get => base.P9_SystemLastEditUser;
			set => base.P9_SystemLastEditUser = value;
		}

		public override ZPropertyInfo P9_SystemLastEditUserInfo
		{
			get
			{
				var info = base.P9_SystemLastEditUserInfo;
				return info;
			}
		}

		#endregion

		#endregion

		#region New Properties

		#region CompletionStatement

		public bool IsCompletionStatement
		{
			get
			{
				if (!P9_Type.IsEmpty)
				{
					var workflowProvider = Parent;
					var template = workflowProvider as ProcessTaskTemplate;
					var workflowType = template != null ? template.P0_ProcessType : workflowProvider != null ? workflowProvider.WorkflowType : ZString.Empty;

					return !string.IsNullOrEmpty(workflowType) && P9_Type == GetCompletionStatementTaskType(workflowType);
				}
				else
				{
					return false;
				}
			}
		}

		public static string GetCompletionStatementTaskType(string workflowType)
		{
			return WorkflowDataRegistry.Instance.TaskTypes.GetCompletionStatementTaskType(workflowType);
		}

		#endregion

		#region LongDescription

		public ZString LongDescription => IsCompletionStatement ? P9_NotesAsString : P9_Description;

		#endregion

		#region WorkflowSequence

		[ResourceStringData("ProcessTasks.WorkflowSequence", ShortCaption = "Work. Seq.", Caption = "Workflow Sequence")]
		public ZString WorkflowSequence
		{
			get
			{
				if (ProcessHeader == null)
				{
					return ZString.Empty;
				}
				else
				{
					return ProcessHeader.Sequence;
				}
			}
		}

		#endregion

		#region Relevant Estimate

		[CustomisedControlExclude]
		[ResourceStringData("ProcessTasks.RelevantEstimateHours", ShortCaption = "Relevant Est. Hrs", Caption = "Relevant Estimate Hours")]
		public ZDecimal RelevantEstimateHours
		{
			get { return this.GetRelevantEstimateHours(); }
		}

		[ResourceStringData("ProcessTasks.RelevantEstimateHoursLabel", ShortCaption = "Relevant Est. Hrs", Caption = "Relevant Estimate Hours")]
		public ZString RelevantEstimateHoursLabel
		{
			get { return TimeSpan.FromHours((double)RelevantEstimateHours).ToHoursAndMinutesString(); }
		}

		#endregion

		#region StandardEstimateHours

		[ResourceStringData("ProcessTask.StandardEstimateHours", Caption = "Standard Estimate Hours", ShortCaption = "Std. Est. Hours")]
		public ZDecimal StandardEstimateHours
		{
			get { return this.GetStandardEstimateHours(); }
		}

		#endregion

		#region Has Note

		[ResourceStringData("8fa771b8-486c-4f6f-b1d4-bf2b85adea14", Caption = "Has Note")]
		[VisualBoardSearchable]
		public ZBool HasNote
		{
			get
			{
				return !P9_Notes.IsEmpty ||
				(HasStorageMain &&
				(
				((IDocManagerSupport)this).DocManagerInfo.Documents.Count > 0 ||
				((IDocManagerSupport)this).DocManagerInfo.Files.Count > 0
				));
			}
		}

		bool HasStorageMain
		{
			get
			{
				return ((IDocManagerSupport)this).DocManagerInfo.MasterFactory.GetStorageMainForPK(PK) != null;
			}
		}

		public ZPropertyInfo HasNoteInfo
		{
			get { return GetZPropertyInfo(nameof(HasNote)); }
		}

		#endregion

		#region Additional Action Conditions

		#region Event Reference

		[ReadOnly(true)]
		[CustomisedControlExclude]
		[BusinessObjectTestExclude] // Can't set this property without using ITriggerConditions interface.
		public ZString P9_TriggerConditionValue
		{
			get { return P9_Notes.ToUTF8(); }
			set
			{
				EnsureCanSetTriggerCondition(nameof(P9_TriggerConditionValue));

				ZBlob previousValue = P9_Notes;
				var previousTriggerConditionValue = previousValue.ToUTF8();

				RelatedMilestoneEventDefinition[] GetLogsToUpdate()
				{
					var eventType = Events.All[P9_SE_NKMilestoneEvent];
					if (ShouldUpdateEventLog(P9_SE_NKMilestoneEvent))
					{
						var mp = new WorkflowMilestoneProxy(this);
						var originalActualDate = mp.OriginalActualDate;
						var actualDate = P9_ActualDateForBinding.ToDateTimeOffsetSafe();
						var originalEstimateDate = mp.OriginalEstimateDate;
						var estimateDate = P9_ScheduledDateForBinding.ToDateTimeOffsetSafe();
						return new RelatedMilestoneEventDefinition[]
						{
							new RelatedMilestoneActualEventDefinition(mp, P9_SE_NKMilestoneEvent, originalActualDate, actualDate, actualDate),
							new RelatedMilestoneIsEstimateEvent(mp, P9_SE_NKMilestoneEvent, originalEstimateDate, estimateDate, estimateDate),
							new RelatedMilestoneESTEventDefinition(mp, originalEstimateDate, estimateDate, estimateDate),
						}.Where(e => InMemoryStmALogFinder.Find(this, e, previousTriggerConditionValue) != null).ToArray();
					}
					else
					{
						return Array.Empty<RelatedMilestoneEventDefinition>();
					}
				}

				P9_Notes = Encoding.UTF8.GetBytes(value);

				if (!IsValidationSuspended)
				{
					TriggerConditions.Validation.ValidateTriggerConditionValue();
				}

				if (!TriggerConditions.TriggerConditionValueInfo.HasErrors())
				{
					var logsToUpdate = GetLogsToUpdate();
					if (TryGenerateReferenceForEvent(logsToUpdate, P9_TriggerCondition, value, out List<EventValueStrategyPair> events))
					{
						foreach (var pair in events)
						{
							new UpdateReferenceOfInMemoryLog(this, pair.Strategy, previousTriggerConditionValue).Apply(pair.Value.ToEventValue());
						}
					}
					else
					{
						P9_Notes = previousValue;
						P9_TriggerConditionValueInfo.RefreshBinding();
					}
				}
			}
		}

		public ZPropertyInfo P9_TriggerConditionValueInfo
		{
			get { return GetZPropertyInfo(Schema.P9_TriggerConditionValue); }
		}

		#endregion

		#region IsMatchEventReference

		public static string GenerateRFPCondition(params KeyValuePair<string, string>[] conditions)
		{
			return string.Join(",", conditions.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}={1}", c.Key, c.Value)));
		}

		#endregion

		#endregion

		#region ContactName

		public ZString ContactName
		{
			get
			{
				var contact = Contact;
				return contact != null ? contact.OC_ContactName : ZString.Empty;
			}
		}

		public ZPropertyInfo ContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(ContactName)); }
		}

		#endregion

		#region StaffName

		[VisualBoardSearchable]
		[ResourceStringData("577ab113-f1a4-4dbc-adf4-f1bfd54de920", Caption = "Staff Name")]
		public ZString StaffName
		{
			get { return AssignedStaffMember != null ? AssignedStaffMember.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo StaffNameInfo
		{
			get { return GetZPropertyInfo(nameof(StaffName)); }
		}

		#endregion

		#region Group Name

		[VisualBoardSearchable]
		[ResourceStringData("ba5566d2-f07d-4d7f-9cf7-848cd4b8a1ab", Caption = "Group Name")]
		public ZString GroupName
		{
			get { return AssignedGroup != null ? AssignedGroup.GG_Desc : ZString.Empty; }
		}

		public ZPropertyInfo GroupNameInfo
		{
			get { return GetZPropertyInfo(nameof(GroupName)); }
		}

		#endregion

		#region Capability Name

		[VisualBoardSearchable]
		[ResourceStringData("ProcessTask|CapabilityName", Caption = "Capability Name")]
		public ZString CapabilityName
		{
			get { return RequiredCapability != null ? RequiredCapability.G4_Description : ZString.Empty; }
		}

		public ZPropertyInfo CapabilityNameInfo
		{
			get { return GetZPropertyInfo(nameof(CapabilityName)); }
		}

		#endregion

		#region CapabilityCode

		[BusinessObjectTestExclude]
		public ZString CapabilityCode
		{
			get { return RequiredCapability?.G4_Code ?? ZString.Empty; }
			set
			{
				var capability = Factory.LoadFromNaturalKey<GlbCapability>(GlbCapabilitySchema.G4_Code, value);
				if (capability != null)
				{
					P9_G4_RequiredCapability = capability.PK;
				}
			}
		}

		public ZPropertyInfo CapabilityCodeInfo => GetZPropertyInfo(nameof(CapabilityCode));

		protected bool CapabilityCode_ReadOnly => P9_G4_RequiredCapability_ReadOnly;

		#endregion

		#region Type Description

		[ResourceStringData("3189a439-1b87-4ac6-81db-040122f072dc", Caption = "Type Description")]
		public ZString TypeDescription
		{
			get { return Lookups.Types.GetDescriptionFromCode(P9_Type); }
		}

		public ZPropertyInfo TypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TypeDescription)); }
		}

		#endregion

		#region Status Description

		[VisualBoardSearchable]
		[ResourceStringData("d5e1824d-de1c-4875-93f9-5952347dad2e", Caption = "Status Description")]
		public ZString StatusDescription
		{
			get { return Lookups.Statuses.GetDescriptionFromCode(P9_Status); }
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(StatusDescription)); }
		}

		[ResourceStringData("ProcessTasks.CardStatusDescription", ShortCaption = "Card Status Desc.", Caption = "Card Status Description")]
		public ZString CardStatusDescription
		{
			get
			{
				var assignment = RequiresResourceWithCapability
					? Res.GetString("48c1ec24-7df8-4ff3-b1f5-dfa2b6e4d33e", "requires {0} capability", CapabilityName)
					: AssignedStaffMember != null ? AssignedStaffMember.GS_FullName.ToString() : string.Empty;
				if (!string.IsNullOrEmpty(assignment))
				{
					assignment = string.Format(" ({0})", assignment);
				}
				return StatusDescription.Split('(')[0].Trim() + assignment;
			}
		}

		public ZPropertyInfo CardStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CardStatusDescription)); }
		}

		#endregion

		#region Organisation

		[RelatedBusinessObject("Organisation")]
		[List("Lookups.Organisations")]
		[ResourceStringData("ProcessTasks.OrganisationPK", ShortCaption = "Org.", Caption = "Organization")]
		public ZGuid OrganisationPK
		{
			get { return P9_OA_ZAddress.OrgPK; }
			set { P9_OA_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo OrganisationPKInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationPK)); }
		}

		public OrgHeader Organisation
		{
			get { return Factory.Load<OrgHeader>(OrganisationPK); }
		}

		#endregion

		#region IsBehindSchedule / Estimated Completion Date

		/// <summary>
		/// The Estimated Completion Date been passed and the task is not yet complete.
		/// </summary>
		[ResourceStringData("8ba27ed8-fe80-43f3-a705-4ea2913e46a4", Caption = "Is Behind Schedule")]
		public bool IsBehindSchedule
		{
			get { return P9_ActualDate.IsEmpty && !EstimatedCompletionDate.IsEmpty && ZDateTime.Now > EstimatedCompletionDate; }
		}

		/// <summary>
		/// Calculated by taking the scheduled start date and adding the estimated duration to it.
		/// </summary>
		[ResourceStringData("659d0d71-4616-4df9-8611-cb78016a888e", ShortCaption = "Est. Comp. Date", Caption = "Estimated Completion Date")]
		public ZDateTime EstimatedCompletionDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (P9_ScheduledDate.IsValid)
				{
					result = P9_ScheduledDate;

					if (P9_EstDuration.IsValid)
					{
						result = result.AddHours(P9_EstDuration.Hour).AddMinutes(P9_EstDuration.Minute);
					}
				}

				return result;
			}
		}

		#endregion

		#region Is Suspended

		/// <summary>
		/// Is the task currently in a 'Suspended' status.
		/// </summary>
		[ResourceStringData("a5bdf22a-e3cb-4a03-8112-0e621d6164d2", Caption = "Is Suspended")]
		public bool IsSuspended
		{
			get { return P9_Status == ProcessTaskStatusCodeList.Codes.Suspended; }
		}

		#endregion

		#region Is Closed

		[ResourceStringData("fdcd6c1b-702a-4ec7-a16a-73d40fc2a64e", Caption = "Is Closed")]
		public bool IsClosed
		{
			get { return IsClosedCodes.Contains(P9_Status); }
		}

		public static ZString[] IsClosedCodes
		{
			get
			{
				return new ZString[] { ZString.Empty, ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled, LastCompletedStatusCode };
			}
		}

		public bool IsOpen => !string.Equals(P9_Status, ProcessTaskStatusCodeList.Codes.Closed, StringComparison.InvariantCultureIgnoreCase) &&
							  !string.Equals(P9_Status, ProcessTaskStatusCodeList.Codes.Cancelled, StringComparison.InvariantCultureIgnoreCase);

		#endregion

		#region Is Open or Assigned

		public bool IsOpenOrAssigned => string.Equals(P9_Status, ProcessTaskStatusCodeList.Codes.Open, StringComparison.InvariantCultureIgnoreCase) ||
										string.Equals(P9_Status, ProcessTaskStatusCodeList.Codes.Assigned, StringComparison.InvariantCultureIgnoreCase);

		#endregion

		#region Actual Duration

		[ResourceStringData("31403f5f-55d5-4749-904f-2b8cbbfc82a2", ShortCaption = "Actual Dur Hrs", Caption = "Actual Duration Hours")]
		public ZDecimal ActualDurationHours
		{
			get { return TaskDurationCalculator.GetHoursFromDuration(P9_ActualDuration); }
		}

		public ZPropertyInfo ActualDurationHoursInfo
		{
			get { return GetZPropertyInfo(nameof(ActualDurationHours)); }
		}

		#endregion

		#region EstimatedTimeToCompleteHours

		[ResourceStringData("d5add003-4d8f-4777-9c90-049cf2a00ae8", ShortCaption = "Est. Hours To Complete", Caption = "Estimated Hours To Complete")]
		public ZDecimal EstimatedTimeToCompleteHours
		{
			get { return this.GetEstimatedTimeToCompleteHours(); }
		}

		#endregion

		#region Estimated Duration

		[ResourceStringData("ProcessTasks.LowEstimatedDurationHours", ShortCaption = "Low Est Dur Hrs", Caption = "Low Estimated Duration Hours")]
		public ZDecimal LowEstimatedDurationHours
		{
			get { return this.GetLowEstimatedDurationHours(); }
		}

		public ZPropertyInfo LowEstimatedDurationHoursInfo
		{
			get { return GetZPropertyInfo(nameof(LowEstimatedDurationHours)); }
		}

		[ResourceStringData("c40a2280-f5ff-46c7-913c-aa0994f58326", ShortCaption = "High Est Dur Hrs", Caption = "High Estimated Duration Hours")]
		public ZDecimal HighEstimatedDurationHours
		{
			get { return this.GetHighEstimatedDurationHours(); }
		}

		public ZPropertyInfo HighEstimatedDurationHoursInfo
		{
			get { return GetZPropertyInfo(nameof(HighEstimatedDurationHours)); }
		}

		[ResourceStringData("ca5c5280-a6ae-40a6-88af-06505f62d710", ShortCaption = "Est Desc.", Caption = "Estimate Description")]
		public ZString EstimateDescription
		{
			get
			{
				return Res.GetString("2fa74172-0ce1-478f-bd7b-0aca9f2875d0", "{0} to {1} hours (standard estimate {2})",
					TimeSpan.FromHours((double)LowEstimatedDurationHours).ToHoursAndMinutesString(),
					TimeSpan.FromHours((double)HighEstimatedDurationHours).ToHoursAndMinutesString(),
					TimeSpan.FromHours((double)StandardEstimateHours).ToHoursAndMinutesString());
			}
		}

		#endregion

		#region Suspended Duration

		[ResourceStringData("4bc58c5c-dbf4-45be-947f-7ab0da415cf6", ShortCaption = "Sus Dur Hrs", Caption = "Suspended Duration Hours")]
		public ZDecimal SuspendedDurationHours
		{
			get { return TaskDurationCalculator.GetHoursFromDuration(SuspendedDuration); }
		}

		public ZPropertyInfo SuspendedDurationHoursInfo
		{
			get { return GetZPropertyInfo(nameof(SuspendedDurationHours)); }
		}

		#endregion

		#region P9_OriginalScheduledDateLocal

		public ZDateTime P9_OriginalScheduledDateLocal
		{
			get { return P9_OriginalScheduledDateUtc.IsValid && !P9_OriginalScheduledDateUtc.IsEmpty ? Env.Instance.Time.GetLocalTimeFromUtc(P9_OriginalScheduledDateUtc.ToDateTime()) : P9_OriginalScheduledDateUtc; }
			set
			{
				var convertedUtcDate = value.IsValid && !value.IsEmpty ? Env.Instance.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
				if (convertedUtcDate < ZDateTime.MinSmallDateTimeValue)
				{
					P9_OriginalScheduledDateUtc = ZDateTime.MinSmallDateTimeValue;
				}
				else if (convertedUtcDate > ZDateTime.MaxSmallDateTimeValue)
				{
					P9_OriginalScheduledDateUtc = ZDateTime.MaxSmallDateTimeValue;
				}
				else
				{
					P9_OriginalScheduledDateUtc = convertedUtcDate;
				}
			}
		}

		public ZPropertyInfo P9_OriginalScheduledDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.P9_OriginalScheduledDateLocal, o => P9_OriginalScheduledDateUtcInfo); }
		}

		#endregion

		#region Parent Job Details

		[ResourceStringData("f2fd30e8-676d-462c-bd2e-3e79f433df93", Caption = "Parent Job Details")]
		public ZString ParentJobDetails => this.GetParentJobDetails();

		public ZPropertyInfo ParentJobDetailsInfo
		{
			get { return GetZPropertyInfo(nameof(ParentJobDetails)); }
		}

		#endregion

		#region JobNumber

		[ResourceStringData("9855a692-4abf-4c65-80b8-11a1932f72f8", Caption = "Job Number")]
		public virtual ZString JobNumber
		{
			get { return JobNumberResolver.GetJobNumber(ParentBusinessObject); }
		}

		public ZPropertyInfo JobNumberInfo
		{
			get { return GetZPropertyInfo(nameof(JobNumber)); }
		}

		#endregion

		#region ETA

		[ResourceStringData("c9ae69c2-bdcb-480b-b566-bbedcf1fa4b3", Caption = "ETA")]
		public virtual ZDateTime ETA
		{
			get { return ZDateTime.Empty; }
		}

		public ZPropertyInfo ETAInfo
		{
			get { return GetZPropertyInfo(nameof(ETA)); }
		}

		#endregion

		#region ETD

		[ResourceStringData("eb6890b6-dda4-4248-bc4a-983bafdb05b3", Caption = "ETD")]
		public virtual ZDateTime ETD => ZDateTime.Empty;

		public ZPropertyInfo ETDInfo => GetZPropertyInfo(nameof(ETD));

		#endregion

		#region Load/Origin Port

		[ResourceStringData("c2b568bd-a645-480c-92e1-1c0fee56ae7b", ShortCaption = "Origin Port", Caption = "Load Or Origin Port")]
		public virtual ZString LoadOrOriginPort
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo LoadOrOriginPortInfo
		{
			get { return GetZPropertyInfo(nameof(LoadOrOriginPort)); }
		}

		#endregion

		#region Discharge/DestinationPort

		[ResourceStringData("b0704437-b2b5-49f8-aff7-a540f12fbf2a", ShortCaption = "Dest. Port", Caption = "Discharge Or Destination Port")]
		public virtual ZString DischargeOrDestinationPort
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo DischargeOrDestinationPortInfo
		{
			get { return GetZPropertyInfo(nameof(DischargeOrDestinationPort)); }
		}

		#endregion

		#region Parent Code

		[ResourceStringData("dd5a16ee-6f82-4fca-a092-0ea9c95abf91", Caption = "Parent Code ")]
		public virtual ZString ParentCode
		{
			get
			{
				if (Parent != null)
				{
					try
					{
						return CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)(Parent));
					}
					catch (NoCodePropertyException)
					{
						return ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ParentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ParentCode)); }
		}

		#endregion

		#region Parent Description

		[ResourceStringData("bf981903-a478-488b-a692-124ce6fb7679", ShortCaption = "Parent Desc.", Caption = "Parent Description")]
		public virtual ZString ParentDescription
		{
			get
			{
				if (Parent != null)
				{
					try
					{
						return DescriptionPropertyAttribute.DescriptionFromBusinessObject((BusinessObject)(Parent));
					}
					catch (NoCodePropertyException)
					{
						return ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ParentDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ParentDescription)); }
		}

		#endregion

		#region High Estimate

		public override ZDecimal P9_EstimateVariationFactor
		{
			get { return base.P9_EstimateVariationFactor; }
			set
			{
				base.P9_EstimateVariationFactor = value;
				HighEstimatedDurationInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateP9_EstDuration();
				}
			}
		}

		[ResourceStringData("ProcessTasks.StandardEstimatedDuration", ShortCaption = "Std Est", Caption = "Standard Estimated Duration")]
		public ZDateTime StandardEstimatedDuration
		{
			get
			{
				if (P9_EstDuration.IsValid)
				{
					return TaskDurationCalculator.GetDurationFromHours(StandardEstimateHours);
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		public ZPropertyInfo StandardEstimatedDurationInfo
		{
			get { return GetZPropertyInfo(nameof(StandardEstimatedDuration)); }
		}

		[ResourceStringData("ProcessTasks.HighEstimatedDuration", ShortCaption = "High Est", Caption = "High Estimated Duration")]
		public ZDateTime HighEstimatedDuration
		{
			get { return this.GetHighEstimatedDuration(); }
		}

		public ZPropertyInfo HighEstimatedDurationInfo
		{
			get { return GetZPropertyInfo(nameof(HighEstimatedDuration)); }
		}

		#endregion

		#region RequiresResourceWithCapability

		[ResourceStringData("1caa1435-8c1d-417b-9e4a-7a7a0de697b7", ShortCaption = "Req. User With Capability", Caption = "Requires User With Capability")]
		public bool RequiresResourceWithCapability
		{
			get { return P9_GS_NKAssignedStaffMember.IsEmpty && P9_G4_RequiredCapability.IsValid; }
		}

		#endregion

		#region RequiresResourceWithinGroup

		public bool RequiresResourceWithinGroup
		{
			get { return P9_GS_NKAssignedStaffMember.IsEmpty && AssignedGroup != null; }
		}

		#endregion

		#region Visual Board

		#region NoteText

		[VisualBoardSearchable]
		[ResourceStringData("ProcessTask.VisualBoardNoteText", Caption = "Visual Board Note")]
		[MaxLength(40)]
		public ZString VisualBoardNoteText
		{
			get
			{
				if (!P9_CardNote.IsEmpty)
				{
					return P9_CardNote;
				}
				else if (RequiresResourceWithCapability)
				{
					return Res.GetString("ff1874f5-179a-4856-aad4-0b83758a280d", "Requires {0} capability", RequiredCapability.G4_Code);
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo VisualBoardNoteTextInfo
		{
			get { return GetZPropertyInfo(nameof(VisualBoardNoteText)); }
		}

		#endregion

		#endregion

		#region Workflow

		public WorkflowDescriptor WorkflowDescriptor
		{
			get
			{
				if (workflowDescriptor == null || workflowDescriptor.Code != WorkflowType)
				{
					workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowType);
				}
				return workflowDescriptor;
			}
		}
		WorkflowDescriptor workflowDescriptor;

		public WorkflowDescriptor WorkflowDescriptorCore => WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowTypeCore);

		#endregion

		#region P9_ShareTasksForAllCompanies

		[ReadOnlyMember(nameof(P9_ShareTasksForAllCompanies_ReadOnly))]
		public ZBool P9_ShareTasksForAllCompanies
		{
			get
			{
				if (IsTask)
				{
					return P9_RespondToCascadedEvents || (WorkflowDescriptor != null && !WorkflowDescriptor.AreTasksCompanySpecific);
				}
				return false;
			}
			set
			{
				if (IsTask)
				{
					P9_RespondToCascadedEvents = value;
				}
				P9_ShareTasksForAllCompaniesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo P9_ShareTasksForAllCompaniesInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.P9_ShareTasksForAllCompanies, o => P9_RespondToCascadedEventsInfo); }
		}

		protected bool P9_ShareTasksForAllCompanies_ReadOnly
		{
			get { return (P9_GC.IsValid && P9_GC != GlbCompany.CurrentCompany.PK) || (WorkflowDescriptor != null && !WorkflowDescriptor.AreTasksCompanySpecific); }
		}

		#endregion

		#region Template

		#region TemplateID

		public ZGuid TemplateID
		{
			get
			{
				var id = P9_ParentTemplateID;
				if (!id.IsValid && IsTemplate)
				{
					return PK;
				}
				return id;
			}
		}

		#endregion

		[List("Lookups.Templates")]
		[ResourceStringData("ProcessTasks.SourceTemplatePK", Caption = "Source Template", FullDescription = "The Workflow Template which this item was copied from.")]
		public ZGuid SourceTemplatePK
		{
			get
			{
				var templateID = TemplateID;
				if (templateID.IsValid)
				{
					var templateTask = TemplateVersion;
					return templateTask != null ? templateTask.ParentID : ZGuid.Invalid;
				}
				else
				{
					return templateID.IsEmpty ? ZGuid.Empty : ZGuid.Invalid;
				}
			}
		}

		[ResourceStringData("ProcessTasks.SourceTemplateName", Caption = "Source Template Name", FullDescription = "Name of the Workflow Template which this item was copied from.")]
		public ZString SourceTemplateName
		{
			get
			{
				var templateId = SourceTemplatePK;
				if (templateId.IsEmpty || !templateId.IsValid)
				{
					return ZString.Empty;
				}

				return Factory.Load<ProcessTaskTemplate>(templateId).P0_Name;
			}
		}

		public IWorkflowItem TemplateVersion
		{
			get
			{
				if (this.IsTemplate)
				{
					return this;
				}
				return P9_ParentTemplateID.IsValid ? (IWorkflowItem)TemplateProcessTask ?? TemplateTrigger : null;
			}
		}

		ProcessTask TemplateProcessTask => P9_ParentTemplateID.IsValid ? Factory.Load<ProcessTask>(P9_ParentTemplateID) : null;

		IUniversalTemplateTrigger TemplateTrigger => Factory.LoadTop1<IUniversalTemplateTrigger>(new ZQuery(ProcessTemplateTriggerSchema.PK, P9_ParentTemplateID) { FetchOnlyFromLocalCache = true });

		public ZBool IsNonPersistedRepresentationOfTemplateTrigger { get; set; }

		IWorkflowTrigger processJobTriggerLink;

		public ZBool HasProcessJobTriggerLink => processJobTriggerLink != null;

		void ReportErrorIfGhostedTrigger([CallerMemberName] string callerFunctionName = "")
		{
			if (IsNonPersistedRepresentationOfTemplateTrigger)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Attempted to call {0} for a ghosted trigger representing a Universal trigger. These triggers should never be saved to the database, so there's no need to do other stuff around saving too.", callerFunctionName));
			}
		}

		#endregion

		#region Created Time

		[ResourceStringData("ProcessTask.CreatedTimeLocal", Caption = "Created Time (Local)")]
		public ZDateTime CreatedTimeLocal
		{
			get => CreatedTimeUtc.ToLocalBranchTime(Factory); // Using conversion of UTC to local for consistency with current branch rather than the branch in which the record was created (whatever branch that is).
		}

		[ResourceStringData("ProcessTask.CreatedTimeUtc", Caption = "Created Time (UTC)")]
		public ZDateTime CreatedTimeUtc
		{
			get
			{
				if (IsInDatabase)
				{
					var systemCreateTime = P9_SystemCreateTimeUtc;
					if (!systemCreateTime.IsEmpty)
					{
						return systemCreateTime;
					}
					else
					{
						return GetAddEvent()?.SL_PostedTimeUtc ?? ZDateTime.Empty;
					}
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		StmALog GetAddEvent()
		{
			var logs = Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystemCode));

			if (logs.Length > 1)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "There were multiple ADD events on this record. Why? Task details: [{0}], job details: [{1}]", HumanReadableName, ParentBusinessObject != null ? ParentBusinessObject.HumanReadableName : ZString.Empty));
			}

			return logs.FirstOrDefault();
		}

		#endregion

		#region Iteration

		[MaxLength(3)]
		[List("Lookups.Iterations")]
		[ResourceStringData("54c25010-ddef-4f98-af7a-d920bd1a5db5", Caption = "Quality Iteration", ShortCaption = "Iteration", FullDescription = "The sequence number of the quality iteration associated with this task. Only quality iterations associated with the task's workflow are available to assign here.")]
		public ZString Iteration
		{
			get
			{
				var pivotsForJob = GetIterationPivotsForAllTasksInJob(); // So we don't hit the database for every task in the grid.
				var pivot = pivotsForJob.FirstOrDefault(x => x.P9P_P9_Task == PK);
				var iteration = pivot?.Iteration;

				if (iteration != null)
				{
					iterationEnteredValue = iteration.P9I_Sequence.ToString();
				}

				return iterationEnteredValue;
			}
			set
			{
				CheckMaximumLength(IterationInfo, value);

				var oldValue = Iteration.Trim();
				iterationEnteredValue = value.Trim();

				if (iterationEnteredValue != oldValue)
				{
					AdjustIterationPivots();

					if (!IsValidationSuspended)
					{
						Validation.ValidateIteration();
					}
				}
			}
		}

		protected bool Iteration_ReadOnly => TaskPatchType != TaskPatchType.None;

		IEnumerable<IProcessTaskIterationLinkPivot> GetIterationPivotsForAllTasksInJob()
		{
			if (!P9_ParentID.IsValid || P9_ParentTableCode.IsEmpty)
			{
				return Array.Empty<IProcessTaskIterationLinkPivot>();
			}

			var query = new ZQuery(ProcessTaskIterationLinkPivotSchema.P9P_ParentId, P9_ParentID);
			query.AddToFilter(ProcessTaskIterationLinkPivotSchema.P9P_ParentTableCode, P9_ParentTableCode);

			return Factory.Load<IProcessTaskIterationLinkPivot>(query);
		}

		void AdjustIterationPivots()
		{
			if (!string.IsNullOrEmpty(iterationEnteredValue) && P9_FH_ProcessHeader.IsValid && ZByte.TryParse(iterationEnteredValue, out var sequence))
			{
				var pivot = IterationPivot;

				if (pivot == null || pivot.Iteration.P9I_Sequence != sequence)
				{
					var iteration = GetIterationForSequenceInThisTasksWorkflow(sequence);
					iteration?.TaskPivots.AddNewForTask(this);
				}

				if (pivot != null && pivot.Iteration.P9I_Sequence != sequence)
				{
					pivot.Delete();
				}
			}
			else
			{
				IterationPivot?.Delete();
			}
		}

		IProcessTaskIterationLink GetIterationForSequenceInThisTasksWorkflow(ZByte sequence)
		{
			var query = GetIterationsForThisTasksWorkflowQuery(ProcessTaskIterationLinkSchema.P9I_SystemCreateTimeUtc, orderByDesc: true);
			query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_Sequence, sequence);

			return Factory.LoadTop1<IProcessTaskIterationLink>(query);
		}

		internal ZQuery GetIterationsForThisTasksWorkflowQuery(SchemaColumn orderByColumn, bool orderByDesc)
		{
			var query = new ZQuery(ProcessTaskIterationLinkSchema.P9I_FH_IterationWorkflow, P9_FH_ProcessHeader);
			query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_LinkType, IterationLinkTypeList.Codes.QualityIterationTask);
			query.AddToFilter(ProcessTaskIterationLinkSchema.P9I_Outcome, IterationLinkOutcomeList.Codes.IterationRequired);
			query.OrderBy = orderByColumn.Name + (orderByDesc ? " desc" : string.Empty);

			return query;
		}

		ZString iterationEnteredValue;

		public ZPropertyInfo IterationInfo => GetZPropertyInfo(nameof(Iteration));

		bool HasIterationLink
		{
			get
			{
				var query = new ZQuery(ProcessTaskIterationLinkSchema.P9I_P9_IterationTask, PK);
				return Factory.LoadTop1<IProcessTaskIterationLink>(query) != null;
			}
		}

		#endregion

		#region P9_GG_AssignedGroupCode

		public ZString P9_GG_AssignedGroupCode
		{
			get => AssignedGroup?.GG_Code ?? ZString.Empty;
			set
			{
				if (value.IsEmpty)
				{
					P9_GG_AssignedGroup = ZGuid.Empty;
				}
				else
				{
					var group = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, value);
					if (group != null)
					{
						P9_GG_AssignedGroup = group.PK;
					}
				}
			}
		}

		public ZPropertyInfo P9_GG_AssignedGroupCodeInfo => GetWrappedZPropertyInfo(nameof(P9_GG_AssignedGroupCode), _ => P9_GG_AssignedGroupInfo);

		#endregion

		#region TimeBecameStartable & WorkingTimeSinceBecomingStartable

		[ResourceStringData("38D0AC6E-2B60-4CED-9E69-DA4284DDE9E8", Caption = "Time Task Became Startable (Local)", ShortCaption = "Startable Since", FullDescription = "Local time the task became startable in the current component.")]
		public ZDateTime TimeBecameStartable
		{
			get
			{
				return TimeBecameStartableUtc.ToLocalBranchTime(Factory);
			}
		}

		public ZDateTime TimeBecameStartableUtc
		{
			get
			{
				var time = ZDateTime.Empty;
				var ev = Factory.Load<StmALog>(Logs.MostRecentLogByEventTimeQuery(Events.StartabilityChanged, null)).FirstOrDefault();

				if (ev?.SL_Reference.Contains("|SRT=Y") == true)
				{
					if (ProcessHeader != null)
					{
						time = ProcessHeader.FH_ReleaseDateTime;
					}

					if (ev.SL_PostedTimeUtc > time)
					{
						time = ev.SL_PostedTimeUtc;
					}
				}

				return time;
			}
		}

		public TimeSpan WorkingTimeSinceBecomingStartable
		{
			get
			{
				return ProcessHeader != null ? ProcessHeader.GetWorkingTimeSinceTaskBecameStartable(TimeBecameStartableUtc) : TimeSpan.Zero;
			}
		}

		[ResourceStringData("B2ED613C-4B1D-47CA-AB9D-55A4F44FF4A7", Caption = "Working Time Since Becoming Startable", ShortCaption = "Startable For", FullDescription = "Working time since the task became startable in the current component (in the corresponding branch/department time context).")]
		public ZString WorkingTimeSinceBecomingStartableAsString
		{
			get
			{
				var timespan = WorkingTimeSinceBecomingStartable;
				return timespan != TimeSpan.Zero ? timespan.ToHoursAndMinutesString() : "-";
			}
		}

		#endregion

		#region TaskPatchType

		public virtual TaskPatchType TaskPatchType => TaskPatchType.None;

		#endregion

		public IDisposable SuspendUpdatingIterationPivots()
		{
			return iterationPivotUpdateSuspender.Value.Suspend();
		}

		internal bool IsUpdatingPivotsSuspended => iterationPivotUpdateSuspender.IsValueCreated && iterationPivotUpdateSuspender.Value.IsSuspended;

		readonly Lazy<ActionSuspender> iterationPivotUpdateSuspender = new Lazy<ActionSuspender>(() => new ActionSuspender());

		void UpdateIterationPivots()
		{
			if (!IsBeingCloned && !IsUpdatingPivotsSuspended)
			{
				TaskIterationHelper.TryAutomaticallyAssignIterationAfterChange(this);
			}
		}

		#endregion

		#region Related Business Objects

		#region Completion Trigger Actions

		public IEnumerable<ProcessTaskNotification> ProcessTaskNotificationsWithoutMultipleIndexes
		{
			get
			{
				if (IsMilestoneOrWorkflowTrigger)
				{
					return Factory.Load<ProcessTaskNotification>(ProcessTaskNotificationsQuery);
				}
				else
				{
					return Enumerable.Empty<ProcessTaskNotification>();
				}
			}
		}

		public ZQuery ProcessTaskNotificationsQuery => new ZQuery(ProcessTaskNotificationSchema.PQ_P9, PK) { FetchOnlyFromLocalCache = !IsInDatabase };

		[ChildEditable(true)]
		public ProcessTaskNotificationCollection ProcessTaskNotifications
		{
			get
			{
				if (processTaskNotifications == null)
				{
					processTaskNotifications = GetNewProcessTaskNotificationCollection();
					RegisterEditableChildObject(processTaskNotifications);
				}

				return processTaskNotifications;
			}
		}
		ProcessTaskNotificationCollection processTaskNotifications;

		protected virtual ProcessTaskNotificationCollection GetNewProcessTaskNotificationCollection()
		{
			return new ProcessTaskNotificationCollection(this);
		}

		public void AddCompletionTriggerActionsFetchHint()
		{
			if (IsMilestoneOrWorkflowTrigger)
			{
				Factory.AddFetchHint(ProcessTaskNotificationSchema.PQ_P9, PK);
			}
		}

		#endregion

		#region ProcessHeader

		IProcessHeader processHeader;

		public IProcessHeader ProcessHeader
		{
			get
			{
				if (!P9_FH_ProcessHeader.IsValid || (processHeader?.IsDeleted ?? false))
				{
					return processHeader = null;
				}

				if (processHeader == null || processHeader.PK != P9_FH_ProcessHeader)
				{
					processHeader = Factory.Load<IProcessHeader>(P9_FH_ProcessHeader);
				}

				return processHeader;
			}
		}

		public IProcessHeader JobHeader
		{
			get { return ProcessHeader.JobHeader; }
		}

		#endregion

		#region Parent

		public virtual ControllerID ParentControllerID
		{
			get
			{
				JobInvoicingConsumerType type = Parent == null ? null : JobInvoicingConsumerTypes.New()[Parent.WorkflowType];
				return type == null ? null : type.ControllerID;
			}
		}

		protected internal virtual Type ParentType
		{
			get { return typeof(BusinessObject); }
		}

		/// <summary>
		///		Gets the actual parent business object. i.e. if the process task is line trigger it returns the corresponding line business object (if <see cref="LineTriggerPK"/>
		///		is specified), otherwise returns the business object specified in <see cref="ParentPK"/>.
		/// </summary>
		public IWorkflowProvider Parent
		{
			get { return this.GetJob() as IWorkflowProvider; }
		}

		public BusinessObject ParentBusinessObject
		{
			get { return (BusinessObject)Parent; }
		}

		public virtual void AddParentFetchHint()
		{
			if (P9_ParentID.IsValid && ParentType != typeof(BusinessObject) && !(typeof(NonPersistentBusinessObject).IsAssignableFrom(ParentType)))
			{
				Factory.AddFetchHint(ParentType, P9_ParentID);
			}
		}

		public ProcessTaskCollection ParentTaskCollection
		{
			get { return Parent != null ? Parent.WorkflowItems : null; }
		}

		bool IsParentLoaded
		{
			get
			{
				bool result = false;
				if (ParentType != typeof(BusinessObject))
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(ParentPKColumn, P9_ParentID);
					query.FetchOnlyFromLocalCache = true;
					result = Factory.LoadTop1(ParentType, query) != null;
				}
				return result;
			}
		}

		SchemaGuidColumn ParentPKColumn
		{
			get
			{
				string parentTableName = BusinessObjectFactory.GetTableNameFromType(ParentType);
				return ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(parentTableName);
			}
		}

		#endregion

		#region Working Days Helper

		internal IWorkTimeArithmetic WorkDaysHelper
		{
			get { return workDaysHelper ?? (workDaysHelper = WorkingDays.GetInstance(Factory, GlbDepartment.CurrentDepartment.PK, GlbBranch.CurrentBranch.PK, AssignedStaffMember != null ? AssignedStaffMember.PK : ZGuid.Empty)); }
		}

		IWorkTimeArithmetic workDaysHelper;

		TimeSpan GetTimeDifferenceInLocalTime(ZDateTime startTime, ZDateTime endTime)
		{
			if (endTime < startTime)
			{
				return TimeSpan.Zero;
			}
			else if (startTime.Date == endTime.Date)
			{
				return endTime - startTime;
			}
			else
			{
				return WorkDaysHelper.TimeDifference(startTime.ToDateTime(), endTime.ToDateTime());
			}
		}

		TimeSpan GetTimeDifferenceInUtcTime(ZDateTime startTimeUTC, ZDateTime endTimeUTC)
		{
			return GetTimeDifferenceInLocalTime(Env.Time.GetLocalTimeFromUtc(startTimeUTC.ToDateTime()),
				Env.Time.GetLocalTimeFromUtc(endTimeUTC.ToDateTime()));
		}

		#endregion

		#region Related Tasks

		[BusinessObjectTestExclude] // Can be null
		public ProcessTaskCollection SuspendedAndWorkingTasksForAssignedUser
		{
			get
			{
				ProcessTaskCollection result = null;
				if (!P9_GS_NKAssignedStaffMember.IsEmpty && P9_GS_NKAssignedStaffMember.IsValid)
				{
					result = new ProcessTaskCollection(Factory);
					ZQuery query = new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, P9_GS_NKAssignedStaffMember);

					ZQuery statusQuery = new ZQuery(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Suspended);
					statusQuery.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_Status, SQLComparisonOperator.Equal, ProcessTaskStatusCodeList.Codes.Working);

					query.AddToFilter(statusQuery);

					result.Load(query);
					result.Sort(ProcessTasksSchema.P9_Status.Name, ListSortDirection.Descending);
				}

				return result;
			}
		}

		[BusinessObjectTestExclude]
		public ProcessTaskCollection SuspendedTasksForAssignedUser
		{
			get
			{
				ProcessTaskCollection result = null;
				if (!P9_GS_NKAssignedStaffMember.IsEmpty && P9_GS_NKAssignedStaffMember.IsValid)
				{
					result = new ProcessTaskCollection(Factory);
					result.Load(GetSuspendedTasksQuery());
					result.Sort(ProcessTasksSchema.P9_Status.Name, ListSortDirection.Descending);
				}

				return result;
			}
		}

		protected virtual ZQuery GetSuspendedTasksQuery()
		{
			ZQuery query = new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, P9_GS_NKAssignedStaffMember);
			ZQuery statusQuery = new ZQuery(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Suspended);
			query.AddToFilter(statusQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Extra Resources

		[ChildEditable(true)]
		public ProcessTaskExtraResourceCollection ExtraResources
		{
			get
			{
				if (fExtraResources == null)
				{
					fExtraResources = GetNewExtraResources();
					fExtraResources.Load();
					RegisterEditableChildObject(fExtraResources);
				}

				return fExtraResources;
			}
		}

		protected virtual ProcessTaskExtraResourceCollection GetNewExtraResources()
		{
			return new ProcessTaskExtraResourceCollection(this);
		}

		ProcessTaskExtraResourceCollection fExtraResources;

		internal void AddProcessTaskExtraResourceFetchHint()
		{
			if (IsTask)
			{
				Factory.AddFetchHint(ProcessTaskExtraResourceSchema.PE_P9, PK);
			}
		}

		#endregion

		#region Iteration Links

		public IProcessTaskIterationLinkCollection IterationLinks
		{
			get
			{
				if (iterationLinks == null)
				{
					iterationLinks = ObjectFactory.Get<IProcessTaskIterationLinkCollection>("IProcessTaskIterationLinkCollection", this);
				}

				return iterationLinks;
			}
		}

		IProcessTaskIterationLinkCollection iterationLinks;

		[ChildEditable(true)]
		public ProcessTaskIterationLinkViewModelCollection IterationLinksViewModel
		{
			get
			{
				if (iterationLinksViewModel == null)
				{
					iterationLinksViewModel = new ProcessTaskIterationLinkViewModelCollection(this);

					RegisterEditableChildObject(iterationLinksViewModel);
				}

				return iterationLinksViewModel;
			}
		}

		ProcessTaskIterationLinkViewModelCollection iterationLinksViewModel;

		public IProcessTaskIterationLinkPivot IterationPivot => Factory.LoadTop1<IProcessTaskIterationLinkPivot>(new ZQuery(ProcessTaskIterationLinkPivotSchema.P9P_P9_Task, PK));

		#endregion

		#endregion

		#region Workflow Type

		public string WorkflowType
		{
			get
			{
				if (P9_LineTriggerType.IsEmpty)
				{
					return WorkflowTypeCore;
				}
				else
				{
					return P9_LineTriggerType;
				}
			}
		}

		protected virtual string WorkflowTypeCore => Parent == null ? (ZString)WorkflowDescriptors.StandAloneTaskWorkflowDescriptor : Parent.WorkflowType;

		internal void RefreshWorkflowType()
		{
			Lookups.RefreshTypeList();
		}

		#endregion

		#region Template Matching Logic

		public bool IsMatchForItemTemplateMerge(ProcessTask itemTemplate)
		{
			switch (P9_Type)
			{
				case Core.Constants.Workflow.MilestoneType:
				case Core.Constants.Workflow.WorkflowTriggerType:
					return new TriggerToTemplateComparer().Equals(this, itemTemplate);

				default:
					return new TaskToTemplateComparer().Equals(this, itemTemplate);
			}
		}

		public bool IsDuplicateItemTemplateApplication(ProcessTask potentialDuplicate)
		{
			var result = IsMatchForItemTemplateMerge(potentialDuplicate);

			if (!potentialDuplicate.IsTemplateReapplication)
			{
				return result;
			}

			switch (P9_Type)
			{
				case Core.Constants.Workflow.MilestoneType:
				case Core.Constants.Workflow.WorkflowTriggerType:
					return result;
				default:
					return new TaskToTemplateReapplicationComparer().Equals(this, potentialDuplicate);
			}
		}

		public bool IsTemplateReapplication { get; set; }

		protected internal virtual IEqualityComparer<ProcessTask> GetTriggerConditionsComparer()
		{
			return new TaskToTemplateTriggerConditionsComparer();
		}

		#endregion

		#region Milestones and Exceptions

		#region IsTask / IsMilestone / IsException

		public ZBool IsTask
		{
			get { return !IsMilestone && !IsWorkflowTrigger && !IsException; }
		}

		public ZBool IsStandaloneTask
		{
			get { return IsTask && P9_ParentID.IsEmpty; }
		}

		public bool IsTemplateTriggerOrMilestone
		{
			get { return !IsTask && IsTemplate; }
		}

		public bool IsTemplateTask
		{
			get { return IsTask && IsTemplate; }
		}

		public bool IsTemplate => P9_ParentTableCode == ProcessTaskTemplateSchema.Constants.Prefix;

		[ResourceStringData("2afeec81-1007-4db2-b71b-17d639187f00", Caption = "Is Milestone")]
		[VisualBoardSearchable]
		public ZBool IsMilestone
		{
			get { return !IsDeleted && P9_Type == Core.Constants.Workflow.MilestoneType; }
			set
			{
				if (value != IsMilestone)
				{
					P9_Type = value ? Core.Constants.Workflow.MilestoneType : "";
				}
				Validation.ValidateIsMilestone();
				IsMilestoneInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsMilestoneInfo
		{
			get { return GetZPropertyInfo(nameof(IsMilestone)); }
		}

		public ZBool IsException
		{
			get { return P9_Type == Core.Constants.Workflow.ExceptionType; }
			set
			{
				if (IsException != value)
				{
					P9_Type = value ? Core.Constants.Workflow.ExceptionType : "";
					IsExceptionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsExceptionInfo
		{
			get { return GetZPropertyInfo(nameof(IsException)); }
		}

		public ZBool IsWorkflowTrigger
		{
			get { return P9_Type == Core.Constants.Workflow.WorkflowTriggerType; }
			set
			{
				if (IsWorkflowTrigger != value)
				{
					P9_Type = value ? Core.Constants.Workflow.WorkflowTriggerType : "";
				}
			}
		}

		public ZBool IsMilestoneOrWorkflowTrigger
		{
			get { return IsMilestone || IsWorkflowTrigger; }
		}

		#endregion

		public override bool ReadOnly
		{
			get
			{
				if (IsNonPersistedRepresentationOfTemplateTrigger && HasProcessJobTriggerLink)
				{
					return false;
				}

				return !ProcessTaskSecurityMan.IsAllowEdit(this) || base.ReadOnly;
			}

			set => base.ReadOnly = value;
		}

		protected internal virtual bool GetShouldPropertiesBeReadOnly(PropertyDescriptor propertyDescriptor)
		{
			if (typeof(AutoProcessTasks).IsAssignableFrom(propertyDescriptor.ComponentType)
				&& CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, propertyDescriptor))
			{
				return true;
			}
			else if (IsNonPersistedRepresentationOfTemplateTrigger && HasProcessJobTriggerLink)
			{
				return propertyDescriptor.Name != nameof(ITriggerConditions.TriggerFiredCountdown);
			}
			else
			{
				return ReadOnly;
			}
		}

		public ModuleIdentifier ModuleID => moduleID ?? (moduleID = ParentControllerID?.GetModuleId(Factory));
		ModuleIdentifier moduleID;

		#region P9_SE_NKMilestoneEvent

		[ReadOnly(true)]
		[ActionField(CollectionType = typeof(StmEventCodeDescriptionPairList), FieldType = ActionFieldType.Code)]
		[CustomisedControlExclude]
		[BusinessObjectTestExclude] // Can't set this property without using ITriggerConditions interface.
		public override ZString P9_SE_NKMilestoneEvent
		{
			get { return base.P9_SE_NKMilestoneEvent; }
			set
			{
				EnsureCanSetTriggerCondition(nameof(P9_SE_NKMilestoneEvent));

				if (base.P9_SE_NKMilestoneEvent != value)
				{
					var forceRefreshActualDate = !base.P9_SE_NKMilestoneEvent.IsEmpty && !value.IsEmpty;
					base.P9_SE_NKMilestoneEvent = value;
					if (!IsTemplateTask && IsMilestoneOrWorkflowTrigger)
					{
						WorkflowDefaultDateProvider.DefaultDatesFromMilestoneEvent(this, this.GetJob(), true, forceRefreshActualDate);
					}
				}
			}
		}

		#endregion

		#region P9_SE_NKExceptionEvent

		public ZString P9_SE_NKExceptionEventDescription => GetExceptionEventDescription(P9_SE_NKExceptionEvent);

		public ZPropertyInfo P9_SE_NKExceptionEventDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(P9_SE_NKExceptionEventDescription)); }
		}

		public ZString GetExceptionEventDescription(ZString exceptionCode) => GetProcessWorkflowExceptionType(exceptionCode)?.WET_DescriptionMultilingual ?? ZString.Empty;

		#endregion

		#region P9_SE_NKTaskCompletionEvent

		[List("TriggerConditions.Lookups.MilestoneEventTypes")]
		[ReadOnlyMember(nameof(P9_SE_NKTaskCompletionEvent_ReadOnly))]
		[ActionField(CollectionType = typeof(StmEventCodeDescriptionPairList), FieldType = ActionFieldType.Code)]
		public override ZString P9_SE_NKTaskCompletionEvent
		{
			get { return base.P9_SE_NKTaskCompletionEvent; }
			set { base.P9_SE_NKTaskCompletionEvent = value; }
		}

		protected bool P9_SE_NKTaskCompletionEvent_ReadOnly
		{
			get { return !IsTask || IsStandaloneTask; }
		}

		#endregion

		#region Description with Reference

		public ZString DescriptionWithReference => this.GetDescriptionWithReference();

		public ZPropertyInfo DescriptionWithReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(DescriptionWithReference)); }
		}

		protected bool P9_SE_NKExceptionEvent_ReadOnly
		{
			get { return !IsMilestone && !IsException; }
		}

		#endregion

		#region ReferenceCode

		[BusinessObjectTestExclude]
		[List("ReferenceCodeList")]
		[ResourceStringData("a27f3590-dd7c-41ad-a136-26aa55f9ca25", Caption = "Reference Code")]
		public virtual ZString ReferenceCode
		{
			get { return ""; }
			set
			{
				Validation.ValidateReferenceCode();
				ReferenceCodeInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ReferenceCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ReferenceCode)); }
		}

		public virtual CodeDescriptionPairList ReferenceCodeList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public override ZGuid P9_ReferencedID
		{
			get { return base.P9_ReferencedID; }
			set
			{
				if (P9_ReferencedID != value)
				{
					base.P9_ReferencedID = value;
					if (!IsTemplateTask && IsMilestoneOrWorkflowTrigger)
					{
						WorkflowDefaultDateProvider.DefaultDatesFromMilestoneEvent(this, this.GetJob(), true, false);
					}
				}
			}
		}

		#endregion

		#region Synchronizing StmALog and Parent Date Properties

		//Most of these references need to be changed.
		//We could determine the Offset from Branch logic that looks at UNLOCO also
		[ReadOnlyMember(nameof(P9_ScheduledDate_ReadOnly))]
		public new ZDateTime P9_ScheduledDate
		{
			get { return base.P9_ScheduledDate; }
			set { TrySetScheduledDateOffset(value.ToOffset()); }
		}

		//This property should be used to set ScheduleDate
		public new ZDateTimeOffset P9_ScheduledDateOffset
		{
			get { return base.P9_ScheduledDateOffset; }
			set
			{
				using (SupressScheduledDateChangeErrorReport())
				{
					TrySetScheduledDateOffset(value);
				}
			}
		}

		void TrySetScheduledDateOffset(ZDateTimeOffset value)
		{
			IEnumerable<RelatedMilestoneEventDefinition> GetReferencesBuilders(bool shouldUpdateRelatedEvents, bool suspendMilestoneEstimate, bool suspendEstiamteDateChanged)
			{
				var eventType = Events.All[P9_SE_NKMilestoneEvent];
				if (shouldUpdateRelatedEvents)
				{
					var proxy = new WorkflowMilestoneProxy(this);
					if (!suspendMilestoneEstimate && eventType != null)
					{
						yield return new RelatedMilestoneIsEstimateEvent(proxy, P9_SE_NKMilestoneEvent, newDate: value.ToDateTimeOffsetSafe(), previousDate: proxy.EstimateDate, originalDate: proxy.OriginalEstimateDate);
					}
					if (!suspendEstiamteDateChanged)
					{
						yield return new RelatedMilestoneESTEventDefinition(proxy, newDate: value.ToDateTimeOffsetSafe(), previousDate: proxy.EstimateDate, originalDate: proxy.OriginalEstimateDate);
					}
				}
			}

			if (scheduledDateChangeSuspended > 0)
			{
				return;
			}

			if (value.IsValidSmallDateTime)
			{
				value = value.ToSmallDateTimeFloor();
			}

			if (value != base.P9_ScheduledDateOffset)
			{
				var eventType = Events.All[P9_SE_NKMilestoneEvent];

				var isMilestoneEventSuspended = Suspender<string>.IsSuspended(ref suspender, P9_SE_NKMilestoneEvent);
				var isEstimatedDateChangedSuspended = Suspender<string>.IsSuspended(ref suspender, Events.EstimatedDateChangedCode);

				var shouldUpdateEventLog_IgnoringSuspend = ShouldUpdateEventLog_IgnoringSuspend() && (!isMilestoneEventSuspended || !isEstimatedDateChangedSuspended);

				var eventUpdateStrategies = GetReferencesBuilders(shouldUpdateEventLog_IgnoringSuspend, isMilestoneEventSuspended, isEstimatedDateChangedSuspended).ToArray();

				if (TryGenerateReferenceForEvent(eventUpdateStrategies, P9_TriggerCondition, P9_TriggerConditionValue, out var eventValues))
				{
					base.P9_ScheduledDateOffset = value;

					if (IsMilestone)
					{
						if (P9_OriginalScheduledDateUtc.IsEmpty)
						{
							var originalDate = P9_ScheduledDateUtcInfo.OriginalValue.IsValid && !P9_ScheduledDateUtcInfo.OriginalValue.IsEmpty ? (ZDateTime)P9_ScheduledDateUtcInfo.OriginalValue : P9_ScheduledDateUtc;
							P9_OriginalScheduledDateUtc = originalDate;
						}

						using (SuspendedScheduledDateChange())
						{
							if (Parent != null && !IsDefaultingFromSameEvent && eventType != null)
							{
								var estimateDateProperties = EventDatePropertyAttribute.FindPropertyInfos(ParentBusinessObject, eventType, EstimateActual.MilestoneEstimateOnly);
								foreach (var estimateDateProperty in estimateDateProperties)
								{
									ZDateTime estimateDate = (ZDateTime)estimateDateProperty.Property.Value;
									if (value.ToZDateTime() != estimateDate)
									{
										estimateDateProperty.Property.Value = value.ToZDateTime();
									}
								}
							}

							using (Suspender<string>.Suspend(ref suspender, Events.EstimatedDateChangedCode))
							{
								foreach (var pair in eventValues)
								{
									new UpdateOrCreateOrDeleteEstimatedEvent(this, pair.Strategy).Apply(pair.Value.ToEventValue());
								}
							}
						}
					}

					P9_ScheduledDateForBindingInfo.RefreshBinding();
				}
			}

			OnSetScheduledDateCore(new ZDateTimeOffset(value));
		}

		protected virtual void OnSetScheduledDateCore(ZDateTimeOffset value)
		{
		}

		int scheduledDateChangeSuspended;

#if DEBUG
		public
#endif
		IDisposable SuspendedScheduledDateChange()
		{
			scheduledDateChangeSuspended++;
			return new DisposableAction(() => scheduledDateChangeSuspended--);
		}

#if DEBUG
		public void DefaultDatesFromMilestoneEvent_ForTest(bool clearNonEmptyDate) => WorkflowDefaultDateProvider.DefaultDatesFromMilestoneEvent(this, this.GetJob(), clearNonEmptyDate, forceRefreshActualDate: false);
#endif
		#endregion

		#region ReadOnly for Estimates

		protected bool P9_EstDuration_ReadOnly
		{
			get { return IsMilestone || IsException; }
		}

		protected bool P9_ScheduledDate_ReadOnly => IsTemplateTask || DisallowCreateEventBySettingDates;

		#endregion

		#region CreateMilestoneException

		public ProcessTask CreateMilestoneException() => CreateMilestoneException(P9_SE_NKExceptionEvent);

		public ProcessTask MilestoneException => GetMilestoneException(P9_SE_NKExceptionEvent);

		internal ProcessTask CreateMilestoneException(ZString exceptionCode)
		{
			if (exceptionCode.IsEmpty)
			{
				return null;
			}

			if (exceptionCode.Equals(P9_SE_NKExceptionEvent))
			{
				P9_MilestoneExceptionAdded = ZDateTime.Now; // We still need to mark exception as added on cancelled jobs otherwise WEX thrashes forever.
			}

			if (IsParentCancelled())
			{
				return null;
			}

			var exceptionType = GetProcessWorkflowExceptionType(exceptionCode);

			if (exceptionType == null)
			{
				return null;
			}

			var result = GetMilestoneException(exceptionCode);
			if (result == null)
			{
				result = IsParentLoaded ? Parent.WorkflowItems.Exceptions.AddNew() : (ProcessTask)Factory.New(GetType());
				result.P9_ParentID = P9_ParentID;
				result.P9_ParentTableCode = P9_ParentTableCode;

				result.P9_Type = Core.Constants.Workflow.ExceptionType;
				result.P9_Description = P9_Description;
				result.TriggerConditions.TriggerEventCode = P9_SE_NKMilestoneEvent;
				result.P9_GS_NKAssignedStaffMember = P9_GS_NKAssignedStaffMember;
				result.P9_GG_AssignedGroup = P9_GG_AssignedGroup;
			}

			result.P9_SE_NKExceptionEvent = exceptionType.WET_Code;

			ProcessTaskLogger.AddExceptionRaisedLog(exceptionCode);

			return result;
		}

		internal ProcessTask GetExceptionMilestone(IZType description)
		{
			var query = new ZQuery();
			query.AddToFilter(ProcessTasksSchema.P9_ParentID, P9_ParentID);
			query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, P9_ParentTableCode);
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, P9_SE_NKMilestoneEvent);
			query.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);
			query.AddToFilter(ProcessTasksSchema.P9_Description, description);
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKExceptionEvent, P9_SE_NKExceptionEvent);

			return (ProcessTask)Factory.LoadTop1(GetType(), query);
		}

		ProcessTask GetMilestoneException(ZString exceptionCode)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(ProcessTasksSchema.P9_ParentID, P9_ParentID);
			query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, P9_ParentTableCode);
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, P9_SE_NKMilestoneEvent);
			query.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.ExceptionType);
			query.AddToFilter(ProcessTasksSchema.P9_Description, P9_Description);
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKExceptionEvent, exceptionCode);

			return (ProcessTask)Factory.LoadTop1(GetType(), query);
		}

		#endregion

		#region IsMilestoneOverdueAndHasOpenException

		public bool IsMilestoneOverdueAndHasOpenException
		{
			get
			{
				Event milestoneEvent = ZArchitecture.Business.Events.All[P9_SE_NKMilestoneEvent];
				return
					IsMilestoneOverdue &&
					milestoneEvent != null &&
					Parent != null &&
					Parent.WorkflowItems.Exceptions.HasOpenExceptionsWithEventType(milestoneEvent, P9_Description);
			}
		}

		bool IsMilestoneOverdue
		{
			get { return P9_ActualDate.IsEmpty && ZDateTime.Now > P9_ScheduledDate; }
		}

		#endregion

		#region IsExceptionActioned
		
		[BusinessObjectTestExclude]
		public ZBool IsExceptionActioned
		{
			get { return P9_Status == ExceptionStatusCodeList.Codes.Actioned; }
			set
			{
				if (!IsException)
				{
					return;
				}

				using (IsSettingExceptionActioned())
				{
					SetExceptionActioned(value);
				}

				IsExceptionActionedInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo IsExceptionActionedInfo
		{
			get { return GetZPropertyInfo(nameof(IsExceptionActioned)); }
		}

		bool exceptionStaffWasAutoAssigned;

		[BusinessObjectTestExclude]
		public ZBool IsExceptionActionedForBinding
		{
			get => IsExceptionActioned;
			set
			{
				using (SuppressSetExceptionDefaultCauseAndResolution())
				{
					IsExceptionActioned = value;
				}

				IsExceptionActionedForBindingInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo IsExceptionActionedForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(IsExceptionActionedForBinding)); }
		}

		bool suppressSetExceptionDefaultCauseAndResolution;

		IDisposable SuppressSetExceptionDefaultCauseAndResolution()
		{
			suppressSetExceptionDefaultCauseAndResolution = true;
			return new DisposableAction(() => suppressSetExceptionDefaultCauseAndResolution = false);
		}

		void SetExceptionActioned(ZBool value)
		{
			if (IsExceptionActioned == value)
			{
				return;
			}

			if (value)
			{
				if (P9_GS_NKAssignedStaffMember.IsEmpty && P9_GG_AssignedGroup.IsEmpty && WorkflowDataRegistry.Instance.ExceptionAutoAssignStaff.Value)
				{
					P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
					exceptionStaffWasAutoAssigned = true;
				}

				ProcessTaskLogger.AddExceptionActionedLog(ExceptionTypeCode);
			}
			else
			{
				if (exceptionStaffWasAutoAssigned)
				{
					P9_GS_NKAssignedStaffMember = ZString.Empty;
					exceptionStaffWasAutoAssigned = false;
				}

				P9_Status = ExceptionStatusCodeList.Codes.Open;
				P9_CompletedTimeUtc = ZDateTime.Empty;

				ProcessTaskLogger.RemoveExceptionLog(ExceptionTypeCode, Events.ExceptionActioned);

				return;
			}

			P9_Status = ExceptionStatusCodeList.Codes.Actioned;
			P9_CompletedTimeUtc = ZDateTime.UtcNow;

			if (suppressSetExceptionDefaultCauseAndResolution || ExceptionType == null)
			{
				return;
			}

			if (ExceptionCausePK == Guid.Empty && ExceptionType.WET_IsCauseRequired)
			{
				ExceptionCausePK = ExceptionType.Causes.Find(c => c.WEC_IsDefault && c.WEC_IsActive).SingleOrDefault()?.PK ?? ZGuid.Empty;
			}

			if (ExceptionResolutionPK == Guid.Empty && ExceptionType.WET_IsResolutionRequired)
			{
				ExceptionResolutionPK = ExceptionType.Resolutions.Find(c => c.WER_IsDefault && c.WER_IsActive).SingleOrDefault()?.PK ?? ZGuid.Empty;
			}
		}

		bool isSettingExceptionActioned;

		IDisposable IsSettingExceptionActioned()
		{
			if (!isSettingExceptionActioned)
			{
				isSettingExceptionActioned = true;
				return new DisposableAction(() => isSettingExceptionActioned = false);
			}
			return null;
		}

		#endregion

		#region ProcessWorkflowException

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo == P9_SE_NKExceptionEventInfo)
			{
				return false;
			}

			return base.IsValidationEnabledCore(propertyInfo);
		}

		public bool ShouldAddProcessWorkflowExceptionFetchHintsForLoadChildEditableObjects { get; set; }

		public void AddProcessWorkflowExceptionFetchHint()
		{
			if (HasProcessWorkflowException && IsInDatabase)
			{
				var processWorkflowExceptionQuery = new ZQuery(ProcessWorkflowExceptionSchema.WEX_P9_ProcessTask, PK);
				Factory.AddFetchHint(typeof(ProcessWorkflowException), processWorkflowExceptionQuery);

				var processWorkflowExceptionTypeQuery = new ZQuery(ProcessWorkflowExceptionTypeSchema.WET_Code, ExceptionTypeCode);
				Factory.AddFetchHint(typeof(ProcessWorkflowExceptionType), processWorkflowExceptionTypeQuery);
			}
		}

		[BusinessObjectTestExclude]
		[MaxLength(3)]
		[List("Lookups.ExceptionTypes")]
		public ZString ExceptionTypeCode
		{
			get => IsException || IsMilestone ? P9_SE_NKExceptionEvent : ZString.Empty;
			set
			{
				if (IsException || IsMilestone && P9_SE_NKExceptionEvent != value)
				{
					exceptionType = null;
					P9_SE_NKExceptionEvent = value;
					ExceptionTypeCategory = ZString.Empty;
					ExceptionCausePK = ZGuid.Empty;
					ExceptionResolutionPK = ZGuid.Empty;

					if (!IsValidationSuspended)
					{
						Validation.ValidateExceptionTypeCode();
					}

					if (IsException && ExceptionType != null)
					{
						if (P9_Description.IsEmpty)
						{
							P9_Description = new ZString(ExceptionType.WET_DescriptionMultilingual).Substring(0, P9_DescriptionInfo.MaxLength - 1);
						}
					}

					SetAdditionalFieldExceptionTypeCode();
				}
			}
		}

		public virtual void SetAdditionalFieldExceptionTypeCode() { }

		public virtual ZPropertyInfo ExceptionTypeCodeInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ExceptionTypeCode)); }
		}

		public ZBool HasProcessWorkflowException => !ExceptionTypeCode.IsEmpty;

		public ZBool HasNoProcessWorkflowException => !HasProcessWorkflowException;

		ProcessWorkflowException processWorkflowException;

		public ProcessWorkflowException ProcessWorkflowException
		{
			get
			{
				if (!IsException || HasNoProcessWorkflowException)
				{
					return null;
				}

				if (processWorkflowException == null)
				{
					processWorkflowException = Factory.LoadTop1<ProcessWorkflowException>(new ZQuery(ProcessWorkflowExceptionSchema.WEX_P9_ProcessTask, PK));
				}

				if (processWorkflowException == null)
				{
					processWorkflowException = Factory.New<ProcessWorkflowException>();

					using (processWorkflowException.SuspendSettingHasChanges())
					{
						processWorkflowException.WEX_P9_ProcessTask = PK;
					}
				}

				RegisterEditableChildObject(processWorkflowException);

				return processWorkflowException;
			}
		}

		ProcessWorkflowExceptionType exceptionType;

		ProcessWorkflowExceptionType GetProcessWorkflowExceptionType(ZString exceptionTypeCode) =>
			Factory.GetCachedValue("ProcessWorkflowExceptionType" + exceptionTypeCode,
				() => Factory.LoadTop1<ProcessWorkflowExceptionType>(new ZQuery(ProcessWorkflowExceptionTypeSchema.WET_Code, exceptionTypeCode)));

		public ProcessWorkflowExceptionType ExceptionType
		{
			get
			{
				if (HasNoProcessWorkflowException)
				{
					return null;
				}

				if (exceptionType == null && !ExceptionTypeCode.IsEmpty)
				{
					exceptionType = GetProcessWorkflowExceptionType(ExceptionTypeCode);
				}

				return exceptionType;
			}
		}

		public ZString ExceptionTypeDescription => ExceptionType?.WET_DescriptionMultilingual ?? ZString.Empty;

		ZString exceptionTypeCategory = ZString.Empty;

		[ReadOnlyMember(nameof(HasProcessWorkflowException))]
		[List("Lookups.ExceptionTypeCategories")]
		public ZString ExceptionTypeCategory
		{
			get
			{
				var type = ExceptionType;

				if (type != null)
				{
					exceptionTypeCategory = type.WET_Category;
				}

				return exceptionTypeCategory;
			}
			set
			{
				exceptionTypeCategory = value;
			}
		}

		public virtual ZPropertyInfo ExceptionTypeCategoryInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ExceptionTypeCategory)); }
		}

		public ZString ExceptionTypeCategoryDescription => WorkflowDataRegistry.Instance.ExceptionCategories.Value.GetDescriptionFromCode(ExceptionTypeCategory) ?? ZString.Empty;

		[ReadOnlyMember(nameof(HasNoProcessWorkflowException))]
		[List("Lookups.ExceptionCauses")]
		public ZGuid ExceptionCausePK
		{
			get => ProcessWorkflowException?.WEX_WEC_Cause ?? ZGuid.Empty;
			set
			{
				var processWorkflowException = ProcessWorkflowException;

				if (processWorkflowException == null || processWorkflowException.WEX_WEC_Cause == value)
				{
					return;
				}

				processWorkflowException.WEX_WEC_Cause = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateExceptionCausePK();
				}
			}
		}

		public virtual ZPropertyInfo ExceptionCausePKInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ExceptionCausePK)); }
		}

		public ZString ExceptionCauseDescription => ProcessWorkflowException?.Cause?.WEC_Description ?? ZString.Empty;

		[ReadOnlyMember(nameof(HasNoProcessWorkflowException))]
		[List("Lookups.ExceptionResolutions")]
		public ZGuid ExceptionResolutionPK
		{
			get => ProcessWorkflowException?.WEX_WER_Resolution ?? ZGuid.Empty;
			set
			{
				var processWorkflowException = ProcessWorkflowException;

				if (processWorkflowException == null || processWorkflowException.WEX_WER_Resolution == value)
				{
					return;
				}

				processWorkflowException.WEX_WER_Resolution = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateExceptionResolutionPK();
				}
			}
		}

		public virtual ZPropertyInfo ExceptionResolutionPKInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ExceptionResolutionPK)); }
		}

		public ZString ExceptionResolutionDescription => ProcessWorkflowException?.Resolution?.WER_Description ?? ZString.Empty;

		[ReadOnlyMember(nameof(P9_ExceptionDurationHours_ReadOnly))]
		public override ZInt P9_ExceptionDurationHours { get => base.P9_ExceptionDurationHours; set => base.P9_ExceptionDurationHours = value; }

		public ZBool P9_ExceptionDurationHours_ReadOnly
		{
			get
			{
				return !ProcessTaskSecurityMan.IsAllowOverrideExceptionDuration(this);
			}
		}

		#endregion

		public ZDateTime ExceptionAssignedDate
		{
			get
			{
				if (!IsException || (P9_GS_NKAssignedStaffMember.IsEmpty && P9_GG_AssignedGroup.IsEmpty))
				{
					return ZDateTime.Empty;
				}

				return Logs.MostRecentLogByEventTime(Events.Assigned)?.SL_EventTime ?? ZDateTime.Empty;
			}
		}

		#endregion

		#region Defaulting Milestone Estimated Dates

		#region Estimated Defaulting from Predecessor

		public override ZInt P9_EstimatedDefaultFromPredecessor
		{
			get { return base.P9_EstimatedDefaultFromPredecessor; }
			set
			{
				base.P9_EstimatedDefaultFromPredecessor = value;
				if (!IsEstimateDefaulted && P9_EstimatedDefaultTimeDelta.IsValid)
				{
					P9_EstimatedDefaultTimeDelta = ZDateTime.Empty;
				}
				estimateDefaultedFromPredecessorPopulated = false;
				P9_EstimateDefaultedFromAsStringInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateP9_EstimatedDefaultTimeDelta();
				}
			}
		}

		ProcessTask EstimateDefaultedFromPredecessor
		{
			get
			{
				if (!estimateDefaultedFromPredecessorPopulated || (estimateDefaultedFromPredecessor != null && estimateDefaultedFromPredecessor.IsDeleted))
				{
					estimateDefaultedFromPredecessor = null;
					if (P9_EstimatedDefaultFromPredecessor > 0 && Parent != null && (IsTask || IsMilestone))
					{
						ZQuery query = new ZQuery();
						query.AddToFilter(ProcessTasksSchema.PK, SQLComparisonOperator.NotEqual, PK);
						query.AddToFilter(ProcessTasksSchema.P9_ParentID, P9_ParentID);
						query.AddToFilter(ProcessTasksSchema.P9_Sequence, P9_EstimatedDefaultFromPredecessor);
						query.AddToFilter(ProcessTasksSchema.P9_Type, IsMilestone ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, Core.Constants.Workflow.MilestoneType);
						estimateDefaultedFromPredecessor = Factory.LoadTop1<ProcessTask>(query);
					}
					estimateDefaultedFromPredecessorPopulated = true;
				}
				return estimateDefaultedFromPredecessor;
			}
		}
		bool estimateDefaultedFromPredecessorPopulated;
		ProcessTask estimateDefaultedFromPredecessor;

		public ZBool IsEstimateDefaultedFromPredecessorActualDate
		{
			get { return P9_EstimatedDefaultedFrom == P9_EstimatedDefaultedFrom_Actual; }
			set { P9_EstimatedDefaultedFrom = value ? P9_EstimatedDefaultedFrom_Actual : ""; }
		}

		public const string P9_EstimatedDefaultedFrom_Actual = "ACT";

		#endregion

		#region P9_EstimatedDefaultedFrom

		[List("WorkflowDescriptor.EstimateDefaultedFromList")]
		public override ZString P9_EstimatedDefaultedFrom
		{
			get { return base.P9_EstimatedDefaultedFrom; }
			set
			{
				base.P9_EstimatedDefaultedFrom = value;
				if (!IsEstimateDefaulted && P9_EstimatedDefaultTimeDelta.IsValid)
				{
					P9_EstimatedDefaultTimeDelta = ZDateTime.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateP9_EstimatedDefaultTimeDelta();
				}
			}
		}

		#endregion

		#region P9_EstimateDefaultedFromAsString

		[BusinessObjectTestExclude]
		[List("Lookups.EstimateDefaultedFromList")]
		[MaxLength(100)]
		public ZString P9_EstimateDefaultedFromAsString
		{
			get
			{
				ProcessTask estimateDefaultedFromPredecessor = this.EstimateDefaultedFromPredecessor;
				string result = "";
				if (estimateDefaultedFromPredecessor != null)
				{
					result = estimateDefaultedFromPredecessor.PredecessorsEstimateDefaultedFromString(IsEstimateDefaultedFromPredecessorActualDate);
				}
				else
				{
					result = P9_EstimatedDefaultedFrom;
				}
				return result;
			}
			set
			{
				int intValue = GetSequenceNumberFromSequenceAndCode(value);
				P9_EstimatedDefaultFromPredecessor = intValue;

				if (EstimateDefaultedFromPredecessor != null)
				{
					IsEstimateDefaultedFromPredecessorActualDate = value.ToLower().Contains((NoResString)"(act)");
				}
				else
				{
					P9_EstimatedDefaultFromPredecessor = 0;
					P9_EstimatedDefaultedFrom = value.Length == 3 ? value : ZString.Empty;
				}
			}
		}

		public ZString PredecessorsEstimateDefaultedFromString(bool fromActualDate)
		{
			if (fromActualDate)
			{
				return Res.GetString("15b71f3f-6ce9-4faa-a0a5-c7bee3587416", "{0} - {1} (act)", P9_Sequence.ToString(), P9_SE_NKMilestoneEvent);
			}
			else
			{
				return Res.GetString("3ef286d5-6e38-4e54-983c-05142564d10c", "{0} - {1} (est)", P9_Sequence.ToString(), P9_SE_NKMilestoneEvent);
			}
		}

		public ZPropertyInfo P9_EstimateDefaultedFromAsStringInfo => GetWrappedZPropertyInfo(nameof(P9_EstimateDefaultedFromAsString), _ => P9_EstimatedDefaultedFromInfo);

		int GetSequenceNumberFromSequenceAndCode(string value)
		{
			string number = "";
			for (int i = 0; i < value.Length; i++)
			{
				if (!char.IsNumber(value[i]))
				{
					break;
				}
				number += value[i];
			}
			int result = -1;
			return int.TryParse(number, out result) ? result : -1;
		}

		#endregion

		#region P9_Sequence

		public override ZInt P9_Sequence
		{
			get { return base.P9_Sequence; }
			set
			{
				if (base.P9_Sequence != value)
				{
					ZInt previousSequence = P9_Sequence;
					base.P9_Sequence = value;
					if (TaskOrMilestoneCollection != null)
					{
						foreach (ProcessTask milestone in TaskOrMilestoneCollection)
						{
							if (milestone.PK != PK &&
								milestone.P9_EstimatedDefaultFromPredecessor > 0 &&
								milestone.P9_EstimatedDefaultFromPredecessor == previousSequence)
							{
								milestone.P9_EstimatedDefaultFromPredecessor = value;
							}
						}
					}

					if (previousSequence != value)
					{
						UpdateIterationPivots();
					}
				}
			}
		}

		WorkflowItemCollectionView TaskOrMilestoneCollection
		{
			get
			{
				WorkflowItemCollectionView result = null;
				if (Parent != null)
				{
					if (IsTask)
					{
						result = Parent.WorkflowItems.Tasks;
					}
					else if (IsMilestone)
					{
						result = Parent.WorkflowItems.Milestones;
					}
				}
				return result;
			}
		}

		public ZInt NextSequenceNumber
		{
			get
			{
				ZInt nextSequenceNumber = 0;
				var parent = Parent;
				if (parent != null)
				{
					var filter = new ZQuery(ProcessTasksSchema.P9_ParentID, P9_ParentID);

					var parentBizo = parent as BusinessObject;
					if (parentBizo != null && !parentBizo.IsInDatabase)
					{
						filter.FetchOnlyFromLocalCache = true;
					}

					nextSequenceNumber = Factory.Load<ProcessTask>(filter).Max(item => item.P9_Sequence) + 1;
				}
				return nextSequenceNumber;
			}
		}

		#endregion

		#region P9_EstimatedDefaultTimeDelta

		[ZDateTimeOffsetValueNegatable]
		public override ZDateTime P9_EstimatedDefaultTimeDelta
		{
			get => base.P9_EstimatedDefaultTimeDelta;
			set => base.P9_EstimatedDefaultTimeDelta = value.ConvertToDurationBasedDate(P9_EstimatedDefaultTimeDeltaInfo);
		}

		protected bool P9_EstimatedDefaultTimeDelta_ReadOnly
		{
			get { return P9_EstimateDefaultedFromAsString.IsEmpty; }
		}

		#endregion

		#region P9_EstimatedTimeToComplete

		[ZDateTimeDurationValue]
		public override ZDateTime P9_EstimatedTimeToComplete
		{
			get => base.P9_EstimatedTimeToComplete;
			set => base.P9_EstimatedTimeToComplete = value.ConvertToDurationBasedDate(P9_EstimatedTimeToCompleteInfo);
		}

		#endregion

		#region P9_NonWorkHours

		[ZDateTimeDurationValue]
		public override ZDateTime P9_NonWorkHours
		{
			get => base.P9_NonWorkHours;
			set => base.P9_NonWorkHours = value.ConvertToDurationBasedDate(P9_NonWorkHoursInfo);
		}

		#endregion

		#region P9_TotalSuspendedDuration

		[ZDateTimeDurationValue]
		public override ZDateTime P9_TotalSuspendedDuration
		{
			get => base.P9_TotalSuspendedDuration;
			set => base.P9_TotalSuspendedDuration = value.ConvertToDurationBasedDate(P9_TotalSuspendedDurationInfo);
		}

		#endregion

		#region Defaulted From Date

		ZDateTimeOffset IDefaultedFromDateProvider.PredecessorDefaultDate
		{
			get
			{
				return EstimateDefaultedFromPredecessor != null
						? IsEstimateDefaultedFromPredecessorActualDate ? EstimateDefaultedFromPredecessor.P9_ActualDateOffset : EstimateDefaultedFromPredecessor.P9_ScheduledDateOffset
						: ZDateTimeOffset.Empty;
			}
		}

		IWorkflowProvider IDefaultedFromDateProvider.Parent => Parent;

		internal ZDateTimeOffset EstimateDefaultedFromDate
		{
			get
			{
				if (WorkflowDescriptor != null)
				{
					return WorkflowDescriptor.GetScheduleDateTimeAndLocationForTimezoneFromParent(this, P9_EstimatedDefaultedFrom, new Lazy<RefUNLOCO>(() => Company?.OrgProxy?.ClosestPort));
				}
				return ZDateTimeOffset.Empty;
			}
		}

		#endregion

		ZBool IsEstimateDefaulted
		{
			get { return !P9_EstimatedDefaultedFrom.IsEmpty || P9_EstimatedDefaultFromPredecessor > 0; }
		}

		#endregion

		#region Workflow Triggers

		public virtual bool MatchesTriggerAction(ProcessTask relatedTrigger)
		{
			return MatchesParentForMessageTriggerAction(relatedTrigger) && MatchesTriggerNotificationsActions(relatedTrigger);
		}

		protected virtual bool MatchesTriggerNotificationsActions(ProcessTask relatedTrigger)
		{
			return
				ProcessTaskNotifications.Count == relatedTrigger.ProcessTaskNotifications.Count &&
					ProcessTaskNotifications.All(localNotification =>
						relatedTrigger.ProcessTaskNotifications.Any(relatedNotification =>
							localNotification.PQ_TriggerType == relatedNotification.PQ_TriggerType &&
							localNotification.PQ_Calc_TriggerParty == relatedNotification.PQ_Calc_TriggerParty &&
							localNotification.PQ_TriggerParty == relatedNotification.PQ_TriggerParty &&
							localNotification.PQ_OH_Recipient == relatedNotification.PQ_OH_Recipient &&
							localNotification.PQ_SU_Document == relatedNotification.PQ_SU_Document));
		}

		protected virtual bool MatchesParentForMessageTriggerAction(ProcessTask relatedTrigger)
		{
			return P9_ParentID == relatedTrigger.P9_ParentID;
		}

		#endregion

		#region Calendar Reminders

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (ShouldCreateTaskReminder)
				{
					TaskReminder.CreateAppointment();
				}
			}
		}

		public bool ShouldCreateTaskReminder
		{
			get
			{
				return P9_IsCalendarItem && !P9_ScheduledDate.IsEmpty
					&& ((AssignedStaffMember != null && !AssignedStaffMember.GS_EmailAddress.IsEmpty)
						|| (AssignedGroup != null && AssignedGroup.Staff.Cast<GlbStaff>().Any(staff => !staff.GS_EmailAddress.IsEmpty)))
					&& (P9_ScheduledDate != OriginalP9_ScheduledDate || P9_IsCalendarItem != OriginalP9_IsCalendarItem);
			}
		}

		public Reminder TaskReminder
		{
			get
			{
				Reminder reminder = null;

				ZDateTime toDate = P9_EstDuration.IsEmpty ? P9_ScheduledDate : P9_ScheduledDate.Add(TimeSpan.FromHours((double)LowEstimatedDurationHours));
				string bodyTemplate = ReminderBodyTemplate;
				reminder = new Reminder(PK.ToString(), PK, new ZString(TableName), DateTimeKind.Local, P9_ScheduledDate, toDate, ReminderSubject,
					GetPlainTextReminderBody(bodyTemplate),
					GetHtmlReminderBody(bodyTemplate),
					ReminderTimeZone);

				reminder.Location = Address != null ? Address.AddressAsASingleLine : ZString.Empty;

				if (AssignedStaffMember != null && !AssignedStaffMember.GS_EmailAddress.IsEmpty)
				{
					reminder.Recipients.Add(AssignedStaffMember.GS_FullName, AssignedStaffMember.GS_EmailAddress);
				}
				else if (AssignedGroup != null)
				{
					foreach (GlbStaff staff in AssignedGroup.Staff)
					{
						if (!staff.GS_EmailAddress.IsEmpty)
						{
							reminder.Recipients.Add(staff.GS_FullName, staff.GS_EmailAddress);
						}
					}
				}

				return reminder;
			}
		}

		protected virtual string ReminderSubject
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				builder.Append(ParentJobDetails);

				if (Organisation != null)
				{
					if (builder.Length > 0)
					{
						builder.Append(" - ");
					}
					builder.Append(Organisation.OH_FullNameTruncated);
				}

				if (Contact != null)
				{
					builder.Append(" - ");
					builder.Append(Contact.OC_ContactName);
					builder.Append(", ");
					ZString contactPhone = "";
					if (!Contact.OC_Phone.IsEmpty)
					{
						contactPhone = Contact.OC_Phone;
					}
					else if (Organisation != null)
					{
						contactPhone = Organisation.MainAddress.OA_Phone;
					}
					builder.Append(contactPhone);
				}
				else if (Organisation != null)
				{
					builder.Append(" - ");
					builder.Append(Organisation.MainAddress.OA_Phone);
				}

				AppendReminderSubjectTaskID(builder);

				return builder.ToString();
			}
		}

		protected void AppendReminderSubjectTaskID(ZStringBuilder builder)
		{
			builder.Append(" " + Res.GetString("2bdc00e5-51c5-4ef5-878e-f75ac74d2022", "(Task {0})", P9_TaskID));
		}

		protected virtual string BodyTaskExtraDetails
		{
			get { return ""; }
		}

		protected virtual string GetPlainTextReminderBody(string template)
		{
			return ReplaceReminderMacros(template, false);
		}

		protected virtual string GetHtmlReminderBody(string template)
		{
			string result = WebUtility.HtmlEncode(template);
			result = ReplaceReminderMacros(result, true);
			return string.Format("<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{0}</BODY></HTML>", result);
		}

		protected virtual string ReplaceReminderMacros(string template, bool forHtml)
		{
			return template.Replace(TaskIDMacroTemplate, forHtml
				? string.Format(@"<a href=""{0}"">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ProcessTasks, PK.ToGuid()), P9_TaskID)
				: (string)P9_TaskID);
		}

		public const string TaskIDMacroTemplate = "{!TASKID!}"; // macro name not to be translated

		/// <summary>
		/// Reminder body template suitable for plain text or HTML.
		/// Contains macros that will be replaced with object IDs (plain text) or URLs (HTML).
		/// Macro substitution occurs after HTML encoding so the URLs do not get encoded.
		/// </summary>
		protected virtual string ReminderBodyTemplate
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				builder.Append(ParentJobDetails);

				if (Organisation != null)
				{
					if (builder.Length > 0)
					{
						builder.Append(System.Environment.NewLine);
					}
					builder.Append(Res.GetString("8b62f1b4-91cd-48b4-b7f1-ebcfe8b59a6e", "Organization: {0}", Organisation.OH_FullNameTruncated));
					builder.Append(System.Environment.NewLine);
				}

				if (Contact != null)
				{
					builder.Append(Res.GetString("32da030a-4c42-4dc9-854e-9c76d58c9bfa", "Contact: {0}", Contact.OC_ContactName));
					builder.Append(System.Environment.NewLine);

					ZString contactPhone = "";
					if (!Contact.OC_Phone.IsEmpty)
					{
						contactPhone = Res.GetString("c2e573fe-7b39-4c88-8797-b4454a92222b", "Phone: {0}", Contact.OC_Phone);
					}
					else if (Organisation != null)
					{
						contactPhone = Res.GetString("81c00e29-cf60-4619-a46a-69eab00541de", "Company Phone: {0}", Organisation.MainAddress.OA_Phone);
					}
					builder.Append(contactPhone);
				}
				else if (Organisation != null)
				{
					builder.Append(Res.GetString("6b413b4b-078a-413d-b832-82098e39f06a", "Phone: {0}", Organisation.MainAddress.OA_Phone));
				}
				builder.Append(System.Environment.NewLine);

				if (Address != null)
				{
					builder.Append(Res.GetString("70ec5708-c7b1-42ba-adaf-6a8be82e7de2", "Task Address: {0}", Address.AddressAsASingleLine));
					builder.Append(System.Environment.NewLine);
				}

				builder.Append(System.Environment.NewLine);

				builder.Append(BodyTaskExtraDetails);

				AppendTaskDetails(builder);

				return builder.ToString();
			}
		}

		internal void AppendTaskDetails(ZStringBuilder builder)
		{
			builder.AppendLine(Res.GetString("03ce7507-eb6c-480c-a5b3-aa481cc5b4c3", "Task Type: {0}", Lookups.Types.GetDescriptionFromCode(P9_Type)));
			builder.AppendLine(Res.GetString("0ffa7661-2fde-44ee-95c6-bbb779ab7efa", "Task Description: {0}", P9_Description));
			builder.AppendLine(Res.GetString("509d823a-fbe4-4e2c-a50a-341968e33a63", "Task ID: {0}", TaskIDMacroTemplate));
			builder.AppendLine(Res.GetString("dd2dfb14-1309-46bd-8fcf-6e94ed4cf221", "Task Notes: {0}", ORtfTextUtil.RtfToText(P9_Notes)));
		}

		protected virtual ITimeZone ReminderTimeZone
		{
			get
			{
				RefUNLOCO loco = null;

				if (Address != null && Address.RelatedPortCode != null)
				{
					loco = Address.RelatedPortCode;
				}
				else if (Organisation != null)
				{
					loco = Organisation.ClosestPort;
				}

				if (loco != null && loco.TimeZoneSet != null)
				{
					return loco.TimeZoneSet.GetCalculationTimeZone();
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ProcessTask) { UseBusinessEntityFactoryAsInternal = true };
				}
				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IProcessTask Members

		ITemplateConditional IProcessTask.TemplateConditions => TemplateConditions;

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[]
			{
				ProcessTask.Schema.P9_Status,
				ProcessTasksSchema.P9_ActualDate.Name,
				ProcessTasksSchema.P9_ActualDateUtc.Name,
				ProcessTask.Schema.P9_ActualDuration,
				ProcessTask.Schema.P9_CompletedTimeUtc,
				ProcessTask.Schema.P9_EstimatedTimeToComplete,
				ProcessTask.Schema.P9_EstimatedHandoverTimeUtc,
				ProcessTask.Schema.P9_MilestoneExceptionAdded,
				ProcessTask.Schema.P9_NonWorkHours,
				ProcessTasksSchema.P9_ParentTemplateID.Name,
				ProcessTask.Schema.P9_RN_NKOriginCountry,
				ProcessTask.Schema.P9_RN_NKDestinationCountry,
				ProcessTask.Schema.P9_SE_NKExceptionEvent,
				ProcessTasksSchema.P9_SuspendedAt.Name,
				ProcessTasksSchema.P9_SuspendedAtUtc.Name,
				ProcessTask.Schema.P9_TaskID,
				ProcessTask.Schema.P9_TotalSuspendedDuration,
			});

			var newProcessTask = (ProcessTask)base.CloneInternal(args);

			newProcessTask.HasChanges = true;
			newProcessTask.P9_Status = (P9_Status == ProcessTaskStatusCodeList.Codes.Open) ? ProcessTaskStatusCodeList.Codes.Open : ProcessTaskStatusCodeList.Codes.Assigned;
			newProcessTask.P9_IsCalendarItem = this.P9_IsCalendarItem;

			if (!IsTask)
			{
				newProcessTask.TriggerConditions.TriggerEventCode = ZString.Empty;
			}

			newProcessTask.RefreshWorkflowType();

			CloneIterationPivots(newProcessTask);

			return newProcessTask;
		}

		void CloneIterationPivots(ProcessTask newProcessTask)
		{
			if (!IsUpdatingPivotsSuspended)
			{
				var existingPivot = IterationPivot;

				if (existingPivot != null)
				{
					var newPivot = (IProcessTaskIterationLinkPivot)((BusinessObject)existingPivot).Clone();
					newPivot.P9P_P9_Task = newProcessTask.PK;
				}
			}
		}

		#endregion

		#region Status

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this should be constant")]
		public const string Overdue = "Overdue";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this should be constant")]
		public const string CompletedLate = "Completed Late";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this should be constant")]
		public const string Completed = "Completed";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this should be constant")]
		public const string Pending = "Pending";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard code for SL_Reference")]
		const string toWorkReferenceString = "to [WRK]";

		[ResourceStringData("94d04697-d37f-408a-9628-6b3792ac8e41", Caption = "Status")]
		public ZString Status
		{
			get { return GetStatusText(); }
		}

		public virtual ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(Schema.Status); }
		}

		protected virtual ZString GetStatusText()
		{
			return GetStatus(P9_ActualDate, P9_ScheduledDate);
		}

		public static ZString GetStatus(ZDateTime actualDate, ZDateTime scheduledDate)
		{
			return GetStatus(new ZDateTimeOffset(actualDate), new ZDateTimeOffset(scheduledDate));
		}

		public static ZString GetStatus(ZDateTimeOffset actualDate, ZDateTimeOffset scheduledDate)
		{
			ZString result = ZString.Empty;

			if (!actualDate.IsValid && (scheduledDate.Date < ZDateTime.Today))
			{
				result = Overdue;
			}
			else if (scheduledDate.IsValid && (actualDate.Date > scheduledDate.Date))
			{
				result = CompletedLate;
			}
			else if ((scheduledDate.IsValid && actualDate.Date <= scheduledDate.Date)
					 || (!scheduledDate.IsValid && actualDate.IsValid))
			{
				result = Completed;
			}
			else if (scheduledDate.IsValid && !actualDate.IsValid)
			{
				result = Pending;
			}

			return result;
		}

		#endregion

		#region ICustomTextTemplateContext Members

		public BusinessObject[] GetTextTemplateContextBusinessObject(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return Parent != null && Parent is BusinessObject ?
				new BusinessObject[] { this, ParentBusinessObject } :
				new BusinessObject[] { this };
		}

		public string GetTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return "ProcessTask." + bindingMemberInfo.BindingField + "." + this.P9_Type;
		}

		#endregion

		#region IRootTypeProvider Members

		Type[] IRootTypeProvider.RootTypes
		{
			get => this.GetRootTypes().Distinct().ToArray();
		}

		BusinessObject[] IRootTypeProvider.Roots
		{
			get => ((IDynamicRootProvider)this).AugmentedRoots((BusinessObject)Parent);
		}

		BusinessObject[] IDynamicRootProvider.AugmentedRoots(BusinessObject parent)
		{
			if (IsTemplate)
			{
				return Array.Empty<BusinessObject>();
			}
			else
			{
				return this.GetRoots(parent);
			}
		}

		#endregion

		#region IsCurrent

		[ResourceStringData("ProcessTask.IsCurrent", Caption = "Is Current")]
		public bool IsCurrent
		{
			get { return GetCurrentTaskStatuses().Any(s => s == P9_Status); }
		}

		public static IEnumerable<string> GetCurrentTaskStatuses()
		{
			return new[]
			{
				ProcessTaskStatusCodeList.Codes.Working,
				ProcessTaskStatusCodeList.Codes.Assigned,
				ProcessTaskStatusCodeList.Codes.Suspended,
			};
		}

		public static IEnumerable<string> GetOpenTaskStatuses()
		{
			yield return ProcessTaskStatusCodeList.Codes.Open;
			yield return ProcessTaskStatusCodeList.Codes.Assigned;
			yield return ProcessTaskStatusCodeList.Codes.Working;
			yield return ProcessTaskStatusCodeList.Codes.Suspended;
		}

		public static IEnumerable<string> GetNonTaskTypes()
		{
			yield return Constants.Workflow.MilestoneType;
			yield return Constants.Workflow.WorkflowTriggerType;
			yield return Constants.Workflow.ExceptionType;
		}

		public static ZQuery GetNonTasksExclusionQuery()
		{
			return new ZQuery(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, GetNonTaskTypes());
		}

		#endregion

		#region CompletionTask

		#region DisplayOrder

		[ResourceStringData("ProcessTask.DisplayOrder", Caption = "Order")]
		public ZInt DisplayOrder
		{
			get { return Math.Max(0, P9_Sequence - CompletionTaskSequenceStart); }
			set
			{
				P9_Sequence = value + CompletionTaskSequenceStart;
				DisplayOrderInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DisplayOrderInfo
		{
			get { return GetZPropertyInfo(nameof(DisplayOrder)); }
		}

		public const int CompletionTaskSequenceStart = 1000;

		#endregion

		#region IsEffectAchieved

		[ResourceStringData("ProcessTask.IsEffectAchieved", Caption = "Effect Achieved")]
		[VisualBoardSearchable]
		public ZBool IsEffectAchieved
		{
			get { return P9_Status == ProcessTaskStatusCodeList.Codes.Closed; }
			set
			{
				if (value)
				{
					TaskStatusBeforeEffectAchieved = P9_Status;
					P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				}
				else
				{
					P9_Status = TaskStatusBeforeEffectAchieved.IsEmpty || TaskStatusBeforeEffectAchieved == ProcessTaskStatusCodeList.Codes.Closed
						? (ZString)ProcessTaskStatusCodeList.Codes.Assigned
						: TaskStatusBeforeEffectAchieved;
				}

				IsEffectAchievedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsEffectAchievedInfo
		{
			get { return GetZPropertyInfo(nameof(IsEffectAchieved)); }
		}

		ZString TaskStatusBeforeEffectAchieved { get; set; }

		#endregion

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get
			{
				if (IsDeleted)
				{
					return null;
				}

				ProcessHandlingInfo lineTriggerProcessHandlingInfo = null;

				if (IsTask)
				{
					lineTriggerProcessHandlingInfo = (WorkflowDescriptor?.SupportsTaskLineTriggers ?? false) ? new TaskLineTriggerProcessHandlingInfo(this) : null;
				}

				if (IsException)
				{
					lineTriggerProcessHandlingInfo = (WorkflowDescriptor?.SupportsExceptionLineTriggers ?? false) ? new ExceptionLineTriggerProcessHandlingInfo(this) : null;
				}

				return lineTriggerProcessHandlingInfo;
			}
		}

		#endregion

		#region ITagable Members

		ITagLinkCollection ITagBindable.TagLinks_ForBinding
		{
			get
			{
				if (tagLinks == null)
				{
					tagLinks = ObjectFactory.Get<ITagLinkCollection>("ITagLinkCollection", this);
					RegisterEditableChildObject(tagLinks);
				}

				return tagLinks;
			}
		}
		ITagLinkCollection tagLinks;

		ICollection<ITagLink> ITagable.TagLinks => Factory.Load<ITagLink>(this.CreateTagLinksQuery());

		ITagable ITagable.Parent => ProcessHeader;

		string ITagable.Description => P9_Description;

		#region Tags For UC

		/// <summary>
		/// This property has been created to intercept the signature [CollectionRelationProperty("Tags", "TGL_ParentId", "P9Tags")] into Odyssey.Interfaces.cs.
		/// As we never want Tags with the UsageScope RUL to be copied, we are always using this property to copy.
		/// </summary>
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public IEnumerable<ITagLink> Tags
		{
			get
			{
				if (tagLinks_ForUC == null)
				{
					tagLinks_ForUC = ObjectFactory.Get<ITagLinkCollection>("ITagLinkCollection", this) as IEnumerable<ITagLink>;
				}
				return tagLinks_ForUC.Where(o => o.TagMagnitude.TagDefinition.TGD_UsageScope != "RUL");
			}
		}
		IEnumerable<ITagLink> tagLinks_ForUC;

		#endregion

		#region Nudge

		ITagLink[] TagLinks_ForNudge => (this as ITagBindable).TagLinks_ForBinding.Cast<ITagLink>().ToArray();

		ZDecimal NudgeFromTags => TagLinks_ForNudge.Sum(t => t.EffectiveNudge);

		public ZString EffectiveTaskNudge => HasNudge() ? GetEffectiveTaskNudgeValue().ToString() : string.Empty;

		internal ZDecimal GetEffectiveTaskNudgeValue()
		{
			return HasNudge() ? ((ProcessHeader?.EffectiveNudge ?? ZDecimal.Zero) + NudgeFromTags) : 0m;
		}

		bool HasNudge()
		{
			return ProcessHeader != null || TagLinks_ForNudge.Any();
		}

		public ZPropertyInfo EffectiveTaskNudgeInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveTaskNudge); }
		}

		#endregion

		#endregion

		#region IWorkflowItem Members

		ZGuid IWorkflowItem.ParentID => P9_ParentID;

		ZString IWorkflowItem.ParentTableCode => P9_ParentTableCode;

		ZGuid IWorkflowItem.CompanyPK => P9_GC;

		ZString IWorkflowItem.Description
		{
			get { return P9_Description; }
			set { P9_Description = value; }
		}

		ZInt IWorkflowItem.Sequence => P9_Sequence;

		ZString IWorkflowItem.WorkflowItemType => P9_Type;

		ZString IWorkflowTypeProvider.WorkflowProcessType => WorkflowType;

		#endregion

		#region ITemplateConditional Members

		ZString ITemplateConditional.TemplateCondition1
		{
			get { return P9_Condition1; }
			set
			{
				using (TemporarilyAllowSettingCondition(nameof(P9_Condition1)))
				{
					P9_Condition1 = value;
				}
			}
		}

		ZString ITemplateConditional.TemplateCondition2
		{
			get { return P9_Condition2; }
			set
			{
				using (TemporarilyAllowSettingCondition(nameof(P9_Condition2)))
				{
					P9_Condition2 = value;
				}
			}
		}

		ZString ITemplateConditional.TemplateCondition2Value
		{
			get
			{
				return P9_Condition2Value;
			}
			set
			{
				using (TemporarilyAllowSettingCondition(nameof(P9_Condition2Value)))
				{
					P9_Condition2Value = value;
				}
			}
		}

		ZString ITemplateConditional.OriginCountryCode
		{
			get { return P9_RN_NKOriginCountry; }
			set { P9_RN_NKOriginCountry = value; }
		}

		ZString ITemplateConditional.DestinationCountryCode
		{
			get { return P9_RN_NKDestinationCountry; }
			set { P9_RN_NKDestinationCountry = value; }
		}

#if DEBUG
		public
#endif
		IDisposable TemporarilyAllowSettingCondition(string conditionPropertyName)
		{
			var previousConditionPropertyName = allowedConditionPropertyName;

			allowedConditionPropertyName = conditionPropertyName;

			return new DisposableAction(() => allowedConditionPropertyName = previousConditionPropertyName);
		}

		void EnsureCanSetTemplateCondition(string conditionPropertyName)
		{
			if (allowedConditionPropertyName != conditionPropertyName && !IsCopying)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Set template condition property {0} without calling through {1} interface. Please use the ProcessTask.{2} property to set conditions.",
					conditionPropertyName,
					nameof(ITemplateConditional),
					nameof(TemplateConditions)));
			}
		}

		string allowedConditionPropertyName;

		#endregion

		#region IBaseTrigger Members

		IActiveBusinessObjectCollection IBaseTrigger.TriggerActions => ProcessTaskNotifications;

		ITriggerConditions IBaseTrigger.TriggerConditions_ForBinding => TriggerConditions;

		ZDateTime IBaseTrigger.ActualDate => P9_ActualDate;

		ZBool IBaseTrigger.SuppressDuplicates => P9_SuppressDuplicates;

		ZInt IBaseTrigger.DelayDurationSeconds => P9_DelayDurationSeconds;

		bool IBaseTrigger.AreTriggerConditionsMet(IStmALog @event, IBusiness parent) => TriggerConditionEvaluator.AreTriggerConditionsMet(this, @event, (BusinessObject)parent);

		void IBaseTrigger.SetEventTime(IStmALog @event, IBusiness job, ZDateTimeOffset eventTime) => TrySetActualDateForEvent(@event, (BusinessObject)job, eventTime);

		void IBaseTrigger.SetEventTimeWithoutFiringWorkflow(ZDateTimeOffset eventTime)
		{
			base.P9_ActualDateOffset = eventTime;
		}

		void IBaseTrigger.SetEstimateTime(IStmALog @event, ZDateTimeOffset estimateTime)
		{
			using (SuspendUpdateEventLog())
			{
				P9_ScheduledDateOffset = estimateTime;
			}
		}

		void IBaseTrigger.Fire(IBusiness workflowParent, IStmALog @event)
		{
			if (P9_TriggerFiredCountdown > 0)
			{
				this.AddWorkflowTriggerEventLog((BusinessObject)workflowParent, new EventSource(@event), @event, null);
			}
		}

		void IBaseTrigger.Withdraw(IBusiness workflowParent, IStmALog @event) => this.DeleteWorkflowTriggerEventLog(new EventSource(@event));

#if DEBUG
		void IBaseTrigger.SetShouldTriggerOnEstimateEvents_ForTests(bool value)
		{
			ShouldTriggerOnEstimateEvents = value;
		}
#endif

		#endregion

		#region IWorkflowTrigger Members

		public ZDateTime LastFiredTimeUtc => LastFiredTime.ToUtcZDateTime();

		public ZDateTime LastFiredTimeLocal => LastFiredTime.ToLocalZDateTime();

		public ZDateTimeOffset LastFiredTime
		{
			get => P9_ActualDateOffset;
		}

		ZGuid IWorkflowTrigger.ParentTemplateID => P9_ParentTemplateID;

		void EnsureCanSetTriggerCondition(string conditionPropertyName)
		{
			if (allowedConditionPropertyName != conditionPropertyName && !IsCopying)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Set trigger condition property {0} without calling through {1} interface. Please use the ProcessTask.{2} property to set conditions.",
					conditionPropertyName,
					nameof(ITriggerConditions),
					nameof(TriggerConditions)));
			}
		}

		NotificationCollection triggerApplicationErrors = new NotificationCollection();

		void IWorkflowTrigger.UpdateTriggerActionNotifications(NotificationCollection notifications)
		{
			foreach (var error in triggerApplicationErrors.GetErrors())
			{
				RemoveRowWarning(error.Message);
			}

			triggerApplicationErrors = notifications;

			foreach (var error in triggerApplicationErrors.GetErrors())
			{
				AddRowWarning(error.Message);
			}
		}

		#endregion

		#region ITriggerConditions Members

		ZString ITriggerConditions.TriggerEventCode
		{
			get { return P9_SE_NKMilestoneEvent; }
			set
			{
				using (TemporarilyAllowSettingCondition(nameof(P9_SE_NKMilestoneEvent)))
				{
					P9_SE_NKMilestoneEvent = value;
				}
			}
		}

		ZString ITriggerConditions.TriggerFieldName
		{
			get { return P9_TriggerField; }
			set
			{
				using (TemporarilyAllowSettingCondition(nameof(P9_TriggerField)))
				{
					P9_TriggerField = value;
				}
			}
		}

		ZString IEventReferenceConditions.TriggerCondition
		{
			get { return P9_TriggerCondition; }
			set
			{
				using (TemporarilyAllowSettingCondition(nameof(P9_TriggerCondition)))
				{
					P9_TriggerCondition = value;
				}
			}
		}

		ZString IEventReferenceConditions.TriggerConditionValue
		{
			get { return P9_TriggerConditionValue; }
			set
			{
				using (TemporarilyAllowSettingCondition(nameof(P9_TriggerConditionValue)))
				{
					P9_TriggerConditionValue = value;
				}
			}
		}

		ZShort ITriggerConditions.TriggerFiredCountdown
		{
			get
			{
				if (HasProcessJobTriggerLink)
				{
					return processJobTriggerLink.TriggerFiredCountdown;
				}
				else if (!HasProcessJobTriggerLink && TemplateTrigger != null)
				{
					return TemplateTrigger.TriggerFiredCountdown;
				}
				else
				{
					return P9_TriggerFiredCountdown;
				}
			}
			set
			{
				using (TemporarilyAllowSettingCondition(nameof(P9_TriggerFiredCountdown)))
				{
					if (HasProcessJobTriggerLink)
					{
						processJobTriggerLink.TriggerFiredCountdown = value;
					}
					else
					{
						P9_TriggerFiredCountdown = value;
					}
				}
			}
		}

		ZBool ITriggerConditions.Cascading => P9_RespondToCascadedEvents;

		ZString ITriggerConditions.CascadingContext => P9_CascadedEventsContext;

		#endregion

		#region ITemplateTrigger Members

		ZBool ITemplateTrigger.IsActive
		{
			get { return true; }
			set { }
		}

		IWorkflowTrigger ITemplateTrigger.GetOrCreateJobVersionOfTrigger(IBusiness job, bool createIfNotFound) => this;
		ZQuery ITemplateTrigger.GetJobVersionOfTriggerQuery(IBusiness job) => throw new InvalidOperationException();
		ITemplateTrigger ITemplateTrigger.Clone()
		{
			if (this.IsTemplateTask && this.IsWorkflowTrigger)
			{
				return TemplateProcessTaskCopier.Clone(this, this.GetType());
			}
			else
			{
				throw new InvalidOperationException("Process task is not a template trigger");
			}
		}

		#endregion

		#region IAssignedWorkflowItem Members

		ZString IAssignedWorkflowItem.AssignedStaffCode => P9_GS_NKAssignedStaffMember;

		ZGuid IAssignedWorkflowItem.AssignedGroupPK => P9_GG_AssignedGroup;

		#endregion

		#region IExternalReferencingTrigger Members

		ZGuid IExternalReferencingTrigger.ReferencedID => P9_ReferencedID;

		#endregion

		#region IAntrlMacroContextProvider
		IAntlrMacroContext IAntlrMacroContextProvider.GetSampleContext()
		{
			Type parentType;
			BusinessObject parent = null;

			if (!P9_LineTriggerType.IsEmpty && WorkflowDescriptors.Instance.TryGetValue(P9_LineTriggerType, out var lineDescriptor))
			{
				parentType = lineDescriptor.WorkflowProviderType;
			}
			else if (Parent is ProcessTaskTemplate template)
			{
				parentType = template.WorkflowDescriptor.WorkflowProviderType;
			}
			else
			{
				parent = this.GetJob();
				parentType = parent.GetType();
			}

			return ObjectFactory.Get<IWorkflowMacroContextDecider>().GetContextForTriggerConditionsForUserInterface(parentType: parentType, parent: parent, trigger: this, logParentType: parentType, logParent: parent, MacroErrorMessageExtender); 
		}

		string MacroErrorMessageExtender(string errorMessage)
		{
			if (errorMessage.Contains(Res.GetString("2837D9CA-1F99-407C-A0F5-6EBA7386101D", "does not contain property")))
			{
				errorMessage += "\r\n" + TriggerConditionGenericErrorMessage;
			}
			return errorMessage;
		}

		static readonly MultilingualString TriggerConditionGenericErrorMessage = ResString.GetMultilingualString("59113E04-350C-497B-8B87-B7ACB4BF97FF", @"The MCR data field map has been enhanced with additional data models and variables. For more details, refer to the MCR Macro Update Notes. Existing MCR macro conditions will continue to evaluate as before.");

		#endregion

		#region Required Job Skills

		[ChildEditable]
		public ProcessTaskRequiredSkillCollection SkillsPivots
		{
			get
			{
				if (skillPivots == null)
				{
					skillPivots = new ProcessTaskRequiredSkillCollection(this);
					skillPivots.Load();
					RegisterEditableChildObject(skillPivots);
				}
				return skillPivots;
			}
		}
		ProcessTaskRequiredSkillCollection skillPivots;

		internal void AddSkillsPivotsFetchHint()
		{
			if (IsTask)
			{
				Factory.AddFetchHint(ProcessTaskRequiredSkillSchema.P9S_P9, PK);
			}
		}

		[ChildEditable]
		[List("Lookups.SkillList")]

		#endregion

		#region ILineTriggerSupport Members

		ZString ILineTriggerSupport.LineTriggerType
		{
			get => P9_LineTriggerType;
			set => P9_LineTriggerType = value;
		}

		#endregion

		#region INumberFountainConsumer

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.ProcessTaskID;

		ZString INumberFountainEntityWithID.ID
		{
			get => P9_TaskID;
			set => P9_TaskID = value;
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber => P9_TaskID;

		#endregion

		#region IRegisterStatusChangeMode Members

		IDisposable IRegisterStatusChangeMode.TemporarilySetStatusChangeModeToChangedByOperationalAction()
		{
			return SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.OperationalAction);
		}

		IDisposable IRegisterStatusChangeMode.TemporarilySetStatusChangeModeToChangedByTriggerOrMilestone()
		{
			return SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.TriggerOrOtherAutomation);
		}

		#endregion

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) == TestBusinessObjectKind.PopulateDependentCollections)
			{
				// Since we are going to populate ProcessTaskNotification, we need to ensure this is a trigger so that collection allows us to add elements.
				P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
			if (IsException)
			{
				IsExceptionActioned = false;
			}
			else
			{
				P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			}
			this.HasChanges = false;
		}

#endif
		#endregion

		#region DateTimeOffset Hackery

		public void SetScheduledDate(ZDateTimeOffset date)
		{
			P9_ScheduledDate = date.ToZDateTime();
		}

		public ZDateTimeOffset P9_CompletedTime
		{
			get => new ZDateTimeOffset(P9_CompletedTimeUtc);
			set => P9_CompletedTimeUtc = value.ToUtcZDateTime();
		}

		public ZPropertyInfo P9_CompletedTimeInfo => GetWrappedZPropertyInfo(nameof(P9_CompletedTime), _ => P9_CompletedTimeUtcInfo);

		public ZDateTimeOffset P9_EstimatedHandoverTime
		{
			get => new ZDateTimeOffset(P9_EstimatedHandoverTimeUtc);
			set => P9_EstimatedHandoverTimeUtc = value.ToUtcZDateTime();
		}

		public ZPropertyInfo P9_EstimatedHandoverTimeInfo => GetWrappedZPropertyInfo(nameof(P9_EstimatedHandoverTime), _ => P9_EstimatedHandoverTimeUtcInfo);

		ZDateTimeOffset IProcessTask.P9_ActualDate => P9_ActualDateOffset;

		ZDateTimeOffset IProcessTask.P9_ScheduledDate
		{
			get => P9_ScheduledDateOffset;
			set => P9_ScheduledDateOffset = value;
		}

		#endregion

		#region ProcessTaskLogger

		internal ProcessTaskLogger ProcessTaskLogger
		{
			get
			{
				if (processTaskLogger == null)
				{
					processTaskLogger = new ProcessTaskLogger(this);
				}

				return processTaskLogger;
			}
		}

		ZDateTimeOffset IWorkflowTask.P9_ScheduledDate { get => P9_ScheduledDateForBinding; set => P9_ScheduledDateForBinding = value; }

		ZDateTimeOffset IWorkflowTask.P9_ActualDate => P9_ActualDateForBinding;

		ZDateTimeOffset IWorkflowTask.P9_SuspendedAt { get => P9_SuspendedAtForBinding; set => P9_SuspendedAtForBinding = value; }

		ProcessTaskLogger processTaskLogger;
		#endregion

		sealed class ProcessTaskDeleteHook : IDisposable
		{
			public ProcessTaskDeleteHook()
			{
				ProcessTask.OnDeleteAction.Value = (processTask) => Report(processTask);
			}

			public void Dispose()
			{
				ProcessTask.OnDeleteAction.Value = null;
			}

			public void Report(ProcessTask processTask)
			{
				ErrorReporter.ReportOnce("ProcessTaskDeleteHook", "A Process Task was deleted during CreateTasksAndMilestonesFromTemplateIfRequired().");
			}
		}

		void IUniversalCopiedIdentifier.MarkAsCopiedByUniversalCopy()
		{
			CreatedByUniversalCopy = true;
		}

		public bool CreatedByUniversalCopy { get; private set; }
	}
	#region For Auto-Generated Code Only

	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class ProcessTasks : ProcessTask
	{
		public ProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}

	#endregion
}

#region Test
#if DEBUG
namespace Enterprise.MasterFiles.Business
{
	public partial class ProcessTask
	{
		static readonly Overridable<Action> OnCreateTasksAndMilestonesFromTemplateIfRequired = new Overridable<Action>(null);

		public static void SetOnCreateTasksAndMilestonesFromTemplateIfRequiredForTest(Action hook)
		{
			OnCreateTasksAndMilestonesFromTemplateIfRequired.Value = hook;
		}

		static partial void HookOnCreateTasksAndMilestonesFromTemplateIfRequired()
		{
			OnCreateTasksAndMilestonesFromTemplateIfRequired.Value?.Invoke();
		}
	}
}
// Tests are now in Enterprise.MasterFiles.Business.Test assembly, ProcessTasksTest.cs
#endif
#endregion
