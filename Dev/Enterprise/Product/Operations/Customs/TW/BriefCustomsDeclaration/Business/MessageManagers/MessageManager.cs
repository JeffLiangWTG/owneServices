using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class MessageManager : SingleMessageManager
	{
		public MessageManager(MessageSendingObject messageSender)
		{
			this.messageSender = messageSender;
		}

		readonly MessageSendingObject messageSender;

		public override string MessageFriendlyName => (NoResString)"Send To Customs Message";

		public override BusinessObject BusinessObject => messageSender;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => true;

		public override bool IsWaitingForResponse => false;

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo) => GenerateMessage(bizo as MessageSendingObject);

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo) => GenerateMessage(bizo as MessageSendingObject);

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo) => GenerateMessage(bizo as MessageSendingObject);

		EDIMessage[] GenerateMessage(MessageSendingObject sendingObject)
		{
			return sendingObject != null ? new EDIMessage[] { GenerateCustomsMessage(sendingObject) } : Array.Empty<EDIMessage>();
		}

		EDIMessage GenerateCustomsMessage(MessageSendingObject sendingObject)
		{
			var header = sendingObject.Header;
			var msg = header.Factory.New<AsycudaMessage>();
			msg.EM_MessageText = sendingObject.SerializeToMessageString();
			msg.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			msg.EM_Status = EDIMessage.Status.Queued;
			msg.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			msg.EM_MessageType = new MessageTypeCodeList().GetCodeFromDescription(sendingObject.MessageType);
			msg.EM_LinkUniqueID = header.PK;
			msg.EM_LinkTable = header.TableName;
			msg.EM_ApplicationReference = header.AMA_CustomsProfile;
			msg.EM_IsTestMessage = TWCustomsDataRegistry.IsTestMode;
			msg.EM_SendWithMessageErrors = header.HasMessageErrors || sendingObject.HasMessageErrors;
			header.Messages.Add(msg);
			return msg;
		}
	}
}
