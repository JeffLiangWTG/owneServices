using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.FetchStrategies
{
	class JobComInvLineComponentInventoryFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobComInvLineComponentInventoryFetchStrategy(JobComInvLineComponentInventory jobComInvLineComponentInventory)
			: base(jobComInvLineComponentInventory)
		{
		}

		protected new JobComInvLineComponentInventory BusinessObject
		{
			get { return (JobComInvLineComponentInventory)base.BusinessObject; }
		}
	}
}
