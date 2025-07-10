using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class BillOfLadingWorkflowDescriptor : AgencyShipmentWorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Freight|BillOfLadingWorkflowDescriptor|Description", "Bill Of Lading"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(BillOfLading); }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.AgencyDocumentation }; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return base.SupportedMessageRecipientParties(trigger, business) |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.ArrivalCTO;
		}

		protected override void AddShipmentDirectPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, AgencyShipment shipment, ZString partyType)
		{
			base.AddShipmentDirectPartiesToMessageRecipientPartyList(messageRecipientParties, shipment, partyType);

			if (partyType == MessageRecipientPartyTypeList.Codes.ArrivalCTO && shipment.Sailing != null)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Sailing.ArrivalCTOAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.PickupCartage)
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

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AgencyBillOfLading; }
		}

		public override bool SupportsCreateTransportBooking
		{
			get { return true; }
		}

		public override CodeDescriptionPairList EstimateDefaultedFromList => new BillOfLadingDefaultedFromList();

		protected override (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType)
		{
			if (!dateTimeSourceType.IsEmpty && defaultedFromDateProvider.Parent is BillOfLading concreteParent)
			{
				if (dateTimeSourceType == BillOfLadingDefaultedFromList.Codes.ShippedOnBoard)
				{
					return (concreteParent.JS_ShippedOnBoardDate, concreteParent.LoadPort);
				}
				else if (dateTimeSourceType == BillOfLadingDefaultedFromList.Codes.AnticipatedTimeOfDeparture && concreteParent.Transports?.Count > 0)
				{
					var departureTransport = concreteParent.Transports[0];
					var result = departureTransport?.JW_ATD ?? ZDateTime.Empty;

					return (result, departureTransport?.LoadPort);
				}
				else if (dateTimeSourceType == BillOfLadingDefaultedFromList.Codes.AnticipatedTimeOfArrival && concreteParent.Transports?.Count > 0)
				{
					var arrivalTransport = concreteParent.Transports[concreteParent.Transports.Count - 1];
					var result = arrivalTransport?.JW_ATA ?? ZDateTime.Empty;

					return (result, arrivalTransport?.DiscPort);
				}
			}

			return (ZDateTime.Empty, null);
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new BillOfLadingFormCustomisationSettingsProvider();
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = (CodeDescriptionPairList)base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);

			if (!IsGlobalTemplate(parent))
			{
				if (trigger.IsForCountry((BusinessObject)parent, Core.Constants.CountryCodes.Australia))
				{
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendEIDO, WorkflowTriggerActionTypeConstants.Descriptions.SendEIDO);
				}
				else
				{
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendImportReleaseOrder, WorkflowTriggerActionTypeConstants.Descriptions.SendImportReleaseOrder);
				}
			}

			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var result = base.GetWorkflowTriggerActionCore(source, queuedLog);

			if (result != null)
			{
				return result;
			}

			var action = source.Action;
			var bizo = (BillOfLading)source.Job;

			switch (action.PQ_TriggerType)
			{
				case WorkflowTriggerActionTypeConstants.Codes.SendEIDO:
					result = new EIDOMessageProcessor(bizo);
					break;

				case WorkflowTriggerActionTypeConstants.Codes.SendImportReleaseOrder:
					result = new ReleaseImportOrderMessageProcessor(bizo);
					break;
			}

			return result;
		}
	}
}


