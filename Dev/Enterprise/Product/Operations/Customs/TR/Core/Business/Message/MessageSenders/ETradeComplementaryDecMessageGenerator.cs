using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeComplementaryDecMessageGenerator : ETradeBaseMessageGenerator
	{
		public ETradeComplementaryDecMessageGenerator(IETradeComplementaryDec provider) : base(provider)
		{
			messageBuilder = new ETradeComplementaryDecMessageBuilder(provider);
		}
		protected readonly ETradeComplementaryDecMessageBuilder messageBuilder;
		protected override ZString MessageText => messageBuilder.GetMessageText(TRBPassword.GP_UserID, TRBPassword.CurrentDecryptedPassword, ApplicationReference);
		public override ZString MessageType => TRMessageTypes.Codes.TCD;
		protected override ZString ApplicationReference => ETradeMessageSenderHelper.CreateIncrementedReferenceId(Sender, TRBPassword.GP_UserID);
	}
}
