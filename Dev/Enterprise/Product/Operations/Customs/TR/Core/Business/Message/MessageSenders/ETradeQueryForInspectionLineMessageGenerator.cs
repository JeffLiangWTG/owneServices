using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeQueryForInspectionLineMessageGenerator : ETradeBaseMessageGenerator
	{
		public ETradeQueryForInspectionLineMessageGenerator(IETradeQueryForInspectionLine provider) : base(provider)
		{
			messageBuilder = new ETradeQueryForInspectionLineMessageBuilder();
			this.provider = provider;
		}
		protected readonly ETradeQueryForInspectionLineMessageBuilder messageBuilder;
		readonly IETradeQueryForInspectionLine provider;
		protected override ZString MessageText => messageBuilder.GetMessageText(provider);
		public override ZString MessageType => TRMessageTypes.Codes.TRL;
	}
}
