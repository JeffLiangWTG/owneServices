using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using static Enterprise.Customs.PL.NCTS.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE014RootProvider : IIE014
{
	public IE014RootProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var nctsHeader = messageSendingObject.NctsHeader;
		movementHeader = Argument.NotNull(nctsHeader.MovementHeader, $"{nameof(messageSendingObject)}.{nameof(MessageSendingObject.NctsHeader)}.{nameof(NctsHeader.MovementHeader)}");
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public IChannelAndRepresentative CountrySpecificDataPL => countrySpecificDataPL ?? (countrySpecificDataPL = new ChannelAndRepresentativeProvider(movementHeader));
	IChannelAndRepresentative countrySpecificDataPL;

	public ICC014C CC014C => cc014C ?? (cc014C = new CC014CProvider(movementHeader, MessageTypeCodes.IE014, messageSendingObject));
	ICC014C cc014C;
}
