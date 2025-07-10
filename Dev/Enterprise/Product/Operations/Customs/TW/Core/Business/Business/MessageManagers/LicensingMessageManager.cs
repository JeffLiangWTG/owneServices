using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TW.Business.MessageManagers
{
	public class LicensingMessageManager : SingleMessageManager
	{
		public LicensingMessageManager(LicensingMessageSendingObject messageSender)
		{
			this.messageSender = messageSender;
		}

		readonly LicensingMessageSendingObject messageSender;

		public override string MessageFriendlyName => (NoResString)"Send Licensing Message";

		public override BusinessObject BusinessObject => messageSender;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => true;

		public override bool IsWaitingForResponse => false;

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo) => GenerateMessage(bizo);

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo) => GenerateMessage(bizo);

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo) => GenerateMessage(bizo);

		EDIMessage[] GenerateMessage(BusinessObject sendingObject)
		{
			return new EDIMessage[] { GenerateCustomsMessage((LicensingMessageSendingObject)sendingObject) };
		}

		TWMessage GenerateCustomsMessage(LicensingMessageSendingObject sendingObject)
		{
			var header = sendingObject.Header;
			var message = header.Factory.New<TWMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message.EM_Status = Status.Queued;
			message.EM_ReceiveTransmit = Direction.Transmit;
			message.EM_MessageType = sendingObject.EM_MessageType;
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = CusTWControllingMessageHeaderSchema.Constants.TableName;
			message.EM_ApplicationReference = header.EntryInstruction?.JobDeclaration?.JE_CustomsProfile ?? ZString.Empty;
			message.EM_IsTestMessage = TWCustomsDataRegistry.IsTestMode;
			message.EM_MessageText = sendingObject.SerializeToMessageString();
			message.EM_MessageOwner = ZString.Empty;
			header.Messages.Add(message);
			header.TW1_MessageStatus = TWMessageStatusCodeList.Codes.AwaitingResponse;
			return message;
		}

		CusTWControllingMessageHeader Header => messageSender.Header;

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
