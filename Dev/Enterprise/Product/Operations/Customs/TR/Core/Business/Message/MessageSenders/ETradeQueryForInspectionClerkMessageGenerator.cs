using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeQueryForInspectionClerkMessageGenerator : ETradeBaseMessageGenerator
	{
		public ETradeQueryForInspectionClerkMessageGenerator(IETradeQueryForInspectionClerk provider) : base(provider)
		{
			messageBuilder = new ETradeQueryForInspectionClerkMessageBuilder();
			this.provider = provider;
		}
		protected readonly ETradeQueryForInspectionClerkMessageBuilder messageBuilder;
		readonly IETradeQueryForInspectionClerk provider;
		protected override ZString MessageText => messageBuilder.GetMessageText(provider);
		public override ZString MessageType => TRMessageTypes.Codes.TRI;
	}
}
