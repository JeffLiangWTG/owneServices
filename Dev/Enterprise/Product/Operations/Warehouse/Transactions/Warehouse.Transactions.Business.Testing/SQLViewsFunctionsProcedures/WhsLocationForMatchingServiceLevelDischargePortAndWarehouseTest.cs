#if DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLocationForMatchingServiceLevelDischargePortAndWarehouseSPTest :
		WhsLocationForMatchingServiceLevelDischargePortAndWarehouseTest
	{
		protected override void AssertFunctionResult(string message, object expectedResult, string serviceLevel,
			string dischargePort, ZGuid warehousePK)
		{
			var result = new List<ZGuid>();

			using (var command = Db.Connection.Command("WhsLocationForMatchingServiceLevelDischargePortAndWarehouseSP"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@ServiceLevel", SqlDbType.VarChar, 3,
					serviceLevel ?? "NULL");
				command.AddParameter("@DischargePort", SqlDbType.VarChar, 5,
					dischargePort ?? "NULL");
				command.AddParameter("@WarehousePK", SqlDbType.UniqueIdentifier,
					warehousePK.IsEmpty ? Guid.Empty : warehousePK.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetGuid(0));
					}
				}
			}

			if (expectedResult != DBNull.Value)
			{
				AssertEquals(1, result.Count);
				AssertEquals(message, expectedResult, result[0]);
			}
			else
			{
				AssertEquals(0, result.Count);
			}
		}
	}

	class WhsLocationForMatchingServiceLevelDischargePortAndWarehouseTest : WhsTestCaseWithFactory
	{
		#region TestParameters

		public void TestParameters()
		{
			// test if any parameter has no value, returns empty guid
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			Helper.CreateUNLOCO("AUTST");
			Factory.Save();

			var location = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-1", "STD", "AUTST");
			Factory.Save();

			AssertFunctionResult("All parameters are empty, should return null", DBNull.Value, "", "", ZGuid.Empty);
			AssertFunctionResult("Both Service Level and Warehouse PK are empty, should return null", DBNull.Value, "",
				"AUTST", ZGuid.Empty);
			AssertFunctionResult("Warehouse PK is empty, should return null", DBNull.Value, "STD", "AUTST",
				ZGuid.Empty);
			AssertFunctionResult("Both Service Level and Discharge Port are empty, should null", DBNull.Value, "", "",
				warehouse.PK);
			AssertFunctionResult("Service level is empty, should return null.", DBNull.Value, "", "AUTST",
				warehouse.PK);
			AssertFunctionResult("Discharge port is empty, should return null.", DBNull.Value, "STD", "", warehouse.PK);

			AssertFunctionResult("All parameters are null, should return null", DBNull.Value, null, null, ZGuid.Empty);
			AssertFunctionResult("Both Service Level and Warehouse PK are null, should return null", DBNull.Value, null,
				"AUTST", ZGuid.Empty);
			AssertFunctionResult("Warehouse PK is null, should return null", DBNull.Value, "STD", "AUTST", ZGuid.Empty);
			AssertFunctionResult("Both Service Level and Discharge Port are empty, should null", DBNull.Value, null,
				null, warehouse.PK);
			AssertFunctionResult("Service level is empty, should return null.", DBNull.Value, null, "AUTST",
				warehouse.PK);
			AssertFunctionResult("Discharge port is empty, should return null.", DBNull.Value, "STD", null,
				warehouse.PK);

			AssertFunctionResult("Should find location with matched service level, discharge port and warehouse.",
				location.PK, "STD", "AUTST", warehouse.PK);
		}

		#endregion

		#region TestLocationMatchServiceLevelAndWarehouse

		public void TestLocationMatchServiceLevelAndWarehouse()
		{
			// create a few combination of different service level and different warehouse, same discharge port,
			// assert result for different combination
			var warehouse1 = Helper.CreateWarehouse("Whs1", "A", 4, 1);
			var warehouse2 = Helper.CreateWarehouse("Whs2", "A", 4, 1);
			Helper.CreateUNLOCO("ZZ111");
			Helper.CreateUNLOCO("ZZ789");
			Factory.Save();

			var location11 = SetupLocationWithServiceLevelAndDischarge(warehouse1, "A-1", "STD", "ZZ111");
			var location12 = SetupLocationWithServiceLevelAndDischarge(warehouse1, "A-2", "STD", "ZZ789");
			var location13 = SetupLocationWithServiceLevelAndDischarge(warehouse1, "A-3", "D2D", "ZZ111");
			var location14 = SetupLocationWithServiceLevelAndDischarge(warehouse1, "A-4", "D2D", "ZZ789");

			var location21 = SetupLocationWithServiceLevelAndDischarge(warehouse2, "A-1", "STD", "ZZ111");
			var location22 = SetupLocationWithServiceLevelAndDischarge(warehouse2, "A-2", "STD", "ZZ789");
			var location23 = SetupLocationWithServiceLevelAndDischarge(warehouse2, "A-3", "D2D", "ZZ111");
			var location24 = SetupLocationWithServiceLevelAndDischarge(warehouse2, "A-4", "D2D", "ZZ789");

			Factory.Save();

			AssertFunctionResult("Should find location with matched service level, discharge port and warehouse.",
				location11.PK, "STD", "ZZ111", warehouse1.PK);
			AssertFunctionResult("Should find location with matched service level, discharge port and warehouse.",
				location12.PK, "STD", "ZZ789", warehouse1.PK);
			AssertFunctionResult("Should find location with matched service level, discharge port and warehouse.",
				location13.PK, "D2D", "ZZ111", warehouse1.PK);
			AssertFunctionResult("Should find location with matched service level, discharge port and warehouse.",
				location14.PK, "D2D", "ZZ789", warehouse1.PK);

			AssertFunctionResult("Should find location with matched service level, discharge port and warehouse.",
				location21.PK, "STD", "ZZ111", warehouse2.PK);
			AssertFunctionResult("Should find location with matched service level, discharge port and warehouse.",
				location22.PK, "STD", "ZZ789", warehouse2.PK);
			AssertFunctionResult("Should find location with matched service level, discharge port and warehouse.",
				location23.PK, "D2D", "ZZ111", warehouse2.PK);
			AssertFunctionResult("Should find location with matched service level, discharge port and warehouse.",
				location24.PK, "D2D", "ZZ789", warehouse2.PK);

			AssertFunctionResult("All parameters are empty, should return empty guid.", DBNull.Value, null, null,
				ZGuid.Empty);
			AssertFunctionResult("Invalid Discharge Port, should return empty guid.", DBNull.Value, "STD", "ZZ999",
				warehouse1.PK);
			AssertFunctionResult("Invalid Discharge Port, should return empty guid.", DBNull.Value, "D2D", "ZZ999",
				warehouse1.PK);
		}

		#endregion

		#region TestDischargePortFallBack_DischargePort

		public void TestDischargePortFallBack_DischargePort()
		{
			// create a location with a discharge port, call function with same discharge port, return correct location
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			Helper.CreateUNLOCO("AUTST");
			Factory.Save();

			var location = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-1", "STD", "AUTST");
			Factory.Save();

			AssertFunctionResult("Should find matching location.", location.PK, "STD", "AUTST", warehouse.PK);
		}

		#endregion

		#region TestDischargePortFallBack_Zone

		public void TestDischargePortFallBack_Zone()
		{
			// 2. create a location with a Zone, call function with a discharge port that belongs to the zone, return the location
			//          add a new location with exact passed in discharge port, call fucntion again, should return new added location
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			var dischargePort = Helper.CreateUNLOCO("AUTST");
			var zone = Helper.CreateTransitWarehouseZone("SYDE", "SYDNEY EAST");
			zone.UNLOCOs.Add(dischargePort);
			Factory.Save();

			var locationWithZone = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-1", "STD", "SYDE");
			Factory.Save();
			AssertFunctionResult("Find location with a Zone covers matching Discharge Port", locationWithZone.PK, "STD",
				"AUTST", warehouse.PK);

			var locationWithPort = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-2", "STD", "AUTST");
			Factory.Save();
			AssertFunctionResult("Find the location with matched Discharge Port.", locationWithPort.PK, "STD", "AUTST",
				warehouse.PK);
		}

		public void TestDischargePortFallBack_Zone_WithCorrectZone()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			var dischargePort1 = Helper.CreateUNLOCO("AU111");
			var dischargePort2 = Helper.CreateUNLOCO("AU222");
			Factory.Save();

			var zone1 = Helper.CreateTransitWarehouseZone("Z111", "Zone 111");
			zone1.UNLOCOs.Add(dischargePort1);

			var zone2 = Helper.CreateTransitWarehouseZone("Z222", "Zone 222");
			zone2.UNLOCOs.Add(dischargePort2);

			var locationWithZone1 = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-1", "STD", "Z111");
			var locationWithZone2 = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-2", "STD", "Z222");
			Factory.Save();

			AssertFunctionResult("Find the location with matched Discharge Port.", locationWithZone1.PK, "STD", "AU111",
				warehouse.PK);
		}

		#endregion

		#region TestDischargePortFallback_Country

		public void TestDischargePortFallback_Country()
		{
			// 3. similar to 2, start wtih a country, then add a location with a zone, then add a location with same passing-in discharge port
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 3, 1);
			var dischargePort = Helper.CreateUNLOCO("AUTST", "AU");
			Factory.Save();

			var locationWithCountry = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-1", "STD", "AU");
			Factory.Save();
			AssertFunctionResult("Find a location with a Country that covers matching Dischage Port.",
				locationWithCountry.PK, "STD", "AUTST", warehouse.PK);

			var zone = Helper.CreateTransitWarehouseZone("SYDE", "SYDNEY EAST");
			zone.UNLOCOs.Add(dischargePort);
			var locationWithZone = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-2", "STD", "SYDE");
			Factory.Save();
			AssertFunctionResult("Find a location with a Zone that covers matching Discharge Port.",
				locationWithZone.PK, "STD", "AUTST", warehouse.PK);
		}

		#endregion

		#region TestDischargePortFallback_InternationalZone

		public void TestDischargePortFallback_InternationalZone()
		{
			//The zone may have Countries attached so we need to make sure we also check for Zones that contain the Country as part of the Discharge fall back for matching Locations, then fall back to country alone.
			//similar to 2, start wtih a country, then add a location with a zone, then add a location with same passing-in discharge port
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 3, 1);
			var dischargePort = Helper.CreateUNLOCO("AUTST", "AU");
			Factory.Save();

			var locationWithCountry = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-1", "STD", "AU");
			Factory.Save();
			AssertFunctionResult("Find a location with a Country that covers matching Dischage Port.",
				locationWithCountry.PK, "STD", "AUTST", warehouse.PK);

			var countryAU = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			var zoneSyd = Helper.CreateTransitWarehouseZone("SYDZ", "Sydney Zone");
			zoneSyd.Countries.Add(countryAU);
			AssertEquals(1, zoneSyd.Countries.Count);

			var locationWithZone = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-2", "STD", "SYDZ");
			Factory.Save();
			AssertFunctionResult("Find a location with a Zone that covers matching Discharge Port's Country.",
				locationWithZone.PK, "STD", "AUTST", warehouse.PK);

			var locationWithPort = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-3", "STD", "AUTST");
			Factory.Save();
			AssertFunctionResult("Find the location with matched Discharge Port.", locationWithPort.PK, "STD", "AUTST",
				warehouse.PK);
		}

		#endregion

		#region TestDischargePortFallback_ZoneWithUNLOCO_Over_InternationalZone

		public void TestDischargePortFallback_ZoneWithUNLOCO_Over_InternationalZone()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 3, 1);
			var dischargePort = Helper.CreateUNLOCO("AUTST", "AU");
			Factory.Save();

			var locationWithCountry = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-1", "STD", "AU");
			Factory.Save();
			AssertFunctionResult("Find a location with a Country that covers matching Dischage Port.",
				locationWithCountry.PK, "STD", "AUTST", warehouse.PK);

			var countryAU = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			var zoneSyd = Helper.CreateTransitWarehouseZone("SYDZ", "Sydney Zone");
			zoneSyd.Countries.Add(countryAU);
			AssertEquals(1, zoneSyd.Countries.Count);

			var locationWithInternationalZone =
				SetupLocationWithServiceLevelAndDischarge(warehouse, "A-2", "STD", "SYDZ");
			Factory.Save();
			AssertFunctionResult("Find a location with a Zone that covers matching Discharge Port's Country.",
				locationWithInternationalZone.PK, "STD", "AUTST", warehouse.PK);

			var zoneSydE = Helper.CreateTransitWarehouseZone("SYDE", "SYDNEY EAST");
			zoneSydE.UNLOCOs.Add(dischargePort);
			var locationWithZoneWithUNLOCO = SetupLocationWithServiceLevelAndDischarge(warehouse, "A-3", "STD", "SYDE");
			Factory.Save();
			AssertFunctionResult("Find a location with a Zone that covers matching Discharge Port.",
				locationWithZoneWithUNLOCO.PK, "STD", "AUTST", warehouse.PK);
		}

		#endregion

		#region Implementation

		protected virtual void AssertFunctionResult(string message, object expectedResult, string serviceLevel,
			string dischargePort, ZGuid warehousePK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql =
				@"SELECT WL_PK From dbo.WhsLocationForMatchingServiceLevelDischargePortAndWarehouse(@ServiceLevel, @DischargePort, @WarehousePK)";
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@ServiceLevel", serviceLevel ?? "NULL",
				WhsLocationViewSchema.WLV_RS_NKTransitServiceLevel);
			parameters.Add("@DischargePort", dischargePort ?? "NULL",
				WhsLocationViewSchema.WLV_TransitDischargeLRC);
			parameters.Add("@WarehousePK", warehousePK, WhsLocationViewSchema.WLV_WW_Whs);

			result.Load(sql, parameters);

			if (expectedResult != DBNull.Value)
			{
				AssertEquals(1, result.Count);
				AssertEquals(message, expectedResult, result[0]["WL_PK"]);
			}
			else
			{
				AssertEquals(0, result.Count);
			}
		}

		WhsLocation SetupLocationWithServiceLevelAndDischarge(WhsWarehouse warehouse, string locationString,
			string serviceLevel, string transitDischargeLRC)
		{
			var location = warehouse.FindLocation(locationString);
			if (location != null)
			{
				location.WLV_RS_NKTransitServiceLevel = serviceLevel;
				location.WLV_TransitDischargeLRC = transitDischargeLRC;
			}

			return location;
		}

		#endregion
	}
}

#endif
