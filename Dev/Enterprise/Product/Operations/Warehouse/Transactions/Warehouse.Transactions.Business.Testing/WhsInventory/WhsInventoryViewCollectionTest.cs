using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsInventoryViewCollection))]
	public class WhsInventoryViewCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		#region Business Object Collection Overrides

		#region TestGetAdditionalListChangedSuspenders

		public void TestGetAdditionalListChangedSuspenders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save(); // creates docket line
			AssertEquals("Precondition: Docket Line created", 1, receive.Lines.Count);

			int listChangedHitCount = 0;
			((IBindingList)receive.Lines).ListChanged += delegate
			{ listChangedHitCount++; };
			using (receive.Inventory.SuspendListChanged())
			{
				receive.Inventory.RemoveAndDelete(inventory);
				AssertEquals("While Inventory collection is suspended the Docket Line collection should also be.", 0, listChangedHitCount);
			}

			AssertEquals("When suspension is lifted list will fire reset event.", 1, listChangedHitCount);
		}

		#endregion

		#region TestSetDefaultsForNewChild

		public void TestSetDefaultsForNewChild()
		{
			var whs = Helper.CreateWarehouse("1");
			var client = Helper.CreateClient();
			var receive = Helper.CreateWhsReceive(client, whs, "1");

			var collection = new WhsInventoryViewCollection(Factory, receive);
			var inventory = collection.AddNew();
			AssertEquals(inventory.WI_ArrivalDate, receive.WD_ArrivalDate);
			AssertEquals(inventory.WI_OH_Client, client.PK);
			AssertEquals(true, inventory.IsReceiveLineInventory);
			AssertEquals(receive.PK, inventory.WI_WD);
			AssertEquals(InventoryStatus.Codes.Arrived, inventory.WI_InventoryStatus);

			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			var inventory2 = collection.AddNew();
			AssertEquals(true, inventory2.IsReceiveLineInventory);
			AssertEquals(InventoryStatus.Codes.Pending, inventory2.WI_InventoryStatus);

			// collection without parent receive
			var collection2 = new WhsInventoryViewCollection(Factory);
			var inventory3 = collection2.AddNew();
			AssertEquals(ZDateTimeOffset.Empty, inventory3.WI_ArrivalDate);
			AssertEquals(ZGuid.Empty, inventory3.WI_OH_Client);
			AssertEquals(ZGuid.Empty, inventory3.WI_WD);
			AssertEquals(false, inventory3.IsReceiveLineInventory);
			AssertEquals(InventoryStatus.Codes.Pending, inventory3.WI_InventoryStatus);
		}

		#endregion

		#region TestSetDefaultsForNewChild_FromDocketLine

		public void TestSetDefaultsForNewChild_FromDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "", "");

			var collection = new WhsInventoryViewCollection(Factory, transferLine);
			var inventory = collection.AddNew();
			AssertEquals(transfer.PK, inventory.WI_WD);
			AssertEquals(transferLine.PK, inventory.WI_WE_InDocketLine);
			AssertEquals(DocketType.Codes.Transfer, inventory.WI_InDocketLineType);

			transferLine.WE_WD = ZGuid.Empty;
			var collection2 = new WhsInventoryViewCollection(Factory, transferLine);
			var inventory2 = collection.AddNew();
			AssertEquals(ZGuid.Empty, inventory2.WI_WD);
			AssertEquals(ZGuid.Empty, inventory2.WI_WE_InDocketLine);
			AssertEquals("", inventory2.WI_InDocketLineType);
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, new WhsInventoryViewCollection(Factory).AllowNew);

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			var collection = new WhsInventoryViewCollection(Factory, receive);

			AssertEquals(false, collection.AllowNew);

			receive.WD_OH_Client = data.Org1.PK;
			AssertEquals(false, collection.AllowNew);

			receive.WD_WW_Whs = data.Whs1.PK;
			AssertEquals(true, collection.AllowNew);

			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			receive.CancelReactivateDocket();
			AssertEquals(false, collection.AllowNew);

			receive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals(true, collection.AllowNew);

			receive.WD_WD_ParentDocket = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1).PK;
			AssertEquals(false, collection.AllowNew);

			receive.WD_WD_ParentDocket = ZGuid.Empty;
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(false, collection.AllowNew);
		}

		#endregion

		#endregion

		#region Manual Filter

		public void TestFilterWarehouse()
		{
			SetupForFilterTests();
			AssertEquals(5, Collection.Count);
			Collection.FilterWarehouse(Whs1);

			AssertEquals(false, Collection.Contains(Inv1.PK));
			AssertEquals(true, Collection.Contains(Inv2.PK));
			AssertEquals(true, Collection.Contains(Inv3.PK));
			AssertEquals(false, Collection.Contains(Inv4.PK));
			AssertEquals(true, Collection.Contains(Inv5.PK));

			SetupForFilterTests();
			Collection.FilterWarehouse(Whs2);

			AssertEquals(2, Collection.Count);
			AssertEquals(true, Collection.Contains(Inv1.PK));
			AssertEquals(false, Collection.Contains(Inv2.PK));
			AssertEquals(false, Collection.Contains(Inv3.PK));
			AssertEquals(true, Collection.Contains(Inv4.PK));
			AssertEquals(false, Collection.Contains(Inv5.PK));
		}

		#endregion

		#region FilterBusinessObjectDefaults

		public void TestAddWarehouseClientProductFilterDefaults()
		{
			Collection.AddWarehouseClientProductFilterDefaults(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Client" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Product" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			Collection.AddWarehouseClientProductFilterDefaults(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Client" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Product" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		public void TestAddAttributeFilterDefaults()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ Collection.AddAttributeFilterDefaults(null); });

			TestILineAttributes attributes = new TestILineAttributes();
			Collection.AddAttributeFilterDefaults(attributes);
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Expiry Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Expiry Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Packing Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Packing Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 1" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 2" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 3" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			attributes = new TestILineAttributes("BEK-1", ZDate.Today, ZDate.Today, "PA1", "PA2", "PA3");
			Collection.AddAttributeFilterDefaults(attributes);
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Expiry Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Expiry Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Packing Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Packing Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 1" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 2" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 3" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		public void TestAddLocationFilterDefaults()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ Collection.AddLocationFilterDefaults(null); });

			WhsLocation location = Factory.New<WhsLocation>();
			Collection.AddLocationFilterDefaults(location);
			AssertEquals(false, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Row" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			location.WLV_WR = Factory.New<WhsRow>().PK;
			Collection.AddLocationFilterDefaults(location);
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Row" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Column" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Level" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, Collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Tray" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		#endregion

		#region TestLocationStringSortedProperly

		public void TestLocationStringSortedProperly()
		{
			TestLocationSortedProperlyCore(nameof(WhsInventoryView.LocationString));
		}

		public void TestWI_WLSortedProperly()
		{
			TestLocationSortedProperlyCore(nameof(WhsInventoryView.WI_WL));
		}

		public void TestCurrentLocationSortedProperly()
		{
			TestLocationSortedProperlyCore(nameof(WhsInventoryView.CurrentLocationString));
		}

		void TestLocationSortedProperlyCore(string locationPropertyToCompare)
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-3"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, data.Whs1.FindLocation("A-5"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-10"));

			// Check sort ascending
			receive.Inventory.Sort(locationPropertyToCompare, ListSortDirection.Ascending);
			AssertEquals("A-1", receive.Inventory[0].LocationString);
			AssertEquals("A-2", receive.Inventory[1].LocationString);
			AssertEquals("A-3", receive.Inventory[2].LocationString);
			AssertEquals("A-5", receive.Inventory[3].LocationString);
			AssertEquals("A-10", receive.Inventory[4].LocationString);

			// Check sort descending
			receive.Inventory.Sort(locationPropertyToCompare, ListSortDirection.Descending);
			AssertEquals("A-10", receive.Inventory[0].LocationString);
			AssertEquals("A-5", receive.Inventory[1].LocationString);
			AssertEquals("A-3", receive.Inventory[2].LocationString);
			AssertEquals("A-2", receive.Inventory[3].LocationString);
			AssertEquals("A-1", receive.Inventory[4].LocationString);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsInventoryViewCollection(Factory);
		}

		protected new WhsInventoryViewCollection Collection
		{
			get { return (WhsInventoryViewCollection)base.Collection; }
		}

		void SetupForFilterTests()
		{
			WhsTestHelperFunctions helper = new WhsTestHelperFunctions(Factory);
			Collection.RemoveAndDeleteAll();

			Whs1 = helper.CreateWarehouse("1", "A", 2, 2);
			Whs2 = helper.CreateWarehouse("2", "A", 2, 2);
			var locations1 = Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var locations2 = Whs2.Rows.Single(r => r.WR_Name == "A").Locations;

			Inv1 = Collection.AddNew();
			Inv2 = Collection.AddNew();
			Inv3 = Collection.AddNew();
			Inv4 = Collection.AddNew();
			Inv5 = Collection.AddNew();

			Inv1.WI_WL = locations2[0].PK;
			Inv2.WI_WL = locations1[2].PK;
			Inv3.WI_WL = locations1[1].PK;
			Inv4.WI_WL = locations2[1].PK;
			Inv5.WI_WL = locations1[0].PK;

			AssertEquals(true, Collection.Contains(Inv1.PK));
			AssertEquals(true, Collection.Contains(Inv2.PK));
			AssertEquals(true, Collection.Contains(Inv3.PK));
			AssertEquals(true, Collection.Contains(Inv4.PK));
			AssertEquals(true, Collection.Contains(Inv5.PK));
		}

		WhsWarehouse Whs1;
		WhsWarehouse Whs2;
		WhsInventoryView Inv1;
		WhsInventoryView Inv2;
		WhsInventoryView Inv3;
		WhsInventoryView Inv4;
		WhsInventoryView Inv5;

		#endregion
	}
}
