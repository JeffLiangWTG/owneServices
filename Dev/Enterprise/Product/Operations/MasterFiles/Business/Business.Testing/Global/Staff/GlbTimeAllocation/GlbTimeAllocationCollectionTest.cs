using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbTimeAllocationCollection))]
	sealed class GlbTimeAllocationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRecordTypeFilter()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			GlbStaffHoliday hol = Factory.New<GlbStaffHoliday>();
			hol.GA_GS = staff.PK;
			GlbTimeAllocation time = Factory.New<GlbTimeAllocation>();
			time.GA_GS = staff.PK;

			GlbTimeAllocationCollection coll = new GlbTimeAllocationCollection(staff);
			coll.Load();
			AssertEquals(1, coll.Count);
			AssertEquals(time, coll[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			return new GlbTimeAllocationCollection(staff);
		}
	}
}
