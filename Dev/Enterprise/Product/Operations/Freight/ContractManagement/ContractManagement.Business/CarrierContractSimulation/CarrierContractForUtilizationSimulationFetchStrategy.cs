using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public class CarrierContractForUtilizationSimulationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CarrierContractForUtilizationSimulationFetchStrategy(CarrierContractForUtilizationSimulation businessObject) : base(businessObject)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			var carrierContract = (CarrierContractForUtilizationSimulation)BusinessObject;

			Factory.AddFetchHint(RatingContractAllocationLineSchema.RCA_RCT_RatingContract, carrierContract.PK);
			Factory.AddFetchHint(RatingContractNamedAccountPivotSchema.RNP_ParentID, carrierContract.PK);
			Factory.AddFetchHint(ViewRatingContractSummarySchema.RCV_RCT_RatingContract, carrierContract.PK);
		}
	}
}
