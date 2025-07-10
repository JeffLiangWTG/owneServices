using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
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
	public class OrderWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.OrderWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Forwarding|OrderWorkflowDescriptor|Description", "Forwarding Order"); }
		}

		public override ZString MilestoneTemplateHintCaption
		{
			get { return Res.GetString("e3ba95db-28d4-443a-ba21-61c39d8e081e", "Shipment milestones are combined with the Order milestones below to produce the complete list."); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("c1d083e4-3df0-4436-b552-63162bb766e8", "Transport Mode"), TransportModeList));
				return result.ToArray();
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("3cad3031-167d-4665-85b4-87f5f58caaaf", "All"));
				result.AddRange(new CodeDescriptionPairList(OLookUpEditType.TransportType));
				return result;
			}
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override CodeDescriptionPairList GetConditionList1(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new OrderWorkflowCondition1CodeList();
		}

		public override CodeDescriptionPairList GetConditionList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new OrderWorkflowCondition2CodeList();
		}

		#endregion

		#region EstimateDefaultedFromList

		public override CodeDescriptionPairList EstimateDefaultedFromList
		{
			get { return new OrderMilestoneEstimateDefaultedFromList(); }
		}

		#endregion

		#region Workflow Trigger

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				JobOrderLineSchema.JO_QtyReceived,
				JobOrderHeaderSchema.JD_BookingConfRef,
				JobOrderHeaderSchema.JD_DeliveryRequiredBy
			};
		}

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return true; }
		}

		public override bool SupportsWorkflowTriggerActionXMLWithJobFallback
		{
			get { return true; }
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
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.WarehouseInwards;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.Order }; }
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queued)
		{
			var action = source.Action;
			IProcessor result = base.GetWorkflowTriggerActionCore(source, queued);
			if (result != null)
			{
				return result;
			}

			EventsWithSourceType orderTriggeredByEvents = !queued.ChangeLogs.Any() ? new EventsWithSourceType(EventsWithSourceType.SourceType.Order, action, source.Job) : EventsWithSourceType.Empty;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType) || WorkflowTriggerActionTypeConstants.IsXmlWithJobFallback(action.PQ_TriggerType))
			{
				Order order = (Order)source.Job;
				if (action.Parent.TriggerFieldName == JobOrderHeaderSchema.JD_BookingConfRef.Name
					|| !OrdersDataRegistry.Instance.SuppressXmlTriggersOnOrdersWithZeroQuantity.Value
					|| order.OrderLines.HasOrHadOrderLinesWithQuantityReceived())
				{
					var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
					result = GetWorkflowTriggerActionForXmlActionType(order, xmlModes, action.PQ_TriggerType, orderTriggeredByEvents, action);
				}
			}
			else
			{
				result = base.GetWorkflowTriggerActionCore(source, queued);
			}

			return result;
		}

		IMessageProcessor GetWorkflowTriggerActionForXmlActionType(Order order, MessageProcessorCommunicationModesResult xmlModes, ZString triggerActionType, EventsWithSourceType orderTriggeredByEvents, ProcessTaskNotification action)
		{
			IMessageProcessor result = null;
			ForwardingShipment shipment = order.Shipment;
			JobShipmentPreplanning shipmentPreplanning = order.PreAdvice;

			if (WorkflowTriggerActionTypeConstants.IsXmlWithJobFallback(triggerActionType))
			{
				if (shipment != null)
				{
					ForwardingShipmentWorkflowDescriptor shipmentDescriptor = new ForwardingShipmentWorkflowDescriptor();
					result = shipmentDescriptor.GetWorkflowTriggerActionForXmlActionType(shipment, order, xmlModes, triggerActionType, orderTriggeredByEvents, action);
				}
				else if (shipmentPreplanning != null)
				{
					JobShipmentPreplanningWorkflowDescriptor shipmentPreplanningDescriptor = new JobShipmentPreplanningWorkflowDescriptor();
					result = shipmentPreplanningDescriptor.GetWorkflowTriggerActionForXmlActionType(shipmentPreplanning, order, xmlModes, triggerActionType, orderTriggeredByEvents, action);
				}
			}

			if (result == null)
			{
				result = new XmlMessageDeliver(xmlModes, order, new OrderValueObjectDataAdapter(orderTriggeredByEvents), action);
			}

			return result;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			Order jobOrder = (Order)bizObj;
			AddShipmentPartiesToMessageRecipientPartyList(messageRecipientParties, jobOrder, partyType);
			AddOrderPartiesToMessageRecipientPartyList(messageRecipientParties, jobOrder, partyType);
		}

		void AddShipmentPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, Order jobOrder, ZString partyType)
		{
			ForwardingShipment shipment = jobOrder.Shipment;

			if (shipment != null)
			{
				ForwardingShipmentWorkflowDescriptor shipmentDescriptor = new ForwardingShipmentWorkflowDescriptor();
				shipmentDescriptor.AddAllShipmentPartiesToMessageRecipientPartyList(messageRecipientParties, shipment, partyType);
			}
		}

		void AddOrderPartiesToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, Order jobOrder, ZString partyType)
		{
			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.Consignee:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(jobOrder.Buyer, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.Consignor:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(jobOrder.Supplier, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.SendingAgent:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(jobOrder.SendingAgent, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.ReceivingAgent:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(jobOrder.ReceivingAgent, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.WarehouseInwards:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(jobOrder.WarehouseDocAddress));
					break;
			}
		}

		#endregion

		#region Form Customisation

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new OrderFormCustomisationSettingsProvider();
		}

		#endregion

		#region Default Date

		protected override (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType)
		{
			if (!dateTimeSourceType.IsEmpty && defaultedFromDateProvider.Parent is Order concreteParent)
			{
				if (dateTimeSourceType == OrderMilestoneEstimateDefaultedFromList.Codes.OrderDate && concreteParent.JD_OrderDate.IsValid)
				{
					return (concreteParent.JD_OrderDate, null);
				}
				else if (dateTimeSourceType == OrderMilestoneEstimateDefaultedFromList.Codes.OrderLineRequiredDate)
				{
					var date = GetOrderLineMinRequiredDate(concreteParent);

					if (date.IsValid)
					{
						return (GetOrderLineMinRequiredDate(concreteParent), null);
					}
				}
			}

			return (ZDateTime.Empty, null);
		}

		ZDateTime GetOrderLineMinRequiredDate(Order parent)
		{
			var result = ZDateTime.Empty;
			if (parent != null)
			{
				foreach (OrderLine orderLine in parent.OrderLines)
				{
					if (result.IsEmpty || orderLine.JO_LineDropDate < result)
					{
						result = orderLine.JO_LineDropDate;
					}
				}
			}
			return result;
		}

		#endregion

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(Order); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Orders; }
		}

		protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return new OrderEventDataModel((Order)businessObject);
		}

		protected override ValidationToolSettings GetValidationToolSettings() => new OrderValidationToolSettings(this);
	}
}
