using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TW.Business.MessageManagers
{
	public class TransferApplicationMessageManager : SingleMessageManager
	{
		public TransferApplicationMessageManager(TranshipmentMessageSendingObject messageSender)
		{
			this.messageSender = messageSender;
		}

		readonly TranshipmentMessageSendingObject messageSender;

		public override string MessageFriendlyName => (NoResString)"Send Transhipment Application";

		public override BusinessObject BusinessObject => messageSender;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => true;

		public override bool IsWaitingForResponse => false;

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as TranshipmentMessageSendingObject);
		}

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as TranshipmentMessageSendingObject);
		}

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as TranshipmentMessageSendingObject);
		}

		EDIMessage[] GenerateMessage(TranshipmentMessageSendingObject sendingObject)
		{
			return new EDIMessage[] { GenerateCustomsMessage(sendingObject) };
		}

		TWMessage GenerateCustomsMessage(TranshipmentMessageSendingObject sendingObject)
		{
			var header = sendingObject.Header;
			var message = header.Factory.New<TWMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message.EM_Status = Status.Queued;
			message.EM_ReceiveTransmit = Direction.Transmit;
			message.EM_MessageType = messageSender.MessageType;
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			message.EM_ApplicationReference = header.BH_CustomsProfile;
			message.EM_IsTestMessage = TWCustomsDataRegistry.IsTestMode;
			message.EM_MessageText = new N5301MessageBuilder(sendingObject as IN5301Declaration).PopulateXml();
			message.EM_MessageOwner = sendingObject.GetMessageOwner().Left(20);
			header.Messages.Add(message);
			UpdateInBondHeaderStatus(header);
			return message;
		}

		void UpdateInBondHeaderStatus(CusInBondHeader header)
		{
			header.BH_MessageStatus = TWMessageStatusCodeList.Codes.AwaitingResponse;
		}

		CusInBondHeader Header => messageSender.Header;

		protected override void OnAmendmentSentCore()
		{
			base.OnAmendmentSentCore();
			Header.Messages.Load();
		}

		protected override void OnOriginalSentCore()
		{
			base.OnOriginalSentCore();
			Header.Messages.Load();
		}

		protected override void OnWithdrawalSentCore()
		{
			base.OnWithdrawalSentCore();
			Header.Messages.Load();
		}
	}
}
