using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public sealed class CarrierContractForUtilizationSimulationCollection : ActiveBusinessObjectCollection<CarrierContractForUtilizationSimulation>
	{
		public CarrierContractForUtilizationSimulationCollection(BusinessObjectFactory factory, IContractSimulationFormConfiguration formConfiguration)
			: base(factory, new ZQuery(RatingContractSchema.RCT_ContractType, Core.Constants.RatingContractTypes.Provider))
		{
			this.formConfiguration = formConfiguration;
		}

		readonly IContractSimulationFormConfiguration formConfiguration;

		protected override void OnAdded(CarrierContractForUtilizationSimulation contract)
		{
			base.OnAdded(contract);
			contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
		}

		protected override void OnLoadingIntoCollectionCore(CarrierContractForUtilizationSimulation carrierContract)
		{
			base.OnLoadingIntoCollectionCore(carrierContract);
			carrierContract.FormConfiguration = formConfiguration;
		}
	}
}
