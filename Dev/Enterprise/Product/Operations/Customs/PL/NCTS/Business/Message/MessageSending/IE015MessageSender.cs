using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE015MessageSender : IE013IE015MessageSender
{
	public IE015MessageSender(MessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	protected override ZString MessageType => EUJobMessageTypeList.Codes.NctsDeparture;

	protected override ZString MessageSubType => Constants.MessageSubTypeCodes.IE015;

	protected override ZString PhaseCode => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected override IXmlMessageBuilder GetMessageBuilder()
	{
		var provider = new IE015RootProvider(sendingObject);
		return new IE015MessageBuilder(provider);
	}

	protected override void PreSend()
	{
		base.PreSend();
		NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, isAmending: false);
	}
}
