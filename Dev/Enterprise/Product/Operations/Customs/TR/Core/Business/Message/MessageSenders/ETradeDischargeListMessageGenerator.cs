using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeDischargeListMessageGenerator : ETradeBaseMessageGenerator
	{
		public ETradeDischargeListMessageGenerator(IDischargeList sender) : base(sender)
		{
			messageBuilder = new ETradeDischargeListMessageBuilder(sender);
			header = Argument.NotNull(sender, nameof(sender));
		}
		readonly IDischargeList header;
		protected readonly ETradeDischargeListMessageBuilder messageBuilder;
		protected override ZString MessageText => messageBuilder.GetMessageText(TRBPassword.GP_UserID, TRBPassword.CurrentDecryptedPassword, ApplicationReference, header);
		public override ZString MessageType => TRMessageTypes.Codes.TRD;
		protected override ZString ApplicationReference => ETradeMessageSenderHelper.CreateIncrementedReferenceId(Sender, TRBPassword.GP_UserID);
	}
}
