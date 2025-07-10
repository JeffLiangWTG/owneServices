using System;

namespace Enterprise.Services.ServiceHost
{
	public class GetJobProfitTotalsRequest
	{
		public JobParentInfo JobParentInfo { get; set; }

		public Guid CompanyPK { get; set; }
	}
}
