using System;

namespace Enterprise.Services.ServiceHost
{
	public class ChangeTaskPlanningStatusArgs
	{
		public Guid JobPK;
		public string JobType;
		public bool ChangeStatusToReady;
	}
}
