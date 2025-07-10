using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business
{
	public abstract class ETradeBaseMessageGenerator : TRBaseMessageGenerator<ETradeEDIMessage>
	{
		protected ETradeBaseMessageGenerator(IMessageSender sender)
			: base(sender)
		{
		}

		protected override ZString ApplicationReference => Sender.JobReference + "|" + GlbStaff.CurrentUser.GS_Code;
	}
}
