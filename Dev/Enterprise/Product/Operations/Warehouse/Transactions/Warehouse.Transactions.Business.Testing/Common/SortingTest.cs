using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class SortingTest : WhsTestCaseWithFactory
	{
		#region SortForPickingByAttribute

		class SortForPickingByAttributeTest : SortByPropertiesComparer<TestILineAttributes>
		{
			protected override IEnumerable<IComparer<TestILineAttributes>> GetElementaryComparers() =>
				SortForPickingByAttribute.GetBaseComparers(this);
		}

		public void TestSortForPickingByAttribute()
		{
			var linesCount = 10;
			var lines = GetIAttributesTestCollection();
			var sortedLines = new TestILineAttributes[linesCount];

			Array.Copy(lines, sortedLines, linesCount);
			Array.Sort(sortedLines, new SortForPickingByAttributeTest());

			StandardAsserts(lines, sortedLines);
		}

		#endregion

		#region SortItemsForPicking

		public void TestSortItemsForPicking()
		{
			var linesCount = 10;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var attributes = GetIAttributesTestCollection();
			var orderedInventories = new WhsPickOrderedInventory[linesCount];
			Factory.Save();

			var order = Factory.New<WhsOrder>();
			var pick = Factory.New<WhsPick>();
			var sortedOrderedInventories = new WhsPickOrderedInventoryCollection(Factory, pick);

			orderedInventories[0] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[0], data.Part2.PK, 60m);
			orderedInventories[1] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[0], data.Part1.PK, 20m);
			orderedInventories[2] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[1], data.Part1.PK, 20m);
			orderedInventories[3] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[2], data.Part1.PK, 20m);
			orderedInventories[4] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[3], data.Part1.PK, 20m);
			orderedInventories[5] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[4], data.Part1.PK, 20m);
			orderedInventories[6] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[5], data.Part1.PK, 20m);
			orderedInventories[7] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[6], data.Part1.PK, 20m);
			orderedInventories[8] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[7], data.Part1.PK, 20m);
			orderedInventories[9] =
				GetPickOrderedInventory(sortedOrderedInventories, order, attributes[8], data.Part1.PK, 20m);

			sortedOrderedInventories.Sort(new SortItemsForPicking());

			StandardAsserts(orderedInventories, sortedOrderedInventories);
		}

		public void TestSortItemsForPickingBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct1 =
				Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			var bomPartForMainProduct2 =
				Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 =
				Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			var inventory3 =
				Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine11 = Helper.CreateWhsOrderLine(order1, mainProduct, 6m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine21 = Helper.CreateWhsOrderLine(order2, mainProduct, 2m);
			Factory.Save();

			var pick = Helper.CreatePickNew(new WhsPickableDocket[2] { order1, order2 });

			AssertEquals("3 OrderedInventories should return", 3, pick.OrderedInventories.Count);
			AssertEquals("Inventory for mainProduct should be at first index", mainProduct.PK,
				pick.OrderedInventories[0].SupplierPart.PK);
		}

		#endregion

		#region SortInventoryForPicking

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2018, 12, 12)]
		public void TestSortInventoryForPicking()
		{
			TestDateAttribute.UseUNLOCO = true;

			var linesCount = 17;
			var today = ZDateTimeOffset.Today;
			var emptyDate = ZDateTimeOffset.Empty;
			var attributes = GetIAttributesTestCollection();
			var lines = new WhsInventoryView[linesCount];
			var sortedLines = new WhsInventoryViewCollection(Factory);
			var client = Helper.CreateClient("Client");
			var part = Helper.CreateProduct(client, "P1");
			var product = WhsProduct.GetWhsProduct(part);
			var whs = Helper.CreateWarehouse("Warehouse");
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2);
			for (int i = 0; i < linesCount; i++)
			{
				lines[i] = sortedLines.AddNew();
			}

			product.PickFaces.Add(Helper.CreateProductPickFace(product, client, row1.Locations[4]));

			SetInventory(lines[0], attributes[0], emptyDate, row2.Locations[7].PK, part.PK);
			SetInventory(lines[1], attributes[0], emptyDate, row2.Locations[6].PK, part.PK);
			SetInventory(lines[2], attributes[0], emptyDate, row2.Locations[4].PK, part.PK);
			SetInventory(lines[3], attributes[0], emptyDate, row2.Locations[0].PK, part.PK);
			SetInventory(lines[4], attributes[0], emptyDate, row1.Locations[7].PK, part.PK);
			SetInventory(lines[5], attributes[1], emptyDate, row1.Locations[0].PK, part.PK);
			SetInventory(lines[6], attributes[2], emptyDate, row1.Locations[0].PK, part.PK);
			SetInventory(lines[7], attributes[3], emptyDate, row1.Locations[0].PK, part.PK);
			SetInventory(lines[8], attributes[4], emptyDate, row1.Locations[0].PK, part.PK);
			SetInventory(lines[9], attributes[5], emptyDate, row1.Locations[0].PK, part.PK);
			SetInventory(lines[10], attributes[5], today, row1.Locations[0].PK, part.PK);
			SetInventory(lines[11], attributes[5], today.AddMonths(-1), row1.Locations[0].PK, part.PK);
			SetInventory(lines[12], attributes[5], emptyDate, row1.Locations[4].PK, part.PK);
			SetInventory(lines[13], attributes[6], emptyDate, row1.Locations[0].PK, part.PK);
			SetInventory(lines[14], attributes[7], emptyDate, row1.Locations[0].PK, part.PK);
			SetInventory(lines[15], attributes[8], emptyDate, row1.Locations[0].PK, part.PK);
			SetInventory(lines[16], attributes[9], emptyDate, row1.Locations[0].PK, part.PK);

			sortedLines.Sort(new SortInventoryForPicking());

			StandardAsserts(lines, sortedLines);
		}

		#region TestSortInventoryForPicking_SortInvalidLocations

		public void TestSortInventoryForPicking_SortInvalidLocations_EmptyGuid()
		{
			TestSortInventoryForPicking_SortInvalidLocationsCore("Sorting shouldn't be an issue.", (whs) => ZGuid.Empty,
				(whs) => ZGuid.Empty, expectSameOrder: true);
		}

		public void TestSortInventoryForPicking_SortInvalidLocations_EmptyAndInvalidGuid()
		{
			TestSortInventoryForPicking_SortInvalidLocationsCore("Sorting shouldn't be an issue.", (whs) => ZGuid.Empty,
				(whs) => ZGuid.Invalid, expectSameOrder: true);
		}

		public void TestSortInventoryForPicking_SortInvalidLocations_FirstInventoryInvalid()
		{
			TestSortInventoryForPicking_SortInvalidLocationsCore(
				"The line with the location should be sorted and moved to the top of the list.", (whs) => ZGuid.Empty,
				(whs) => whs.FindLocation("A-1-1-1").PK, expectSameOrder: false);
		}

		public void TestSortInventoryForPicking_SortInvalidLocations_SecondInventoryInvalid()
		{
			TestSortInventoryForPicking_SortInvalidLocationsCore(
				"The line with the location should be sorted and moved to the top of the list.",
				(whs) => whs.FindLocation("A-1-1-1").PK, (whs) => ZGuid.Empty, expectSameOrder: true);
		}

		void TestSortInventoryForPicking_SortInvalidLocationsCore(string msg, Func<WhsWarehouse, ZGuid> location1PK,
			Func<WhsWarehouse, ZGuid> location2PK, bool expectSameOrder)
		{
			var emptyDate = ZDateTimeOffset.Empty;
			var attributes = GetIAttributesTestCollection();
			var lines = new WhsInventoryView[2];
			var sortedLines = new WhsInventoryViewCollection(Factory);
			var client = Helper.CreateClient("Client");
			var part = Helper.CreateProduct(client, "P1");
			var whs = Helper.CreateWarehouse("Warehouse");
			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			Factory.Save();
			lines[0] = sortedLines.AddNew();
			lines[1] = sortedLines.AddNew();

			SetInventory(lines[0], attributes[0], emptyDate, location1PK(whs), part.PK);
			SetInventory(lines[1], attributes[0], emptyDate, location2PK(whs), part.PK);

			sortedLines.Sort(new SortInventoryForPicking());

			if (expectSameOrder)
			{
				AssertContainsExactElementsInExactOrder(msg, sortedLines, lines[0], lines[1]);
			}
			else
			{
				AssertContainsExactElementsInExactOrder(msg, sortedLines, lines[1], lines[0]);
			}
		}

		#endregion

		#endregion

		#region SortPickInventoryForPicking

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2018, 12, 12)]
		public void TestSortPickInventoryForPicking()
		{
			TestDateAttribute.UseUNLOCO = true;

			var linesCount = 16;
			var today = ZDate.Today;
			var todayOffset = ZDateTimeOffset.Today;
			var emptyDate = ZDate.Empty;
			var emptyDateOffset = ZDateTimeOffset.Empty;
			var availableInventories = new WhsPickAvailableInventory[linesCount];
			var sortedAvailableInventories = new WhsPickAvailableInventoryCollection(Factory);
			var client = Helper.CreateClient("Client");
			var part = Helper.CreateProduct(client, "P1");
			var product = WhsProduct.GetWhsProduct(part);
			var whs = Helper.CreateWarehouse("Warehouse");
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			var order = Helper.CreateWhsOrder(client, whs);
			for (int i = 0; i < linesCount; i++)
			{
				availableInventories[i] = sortedAvailableInventories.AddNew();
			}

			product.PickFaces.Add(Helper.CreateProductPickFace(product, client, row1.Locations[2]));

			SetPickAvailableInventory(availableInventories[0], order,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", "", "SN1"), todayOffset, row1.Locations[7].PK,
				part.PK, "A");
			SetPickAvailableInventory(availableInventories[1], order,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", "PA3"), todayOffset, row1.Locations[7].PK, part.PK, "B");
			SetPickAvailableInventory(availableInventories[2], order,
				new TestILineAttributes("", emptyDate, emptyDate, "", "PA2", ""), todayOffset, row1.Locations[7].PK, part.PK, "A");
			SetPickAvailableInventory(availableInventories[3], order,
				new TestILineAttributes("", emptyDate, emptyDate, "PA1", "", ""), todayOffset, row1.Locations[7].PK, part.PK, "B");
			SetPickAvailableInventory(availableInventories[4], order,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset, row1.Locations[6].PK, part.PK, "A");
			SetPickAvailableInventory(availableInventories[5], order,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset, row1.Locations[4].PK, part.PK, "B");
			SetPickAvailableInventory(availableInventories[6], order,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset.AddDays(-3), row1.Locations[0].PK,
				part.PK, "A");
			SetPickAvailableInventory(availableInventories[7], order,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset.AddDays(-4), row1.Locations[0].PK,
				part.PK, "A");
			SetPickAvailableInventory(availableInventories[8], order,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset.AddDays(-5), row1.Locations[0].PK,
				part.PK, "B");
			SetPickAvailableInventory(availableInventories[9], order,
				new TestILineAttributes("", emptyDate, emptyDate, "PA2", "", ""), todayOffset.AddMonths(-1),
				row1.Locations[0].PK, part.PK, "A");
			SetPickAvailableInventory(availableInventories[10], order,
				new TestILineAttributes("", emptyDate, emptyDate, "PA1", "", ""), todayOffset.AddMonths(-1),
				row1.Locations[0].PK, part.PK, "B");
			SetPickAvailableInventory(availableInventories[11], order,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset.AddDays(-2), row1.Locations[2].PK,
				part.PK, "A");
			SetPickAvailableInventory(availableInventories[12], order,
				new TestILineAttributes("", today.AddMonths(-2), today.AddDays(-1), "", "", ""), emptyDateOffset,
				row1.Locations[0].PK, part.PK, "B");
			SetPickAvailableInventory(availableInventories[13], order,
				new TestILineAttributes("", today.AddMonths(-2), today.AddDays(-2), "", "", ""), emptyDateOffset,
				row1.Locations[0].PK, part.PK, "A");
			SetPickAvailableInventory(availableInventories[14], order,
				new TestILineAttributes("", today.AddMonths(-3), emptyDate, "PA1", "", ""), todayOffset.AddDays(-1),
				row1.Locations[0].PK, part.PK, "B");
			SetPickAvailableInventory(availableInventories[15], order,
				new TestILineAttributes("", today.AddMonths(-3), emptyDate, "PA2", "", ""), todayOffset.AddDays(-2),
				row1.Locations[0].PK, part.PK, "A");

			sortedAvailableInventories.Sort(new SortPickInventoryForPicking());

			StandardAsserts(availableInventories, sortedAvailableInventories);
		}

		public void TestSortPickInventoryForPicking_FallsBackToPalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT5");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT4");

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);

			var availableInventories = pick1.OrderedInventories[0].AvailableInventories;
			var availableInventoriesSortedArray = availableInventories.Cast<WhsPickAvailableInventory>().OrderBy(ai => ai.PalletID).ToArray();
			availableInventories.Sort(new SortPickInventoryForPicking());

			StandardAssertsCorrectOrder(availableInventoriesSortedArray, availableInventories);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2018, 12, 12)]
		public void TestSortPickInventoryForBondReleasePicking()
		{
			TestDateAttribute.UseUNLOCO = true;

			var linesCount = 18;
			var today = ZDate.Today;
			var todayOffset = ZDateTimeOffset.Today;
			var emptyDate = ZDate.Empty;
			var emptyDateOffset = ZDateTimeOffset.Empty;
			var attributes = GetIAttributesTestCollection();
			var lines = new WhsPickAvailableInventory[linesCount];
			var sortedLines = new WhsPickAvailableInventoryCollection(Factory);
			var client = Helper.CreateClient("Client");
			var part = Helper.CreateProduct(client, "P1");
			var product = WhsProduct.GetWhsProduct(part);
			var whs = Helper.CreateWarehouse("Warehouse");
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			var order = Helper.CreateWhsOrder(client, whs);
			for (var i = 0; i < linesCount; i++)
			{
				lines[i] = sortedLines.AddNew();
			}

			product.PickFaces.Add(Helper.CreateProductPickFace(product, client, row1.Locations[2]));

			SetPickAvailableInventory(lines[0], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", "", "SN1"), todayOffset, row1.Locations[7].PK,
				part.PK);
			SetPickAvailableInventory(lines[1], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", "PA3"), todayOffset, row1.Locations[7].PK, part.PK);
			SetPickAvailableInventory(lines[2], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "", "PA2", ""), todayOffset, row1.Locations[7].PK, part.PK);
			SetPickAvailableInventory(lines[3], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "PA1", "", ""), todayOffset, row1.Locations[7].PK, part.PK);
			SetPickAvailableInventory(lines[4], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset, row1.Locations[6].PK, part.PK);
			SetPickAvailableInventory(lines[5], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset, row1.Locations[4].PK, part.PK);
			SetPickAvailableInventory(lines[6], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset.AddDays(-3), row1.Locations[0].PK,
				part.PK);
			SetPickAvailableInventory(lines[7], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset.AddDays(-4), row1.Locations[0].PK,
				part.PK);
			SetPickAvailableInventory(lines[8], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset.AddDays(-5), row1.Locations[0].PK,
				part.PK);
			SetPickAvailableInventory(lines[9], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "PA2", "", ""), todayOffset.AddMonths(-1),
				row1.Locations[0].PK, part.PK);
			SetPickAvailableInventory(lines[10], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "PA1", "", ""), todayOffset.AddMonths(-1),
				row1.Locations[0].PK, part.PK);
			SetPickAvailableInventory(lines[11], order, emptyDate,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset.AddDays(-2), row1.Locations[2].PK,
				part.PK);
			SetPickAvailableInventory(lines[12], order, emptyDate,
				new TestILineAttributes("", today.AddMonths(-2), today.AddDays(-1), "", "", ""), emptyDateOffset,
				row1.Locations[0].PK, part.PK);
			SetPickAvailableInventory(lines[13], order, emptyDate,
				new TestILineAttributes("", today.AddMonths(-2), today.AddDays(-2), "", "", ""), emptyDateOffset,
				row1.Locations[0].PK, part.PK);
			SetPickAvailableInventory(lines[14], order, emptyDate,
				new TestILineAttributes("", today.AddMonths(-3), emptyDate, "PA1", "", ""), todayOffset.AddDays(-1),
				row1.Locations[0].PK, part.PK);
			SetPickAvailableInventory(lines[15], order, emptyDate,
				new TestILineAttributes("", today.AddMonths(-3), emptyDate, "PA2", "", ""), todayOffset.AddDays(-2),
				row1.Locations[0].PK, part.PK);
			SetPickAvailableInventory(lines[16], order, today,
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset, row1.Locations[6].PK, part.PK);
			SetPickAvailableInventory(lines[17], order, today.AddMonths(-1),
				new TestILineAttributes("", emptyDate, emptyDate, "", "", ""), todayOffset, row1.Locations[4].PK, part.PK);

			sortedLines.Sort(new SortPickInventoryForBondReleasePicking());

			StandardAsserts(lines, sortedLines);
		}

		#endregion

		#region SortPickLinesForPickingSlip

		public void TestSortPickLinesForPickingSlip()
		{
			var pick = SetupForSortPickLines();

			// Apply Sort
			var pickLines = pick.GetAllPickLines().ToArray();
			Array.Sort(pickLines, new SortPickLinesForPickingSlip());

			// Sort should be by Client, Pick Method, Pick Group, Row Sequence, Location Sequence, Location, Product Code
			int index = 0;
			AssertEquals("Collection should have right Count", 28, pickLines.Length);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 1, 0, 0, "B-2-2-1", "P5"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 1, 0, 0, "B-2-2-2", "P5"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 2, 0, 0, "B-1-1-1", "P4"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 2, 0, 0, "B-1-1-2", "P4"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 1, "D-1-1-2", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 1, "E-1-1-2", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 2, "D-1-1-1", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 2, "E-1-1-1", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 3, "D-2-1-1", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 3, "D-2-1-2", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 0, "D-2-2-1", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 2, 1, "C-2-2-2", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 2, 2, "C-1-1-2", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 2, 0, "C-1-1-1", "P6"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "A-1-1-1", "P1"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "A-1-1-1", "P2"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "A-1-1-2", "P1"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "A-1-2-1", "P1"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "A-1-2-1", "P2"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "A-2-2-1", "P2"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "B-2-1-1", "P1"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "B-2-1-1", "P2"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "TRB", 0, 0, 0, "A-2-1-1", "P1"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "TRB", 0, 0, 0, "A-2-1-2", "P1"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "2", "ANY", 0, 0, 1, "A-1-2-2", "P3"),
				pickLines[index++].Inventory); // Should Use Row Name To Sort first if Row Sequence is zero
			AssertEquals(GetInventoryWithParameters(pickLines, "2", "ANY", 0, 0, 0, "A-1-2-1", "P3"),
				pickLines[index++].Inventory);
			AssertEquals(GetInventoryWithParameters(pickLines, "2", "ANY", 0, 0, 1, "B-1-2-2", "P3"),
				pickLines[index++].Inventory); // Should Use Row Name To Sort first if Row Sequence is zero
			AssertEquals(GetInventoryWithParameters(pickLines, "2", "ANY", 0, 0, 0, "B-1-2-1", "P3"),
				pickLines[index++].Inventory);
		}

		WhsInventoryView GetInventoryWithParameters(WhsPickLine[] pickLines, ZString clientCode, ZString pickMethod,
			ZShort pickGroup, ZShort rowSequence, ZShort locationSequence, ZString locationString, ZString productCode)
		{
			return pickLines
				.Where(o => o.DocketLine.WE_PickGroup == pickGroup)
				.Select(o => o.InventoryLineForAvailableInventory)
				.Single(o => o.Docket.Client.OH_Code == clientCode
							 && o.Location.WLV_PickMethod == pickMethod
							 && o.Location.RowPathSequence == rowSequence
							 && o.Location.WLV_PickPathSequence == locationSequence
							 && o.Location.WLV_LocationString == locationString
							 && o.ProductCode == productCode).Inventory[0];
		}

		public void TestSortPickLinesForPickingSlip_InTransit_PickMethod()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 2, 2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);

			// Setup Registry
			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			var pickMethod = pickMethods.AddNew();
			pickMethod.Code = "TRB";
			pickMethod.Bool = true;
			pickMethod.Description = (NoResString)"Tractor Beam";
			Factory.Save();

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					   pickMethods))
			{
				SetInventory(receive, "TRB", data.Whs1.FindLocation("B-1-1-2").PK, data.Part1.PK);
				SetInventory(receive, "TRB", data.Whs1.FindLocation("B-1-1-1").PK, data.Part1.PK);
				SetInventory(receive, "ANY", data.Whs1.FindLocation("C-1-1-1").PK, data.Part1.PK);

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);

				var pick = Helper.CreatePickNew(order);

				foreach (var pickLine in pick.GetAllPickLines().ToArray())
				{
					Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				}

				var pickLines = pick.GetAllPickLines().ToArray();
				Array.Sort(pickLines, new SortPickLinesForPickingSlip());

				int index = 0;
				AssertEquals("Collection should have right Count", 3, pickLines.Length);
				AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "C-1-1-1", "P1"),
					pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
				AssertEquals(GetInventoryWithParameters(pickLines, "111", "TRB", 0, 0, 0, "B-1-1-1", "P1"),
					pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
				AssertEquals(GetInventoryWithParameters(pickLines, "111", "TRB", 0, 0, 0, "B-1-1-2", "P1"),
					pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			}
		}

		public void TestSortPickLinesForPickingSlip_InTransit_RowSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			data.Whs1.Rows.Single(r => r.WR_Name == "A").WR_Name = "Z";
			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "A", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2, 2);
			var row3 = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 2, 2, rowSequence: 2);
			var row4 = Helper.CreateRowAndGenerateLocations(data.Whs1, "D", 2, 2, 2, rowSequence: 1);
			var row5 = Helper.CreateRowAndGenerateLocations(data.Whs1, "E", 2, 2, 2, rowSequence: 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Factory.Save();

			SetInventory(receive, "ANY", data.Whs1.FindLocation("B-1-1-1").PK, data.Part1.PK);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("A-1-1-1").PK, data.Part1.PK);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("C-1-1-1").PK, data.Part1.PK);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("D-1-1-1").PK, data.Part1.PK);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("E-1-1-1").PK, data.Part1.PK);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);

			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			}

			var pickLines = pick.GetAllPickLines().ToArray();
			Array.Sort(pickLines, new SortPickLinesForPickingSlip());

			int index = 0;
			AssertEquals("Collection should have right Count", 5, pickLines.Length);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 0, "D-1-1-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 0, "E-1-1-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 2, 0, "C-1-1-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "A-1-1-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 0, "B-1-1-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
		}

		public void TestSortPickLinesForPickingSlip_InTransit_PickPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "D", 2, 2, 2, rowSequence: 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Factory.Save();

			SetInventory(receive, "ANY", data.Whs1.FindLocation("B-1-1-1").PK, data.Part1.PK, pickPathSequence: 1);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("D-1-1-1").PK, data.Part1.PK, pickPathSequence: 2);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("D-1-1-2").PK, data.Part1.PK, pickPathSequence: 1);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("D-2-1-2").PK, data.Part1.PK, pickPathSequence: 3);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("D-2-1-1").PK, data.Part1.PK, pickPathSequence: 3);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);

			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			}

			var pickLines = pick.GetAllPickLines().ToArray();
			Array.Sort(pickLines, new SortPickLinesForPickingSlip());

			int index = 0;
			AssertEquals("Collection should have right Count", 5, pickLines.Length);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 1, "D-1-1-2", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 2, "D-1-1-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 3, "D-2-1-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 1, 3, "D-2-1-2", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 1, "B-1-1-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
		}

		public void TestSortPickLinesForPickingSlip_InTransit_ColumnLevelTray()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2, 2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Factory.Save();

			SetInventory(receive, "ANY", data.Whs1.FindLocation("B-2-2-2").PK, data.Part1.PK, pickPathSequence: 1);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("B-2-2-1").PK, data.Part1.PK, pickPathSequence: 1);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("B-1-2-2").PK, data.Part1.PK, pickPathSequence: 1);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("B-1-2-1").PK, data.Part1.PK, pickPathSequence: 1);
			SetInventory(receive, "ANY", data.Whs1.FindLocation("B-1-1-1").PK, data.Part1.PK, pickPathSequence: 1);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			var pick = Helper.CreatePickNew(order);

			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			}

			var pickLines = pick.GetAllPickLines().ToArray();
			Array.Sort(pickLines, new SortPickLinesForPickingSlip());

			int index = 0;
			AssertEquals("Collection should have right Count", 5, pickLines.Length);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 1, "B-1-1-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 1, "B-1-2-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 1, "B-1-2-2", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 1, "B-2-2-1", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
			AssertEquals(GetInventoryWithParameters(pickLines, "111", "ANY", 0, 0, 1, "B-2-2-2", "P1"),
				pickLines[index++].InventoryLineForAvailableInventory.Inventory[0]);
		}

		public void TestDuplicateSortingInSQL()
		{
			var pick = SetupForSortPickLines();

			var sqlPickLines = new DynamicBusinessObjectCollection(Factory);
			var parameters = new List<ZSqlParameter>();
			var query = SortingInSQL_BuildSql(parameters);
			sqlPickLines.Load(query, parameters.ToArray());

			var sqlPickLinesPks = sqlPickLines.Select(o => (ZGuid)o[WhsPickLineSchema.PK]).ToList();
			var wsPickLines = pick.GetAllPickLines().ToArray();
			Array.Sort(wsPickLines, new SortPickLinesForPickingSlip());

			var wsPickLinesPks = wsPickLines.Select(x => x.PK).ToList();

			StandardAssertsCorrectOrder(sqlPickLinesPks, wsPickLinesPks);
		}

		static string SortingInSQL_BuildSql(List<ZSqlParameter> parameters)
		{
			var pickMethods = WarehouseDataRegistry.Instance.PickMethod.Value;
			var pickMethodsSql = new ZStringBuilder();
			var codes = pickMethods.GetCodeDescriptionPairList().GetAllCodesZString();

			if (codes.Length > 0)
			{
				pickMethodsSql.Append("INSERT INTO @PickMethods VALUES ");

				for (int index = 0; index < codes.Length; index++)
				{
					var key = codes[index];
					var codeParameterName =
						string.Format(CultureInfo.InvariantCulture, "@PickMethod{0}",
							index); // internal, for parameter forming
					var descParameterName =
						string.Format(CultureInfo.InvariantCulture, "@PickMethodDesc{0}",
							index); // internal, for parameter forming
					pickMethodsSql.Append(string.Format(CultureInfo.InvariantCulture, "({0}, {1})", codeParameterName,
						descParameterName));

					if (index < codes.Length - 1)
					{
						pickMethodsSql.Append(", ");
					}

					parameters.Add(ZSqlParameter.New(codeParameterName, key, DummyBizoSchema.Z0_Code));
					parameters.Add(ZSqlParameter.New(descParameterName, pickMethods.GetDescriptionFromCode(key),
						DummyBizoSchema.Z0_NVarCharMax));
				}
			}

			return string.Format(CultureInfo.InvariantCulture, SortingInSQL_Sql, pickMethodsSql.ToString(),
				SortPickLinesForPickingSlip.EquivalentOrderBySql);
		}

		const string SortingInSQL_Sql =
			@"DECLARE @PickMethods TABLE
