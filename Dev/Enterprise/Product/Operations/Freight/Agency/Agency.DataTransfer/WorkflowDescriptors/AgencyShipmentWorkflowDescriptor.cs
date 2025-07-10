using System;
using System.Collections.Generic;
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
	public class AgencyShipmentWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.AgencyShipmentWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("421ad640-fba6-481c-acb2-a1b3f893a392", "Agency Shipment"); }
		}

		public override ControllerID ControllerID
		{
			get { return null; } // Does not have Controller ID
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(AgencyShipment); }
		}

		protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return new AgencyShipmentEventDataModel((AgencyShipment)businessObject);
		}

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("fe7a21b2-1394-4728-ba78-8f6d097d377e", "Cargo type"), ContainerModeList));
				return list.ToArray();
			}
		}

		protected CodeDescriptionPairList ContainerModeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(Constants.ContainerModes.FCL, Res.GetString("ce3d9dca-74ed-44fa-b08d-4d7030b632e5", "Full Container Load"));
				result.AddPair(Constants.ContainerModes.Liquid, Res.GetString("a3665059-25a3-4051-b845-913578742347", "Liquid"));
				result.AddPair(Constants.ContainerModes.Bulk, Res.GetString("64353575-7ef0-4446-af1f-b7c803057986", "Bulk"));
				result.AddPair(Constants.ContainerModes.BreakBulk, Res.GetString("14f90c3b-613a-4a73-9de2-1bd746b85c68", "Break Bulk"));
				result.AddPair(Constants.ContainerModes.RollOnRollOff, Res.GetString("2dadd52b-bbb3-4dc4-b57e-7b9137c2e75b", "Roll On/Roll Off"));

				return result;
			}
		}

		#endregion

		#region Port Names

		public override ZString Port1Name
		{
			get { return Res.GetString("ff99fb1c-7eba-469f-9b07-e112ed2b3fd0", "Load/Origin"); }
		}

		public override ZString Port2Name
		{
			get { return Res.GetString("6003ff86-d726-4949-a051-35b7d53bb033", "Discharge/Destination"); }
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
			return new AgencyShipmentWorkflowCondition1CodeList();
		}

		public override CodeDescriptionPairList GetConditionList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new AgencyShipmentWorkflowCondition2CodeList();
		}

		#endregion

		#region Workflow Triggers

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return true; }
		}

		public override bool SupportsWorkflowTriggerActionXMLWithJobFallback
		{
			get { return true; }
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				JobShipmentSchema.JS_JX,
				JobShipmentSchema.JS_RL_NKOrigin,
				JobShipmentSchema.JS_RL_NKDestination,
				JobConsolTransportSchema.JW_RL_NKLoadPort,
				JobConsolTransportSchema.JW_RL_NKDiscPort,
				JobConsolTransportSchema.JW_Vessel,
				JobConsolTransportSchema.JW_VoyageFlight
			};
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.SendingAgent |
				MessageRecipientPartyType.ReceivingAgent |
				MessageRecipientPartyType.Principal |
				MessageRecipientPartyType.DepartureCTO |
				MessageRecipientPartyType.BookingParty;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			IProcessor result = null;
			var action = source.Action;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType) || WorkflowTriggerActionTypeConstants.IsXmlWithJobFallback(action.PQ_TriggerType))
			{
				var adapterType = new AgencyShipmentValueObjectDataAdapter<AgencyShipment>().GetType();
				var communicationModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				result = WorkflowDescriptorHelper.GetWorkflowTriggerActionForStandardXmlActionType(adapterType, source.Job as IWorkflowProvider, communicationModes, action);
			}
			else
			{
				result = base.GetWorkflowTriggerActionCore(source);
			}

			return result;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			AgencyShipment shipment = (AgencyShipment)bizObj;
			AddShipmentDirectPartiesToMessageRecipientPartyList(messageRecipientParties, shipment, partyType);
		}

		protected virtual void AddShipmentDirectPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, AgencyShipment shipment, ZString partyType)
		{
			if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsigneeDocumentaryAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ConsignorDocumentaryAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.SendingAgent)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.SendingAgentAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.ReceivingAgent)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ReceivingAgentAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Principal)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Principal, ZString.Empty));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.DepartureCTO && shipment.Sailing != null)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Sailing.DepartureCTOAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.BookingPartyDocumentaryAddress));
			}
		}

		#endregion

		#region Pickup/Delivery Leg (Road/Rail)

		protected Transport PickupRoadOrRailLeg(AgencyShipment shipment)
		{
			if (pickupRoadOrRailLeg == null)
			{
				var shipmentPickupLeg = shipment.PickupRoadOrRailLeg;
				if (shipmentPickupLeg != null && !shipmentPickupLeg.JW_OA_DepartureLocation.IsEmpty)
				{
					pickupRoadOrRailLeg = shipmentPickupLeg;
				}
			}

			return pickupRoadOrRailLeg;
		}
		Transport pickupRoadOrRailLeg;

		protected Transport DeliveryRoadOrRailLeg(AgencyShipment shipment)
		{
			if (deliveryRoadOrRailLeg == null)
			{
				var shipmentDeliveryLeg = shipment.DeliveryRoadOrRailLeg;
				if (shipmentDeliveryLeg != null && !shipmentDeliveryLeg.JW_OA_ArrivalLocation.IsEmpty)
				{
					deliveryRoadOrRailLeg = shipmentDeliveryLeg;
				}
			}

			return deliveryRoadOrRailLeg;
		}
		Transport deliveryRoadOrRailLeg;

		#endregion

		#region EstimateDefaultedFromList

		public override CodeDescriptionPairList EstimateDefaultedFromList => new AgencyShipmentDefaultedFromList();

		protected override (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType)
		{
			if (!dateTimeSourceType.IsEmpty && defaultedFromDateProvider.Parent is AgencyShipment concreteParent)
			{
				if (dateTimeSourceType == AgencyShipmentDefaultedFromList.Codes.AnticipatedTimeOfDeparture && concreteParent.Transports?.Count > 0)
				{
					var departureTransport = concreteParent.Transports[0];
					var result = departureTransport?.JW_ATD ?? ZDateTime.Empty;

					return (result, departureTransport?.LoadPort);
				}
			}

			return (ZDateTime.Empty, null);
		}

		#endregion
	}
}


