using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ClickStatDataCollection))]
	sealed class ClickStatDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ClickStatDataCollection>
	{
		public void TestSortClickRatePercentage()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var collection = new ClickStatDataCollection(campaign);

			var data1 = collection.AddNew();
			data1.ClicksRate = 0.02;
			var data2 = collection.AddNew();
			data2.ClicksRate = 0.06;
			var data3 = collection.AddNew();
			data3.ClicksRate = 0.003;

			collection.Sort("ClicksRatePercentage", System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("< 0.01%", collection[0].ClicksRatePercentage);
			AssertEquals("0.02%", collection[1].ClicksRatePercentage);
			AssertEquals("0.06%", collection[2].ClicksRatePercentage);

			collection.Sort("ClicksRatePercentage", System.ComponentModel.ListSortDirection.Descending);
			AssertEquals("0.06%", collection[0].ClicksRatePercentage);
			AssertEquals("0.02%", collection[1].ClicksRatePercentage);
			AssertEquals("< 0.01%", collection[2].ClicksRatePercentage);
		}

		#region Implementation

		protected override ClickStatDataCollection GetCollectionToTest()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return new ClickStatDataCollection(campaign);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return new ClickStatData(campaign);
		}

		#endregion
	}
}
