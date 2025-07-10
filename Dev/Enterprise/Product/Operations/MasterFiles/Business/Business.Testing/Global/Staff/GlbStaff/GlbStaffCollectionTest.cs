using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffCollection))]
	sealed class GlbStaffCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbStaffCollection>
	{
		public void TestRelationshipFilter()
		{
			GlbStaff resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_IsResource = true;

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			GlbStaff cantLoginStaff = Factory.NewWithValidTestData<GlbStaff>();
			cantLoginStaff.GS_CanLogin = false;

			Factory.Save();

			GlbStaffCollection coll = new GlbStaffCollection(Factory);
			AssertCollectionContains(staff, coll);
			AssertCollectionContains(cantLoginStaff, coll);
			AssertCollectionNotContains(resource, coll);
		}

		protected override GlbStaffCollection GetCollectionToTest()
		{
			return new GlbStaffCollection(Factory);
		}
	}
}
