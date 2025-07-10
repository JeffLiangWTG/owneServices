using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceProcessHandlingInfo : ProcessHandlingInfo
	{
		public JobServiceProcessHandlingInfo(BusinessObject logParent) : base(logParent)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			return Service.RequestForServiceParent is IStmALogParent serviceParent
				? TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(serviceParent, logBeingAdded, TriggerLineTypes.Codes.Service).Select(p => p.trigger)
				: Enumerable.Empty<IBaseTrigger>();
		}

		JobService Service => (JobService)LogParent;
	}
}
