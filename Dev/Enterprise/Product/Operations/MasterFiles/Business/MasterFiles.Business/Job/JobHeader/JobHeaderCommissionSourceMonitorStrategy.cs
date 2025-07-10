using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobHeaderCommissionSourceMonitorStrategy : CommissionSourceMonitorStrategy
	{
		readonly JobHeader job;
		readonly ZPropertyInfo info;

		public JobHeaderCommissionSourceMonitorStrategy(JobHeader job) : base(new[] { job.JH_OA_LocalChargesAddrInfo }, job)
		{
			info = job.JH_OA_LocalChargesAddrInfo;
			this.job = job;
		}

		protected override bool CheckPropertiesChanged()
		{
			var result = base.CheckPropertiesChanged();

			if (!result)
			{
				return false;
			}

			var originalAddress = job.Factory.Load<OrgAddress>((ZGuid)info.OriginalValue);
			var newAddress = job.Factory.Load<OrgAddress>((ZGuid)info.Value);

			if (originalAddress == null && newAddress == null)
			{
				return false;
			}

			return originalAddress == null || newAddress == null || originalAddress.OA_OH != newAddress.OA_OH;
		}
	}
}
