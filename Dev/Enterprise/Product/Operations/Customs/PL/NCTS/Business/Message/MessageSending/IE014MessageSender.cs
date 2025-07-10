using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE014MessageSender : MessageSender
{
	public IE014MessageSender(MessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	protected override ZString MessageType => EUJobMessageTypeList.Codes.NctsDeparture;

	protected override ZString MessageSubType => Constants.MessageSubTypeCodes.IE014;

	protected override ZString PhaseCode => NctsMovementHeaderTransactionStatusList.Codes.Cancellation;

	protected override IXmlMessageBuilder GetMessageBuilder()
	{
		var provider = new IE014RootProvider(sendingObject);
		return new IE014MessageBuilder(provider);
	}
}
