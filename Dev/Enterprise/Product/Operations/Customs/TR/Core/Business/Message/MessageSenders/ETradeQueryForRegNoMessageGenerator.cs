using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeQueryForRegNoMessageGenerator : ETradeBaseMessageGenerator
	{
		public ETradeQueryForRegNoMessageGenerator(IETradeQueryForRegNo provider) : base(provider)
		{
			messageBuilder = new ETradeQueryForRegNoMessageBuilder();
			this.provider = provider;
		}
		readonly IETradeQueryForRegNo provider;
		protected readonly ETradeQueryForRegNoMessageBuilder messageBuilder;
		protected override ZString MessageText => messageBuilder.GetMessageText(provider);
		public override ZString MessageType => TRMessageTypes.Codes.TRQ;
	}
}
