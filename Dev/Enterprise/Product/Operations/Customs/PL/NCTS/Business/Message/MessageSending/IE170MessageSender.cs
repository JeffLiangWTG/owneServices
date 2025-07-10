using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE170MessageSender : MessageSender
{
	public IE170MessageSender(MessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	protected override ZString MessageType => EUJobMessageTypeList.Codes.NctsDeparture;

	protected override ZString MessageSubType => Constants.MessageSubTypeCodes.IE170;

	protected override ZString PhaseCode => NctsMovementHeaderTransactionStatusList.Codes.Presentation;

	protected override IXmlMessageBuilder GetMessageBuilder()
	{
		var provider = new IE170RootProvider(sendingObject);
		return new IE170MessageBuilder(provider);
	}
}
