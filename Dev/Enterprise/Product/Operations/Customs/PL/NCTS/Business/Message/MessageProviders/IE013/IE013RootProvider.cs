using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE013RootProvider : IIE013Root
{
	public IE013RootProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var nctsHeader = messageSendingObject.NctsHeader;
		movementHeader = Argument.NotNull(nctsHeader.MovementHeader, $"{nameof(messageSendingObject)}.{nameof(MessageSendingObject.NctsHeader)}.{nameof(NctsHeader.MovementHeader)}");
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public IChannelRepresentativeAndLocationOfGoods CountrySpecificDataPL => countrySpecificDataPL ?? (countrySpecificDataPL = new CountrySpecificDataPLProvider(movementHeader, messageSendingObject));
	IChannelRepresentativeAndLocationOfGoods countrySpecificDataPL;

	public ICC013C CC013C => cc013C ?? (cc013C = new CC013CProvider(movementHeader, Constants.MessageTypeCodes.IE013, messageSendingObject));
	ICC013C cc013C;
}
