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
	class DtbConsignmentWorkflowDescriptor : WorkflowDescriptor
	{
		#region Code

		public override string Code
		{
			get { return WorkflowDescriptors.DtbConsignmentWorkflowDescriptorCode; }
		}

		#endregion

		#region Description

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("TransportConsignment|DtbConsignmentWorkflowDescriptor|Description", "Land Transport Consignment"); }
		}

		#endregion

		#region ControllerID

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.DtbConsignment; }
		}

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.LTConsignment }; }
		}

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType
		{
			get { return typeof(DtbConsignment); }
		}

		#endregion

		#region Flags

		public override bool AreTasksCompanySpecific => false;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => true;

		#endregion

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
			var consignment = (DtbConsignment)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				foreach (var address in consignment.Addresses.Where(a => a.OrganisationType == OrganisationTypesList.Codes.CNR && a.Address != null))
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(address.Address));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				foreach (var address in consignment.Addresses.Where(x => x.OrganisationType == OrganisationTypesList.Codes.CNE && x.Address != null))
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(address.Address));
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
				var bookedBy = consignment.DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
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
