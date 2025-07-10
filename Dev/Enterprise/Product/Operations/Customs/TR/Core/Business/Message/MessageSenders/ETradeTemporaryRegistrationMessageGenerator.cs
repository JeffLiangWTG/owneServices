using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeTemporaryRegistrationMessageGenerator : ETradeBaseMessageGenerator
	{
		public ETradeTemporaryRegistrationMessageGenerator(IETradeTemporaryRegistration sender) : base(sender)
		{
			messageBuilder = new ETradeTemporaryRegistrationMessageBuilder();
			header = Argument.NotNull(sender, nameof(sender));
		}
		protected readonly ETradeTemporaryRegistrationMessageBuilder messageBuilder;
		readonly IETradeTemporaryRegistration header;
		protected override ZString MessageText => messageBuilder.GetMessageText(TRBPassword.GP_UserID, TRBPassword.CurrentDecryptedPassword, ApplicationReference, header);
		public override ZString MessageType => TRMessageTypes.Codes.TRE;
		protected override ZString MessageOwner => TRBPassword?.GP_UserID ?? ZString.Empty;
		protected override ZString ApplicationReference => ETradeMessageSenderHelper.CreateIncrementedReferenceId(Sender, TRBPassword.GP_UserID);
	}
}
