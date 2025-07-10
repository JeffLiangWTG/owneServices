using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(RatingContractContainerDetentionCollection))]
	public class RatingContractContainerDetentionCollectionTest : ActiveBusinessObjectCollectionTestCase<RatingContractContainerDetentionCollection>
	{
		protected override RatingContractContainerDetentionCollection GetCollectionToTest()
		{
			var contract = Factory.New<RatingContract>();
			return new RatingContractContainerDetentionCollection(contract);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<RatingContractContainerDetention>();
		}
	}
}
