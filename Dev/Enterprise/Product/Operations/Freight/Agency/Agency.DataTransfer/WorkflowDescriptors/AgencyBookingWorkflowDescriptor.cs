using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class AgencyBookingWorkflowDescriptor : AgencyShipmentWorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Freight|AgencyBookingWorkflowDescriptor|Description", "Shipping Manager Booking"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(AgencyBooking); }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.AgencyBooking }; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AgencyBooking; }
		}

		public override bool SupportsCreateTransportBooking
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return base.SupportedMessageRecipientParties(trigger, business) |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.DeliveryCartage;
		}

		protected override void AddShipmentDirectPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, AgencyShipment shipment, ZString partyType)
		{
			base.AddShipmentDirectPartiesToMessageRecipientPartyList(messageRecipientParties, shipment, partyType);

			if (shipment != null)
			{
				if (partyType == MessageRecipientPartyTypeList.Codes.PickupCartage)
				{
					var pickupLeg = PickupRoadOrRailLeg(shipment);
					if (pickupLeg != null && pickupLeg.CarrierAddress != null)
					{
						messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(pickupLeg.CarrierAddress));
					}
				}
				else if (partyType == MessageRecipientPartyTypeList.Codes.DeliveryCartage)
				{
					var deliveryLeg = DeliveryRoadOrRailLeg(shipment);
					if (deliveryLeg != null && deliveryLeg.CarrierAddress != null)
					{
						messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(deliveryLeg.CarrierAddress));
					}
				}
			}
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new AgencyBookingFormCustomisationSettingsProvider();
		}
	}
}
