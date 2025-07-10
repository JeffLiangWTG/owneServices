using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode;

		public override IMultilingualString Description => WorkRequest.SingularName;

		public override Type WorkflowProviderType => typeof(WorkRequest);

		public override ControllerID ControllerID => ControllerIDs.CustomerServiceTicket;

		public override bool SupportsEventTracking => true;

		public override bool SupportsAddEConversationMessages => true;

		public override bool RequiresBranch => true;
		public override bool RequiresDepartment => true;
		public override bool RequiresPort1 => true;
		public override ZString Port1Name => ResString.GetMultilingualString("WorkRequestWorkflowDescriptor.Port1Name", "Country/Region");

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				return new[]
				{
					new ProcessTemplateSubType(ProcessManagementRegistry.Instance.SelectionCriterion1Caption.Value, () => ProcessManagementRegistry.Instance.SelectionCriterion1Values.Value, isListRequired: false),
					new ProcessTemplateSubType(ProcessManagementRegistry.Instance.SelectionCriterion2Caption.Value, () => ProcessManagementRegistry.Instance.SelectionCriterion2Values.Value, isListRequired: false),
					new ProcessTemplateSubType(ProcessManagementRegistry.Instance.SelectionCriterion3Caption.Value, () => ProcessManagementRegistry.Instance.SelectionCriterion3Values.Value, isListRequired: false),
					new ProcessTemplateSubType(ProcessManagementRegistry.Instance.SelectionCriterion4Caption.Value, () => ProcessManagementRegistry.Instance.SelectionCriterion4Values.Value, isListRequired: false),
					new ProcessTemplateSubType(ProcessManagementRegistry.Instance.SelectionCriterion5Caption.Value, () => ProcessManagementRegistry.Instance.SelectionCriterion5Values.Value, isListRequired: false),
				};
			}
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null) => new SchemaColumn[]
		{
			WorkRequestSchema.WKR_Description,
			WorkRequestSchema.WKR_GB_Branch,
			WorkRequestSchema.WKR_GE_Department,
			WorkRequestSchema.WKR_OC_Client,
			WorkRequestSchema.WKR_RN_NKCountry,
			WorkRequestSchema.WKR_SelectionCriteria1,
			WorkRequestSchema.WKR_SelectionCriteria2,
			WorkRequestSchema.WKR_SelectionCriteria3,
			WorkRequestSchema.WKR_SelectionCriteria4,
			WorkRequestSchema.WKR_SelectionCriteria5,
			WorkRequestSchema.WKR_Status,
			WorkRequestSchema.WKR_Summary,
		};

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Client
				 | MessageRecipientPartyType.Email
				 | MessageRecipientPartyType.JobLevelWorkflowGroup
				 | MessageRecipientPartyType.NotificationGroup
				 | MessageRecipientPartyType.LastCompletedTaskResource;
		}

		protected override bool IsDirectEmailRecipient(string triggerParty)
		{
			return base.IsDirectEmailRecipient(triggerParty) || AreTriggerPartyAddressedHandledExplictly(triggerParty);
		}

		protected override string[] GetNotificationEmailAddresses(ProcessTaskNotification action, BusinessObject parent)
		{
			return AreTriggerPartyAddressedHandledExplictly(action.PQ_TriggerParty)
				? NotificationRecipientCalculator.GetEmailAddressesForTriggerActionNotification(action, parent)
				: base.GetNotificationEmailAddresses(action, parent);
		}

		static bool AreTriggerPartyAddressedHandledExplictly(string triggerParty)
		{
			switch (triggerParty)
			{
				case MessageRecipientPartyTypeList.Codes.Client:
				case MessageRecipientPartyTypeList.Codes.NotificationGroup:
				case MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup:
				case MessageRecipientPartyTypeList.Codes.LastCompletedTaskResource:
					return true;

				default:
					return false;
			}
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new WorkRequestFormCustomisationSettingsProvider();
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var ticket = (WorkRequest)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ticket.Client.Header, ticket.Client.OC_Email));
			}
		}
	}
}
