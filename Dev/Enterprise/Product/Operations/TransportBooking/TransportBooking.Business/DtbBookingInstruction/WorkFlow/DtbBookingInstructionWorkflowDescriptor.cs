using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingInstructionWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.DtbBookingInstructionWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("TransportBookings|DtbBookingInstructionWorkflowDescriptor|Description", "Transport Booking Instruction"); }
		}

		public override ControllerID ControllerID
		{
			get { return null; } // Does not have controller ID
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(DtbBookingInstruction); }
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var instruction = (DtbBookingInstruction)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.TransportCo)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(instruction.Booking.Address));
			}
			else if ((partyType == MessageRecipientPartyTypeList.Codes.Consignor && instruction.OrganisationType == OrganisationTypesList.Codes.CNR) ||
						 (partyType == MessageRecipientPartyTypeList.Codes.Consignee && instruction.OrganisationType == OrganisationTypesList.Codes.CNE))
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(instruction.Address));
			}
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return !(triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendDocument) && base.IsMessagingOrEmailNotificationTriggerAction(triggerAction);
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.TransportCo |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.Consignee;
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override bool SupportsBufferManagement
		{
			get { return false; }
		}
	}
}