(
	Code NVARCHAR(5) UNIQUE,
	PickMethodDesc NVARCHAR(max)
)

{0}

; WITH PickLines AS
(
SELECT
	WP_PK,
	WZ_PK,
	OH_Code,
	OrderLine.WE_PickGroup,
	WR_PickPathSequence,
	WLV_RowName,
	WLV_Column,
	WLV_Level,
	WLV_Tray,
	WLV_PickPathSequence,
	WZ_GS_NKAssignedTo as AssignedTo,
	WZ_Units,
	WLT_IsPalletIDNeutral as IsPalletIDNeutral,
	InventoryLine.WE_PalletID AS PalletID,
	OrderLine.WE_PartAttrib1 AS OrderedPartAttrib1,
	OrderLine.WE_PartAttrib2 AS OrderedPartAttrib2,
	OrderLine.WE_PartAttrib3 AS OrderedPartAttrib3,
	OrderLine.WE_SerialNumber AS OrderedSerialNumber,
	OrderLine.WE_ExpiryDate AS OrderedExpiryDate,
	OrderLine.WE_PackingDate AS OrderedPackingDate,
	OP_PartNum,
	PickMethodDesc
FROM
	dbo.WhsPick
	JOIN dbo.WhsDocket ON WD_WP = WP_PK
	JOIN dbo.WhsDocketLine AS OrderLine ON WE_WD = WD_PK
	JOIN dbo.WhsPickLine ON WZ_WE_TransactionLine = OrderLine.WE_PK
	JOIN dbo.WhsDocketLine AS InventoryLine ON InventoryLine.WE_PK = ISNULL(WZ_WE_OriginalPickedInventoryLine, WZ_WE_InventoryLine)
	LEFT JOIN dbo.WhsDocketLine AS PutawayLine ON PutawayLine.WE_PK = WZ_WE_InventoryLine AND WZ_WE_OriginalPickedInventoryLine IS NOT NULL AND OrderLine.WE_DocketLineType = 'ORD' -- Dont support DDL putaway for work order
	JOIN dbo.OrgSupplierPart ON OrderLine.WE_OP = OP_PK
	JOIN dbo.OrgHeader ON OH_PK = WD_OH_Client
	JOIN dbo.WhsLocation ON InventoryLine.WE_WL = WL_PK
	JOIN dbo.WhsLocationType ON WLT_PK = WL_WLT_LocationType
	JOIN dbo.WhsLocationView ON InventoryLine.WE_WL = WLV_PK
	JOIN dbo.WhsRow ON WL_WR = WR_PK
	JOIN @PickMethods ON Code = WLV_PickMethod
)

