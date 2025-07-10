using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE054MessageSender : MessageSender
{
	public IE054MessageSender(MessageSendingObject sendingObject) : base(sendingObject)
	{ }

	protected override ZString MessageType => EUJobMessageTypeList.Codes.NctsDeparture;

	protected override ZString MessageSubType => Constants.MessageSubTypeCodes.IE054;

	protected override ZString PhaseCode => Constants.MessageSubTypeCodes.IE054;

	protected override IXmlMessageBuilder GetMessageBuilder()
	{
		var provider = new IE054RootProvider(sendingObject);
		return new IE054MessageBuilder(provider);
	}
}
