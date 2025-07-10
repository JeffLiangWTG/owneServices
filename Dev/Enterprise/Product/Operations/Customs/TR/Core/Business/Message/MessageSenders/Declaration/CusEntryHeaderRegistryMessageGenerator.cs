using CargoWise.Customs.TR.MessageContracts.MessageBuilders.Declaration;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class CusEntryHeaderRegistryMessageGenerator : TRBaseMessageGenerator<TRImportExportMessage>
	{
		public CusEntryHeaderRegistryMessageGenerator(IMessageSender sender) : base(sender)
		{
		}

		public override ZString MessageType => TRMessageTypes.Codes.DTE;

		string fMessageText;
		protected override ZString MessageText
		{
			get
			{
				if (fMessageText == null)
				{
					var messageProvider = new CusEntryHeaderMessageProvider((CusEntryHeader)Sender.Parent, MessageType);
					var messageBuilder = new DeclarationMessageBuilder(messageProvider);
					fMessageText = messageBuilder.GetXMLMessage(TRBPassword.GP_UserID, TRBPassword.CurrentDecryptedPassword, ApplicationReference);
				}
				return fMessageText;
			}
		}

		protected override ZString ApplicationReference => Sender.JobReference;
	}
}
