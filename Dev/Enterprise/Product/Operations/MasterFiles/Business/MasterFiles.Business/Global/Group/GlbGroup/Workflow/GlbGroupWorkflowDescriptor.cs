using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.GlbGroupWorkflowDescriptorCode;
		public override IMultilingualString Description => ResString.GetMultilingualString("MasterFiles|GlbGroupWorkflowDescriptor|Description", "Group");
		public override ControllerID ControllerID => ControllerIDs.GlbGroup;
		public override Type WorkflowProviderType => typeof(GlbGroup);
		public override bool RequiresClient => false;
		public override bool AreTasksCompanySpecific => false;
		public override bool SupportsBufferManagement => false;
		public override bool SupportsEventTracking => true;
		public override bool SupportsTasks => false;
		public override bool ShouldHideTasksTabOnJobs => true;
		public override bool SupportsScreenLayout => false;
		public override bool SupportsWorkflowTemplates => true;
		protected override bool SupportsTaskLineTriggersCore => false;
		protected override bool SupportsReleaseGroupRulesCore => false;

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.GlbGroup };

		public override MessageRecipientPartyType SupportedSendDocumentMessageRecipientParties(ZString triggerAction)
		{
			return base.SupportedSendDocumentMessageRecipientParties(triggerAction) | CommonRecipientPartyTypes;
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return base.SupportedMessageRecipientParties(trigger, business) | CommonRecipientPartyTypes;
		}

		protected internal override bool IsDirectEmailRecipient(string triggerParty)
		{
			return base.IsDirectEmailRecipient(triggerParty)
				|| triggerParty == MessageRecipientPartyTypeList.Codes.CurrentUser
				|| triggerParty == MessageRecipientPartyTypeList.Codes.GroupOwners;
		}

		protected override string[] GetNotificationEmailAddresses(ProcessTaskNotification action, BusinessObject parent)
		{
			switch (action.PQ_TriggerParty)
			{
				case MessageRecipientPartyTypeList.Codes.CurrentUser:
				case MessageRecipientPartyTypeList.Codes.GroupOwners:
					var group = (IEmailAddressGetterForTrigger)parent;

					if (group != null)
					{
						return group.GetEmailAddressesFromTriggerParty(action.PQ_TriggerParty).ToArray();
					}

					break;
			}

			return base.GetNotificationEmailAddresses(action, parent);
		}

		static MessageRecipientPartyType CommonRecipientPartyTypes =>
			MessageRecipientPartyType.Email
			| MessageRecipientPartyType.CurrentUser
			| MessageRecipientPartyType.GroupOwners;
	}
}
