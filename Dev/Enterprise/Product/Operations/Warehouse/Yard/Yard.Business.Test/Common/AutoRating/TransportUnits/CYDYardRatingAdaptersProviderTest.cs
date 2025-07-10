using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	public class CYDYardRatingAdaptersProviderTest : TestCaseWithFactory
	{
		public void TestGetAdapters_ReceiveAdvice()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters();
			var localClient = SetupClientForRating("AAA", rateEntryParams);
			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			var unloadedYardUnit2 = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000039");
			Helper.UnloadYardUnit(unloadedYardUnit2, unloadTime: ZDateTimeOffset.Today, location);

			Factory.Save();

			var interactor = new RatingAdaptersProviderTest.TestUIInteractor();

			AssertEquals(2, receiveAdvice.AdaptersProvider.GetAdapters(interactor, new AutoRateOptions(autoRateCost: true, billingType: BillingType.Invoicing)).Count);
		}

		public void TestGetAdapters_ReleaseAdvice()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters();
			var localClient = SetupClientForRating("AAA", rateEntryParams);
			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var loadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			var waitingForLoadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			var loadedYardUnit2 = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000039");
			Helper.UnloadYardUnit(loadedYardUnit, unloadTime: ZDateTimeOffset.Today.AddDays(-1), location);
			Helper.UnloadYardUnit(waitingForLoadingYardUnit, unloadTime: ZDateTimeOffset.Today, location);
			Helper.UnloadYardUnit(loadedYardUnit2, unloadTime: ZDateTimeOffset.Today.AddDays(-1), location);

			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit);
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit2);
			Helper.LoadYardUnit(loadedYardUnit, ZDateTimeOffset.Today);
			Helper.LoadYardUnit(loadedYardUnit2, loadTime: ZDateTimeOffset.Today);

			Factory.Save();

			var interactor = new RatingAdaptersProviderTest.TestUIInteractor();

			AssertEquals(2, releaseAdvice.AdaptersProvider.GetAdapters(interactor, new AutoRateOptions(autoRateCost: true, billingType: BillingType.Invoicing)).Count);
		}

		#region Implementation

		CYDYardTestHelper Helper
		{
			get { return helper ?? (helper = new CYDYardTestHelper(Factory)); }
		}

		CYDYardTestHelper helper;

		OrgHeader SetupClientForRating(string clientCode, CYDRateEntryParameters rateEntryParams)
		{
			var client = Helper.CreateClient(clientCode);
			client.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(client);
			var rateEntry = Helper.CreateRateEntry(clientRate, rateEntryParams);

			return client;
		}

		#endregion
	}
}
