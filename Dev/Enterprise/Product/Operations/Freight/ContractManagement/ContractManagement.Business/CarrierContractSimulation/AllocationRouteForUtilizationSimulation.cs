using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ContractManagement.Business
{
	public sealed class AllocationRouteForUtilizationSimulation : RatingContractAllocationLine
	{
		public AllocationRouteForUtilizationSimulation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		CarrierContractForUtilizationSimulation ParentContract => Factory.GetCachedValue("ContractUtilizationSimulation_" + RCA_RCT_RatingContract,
			() => Factory.Load<CarrierContractForUtilizationSimulation>(RCA_RCT_RatingContract));

		public CarrierContractQuantityUnitPair SimulatedUtilizationWithJobToBeAllocated
		{
			get
			{
				if (simulatedUtilizationWithJobToBeAllocated == null)
				{
					simulatedUtilizationWithJobToBeAllocated = new CarrierContractQuantityUnitPair(
						SimulatedUtilizationWithJobToBeAllocatedTEU,
						SimulatedUtilizationWithJobToBeAllocatedCN);
				}

				return simulatedUtilizationWithJobToBeAllocated;
			}
		}

		CarrierContractQuantityUnitPair simulatedUtilizationWithJobToBeAllocated;

		public CarrierContractQuantityUnitPair SimulatedOutstandingCommitedWithJobToBeAllocated
		{
			get
			{
				if (simulatedOutstandingCommitedWithJobToBeAllocated == null)
				{
					simulatedOutstandingCommitedWithJobToBeAllocated = new CarrierContractQuantityUnitPair(
						Math.Max(ParentContract.CarrierContractQuantities.TEUValue - SimulatedUtilizationWithJobToBeAllocatedTEU, 0),
						Math.Max(ParentContract.CarrierContractQuantities.ContainerValue - SimulatedUtilizationWithJobToBeAllocatedCN, 0));
				}

				return simulatedOutstandingCommitedWithJobToBeAllocated;
			}
		}

		CarrierContractQuantityUnitPair simulatedOutstandingCommitedWithJobToBeAllocated;

		public CarrierContractQuantityUnitPair SimulatedOutstandingWithVarianceWithJobToBeAllocated
		{
			get
			{
				if (simulatedOutstandingWithVarianceWithJobToBeAllocated == null)
				{
					simulatedOutstandingWithVarianceWithJobToBeAllocated = new CarrierContractQuantityUnitPair(
						Math.Max((ParentContract.ViewRatingContractQuantity?.RCQ_CapacityWithVarianceTU ?? 0) - SimulatedUtilizationWithJobToBeAllocatedTEU, 0),
						Math.Max((ParentContract.ViewRatingContractQuantity?.RCQ_CapacityWithVarianceCN ?? 0) - SimulatedUtilizationWithJobToBeAllocatedCN, 0));
				}

				return simulatedOutstandingWithVarianceWithJobToBeAllocated;
			}
		}

		CarrierContractQuantityUnitPair simulatedOutstandingWithVarianceWithJobToBeAllocated;

		public IRatingContractSimulationQuantityProvider QuantityProvider { get; internal set; }

		ZDecimal SimulatedUtilizationWithJobToBeAllocatedTEU
		{
			get
			{
				if (RCA_AllocatedUQ.EqualsIgnoringCase(Constants.AllocationQuantityUnits.TwentyFootUnits))
				{
					return (ParentContract.ViewRatingContractSummary?.RCV_TEUCount ?? 0) + QuantityProvider?.GetTotalTEUQuantityNotAllocatedToContract(ParentContract) ?? 0;
				}
				else
				{
					return ParentContract.ViewRatingContractSummary?.RCV_TEUCount ?? 0;
				}
			}
		}

		ZDecimal SimulatedUtilizationWithJobToBeAllocatedCN
		{
			get
			{
				if (RCA_AllocatedUQ.EqualsIgnoringCase(Constants.AllocationQuantityUnits.Containers))
				{
					return (ParentContract.ViewRatingContractSummary?.RCV_ContainerCount ?? 0) + QuantityProvider?.GetTotalCNQuantityNotAllocatedToContract(ParentContract) ?? 0;
				}
				else
				{
					return Convert.ToDecimal(ParentContract.ViewRatingContractSummary?.RCV_ContainerCount ?? 0);
				}
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new AllocationRouteForUtilizationSimulationFetchStrategy(this);
		}
	}
}
