using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using static Enterprise.Customs.PL.NCTS.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IE044RootProvider : IIE044Root
{
	public IE044RootProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var nctsHeader = messageSendingObject.NctsHeader;
		movementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, $"{nameof(messageSendingObject)}.{nameof(MessageSendingObject.NctsHeader)}.{nameof(NctsHeader.ArrivalMovementHeader)}");
	}

	readonly NctsArrivalMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public IChannelRepresentativeAndTIRInfo CountrySpecificDataPL => countrySpecificDataPL ?? (countrySpecificDataPL = new CountrySpecificDataPLProvider(movementHeader, messageSendingObject));
	IChannelRepresentativeAndTIRInfo countrySpecificDataPL;

	public ICC044C CC044C => cc044C ?? (cc044C = new CC044CProvider(movementHeader, MessageTypeCodes.IE044));
	ICC044C cc044C;
}
