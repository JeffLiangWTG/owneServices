
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItemScheduleItemsCollection))]
	sealed class GlbCompanyCampaignItemScheduleItemsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GlbCompanyCampaignItemScheduleItemsCollection>
	{
		public void TestSortUtcOffset()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var collection = new GlbCompanyCampaignItemScheduleItemsCollection(campaign);

			var data1 = collection.AddNew();
			data1.UtcOffset = new ZShort(540);
			var data2 = collection.AddNew();
			data2.UtcOffset = ZShort.Zero;
			var data3 = collection.AddNew();
			data3.UtcOffset = new ZShort(-120);

			collection.Sort("UtcOffset", System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals((ZShort)(-120), collection[0].UtcOffset);
			AssertEquals(ZShort.Zero, collection[1].UtcOffset);
			AssertEquals((ZShort)540, collection[2].UtcOffset);

			collection.Sort("UtcOffset", System.ComponentModel.ListSortDirection.Descending);
			AssertEquals((ZShort)540, collection[0].UtcOffset);
			AssertEquals(ZShort.Zero, collection[1].UtcOffset);
			AssertEquals((ZShort)(-120), collection[2].UtcOffset);
		}

		#region Implementation

		protected override GlbCompanyCampaignItemScheduleItemsCollection GetCollectionToTest()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return new GlbCompanyCampaignItemScheduleItemsCollection(campaign);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ScheduleCampaignItems();
		}

		#endregion
	}
}
