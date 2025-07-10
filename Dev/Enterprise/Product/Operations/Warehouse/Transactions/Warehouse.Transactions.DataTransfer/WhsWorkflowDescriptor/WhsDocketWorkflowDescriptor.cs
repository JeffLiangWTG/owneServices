using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsDocketWorkflowDescriptor : WorkflowDescriptor
	{
		#region Code / Description / ControllerID

		public sealed override string Code => GetCode();

		public sealed override IMultilingualString Description => GetDescription();

		public sealed override ControllerID ControllerID => GetControllerID();

		#endregion

		#region Requires Client

		public override bool RequiresClient => true;

		#endregion

		#region Requires Warehouse

		public override bool RequiresWarehouse => true;

		#endregion

		#region Events

		public override bool SupportsEventTracking => true;

		#endregion

		#region Workflow Triggers

		protected virtual bool SupportDocketPlanningStatus => false;

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			if (SupportDocketPlanningStatus)
			{
				result.AddPair(ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning, ActionTypes.Descriptions.SetTaskPlanningStatusToReadyForPlanning);
			}
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var docket = (WhsDocket)source.Job;
			var action = source.Action;

			switch (action.PQ_TriggerType)
			{
				case ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning:
					return new SetTaskPlanningStatusToReadyForPlanningProcessor(docket);

				default:
					return base.GetWorkflowTriggerActionCore(source, queuedLog);
			}
		}

		public sealed override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Client
				| MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.Warehouse
				| SupportedMessageRecipientPartiesCore(trigger);
		}

		protected virtual MessageRecipientPartyType SupportedMessageRecipientPartiesCore(IBaseTrigger trigger) => MessageRecipientPartyType.None;

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var docket = (WhsDocket)bizObj;
			var warehouseAddress = new Lazy<OrgAddress>(() => docket.Warehouse?.WarehouseAddress);

			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(docket.Client, ZString.Empty));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Warehouse && warehouseAddress.Value != null)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(warehouseAddress.Value));
			}
		}

		protected EventsWithSourceType GetDocketTriggeredByEvents(WorkflowTriggerActionSource source, EventsWithSourceType.SourceType sourceType, IQueuedLog queued, ProcessTaskNotification action)
			=> queued != null && !queued.ChangeLogs.Any() ? new EventsWithSourceType(sourceType, action, source.Job) : EventsWithSourceType.Empty;

		#endregion

		#region Workflow Tasks

		public override bool AreTasksCompanySpecific => false;

		#endregion

		#region IsMessagingOrEmailNotificationTriggerAction

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
			=> base.IsMessagingOrEmailNotificationTriggerAction(triggerAction) || triggerAction == ActionTypes.Codes.Confirmation;

		#endregion

		#region GetFormCustomisationSettingsProvider

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
			=> new WhsDocketFormCustomisationSettingsProvider();

		public class WhsDocketFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
		{
			protected override string[] GetPropertiesThatAffectWorkflow() => new[] { WhsDocketSchema.WD_OH_Client.Name };
		}

		#endregion

		protected abstract ZString GetCode();
		protected abstract IMultilingualString GetDescription();
		protected abstract ControllerID GetControllerID();
	}
}
