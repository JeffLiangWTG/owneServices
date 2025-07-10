using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class CommodityFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchHints()
		{
			var shipment1 = Factory.NewWithValidTestData<Shipment>();
			var commodity1 = shipment1.Commodities.AddNew();
			commodity1.C4Codes.AddNew();
			commodity1.HarmonizedNumbers.AddNew();
			commodity1.VehicleIdentificationNumbers.AddNew();
			commodity1.UNDGs.AddNew();

			var shipment2 = Factory.NewWithValidTestData<Shipment>();
			var commodity2 = shipment2.Commodities.AddNew();
			commodity2.C4Codes.AddNew();
			commodity2.HarmonizedNumbers.AddNew();
			commodity2.VehicleIdentificationNumbers.AddNew();
			commodity2.UNDGs.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ CusInBondCargoDescSchema.Constants.TableName, 1 },
				{ CusCodeDataSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var loadedCommodities = newFactory.Load<Commodity>(new ZQuery());
				foreach (var commodity in loadedCommodities)
				{
					_ = commodity.C4Codes;
					_ = commodity.HarmonizedNumbers;
					_ = commodity.VehicleIdentificationNumbers;
					_ = commodity.UNDGs;
				}
			}
		}
	}
}
