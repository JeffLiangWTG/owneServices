using CargoWise.Customs.PL.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE141MessageSender : MessageSender
{
	readonly IE141RootProvider provider;

	public IE141MessageSender(MessageSendingObject sendingObject) : base(sendingObject)
	{
		provider = new IE141RootProvider(sendingObject);
	}

	protected override ZString MessageType => EUJobMessageTypeList.Codes.NctsDeparture;

	protected override ZString MessageSubType => Constants.MessageSubTypeCodes.IE141;

	protected override ZString PhaseCode => NctsMovementHeaderTransactionStatusList.Codes.NonArrivedMovement;

	protected override IXmlMessageBuilder GetMessageBuilder() => new IE141MessageBuilder(provider);

	protected override ZString GetPrettyView(EDIMessage message, NctsHeader header)
		=> new NctsEdiMessagePrettier<IE141RootProvider>(message, provider).MakeOutboundPrettyForInterpretation(header);
}
