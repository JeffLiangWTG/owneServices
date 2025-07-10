using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public abstract class Phase5NcstMessageGenerator : TRBaseMessageGenerator<NCTSMessage>
	{
		public Phase5NcstMessageGenerator(IMessageSender sender) : base(sender)
		{
		}

		protected override ZString ApplicationReference => Sender.JobReference;

		protected override ZGuid GetTRBPasswordPK() => TRBPassword.PK;

		protected override ZString MessageText
		{
			get
			{
				if (fMessageText == null)
				{
					var messageProvider = ObjectFactory.Get<Integration.Customs.TR.INctsHeaderProvider>().GetHeaderProvider((Integration.Customs.TR.ICusInBondHeader)Sender.Parent);
					fMessageText = GetXMLMessage();
				}
				return fMessageText;
			}
		}
		string fMessageText;

		protected virtual ZString GetXMLMessage() => ZString.Empty;
	}
}
