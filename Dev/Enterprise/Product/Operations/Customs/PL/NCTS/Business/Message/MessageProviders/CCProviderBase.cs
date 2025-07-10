using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CCProviderBase
{
	protected readonly NctsHeader nctsHeader;

	public CCProviderBase(NctsCommonMovementHeader movementHeader, string messageType)
	{
		Argument.NotNull(movementHeader, nameof(movementHeader));
		nctsHeader = (NctsHeader)Argument.NotNull(movementHeader.Header, $"{nameof(NctsCommonMovementHeader)}.{nameof(movementHeader.Header)}");
		MessageType = Argument.NotNull(messageType, nameof(messageType));
	}

	public IReadOnlyCollection<IAuthorisation> Authorisation => authorisation ??= (nctsHeader.IsPhase5Departure ? (IBusinessObjectCollection<CusAuthorizationUsage>)nctsHeader.MovementHeader.CusAuthorizationUsages : nctsHeader.CusAuthorizationUsages)
		.Select((x, index) => new AuthorisationProvider(x, index + 1))
		.ToArray();
	IReadOnlyCollection<IAuthorisation> authorisation;

	public string CorrelationIdentifier => null; // not used in outgoing messages

	public string MessageIdentification => EDIMessage.PLMessageNumberPlaceHolder;

	public string MessageRecipient => Constants.MessageRecipient;

	public string MessageSender => CachedValueHelper.GetValue(ref messageSender, GetMessageSender);
	CachedValue<string> messageSender;

	public string MessageType { get; }

	public DateTime PreparationDateAndTime => CachedValueHelper.GetValue(ref preparationDateAndTime, () => ZDateTime.Now.ToDateTime());
	CachedValue<DateTime> preparationDateAndTime;

	string GetMessageSender()
	{
		var result = string.Empty;
		if (!GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsEmpty)
		{
			result = GlbBranch.CurrentBranch.OrgProxy?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) ?? ZString.Empty;
		}
		if (string.IsNullOrEmpty(result)
			&& !GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsEmpty)
		{
			result = GlbCompany.CurrentCompany.OrgProxy?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) ?? ZString.Empty;
		}
		return result;
	}
}
