using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffCapabilityMembersCollection))]
	class GlbStaffCapabilityMembersCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbStaffCapabilityMembersCollection(Factory.NewWithValidTestData<GlbCapability>());
		}

		[TestDate(2013, 7, 19)]
		public void TestMembers()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var collection = new GlbStaffCapabilityMembersCollection(capability);
			AssertEquals(0, collection.Count);

			collection.Add(staff1);
			AssertEquals(1, staff1.Capabilities.Count);
			AssertEquals(capability, staff1.Capabilities[0]);

			collection.Add(staff2);
			AssertEquals(1, staff2.Capabilities.Count);
			AssertEquals(capability, staff2.Capabilities[0]);

			AssertEquals(2, collection.Count);

			var pivots = Factory.Load<GlbResourceCapabilityPivot>(new ZQuery(GlbResourceCapabilityPivotSchema.G5_GS_Resource, new[] { staff1.PK, staff2.PK }));
			AssertEquals(2, pivots.Length);
			AssertEquals(ZDateTime.Now, pivots[0].G5_DateExperienceGained);
			AssertEquals(ZDateTime.Now, pivots[1].G5_DateExperienceGained);

			AssertEquals((ZByte)1, pivots[0].G5_SkillLevel);
			AssertEquals((ZByte)1, pivots[1].G5_SkillLevel);
		}

		public void TestAllowNew()
		{
			AssertEquals("Capability members should not be added by clicking the grid, but rather by using the Add button", false, Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("Capability members still should be able to be deleted by the Delete key, so we need to allow remove", true, Collection.AllowRemove);
		}
	}
}
