using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(CarrierContractCollection))]
	public class CarrierContractCollectionTest : ActiveBusinessObjectCollectionTestCase<CarrierContractCollection>
	{
		protected override CarrierContractCollection GetCollectionToTest()
		{
			return new CarrierContractCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			return carrierContract;
		}
	}
}
