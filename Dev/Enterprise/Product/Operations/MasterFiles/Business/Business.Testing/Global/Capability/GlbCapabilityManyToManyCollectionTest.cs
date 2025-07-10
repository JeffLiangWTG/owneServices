using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCapabilityManyToManyCollection))]
	sealed class GlbCapabilityManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbCapabilityManyToManyCollection(Factory.NewWithValidTestData<GlbStaff>());
		}

		[TestDate(2013, 7, 19)]
		public void TestAdd_ShouldSetResourcePivot()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var pivot = Factory.New<GlbResourceCapabilityPivot>();
			pivot.G5_GS_Resource = staff.PK;
			pivot.G5_G4_Capability = capability.PK;

			AssertNull(capability.ResourcePivot);

			var collection = new GlbCapabilityManyToManyCollection(staff);
			collection.Add(capability);

			AssertEquals(pivot, capability.ResourcePivot);
			AssertEquals(ZDateTime.Now, pivot.G5_DateExperienceGained);
			AssertEquals((byte)1, pivot.G5_SkillLevel);
		}
	}
}
