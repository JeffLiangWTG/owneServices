using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderWorkflowDescriptor : WhsDocketWorkflowDescriptor, IWorkflowParentWithLines
	{
		protected override ZString GetCode() => JobInvoicingConsumerTypes.WarehouseOutwards.Code;

		protected override IMultilingualString GetDescription() => JobInvoicingConsumerTypes.WarehouseOutwards.MultilingualDescription;

		protected override ControllerID GetControllerID() => ControllerIDs.WhsOrder;

		public override Type WorkflowProviderType => typeof(WhsOrder);

		/// <summary>
		/// Provides action types that are specific to WhsOrderWorkflowDescriptor and available for the ProcessTask if specified;
		/// otherwise action types available for any ProcessTask
		/// </summary>
		/// <param name="task">The ProcessTask to find available action types</param>
		/// <returns>The list of available action types</returns>
		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = (CodeDescriptionPairList)base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);
			result.AddPair(ActionTypes.Codes.Confirmation, ActionTypes.Descriptions.Confirmation);
			result.AddPair(ActionTypes.Codes.IFSExport, ActionTypes.Descriptions.IFSExport);
			result.AddPair(ActionTypes.Codes.GenerateTransportReferenceNumber, ActionTypes.Descriptions.GenerateTransportReferenceNumber);
			result.AddPair(ActionTypes.Codes.GeneratePickForOrder, ActionTypes.Descriptions.GeneratePickForOrder);
			return result;
		}

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.WhsOrder };

		protected override MessageRecipientPartyType SupportedMessageRecipientPartiesCore(IBaseTrigger trigger)
			=> MessageRecipientPartyType.Consignee
			| MessageRecipientPartyType.TransportCo
			| MessageRecipientPartyType.DeliveryCartage
			| MessageRecipientPartyType.Forwarder
			| MessageRecipientPartyType.CarrierBookingAgent
			| MessageRecipientPartyType.WarehouseInwards;

		#region WorkflowTriggerActionProcessor

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queued)
		{
			var order = (WhsOrder)source.Job;
			var action = source.Action;

			switch (action.PQ_TriggerType)
			{
				case ActionTypes.Codes.GenerateTransportReferenceNumber:
					return new GenerateTransportReferenceNumberProcessor(order);

				case ActionTypes.Codes.GeneratePickForOrder:
					return new WhsOrderGeneratePickProcessor(order);

				case ActionTypes.Codes.IFSExport:
					return GetIFSExportAction();

				case ActionTypes.Codes.Confirmation:
					return GetConfirmationAction();

				default:
					return base.GetWorkflowTriggerActionCore(source, queued);
			}

			IProcessor GetIFSExportAction()
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.IFS);
				return new XmlMessageDeliverIFS(xmlModes, order, new WhsOrderCartageValueObjectDataAdapterIFS(), action);
			}

			IProcessor GetConfirmationAction()
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				var valueAdapter = new WhsOrderConfirmationValueObjectDataAdapter(
					GetDocketTriggeredByEvents(
						source,
						EventsWithSourceType.SourceType.WhsOrder,
						queued,
						action));

				return new XmlMessageDeliver(xmlModes, order, valueAdapter, action);
			}
		}

		#endregion

		#region SubTypeInformation

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				if (subTypeInformation == null)
				{
					var salesChannelCollection = new WhsSalesChannelCollection(CreateNewFactory());
					subTypeInformation = new[]
					{
						new ProcessTemplateSubType(Enterprise.Warehouse.Transactions.DataTransfer.Res.GetString("419ea1ad-e08c-49ac-956d-66a24d12c47c", "Sales Channel"), salesChannelCollection)
					};
				}
				return subTypeInformation;
			}
		}
		ProcessTemplateSubType[] subTypeInformation;

		#endregion

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			var order = (WhsOrder)bizObj;
			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.Consignee:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(order.ConsigneeDocAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.TransportCo:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(order.TransportCoDocAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.Forwarder:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(order.Forwarder, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.DeliveryCartage:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(order.TransportCoDocAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.CarrierBookingAgent:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(order.CarrierBookingAgentDocAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.WarehouseInwards:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(order.DestinationWarehouseDocAddress));
					break;
			}
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
			=> base.IsMessagingOrEmailNotificationTriggerAction(triggerAction) || triggerAction == ActionTypes.Codes.IFSExport;

		public override bool SupportsCreateTransportBooking => true;

		public IEnumerable<string> SupportedTriggerLineTypes => new[] { TriggerLineTypes.Codes.PkgPackage };
	}
}