SELECT
ROW_NUMBER() OVER
(
	ORDER BY
		{1}
) AS RunningPK,
WZ_PK

FROM
PickLines

ORDER BY
	RunningPK ASC";

		#region TestSortPickLinesForPickingSlip_SortInvalidLocations

		public void TestSortPickLinesForPickingSlip_SortInvalidLocations_EmptyGuid()
		{
			TestSortPickLinesForPickingSlip_SortInvalidLocationsCore("Sorting shouldn't be an issue.",
				(whs) => ZGuid.Empty, (whs) => ZGuid.Empty, expectSameOrder: true);
		}

		public void TestSortPickLinesForPickingSlip_SortInvalidLocations_EmptyAndInvalidGuid()
		{
			TestSortPickLinesForPickingSlip_SortInvalidLocationsCore("Sorting shouldn't be an issue.",
				(whs) => ZGuid.Empty, (whs) => ZGuid.Invalid, expectSameOrder: true);
		}

		public void TestSortPickLinesForPickingSlip_SortInvalidLocations_FirstInventoryInvalid()
		{
			TestSortPickLinesForPickingSlip_SortInvalidLocationsCore(
				"The line with the location should be sorted and moved to the top of the list.", (whs) => ZGuid.Empty,
				(whs) => whs.FindLocation("A-1-1-1").PK, expectSameOrder: false);
		}

		public void TestSortPickLinesForPickingSlip_SortInvalidLocations_SecondInventoryInvalid()
		{
			TestSortPickLinesForPickingSlip_SortInvalidLocationsCore(
				"The line with the location should be sorted and moved to the top of the list.",
				(whs) => whs.FindLocation("A-1-1-1").PK, (whs) => ZGuid.Empty, expectSameOrder: true);
		}

		void TestSortPickLinesForPickingSlip_SortInvalidLocationsCore(string msg, Func<WhsWarehouse, ZGuid> location1PK,
			Func<WhsWarehouse, ZGuid> location2PK, bool expectSameOrder)
		{
			var today = ZDateTimeOffset.Today;
			var availableInventories = new WhsPickAvailableInventory[2];
			var sortedAvailableInventories = new WhsPickAvailableInventoryCollection(Factory);
			var client = Helper.CreateClient("Client");
			var part = Helper.CreateProduct(client, "P1");
			var whs = Helper.CreateWarehouse("Warehouse");
			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			Factory.Save();
			var order = Helper.CreateWhsOrder(client, whs);
			availableInventories[0] = sortedAvailableInventories.AddNew();
			availableInventories[1] = sortedAvailableInventories.AddNew();

			SetPickAvailableInventory(availableInventories[0], order, new TestILineAttributes(), today,
				location1PK(whs), part.PK);
			SetPickAvailableInventory(availableInventories[1], order, new TestILineAttributes(), today,
				location2PK(whs), part.PK);

			sortedAvailableInventories.Sort(new SortPickInventoryForPicking());

			if (expectSameOrder)
			{
				AssertContainsExactElementsInExactOrder(msg, sortedAvailableInventories, availableInventories[0],
					availableInventories[1]);
			}
			else
			{
				AssertContainsExactElementsInExactOrder(msg, sortedAvailableInventories, availableInventories[1],
					availableInventories[0]);
			}
		}

		#endregion

		#endregion

		#region SortTransferLinesForPicking

		public void TestSortTransferLinesForPicking()
		{
			const int linesCount = 10;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var testHeader = Helper.CreateClient();

			var whs = Helper.CreateWarehouse("Warehouse");
			var transfer = Helper.CreateWhsTransfer(testHeader, whs);

			var lines = new WhsTransferLine[linesCount];
			var sortedLines = new List<WhsTransferLine>();

			transfer.WD_WW_Whs = whs.PK;
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2, rowSequence: 1);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2, rowSequence: 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 12, 2, rowSequence: 3);
			row3.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			for (int i = 0; i < linesCount; i++)
			{
				lines[i] = transfer.Lines.AddNew();
				sortedLines.Add(lines[i]);
			}

			SetTransferLine_TransferFromLocAndPartPK(lines[0], whs.FindLocation("A-1-11-1").PK, data.Part1.PK);
			SetTransferLine_TransferFromLocAndPartPK(lines[1], whs.FindLocation("A-1-2-1").PK, data.Part1.PK);
			SetTransferLine_TransferFromLocAndPartPK(lines[2], whs.FindLocation("A-2-1-1").PK, data.Part1.PK);
			SetTransferLine_TransferFromLocAndPartPK(lines[3], whs.FindLocation("A-1-1-1").PK, data.Part1.PK);
			SetTransferLine_TransferFromLocAndPartPK(lines[4], whs.FindLocation("C-2-2-2").PK, data.Part1.PK);
			SetTransferLine_TransferFromLocAndPartPK(lines[5], whs.FindLocation("C-2-2-1").PK, data.Part1.PK);
			SetTransferLine_TransferFromLocAndPartPK(lines[6], whs.FindLocation("C-1-2-2").PK, data.Part1.PK);
			SetTransferLine_TransferFromLocAndPartPK(lines[7], whs.FindLocation("C-1-1-1").PK, data.Part1.PK);
			SetTransferLine_TransferFromLocAndPartPK(lines[8], whs.FindLocation("B-1-1-1").PK, data.Part2.PK);
			SetTransferLine_TransferFromLocAndPartPK(lines[9], whs.FindLocation("B-1-1-1").PK, data.Part1.PK);

			sortedLines.Sort(new SortTransferLinesForPicking());

			StandardAsserts(lines, sortedLines);
		}

		public void TestSortTransferLinesForPicking_AlsoConsidersPickMethod()
		{
			const int linesCount = 10;

			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var testHeader = Helper.CreateClient();

			var whs = Helper.CreateWarehouse("Warehouse");
			var transfer = Helper.CreateWhsTransfer(testHeader, whs);

			var lines = new WhsTransferLine[linesCount];
			var sortedLines = new List<WhsTransferLine>();

			transfer.WD_WW_Whs = whs.PK;
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2, rowSequence: 1);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2, rowSequence: 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2, rowSequence: 3);
			row3.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			// Setup Registry
			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			var pickMethod = pickMethods.AddNew();
			pickMethod.Code = "TRB";
			pickMethod.Bool = true;
			pickMethod.Description = (NoResString)"Tractor Beam";
			Factory.Save();

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					   pickMethods))
			{
				var locations = new WhsLocation[]
				{
					whs.FindLocation("A-1-2-1"), whs.FindLocation("C-2-2-1"), whs.FindLocation("A-2-2-1"),
					whs.FindLocation("A-2-1-1"), whs.FindLocation("A-1-1-1"), whs.FindLocation("C-2-1-1"),
					whs.FindLocation("C-1-2-1"), whs.FindLocation("C-1-1-1"), whs.FindLocation("B-2-1-1"),
					whs.FindLocation("B-1-1-1")
				};

				locations[0].WLV_PickMethod = "TRB";
				locations[1].WLV_PickMethod = "TRB";

				for (int i = 0; i < linesCount; i++)
				{
					lines[i] = transfer.Lines.AddNew();
					sortedLines.Add(lines[i]);
					SetTransferLine_TransferFromLocAndPartPK(lines[i], locations[i].PK, data.Part1.PK);
				}

				sortedLines.Sort(new SortTransferLinesForPicking());

				StandardAsserts(lines, sortedLines);
			}
		}

		public void TestSortTransferLinesForPicking_FollowsRowThenPickPath()
		{
			const int linesCount = 10;

			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var testHeader = Helper.CreateClient();

			var whs = Helper.CreateWarehouse("Warehouse");
			var transfer = Helper.CreateWhsTransfer(testHeader, whs);

			var lines = new WhsTransferLine[linesCount];
			var sortedLines = new List<WhsTransferLine>();

			transfer.WD_WW_Whs = whs.PK;
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2, rowSequence: 1);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2, rowSequence: 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2, rowSequence: 3);
			row3.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			// Setup Registry
			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			var pickMethod = pickMethods.AddNew();
			pickMethod.Code = "TRB";
			pickMethod.Bool = true;
			pickMethod.Description = (NoResString)"Tractor Beam";
			Factory.Save();

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					   pickMethods))
			{
				var locations = new WhsLocation[]
				{
					whs.FindLocation("A-2-2-1"), whs.FindLocation("A-1-2-1"), whs.FindLocation("A-2-1-1"),
					whs.FindLocation("A-1-1-1"), whs.FindLocation("C-1-1-1"), whs.FindLocation("C-2-1-1"),
					whs.FindLocation("C-2-2-1"), whs.FindLocation("C-1-2-1"), whs.FindLocation("B-2-1-1"),
					whs.FindLocation("B-1-1-1")
				};

				locations[4].WLV_PickPathSequence = 10;
				locations[5].WLV_PickPathSequence = 9;
				locations[6].WLV_PickPathSequence = 8;
				locations[7].WLV_PickPathSequence = 7;

				for (int i = 0; i < linesCount; i++)
				{
					lines[i] = transfer.Lines.AddNew();
					sortedLines.Add(lines[i]);
					SetTransferLine_TransferFromLocAndPartPK(lines[i], locations[i].PK, data.Part1.PK);
				}

				sortedLines.Sort(new SortTransferLinesForPicking());

				StandardAsserts(lines, sortedLines);
			}
		}

		#endregion

		#region TestSortTransferLinesForPutaway

		public void TestSortTransferLinesForPutaway()
		{
			const int linesCount = 10;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var testHeader = Helper.CreateClient();

			var whs = Helper.CreateWarehouse("Warehouse");
			var transfer = Helper.CreateWhsTransfer(testHeader, whs);

			var lines = new WhsTransferLine[linesCount];
			var sortedLines = new List<WhsTransferLine>();

			transfer.WD_WW_Whs = whs.PK;
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 3, 2);
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			var destinationLocations = new WhsLocation[]
			{
				whs.FindLocation("A-1-3-1"), whs.FindLocation("A-1-2-1"), whs.FindLocation("A-2-1-1"),
				whs.FindLocation("A-1-1-1"), whs.FindLocation("C-2-2-2"), whs.FindLocation("C-2-2-1"),
				whs.FindLocation("C-1-2-2"), whs.FindLocation("C-1-1-1"), whs.FindLocation("B-1-1-1"),
				whs.FindLocation("B-1-1-1")
			};

			for (var j = 0; j < linesCount; j++)
			{
				destinationLocations[j].WLV_PutawayPathSequence = linesCount - j;
			}

			Factory.Save();

			for (var i = 0; i < linesCount; i++)
			{
				lines[i] = transfer.Lines.AddNew();
				sortedLines.Add(lines[i]);
			}

			SetTransferLine_LocationAndPartPK(lines[0], whs.FindLocation("A-1-3-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[1], whs.FindLocation("A-1-2-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[2], whs.FindLocation("A-2-1-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[3], whs.FindLocation("A-1-1-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[4], whs.FindLocation("C-2-2-2").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[5], whs.FindLocation("C-2-2-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[6], whs.FindLocation("C-1-2-2").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[7], whs.FindLocation("C-1-1-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[8], whs.FindLocation("B-1-1-1").PK, data.Part2.PK);
			SetTransferLine_LocationAndPartPK(lines[9], whs.FindLocation("B-1-1-1").PK, data.Part1.PK);

			sortedLines.Sort(new SortTransferLinesForPutaway());
			AssertEquals("Lines are in correct order.", "4, 3, 2, 1, 10, 9, 8, 7, 6, 5",
				string.Join(", ", sortedLines.Select(l => l.WE_LineNo).Select(p => p)));
		}

		public void TestSortTransferLinesForPutaway_MissingLocations()
		{
			const int linesCount = 10;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var testHeader = Helper.CreateClient();

			var whs = Helper.CreateWarehouse("Warehouse");
			var transfer = Helper.CreateWhsTransfer(testHeader, whs);

			var lines = new WhsTransferLine[linesCount];
			var sortedLines = new List<WhsTransferLine>();

			transfer.WD_WW_Whs = whs.PK;
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 3, 2);
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			var destinationLocations = new WhsLocation[]
			{
				whs.FindLocation("A-1-3-1"), whs.FindLocation("A-1-2-1"), whs.FindLocation("A-2-1-1"),
				whs.FindLocation("A-1-1-1"), whs.FindLocation("C-2-2-2"), whs.FindLocation("C-2-2-1"),
				whs.FindLocation("C-1-2-2"), whs.FindLocation("C-1-1-1"), whs.FindLocation("B-1-1-1"),
				whs.FindLocation("B-1-1-1")
			};

			for (var j = 0; j < linesCount; j++)
			{
				destinationLocations[j].WLV_PutawayPathSequence = linesCount - j;
			}

			Factory.Save();

			for (var i = 0; i < linesCount; i++)
			{
				lines[i] = transfer.Lines.AddNew();
				sortedLines.Add(lines[i]);
			}

			SetTransferLine_LocationAndPartPK(lines[0], ZGuid.Empty, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[1], ZGuid.Empty, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[2], ZGuid.Empty, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[3], whs.FindLocation("A-1-1-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[4], whs.FindLocation("C-2-2-2").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[5], whs.FindLocation("C-2-2-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[6], whs.FindLocation("C-1-2-2").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[7], whs.FindLocation("C-1-1-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[8], whs.FindLocation("A-1-1-1").PK, data.Part1.PK);
			SetTransferLine_LocationAndPartPK(lines[9], ZGuid.Empty, data.Part1.PK);

			sortedLines.Sort(new SortTransferLinesForPutaway());
			AssertEquals("PalletIds of lines are in the correct order, so lines are in correct order.",
				"4, 9, 8, 7, 6, 5, 1, 2, 3, 10",
				string.Join(", ", sortedLines.Select(l => l.WE_LineNo).Select(p => p)));
		}

		#endregion

		#region SortStocktakeLinesForPicking

		public void TestSortStocktakeLinesForPicking()
		{
			const int linesCount = 10;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var stocktake = Factory.New<WhsStocktake>();
			var lines = new WhsStocktakeLine[linesCount];
			var sortedLines = new WhsStocktakeLineCollection(stocktake);
			var whs = Helper.CreateWarehouse("Warehous");
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2, rowSequence: 1);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2, rowSequence: 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 12, 2, rowSequence: 3);
			row3.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			for (int i = 0; i < linesCount; i++)
			{
				lines[i] = sortedLines.AddNew();
			}

			SetStocktakeLine(lines[0], whs.FindLocation("A-1-11-1").PK, data.Part1.PK);
			SetStocktakeLine(lines[1], whs.FindLocation("A-1-2-1").PK, data.Part1.PK);
			SetStocktakeLine(lines[2], whs.FindLocation("A-2-1-1").PK, data.Part1.PK);
			SetStocktakeLine(lines[3], whs.FindLocation("A-1-1-1").PK, data.Part1.PK);
			SetStocktakeLine(lines[4], whs.FindLocation("C-2-2-2").PK, data.Part1.PK);
			SetStocktakeLine(lines[5], whs.FindLocation("C-2-2-1").PK, data.Part1.PK);
			SetStocktakeLine(lines[6], whs.FindLocation("C-1-2-2").PK, data.Part1.PK);
			SetStocktakeLine(lines[7], whs.FindLocation("C-1-1-1").PK, data.Part1.PK);
			SetStocktakeLine(lines[8], whs.FindLocation("B-1-1-1").PK, data.Part2.PK);
			SetStocktakeLine(lines[9], whs.FindLocation("B-1-1-1").PK, data.Part1.PK);

			sortedLines.ApplySort(new SortStocktakeLines());

			StandardAsserts(lines, sortedLines);
		}

		public void TestSortStocktakeLinesForPicking_AlsoConsidersPickMethod()
		{
			const int linesCount = 10;

			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var stocktake = Factory.New<WhsStocktake>();
			var lines = new WhsStocktakeLine[linesCount];
			var sortedLines = new WhsStocktakeLineCollection(stocktake);
			var whs = Helper.CreateWarehouse("Warehouse");

			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2, rowSequence: 1);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2, rowSequence: 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2, rowSequence: 3);
			row3.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			// Setup Registry
			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			var pickMethod1 = pickMethods.AddNew();
			pickMethod1.Code = "ARB";
			pickMethod1.Bool = true;
			pickMethod1.Description = (NoResString)"Andrii's Robot";

			var pickMethod2 = pickMethods.AddNew();
			pickMethod2.Code = "TRB";
			pickMethod2.Bool = true;
			pickMethod2.Description = (NoResString)"Tractor Beam";
			Factory.Save();

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					   pickMethods))
			{
				// Expect B then C then A generally, setup backwards
				var locations = new WhsLocation[]
				{
					// Set second pick method on the following lines ("Tractor Beam")
					whs.FindLocation("A-1-2-1"), whs.FindLocation("C-1-2-1"), whs.FindLocation("B-2-1-1"),

					// Set no pick method on the following lines ("Any")
					whs.FindLocation("A-2-1-1"), whs.FindLocation("A-1-1-1"), whs.FindLocation("C-2-1-1"),
					whs.FindLocation("C-1-1-1"), whs.FindLocation("B-1-1-1"),

					// Set pick method on the following lines ("Andrii's Robot")
					whs.FindLocation("A-2-2-1"), whs.FindLocation("C-2-2-1")
				};

				locations[0].WLV_PickMethod = "TRB";
				locations[1].WLV_PickMethod = "TRB";
				locations[2].WLV_PickMethod = "TRB";

				locations[8].WLV_PickMethod = "ARB";
				locations[9].WLV_PickMethod = "ARB";

				for (int i = 0; i < linesCount; i++)
				{
					lines[i] = sortedLines.AddNew();
					SetStocktakeLine(lines[i], locations[i].PK, data.Part1.PK);
				}

				sortedLines.ApplySort(new SortStocktakeLines());

				StandardAsserts(lines, sortedLines);
			}
		}

		public void TestSortStocktakeLinesForPicking_DoesNotConsiderClient()
		{
			const int linesCount = 10;

			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var stocktake = Factory.New<WhsStocktake>();
			var lines = new WhsStocktakeLine[linesCount];
			var sortedLines = new WhsStocktakeLineCollection(stocktake);
			var whs = Helper.CreateWarehouse("Warehouse");
			var client1 = Helper.CreateClient("ZZZ");
			var client2 = Helper.CreateClient("YYY");

			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2, rowSequence: 1);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2, rowSequence: 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2, rowSequence: 3);
			row3.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			// Expect B then C then A generally, setup backwards
			var locations = new WhsLocation[]
			{
				whs.FindLocation("A-2-2-1"), whs.FindLocation("A-1-2-1"), whs.FindLocation("A-2-1-1"),
				whs.FindLocation("A-1-1-1"), whs.FindLocation("C-2-2-1"), whs.FindLocation("C-2-1-1"),
				whs.FindLocation("C-1-2-1"), whs.FindLocation("C-1-1-1"), whs.FindLocation("B-2-1-1"),
				whs.FindLocation("B-1-1-1")
			};

			for (int i = 0; i < linesCount; i++)
			{
				lines[i] = sortedLines.AddNew();
				SetStocktakeLine(lines[i], locations[i].PK, data.Part1.PK);
			}

			lines[0].WU_OH_Client = client1.PK;
			lines[3].WU_OH_Client = client1.PK;
			lines[5].WU_OH_Client = client1.PK;

			lines[7].WU_OH_Client = client2.PK;
			lines[9].WU_OH_Client = client2.PK;

			sortedLines.ApplySort(new SortStocktakeLines());

			StandardAsserts(lines, sortedLines);
		}

		#region TestSortComparerWhenClientAndProductIsNull

		public void TestSortComparerWhenClientAndProductIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var stocktake = Factory.New<WhsStocktake>();
			var lines = new WhsStocktakeLine[4];
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("A-1-1"));
			var emptyLine = Helper.CreateWhsStocktakeLine(stocktake, null, null, data.Whs1.FindLocation("A-1-2"));
			AssertNoExceptionThrown(() => new SortStocktakeLines().Compare(line, emptyLine));
		}

		#endregion

		#endregion

		#region SortByProduct

		public void TestSortByProduct()
		{
			int linesCount = 2;
			TestDataSimpleEnvironment data = new TestDataSimpleEnvironment(Factory, 2, 2);
			WhsStocktake stocktake = Factory.New<WhsStocktake>();
			WhsStocktakeLine[] lines = new WhsStocktakeLine[linesCount];
			WhsStocktakeLineCollection sortedLines = new WhsStocktakeLineCollection(stocktake);
			for (int i = 0; i < linesCount; i++)
			{
				lines[i] = sortedLines.AddNew();
			}

			SetStocktakeLine(lines[0], ZGuid.Empty, data.Part2.PK);
			SetStocktakeLine(lines[1], ZGuid.Empty, data.Part1.PK);

			sortedLines.ApplySort(new SortByProduct());

			StandardAsserts(lines, sortedLines);
		}

		#endregion

		#region SortInventoryByProduct

		public void TestSortInventoryByProduct()
		{
			int linesCount = 4;
			var client = Helper.CreateClient();
			var part1 = Helper.CreateProduct(client, "Part1");
			var part2 = Helper.CreateProduct(client, "Part2");
			var part3 = Helper.CreateProduct(client, "Part3");
			var part4 = Helper.CreateProduct(client, "Part4");
			var lines = new WhsInventoryView[linesCount];
			var sortedLines = new WhsInventoryViewCollection(Factory);
			for (int i = 0; i < linesCount; i++)
			{
				lines[i] = sortedLines.AddNew();
			}

			lines[0].WI_OP = part4.PK;
			lines[1].WI_OP = part3.PK;
			lines[2].WI_OP = part2.PK;
			lines[3].WI_OP = part1.PK;

			sortedLines.Sort(new SortInventoryByProduct());
			StandardAsserts(lines, sortedLines);
		}

		#endregion

		#region TestSortPickLinesByLocationAndPalletID

		public void TestSortPickLinesByLocationAndPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var part1 = Helper.CreateProduct(data.Org1, "Part1");
			var part2 = Helper.CreateProduct(data.Org1, "Part2");
			var part3 = Helper.CreateProduct(data.Org1, "Part3");
			var part4 = Helper.CreateProduct(data.Org1, "Part4");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Factory.Save();

			// Part 1 60
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part1.PK, palletID: "");
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part1.PK, palletID: "Pallet4", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part1.PK, palletID: "Pallet1", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-2").PK, part1.PK, palletID: "", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-2").PK, part1.PK, palletID: "Pallet2", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-3").PK, part1.PK, palletID: "Pallet3", 0);

			// Part 2 70
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part2.PK, palletID: "Pallet4", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part2.PK, palletID: "Pallet1", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part2.PK, palletID: "Pallet4", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-2").PK, part2.PK, palletID: "", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-2").PK, part2.PK, palletID: "Pallet2", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-3").PK, part2.PK, palletID: "", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-3").PK, part2.PK, palletID: "Pallet3", 0);

			// Part 3 60
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part3.PK, palletID: "", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-3").PK, part3.PK, palletID: "Pallet3", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part3.PK, palletID: "Pallet1", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-3").PK, part3.PK, palletID: "Pallet3", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part3.PK, palletID: "Pallet1", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-3").PK, part3.PK, palletID: "", 0);

			// Part 4 20
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-1").PK, part4.PK, palletID: "Pallet1", 0);
			SetInventory(receive1, "ANY", data.Whs1.FindLocation("A-1-2").PK, part4.PK, palletID: "", 0);

			receive1.FinaliseDocket();
			AssertEquals("Receive1 could not be finalised", true, receive1.IsFinalised);
			Factory.Save();

			// Setup Orders / Pick
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD33");
			var orderLine1 = Helper.CreateWhsOrderLine(order, part1, 60m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, part2, 40m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, part4, 20m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, part3, 20m);
			var orderLine5 = Helper.CreateWhsOrderLine(order, part2, 30m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD93");
			var orderLine21 = Helper.CreateWhsOrderLine(order2, part3, 40m);

			var pick = Helper.CreatePickNew(order, order2);
			Factory.Save();

			var sqlPickLines = new DynamicBusinessObjectCollection(Factory);
			var parameters = new List<ZSqlParameter>();
			var query = SortingInSQL_BuildSql(parameters);
			sqlPickLines.Load(query, parameters.ToArray());
			var sqlPickLinesPks = sqlPickLines.Select(o => (ZGuid)o[WhsPickLineSchema.PK]).ToList();

			AssertEquals("Collection should have right Count", 21, sqlPickLinesPks.Count);

			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[0], "111", "ANY", "A-1-1", "Pallet4",
				"PART1");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[1], "111", "ANY", "A-1-1", "Pallet4",
				"PART2");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[2], "111", "ANY", "A-1-1", "Pallet4",
				"PART2");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[3], "111", "ANY", "A-1-1", "Pallet1",
				"PART1");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[4], "111", "ANY", "A-1-1", "Pallet1",
				"PART2");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[5], "111", "ANY", "A-1-1", "Pallet1",
				"PART3");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[6], "111", "ANY", "A-1-1", "Pallet1",
				"PART3");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[7], "111", "ANY", "A-1-1", "Pallet1",
				"PART4");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[8], "111", "ANY", "A-1-1", "", "PART1");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[9], "111", "ANY", "A-1-1", "", "PART3");

			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[10], "111", "ANY", "A-1-2", "Pallet2",
				"PART1");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[11], "111", "ANY", "A-1-2", "Pallet2",
				"PART2");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[12], "111", "ANY", "A-1-2", "", "PART1");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[13], "111", "ANY", "A-1-2", "", "PART2");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[14], "111", "ANY", "A-1-2", "", "PART4");

			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[15], "111", "ANY", "A-1-3", "Pallet3",
				"PART1");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[16], "111", "ANY", "A-1-3", "Pallet3",
				"PART2");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[17], "111", "ANY", "A-1-3", "Pallet3",
				"PART3");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[18], "111", "ANY", "A-1-3", "Pallet3",
				"PART3");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[19], "111", "ANY", "A-1-3", "", "PART2");
			AssertInventoryWithParametersIncludingPalletID(sqlPickLinesPks[20], "111", "ANY", "A-1-3", "", "PART3");
		}

		void AssertInventoryWithParametersIncludingPalletID(ZGuid pickLinePK, ZString clientCode, ZString pickMethod,
			ZString locationString, ZString palletID, ZString productCode)
		{
			var pickLineInventory = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, pickLinePK)).Single()
				.InventoryLineForAvailableInventory;
			AssertEquals("ClientCode should be correct.", clientCode, pickLineInventory.Docket.Client.OH_Code);
			AssertEquals("PickMethod should be correct.", pickMethod, pickLineInventory.Location.WLV_PickMethod);
			AssertEquals("LocationString should be correct.", locationString,
				pickLineInventory.Location.WLV_LocationString);
			AssertEquals("PalletID should be correct.", palletID, pickLineInventory.WE_PalletID);
			AssertEquals("ProductCode should be correct.", productCode, pickLineInventory.ProductCode);
		}

		#endregion

		#region TestSortPalletIDNeutral

		#region TestSortPalletIDNeutralByAttributes

		public void TestSortPalletIDNeutralByAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			data.Whs1.LocationType.WLT_IsPalletIDNeutral = true;

			var part = CreateProductWithAttributes(data.Org1, "PART");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(1);
			var packingDate = ZDate.Today.AddDays(1);
			var attribVal1 = "Att1";
			var attribVal2 = "Att2";

			CreateInventory(receive, expiryDate, packingDate, attribVal1, "", "", "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet");
			CreateInventory(receive, expiryDate, packingDate, "", "", attribVal2, "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet");
			CreateInventory(receive, expiryDate, packingDate, attribVal2, "", attribVal1, "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet");
			CreateInventory(receive, expiryDate, packingDate, "", attribVal2, "", "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet");
			CreateInventory(receive, expiryDate, packingDate, attribVal1, "", attribVal1, "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet");
			CreateInventory(receive, expiryDate, packingDate, "", "", attribVal1, "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet");
			CreateInventory(receive, expiryDate, packingDate, "", attribVal1, "", "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet");
			CreateInventory(receive, expiryDate, packingDate, attribVal2, "", attribVal2, "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet");
			receive.FinaliseDocket();
			AssertEquals("Receive could not be finalised", true, receive.IsFinalised);
			Factory.Save();

			var orderLine1 = Helper.CreateWhsOrderLine(order, part, 20m);
			orderLine1.WE_PartAttrib1 = attribVal2;

			var orderLine2 = Helper.CreateWhsOrderLine(order, part, 20m);
			orderLine2.WE_PartAttrib1 = attribVal1;

			var orderLine3 = Helper.CreateWhsOrderLine(order, part, 40m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var sqlPickLines = new DynamicBusinessObjectCollection(Factory);
			var parameters = new List<ZSqlParameter>();
			var query = SortingInSQL_BuildSql(parameters);
			sqlPickLines.Load(query, parameters.ToArray());
			var sqlPickLinesPks = sqlPickLines.Select(o => (ZGuid)o[WhsPickLineSchema.PK]).ToList();

			AssertEquals("Collection should have right Count", 8, sqlPickLinesPks.Count);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(0, 4), "PART", 10m, "", "", "", "", ZDate.Empty,
				ZDate.Empty);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(4, 2), "PART", 10m, attribVal1, "", "", "",
				ZDate.Empty, ZDate.Empty);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(6, 2), "PART", 10m, attribVal2, "", "", "",
				ZDate.Empty, ZDate.Empty);
		}

		public void TestSortPalletIDNeutralByAttributes_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			data.Whs1.LocationType.WLT_IsPalletIDNeutral = true;

			var part = CreateProductWithAttributes(data.Org1, "PART");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, part, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(1);
			var packingDate = ZDate.Today.AddDays(1);
			var attribVal1 = "Att1";
			var attribVal2 = "Att2";

			CreateInventory(receive, expiryDate, packingDate, attribVal1, "", "", "SN1",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet", qty: 1m);
			CreateInventory(receive, expiryDate, packingDate, attribVal2, "", "", "SN2",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet", qty: 1m);
			CreateInventory(receive, expiryDate, packingDate, attribVal1, "", "", "SN3",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet", qty: 1m);
			CreateInventory(receive, expiryDate, packingDate, attribVal2, "", "", "SN4",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet", qty: 1m);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var orderLine1 = Helper.CreateWhsOrderLine(order, part, 1m);
			orderLine1.WE_SerialNumber = "SN2";

			var orderLine2 = Helper.CreateWhsOrderLine(order, part, 1m);
			orderLine2.WE_SerialNumber = "SN3";

			var orderLine3 = Helper.CreateWhsOrderLine(order, part, 2m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var sqlPickLines = new DynamicBusinessObjectCollection(Factory);
			var parameters = new List<ZSqlParameter>();
			var query = SortingInSQL_BuildSql(parameters);
			sqlPickLines.Load(query, parameters.ToArray());
			var sqlPickLinesPks = sqlPickLines.Select(o => (ZGuid)o[WhsPickLineSchema.PK]).ToList();

			AssertEquals("Collection should have right Count", 4, sqlPickLinesPks.Count);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks[0], "PART", 1m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks[1], "PART", 1m, "", "", "", "", ZDate.Empty, ZDate.Empty);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks[2], "PART", 1m, "", "", "", "SN2", ZDate.Empty,
				ZDate.Empty);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks[3], "PART", 1m, "", "", "", "SN3", ZDate.Empty,
				ZDate.Empty);
		}

		#endregion

		#region TestSortPalletIDNeutralByDates

		public void TestSortPalletIDNeutralByDates()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			data.Whs1.LocationType.WLT_IsPalletIDNeutral = true;

			var part = CreateProductWithAttributes(data.Org1, "PART1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Factory.Save();

			var today = ZDate.Today;
			var expiryDate1 = today.AddDays(1);
			var expiryDate2 = expiryDate1.AddDays(10);
			var packingDate1 = today.AddDays(1);
			var packingDate2 = packingDate1.AddDays(10);

			CreateInventory(receive1, expiryDate1, packingDate1, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part.PK, palletID: "Pallet");
			CreateInventory(receive1, expiryDate2, packingDate1, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part.PK, palletID: "Pallet");
			CreateInventory(receive1, expiryDate1, packingDate2, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part.PK, palletID: "Pallet");
			CreateInventory(receive1, expiryDate2, packingDate2, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part.PK, palletID: "Pallet");
			CreateInventory(receive1, expiryDate1, packingDate1, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part.PK, palletID: "Pallet");
			CreateInventory(receive1, expiryDate1, packingDate2, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part.PK, palletID: "Pallet");
			receive1.FinaliseDocket();
			AssertEquals("Receive1 could not be finalised", true, receive1.IsFinalised);
			Factory.Save();

			var orderLine1 = Helper.CreateWhsOrderLine(order, part, 20m);
			orderLine1.WE_ExpiryDate = expiryDate1;
			orderLine1.WE_PackingDate = packingDate1;

			var orderLine2 = Helper.CreateWhsOrderLine(order, part, 20m);
			orderLine2.WE_ExpiryDate = expiryDate2;

			var orderLine3 = Helper.CreateWhsOrderLine(order, part, 20m);
			orderLine3.WE_ExpiryDate = expiryDate1;
			orderLine3.WE_PackingDate = packingDate2;

			Helper.CreatePickNew(order);
			Factory.Save();

			var sqlPickLines = new DynamicBusinessObjectCollection(Factory);
			var parameters = new List<ZSqlParameter>();
			var query = SortingInSQL_BuildSql(parameters);
			sqlPickLines.Load(query, parameters.ToArray());
			var sqlPickLinesPks = sqlPickLines.Select(o => (ZGuid)o[WhsPickLineSchema.PK]).ToList();

			AssertEquals("Collection should have right Count", 6, sqlPickLinesPks.Count);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(0, 2), "PART1", 10m, "", "", "", "", expiryDate1,
				packingDate1);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(2, 2), "PART1", 10m, "", "", "", "", expiryDate1,
				packingDate2);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(4, 2), "PART1", 10m, "", "", "", "", expiryDate2,
				ZDate.Empty);
		}

		#endregion

		#region TestSortPalletIDNeutralByProduct

		public void TestSortPalletIDNeutralByProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			data.Whs1.LocationType.WLT_IsPalletIDNeutral = true;

			var part1 = CreateProductWithAttributes(data.Org1, "PART1");
			var part2 = CreateProductWithAttributes(data.Org1, "PART2");
			var part3 = CreateProductWithAttributes(data.Org1, "PART3");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(1);
			var packingDate = ZDate.Today;

			CreateInventory(receive1, expiryDate, packingDate, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part3.PK, qty: 10m, palletID: "Pallet");
			CreateInventory(receive1, expiryDate, packingDate, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part1.PK, qty: 10m, palletID: "Pallet");
			CreateInventory(receive1, expiryDate, packingDate, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part2.PK, qty: 10m, palletID: "Pallet");
			CreateInventory(receive1, expiryDate, packingDate, "", "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part1.PK, qty: 10m, palletID: "Pallet");
			receive1.FinaliseDocket();
			AssertEquals("Receive1 could not be finalised", true, receive1.IsFinalised);
			Factory.Save();

			var orderLine1 = Helper.CreateWhsOrderLine(order, part2, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, part1, 20m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, part3, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var sqlPickLines = new DynamicBusinessObjectCollection(Factory);
			var parameters = new List<ZSqlParameter>();
			var query = SortingInSQL_BuildSql(parameters);
			sqlPickLines.Load(query, parameters.ToArray());
			var sqlPickLinesPks = sqlPickLines.Select(o => (ZGuid)o[WhsPickLineSchema.PK]).ToList();

			AssertEquals("Collection should have right Count", 4, sqlPickLinesPks.Count);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(0, 2), "PART1", 10m, "", "", "", "", ZDate.Empty,
				ZDate.Empty);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks[2], "PART2", 10m, "", "", "", "", ZDate.Empty,
				ZDate.Empty);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks[3], "PART3", 10m, "", "", "", "", ZDate.Empty,
				ZDate.Empty);
		}

		#endregion

		#region TestSortPalletIDNeutralByQuantity

		public void TestSortPalletIDNeutralByQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			data.Whs1.LocationType.WLT_IsPalletIDNeutral = true;

			var part1 = CreateProductWithAttributes(data.Org1, "PART1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Factory.Save();

			var today = ZDate.Today;
			var expiryDate = today.AddDays(1);
			var packingDate = today;

			var inv1 = CreateInventory(receive1, expiryDate, packingDate, "", "", "", "",
				data.Whs1.FindLocation("A-1-1").PK, part1.PK, qty: 30m, palletID: "Pallet");
			var inv2 = CreateInventory(receive1, expiryDate, packingDate, "", "", "", "",
				data.Whs1.FindLocation("A-1-1").PK, part1.PK, qty: 10m, palletID: "Pallet");
			var inv3 = CreateInventory(receive1, expiryDate, packingDate, "", "", "", "",
				data.Whs1.FindLocation("A-1-1").PK, part1.PK, qty: 20m, palletID: "Pallet");
			var inv4 = CreateInventory(receive1, expiryDate, packingDate, "", "", "", "",
				data.Whs1.FindLocation("A-1-1").PK, part1.PK, qty: 10m, palletID: "Pallet");

			receive1.FinaliseDocket();
			AssertEquals("Receive1 could not be finalised", true, receive1.IsFinalised);
			Factory.Save();

			var orderLine1 = Helper.CreateWhsOrderLine(order, part1, 30m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, part1, 40m);

			var pick = Factory.New<WhsPick>();
			pick.AddOrders(new[] { order });

			Helper.CreateWhsPickLine(orderLine1, inv1, 30m);
			Helper.CreateWhsPickLine(orderLine2, inv2, 10m);
			Helper.CreateWhsPickLine(orderLine2, inv3, 20m);
			Helper.CreateWhsPickLine(orderLine2, inv4, 10m);
			Factory.Save();

			var sqlPickLines = new DynamicBusinessObjectCollection(Factory);
			var parameters = new List<ZSqlParameter>();
			var query = SortingInSQL_BuildSql(parameters);
			sqlPickLines.Load(query, parameters.ToArray());
			var sqlPickLinesPks = sqlPickLines.Select(o => (ZGuid)o[WhsPickLineSchema.PK]).ToList();

			AssertEquals("Collection should have right Count", 4, sqlPickLinesPks.Count);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(0, 2), "PART1", 10m, "", "", "", "", ZDate.Empty,
				ZDate.Empty);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks[2], "PART1", 20m, "", "", "", "", ZDate.Empty,
				ZDate.Empty);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks[3], "PART1", 30m, "", "", "", "", ZDate.Empty,
				ZDate.Empty);
		}

		#endregion

		#region TestSortPalletIDNeutral_DifferentPickLinesOnSameInventoryLine

		public void TestSortPalletIDNeutral_DifferentPickLinesOnSameInventoryLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			data.Whs1.LocationType.WLT_IsPalletIDNeutral = true;

			var part = CreateProductWithAttributes(data.Org1, "PART1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "2", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(1);
			var packingDate = ZDate.Today;
			var attribVal = "Att";

			CreateInventory(receive, expiryDate, packingDate, attribVal, "", "", "", data.Whs1.FindLocation("A-1-1").PK,
				part.PK, palletID: "Pallet", 1m);
			CreateInventory(receive, expiryDate, packingDate, attribVal, attribVal, "", "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet", 1m);
			receive.FinaliseDocket();
			AssertEquals("Receive could not be finalised", true, receive.IsFinalised);
			Factory.Save();

			var orderLine1 = Helper.CreateWhsOrderLine(order, part, 2m);
			orderLine1.WE_PartAttrib1 = attribVal;
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			CreateInventory(receive2, expiryDate, packingDate, null, attribVal, null, "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet", 1m);
			CreateInventory(receive2, expiryDate, packingDate, attribVal, attribVal, null, "",
				data.Whs1.FindLocation("A-1-1").PK, part.PK, palletID: "Pallet", 1m);
			receive2.FinaliseDocket();
			AssertEquals("Receive could not be finalised", true, receive2.IsFinalised);
			Factory.Save();

			var orderLine2 = Helper.CreateWhsOrderLine(order2, part, 2m);
			orderLine2.WE_PartAttrib2 = attribVal;
			pick.Orders.Add(order2);
			pick.AutoAllocateItemsWithMock();

			Factory.Save();

			var sqlPickLines = new DynamicBusinessObjectCollection(Factory);
			var parameters = new List<ZSqlParameter>();
			var query = SortingInSQL_BuildSql(parameters);
			sqlPickLines.Load(query, parameters.ToArray());
			var sqlPickLinesPks = sqlPickLines.Select(o => (ZGuid)o[WhsPickLineSchema.PK]).ToList();

			AssertEquals("Collection should have right Count", 4, sqlPickLinesPks.Count);
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(0, 2), "PART1", 1m, "", attribVal, "", "");
			AssertSortedInventoryMatchesOrder(sqlPickLinesPks.GetRange(2, 2), "PART1", 1m, attribVal, "", "", "");
		}

		#endregion

		#endregion

		#region SortByStatusThenByHeldCodeAndThenByProduct

		public void TestSortByStatusThenByHeldCodeAndThenByProduct()
		{
			new SortByStatusThenByHeldCodeAndThenByProductTest(Factory).RunTest();
		}

		class SortByStatusThenByHeldCodeAndThenByProductTest : SortTest<WhsReceiveLine,
			SortByStatusThenByHeldCodeAndThenByProduct, ILineToPutaway>
		{
			readonly TestDataSimpleEnvironment data;

			public SortByStatusThenByHeldCodeAndThenByProductTest(BusinessObjectFactory factory)
				: base(factory)
			{
				data = new TestDataSimpleEnvironment(factory, 0, 0, saveFactory_doNotUseForNewTests: false);
			}

			protected override void AddTestData()
			{
				AddLine(data.Part2, InventoryStatus.Codes.Pending);
				AddLine(data.Part1, InventoryStatus.Codes.Pending);
				AddLine(data.Part2, InventoryStatus.Codes.Pending, InventoryHoldCodes.Codes.Held);
				AddLine(data.Part1, InventoryStatus.Codes.Pending, InventoryHoldCodes.Codes.Held);
				AddLine(data.Part2, InventoryStatus.Codes.Pending, InventoryHoldCodes.Codes.Damaged);
				AddLine(data.Part1, InventoryStatus.Codes.Pending, InventoryHoldCodes.Codes.Damaged);
				AddLine(data.Part2, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				AddLine(data.Part1, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				AddLine(data.Part2, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				AddLine(data.Part1, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			}

			void AddLine(OrgSupplierPart part, ZString status, string heldCode = "")
			{
				var receiveLine = CreateLine();
				receiveLine.WE_CurrentInventoryStatus = status;
				receiveLine.WE_WHC_NKOriginalInventoryHeldCode = heldCode;
				receiveLine.WE_OP = part.PK;
			}
		}

		#endregion

		#region Implementation

		abstract class SortTest<T, TSort, TSortType>
			where T : BusinessObject, TSortType
			where TSort : SortByPropertiesComparer<TSortType>, new()
			where TSortType : class
		{
			#region Public

			public SortTest(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public void RunTest()
			{
				original = new List<TSortType>(50);
				sorted = new List<TSortType>(original.Capacity);
				AddTestData();
				sorted.Sort(new TSort());

				AssertEquals("Collections should have same Count", original.Count, sorted.Count);
				Assert("Both collections are empty", original.Count != 0);
				for (int i = 0; i < original.Count; i++)
				{
					AssertEquals("Line " + i + " failed", original[i], sorted[original.Count - i - 1]);
				}
			}

			#endregion

			#region Protected

			protected abstract void AddTestData();

			protected T CreateLine()
			{
				T result = factory.New<T>();
				original.Add(result);
				sorted.Add(result);
				return result;
			}

			protected BusinessObjectFactory factory;

			#endregion

			#region Implementation

			List<TSortType> original, sorted;

			#endregion
		}

		#region SetPickAvailableInventory

		void SetPickAvailableInventory(WhsPickAvailableInventory availableInventory, WhsOrder order,
			TestILineAttributes attributes, ZDateTimeOffset arrivalDate, ZGuid locationPK, ZGuid partPK, string palletID = "")
		{
			if (availableInventory != null)
			{
				var inventory = Factory.New<WhsInventoryView>();
				var collection = new WhsPickOrderedInventoryCollection(Factory, order.Pick);
				var orderedInventory = GetPickOrderedInventory(collection, order, attributes, partPK, 10m);
				SetInventory(inventory, attributes, arrivalDate, locationPK, partPK);
				inventory.WI_PalletID = palletID;
				((IWhsPickAvailableInventoryInternals)availableInventory).SetAllProperties(orderedInventory, inventory);
			}
		}

		void SetPickAvailableInventory(WhsPickAvailableInventory availableInventory, WhsOrder order,
			ZDateTime wB_EntryDate, TestILineAttributes attributes, ZDateTimeOffset arrivalDate, ZGuid locationPK,
			ZGuid partPK)
		{
			if (availableInventory != null)
			{
				var inventory = Factory.New<WhsInventoryView>();
				var collection = new WhsPickOrderedInventoryCollection(Factory, order.Pick);
				var orderedInventory = GetPickOrderedInventory(collection, order, attributes, partPK, 10m);
				SetInventory(inventory, attributes, arrivalDate, locationPK, partPK);
				inventory.CustomsData.WB_EntryDate = wB_EntryDate;
				((IWhsPickAvailableInventoryInternals)availableInventory).SetAllProperties(orderedInventory, inventory);
			}
		}

		#endregion

		#region SetPickOrderedInventory

		WhsPickOrderedInventory GetPickOrderedInventory(WhsPickOrderedInventoryCollection collection, WhsOrder order,
			TestILineAttributes attributes, ZGuid partPK, ZDecimal quantity)
		{
			var orderLine = order.Lines.AddNew();
			orderLine.SetAttributes(attributes);
			orderLine.WE_OP = partPK;
			orderLine.WE_TransactionQuantity = quantity;
			collection.AddNewFromOrder(order);

			return collection.GetOrderedInventoryForLine(orderLine);
		}

		#endregion

		#region SetInventory

		void SetInventory(WhsReceive receive, ZString pickMethod, ZGuid location, ZGuid part, string palletID = "",
			short pickPathSequence = 0)
		{
			var line = Helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
			line.WI_WL = location;
			line.Location.WLV_PickMethod = pickMethod;
			line.Location.WLV_PickPathSequence = pickPathSequence;
			line.WI_PalletID = palletID;
		}

		void SetInventory(WhsInventoryView inventory, TestILineAttributes attributes, ZDateTimeOffset arrivalDate,
			ZGuid locationPK, ZGuid partPK)
		{
			inventory.SetAttributes(attributes);
			inventory.WI_ArrivalDate = arrivalDate;
			inventory.WI_WL = locationPK;
			inventory.WI_OP = partPK;
		}

		WhsInventoryView CreateInventory(WhsReceive receive, ZDate expiryDate, ZDate packingDate, ZString partAttrib1,
			ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZGuid location, ZGuid part,
			string palletID = "", decimal qty = 10m)
		{
			var line = Helper.CreateWhsReceiveInventoryLine(receive, part, qty);
			line.SetAttributes(new TestILineAttributes("", expiryDate, packingDate, partAttrib1, partAttrib2,
				partAttrib3, serialNumber));
			line.WI_WL = location;
			line.WI_PalletID = palletID;
			return line;
		}

		#endregion

		#region SetTransferLine

		void SetTransferLine_TransferFromLocAndPartPK(WhsTransferLine line, ZGuid transferFromLocationPK, ZGuid partPK)
		{
			line.WE_WL_TransferFrom = transferFromLocationPK;
			line.WE_OP = partPK;
		}

		void SetTransferLine_LocationAndPartPK(WhsTransferLine line, ZGuid locationPK, ZGuid partPK)
		{
			line.WE_WL = locationPK;
			line.WE_OP = partPK;
		}

		#endregion

		#region SetStocktakeLine

		void SetStocktakeLine(WhsStocktakeLine line, ZGuid locationPK, ZGuid partPK)
		{
			line.WU_WL = locationPK;
			line.WU_OP = partPK;
		}

		#endregion

		TestILineAttributes[] GetIAttributesTestCollection()
		{
			var count = 10;
			var today = ZDate.Today;
			var emptyDate = ZDate.Empty;
			var collection = new TestILineAttributes[count];

			collection[0] = new TestILineAttributes("", emptyDate, emptyDate, "", "", "");
			collection[1] = new TestILineAttributes("", emptyDate, emptyDate, "", "", "", "SN1");
			collection[2] = new TestILineAttributes("", emptyDate, emptyDate, "", "", "PA3");
			collection[3] = new TestILineAttributes("", emptyDate, emptyDate, "", "PA2", "");
			collection[4] = new TestILineAttributes("", emptyDate, emptyDate, "PA1", "", "");
			collection[5] = new TestILineAttributes("BEK", emptyDate, emptyDate, "", "", "");
			collection[6] = new TestILineAttributes("", emptyDate, today, "", "", "");
			collection[7] = new TestILineAttributes("", emptyDate, today.AddMonths(-1), "", "", "");
			collection[8] = new TestILineAttributes("", today, emptyDate, "", "", "");
			collection[9] = new TestILineAttributes("", today.AddMonths(-1), emptyDate, "", "", "");

			return collection;
		}

		void StandardAsserts(IList source, IList result)
		{
			Assert("One or both collection are empty", source.Count != 0 && result.Count != 0);
			AssertEquals("Collections should have same Count", source.Count, result.Count);
			for (int i = 0; i < source.Count; i++)
			{
				AssertEquals("Lines should be equal", source[i], result[source.Count - i - 1]);
			}
		}

		void StandardAssertsCorrectOrder(IList source, IList result)
		{
			Assert("One or both collection are empty", source.Count != 0 && result.Count != 0);
			AssertEquals("Collections should have same Count", source.Count, result.Count);
			CombineAssertions(() =>
			{
				for (int i = 0; i < source.Count; i++)
				{
					AssertEquals("Lines should be equal", source[i], result[i]);
				}
			});
		}

		#region SortPickLines

		WhsPick SetupForSortPickLines()
		{
			//Setup Warehouse/Products
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var org2 = Helper.CreateClient("2");
			var part3 = Helper.CreateProduct(org2, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var part5 = Helper.CreateProduct(data.Org1, "P5");
			var part6 = Helper.CreateProduct(data.Org1, "P6");
			var whs2 = Helper.CreateWarehouse("Warehous");
			var row1 = Helper.CreateRowAndGenerateLocations(whs2, "A", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(whs2, "B", 2, 2, 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs2, "C", 2, 2, 2, rowSequence: 2);
			var row4 = Helper.CreateRowAndGenerateLocations(whs2, "D", 2, 2, 2, rowSequence: 1);
			var row5 = Helper.CreateRowAndGenerateLocations(whs2, "E", 2, 2, 2, rowSequence: 1);
			var receive1 = Helper.CreateWhsReceive(data.Org1, whs2, "1", Notify);
			var receive2 = Helper.CreateWhsReceive(org2, whs2, "2", Notify);

			SetupRegistry();

			// Setup Inventory
			SetInventory(receive2, "ANY", whs2.FindLocation("B-1-2-2").PK, part3.PK, pickPathSequence: 1);
			SetInventory(receive2, "ANY", whs2.FindLocation("B-1-2-1").PK, part3.PK);
			SetInventory(receive2, "ANY", whs2.FindLocation("A-1-2-2").PK, part3.PK, pickPathSequence: 1);
			SetInventory(receive2, "ANY", whs2.FindLocation("A-1-2-1").PK, part3.PK);
			SetInventory(receive1, "TRB", whs2.FindLocation("A-2-1-2").PK, data.Part1.PK);
			SetInventory(receive1, "TRB", whs2.FindLocation("A-2-1-1").PK, data.Part1.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("B-2-1-1").PK, data.Part2.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("A-2-2-1").PK, data.Part2.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("A-1-2-1").PK, data.Part2.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("A-1-1-1").PK, data.Part2.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("B-2-1-1").PK, data.Part1.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("A-1-2-1").PK, data.Part1.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("A-1-1-2").PK, data.Part1.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("A-1-1-1").PK, data.Part1.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("B-1-1-2").PK, part4.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("B-1-1-1").PK, part4.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("B-2-2-2").PK, part5.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("B-2-2-1").PK, part5.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("C-1-1-1").PK, part6.PK);
			SetInventory(receive1, "ANY", whs2.FindLocation("C-1-1-2").PK, part6.PK, pickPathSequence: 2);
			SetInventory(receive1, "ANY", whs2.FindLocation("C-2-2-2").PK, part6.PK, pickPathSequence: 1);
			SetInventory(receive1, "ANY", whs2.FindLocation("E-1-1-1").PK, part6.PK, pickPathSequence: 2);
			SetInventory(receive1, "ANY", whs2.FindLocation("E-1-1-2").PK, part6.PK, pickPathSequence: 1);
			SetInventory(receive1, "ANY", whs2.FindLocation("D-1-1-1").PK, part6.PK, pickPathSequence: 2);
			SetInventory(receive1, "ANY", whs2.FindLocation("D-1-1-2").PK, part6.PK, pickPathSequence: 1);
			SetInventory(receive1, "ANY", whs2.FindLocation("D-2-1-1").PK, part6.PK, pickPathSequence: 3);
			SetInventory(receive1, "ANY", whs2.FindLocation("D-2-1-2").PK, part6.PK, pickPathSequence: 3);
			SetInventory(receive1, "ANY", whs2.FindLocation("D-2-2-1").PK, part6.PK);

			receive2.FinaliseDocket();
			AssertEquals("Receive2 could not be finalised", true, receive2.IsFinalised);
			receive1.FinaliseDocket();
			AssertEquals("Receive1 could not be finalised", true, receive1.IsFinalised);
			Factory.Save();

			// Setup Orders / Pick
			var order1 = Helper.CreateWhsOrder(data.Org1, whs2);
			var orderLine11 = Helper.CreateWhsOrderLine(order1, data.Part1, 60m);
			var orderLine12 = Helper.CreateWhsOrderLine(order1, data.Part2, 40m);
			var orderLine13 = Helper.CreateWhsOrderLine(order1, part4, 20m, pickGroup: 2);
			var orderLine14 = Helper.CreateWhsOrderLine(order1, part5, 20m, pickGroup: 1);
			var orderLine15 = Helper.CreateWhsOrderLine(order1, part6, 100m);

			var order2 = Helper.CreateWhsOrder(org2, whs2);
			var orderLine21 = Helper.CreateWhsOrderLine(order2, part3, 40m);

			var pick = Helper.CreatePickNew(order1, order2);

			return pick;
		}

		void SetupRegistry()
		{
			var pickGroupCollection = new PickGroupCollection();
			var pickGroup1 = pickGroupCollection.AddNew();
			pickGroup1.Description = (NoResString)"Heavy Products";
			var pickGroup2 = pickGroupCollection.AddNew();
			pickGroup2.Description = (NoResString)"Soft Drinks";

			var codeDescriptionPairList = new SystemDefinableCodeDescriptionBoolCollection();
			var codeDescriptionPair = codeDescriptionPairList.AddNew();
			codeDescriptionPair.Code = "TRB";
			codeDescriptionPair.Bool = true;
			codeDescriptionPair.Description = (NoResString)"Tractor Beam";

			WarehouseDataRegistry.Instance.PickGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pickGroupCollection);
			WarehouseDataRegistry.Instance.PickMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				codeDescriptionPairList);
			Factory.Save();
		}

		public OrgSupplierPart CreateProductWithAttributes(OrgHeader owner, string code)
		{
			var product = Helper.CreateProduct(owner, code);
			Helper.SetClientAllAttributeType(owner, false);
			Helper.SetProductAllAttributeUse(owner, product, true);
			return product;
		}

		void AssertSortedInventoryMatchesOrder(List<ZGuid> pickLines, ZString productCode, ZDecimal quantity,
			ZString attribute1, ZString attribute2, ZString attribute3, ZString serialNumber,
			ZDate expiryDate = default, ZDate packingDate = default)
		{
			var distinctPickLines = pickLines.Distinct();
			AssertEquals("Duplicate Picklines should not appear.", pickLines.Count, distinctPickLines.Count());
			foreach (var pickLinePK in pickLines)
			{
				AssertSortedInventoryMatchesOrder(pickLinePK, productCode, quantity, attribute1, attribute2, attribute3,
					serialNumber, expiryDate, packingDate);
			}
		}

		void AssertSortedInventoryMatchesOrder(ZGuid pickLinePK, ZString productCode, ZDecimal quantity,
			ZString attribute1, ZString attribute2, ZString attribute3, ZString serialNumber,
			ZDate expiryDate = default, ZDate packingDate = default)
		{
			var pickLine = Factory.Load<WhsPickLine>(pickLinePK);
			var orderLine = pickLine.DocketLine;
			AssertEquals("ProductCode should be correct.", productCode, orderLine.ProductCode);
			AssertEquals("Quantity should be correct.", quantity, pickLine.WZ_Units);
			AssertEquals("Attribute1 should be correct.", attribute1, orderLine.WE_PartAttrib1);
			AssertEquals("Attribute2 should be correct.", attribute2, orderLine.WE_PartAttrib2);
			AssertEquals("Attribute3 should be correct.", attribute3, orderLine.WE_PartAttrib3);
			AssertEquals("SerialNumber should be correct.", serialNumber, orderLine.WE_SerialNumber);
			AssertEquals("Expiry Date should be correct.", expiryDate, orderLine.WE_ExpiryDate);
			AssertEquals("Packing Date should be correct.", packingDate, orderLine.WE_PackingDate);
		}

		static void AssertContainsExactElementsInExactOrder<T>(string message, BusinessObjectCollection sortedLines,
			params T[] lines)
		{
			AssertContainsExactElementsInExactOrder(message, lines, sortedLines.Cast<T>());
		}

		#endregion

		#endregion
	}
}
