using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE044MessageSender : MessageSender
{
	public IE044MessageSender(MessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	protected override ZString MessageType => EUJobMessageTypeList.Codes.NctsArrivalNotification;

	protected override ZString MessageSubType => Constants.MessageSubTypeCodes.IE044;

	protected override ZString PhaseCode => NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;

	protected override IXmlMessageBuilder GetMessageBuilder()
	{
		var provider = new IE044RootProvider(sendingObject);
		return new IE044MessageBuilder(provider);
	}
}
