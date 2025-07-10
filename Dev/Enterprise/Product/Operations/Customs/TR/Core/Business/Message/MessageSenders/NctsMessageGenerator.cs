using CargoWise.Application;
using CargoWise.Customs.TR.MessageContracts.MessageBuilders.NCTS;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class NctsMessageGenerator : TRBaseMessageGenerator<NCTSMessage>
	{
		public NctsMessageGenerator(IMessageSender sender) : base(sender)
		{
		}

		public override ZString MessageType => TRMessageTypes.Codes.TRN;

		string fMessageText;
		protected override ZString MessageText
		{
			get
			{
				if (fMessageText == null)
				{
					var messageProvider = ObjectFactory.Get<Integration.Customs.TR.INctsHeaderProvider>().GetHeaderProvider((Integration.Customs.TR.ICusInBondHeader)Sender.Parent);
					var messageBuilder = new NCTSMessageBuilder((CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS.INCTSHeader)messageProvider);
					fMessageText = messageBuilder.GetXMLMessage();
				}
				return fMessageText;
			}
		}

		protected override ZString ApplicationReference => Sender.JobReference;
	}
}
