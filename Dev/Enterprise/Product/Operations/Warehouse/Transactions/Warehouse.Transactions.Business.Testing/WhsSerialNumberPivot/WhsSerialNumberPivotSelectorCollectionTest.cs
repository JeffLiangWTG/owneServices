using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsSerialNumberPivotSelectorCollection))]
	class WhsSerialNumberPivotSelectorCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WhsSerialNumberPivotSelectorCollection>
	{
		#region Constructor

		public void TestConstructor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var pivot = receiveLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);

			new WhsSerialNumberPivotSelector(pivot, _ => false);

			AssertNoExceptionThrown(() => new WhsSerialNumberPivotSelectorCollection(receiveLine.SerialNumbers, pick, ZString.Empty, _ => false));
			AssertExceptionThrown<ArgumentException>("PickLines is null", () => new WhsSerialNumberPivotSelectorCollection(receiveLine.SerialNumbers, null, ZString.Empty, _ => false));
			AssertExceptionThrown<ArgumentException>("Pivots is null", () => new WhsSerialNumberPivotSelectorCollection(null, pick, ZString.Empty, _ => false));
			AssertExceptionThrown<ArgumentException>("ReadOnlyForChangingAllocationsProvider is null", () => new WhsSerialNumberPivotSelectorCollection(receiveLine.SerialNumbers, pick, ZString.Empty, null));

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, data.Whs1.DefaultLocation);
			AssertNoExceptionThrown(() => new WhsSerialNumberPivotSelectorCollection(adjustmentLine, true));
			AssertExceptionThrown<ArgumentException>("lineWithCommittedPickLines is null", () => new WhsSerialNumberPivotSelectorCollection(null, true));
		}

		#endregion

		#region TestEmpty

		public void TestEmpty()
		{
			var emptyCollection = WhsSerialNumberPivotSelectorCollection.Empty;
			AssertEquals(0, emptyCollection.Count);
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_Sort

		public void TestWhsSerialNumberPivotSelectorCollection_Sort1()
		{
			TestWhsSerialNumberPivotSelectorCollection_SortCore("SN5", "SN4", "SN3", "SN2", "SN1");
		}

		public void TestWhsSerialNumberPivotSelectorCollection_Sort2()
		{
			TestWhsSerialNumberPivotSelectorCollection_SortCore("SN1", "SN4", "SN5", "SN2", "SN3");
		}

		void TestWhsSerialNumberPivotSelectorCollection_SortCore(params ZString[] serials)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

			foreach (var serial in serials)
			{
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = serial;
			}
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var collection = availableInventory.SerialNumbers;

			AssertContainsExactElementsInExactOrder("Result should be same and sorted.", ["SN1", "SN2", "SN3", "SN4", "SN5"],
				collection.Select(c => c.SerialNumberValue));
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_AllowNew

		public void TestWhsSerialNumberPivotSelectorCollection_AllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_AddNew

		public void TestWhsSerialNumberPivotSelectorCollection_AddNew()
		{
			AssertExceptionThrown(typeof(InvalidOperationException), "Creating new items directly is not supported for this collection.", () => GetCollectionToTest().AddNew());
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_Select

		public void TestWhsSerialNumberPivotSelectorCollection_Select()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

			var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var pivot = receiveLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";

			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var collection = availableInventory.SerialNumbers;

			AssertEquals("By default is select when it is allocated.", true, collection[0].Selected);
			collection[0].Selected = false;
			AssertEquals(false, collection[0].Selected);
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_SerialNumberSpecified

		public void TestWhsSerialNumberPivotSelectorCollection_SerialNumberSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

			var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 4m, data.Whs1.DefaultLocation);
			var serialNumbers = receiveLine.SerialNumbers;
			serialNumbers.AddNew().SerialNumberValue = "SN1";
			serialNumbers.AddNew().SerialNumberValue = "SN2";
			serialNumbers.AddNew().SerialNumberValue = "SN3";
			serialNumbers.AddNew().SerialNumberValue = "SN4";

			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN1";
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "Sn2";
			var orderLine3 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = helper.CreatePickNew(order);

			AssertEquals("Precondition", string.Empty, orderLine3.WE_SerialNumber);
			AssertSerialNumberFilter(orderLineSerialNumber: "SN1", expectedSerialNumbers: ["SN1"]);
			AssertSerialNumberFilter(orderLineSerialNumber: "SN2", expectedSerialNumbers: ["SN2"]);
			AssertSerialNumberFilter(orderLineSerialNumber: "", expectedSerialNumbers: ["SN1", "SN2", "SN3", "SN4"]);

			void AssertSerialNumberFilter(string orderLineSerialNumber, string[] expectedSerialNumbers)
			{
				var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ol => ol.SerialNumber.EqualsIgnoringCase(orderLineSerialNumber));
				var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

				AssertContainsExactElementsInExactOrder(expectedSerialNumbers, availableInventory.SerialNumbers.Select(s => s.SerialNumberValue));
			}
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_Allocate

		public void TestWhsSerialNumberPivotSelectorCollection_Allocate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = $"SN0";
			for (var i = 1; i <= 5; i++)
			{
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = $"SN{i}";
			}
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			AssertEquals("Precondition", 0, PopulatedWhsSerialNumberPivotInDB().Length);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var collection = availableInventory.SerialNumbers;

			var expectedSelectedSerialNumbers = Enumerable.Range(1, 5).Select(i => $"SN{i}").ToArray();
			AssertContainsExactElementsInExactOrder(expectedSelectedSerialNumbers, collection.Where(s => s.Selected).Select(s => s.SerialNumberValue));

			var whsSerialNumberPivotsInDB = PopulatedWhsSerialNumberPivotInDB();
			AssertEquals(5, whsSerialNumberPivotsInDB.Length);
			AssertContainsExactElementsInAnyOrder("Should be able to save PickingLine columns for Pivots.",
				pick.GetAllPickLines().Select(p => p.PK),
				whsSerialNumberPivotsInDB.Select(p => p.WSV_WZ_PickingLine).Distinct());

			availableInventory.Allocate = false;
			AssertContainsExactElementsInExactOrder(Array.Empty<ZString>(), collection.Where(s => s.Selected).Select(s => s.SerialNumberValue));
			Factory.Save();
			AssertEquals("Should clear selection.", 0, PopulatedWhsSerialNumberPivotInDB().Length);

			availableInventory.Allocate = true;
			AssertContainsExactElementsInExactOrder(expectedSelectedSerialNumbers, collection.Where(s => s.Selected).Select(s => s.SerialNumberValue));
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_SerialNumberSelection

		public void TestWhsSerialNumberPivotSelectorCollection_ChangeSerialNumberSelection_SingleInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 5, true, false);
			receive.Lines[0].WE_SerialNumber = "SN0";
			for (var i = 1; i <= 5; i++)
			{
				receive.Lines[0].SerialNumbers.AddNew().SerialNumberValue = $"SN{i}";
			}
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			AssertEquals("Precondition", 0, PopulatedWhsSerialNumberPivotInDB().Length);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var collection = availableInventory.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();

			AssertContainsExactElementsInExactOrder(["SN1", "SN2"], collection.Where(s => s.Selected).Select(s => s.SerialNumberValue));

			var pivotsInDB = PopulatedWhsSerialNumberPivotInDB();
			AssertEquals(2, pivotsInDB.Length);
			AssertEquals(1, pivotsInDB.Select(p => p.WSV_WZ_PickingLine).Distinct().Count());
			AssertContainsExactElementsInAnyOrder("Should be able to save PickingLine columns for Pivots.",
				pick.GetAllPickLines().Select(p => p.PK),
				pivotsInDB.Select(p => p.WSV_WZ_PickingLine).Distinct());

			AssertContainsExactElementsInExactOrder(["SN1", "SN2", "SN3", "SN4", "SN5"], collection.Select(s => s.SerialNumberValue));

			collection.Single(c => c.SerialNumberValue == "SN1").Selected = false;
			collection.Single(c => c.SerialNumberValue == "SN3").Selected = true;

			AssertEquals("Precondition", true, collection.Single(c => c.SerialNumberValue == "SN3").Selected);
			AssertEquals("Precondition", true, collection.Single(c => c.SerialNumberValue == "SN2").Selected);

			AssertContainsExactElementsInExactOrder(["SN2", "SN3"], collection.Where(s => s.Selected).Select(s => s.SerialNumberValue));
			Factory.Save();

			var pivotsInDB2 = PopulatedWhsSerialNumberPivotInDB();
			AssertEquals(2, pivotsInDB2.Length);
			AssertEquals(1, pivotsInDB2.Select(p => p.WSV_WZ_PickingLine).Distinct().Count());
			AssertContainsExactElementsInAnyOrder("PickingLine are match.",
				pick.GetAllPickLines().Select(p => p.PK),
				pivotsInDB2.Select(p => p.WSV_WZ_PickingLine).Distinct());

			AssertContainsExactElementsInAnyOrder("Should save change selected pivots.",
				new[] { "SN2", "SN3" },
				pivotsInDB2.Select(p => p.SerialNumberValue));
		}

		public void TestWhsSerialNumberPivotSelectorCollection_ChangeSerialNumberSelection_MultiInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			for (var i = 0; i < 3; i++)
			{
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = $"SN{i}";
				for (var j = 1; j <= 2; j++)
				{
					var pivot = receiveLine.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{(i * 2 + j)}";
				}
			}

			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			AssertEquals("Precondition", 0, PopulatedWhsSerialNumberPivotInDB().Length);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var collection = availableInventory.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();

			var expectedSelectedSerialNumbers = Enumerable.Range(1, 6).Select(i => $"SN{i}").ToArray();
			AssertContainsExactElementsInExactOrder(expectedSelectedSerialNumbers, collection.Select(s => s.SerialNumberValue));
			AssertContainsExactElementsInExactOrder(["SN1", "SN2"], collection.Where(s => s.Selected).Select(s => s.SerialNumberValue));

			var pivotsInDB = PopulatedWhsSerialNumberPivotInDB();
			AssertEquals(2, pivotsInDB.Length);
			AssertEquals(1, pivotsInDB.Select(p => p.WSV_WZ_PickingLine).Distinct().Count());
			AssertContainsExactElementsInAnyOrder("Should be able to save PickingLine columns for Pivots.",
				pick.GetAllPickLines().Select(p => p.PK),
				pivotsInDB.Select(p => p.WSV_WZ_PickingLine).Distinct());

			collection.Single(c => c.SerialNumberValue == "SN1").Selected = false;
			collection.Single(c => c.SerialNumberValue == "SN3").Selected = true;
			Factory.Save();

			var pivotsInDB2 = PopulatedWhsSerialNumberPivotInDB();
			AssertEquals(2, pivotsInDB2.Length);
			AssertEquals(2, pivotsInDB2.Select(p => p.WSV_WZ_PickingLine).Distinct().Count());
			AssertContainsExactElementsInAnyOrder("PickingLine are match.",
				pick.GetAllPickLines().Select(p => p.PK),
				pivotsInDB2.Select(p => p.WSV_WZ_PickingLine).Distinct());

			AssertContainsExactElementsInAnyOrder("Should save change selected pivots.",
				new[] { "SN2", "SN3" },
				pivotsInDB2.Select(p => p.SerialNumberValue));
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_SerialNumberSelection_Deallocate

		public void TestWhsSerialNumberPivotSelectorCollection_SerialNumberSelection_Deallocate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			for (var i = 0; i < 3; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R0{i}");
				receive.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-4 + i); // Just make a different date
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = $"SN{i}";
				for (var j = 1; j <= 2; j++)
				{
					var pivot = receiveLine.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{(i * 2 + j)}";
				}
				receive.FinaliseDocket();
				AssertEquals("Precondition", true, receive.IsFinalised);
				Factory.Save();
			}

			AssertEquals("Precondition", 0, PopulatedWhsSerialNumberPivotInDB().Length);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2);
			var pick = Helper.CreatePickNew(order);

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray();
			var availableInventory1 = orderedInventories[0].AvailableInventories[0];
			var collection1 = availableInventory1.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();

			AssertContainsExactElementsInExactOrder(["SN1", "SN2"], collection1.Select(s => s.SerialNumberValue));
			AssertContainsExactElementsInExactOrder(["SN1", "SN2"], collection1.Where(s => s.Selected).Select(s => s.SerialNumberValue));

			var availableInventory2 = orderedInventories[0].AvailableInventories[1];
			var collection2 = availableInventory2.SerialNumbers.Cast<WhsSerialNumberPivotSelector>();

			availableInventory2.Allocate = true; // No errors occur when selecting or unselecting.
			AssertContainsExactElementsInExactOrder(["SN3", "SN4"], collection2.Select(s => s.SerialNumberValue));
			AssertContainsExactElementsInExactOrder("All special numbers are already assigned; no more allocations."
				, Enumerable.Empty<ZString>(), collection2.Where(s => s.Selected).Select(s => s.SerialNumberValue));

			availableInventory2.Allocate = false; // No errors occur when selecting or unselecting.
			AssertContainsExactElementsInExactOrder(["SN3", "SN4"], collection2.Select(s => s.SerialNumberValue));
			AssertContainsExactElementsInExactOrder(Enumerable.Empty<ZString>(), collection2.Where(s => s.Selected).Select(s => s.SerialNumberValue));

			availableInventory1.Allocate = false;
			availableInventory2.Allocate = true;
			AssertContainsExactElementsInExactOrder(["SN3", "SN4"], collection2.Select(s => s.SerialNumberValue));
			AssertContainsExactElementsInExactOrder(["SN3", "SN4"], collection2.Where(s => s.Selected).Select(s => s.SerialNumberValue));
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_Save

		public void TestWhsSerialNumberPivotSelectorCollection_Save()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = $"SN0";

			for (var i = 1; i <= 5; i++)
			{
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = $"SN{i}";
			}
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			AssertContainsExactElementsInExactOrder(["SN1", "SN2", "SN3", "SN4", "SN5"], availableInventory.SerialNumbers.Select(s => s.SerialNumberValue));

			var collection = availableInventory.SerialNumbers;
			AssertEquals("By default is select the first to items match with quantity.", true, collection[0].Selected);
			for (var i = 0; i < 5; i++)
			{
				AssertEquals("By default is select the first to items match with quantity.", true, collection[i].Selected);
			}

			AssertContainsExactElementsInAnyOrder("Should be able to save PickingLine columns for Pivots.",
				pick.GetAllPickLines().Select(p => p.PK),
				PopulatedWhsSerialNumberPivotInDB().Select(p => p.WSV_WZ_PickingLine).Distinct());

			WhsSerialNumberPivot[] PopulatedWhsSerialNumberPivotInDB()
			{
				return NewFactory().Load<WhsSerialNumberPivot>(new ZDBOnlyQuery(typeof(WhsSerialNumberPivot)).AddToFilter(WhsSerialNumberPivotSchema.WSV_WZ_PickingLine, SQLComparisonOperator.NotEqual, null));
			}
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_SelectionChanged

		public void TestWhsSerialNumberPivotSelectorCollection_SelectionChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

			var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = "SN0";
			for (var i = 1; i <= 5; i++)
			{
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = $"SN{i}";
			}

			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var pick = helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			var collection = availableInventory.SerialNumbers;

			WhsSerialNumberPivotSelector selector = null;
			collection.SelectionChanged += (sender, args) => selector = (WhsSerialNumberPivotSelector)sender;

			AssertEquals("By default is select when it is allocated.", true, collection[0].Selected);
			collection[0].Selected = false;
			AssertEquals(selector, collection[0]);

			AssertEquals("By default is not selected.", false, collection[3].Selected);
			collection[3].Selected = true;
			AssertEquals(selector, collection[3]);
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_GetSerialsByInventory

		public void TestWhsSerialNumberPivotSelectorCollection_GetSerialsByInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var inventoryPKs = new List<ZGuid>();
			var now = ZDateTimeOffset.Today;
			for (var i = 0; i < 3; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R0{i}");
				receive.WD_ArrivalDate = now;
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = $"SN{i}";
				inventoryPKs.Add(receiveLine.PK);
				for (var j = 1; j <= 2; j++)
				{
					var pivot = receiveLine.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{(i * 2 + j)}";
				}
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Precondition", true, receive.IsFinalised);
				Factory.Save();
			}

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2);
			var pick = Helper.CreatePickNew(order);

			var serialNumbers = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single()
				.AvailableInventories.Cast<WhsPickAvailableInventory>().Single().SerialNumbers;

			AssertContainsExactElementsInExactOrder(["SN1", "SN2"], serialNumbers.GetSerialsByInventory(inventoryPKs[0]).Select(s => s.SerialNumberValue));
			AssertContainsExactElementsInExactOrder(["SN3", "SN4"], serialNumbers.GetSerialsByInventory(inventoryPKs[1]).Select(s => s.SerialNumberValue));
			AssertContainsExactElementsInExactOrder(["SN5", "SN6"], serialNumbers.GetSerialsByInventory(inventoryPKs[2]).Select(s => s.SerialNumberValue));
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_SerialNumberSelection_UnselectedOnly

		public void TestWhsSerialNumberPivotSelectorCollection_SerialNumberSelection_UnselectedOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 5, true, false);
			receive.Lines[0].WE_SerialNumber = "SN0";
			for (var i = 1; i <= 5; i++)
			{
				receive.Lines[0].SerialNumbers.AddNew().SerialNumberValue = $"SN{i}";
			}
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			AssertEquals("Precondition", 0, PopulatedWhsSerialNumberPivotInDB().Length);
			PickAndAssertSelectedSerialNumbres(orderRef: "O1", "SN1", "SN2");
			AssertEquals(2, PopulatedWhsSerialNumberPivotInDB().Length);

			PickAndAssertSelectedSerialNumbres(orderRef: "O2", "SN3", "SN4");
			AssertEquals(4, PopulatedWhsSerialNumberPivotInDB().Length);

			void PickAndAssertSelectedSerialNumbres(string orderRef, params string[] expectedSelectedSerials)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, orderRef, data.Part1, 2);
				var pick = Helper.CreatePickNew(order);
				var collection = pick.OrderedInventories[0].AvailableInventories[0].SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				Factory.Save();

				AssertContainsExactElementsInExactOrder(expectedSelectedSerials, collection.Where(s => s.Selected).Select(s => s.SerialNumberValue));
			}
		}

		public void TestWhsSerialNumberPivotSelectorCollection_SerialNumberSelection_UnselectedOnly_MultiLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = $"SN0";

			for (var i = 1; i <= 5; i++)
			{
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = $"SN{i}";
			}
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			AssertEquals("Precondition", 0, PopulatedWhsSerialNumberPivotInDB().Length);
			PickAndAssertSelectedSerialNumbres(orderRef: "O1", "SN1", "SN2");
			AssertEquals(2, PopulatedWhsSerialNumberPivotInDB().Length);

			PickAndAssertSelectedSerialNumbres(orderRef: "O2", "SN3", "SN4");
			AssertEquals(4, PopulatedWhsSerialNumberPivotInDB().Length);

			void PickAndAssertSelectedSerialNumbres(string orderRef, params string[] expectedSelectedSerials)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, orderRef, data.Part1, 2);
				var pick = Helper.CreatePickNew(order);
				var collection = pick.OrderedInventories[0].AvailableInventories[0].SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				Factory.Save();

				AssertContainsExactElementsInExactOrder(expectedSelectedSerials, collection.Where(s => s.Selected).Select(s => s.SerialNumberValue));
			}
		}

		public void TestWhsSerialNumberPivotSelectorCollection_SerialNumberSelection_UnselectedOnly_MultiInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			for (var i = 0; i < 5; i++)
			{
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = $"SN0{5 - i}";
				receiveLine.SerialNumbers.AddNew().SerialNumberValue = $"SN{5 - i}";
			}
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			AssertEquals("Precondition", 0, PopulatedWhsSerialNumberPivotInDB().Length);
			PickAndAssertSelectedSerialNumbres(orderRef: "O1", "SN1", "SN2");

			AssertEquals("Precondition", 2, PopulatedWhsSerialNumberPivotInDB().Length);
			PickAndAssertSelectedSerialNumbres(orderRef: "O2", "SN3", "SN4");

			void PickAndAssertSelectedSerialNumbres(string orderRef, params string[] expectedSelectedSerials)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, orderRef, data.Part1, 2);
				var pick = Helper.CreatePickNew(order);
				var collection = pick.OrderedInventories[0].AvailableInventories[0].SerialNumbers.Cast<WhsSerialNumberPivotSelector>();
				Factory.Save();

				AssertContainsExactElementsInExactOrder(expectedSelectedSerials, collection.Where(s => s.Selected).Select(s => s.SerialNumberValue));
			}
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_PickLines_Sort

		public void TestWhsSerialNumberPivotSelectorCollection_PickLines_Sort1()
		{
			TestWhsSerialNumberPivotSelectorCollection_PickLines_SortCore("SN5", "SN4", "SN3", "SN2", "SN1");
		}

		public void TestWhsSerialNumberPivotSelectorCollection_PickLines_Sort2()
		{
			TestWhsSerialNumberPivotSelectorCollection_PickLines_SortCore("SN1", "SN4", "SN5", "SN2", "SN3");
		}

		void TestWhsSerialNumberPivotSelectorCollection_PickLines_SortCore(params ZString[] serials)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

			foreach (var serial in serials)
			{
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = serial;
			}
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, data.Whs1.DefaultLocation);
			var collection = adjustmentLine.SerialNumberSelectors;

			AssertContainsExactElementsInExactOrder("Result should be same and sorted.", ["SN1", "SN2", "SN3", "SN4", "SN5"],
				collection.Select(c => c.SerialNumberValue));
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorCollection_PickLines_Select

		public void TestWhsSerialNumberPivotSelectorCollection_PickLines_Select()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3, true, false);
			var receiveLine = receive.Lines[0];
			receiveLine.SerialNumbers.AddNew().SerialNumberValue = "SN1";
			receiveLine.SerialNumbers.AddNew().SerialNumberValue = "SN2";
			receiveLine.SerialNumbers.AddNew().SerialNumberValue = "SN3";
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertEquals("Precondition", true, receive.IsFinalised);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -2m, data.Whs1.DefaultLocation);
			var collection = adjustmentLine.SerialNumberSelectors;
			AssertEquals(3, collection.Count);

			AssertEquals("By default is select when it is allocated.", 2, collection.Where(c => c.Selected).Count());
			collection[0].Selected = false;
			AssertEquals(false, collection[0].Selected);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			enableSchemaRedesignChanges = WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			enableSchemaRedesignChanges.Dispose();
			base.TearDown();
		}

		WhsSerialNumberPivot[] PopulatedWhsSerialNumberPivotInDB()
		{
			return NewFactory().Load<WhsSerialNumberPivot>(new ZDBOnlyQuery(typeof(WhsSerialNumberPivot)).AddToFilter(WhsSerialNumberPivotSchema.WSV_WZ_PickingLine, SQLComparisonOperator.NotEqual, null));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var pivot = Factory.New<WhsSerialNumberPivot>();
			return new WhsSerialNumberPivotSelector(pivot, _ => false);
		}

		protected override WhsSerialNumberPivotSelectorCollection GetCollectionToTest()
		{
			return new WhsSerialNumberPivotSelectorCollection(Enumerable.Empty<WhsSerialNumberPivot>(), Factory.New<WhsPick>(), ZString.Empty, _ => false);
		}

		IDisposable enableSchemaRedesignChanges;

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
