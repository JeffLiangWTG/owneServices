using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseInventoryEDocsViaUniversalXmlSupportTest : WhsTestCaseWithFactory
	{
		public void TestLoadInventoryViewFromCode()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();

			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			var codeParts1 = new CodeParts(client1.OH_Code, product1.OP_PartNum, warehouse1.WW_WarehouseCode, location1.WLV_LocationString, "PLT01", inventory1.WI_ArrivalDate.ToShortDateString(), "X", "Y", "Z", "S");
			AssertEquals(inventory1.PK, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty())?.PK);

			var client2 = Helper.CreateClient("Client2");
			codeParts1.ClientCode = client2.OH_Code;
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
			codeParts1.ClientCode = client1.OH_Code; // Reverted

			var product2 = data.Part2;
			codeParts1.ProductCode = product2.OP_PartNum;
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
			codeParts1.ProductCode = product1.OP_PartNum; // Reverted

			var warehouse2 = Helper.CreateWarehouse("WH2");
			Helper.CreateRowAndGenerateLocations(warehouse2, "B", 1, 1);
			Factory.Save();
			var location2 = warehouse2.FindLocation("B");
			codeParts1.WarehouseCode = warehouse2.WW_WarehouseCode;
			codeParts1.LocationString = location2.WLV_LocationString;
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
			codeParts1.WarehouseCode = warehouse1.WW_WarehouseCode; // Reverted
			codeParts1.LocationString = location1.WLV_LocationString; // Reverted

			codeParts1.PalletID = "PLT02";
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
			codeParts1.PalletID = "PLT01"; // Reverted

			codeParts1.ArrivalDate = ZDateTimeOffset.Today.AddDays(-7).ToShortDateString();
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
			codeParts1.ArrivalDate = inventory1.WI_ArrivalDate.ToShortDateString();  // Reverted

			codeParts1.PartAttrib1 = "X1";
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
			codeParts1.PartAttrib1 = "X";  // Reverted

			codeParts1.PartAttrib2 = "Y1";
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
			codeParts1.PartAttrib2 = "Y";  // Reverted

			codeParts1.PartAttrib3 = "Z1";
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
			codeParts1.PartAttrib3 = "Z";  // Reverted

			codeParts1.SerialNumber = "S1";
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
			codeParts1.SerialNumber = "S";  // Reverted
		}

		public void TestLoadInventoryViewFromCodeWithWrongPartsLength()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			var codeParts1 = string.Join("|", new string[] { client1.OH_Code, product1.OP_PartNum, warehouse1.WW_WarehouseCode, location1.WLV_LocationString, "PLT01", inventory1.WI_ArrivalDate.ToShortDateString(), "X", "Y", "Z" });
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1));

			var codeParts2 = string.Join("|", new string[] { client1.OH_Code, product1.OP_PartNum, warehouse1.WW_WarehouseCode, location1.WLV_LocationString, "PLT01", inventory1.WI_ArrivalDate.ToShortDateString(), "X", "Y", "Z", "S", "Any" });
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts2));
		}

		public void TestLoadInventoryViewFromCodeWithEmptyParts()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			var codeParts1 = new CodeParts(client1.OH_Code, product1.OP_PartNum, warehouse1.WW_WarehouseCode, location1.WLV_LocationString, "PLT01", inventory1.WI_ArrivalDate.ToShortDateString(), "X", "Y", "Z", "S");
			codeParts1.ClientCode = ZString.Empty;
			AssertEquals("Should return null if client code is missing.", null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));

			codeParts1.ProductCode = ZString.Empty;
			AssertEquals("Should return null if client code and product code are missing.", null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));

			codeParts1.WarehouseCode = ZString.Empty;
			AssertEquals("Should return null if client code, product code and warehouse code are missing.", null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));

			codeParts1.LocationString = ZString.Empty;
			AssertEquals("Should return null if client code, product code, warehouse code and location string are missing.", null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));

			codeParts1.ArrivalDate = ZString.Empty;
			AssertEquals("Should return null if client code, product code and warehouse code, location string and arrivalDate are missing.", null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));

			var codeParts2 = new CodeParts(client1.OH_Code, product1.OP_PartNum, warehouse1.WW_WarehouseCode, location1.WLV_LocationString, ZString.Empty, inventory1.WI_ArrivalDate.ToShortDateString(), ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("Pallet id, part attribs and serial number are optional", inventory1.PK, loader.LoadBusinessObjectFromCode(Factory, codeParts2.GetCodeProperty())?.PK);
		}

		public void TestLoadInventoryViewFromCodeWithInvalidClientCode()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			var codeParts1 = new CodeParts("fakeClient", product1.OP_PartNum, warehouse1.WW_WarehouseCode, location1.WLV_LocationString, "PLT01", inventory1.WI_ArrivalDate.ToShortDateString(), "X", "Y", "Z", "S");
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
		}

		public void TestLoadInventoryViewFromCodeWithInvalidProductCode()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			var codeParts1 = new CodeParts(client1.OH_Code, "fakeProduct", warehouse1.WW_WarehouseCode, location1.WLV_LocationString, "PLT01", inventory1.WI_ArrivalDate.ToShortDateString(), "X", "Y", "Z", "S");
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
		}

		public void TestLoadInventoryViewFromCodeWithInvalidWarehouseCode()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			var codeParts1 = new CodeParts(client1.OH_Code, product1.OP_PartNum, "FWH", location1.WLV_LocationString, "PLT01", inventory1.WI_ArrivalDate.ToShortDateString(), "X", "Y", "Z", "S");
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
		}

		public void TestLoadInventoryViewFromCodeWithInvalidLocationString()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			var codeParts1 = new CodeParts(client1.OH_Code, product1.OP_PartNum, warehouse1.WW_WarehouseCode, "L-1", "PLT01", inventory1.WI_ArrivalDate.ToShortDateString(), "X", "Y", "Z", "S");
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
		}

		public void TestLoadInventoryViewFromCodeWithInvalidArrivalDate()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			var codeParts1 = new CodeParts(client1.OH_Code, product1.OP_PartNum, warehouse1.WW_WarehouseCode, location1.WLV_LocationString, "PLT01", "2024-10-30", "X", "Y", "Z", "S");
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));

			codeParts1.ArrivalDate = "30-10-2024";
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));

			codeParts1.ArrivalDate = "30-Oct-2024 02:00:00";
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
		}

		public void TestLoadInventoryViewFromCode_ShouldFilterOutZeroStock()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();

			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 0m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			AssertEquals("Precondition:", 0m, inventory1.WI_TotalUnits);

			var codeParts1 = new CodeParts(client1.OH_Code, product1.OP_PartNum, warehouse1.WW_WarehouseCode, location1.WLV_LocationString, "PLT01", inventory1.WI_ArrivalDate.ToShortDateString(), "X", "Y", "Z", "S");
			AssertEquals("Should filter out 0 stock on hand inventory", null, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty())?.PK);
		}

		public void TestLoadInventoryViewFromCode_MultipleMatchedRecords()
		{
			var loader = new WarehouseInventoryEDocsViaUniversalXmlSupport();

			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var warehouse1 = data.Whs1;
			var product1 = data.Part1;
			var location1 = data.Whs1.FindLocation("A");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m, location1, "PLT01");
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_PartAttrib1 = "X";
			inventory1.WI_PartAttrib2 = "Y";
			inventory1.WI_PartAttrib3 = "Z";
			inventory1.WI_SerialNumber = "S";

			var codeParts1 = new CodeParts(client1.OH_Code, product1.OP_PartNum, warehouse1.WW_WarehouseCode, location1.WLV_LocationString, ZString.Empty, inventory1.WI_ArrivalDate.ToShortDateString(), ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("Precondition: should return the record if there is only one matched record", inventory1.PK, loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty())?.PK);

			var receive2 = Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R2", product1, 20m, location1, "PLT02");
			var inventory2 = receive2.Inventory[0];
			inventory2.WI_PartAttrib1 = "X1";
			inventory2.WI_PartAttrib2 = "Y1";
			inventory2.WI_PartAttrib3 = "Z1";
			inventory2.WI_SerialNumber = "S1";

			var expectedMessage = string.Format("The unique code {0} you have supplied for Warehouse Inventory exists in multiple records.", codeParts1.GetCodeProperty());
			AssertExceptionThrown<NonUniqueAllocationCodeException>("Should throw exception if there are multiple matched records.", expectedMessage, () => loader.LoadBusinessObjectFromCode(Factory, codeParts1.GetCodeProperty()));
		}

		class CodeParts
		{
			public CodeParts(ZString clientCode, ZString productCode, ZString warehouseCode, ZString locationString, ZString palletID, ZString arrivalDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber)
			{
				ClientCode = clientCode;
				ProductCode = productCode;
				WarehouseCode = warehouseCode;
				LocationString = locationString;
				PalletID = palletID;
				ArrivalDate = arrivalDate;
				PartAttrib1 = partAttrib1;
				PartAttrib2 = partAttrib2;
				PartAttrib3 = partAttrib3;
				SerialNumber = serialNumber;
			}

			public ZString ClientCode { get; set; }
			public ZString ProductCode { get; set; }
			public ZString WarehouseCode { get; set; }
			public ZString LocationString { get; set; }
			public ZString PalletID { get; set; }
			public ZString ArrivalDate { get; set; }
			public ZString PartAttrib1 { get; set; }
			public ZString PartAttrib2 { get; set; }
			public ZString PartAttrib3 { get; set; }
			public ZString SerialNumber { get; set; }

			public string GetCodeProperty()
			{
				return string.Join("|", ClientCode, ProductCode, WarehouseCode, LocationString, PalletID, ArrivalDate, PartAttrib1, PartAttrib2, PartAttrib3, SerialNumber);
			}
		}
	}
}
