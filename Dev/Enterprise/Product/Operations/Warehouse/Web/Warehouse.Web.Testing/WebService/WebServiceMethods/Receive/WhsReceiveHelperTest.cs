using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class WhsReceiveHelperTest : WhsTestCaseWithFactory
	{
		#region TestSetSingleDockDoorLocationDetails

		public void TestSetSingleDockDoorLocationDetails_SingleDockDoorLocation()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 5, 1);
			Factory.Save();

			var response = new WhsDocketWebServiceResponse();
			WhsReceiveHelper.SetSingleDockDoorLocationDetails(Factory, response, "WHS");

			var expectedDDL = whs.DefaultInboundDockDoorLocation;
			AssertEquals("Should set flag to true.", true, response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Should successfully set single Dock Door Location string.", expectedDDL.WLV_LocationString, response.SingleDockDoorLocation);
			AssertEquals("Should successfully set single Dock Door Location PK.", expectedDDL.PK, response.SingleDockDoorLocationPK);
		}

		public void TestSetSingleDockDoorLocationDetails_MultipleDockDoorLocation()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 5, 1);
			var dockDoorLocationType = Helper.CreateLocationType("TS1", "Test1", false, 0, LocationClasses.Codes.DDL);

			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WLT_LocationType = dockDoorLocationType.PK;
			locations[1].WLV_WLT_LocationType = dockDoorLocationType.PK;
			locations[2].WLV_WLT_LocationType = dockDoorLocationType.PK;

			Factory.Save();

			var response = new WhsDocketWebServiceResponse();
			WhsReceiveHelper.SetSingleDockDoorLocationDetails(Factory, response, "WHS");

			var expectedDDL = whs.DefaultInboundDockDoorLocation;
			AssertEquals("Should set flag to false.", false, response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Should not set single Dock Door Location string.", null, response.SingleDockDoorLocation);
			AssertEquals("Should not set single Dock Door Location PK.", Guid.Empty, response.SingleDockDoorLocationPK);
		}

		public void TestSetSingleDockDoorLocationDetails_SingleDockDoorLocationUserFriendly()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("WHS", 2, 2, 2, shouldPreGenerateDDL: false);
			Helper.CreateRowAndGenerateLocations(whs, "Z", 4, 3, 2);
			Factory.Save();

			var dockdoorLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, "DDL"));
			var dockdoorLocation = whs.FindLocation("Z040302");
			dockdoorLocation.WLV_WLT_LocationType = dockdoorLocationType.PK;
			dockdoorLocation.WLV_LocationStatus = "NOR";
			Factory.Save();

			var defaultInboundDockDoorLocation = whs.DefaultInboundDockDoorLocation;
			var defaultOutboundDockDoorLocation = whs.DefaultOutboundDockDoorLocation;
			whs.WW_DefaultInboundDockDoor = dockdoorLocation.PK;
			whs.WW_DefaultOutboundDockDoor = dockdoorLocation.PK;
			Factory.Save();

			defaultInboundDockDoorLocation.Delete();
			defaultOutboundDockDoorLocation.Delete();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var response = new WhsDocketWebServiceResponse();
			WhsReceiveHelper.SetSingleDockDoorLocationDetails(newFactory, response, "WHS");

			AssertEquals("Should set flag to true.", true, response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Should successfully set single Dock Door Location string.", "Z040302", response.SingleDockDoorLocation);
			AssertEquals("Should successfully set single Dock Door Location string.", "Z-04-03-02", response.SingleDockDoorLocation_UserFriendly);
			AssertEquals("Should successfully set single Dock Door Location PK.", dockdoorLocation.PK, response.SingleDockDoorLocationPK);
		}

		#endregion
	}
}
