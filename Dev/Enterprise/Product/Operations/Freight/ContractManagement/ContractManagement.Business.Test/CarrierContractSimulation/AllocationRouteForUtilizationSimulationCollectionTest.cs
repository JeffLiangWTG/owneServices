using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(AllocationRouteForUtilizationSimulationCollection))]
	public class AllocationRouteForUtilizationSimulationCollectionTest : ActiveBusinessObjectCollectionTestCase<AllocationRouteForUtilizationSimulationCollection>
	{
		protected override AllocationRouteForUtilizationSimulationCollection GetCollectionToTest()
		{
			var contract = Factory.New<RatingContract>();
			return new AllocationRouteForUtilizationSimulationCollection(contract, new Mock<IContractSimulationFormConfiguration>().Object);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<AllocationRouteForUtilizationSimulation>();
		}
	}
}
