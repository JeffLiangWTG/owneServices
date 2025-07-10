using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(RatingContractAllocationLineCollection))]
	public class RatingContractAllocationLineCollectionTest : ActiveBusinessObjectCollectionTestCase<RatingContractAllocationLineCollection>
	{
		protected override RatingContractAllocationLineCollection GetCollectionToTest()
		{
			var contract = Factory.New<RatingContract>();
			return new RatingContractAllocationLineCollection(contract);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<RatingContractAllocationLine>();
		}
	}
}
