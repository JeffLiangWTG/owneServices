using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC007CSender : MessageSender<ICC007C>
{
	public CC007CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	protected override ICC007C GetDataProvider(BusinessObject messageObject) => new CC007CProvider(messageSendingAction.Header);

	protected override IXmlMessageBuilder GetProducer(ICC007C dataProvider) => new CC007CMessageBuilder(dataProvider);

	protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Arrival;
}
