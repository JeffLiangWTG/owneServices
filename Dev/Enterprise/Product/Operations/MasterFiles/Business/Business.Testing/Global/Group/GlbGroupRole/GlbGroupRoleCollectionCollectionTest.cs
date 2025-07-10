using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupRoleCollection))]
	sealed class GlbGroupRoleCollectionCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbGroupRoleCollection>
	{
		protected override GlbGroupRoleCollection GetCollectionToTest()
		{
			return new GlbGroupRoleCollection(Factory.NewWithValidTestData<GlbGroup>());
		}
	}
}
