using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickCollection))]
	internal class WhsPickCollectionTestCase : WhsBusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsPickCollection(Factory);
		}

		public void TestSortingByDockdoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var dockDoorLocation3 = data.Whs1.FindLocation("A-10");
			dockDoorLocation3.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_WL_DockDoor = dockDoorLocation1.PK;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_WL_DockDoor = dockDoorLocation2.PK;

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			var pick3 = Helper.CreatePickNew(order3);
			pick3.WP_WL_DockDoor = dockDoorLocation3.PK;
			Factory.Save();

			var pickCollection = new WhsPickCollection(Factory);
			pickCollection.AddRange(new[] { pick1, pick2, pick3 });

			pickCollection.Sort(nameof(WhsPick.WP_WL_DockDoor), ListSortDirection.Ascending);
			AssertEquals("A-1", pickCollection[0].DockDoorLocation.ToLocationString());
			AssertEquals("A-2", pickCollection[1].DockDoorLocation.ToLocationString());
			AssertEquals("A-10", pickCollection[2].DockDoorLocation.ToLocationString());

			pickCollection.Sort(nameof(WhsPick.WP_WL_DockDoor), ListSortDirection.Descending);
			AssertEquals("A-10", pickCollection[0].DockDoorLocation.ToLocationString());
			AssertEquals("A-2", pickCollection[1].DockDoorLocation.ToLocationString());
			AssertEquals("A-1", pickCollection[2].DockDoorLocation.ToLocationString());
		}

		public void TestGetFetchStrategy()
		{
			var collection = new WhsPickCollection(Factory, ZQuery.NoResultQuery); // query is not effect on FetchStrategy
			AssertType<WhsPickCollectionFetchStrategy>(((IBusinessObjectCollection)collection).FetchStrategy);
		}
	}
}
