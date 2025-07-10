using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class InBondWPMQEDIMessageBuilder
	{
		public MQEDIMessage Generate(IInBondArriveExportTOLHeader inBondData, string actionCode, IInbondMessageSendingData messageSendingData = null)
		{
			return Generate(inBondData, null, null, actionCode, messageSendingData);
		}

		public MQEDIMessage Generate(IInBondArriveExportTOLHeader inBondData, Integration.Customs.US.InBond.ICusInBondBill iBill, IMessageAttachee messageBill, string actionCode, IInbondMessageSendingData messageSendingData = null)
		{
			return Generate(inBondData, iBill, messageBill, null, null, actionCode, messageSendingData);
		}

		public MQEDIMessage Generate(IInBondArriveExportTOLHeader inBondData, Integration.Customs.US.InBond.ICusInBondBill iBill, IMessageAttachee messageBill, Integration.Customs.US.InBond.ICusInBondContainer iContainer, IMessageAttachee messageContainer, string actionCode, IInbondMessageSendingData messageSendingData = null)
		{
			var block = new ACEInputBlockControlGenerator(inBondData);
			block.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability;

			AddMessageBlocks(actionCode, block, inBondData, iBill, iContainer, messageSendingData);

			var messageAttache = GetMessageAttachees(actionCode, inBondData, messageBill, messageContainer);

			var message = block.CreateMessage<MQEDIMessage>(inBondData.Factory);
			messageAttache.Messages.Add(message);
			message.EM_MessageSubType = GetMessageSubType(actionCode);
			SetMessageOwner(message);

			if (!InBondWPActionCodeList.IsDiversionRequest(actionCode))
			{
				new InBondMessageStatusCalculator(messageAttache).CalculateStatus(message, ABIResponseStatus.Undefined);
			}

			return message;
		}

		IMessageAttachee GetMessageAttachees(string actionCode, IInBondArriveExportTOLHeader inBondData, IMessageAttachee messageBill, IMessageAttachee messageContainer)
		{
			if (InBondWPActionCodeList.IsContainerLevel(actionCode))
			{
				return messageContainer;
			}
			else if (InBondWPActionCodeList.IsBillLevel(actionCode))
			{
				return messageBill;
			}
			else
			{
				return inBondData;
			}
		}

		void AddMessageBlocks(string actionCode, ACEInputBlockControlGenerator block, IInBondArriveExportTOLHeader inBondData, Integration.Customs.US.InBond.ICusInBondBill iBill, Integration.Customs.US.InBond.ICusInBondContainer iContainer, IInbondMessageSendingData messageSendingData = null)
		{
			if (InBondWPActionCodeList.IsContainerLevel(actionCode))
			{
				block.AddMessageBlocks(new ACEInBondWPMessageBlockBuilder(inBondData, iBill, iContainer, messageSendingData).Build(actionCode));
			}
			else if (InBondWPActionCodeList.IsBillLevel(actionCode))
			{
				block.AddMessageBlocks(new ACEInBondWPMessageBlockBuilder(inBondData, iBill, messageSendingData).Build(actionCode));
			}
			else
			{
				block.AddMessageBlocks(new ACEInBondWPMessageBlockBuilder(inBondData, messageSendingData).Build(actionCode));
			}
		}

		string GetMessageSubType(string actionCode)
		{
			if (InBondWPActionCodeList.IsArrivalAction(actionCode))
			{
				return EM_MessageSubTypeList.Codes.InBondArrival;
			}

			if (InBondWPActionCodeList.IsExportationAction(actionCode))
			{
				return EM_MessageSubTypeList.Codes.InBondExportation;
			}

			if (InBondWPActionCodeList.IsTOLAction(actionCode))
			{
				return EM_MessageSubTypeList.Codes.InBondTransferOfLiability;
			}

			if (InBondWPActionCodeList.IsDiversionRequest(actionCode))
			{
				return EM_MessageSubTypeList.Codes.InBondDiversionRequest;
			}

			return "";
		}

		void SetMessageOwner(MQEDIMessage message)
		{
			message.EM_MessageOwner = Constants.ACE;
		}
	}
}
