using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.MessageDelivery;
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
	class OrderLineWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code => WorkflowDescriptors.OrderLineWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("Forwarding|OrderLineWorkflowDescriptor|Description", "Forwarding Order Line");

		public override ZString MilestoneTemplateHintCaption
			=> Res.GetString("a522d971-9acd-4aa8-bc42-5e7eb7bef71a", "Shipment milestones are combined with the Order Line milestones below to produce the complete list.");

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("c27b224b-1a4b-4275-a7c8-cca7c72e04de", "Transport Mode"), TransportModeList));
				return result.ToArray();
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("4c0973c4-502c-4fae-ad98-fdcbe1aaa930", "All"));
				result.AddRange(new CodeDescriptionPairList(OLookUpEditType.TransportType));
				return result;
			}
		}

		public override bool SupportsTasks => false;
		public override bool SupportsScreenLayout => false;
		public override bool SupportsBufferManagement => false;
		public override bool SupportsUniversalTemplates => true;

		#endregion

		#region Disable trigger and milestone support when ProcessTaskTemplate.P0_IsUniversal is unchecked

		public override bool SupportsEventTracking => true;
		public override bool OnlySupportEventTrackingForUniversalTemplates => true;

		#endregion

		#region Workflow Trigger

		public SchemaColumn[] WorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					JobOrderLineSchema.JO_QtyReceived
				};
			}
		}

		public override bool SupportsWorkflowTriggerActionXML => true;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Consignee
				| MessageRecipientPartyType.Consignor
				| MessageRecipientPartyType.ControllingCustomer;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.Order }; }
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queued)
		{
			var action = source.Action;
			var result = base.GetWorkflowTriggerActionCore(source, queued);
			if (result != null)
			{
				return result;
			}

			var orderTriggeredByEvents = !queued.ChangeLogs.Any() ? new EventsWithSourceType(EventsWithSourceType.SourceType.Order, action, source.Job) : EventsWithSourceType.Empty;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType))
			{
				var order = ((OrderLine)source.Job).Order;
				if (order != null
					&& (!OrdersDataRegistry.Instance.SuppressXmlTriggersOnOrdersWithZeroQuantity.Value
						|| order.OrderLines.HasOrHadOrderLinesWithQuantityReceived()))
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
			return new XmlMessageDeliver(xmlModes, order, new OrderValueObjectDataAdapter(orderTriggeredByEvents), action);
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			var jobOrderLine = bizObj as OrderLine;
			if (jobOrderLine?.Order != null)
			{
				AddOrderPartiesToMessageRecipientPartyList(messageRecipientParties, jobOrderLine.Order, partyType);
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
				case MessageRecipientPartyTypeList.Codes.ControllingCustomer:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(jobOrder.ControllingCustomerDocAddress.Organisation, ZString.Empty));
					break;
			}
		}

		#endregion

		#region Form Customisation

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new OrderLineFormCustomisationSettingsProvider();
		}

		#endregion

		public override bool AreTasksCompanySpecific => false;

		public override ControllerID ControllerID => ControllerIDs.OrderLine;

		public override Type WorkflowProviderType => typeof(OrderLine);

		protected override ValidationToolSettings GetValidationToolSettings() => new OrderLineValidationToolSettings(this);
	}
}

