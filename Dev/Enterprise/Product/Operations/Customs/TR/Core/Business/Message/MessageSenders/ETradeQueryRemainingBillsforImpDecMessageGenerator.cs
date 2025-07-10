using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeQueryRemainingBillsforImpDecMessageGenerator : ETradeBaseMessageGenerator
	{
		public ETradeQueryRemainingBillsforImpDecMessageGenerator(IETradeQueryRemainingBillsforImpDec provider) : base(provider)
		{
			messageBuilder = new ETradeQueryRemainingBillsforImpDecMessageBuilder();
			this.provider = provider;
		}
		readonly IETradeQueryRemainingBillsforImpDec provider;
		protected readonly ETradeQueryRemainingBillsforImpDecMessageBuilder messageBuilder;
		protected override ZString MessageText => messageBuilder.GetMessageText(provider);
		public override ZString MessageType => TRMessageTypes.Codes.TRB;
		protected override ZString ApplicationReference => provider.JobReference;
	}
}
