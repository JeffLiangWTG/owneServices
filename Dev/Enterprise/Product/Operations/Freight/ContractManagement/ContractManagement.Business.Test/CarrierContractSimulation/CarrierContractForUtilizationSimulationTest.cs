using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(CarrierContractForUtilizationSimulation))]
	internal class CarrierContractForUtilizationSimulationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCarrierContractCapacityWithVariance()
		{
			var contract = Factory.New<CarrierContractForUtilizationSimulation>();
			var viewQuantity = Factory.New<ViewRatingContractQuantity>();
			viewQuantity.RCQ_RCT_RatingContract = contract.PK;
			viewQuantity.RCQ_CapacityWithVarianceTU = 100;
			viewQuantity.RCQ_CapacityWithVarianceCN = 10;

			AssertEquals("TU Capacity With Variance", 100m, contract.CarrierContractCapacityWithVariance.TEUValue);
			AssertEquals("CN Capacity With Variance", 10m, contract.CarrierContractCapacityWithVariance.ContainerValue);
		}

		public void TestCurrentContractUtilization()
		{
			var contract = Factory.New<CarrierContractForUtilizationSimulation>();
			var viewSummary = Factory.New<ViewRatingContractSummary>();
			viewSummary.RCV_RCT_RatingContract = contract.PK;
			viewSummary.RCV_TEUCount = 10;
			viewSummary.RCV_ContainerCount = 100;

			AssertEquals("TU Current Utilization", 10m, contract.CurrentContractUtilisation.TEUValue);
			AssertEquals("CN Current Utilization", 100m, contract.CurrentContractUtilisation.ContainerValue);
		}

		public void TestCurrentContractOutstandingCommitted()
		{
			var contract = Factory.New<CarrierContractForUtilizationSimulation>();

			var viewQuantity = Factory.New<ViewRatingContractQuantity>();
			viewQuantity.RCQ_RCT_RatingContract = contract.PK;
			viewQuantity.RCQ_AllocatedQuantityTU = 500;
			viewQuantity.RCQ_AllocatedQuantityCN = 300;

			var viewSummary = Factory.New<ViewRatingContractSummary>();
			viewSummary.RCV_RCT_RatingContract = contract.PK;
			viewSummary.RCV_TEUCount = 150;
			viewSummary.RCV_ContainerCount = 50;

			AssertEquals("TU Outstanding Committed", 350m, contract.CurrentContractOutstandingCommitted.TEUValue);
			AssertEquals("CN Outstanding Committed", 250m, contract.CurrentContractOutstandingCommitted.ContainerValue);
		}

		public void TestCurrentContractOutstandingWithVariance()
		{
			var contract = Factory.New<CarrierContractForUtilizationSimulation>();

			var viewQuantity = Factory.New<ViewRatingContractQuantity>();
			viewQuantity.RCQ_RCT_RatingContract = contract.PK;
			viewQuantity.RCQ_CapacityWithVarianceTU = 500;
			viewQuantity.RCQ_CapacityWithVarianceCN = 300;

			var viewSummary = Factory.New<ViewRatingContractSummary>();
			viewSummary.RCV_RCT_RatingContract = contract.PK;
			viewSummary.RCV_TEUCount = 150;
			viewSummary.RCV_ContainerCount = 50;

			AssertEquals("TU Outstanding With Variance", 350m, contract.CurrentContractOutstandingWithVariance.TEUValue);
			AssertEquals("CN Outstanding With Variance", 250m, contract.CurrentContractOutstandingWithVariance.ContainerValue);
		}

		#region Active Filter

		public void TestActiveFilter()
		{
			var activeContract = Factory.New<CarrierContractForUtilizationSimulation>();
			activeContract.RCT_ContractNumber = "ACTIVECONTRACT";

			var inactiveContract = Factory.New<CarrierContractForUtilizationSimulation>();
			inactiveContract.RCT_ContractNumber = "INACTIVECONTRACT";
			inactiveContract.RCT_IsActive = false;

			CarrierContractForUtilizationSimulation[] results;
			var filter = new ZQuery(RatingContractSchema.PK, new ZGuid[] { activeContract.PK, inactiveContract.PK });

			results = Factory.Load<CarrierContractForUtilizationSimulation>(filter);
			AssertContainsExactElementsInAnyOrder(
				new string[] { "ACTIVECONTRACT" },
				Array.ConvertAll(results, (v) => v.RCT_ContractNumber.ToString()));

			filter.IgnoreActiveFilter = true;
			results = Factory.Load<CarrierContractForUtilizationSimulation>(filter);
			AssertContainsExactElementsInAnyOrder(
				new string[] { "ACTIVECONTRACT", "INACTIVECONTRACT" },
				Array.ConvertAll(results, (v) => v.RCT_ContractNumber.ToString()));
		}

		#endregion

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
