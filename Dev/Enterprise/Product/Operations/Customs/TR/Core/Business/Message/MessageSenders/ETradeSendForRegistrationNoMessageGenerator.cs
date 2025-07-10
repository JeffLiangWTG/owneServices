using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeSendForRegistrationNoMessageGenerator : ETradeBaseMessageGenerator
	{
		public ETradeSendForRegistrationNoMessageGenerator(IETradeSendForRegistrationNo provider) : base(provider)
		{
			messageBuilder = new ETradeSendForRegistrationNoMessageBuilder();
			this.provider = provider;
		}
		readonly IETradeSendForRegistrationNo provider;
		protected readonly ETradeSendForRegistrationNoMessageBuilder messageBuilder;
		protected override ZString MessageText => messageBuilder.GetMessageText(TRBPassword.GP_UserID, TRBPassword.CurrentDecryptedPassword, ApplicationReference, provider);
		public override ZString MessageType => TRMessageTypes.Codes.TRS;
		protected override ZString ApplicationReference => ETradeMessageSenderHelper.CreateIncrementedReferenceId(Sender, TRBPassword.GP_UserID);
	}
}
