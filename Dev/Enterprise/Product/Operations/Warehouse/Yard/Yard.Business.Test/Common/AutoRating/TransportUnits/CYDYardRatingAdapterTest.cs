using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	public class CYDYardRatingAdapterTest : TestCaseWithFactory
	{
		public void TestHasIAutoRatingWarehouseInfoMembers()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];
			var rateEntryParams = new CYDRateEntryParameters();
			var localClient = SetupClientForRating("AAA", rateEntryParams);
			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(yardUnit, unloadTime: ZDateTimeOffset.Today, location);
			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			Helper.AddReleaseAdviceLine(releaseAdvice, yardUnit);
			Helper.LoadYardUnit(yardUnit, ZDateTimeOffset.Today);

			Factory.Save();

			var interactor = new RatingAdaptersProviderTest.TestUIInteractor();
			var adapters = receiveAdvice.AdaptersProvider.GetAdapters(interactor, new AutoRateOptions(autoRateCost: true, billingType: BillingType.Invoicing));
			AssertEquals(1, adapters.Count);
			Assert(adapters[0] is IAutoRatingWarehouseInfo);
			AssertEquals(receiveAdvice.Yard.PK, (adapters[0] as IAutoRatingWarehouseInfo).WarehousePK);
			AssertEquals(null, (adapters[0] as IAutoRatingWarehouseInfo).WarehouseFallbackConsignorForFilterOnly);

			adapters = releaseAdvice.AdaptersProvider.GetAdapters(interactor, new AutoRateOptions(autoRateCost: true, billingType: BillingType.Invoicing));
			AssertEquals(1, adapters.Count);
			Assert(adapters[0] is IAutoRatingWarehouseInfo);
			AssertEquals(releaseAdvice.Yard.PK, (adapters[0] as IAutoRatingWarehouseInfo).WarehousePK);
			AssertEquals(null, (adapters[0] as IAutoRatingWarehouseInfo).WarehouseFallbackConsignorForFilterOnly);
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
