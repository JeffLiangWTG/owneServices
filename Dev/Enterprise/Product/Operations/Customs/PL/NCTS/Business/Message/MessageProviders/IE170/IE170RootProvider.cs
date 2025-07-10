using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used later")]
public class IE170RootProvider : IIE170
{
	public IE170RootProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var nctsHeader = messageSendingObject.NctsHeader;
		movementHeader = Argument.NotNull(nctsHeader.MovementHeader, $"{nameof(messageSendingObject)}.{nameof(MessageSendingObject.NctsHeader)}.{nameof(NctsHeader.MovementHeader)}");
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public IChannelRepresentativeAndLocationOfGoods CountrySpecificDataPL => countrySpecificDataPL ??= new CountrySpecificDataPLProvider(movementHeader, messageSendingObject);
	IChannelRepresentativeAndLocationOfGoods countrySpecificDataPL;

	public ICC170C CC170C => cc170C ??= new CC170CProvider(movementHeader, Constants.MessageTypeCodes.IE170);
	ICC170C cc170C;
}
