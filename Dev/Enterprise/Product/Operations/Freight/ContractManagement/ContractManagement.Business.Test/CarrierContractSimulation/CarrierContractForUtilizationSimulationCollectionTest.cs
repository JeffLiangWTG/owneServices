using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(CarrierContractForUtilizationSimulationCollection))]
	internal class CarrierContractForUtilizationSimulationCollectionTest : ActiveBusinessObjectCollectionTestCase<CarrierContractForUtilizationSimulationCollection>
	{
		protected override CarrierContractForUtilizationSimulationCollection GetCollectionToTest()
		{
			return new CarrierContractForUtilizationSimulationCollection(Factory, new Mock<IContractSimulationFormConfiguration>().Object);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var contract = Factory.NewWithValidTestData<CarrierContractForUtilizationSimulation>();
			contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			return contract;
		}
	}
}
