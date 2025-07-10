using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccStaffCommissionRuleCollection))]
	sealed class AccStaffCommissionRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<AccStaffCommissionRuleCollection>
	{
		protected override AccStaffCommissionRuleCollection GetCollectionToTest()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			return new AccStaffCommissionRuleCollection(staff);
		}
	}
}
