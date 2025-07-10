using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(CommodityCollection))]
	sealed class CommodityCollectionTest : ActiveBusinessObjectCollectionTestCase<CommodityCollection>
	{
		public void TestShipmentQtyAndWeightBalanceToNewCommodity()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ManifestQty = 19;
			shipment.B0_ManifestUQ = "MU";
			shipment.B0_Weight = 123.2m;
			shipment.B0_WeightUQ = "KG";
			shipment.B0_DescriptionOfCargo = "Test B0_DescriptionOfCargo";
			shipment.B0_RN_NKCountryOfExport = "AU";
			var commodity = shipment.Commodities[0];
			AssertEquals(commodity.BY_PieceCount, shipment.B0_ManifestQty);
			AssertEquals(commodity.BY_GrossWeight, shipment.B0_Weight);
			AssertShipmentQtyAndWeightEqualsToTotalCommodityValues(shipment);
			shipment.Commodities.AddNew();
			AssertShipmentQtyAndWeightEqualsToTotalCommodityValues(shipment);
			shipment.B0_ManifestQty += 10;
			shipment.B0_Weight += 10.1m;
			shipment.Commodities.AddNew();
			AssertShipmentQtyAndWeightEqualsToTotalCommodityValues(shipment);
		}

		public void TestCollection()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			var commodity = shipment.Commodities.AddNew();
			AssertCommodity(shipment, commodity);
			Factory.Save();
			shipment = new BusinessObjectFactory().Load<Shipment>(shipment.PK);
			AssertCommodity(shipment, shipment.Commodities[0]);
		}

		protected override CommodityCollection GetCollectionToTest() => new CommodityCollection(Factory.New<Trip>().Shipments.AddNew());

		static void AssertCommodity(Shipment shipment, Commodity commodity)
		{
			AssertEquals("Commodity shipment", shipment, commodity.Shipment);
			AssertEquals("XX_Relation1ID", shipment.PK, commodity.BY_ParentID);
			AssertEquals("XX_Relation1TableCode", shipment.TablePrefix, commodity.BY_ParentTableCode);
		}

		void AssertShipmentQtyAndWeightEqualsToTotalCommodityValues(Shipment shipment)
		{
			var totalBY_PieceCount = 0;
			var totalBY_GrossWeight = 0m;
			foreach (var item in shipment.Commodities)
			{
				totalBY_PieceCount += item.BY_PieceCount;
				totalBY_GrossWeight += item.BY_GrossWeight;
			}

			AssertEquals(totalBY_PieceCount, shipment.B0_ManifestQty);
			AssertEquals(totalBY_GrossWeight, shipment.B0_Weight);
		}
	}
}
