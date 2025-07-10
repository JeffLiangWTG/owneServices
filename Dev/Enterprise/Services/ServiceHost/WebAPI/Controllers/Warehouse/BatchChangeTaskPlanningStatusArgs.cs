using System;
using System.Collections.Generic;

namespace Enterprise.Services.ServiceHost
{
	public class BatchChangeTaskPlanningStatusArgs
	{
		public IEnumerable<Guid> JobPKs;
		public string JobType;
		public bool ChangeStatusToReady;
	}
}
