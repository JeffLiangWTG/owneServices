using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC013CSender : MessageSender<ICC013C>
{
	public CC013CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Amendment;

	protected override ICC013C GetDataProvider(BusinessObject messageObject) => new CC013CProvider(messageSendingAction);

	protected override IXmlMessageBuilder GetProducer(ICC013C dataProvider) => new CC013CMessageBuilder(dataProvider);

	protected override void PreSend()
	{
		var nctsHeader = (NctsHeader)MessageObject;
		NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, true);
	}

	protected override void PostSendProcess(NLEDIMessage newMessage)
	{
		base.PostSendProcess(newMessage);
		CreateGuaranteeTransactions(newMessage);
	}

	void CreateGuaranteeTransactions(NLEDIMessage message)
	{
		var movementHeader = ((NctsHeader)MessageObject).MovementHeader;
		movementHeader.GuaranteeTransactionCoordinator.UpdatePendingTransactions(message.EM_MessageNum);
	}
}
