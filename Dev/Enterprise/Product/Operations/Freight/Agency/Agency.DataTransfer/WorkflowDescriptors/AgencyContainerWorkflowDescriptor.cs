using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class AgencyContainerWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.AgencyContainerWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("e8d719b2-c4c9-4a6c-97c5-9cd3ca892e93", "Shipping Job Container/Break-bulk"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Containers; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(AgencyShipmentContainer); }
		}

		public override bool SupportsBufferManagement
		{
			get { return false; }
		}

		public override bool SupportsUniversalTemplates => false;

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				return new[]
				{
					new ProcessTemplateSubType(Res.GetString("4069bb02-76b3-4c08-9660-3a7b540c697b", "Cargo type"), new AgencyCargoTypeCodeDescriptionPairList())
				};
			}
		}

		#endregion

		#region Port Names

		public override ZString Port1Name
		{
			get { return Res.GetString("0a7a6c0c-230c-4482-9cf5-d792b4f80867", "Load/Origin"); }
		}

		public override ZString Port2Name
		{
			get { return Res.GetString("c5920d7b-de4a-4c2c-be41-f9d8c3ec3fc3", "Discharge/Destination"); }
		}

		#endregion

		#region Requires Client / Ports

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return true; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		#endregion

		#region Conditions

		public override CodeDescriptionPairList GetConditionList1(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new AgencyContainerWorkflowCondition1CodeList();
		}

		public override CodeDescriptionPairList GetConditionList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new AgencyContainerWorkflowCondition2CodeList();
		}

		public override IWorkflowTemplateApplicationExtender GetTemplateApplicationExtender()
		{
			return templateApplicationExtender ??= new AgencyContainerWorkflowTemplateApplicationExtender();
		}
		AgencyContainerWorkflowTemplateApplicationExtender templateApplicationExtender;

		#endregion

		#region Workflow Triggers

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				JobContainerSchema.JC_RC,
				JobContainerSchema.JC_ContainerCount,
				JobContainerSchema.JC_ContainerNum,
				JobContainerSchema.JC_SetPointTemp,
				JobContainerSchema.JC_SetPointTempUnit,
				JobContainerSchema.JC_RH_NKContainerCommodityCode,
				JobContainerSchema.JC_AirVentFlow
			};
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.Principal |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.DepartureContainerYard |
				MessageRecipientPartyType.ArrivalContainerYard |
				MessageRecipientPartyType.DepartureCTO |
				MessageRecipientPartyType.ArrivalCTO |
				MessageRecipientPartyType.SendingAgent |
				MessageRecipientPartyType.ReceivingAgent |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			AgencyShipmentContainer container = (AgencyShipmentContainer)bizObj;
			AgencyShipment shipment = container.Booking;

			if (shipment != null)
			{
				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.Consignee:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsigneeDocumentaryAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.Consignor:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsignorDocumentaryAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.BillToParty:
						if (shipment.Job != null)
						{
							messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Job.LocalChargesAddr));
						}
						break;
					case MessageRecipientPartyTypeList.Codes.Principal:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Principal, ZString.Empty));
						break;
					case MessageRecipientPartyTypeList.Codes.PickupCartage:
						if (!shipment.IsDeleted)
						{
							var pickupLeg = PickupRoadOrRailLeg(shipment);
							if (pickupLeg != null && pickupLeg.CarrierAddress != null)
							{
								messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(pickupLeg.CarrierAddress));
							}
						}
						break;
					case MessageRecipientPartyTypeList.Codes.DeliveryCartage:
						if (!shipment.IsDeleted)
						{
							var deliveryLeg = DeliveryRoadOrRailLeg(shipment);
							if (deliveryLeg != null && deliveryLeg.CarrierAddress != null)
							{
								messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(deliveryLeg.CarrierAddress));
							}
						}
						break;
					case MessageRecipientPartyTypeList.Codes.DepartureCTO:
						if (shipment.Sailing != null)
						{
							messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Sailing.DepartureCTOAddress));
						}
						break;
					case MessageRecipientPartyTypeList.Codes.ArrivalCTO:
						if (shipment.Sailing != null)
						{
							messageTriggerParties.AddNotNullAndNotDuplicatedItem(
								new MessageRecipientParty(shipment.Sailing.ArrivalCTOAddress));
						}
						break;
					case MessageRecipientPartyTypeList.Codes.SendingAgent:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.SendingAgentAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.ReceivingAgent:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ReceivingAgentAddress));
						break;
				}
			}

			if (container != null)
			{
				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.DepartureContainerYard:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(container.DepartureContainerYardAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.ArrivalContainerYard:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(container.ArrivalContainerYardAddress));
						break;
				}
			}
		}

		protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return new AgencyContainerEventDataModel((AgencyShipmentContainer)businessObject);
		}

		#endregion

		#region Pickup/Delivery Leg (Road/Rail)

		protected Transport PickupRoadOrRailLeg(AgencyShipment shipment)
		{
			if (pickupRoadOrRailLeg == null)
			{
				pickupRoadOrRailLeg = shipment.TransportsIncludingRelated.FirstLegMatching(l =>
					(l.IsRoad || l.IsRail) &&
					l.JW_RL_NKLoadPort == shipment.JS_RL_NKOrigin &&
					l.JW_TransportType != Constants.TransportPlanningType.MainVessel &&
					!l.JW_OA_DepartureLocation.IsEmpty);
			}
			return pickupRoadOrRailLeg;
		}
		Transport pickupRoadOrRailLeg;

		protected Transport DeliveryRoadOrRailLeg(AgencyShipment shipment)
		{
			if (deliveryRoadOrRailLeg == null)
			{
				deliveryRoadOrRailLeg = shipment.TransportsIncludingRelated.LastLegMatching(l =>
					(l.IsRoad || l.IsRail) &&
					l.JW_RL_NKDiscPort == shipment.JS_RL_NKDestination &&
					l.JW_TransportType != Constants.TransportPlanningType.MainVessel &&
					!l.JW_OA_ArrivalLocation.IsEmpty);
			}
			return deliveryRoadOrRailLeg;
		}
		Transport deliveryRoadOrRailLeg;

		#endregion
	}
}


