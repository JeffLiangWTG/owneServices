using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC014CSender : MessageSender<ICC014C>
{
	public CC014CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Cancellation;

	protected override ICC014C GetDataProvider(BusinessObject messageObject) => new CC014CProvider(messageSendingAction);

	protected override IXmlMessageBuilder GetProducer(ICC014C dataProvider) => new CC014CMessageBuilder(dataProvider);
}
