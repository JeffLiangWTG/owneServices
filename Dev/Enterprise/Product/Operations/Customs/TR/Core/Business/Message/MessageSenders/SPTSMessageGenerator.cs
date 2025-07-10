using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class SPTSMessageGenerator : TRBaseMessageGenerator<SPTSMessage>
	{
		public SPTSMessageGenerator(ISPTS provider) : base(provider)
		{
			messageBuilder = new SPTSMessageBuilder(provider);
			this.provider = provider;
		}
		readonly SPTSMessageBuilder messageBuilder;
		readonly ISPTS provider;

		protected override ZString MessageText => messageBuilder.GetMessageText(TRBPassword.GP_UserID, TRBPassword.CurrentDecryptedPassword, ApplicationReference);
		protected override ZString ApplicationReference => provider.JobReference + "|" + TRBPassword.GP_UserID;
		public override ZString MessageType => TRMessageTypes.Codes.TSP;
	}
}
