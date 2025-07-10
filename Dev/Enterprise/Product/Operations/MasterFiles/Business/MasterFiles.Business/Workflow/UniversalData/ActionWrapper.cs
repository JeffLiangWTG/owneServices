using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.UniversalData
{
	public class ActionWrapper : IUniversalActionInfo, IEDIMessageDeliveryContextProvider
	{
		public ActionWrapper(ProcessTaskNotification action, BusinessObject parent, Lazy<IStmALog> eventProvider, IQueuedLog workflowTriggerEvent = null)
		{
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(action.Parent, "action.Parent");

			Action = action;
			ActionType = action.PQ_TriggerType;
			PurposeCode = action.PQ_MessagePurpose;
			purposeCodes = Lazy.Create(() => Action.Lookups.ProcessTaskTriggerPurposeList);
			PopulateRecipientRoleDetails(action.PQ_TriggerParty, action.PQ_TriggerPartyService);

			FactoryForProcessing = action.Factory;

			var trigger = (IWorkflowTrigger)action.Parent;

			TriggerEventCode = trigger.TriggerEventCode;
			TriggerDescription = trigger.Description;

			switch (trigger.WorkflowItemType)
			{
				case Constants.Workflow.WorkflowTriggerType:
					TriggerType = TriggerType.Trigger;
					break;
				case Constants.Workflow.MilestoneType:
					TriggerType = TriggerType.Milestone;
					break;
				case Constants.Workflow.ExceptionType:
					TriggerType = TriggerType.Exception;
					break;
			}

			var processTask = trigger as ProcessTask;

			if (processTask != null)
			{
				TriggerScheduledDate = processTask.P9_ScheduledDateForBinding;
			}

			TriggerActualDate = trigger.LastFiredTime.IsEmpty && workflowTriggerEvent != null ? workflowTriggerEvent.EventTimeOffset : trigger.LastFiredTime;
			TriggerReference = trigger.TriggerConditionValue;

			TriggerCountLazy = workflowTriggerEvent != null ? new Lazy<ZInt>(() =>
			{
				var query = new ZQuery().AddToFilter(StmALogSchema.SL_Parent, trigger.Identifier).AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				query.OrderBy = $"{StmALogSchema.Constants.SL_PostedTimeUtc} ASC, {StmALogSchema.Constants.SL_EventTime} ASC";
				var wteEvents = trigger.Factory.Load<StmALog>(query);
				return (Array.FindIndex(wteEvents, (wteEvent) => workflowTriggerEvent.SJ_ALogReference == wteEvent.PK) + 1);
			}) : new Lazy<ZInt>(() => trigger.Factory.GetDatabaseCount(typeof(StmALog), new ZQuery(new ZQuery(StmALogSchema.SL_Parent, trigger.Identifier), new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode))), false);

			ParentBO = parent;
			RecipientOrganization = action.Recipient;

			this.eventProvider = eventProvider;
		}

		public ActionWrapper(ManualDataExport dataExport, BusinessObject parent)
		{
			PurposeCode = dataExport.PurposeCode;
			purposeCodes = Lazy.Create(() => dataExport.Lookups.PurposeCodeList);
			PopulateRecipientRoleDetails(dataExport.RecipientType, dataExport.RecipientService);
			TriggerEventCode = dataExport.EventCode;
			TriggerDescription = dataExport.TriggerDescription;
			TriggerType = TriggerType.Manual;
			TriggerScheduledDate = ZDateTimeOffset.Empty;
			TriggerActualDate = ZDateTimeOffset.Now;
			TriggerReference = dataExport.EventReference;
			ParentBO = parent;
			FactoryForProcessing = dataExport.Factory;
			RecipientOrganization = dataExport.RecipientOrganization;
		}

		public ActionWrapper(BusinessObject parent, ZString recipientRole, string recipientService = "")
		{
			ParentBO = parent;
			FactoryForProcessing = parent.Factory;
			PopulateRecipientRoleDetails(recipientRole, recipientService);
		}

		public ZString ActionType { get; private set; }
		public ZString PurposeCode { get; private set; }
		public RecipientRoleDetail[] RecipientRoleDetails { get; private set; } = Array.Empty<RecipientRoleDetail>();
		public CodeDescriptionPairList PurposeCodeList => purposeCodes.Value;
		readonly Lazy<CodeDescriptionPairList> purposeCodes;

		public ZString TriggerEventCode { get; private set; }
		public ZString TriggerEventReference => TriggeringEvent?.SL_Reference ?? TriggerReference;
		public ZString TriggerDescription { get; private set; }
		public ZInt TriggerCount => TriggerCountLazy?.Value ?? 0;
		Lazy<ZInt> TriggerCountLazy { get; set; }
		public TriggerType TriggerType { get; private set; }
		public ZDateTimeOffset TriggerScheduledDate { get; private set; }
		public ZDateTimeOffset TriggerActualDate { get; private set; }
		public ZString TriggerReference { get; private set; }
		public IStmALog TriggeringEvent => eventProvider?.Value;
		readonly Lazy<IStmALog> eventProvider;
		public INotifications Notifications { get; set; }

		public BusinessObject ParentBO { get; private set; }
		public BusinessObjectFactory FactoryForProcessing { get; private set; }
		public IOrgHeader RecipientOrganization { get; private set; }

		public void PopulateRecipientRoleDetails(ZString recipientTypeCode, ZString recipientServiceCode)
		{
			var recipientRoleCode = GetEnumTypeFromName<RecipientRoleType>(recipientTypeCode);
			if (recipientRoleCode.HasValue)
			{
				var recipientService = GetEnumTypeFromName<ServiceCodeType>(recipientServiceCode);
				RecipientRoleDetails = RecipientRoleDetails.Concat(new[] { new RecipientRoleDetail() { Type = recipientRoleCode.Value, ServiceCode = recipientService } }).ToArray();
			}
		}

		public static TEnum? GetEnumTypeFromName<TEnum>(ZString name) where TEnum : struct
		{
			if (!name.IsEmpty && Enum.TryParse(name, out TEnum enumType))
			{
				return enumType;
			}

			return null;
		}

		#region IEDIMessageDeliveryContextProvider

		readonly ProcessTaskNotification Action;

		BusinessObject[] IEDIMessageDeliveryContextProvider.GetRoots()
		{
			if (Action is IDynamicRootProvider rootProvider)
			{
				return rootProvider.Roots;
			}
			return Array.Empty<BusinessObject>();
		}

		ZGuid IEDIMessageDeliveryContextProvider.EDIMessageDeliveryContextSelectorPK => Action?.PQ_ECS_MessageDeliveryContextSelector ?? ZGuid.Empty;

		#endregion
	}
}
