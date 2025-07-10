using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbTimeAllocationFilter))]
	sealed class GlbTimeAllocationFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			GlbTimeAllocationCollection coll = new GlbTimeAllocationCollection(GlbStaff.CurrentUser);
			GlbTimeAllocationFilter filter = new GlbTimeAllocationFilter(coll);
			AssertEquals(ZDateTime.Today.AddDays(-7), filter.StartTime);
			AssertEquals(ZDateTime.Today.AddDays(21), filter.EndTime);
		}

		public void TestFilter()
		{
			GlbTimeAllocationCollection coll = new GlbTimeAllocationCollection(GlbStaff.CurrentUser);

			GlbTimeAllocation time1 = coll.AddNew();
			time1.GA_StartTime = new ZDateTime(2006, 10, 5, 9, 0, 0);
			time1.GA_EndTime = new ZDateTime(2006, 10, 5, 11, 0, 0);

			GlbTimeAllocation time2 = coll.AddNew();
			time2.GA_StartTime = new ZDateTime(2006, 10, 10, 13, 0, 0);
			time2.GA_EndTime = new ZDateTime(2006, 10, 10, 15, 0, 0);

			GlbTimeAllocationFilter filter = new GlbTimeAllocationFilter(coll);
			filter.StartTime = ZDateTime.Empty;
			filter.EndTime = ZDateTime.Empty;

			filter.AllocationsView.CollectionToFilter.Load();
			AssertEquals(2, filter.AllocationsView.Count);

			filter.StartTime = new ZDateTime(2006, 10, 4);
			AssertEquals(2, filter.AllocationsView.Count);

			filter.EndTime = new ZDateTime(2006, 10, 8);
			AssertEquals(1, filter.AllocationsView.Count);

			filter.StartTime = new ZDateTime(2006, 10, 8);
			AssertEquals(0, filter.AllocationsView.Count);

			filter.EndTime = new ZDateTime(2006, 10, 20);
			AssertEquals(1, filter.AllocationsView.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			GlbTimeAllocationCollection coll = new GlbTimeAllocationCollection(GlbStaff.CurrentUser);
			return new GlbTimeAllocationFilter(coll);
		}

		#endregion
	}
}
