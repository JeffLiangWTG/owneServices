using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Customs.NO.Business;

sealed class CusEntryHeaderProcessHandlingInfo(CusEntryHeader entryHeader) : ProcessHandlingInfo(entryHeader)
{
	protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded) => [];

	protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
	{
		var declaration = entryHeader.Declaration;
		var relatedShipment = declaration?.Shipment;
		var logParent = (IStmALogParent)relatedShipment ?? declaration;
		if (logParent is null)
		{
			return [];
		}

		return TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(logParent, logBeingAdded, TriggerLineTypes.Codes.CusEntryHeader)
			.Union(TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(logParent, logBeingAdded, TriggerLineTypes.Codes.CusNOEmmaMessageGenerator))
			.Select(p => p.trigger);
	}
}
