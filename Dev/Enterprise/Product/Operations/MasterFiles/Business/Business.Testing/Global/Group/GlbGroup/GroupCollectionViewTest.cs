using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GroupCollectionView))]
	sealed class GroupCollectionViewTest : BusinessObjectCollectionViewTestCase<GroupCollectionView>
	{
		public void TestIsThisPartOfTheCollection()
		{
			GlbGroup groupActive = CollectionToFilter.AddNew();

			GlbGroup groupInactive = CollectionToFilter.AddNew();
			groupInactive.GG_IsActive = false;

			GlbGroup groupSales = CollectionToFilter.AddNew();
			groupSales.GG_IsSales = true;

			var view = new GroupCollectionView(CollectionToFilter, false, false);
			AssertContainsExactElementsInAnyOrder(new GlbGroup[] { groupActive, groupInactive }, view);

			view = new GroupCollectionView(CollectionToFilter, false, true);
			AssertContainsExactElementsInAnyOrder(new GlbGroup[] { groupActive }, view);

			view = new GroupCollectionView(CollectionToFilter, true, false);
			AssertContainsExactElementsInAnyOrder(new GlbGroup[] { groupSales }, view);
		}

		#region Implementation

		protected override GroupCollectionView GetCollectionToTest()
		{
			return new GroupCollectionView(CollectionToFilter, false, true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<GlbGroup>();
		}

		GlbGroupManyToManyCollection CollectionToFilter
		{
			get
			{
				if (collectionToFilter == null)
				{
					collectionToFilter = new GlbGroupManyToManyCollection(Staff);
				}
				return collectionToFilter;
			}
		}

		GlbStaff Staff
		{
			get { return staff ?? (staff = Factory.NewWithValidTestData<GlbStaff>()); }
		}

		GlbGroupManyToManyCollection collectionToFilter;
		GlbStaff staff;

		#endregion
	}
}
