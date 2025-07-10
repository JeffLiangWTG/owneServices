using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.ContainerYard.DataTransfer
{
	public class GateBookingWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.FacilityGateWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("Freight|GateBookingWorkflowDescriptor|Description", "Gate Booking");

		public override ZString WarehouseName => Res.GetString("{62BA6927-8929-4451-9A3B-0E519812C8A4}", "Facility");

		public override ControllerID ControllerID => null;

		public override Type WorkflowProviderType => typeof(GateBooking);

		public override bool SupportsBufferManagement => false;

		public override bool RequiresWarehouse => true;

		public override bool RequiresBranch => true;

		public override bool RequiresClient => false;

		public override bool SupportsEventTracking => true;

		public override CodeDescriptionPairList GetConditionList1(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new GateBookingWorkflowCondition1CodeList();
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.DeliveryCartage
				| MessageRecipientPartyType.Carrier
				| MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.Consignee
				| MessageRecipientPartyType.Consignor;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			if (bizObj is GateBooking gateBooking)
			{
				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.DeliveryCartage:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(gateBooking.TransportCompanyAddress.Header, ZString.Empty));
						break;

					case MessageRecipientPartyTypeList.Codes.Carrier:
						var carrier = gateBooking
							.GateBookingDetails
							.FirstOrDefault()?
							.YardUnit?
							.UnitOwnerAddress?
							.Header;

						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(carrier, ZString.Empty));
						break;

					case MessageRecipientPartyTypeList.Codes.Consignee:
					case MessageRecipientPartyTypeList.Codes.Consignor:
						var bookingPartyAddress = gateBooking
							.GateBookingDetails
							.FirstOrDefault()?
							.BookingPartyAddress;

						if (bookingPartyAddress != null)
						{
							messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(bookingPartyAddress));
						}
						break;
				}
			}
		}

		// Will be supported at a later date
		//protected override bool SupportsWorkflowTriggerActionUniversalEventXML => true;
		//protected override bool SupportsWorkflowTriggerActionUniversalShipmentXML => true;
	}
}
