using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAllowedBranchDepartmentComboCollection))]
	sealed class AccAllowedBranchDepartmentComboCollectionTest : ActiveBusinessObjectCollectionTestCase<AccAllowedBranchDepartmentComboCollection>
	{
		protected override AccAllowedBranchDepartmentComboCollection GetCollectionToTest()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			return new AccAllowedBranchDepartmentComboCollection(branch);
		}
	}
}
