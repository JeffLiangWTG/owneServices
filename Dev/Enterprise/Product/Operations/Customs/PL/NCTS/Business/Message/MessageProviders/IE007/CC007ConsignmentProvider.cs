using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC007ConsignmentProvider : ICC007Consignment
{
	public CC007ConsignmentProvider(NctsArrivalMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		nctsHeader = Argument.NotNull(movementHeader.Header, nameof(movementHeader) + "." + nameof(movementHeader.Header));
	}

	readonly NctsArrivalMovementHeader movementHeader;
	readonly NctsHeader nctsHeader;

	public ILocationOfGoods LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, ()
		=> movementHeader.GoodsLocation.CGL_LocationUse == CusGoodsLocationUseList.Codes.Arrival ? new LocationOfGoodsProvider(movementHeader.GoodsLocation) : null);
	CachedValue<ILocationOfGoods> locationOfGoods;

	public IReadOnlyCollection<IIncident> Incidents => incidents ?? (incidents = GetIncidents());
	IReadOnlyCollection<IIncident> incidents;

	IReadOnlyCollection<IIncident> GetIncidents() => nctsHeader.IsInPhase5TransitionPeriod && nctsHeader.BH_ExportFlag == EventFlagList.Codes.Yes
		? movementHeader.Header.EnRouteIncidents.Cast<EnRouteIncident>().Select((x, i) => new IncidentProvider(i + 1, x)).ToArray()
		: Array.Empty<IIncident>();
}
