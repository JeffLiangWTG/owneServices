using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class JobShipmentPreplanningWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.JobShipmentPreplanningWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Forwarding|JobShipmentPreplanningWorkflowDescriptor|Description", "Shipment Pre Advice"); }
		}

		#endregion

		#region EstimateDefaultedFromList

		public override CodeDescriptionPairList EstimateDefaultedFromList
		{
			get { return new ShipmentPreAdviceMilestoneEstimateDefaultedFromList(); }
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
				JobConsolTransportSchema.JW_RL_NKLoadPort,
				JobConsolTransportSchema.JW_RL_NKDiscPort,
				JobConsolTransportSchema.JW_Vessel,
				JobConsolTransportSchema.JW_VoyageFlight,
				JobConsolTransportSchema.JW_ETD,
				JobConsolTransportSchema.JW_ETA,
				JobConsolTransportSchema.JW_ATD,
				JobConsolTransportSchema.JW_ATA,
				JobShipmentSchema.JS_HouseBill,
				JobShipmentPreplanningSchema.EF_HouseBill,
				JobShipmentPreplanningSchema.EF_MasterBill,
				JobOrderLineSchema.JO_QtyReceived,
				JobShipmentPreplanningSchema.EF_JS,
				JobShipmentPreplanningSchema.EF_JE,
			};
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.Broker |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.ReceivingAgent |
				MessageRecipientPartyType.SendingAgent |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		//changed - rewrite test for this
		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var action = source.Action;
			EventsWithSourceType shipmentPrePlanningTriggeredByEvents = (queuedLog == null || !queuedLog.ChangeLogs.Any()) ? new EventsWithSourceType(EventsWithSourceType.SourceType.ShipmentPrePlanning, action, source.Job) : EventsWithSourceType.Empty;
			IProcessor result = null;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType) || WorkflowTriggerActionTypeConstants.IsXmlWithJobFallback(action.PQ_TriggerType))
			{
				JobShipmentPreplanning preadvice = (JobShipmentPreplanning)source.Job;
				if (!OrdersDataRegistry.Instance.SuppressXmlTriggersOnOrdersWithZeroQuantity.Value
					|| preadvice.Orders.HasOrHadOrdersWithQuantityReceived())
				{
					var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
					result = GetWorkflowTriggerActionForXmlActionType(preadvice, preadvice, xmlModes, action.PQ_TriggerType, shipmentPrePlanningTriggeredByEvents, action);
				}
			}
			else
			{
				result = base.GetWorkflowTriggerActionCore(source, queuedLog);
			}

			return result;
		}

		internal IMessageProcessor GetWorkflowTriggerActionForXmlActionType(JobShipmentPreplanning shipmentPreplanning, BusinessObject triggerParent, MessageProcessorCommunicationModesResult xmlModes, ZString triggerActionType, EventsWithSourceType triggeredByEvents, ProcessTaskNotification action)
		{
			IMessageProcessor result = null;

			if (WorkflowTriggerActionTypeConstants.IsXmlWithJobFallback(triggerActionType) && shipmentPreplanning.Shipment != null)
			{
				ForwardingShipmentWorkflowDescriptor shipmentDescriptor = new ForwardingShipmentWorkflowDescriptor();
				result = shipmentDescriptor.GetWorkflowTriggerActionForXmlActionType(shipmentPreplanning.Shipment, shipmentPreplanning, xmlModes, triggerActionType, triggeredByEvents, action);
			}
			else
			{
				result = new XmlMessageDeliver(xmlModes, shipmentPreplanning, triggerParent as IJobNumber, new ShipmentPreAdviceValueObjectDataAdapter(triggerParent as Order, triggeredByEvents), action);
			}

			return result;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			JobShipmentPreplanning shipmentPreplanning = (JobShipmentPreplanning)bizObj;
			AddShipmentPartiesToMessageRecipientPartyList(messageTriggerParties, shipmentPreplanning, partyType);
			AddShipmentPreplanningPartiesToMessageRecipientPartyList(messageTriggerParties, shipmentPreplanning, partyType);
		}

		void AddShipmentPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, JobShipmentPreplanning shipmentPreplanning, ZString partyType)
		{
			ForwardingShipment shipment = shipmentPreplanning.Shipment;

			if (shipment != null)
			{
				ForwardingShipmentWorkflowDescriptor shipmentDescriptor = new ForwardingShipmentWorkflowDescriptor();
				shipmentDescriptor.AddAllShipmentPartiesToMessageRecipientPartyList(messageTriggerParties, shipment, partyType);
			}
		}

		void AddShipmentPreplanningPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, JobShipmentPreplanning shipmentPreplanning, ZString partyType)
		{
			if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipmentPreplanning.Buyer, ZString.Empty));
			}

			if (partyType == MessageRecipientPartyTypeList.Codes.ReceivingAgent)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipmentPreplanning.ReceivingAgent, ZString.Empty));
			}

			if (partyType == MessageRecipientPartyTypeList.Codes.SendingAgent)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipmentPreplanning.SendingAgent, ZString.Empty));
			}
		}

		#endregion

		#region Default Date

		protected override (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType)
		{
			if (defaultedFromDateProvider.Parent is JobShipmentPreplanning concreteParent)
			{
				if (dateTimeSourceType == ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETD)
				{
					var transport = concreteParent.PreAdviceTransports.FindTransportByLoadPort(concreteParent.EF_RL_NKPortLoad);
					if (transport != null)
					{
						return (transport.JW_ETD, transport.LoadPort);
					}
				}
				else if (dateTimeSourceType == ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETA)
				{
					var transport = concreteParent.PreAdviceTransports.FindTransportByDischargePort(concreteParent.EF_RL_NKPortDisch);
					if (transport != null)
					{
						return (transport.JW_ETA, transport.DiscPort);
					}
				}
			}

			return (ZDateTime.Empty, null);
		}

		#endregion

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(JobShipmentPreplanning); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.JobShipmentPreplanning; }
		}

		protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return new JobShipmentPreplanningEventModel((JobShipmentPreplanning)businessObject);
		}
	}
}

#region Workflow Triggers
#endregion
