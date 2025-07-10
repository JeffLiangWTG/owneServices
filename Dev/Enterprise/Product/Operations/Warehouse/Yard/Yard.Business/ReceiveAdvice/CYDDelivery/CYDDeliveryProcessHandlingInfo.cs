using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDDeliveryProcessHandlingInfo(BusinessObject logParent) : ProcessHandlingInfo(logParent)
	{
		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return [];
		}

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			var tpu = Delivery.Factory.Load<CYDTransportationUnit>(Delivery.YDL_YTU_DeliveryTransportationUnit);
			return tpu is not null ?
				TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(tpu, logBeingAdded, TriggerLineTypes.Codes.CYDDelivery).Select(p => p.trigger) :
				[];
		}

		CYDDelivery Delivery => (CYDDelivery)LogParent;
	}
}
