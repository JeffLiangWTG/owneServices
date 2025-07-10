using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPickupProcessHandlingInfo(BusinessObject logParent) : ProcessHandlingInfo(logParent)
	{
		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return [];
		}

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			var tpu = Pickup.Factory.Load<CYDTransportationUnit>(Pickup.YPL_YTU_PickupTransportationUnit);
			return tpu is not null ?
				TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(tpu, logBeingAdded, TriggerLineTypes.Codes.CYDPickup).Select(p => p.trigger) :
				[];
		}

		CYDPickup Pickup => (CYDPickup)LogParent;
	}
}
