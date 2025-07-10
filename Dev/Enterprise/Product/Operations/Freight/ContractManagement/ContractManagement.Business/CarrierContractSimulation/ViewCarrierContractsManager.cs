using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;

namespace Enterprise.ContractManagement.Business
{
	public class ViewCarrierContractsManager : NonPersistentBusinessObject
	{
		public ViewCarrierContractsManager(
			BusinessObjectFactory factory,
			IContractSimulationFormConfiguration formConfiguration)
			: base(factory)
		{
			this.formConfiguration = formConfiguration;
		}

		readonly IContractSimulationFormConfiguration formConfiguration;

		public CarrierContractQuantityUnitPair JobToBeAllocatedQuantities
		{
			get
			{
				if (jobToBeAllocatedQuantities == null)
				{
					jobToBeAllocatedQuantities = new CarrierContractQuantityUnitPair(
						formConfiguration.QuantityProvider?.GetTotalTEUQuantityForAllocation() ?? 0,
						formConfiguration.QuantityProvider?.GetTotalCNQuantityForAllocation() ?? 0);
				}

				return jobToBeAllocatedQuantities;
			}
		}

		CarrierContractQuantityUnitPair jobToBeAllocatedQuantities;

		public CarrierContractForUtilizationSimulationCollection CarrierContracts
		{
			get
			{
				if (carrierContracts == null)
				{
					carrierContracts = new CarrierContractForUtilizationSimulationCollection(Factory, formConfiguration);
				}

				return carrierContracts;
			}
		}

		CarrierContractForUtilizationSimulationCollection carrierContracts;
	}
}
