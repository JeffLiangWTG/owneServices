using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAsnLineCollection))]
	class WhsAsnLineCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		#region Test Cases

		public void TestAddInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var year = ZDateTime.Now.Year;
			data.Org1.MiscServ.OM_IMPartAttrib1Name = "Attr1";
			data.Org1.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "Attr2";
			data.Org1.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "Attr3";
			data.Org1.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;

			data.Part1.OP_StockKeepingUnit = "UNT";
			data.Part2.OP_StockKeepingUnit = "KG";
			AssertEquals(0, Collection.Parent.Inventory.Count);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0, null, "PLT-1");
			receive.RunPreSaveValidation();
			inventory.InDocketLine.WE_ClientOrderedUnits = 10m;

			var asnLine = Collection.Add(inventory.InDocketLine);

			AssertASNLineCreatedCorrectlyFromInventory(asnLine, inventory.InDocketLine);

			AssertEquals(1, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 5m;
			Collection.Add(inventory.InDocketLine);
			AssertEquals(1, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "UNT", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 10m;
			inventory.InDocketLine.WE_OP = data.Part2.PK;
			Collection.Add(inventory.InDocketLine);
			AssertEquals(2, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 10m;
			inventory.InDocketLine.WE_OP = data.Part1.PK;
			Collection.Add(inventory.InDocketLine);
			AssertEquals(2, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 10m;
			inventory.InDocketLine.WE_PartAttrib1 = "PA11";
			Collection.Add(inventory.InDocketLine);
			AssertEquals(3, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 5m;
			inventory.InDocketLine.WE_PartAttrib1 = "PA12";
			Collection.Add(inventory.InDocketLine);
			AssertEquals(4, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 15m;
			inventory.InDocketLine.WE_OP = data.Part2.PK;
			inventory.InDocketLine.WE_PartAttrib1 = "PA11";
			Collection.Add(inventory.InDocketLine);
			AssertEquals(5, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 35m;
			inventory.InDocketLine.WE_OP = data.Part1.PK;
			inventory.InDocketLine.WE_PartAttrib2 = "PA21";
			Collection.Add(inventory.InDocketLine);
			AssertEquals(6, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 45m;
			inventory.InDocketLine.WE_PartAttrib3 = "PA31";
			Collection.Add(inventory.InDocketLine);
			AssertEquals(7, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 40m;
			inventory.InDocketLine.WE_PackingDate = new ZDate(year, 05, 09);
			Collection.Add(inventory.InDocketLine);
			AssertEquals(8, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 30m;
			inventory.InDocketLine.WE_PackingDate = new ZDate(year, 05, 09);
			inventory.InDocketLine.WE_ExpiryDate = new ZDate(year, 05, 10);
			Collection.Add(inventory.InDocketLine);
			AssertEquals(9, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "UNT", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 30m;
			inventory.InDocketLine.WE_OP = data.Part2.PK;
			inventory.InDocketLine.WE_PackingDate = new ZDate(year, 05, 09);
			inventory.InDocketLine.WE_ExpiryDate = new ZDate(year, 05, 10);
			Collection.Add(inventory.InDocketLine);
			AssertEquals(10, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);

			inventory.InDocketLine.WE_ClientOrderedUnits = 30m;
			inventory.InDocketLine.WE_OP = data.Part1.PK;
			inventory.InDocketLine.WE_PartAttrib1 = "";
			inventory.InDocketLine.WE_PartAttrib2 = "PA21";
			inventory.InDocketLine.WE_PartAttrib3 = "PA31";
			inventory.InDocketLine.WE_PackingDate = new ZDate(year, 05, 09);
			inventory.InDocketLine.WE_ExpiryDate = new ZDate(year, 05, 10);
			Collection.Add(inventory.InDocketLine);
			AssertEquals(10, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 60m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);

			inventory.InDocketLine.WE_ExpiryDate = ZDate.Empty;
			inventory.InDocketLine.WE_ClientOrderedUnits = 0m;
			Collection.Add(inventory.InDocketLine);
			AssertEquals(10, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 60m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);

			inventory.InDocketLine.WE_PackingDate = ZDate.Empty;
			inventory.InDocketLine.WE_ClientOrderedUnits = 0m;
			Collection.Add(inventory.InDocketLine);
			AssertEquals(10, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 60m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);

			inventory.InDocketLine.WE_PartAttrib3 = "";
			inventory.InDocketLine.WE_ClientOrderedUnits = 0m;
			Collection.Add(inventory.InDocketLine);
			AssertEquals(10, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 60m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);

			inventory.InDocketLine.WE_PartAttrib1 = "PA11";
			inventory.InDocketLine.WE_PartAttrib2 = "PA21";
			inventory.InDocketLine.WE_ClientOrderedUnits = 30m;
			Collection.Add(inventory.InDocketLine);
			AssertEquals(11, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 60m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "PA21", "", "", ZDate.Empty, ZDate.Empty, 30m, "UNT", 1, 0);

			inventory.InDocketLine.WE_PartAttrib2 = "";
			inventory.InDocketLine.WE_ClientOrderedUnits = 30m;
			inventory.InDocketLine.WE_LineNo = 1;
			inventory.InDocketLine.WE_SubLineNo = 1;
			Collection.Add(inventory.InDocketLine);
			AssertEquals(12, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 60m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "PA21", "", "", ZDate.Empty, ZDate.Empty, 30m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 30m, "UNT", 1, 1);

			Collection.Add(inventory.InDocketLine);
			AssertEquals(12, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 60m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "PA21", "", "", ZDate.Empty, ZDate.Empty, 30m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 60m, "UNT", 1, 1);

			inventory.InDocketLine.WE_SubLineNo = 2;
			inventory.InDocketLine.WE_ClientOrderedUnits = 30m;
			Collection.Add(inventory.InDocketLine);
			AssertEquals(13, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 60m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "PA21", "", "", ZDate.Empty, ZDate.Empty, 30m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 60m, "UNT", 1, 1);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 30m, "UNT", 1, 2);

			inventory.InDocketLine.WE_PalletID = "PLT-2";

			Collection.Add(inventory.InDocketLine);
			AssertEquals(14, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 25m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA12", "", "", "", ZDate.Empty, ZDate.Empty, 5m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 15m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "", "", ZDate.Empty, ZDate.Empty, 35m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", ZDate.Empty, ZDate.Empty, 45m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), ZDate.Empty, 40m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "PA21", "PA31", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 60m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part2, "PLT-1", "", "", "", "", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 30m, "KG", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "PA21", "", "", ZDate.Empty, ZDate.Empty, 30m, "UNT", 1, 0);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 60m, "UNT", 1, 1);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 30m, "UNT", 1, 2);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-2", "PA11", "", "", "", ZDate.Empty, ZDate.Empty, 30m, "UNT", 1, 2);
		}

		public void TestAddInventory_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			AssertEquals(0, Collection.Parent.Inventory.Count);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0, null, "PLT-1");
			inventory.WI_SerialNumber = "SN1";
			receive.RunPreSaveValidation();
			inventory.InDocketLine.WE_ClientOrderedUnits = 1m;

			var asnLine = Collection.Add(inventory.InDocketLine);
			AssertASNLineCreatedCorrectlyFromInventory(asnLine, inventory.InDocketLine);

			AssertEquals(1, Collection.Count);
			AssertUniqueASNLine(Collection, data.Part1, "PLT-1", "", "", "", "SN1", ZDate.Empty, ZDate.Empty, 1m, "UNT", 1, 0);
		}

		public void TestParent()
		{
			AssertNotNull(Collection.Parent);
			AssertEquals(typeof(WhsReceive), Collection.Parent.GetType());
			AssertEquals(Receive, Collection.Parent);
		}

		#endregion

		#region Implementation

		protected void AssertASNLineCreatedCorrectlyFromInventory(WhsAsnLine asnLine, WhsDocketLine docketLine)
		{
			AssertEquals(asnLine.WN_OP, docketLine.WE_OP);
			AssertEquals(asnLine.WN_PackingDate, docketLine.WE_PackingDate);
			AssertEquals(asnLine.WN_ExpiryDate, docketLine.WE_ExpiryDate);
			AssertEquals(asnLine.WN_PartAttrib1, docketLine.WE_PartAttrib1);
			AssertEquals(asnLine.WN_PartAttrib2, docketLine.WE_PartAttrib2);
			AssertEquals(asnLine.WN_PartAttrib3, docketLine.WE_PartAttrib3);
			AssertEquals(asnLine.WN_SerialNumber, docketLine.WE_SerialNumber);
			AssertEquals(asnLine.WN_QuantityUQ, docketLine.ProductUQ);
			AssertEquals(asnLine.WN_LineNo, docketLine.WE_LineNo);
			AssertEquals(asnLine.WN_SubLineNo, docketLine.WE_SubLineNo);
			AssertEquals(asnLine.WN_PalletId, docketLine.WE_PalletID);
		}

		protected void AssertUniqueASNLine(WhsAsnLineCollection asnLines, OrgSupplierPart part, ZString palletID, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate packingDate, ZDate expiryDate, ZDecimal units, ZString unitsUQ)
		{
			AssertUniqueASNLine(asnLines, part, palletID, partAttrib1, partAttrib2, partAttrib3, serialNumber, packingDate, expiryDate, units, unitsUQ, 0, 0);
		}

		protected void AssertUniqueASNLine(WhsAsnLineCollection asnLines, OrgSupplierPart part, ZString palletID, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate packingDate, ZDate expiryDate, ZDecimal units, ZString unitsUQ, ZShort lineNo, ZShort subLineNo)
		{
			int matchCount = 0;

			foreach (WhsAsnLine line in asnLines)
			{
				if (line.WN_OP == part.PK)
				{
					if (line.WN_PartAttrib1 == partAttrib1 && line.WN_PartAttrib2 == partAttrib2 && line.WN_PartAttrib3 == partAttrib3 && line.WN_SerialNumber == serialNumber)
					{
						if (line.WN_PackingDate == packingDate && line.WN_ExpiryDate == expiryDate && line.WN_PalletId == palletID)
						{
							if (line.WN_LineNo == lineNo && line.WN_SubLineNo == subLineNo)
							{
								AssertEquals(units, line.WN_Quantity);
								AssertEquals(unitsUQ, line.WN_QuantityUQ);
								matchCount++;
							}
						}
					}
				}
			}

			AssertEquals(1, matchCount);
		}

		protected new WhsAsnLineCollection Collection
		{
			get { return (WhsAsnLineCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsAsnLineCollection(Receive, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Client = Helper.CreateClient();
			Warehouse = Helper.CreateWarehouse("WHS");
			Receive = Helper.CreateWhsReceive(Client, Warehouse);
		}

		OrgHeader Client;
		WhsWarehouse Warehouse;
		WhsReceive Receive;

		#endregion
	}
}
