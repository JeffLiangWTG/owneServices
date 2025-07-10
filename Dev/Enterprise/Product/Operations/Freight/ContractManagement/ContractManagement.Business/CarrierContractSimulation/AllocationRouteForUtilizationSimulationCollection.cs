using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public sealed class AllocationRouteForUtilizationSimulationCollection : ActiveBusinessObjectCollection<AllocationRouteForUtilizationSimulation>
	{
		public AllocationRouteForUtilizationSimulationCollection(RatingContract parent, IContractSimulationFormConfiguration formConfiguration)
			: base(parent.Factory, parent, new ZQuery(), RatingContractAllocationLineSchema.RCA_RCT_RatingContract)
		{
			this.formConfiguration = formConfiguration;
		}

		readonly IContractSimulationFormConfiguration formConfiguration;

		protected override void OnLoadingIntoCollectionCore(AllocationRouteForUtilizationSimulation allocationRoute)
		{
			base.OnLoadingIntoCollectionCore(allocationRoute);
			allocationRoute.QuantityProvider = formConfiguration?.QuantityProvider;
		}
	}
}
