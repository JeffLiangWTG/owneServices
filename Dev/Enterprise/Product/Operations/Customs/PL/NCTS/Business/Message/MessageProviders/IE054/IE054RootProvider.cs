using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using static Enterprise.Customs.PL.NCTS.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE054RootProvider : IIE054Root
{
	public IE054RootProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var nctsHeader = messageSendingObject.NctsHeader;
		movementHeader = Argument.NotNull(nctsHeader.MovementHeader, $"{nameof(messageSendingObject)}.{nameof(MessageSendingObject.NctsHeader)}.{nameof(NctsHeader.MovementHeader)}");
	}

	readonly MessageSendingObject messageSendingObject;
	readonly NctsDepartureMovementHeader movementHeader;

	public IChannelAndRepresentative CountrySpecificDataPL => countrySpecificDataPL ?? (countrySpecificDataPL = new ChannelAndRepresentativeProvider(movementHeader));
	IChannelAndRepresentative countrySpecificDataPL;

	public ICC054C CC054C => cc054C ?? (cc054C = new CC054CProvider(messageSendingObject, MessageTypeCodes.IE054));
	ICC054C cc054C;
}
