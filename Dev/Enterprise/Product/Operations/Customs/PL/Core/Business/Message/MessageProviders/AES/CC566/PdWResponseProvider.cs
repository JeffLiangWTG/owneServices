using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

sealed class PdWResponseProvider(BaseMessageSendingObject sendingObject) : IPdWResponse
{
	readonly BaseMessageSendingObject sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));

	public DateTime NotificationDate => ZDateTime.UtcToday.ToDateTime();

	public bool CorrectionAcceptance => sendingObject.CorrectionAcceptance == MessageSendingObjectCorrectionAcceptanceList.Codes._1;

	public string AcceptanceComment => sendingObject.AcceptanceComment;
}
