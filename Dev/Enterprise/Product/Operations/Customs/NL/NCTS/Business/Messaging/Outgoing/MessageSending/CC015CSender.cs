using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC015CSender : MessageSender<ICC015C>
{
	public CC015CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected override ICC015C GetDataProvider(BusinessObject messageObject) => new CC015CProvider(messageSendingAction);

	protected override IXmlMessageBuilder GetProducer(ICC015C dataProvider) => new CC015CMessageBuilder(dataProvider);

	protected override void PreSend()
	{
		var nctsHeader = (NctsHeader)MessageObject;
		NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, false);
	}

	protected override void PostSendProcess(NLEDIMessage newMessage)
	{
		base.PostSendProcess(newMessage);
		CreateGuaranteeTransaction(newMessage);
	}

	void CreateGuaranteeTransaction(NLEDIMessage message)
	{
		var movementHeader = ((NctsHeader)MessageObject).MovementHeader;
		movementHeader.GuaranteeTransactionCoordinator.CreatePendingTransactions(message.EM_MessageNum);
	}
}
