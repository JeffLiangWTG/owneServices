using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class TRManifestMessageGenerator : TRBaseMessageGenerator<TRManifestMessage>
	{
		public TRManifestMessageGenerator(ISummaryDeclarationInformation sender) : base(sender)
		{
			messageBuilder = new TRManifestMessageBuilder(sender);
			header = Argument.NotNull(sender, nameof(sender));
		}
		readonly TRManifestMessageBuilder messageBuilder;
		readonly ISummaryDeclarationInformation header;

		protected override ZString MessageText => messageBuilder.GetMessageText(TRBPassword.GP_UserID, TRBPassword.CurrentDecryptedPassword, ApplicationReference, header);

		protected override ZString ApplicationReference => Sender.JobReference + "|" + TRBPassword.GP_UserID;

		public override ZString MessageType => TRMessageTypes.Codes.TRO;

		protected override ZString MessageOwner => TRBPassword?.GP_UserID ?? ZString.Empty;
	}
}
