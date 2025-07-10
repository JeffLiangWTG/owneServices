using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC044CSender : MessageSender<ICC044C>
{
	public CC044CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
		NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(messageSendingAction.Header);
	}

	protected override ICC044C GetDataProvider(BusinessObject messageObject) => new CC044CProvider((NctsHeader)messageSendingAction.NctsHeader);

	protected override IXmlMessageBuilder GetProducer(ICC044C dataProvider) => new CC044CMessageBuilder(dataProvider);

	protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
}
