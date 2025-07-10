using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;

namespace Enterprise.ContractManagement.Business.Testing
{
	internal class RatingContractValidationHelperTest : TestCaseWithFactory
	{
		public void TestIsClientContractNumberExists()
		{
			var ratingContractValidationHelper = ObjectFactory.Get<IRatingContractValidationHelper>();

			AssertEquals(false, ratingContractValidationHelper.DoesClientContractNumberExist(Factory, "ANYTHING"));

			var ratingContract1 = Factory.NewWithValidTestData<RatingContract>();
			var ratingContract2 = Factory.NewWithValidTestData<RatingContract>();

			ratingContract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			AssertEquals(false, ratingContractValidationHelper.DoesClientContractNumberExist(Factory, ratingContract1.RCT_ContractNumber));

			ratingContract2.RCT_ContractType = Core.Constants.RatingContractTypes.Client;
			AssertEquals(true, ratingContractValidationHelper.DoesClientContractNumberExist(Factory, ratingContract2.RCT_ContractNumber));
		}
	}
}
