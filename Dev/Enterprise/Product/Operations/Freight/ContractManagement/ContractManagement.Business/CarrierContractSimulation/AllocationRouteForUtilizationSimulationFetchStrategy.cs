using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public class AllocationRouteForUtilizationSimulationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public AllocationRouteForUtilizationSimulationFetchStrategy(AllocationRouteForUtilizationSimulation businessObject) : base(businessObject)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			var allocationRoute = (AllocationRouteForUtilizationSimulation)BusinessObject;

			Factory.AddFetchHint(RatingContractNamedAccountPivotSchema.RNP_ParentID, allocationRoute.PK);
			Factory.AddFetchHint(JobSailingSchema.PK, allocationRoute.RCA_JX_SailingSchedule);
			Factory.AddFetchHint(ViewRatingContractAllocationLineSummarySchema.RAV_RCA_AllocationLine, allocationRoute.PK);
		}
	}
}
