using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public abstract class TRBaseMessageGenerator<T> : ITRCustomsMessageGenerator
		where T : TRBaseMessage
	{
		protected TRBaseMessageGenerator(IMessageSender sender)
		{
			Sender = Argument.NotNull(sender, nameof(sender));
			Parent = Argument.NotNull(sender.Parent, nameof(sender.Parent));
		}
		protected readonly IMessageSender Sender;
		protected readonly BusinessObject Parent;

		protected T GenerateMessage()
		{
			var message = Parent.Factory.New<T>();
			message.EM_MessageType = MessageType;
			message.EM_MessageSubType = MessageSubType;
			message.EM_IsTestMessage = TRCustomsDataRegistry.Instance.IsTRTestingSystem;
			message.EM_MessageOwner = MessageOwner;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = ApplicationReference;
			message.EM_MessageInterpretation = MessageInterpretation;
			if (!message.NeedToSignMessage)
			{
				message.EM_MessageText = MessageText;
			}
			message.EM_LinkedObject = Parent;
			message.EM_GP = GetTRBPasswordPK();

			return message;
		}

		protected abstract ZString MessageText { get; }
		protected virtual ZString MessageInterpretation => MessageText;
		protected abstract ZString ApplicationReference { get; }
		public abstract ZString MessageType { get; }
		protected virtual ZString MessageSubType => ZString.Empty;
		protected virtual ZString MessageOwner => WhoSendThisMessage != null ? WhoSendThisMessage.GS_Code : ZString.Empty;

		protected GlbExternalPassword_TR TRBPassword => TRGlbStaffWrapper.Get(WhoSendThisMessage)?.TRBPassword;

		protected virtual GlbStaff WhoSendThisMessage => GlbStaff.CurrentUser;

		protected virtual ZGuid GetTRBPasswordPK() => ZGuid.Empty;

		EDIMessage ICustomsMessageGenerator.GenerateMessage() => GenerateMessage();
		ZBool ITRCustomsMessageGenerator.IsMessageSigningRequired => TRBaseMessageExtensions.IsMessageSigningRequired(typeof(T), MessageType);
	}
}
