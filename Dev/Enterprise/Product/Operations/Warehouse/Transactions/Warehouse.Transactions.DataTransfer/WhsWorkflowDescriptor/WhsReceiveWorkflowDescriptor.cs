using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveWorkflowDescriptor : WhsDocketWorkflowDescriptor
	{
		#region Overrides

		protected override ZString GetCode()
			=> JobInvoicingConsumerTypes.WarehouseInwards.Code;

		protected override IMultilingualString GetDescription()
			=> JobInvoicingConsumerTypes.WarehouseInwards.MultilingualDescription;

		protected override ControllerID GetControllerID() => ControllerIDs.WhsReceive;

		public override Type WorkflowProviderType => typeof(WhsReceive);

		public override bool SupportsCreateTransportBooking => true;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				if (subTypeInformation == null)
				{
					subTypeInformation = new[]
						{
							new ProcessTemplateSubType(Enterprise.Warehouse.Transactions.DataTransfer.Res.GetString("2959f5b0-e2e4-445b-ad3d-74bb15984933", "Receive Category"),
							WarehouseDataRegistry.Instance.ReceiveCategories.Value)
						};
				}
				return subTypeInformation;
			}
		}
		ProcessTemplateSubType[] subTypeInformation;

		#endregion

		#region Workflow Triggers

		protected override bool SupportDocketPlanningStatus => WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value;

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			result.AddPair(ActionTypes.Codes.Confirmation, ActionTypes.Descriptions.Confirmation);
			result.AddPair(ActionTypes.Codes.GeneratePalletIdForReceive, ActionTypes.Descriptions.GeneratePalletIdForReceive);
			result.AddPair(ActionTypes.Codes.AutoPalletizeForReceive, ActionTypes.Descriptions.AutoPalletizeForReceive);
			result.AddPair(ActionTypes.Codes.StartedReceiving, ActionTypes.Descriptions.StartedReceiving);
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queued)
		{
			var receive = (WhsReceive)source.Job;
			var action = source.Action;

			switch (action.PQ_TriggerType)
			{
				case ActionTypes.Codes.GeneratePalletIdForReceive:
					return new ReceivePalletIDGenerator(receive);

				case ActionTypes.Codes.AutoPalletizeForReceive:
					return new ReceiveAutoPalletiseProcessor(receive);

				case ActionTypes.Codes.Confirmation:
					return GetConfirmationAction();

				case ActionTypes.Codes.StartedReceiving:
					return new StartedReceivingProcessor(receive);

				default:
					return base.GetWorkflowTriggerActionCore(source, queued);
			}

			IProcessor GetConfirmationAction()
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				var valueAdapter = new WhsReceiveConfirmationValueObjectDataAdapter(
					GetDocketTriggeredByEvents(source,
					EventsWithSourceType.SourceType.WhsReceipt,
					queued,
					action));

				return new XmlMessageDeliver(xmlModes, receive, valueAdapter, action);
			}
		}

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.WhsInwards };

		#endregion

		#region Recipients

		protected override MessageRecipientPartyType SupportedMessageRecipientPartiesCore(IBaseTrigger trigger)
			=> MessageRecipientPartyType.PickupCartage | MessageRecipientPartyType.TransportCo;

		protected override void AddToMessageRecipientPartyList(
			MessageRecipientPartyCollection messageTriggerParties,
			BusinessObject bizObj,
			ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var receive = (WhsReceive)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.TransportCo)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(receive.TransportCoDocAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.PickupCartage)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(receive.TransportCoDocAddress));
			}
		}

		#endregion
	}
}
