using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TW.Manifest.Business
{
	class N5101HMessageManager : Customs.Business.SingleMessageManager
	{
		public N5101HMessageManager(N5101HMessageSendingObject messageSender)
		{
			this.messageSender = messageSender;
		}

		readonly N5101HMessageSendingObject messageSender;

		public override string MessageFriendlyName => "N5101HMessage";

		public override BusinessObject BusinessObject => messageSender;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => false;

		public override bool IsWaitingForResponse => false;

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			throw new NotImplementedException();
		}

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as N5101HMessageSendingObject);
		}

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			throw new NotImplementedException();
		}

		EDIMessage[] GenerateMessage(N5101HMessageSendingObject sendingObject)
		{
			return sendingObject != null ? new EDIMessage[] { GenerateCustomsMessage(sendingObject) } : Array.Empty<EDIMessage>();
		}

		AsycudaMessage GenerateCustomsMessage(N5101HMessageSendingObject msgSendingObj)
		{
			var header = msgSendingObj.Header;
			var msg = header.Factory.New<AsycudaMessage>();
			msg.EM_MessageText = new N5101HMessageBuilder().PopulateXml(msgSendingObj);
			msg.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			msg.EM_Status = Status.Queued;
			msg.EM_ReceiveTransmit = Direction.Transmit;
			msg.EM_MessageType = msgSendingObj.MessageType;
			msg.EM_LinkUniqueID = header.PK;
			msg.EM_LinkTable = header.TableName;
			msg.EM_ApplicationReference = header.AMA_MailBox;
			msg.EM_IsTestMessage = TWCustomsDataRegistry.IsTestMode;
			msg.EM_SendWithMessageErrors = header.HasMessageErrors || msgSendingObj.HasMessageErrors;
			msg.Bills = msgSendingObj.Bills;
			header.Messages.Add(msg);
			return msg;
		}
	}
}
