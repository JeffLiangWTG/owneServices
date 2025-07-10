using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class CommodityDefaultsManagerTest : TestCaseWithFactory
	{
		public void TestNewCommodity_DefaultFrom_Shipment()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ManifestQty = 999;
			shipment.B0_ManifestUQ = "MU";
			shipment.B0_Weight = 123.2m;
			shipment.B0_WeightUQ = "WU";
			shipment.B0_DescriptionOfCargo = "Test B0_DescriptionOfCargo";
			shipment.B0_RN_NKCountryOfExport = "AU";
			for (var i = 0; i < 5; i++)
			{
				var newCommodity = shipment.Commodities.AddNew();
				if (shipment.Commodities.Count == 1)
				{
					var commodity = shipment.Commodities[0];
					AssertEquals("first commodity.BY_PieceCount default to Shipment.B0_ManifestQty", shipment.B0_ManifestQty, commodity.BY_PieceCount);
					AssertEquals("first commodity.BY_GrossWeight default to Shipment.B0_Weight", shipment.B0_Weight, commodity.BY_GrossWeight);
				}

				AssertEquals("commodity.BY_ManifestUnitCode default to Shipment.B0_ManifestUQ", shipment.B0_ManifestUQ, newCommodity.BY_ManifestUnitCode);
				AssertEquals("commodity.BY_GrossWeightUnit default to Shipment.B0_WeightUQ", shipment.B0_WeightUQ, newCommodity.BY_GrossWeightUnit);
				AssertEquals("commodity.BY_Description default to Shipment.B0_DescriptionOfCargo", shipment.B0_DescriptionOfCargo, newCommodity.BY_Description);
				AssertEquals("commodity.BY_RN_NKCountryOfOrigin default to Shipment.B0_RN_NKCountryOfExport", shipment.B0_RN_NKCountryOfExport, newCommodity.BY_RN_NKCountryOfOrigin);
				if (shipment.Trip.Conveyance.BJ_RQ_Equipment.IsValid)
				{
					AssertEquals("commodity.BY_BJ_Equipment default to Shipment.Trip.Conveyance.PK", shipment.Trip.Conveyance.PK, newCommodity.BY_BJ_Equipment);
				}
			}
		}

		public void TestSetDefaults_WhenDescriptionNotEmpty_ShouldNotPopulateFromShipment()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_DescriptionOfCargo = "Some Shipment";
			var commodity = Factory.New<Commodity>();
			commodity.BY_ParentID = shipment.PK;
			commodity.BY_ParentTableCode = shipment.TablePrefix;
			commodity.Shipment = shipment;
			commodity.BY_Description = "Some Commodity";
			new CommodityDefaultsManager(commodity).SetDefaults();
			AssertEquals("Should not populate from shipment description of cargo", "Some Commodity", commodity.BY_Description);
		}

		public void TestSetDefaults()
		{
			var equipment1 = Factory.New<RefEquipment>();
			var equipment2 = Factory.New<RefEquipment>();
			var trip = Factory.New<Trip>();
			var conveyance = trip.Conveyance;
			var shipment = trip.Shipments.AddNew();
			var commodity = shipment.Commodities.AddNew();
			conveyance.BJ_RQ_Equipment = equipment1.PK;
			commodity = shipment.Commodities.AddNew();
			AssertEquals("The commodity should be linked to the trip's first equipment", conveyance.PK, commodity.BY_BJ_Equipment);
			var equipment = trip.Equipment.AddNew();
			equipment.BJ_RQ_Equipment = equipment2.PK;
			commodity = shipment.Commodities.AddNew();
			AssertEquals("The commodity should be linked to the trip's first equipment", conveyance.PK, commodity.BY_BJ_Equipment);
		}
	}
}
