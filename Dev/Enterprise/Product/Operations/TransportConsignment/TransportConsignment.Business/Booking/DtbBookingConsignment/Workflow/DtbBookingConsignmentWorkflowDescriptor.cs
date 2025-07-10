using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.DtbBookingConsignmentWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("TransportBookingConsignment|DtbBookingConsignmentWorkflowDescriptor|Description", "Transport Consignment"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.DtbBookingConsignment; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.DtbConsignment }; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(DtbBookingConsignment); }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		#region Recipients

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.BookingParty
				| MessageRecipientPartyType.BillToParty
				| MessageRecipientPartyType.Consignor
				| MessageRecipientPartyType.Consignee
				| MessageRecipientPartyType.NotifyParty;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var consignment = (DtbBookingConsignment)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				foreach (var instruction in consignment.Instructions.Where(x => x.OrganisationType == OrganisationTypesList.Codes.CNR && x.Address != null))
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(instruction.Address));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				foreach (var instruction in consignment.Instructions.Where(x => x.OrganisationType == OrganisationTypesList.Codes.CNE && x.Address != null))
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(instruction.Address));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.NotifyParty)
			{
				var notifyParty = consignment.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);
				if (notifyParty != null)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(notifyParty));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BillToParty)
			{
				var billToParty = consignment.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
				if (billToParty != null)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(billToParty));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				var bookedBy = consignment.ConsolidationSingleJob.BookedByAddress;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(bookedBy));
			}
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return triggerAction != WorkflowTriggerActionTypeConstants.Codes.SendDocument && base.IsMessagingOrEmailNotificationTriggerAction(triggerAction);
		}

		#endregion
	}
}
