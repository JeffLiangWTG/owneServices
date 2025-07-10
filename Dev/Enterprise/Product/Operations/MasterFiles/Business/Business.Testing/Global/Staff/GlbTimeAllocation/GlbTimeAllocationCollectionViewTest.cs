using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbTimeAllocationCollectionView))]
	sealed class GlbTimeAllocationCollectionViewTest : BusinessObjectCollectionViewTestCase<GlbTimeAllocationCollectionView>
	{
		public void TestIsThisPartOfTheCollection()
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

		protected override GlbTimeAllocationCollectionView GetCollectionToTest()
		{
			GlbTimeAllocationCollection coll = new GlbTimeAllocationCollection(GlbStaff.CurrentUser);
			GlbTimeAllocationFilter filter = new GlbTimeAllocationFilter(coll);
			filter.StartTime = ZDateTime.Empty;
			filter.EndTime = ZDateTime.Empty;
			return new GlbTimeAllocationCollectionView(filter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<GlbTimeAllocation>();
		}

		#endregion
	}
}
