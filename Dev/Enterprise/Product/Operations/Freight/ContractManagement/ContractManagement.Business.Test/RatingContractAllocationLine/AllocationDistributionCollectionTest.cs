using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Test
{
	[TestedType(typeof(AllocationDistributionCollection))]
	public class AllocationDistributionCollectionTest : ActiveBusinessObjectCollectionTestCase<AllocationDistributionCollection>
	{
		protected override AllocationDistributionCollection GetCollectionToTest()
		{
			return new AllocationDistributionCollection(ParentAllocationLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<RatingContractAllocationLine>();
			result.RCA_RCA_ParentAllocationRoute = ParentAllocationLine.PK;

			return result;
		}

		RatingContractAllocationLine ParentAllocationLine => parentAllocationLine ??= Factory.NewWithValidTestData<RatingContractAllocationLine>();
		RatingContractAllocationLine parentAllocationLine;
	}
}
