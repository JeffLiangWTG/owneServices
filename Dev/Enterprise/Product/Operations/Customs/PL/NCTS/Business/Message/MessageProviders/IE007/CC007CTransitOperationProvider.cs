using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC007CTransitOperationProvider : ICC007CTransitOperation
{
	readonly NctsArrivalMovementHeader movementHeader;

	readonly NctsHeader nctsHeader;

	public CC007CTransitOperationProvider(NctsArrivalMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		nctsHeader = Argument.NotNull(movementHeader.Header, nameof(movementHeader) + "." + nameof(movementHeader.Header));
	}

	public string MRN => CachedValueHelper.GetValue(ref mrn, () => nctsHeader.ArrivalMrnFromUser);
	CachedValue<string> mrn;

	public DateTime ArrivalNotificationDateAndTime => CachedValueHelper.GetValue(ref arrivalNotificationDateAndTime, () => movementHeader.BM_ArrivalDate.IsEmpty ? ZDateTime.UtcToday.ToDateTime() : movementHeader.BM_ArrivalDate.ToDateTime());
	CachedValue<DateTime> arrivalNotificationDateAndTime;

	public NCTSIndicator SimplifiedProcedure => CachedValueHelper.GetValue(ref simplifiedProcedure, () => movementHeader.AuthorizationNumber.IsEmpty ? NCTSIndicator.NO : NCTSIndicator.YES);
	CachedValue<NCTSIndicator> simplifiedProcedure;

	public NCTSIndicator IncidentFlag => CachedValueHelper.GetValue(ref incidentFlag, () => nctsHeader.BH_ExportFlag == YesNoList.Codes.Yes ? NCTSIndicator.YES : NCTSIndicator.NO);
	CachedValue<NCTSIndicator> incidentFlag;
}
