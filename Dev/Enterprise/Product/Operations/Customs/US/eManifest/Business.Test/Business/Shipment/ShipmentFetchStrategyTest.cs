using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchHints()
		{
			var shipment1 = Factory.NewWithValidTestData<Shipment>();
			_ = shipment1.Consignee;
			_ = shipment1.Shipper;

			var commodity1 = shipment1.Commodities.AddNew();
			commodity1.C4Codes.AddNew();
			commodity1.HarmonizedNumbers.AddNew();
			commodity1.VehicleIdentificationNumbers.AddNew();
			commodity1.UNDGs.AddNew();

			var shipment2 = Factory.NewWithValidTestData<Shipment>();
			_ = shipment2.Consignee;
			_ = shipment2.Shipper;

			var commodity2 = shipment2.Commodities.AddNew();
			commodity2.C4Codes.AddNew();
			commodity2.HarmonizedNumbers.AddNew();
			commodity2.VehicleIdentificationNumbers.AddNew();
			commodity2.UNDGs.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ CusInBondBillSchema.Constants.TableName, 1 },
				{ CusInBondCargoDescSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var loadedShipments = newFactory.Load<Shipment>(new ZQuery());
				foreach (var shipment in loadedShipments)
				{
					_ = shipment.Commodities[0];
					_ = shipment.Consignee;
					_ = shipment.Shipper;
				}
			}
		}

		public void TestFetchHints_LoadFirstCommodityShouldAddFetchHintsOfChildrenTableForSiblingCommodities()
		{
			var shipment1 = Factory.NewWithValidTestData<Shipment>();
			_ = shipment1.Consignee;
			_ = shipment1.Shipper;

			var commodity1 = shipment1.Commodities.AddNew();
			commodity1.C4Codes.AddNew();
			commodity1.HarmonizedNumbers.AddNew();
			commodity1.VehicleIdentificationNumbers.AddNew();
			commodity1.UNDGs.AddNew();

			var shipment2 = Factory.NewWithValidTestData<Shipment>();
			_ = shipment2.Consignee;
			_ = shipment2.Shipper;

			var commodity2 = shipment2.Commodities.AddNew();
			commodity2.C4Codes.AddNew();
			commodity2.HarmonizedNumbers.AddNew();
			commodity2.VehicleIdentificationNumbers.AddNew();
			commodity2.UNDGs.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ CusInBondBillSchema.Constants.TableName, 1 },
				{ CusInBondCargoDescSchema.Constants.TableName, 1 },
				{ CusCodeDataSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var loadedShipments = newFactory.Load<Shipment>(new ZQuery());
				foreach (var shipment in loadedShipments)
				{
					var commodity = shipment.Commodities[0];
					_ = commodity.UNDGs;
					_ = commodity.C4Codes;
					_ = commodity.HarmonizedNumbers;
					_ = commodity.VehicleIdentificationNumbers;
				}
			}
		}
	}
}
