using System.ComponentModel;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderCollection))]
	public class WhsOrderCollectionTest : WhsPickableDocketCollectionTest<WhsOrderCollection>
	{
		#region TestSortByCrossDockLocation

		public void TestSortByCrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockdoorLocation1 = data.Whs1.FindLocation("A-1");
			dockdoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var dockdoorLocation2 = data.Whs1.FindLocation("A-2");
			dockdoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var dockdoorLocation3 = data.Whs1.FindLocation("A-10");
			dockdoorLocation3.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order1.WD_WL_CrossDock = dockdoorLocation1.PK;
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			order2.WD_WL_CrossDock = dockdoorLocation2.PK;
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			order3.WD_WL_CrossDock = dockdoorLocation3.PK;
			Factory.Save();

			var orderCollection = new WhsOrderCollection(Factory);
			orderCollection.ApplySort(nameof(WhsDocket.WD_WL_CrossDock), ListSortDirection.Ascending);
			AssertEquals("A-1", orderCollection[0].CrossDockLocation.ToLocationString());
			AssertEquals("A-2", orderCollection[1].CrossDockLocation.ToLocationString());
			AssertEquals("A-10", orderCollection[2].CrossDockLocation.ToLocationString());

			orderCollection.ApplySort(nameof(WhsDocket.WD_WL_CrossDock), ListSortDirection.Descending);
			AssertEquals("A-10", orderCollection[0].CrossDockLocation.ToLocationString());
			AssertEquals("A-2", orderCollection[1].CrossDockLocation.ToLocationString());
			AssertEquals("A-1", orderCollection[2].CrossDockLocation.ToLocationString());
		}

		#endregion

		#region Implementation

		protected override string[] DocketTypesForTest
		{
			get { return new[] { DocketType.Codes.Order }; }
		}

		protected override WhsOrderCollection GetCollectionToTest()
		{
			return new WhsOrderCollection(Factory);
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}
}
