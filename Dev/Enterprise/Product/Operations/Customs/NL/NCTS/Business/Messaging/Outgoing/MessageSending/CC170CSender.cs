using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC170CSender : MessageSender<ICC170C>
{
	public CC170CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	protected override ICC170C GetDataProvider(BusinessObject messageObject) => new CC170CProvider(messageSendingAction);

	protected override IXmlMessageBuilder GetProducer(ICC170C dataProvider) => new CC170CMessageBuilder(dataProvider);

	protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Presentation;
}
