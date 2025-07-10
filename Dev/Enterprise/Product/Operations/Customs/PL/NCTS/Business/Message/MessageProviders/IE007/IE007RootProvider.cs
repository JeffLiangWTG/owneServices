using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using static Enterprise.Customs.PL.NCTS.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE007RootProvider : IE007Root
{
	public IE007RootProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var nctsHeader = messageSendingObject.NctsHeader;
		movementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, $"{nameof(messageSendingObject)}.{nameof(MessageSendingObject.NctsHeader)}.{nameof(NctsHeader.ArrivalMovementHeader)}");
	}

	readonly NctsArrivalMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public IChannelRepresentativeAndLocationOfGoods CountrySpecificDataPL => countrySpecificDataPL ?? (countrySpecificDataPL = new CountrySpecificDataPLProvider(movementHeader, messageSendingObject));
	IChannelRepresentativeAndLocationOfGoods countrySpecificDataPL;

	public ICC007C CC007C => cc007C ?? (cc007C = new CC007CProvider(movementHeader, MessageTypeCodes.IE007));
	ICC007C cc007C;
}
