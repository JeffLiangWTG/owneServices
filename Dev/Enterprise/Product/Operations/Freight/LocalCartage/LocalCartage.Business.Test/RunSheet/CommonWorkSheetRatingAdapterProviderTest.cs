using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonWorkSheetRatingAdapterProviderTest : TestCaseWithFactory
	{
		public void TestGetAdapters()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var legs = Helper.CreateCartageLegs(cartage, 3);
			workSheet.CartageLegs.AddRange(legs);
			// CASE 1: Cost
			var adapterProvider1 = new CommonWorkSheetRatingAdapterProvider(workSheet);
			var costAdapters = adapterProvider1.GetAdapters(new RatingAdaptersProviderTest.TestUIInteractor(), AutoRateOptions.AutorateCosts);
			AssertEquals("Expecting adapters in total", 7, costAdapters.Count);
			AssertEquals("Expecting 1 runsheet adapter (for validation)", 1, costAdapters.OfType<CommonWorkSheetRatingAdapter>().Count());
			var cartageLegAdapters = costAdapters.OfType<CartageLegRatingAdapter>();
			AssertEquals("Expecting cartage leg adapters", 3, cartageLegAdapters.Count());
			AssertEquals("Expecting all cartage leg adapters to cross merge", true, cartageLegAdapters.All(adapter => adapter.MergeCharges == MergeChargeOptions.CrossAdapter));
			Assert(cartageLegAdapters.All(x => x.JobServices.Count == 0));
			var cartageMoveAdapters = costAdapters.OfType<CartageMoveRatingAdapter>();
			AssertEquals("Expecting cartage move adapters", 3, cartageMoveAdapters.Count());
			Assert(cartageMoveAdapters.All(x => x.IsServicesOnly));
			// CASE 2: Revenue
			var adapterProvider2 = new CommonWorkSheetRatingAdapterProvider(workSheet);
			AssertExceptionThrown<NotSupportedException>(() => adapterProvider2.GetAdapters(new RatingAdaptersProviderTest.TestUIInteractor(), AutoRateOptions.AutorateRevenue));
		}

		public void TestGetAdapters_MultipleLegsFromTheSameMove()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.BookedMovesCollection.AddNew();
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			var leg3 = move.CartageLegs.AddNew();
			workSheet.CartageLegs.AddRange(new[] { leg1, leg2, leg3 });
			var adapterProvider1 = new CommonWorkSheetRatingAdapterProvider(workSheet);
			var costAdapters = adapterProvider1.GetAdapters(new RatingAdaptersProviderTest.TestUIInteractor(), AutoRateOptions.AutorateCosts);
			AssertEquals("Expecting adapters in total", 5, costAdapters.Count);
			AssertEquals("Expecting 1 runsheet adapter (for validation)", 1, costAdapters.OfType<CommonWorkSheetRatingAdapter>().Count());
			var cartageLegAdapters = costAdapters.OfType<CartageLegRatingAdapter>();
			AssertEquals("Expecting cartage leg adapters", 3, cartageLegAdapters.Count());
			var cartageMoveAdapters = costAdapters.OfType<CartageMoveRatingAdapter>();
			AssertEquals("Expecting cartage move adapters", 1, cartageMoveAdapters.Count());
		}

		public void TestGetAdditionalJobs()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage1 = Factory.New<CommonCartage>();
			var cartage2 = Factory.New<CommonCartage>();
			workSheet.CartageLegs.AddRange(Helper.CreateCartageLegs(cartage1, 1));
			workSheet.CartageLegs.AddRange(Helper.CreateCartageLegs(cartage2, 2));
			var adapterProvider = new CommonWorkSheetRatingAdapterProvider(workSheet);
			AssertContainsExactElementsInAnyOrder(new[] { cartage1, cartage2 }, adapterProvider.GetAdditionalJobs());
		}

		readonly TestCommonWorkSheetHelper Helper;
		public CommonWorkSheetRatingAdapterProviderTest()
		{
			Helper = new TestCommonWorkSheetHelper(Factory);
		}
	}
}
