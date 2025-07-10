using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.IE054;

public class CC054CTransitOperationProvider : ITransitOperationIE054
{
	public CC054CTransitOperationProvider(MessageSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		nctsHeader = Argument.NotNull(sendingObject.NctsHeader, $"{nameof(sendingObject)}.{nameof(MessageSendingObject.NctsHeader)}");
	}

	readonly MessageSendingObject sendingObject;
	readonly NctsHeader nctsHeader;

	public string ReferenceNumber => nctsHeader.MovementReferenceNumber;

	public NCTSIndicator ReleaseRequested => CachedValueHelper.GetValue(ref releaseRequested, ()
		=> sendingObject.ReleaseRequest.EqualsIgnoringCase(ReleaseRequestedFlagList.Codes.Yes) ? NCTSIndicator.YES : NCTSIndicator.NO);
	CachedValue<NCTSIndicator> releaseRequested;

	public DateTime ReleaseDateTime => ZDateTime.Now.ToDateTime();
}
