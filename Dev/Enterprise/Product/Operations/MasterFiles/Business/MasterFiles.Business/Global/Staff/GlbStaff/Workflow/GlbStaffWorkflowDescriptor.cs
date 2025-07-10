using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public const string WorkflowTypeCode = WorkflowDescriptors.GlbStaffDescriptorCode;

		public override string Code
		{
			get { return WorkflowTypeCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("77BDF9-C33E-4769-9D6A-47F881631485", "Staff and Resources"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(GlbStaff); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.GlbStaff; }
		}

		#endregion

		#region Capabilities

		public override bool RequiresClient { get { return false; } }

		public override bool RequiresBranch { get { return true; } }

		public override bool RequiresDepartment { get { return true; } }

		public override bool SupportsEventTracking { get { return true; } }

		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance { get { return false; } }

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Staff;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.GlbStaff }; }
		}

		protected internal override bool IsDirectEmailRecipient(string triggerParty)
		{
			return base.IsDirectEmailRecipient(triggerParty)
				|| triggerParty == MessageRecipientPartyTypeList.Codes.Staff;
		}

		protected override string[] GetNotificationEmailAddresses(ProcessTaskNotification action, BusinessObject parent)
		{
			if (action.PQ_Calc_TriggerParty == MessageRecipientPartyTypeList.Codes.Staff)
			{
				return new[] { ((GlbStaff)parent).GS_EmailAddress.ToString() };
			}

			return base.GetNotificationEmailAddresses(action, parent);
		}

		protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			if (partyType == MessageRecipientPartyTypeList.Codes.Staff)
			{
				var org = GlbCompany.GetCurrentCompany(bizObj.Factory).OrgProxy;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(org, ((GlbStaff)bizObj).GS_EmailAddress));
			}
		}

		#endregion
	}
}
