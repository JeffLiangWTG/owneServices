using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffForGroupCollection))]
	sealed class GlbStaffForGroupCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbStaffForGroupCollection>
	{
		protected override GlbStaffForGroupCollection GetCollectionToTest()
		{
			return new GlbStaffForGroupCollection(Factory);
		}

		public void TestStaffForGroupCollection()
		{
			GlbGroup group1 = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();

			GlbGroupLink groupStaff1 = Factory.New<GlbGroupLink>();
			groupStaff1.GK_GS = staff1.PK;
			groupStaff1.GK_GG = group1.PK;

			GlbGroupLink groupStaff2 = Factory.New<GlbGroupLink>();
			groupStaff2.GK_GS = staff2.PK;
			groupStaff2.GK_GG = group1.PK;

			Factory.Save();

			GlbStaffForGroupCollection testCollection = new GlbStaffForGroupCollection(Factory, group1.PK);

			AssertEquals("Count", 2, testCollection.Count);

			testCollection = new GlbStaffForGroupCollection(Factory, ZGuid.NewZGuid());

			AssertEquals("Count", 0, testCollection.Count);

			testCollection = new GlbStaffForGroupCollection(Factory, ZGuid.Empty);

			Assert("Count", testCollection.Count > 2);
		}
	}
}
