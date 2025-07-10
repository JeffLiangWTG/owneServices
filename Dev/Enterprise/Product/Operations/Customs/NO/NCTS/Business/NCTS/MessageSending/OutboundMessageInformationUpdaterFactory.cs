using System;
using CargoWise.Common;

namespace Enterprise.Customs.NO.NCTS.Business;

static class OutboundMessageInformationUpdaterFactory
{
	public static IMessageInformationUpdater GetInformationUpdater(NctsHeader nctsHeader)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));

		return nctsHeader.IsArrivalMovement
			? new ArrivalOutboundMessageInformationUpdater()
			: throw new NotSupportedException("Message Information Updater is not supported for Departure Movement");
	}
}
