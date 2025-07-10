using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbResourceCollection))]
	sealed class GlbResourceCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbResourceCollection>
	{
		public void TestRelationshipFilter()
		{
			GlbStaff resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_IsResource = true;

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			GlbResourceCollection coll = new GlbResourceCollection(Factory);
			AssertCollectionNotContains(staff, coll);
			AssertCollectionContains(resource, coll);
		}

		protected override GlbResourceCollection GetCollectionToTest()
		{
			return new GlbResourceCollection(Factory);
		}
	}
}
