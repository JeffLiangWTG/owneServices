using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(AllocationRouteForUtilizationSimulation))]
	class AllocationRouteForUtilizationSimulationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSimulatedUtilizationWithConsolOrContainerToBeAllocated_TEU()
		{
			var allocationRoute = GetNewBusinessObject() as AllocationRouteForUtilizationSimulation;
			allocationRoute.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;

			AssertEquals("RCV_TEUCount: 10 + 30", 40m, allocationRoute.SimulatedUtilizationWithJobToBeAllocated.TEUValue);
			AssertEquals("RCV_ContainerCount: 5", 5m, allocationRoute.SimulatedUtilizationWithJobToBeAllocated.ContainerValue);
		}

		public void TestSimulatedUtilizationWithConsolOrContainerToBeAllocated_Container()
		{
			var allocationRoute = GetNewBusinessObject() as AllocationRouteForUtilizationSimulation;
			allocationRoute.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;

			AssertEquals("RCV_TEUCount: 10", 10m, allocationRoute.SimulatedUtilizationWithJobToBeAllocated.TEUValue);
			AssertEquals("RCT_ContainerCount: 5 + 10", 15m, allocationRoute.SimulatedUtilizationWithJobToBeAllocated.ContainerValue);
		}

		public void TestSimulatedOutstandingCommitedWithConsolOrContainerToBeAllocated_TEU()
		{
			var allocationRoute = GetNewBusinessObject() as AllocationRouteForUtilizationSimulation;
			allocationRoute.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;

			AssertEquals("RCQ_AllocatedQuantityTU: 60 - SimulatedUtilization.TEU: 40", 20m, allocationRoute.SimulatedOutstandingCommitedWithJobToBeAllocated.TEUValue);
			AssertEquals("RCQ_AllocatedQuantityCN: 70 - SimulatedUtilization.CN: 5", 65m, allocationRoute.SimulatedOutstandingCommitedWithJobToBeAllocated.ContainerValue);
		}

		public void TestSimulatedOutstandingCommitedWithConsolOrContainerToBeAllocated_Container()
		{
			var allocationRoute = GetNewBusinessObject() as AllocationRouteForUtilizationSimulation;
			allocationRoute.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;

			AssertEquals("RCQ_AllocatedQuantityTU: 60 - SimulatedUtilization.TEU: 10", 50m, allocationRoute.SimulatedOutstandingCommitedWithJobToBeAllocated.TEUValue);
			AssertEquals("RCQ_AllocatedQUantityCN: 70 - SimulatedUtilization.CN: 15", 55m, allocationRoute.SimulatedOutstandingCommitedWithJobToBeAllocated.ContainerValue);
		}

		public void TestSimulatedOutstandingWithVarianceWithConsolOrContainerToBeAllocated_TEU()
		{
			var allocationRoute = GetNewBusinessObject() as AllocationRouteForUtilizationSimulation;
			allocationRoute.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;

			AssertEquals("RCQ_CapacityWithVarianceTU: 130 - SimulatedUtilization.TEU: 40", 90m, allocationRoute.SimulatedOutstandingWithVarianceWithJobToBeAllocated.TEUValue);
			AssertEquals("RCQ_CapacityWithVarianceCN: 160 - SimulatedUtilization.CN: 5", 155m, allocationRoute.SimulatedOutstandingWithVarianceWithJobToBeAllocated.ContainerValue);
		}

		public void TestSimulatedOutstandingWithVarianceWithConsolOrContainerToBeAllocated_Container()
		{
			var allocationRoute = GetNewBusinessObject() as AllocationRouteForUtilizationSimulation;
			allocationRoute.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;

			AssertEquals("RCQ_CapacityWithVariacenTU: 130 - SimulatedUtilization.TEU: 10", 120m, allocationRoute.SimulatedOutstandingWithVarianceWithJobToBeAllocated.TEUValue);
			AssertEquals("RCQ_CapacityWithVarianceCN: 160 - SimulatedUtilization.CN: 15", 145m, allocationRoute.SimulatedOutstandingWithVarianceWithJobToBeAllocated.ContainerValue);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var allocationRouteForUtilizationSimulation = Factory.NewWithValidTestData<AllocationRouteForUtilizationSimulation>();
			var parentContract = allocationRouteForUtilizationSimulation.Contract;

			var quantityProviderMock = new Mock<IRatingContractSimulationQuantityProvider>();
			quantityProviderMock
				.Setup(quantityProvider => quantityProvider.GetTotalTEUQuantityNotAllocatedToContract(It.IsAny<CarrierContractForUtilizationSimulation>()))
				.Returns(() => 30);

			quantityProviderMock
				.Setup(quantityProvider => quantityProvider.GetTotalCNQuantityNotAllocatedToContract(It.IsAny<CarrierContractForUtilizationSimulation>()))
				.Returns(() => 10);

			allocationRouteForUtilizationSimulation.QuantityProvider = quantityProviderMock.Object;

			Factory.Save();

			var viewContractSummary = Factory.NewWithValidTestData<ViewRatingContractSummary>();
			viewContractSummary.RCV_TEUCount = 10m;
			viewContractSummary.RCV_ContainerCount = 5;
			viewContractSummary.RCV_RCT_RatingContract = parentContract.PK;

			var viewContractQuantity = Factory.NewWithValidTestData<ViewRatingContractQuantity>();
			viewContractQuantity.RCQ_AllocatedQuantityTU = 60;
			viewContractQuantity.RCQ_AllocatedQuantityCN = 70;
			viewContractQuantity.RCQ_CapacityWithVarianceTU = 130m;
			viewContractQuantity.RCQ_CapacityWithVarianceCN = 160m;
			viewContractQuantity.RCQ_RCT_RatingContract = parentContract.PK;

			return allocationRouteForUtilizationSimulation;
		}

		protected override void SetUp()
		{
			base.SetUp();

			refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_TEU = 3;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		RefContainer refContainer;
	}
}
