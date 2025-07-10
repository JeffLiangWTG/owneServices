using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using static Enterprise.Customs.PL.NCTS.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE015RootProvider : IIE015Root
{
	public IE015RootProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var nctsHeader = messageSendingObject.NctsHeader;
		movementHeader = Argument.NotNull(nctsHeader.MovementHeader, $"{nameof(messageSendingObject)}.{nameof(MessageSendingObject.NctsHeader)}.{nameof(NctsHeader.MovementHeader)}");
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public IChannelRepresentativeAndLocationOfGoods CountrySpecificDataPL => countrySpecificDataPL ?? (countrySpecificDataPL = new CountrySpecificDataPLProvider(movementHeader, messageSendingObject));
	IChannelRepresentativeAndLocationOfGoods countrySpecificDataPL;

	public ICC015C CC015C => cc015C ?? (cc015C = new CC015CProvider(movementHeader, MessageTypeCodes.IE015));
	ICC015C cc015C;
}
