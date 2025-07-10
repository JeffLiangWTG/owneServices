using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC014InvalidationProvider : ICC014CInvalidation
{
	public CC014InvalidationProvider(NctsDepartureMovementHeader movementHeader, MessageSendingObject messageSendingObject)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		nctsHeader = Argument.NotNull(movementHeader.Header, $"{nameof(NctsDepartureMovementHeader)}.{nameof(NctsDepartureMovementHeader.Header)}");
	}

	readonly MessageSendingObject messageSendingObject;
	readonly NctsDepartureMovementHeader movementHeader;
	readonly NctsHeader nctsHeader;

	public DateTime RequestDateAndTime => ZDateTime.Now.ToDateTime();

	public DateTime? DecisionDateAndTime => null;

	public NCTSIndicator? Decision => null;

	public NCTSIndicator InitiatedByCustom => NCTSIndicator.NO;

	public string Justification => messageSendingObject.Justification;
}
