using System;

namespace Enterprise.Services.ServiceHost
{
	public class CompleteAllWhsJobServicesArgs
	{
		public Guid JobPK;
		public string JobType;
		public bool FinaliseServiceJob;
	}
}
