using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE007MessageSender : MessageSender
{
	public IE007MessageSender(MessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	protected override ZString MessageType => EUJobMessageTypeList.Codes.NctsArrivalNotification;

	protected override ZString MessageSubType => Constants.MessageSubTypeCodes.IE007;

	protected override ZString PhaseCode => NctsMovementHeaderTransactionStatusList.Codes.Arrival;

	protected override IXmlMessageBuilder GetMessageBuilder()
	{
		var provider = new IE007RootProvider(sendingObject);
		return new IE007MessageBuilder(provider);
	}
}
