using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class CommodityLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var trip = Factory.New<Trip>();
			var conveyance = trip.Conveyance;
			var equipment1 = trip.Equipment.AddNew();
			var equipment2 = trip.Equipment.AddNew();
			var shipment = trip.Shipments.AddNew();
			var commodity = shipment.Commodities.AddNew();
			var lookups = commodity.Lookups;
			var equipment = lookups.Equipment;
			AssertEquals("Equipment.Count", 3, equipment.Count);
			Assert("Should contain conveyance", equipment.Contains(conveyance));
			Assert("Should contain equipment 1 added to the trip", equipment.Contains(equipment1));
			Assert("Should contain equipment 2 added to the trip", equipment.Contains(equipment2));
			Assert("WeightUnits", lookups.WeightUnits.ContainsCode(Core.Constants.Weight.Kilograms));
			AssertEquals("QuantityUnits", typeof(PackageTypes), lookups.QuantityUnits.GetType());
			AssertEquals("Countries", typeof(RefCountryCollection), lookups.Countries.GetType());
			AssertEquals("Currencies", typeof(RefCurrencyCollection), lookups.Currencies.GetType());
			AssertEquals("UNDGSubs", typeof(UNDGSubstanceCollection), lookups.UNDGSubs.GetType());
			AssertEquals("Contacts", typeof(OrgContactCollection), lookups.Contacts.GetType());
		}
	}
}
