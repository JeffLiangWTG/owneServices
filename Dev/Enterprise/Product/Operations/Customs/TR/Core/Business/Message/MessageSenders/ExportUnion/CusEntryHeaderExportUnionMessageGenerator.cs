using CargoWise.Customs.TR.MessageContracts.MessageBuilders.ExportUnion;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class CusEntryHeaderExportUnionMessageGenerator : TRBaseMessageGenerator<ExportUnionMessage>
	{
		public CusEntryHeaderExportUnionMessageGenerator(IMessageSender sender) : base(sender)
		{
		}

		public override ZString MessageType => TRMessageTypes.Codes.EUT;

		string fMessageText;

		protected override ZString MessageText
		{
			get
			{
				if (fMessageText == null)
				{
					var messageProvider = new CusEntryHeaderMessageProvider((CusEntryHeader)Sender.Parent, MessageType);
					var messageBuilder = new ExportUnionMessageBuilder(messageProvider);
					fMessageText = messageBuilder.GetXMLMessage(ApplicationReference);
				}
				return fMessageText;
			}
		}

		protected override ZString ApplicationReference => Sender.JobReference;
	}
}
