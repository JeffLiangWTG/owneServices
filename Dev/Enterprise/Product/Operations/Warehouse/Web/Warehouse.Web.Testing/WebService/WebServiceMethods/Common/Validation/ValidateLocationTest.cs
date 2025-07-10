using System;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ValidateLocationTest : WhsSecureServiceTestCase
	{
		#region TestValidateLocation

		public void TestValidateLocation()
		{
			var whs = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 2);
			Helper.Factory.Save();

			var clientName = "Client1";
			var client = Helper.CreateClient(clientName);
			Helper.CreateClient("C2");

			var partA = Helper.CreateProduct(client, "ProductA");
			var partB = Helper.CreateProduct(client, "ProductB");

			var fixLocationType = Helper.CreateLocationType("XY1", "Test1", false, 1, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("XY2", "Test2", false, 0, LocationClasses.Codes.NOR);

			var fixedLocation = row.Locations[0];
			var normalLocation = row.Locations[1];
			fixedLocation.WLV_WLT_LocationType = fixLocationType.PK;
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var receive = Helper.CreateWhsReceive(client, whs);
			Helper.CreateWhsReceiveInventoryLine(receive, partA, 10m, fixedLocation);
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(partA), client, whs, fixedLocation.ToLocationString());
			var expectedMessage = "Please enter valid data Empty Location: '{0}', Client Code: '{1}', Empty Product: '{2}'";

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			webService1.AllowedToRunServiceHasBeenCalled = false;

			var errorResponse = webService1.ValidateLocation(Guid.Empty, clientName, partA.PK.ToGuid());
			AssertEquals(string.Format(expectedMessage, "Yes", clientName, "No"), errorResponse.ErrorMessage);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			errorResponse = webService2.ValidateLocation(fixedLocation.PK.ToGuid(), "", partA.PK.ToGuid());
			AssertEquals(string.Format(expectedMessage, "No", "", "No"), errorResponse.ErrorMessage);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			errorResponse = webService3.ValidateLocation(fixedLocation.PK.ToGuid(), clientName, Guid.Empty);
			AssertEquals(string.Format(expectedMessage, "No", clientName, "Yes"), errorResponse.ErrorMessage);

			var webService4 = GetNewWebService();
			webService4.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var responseA = webService4.ValidateLocation(fixedLocation.PK.ToGuid(), clientName, partB.PK.ToGuid());
			AssertEquals("This location is a fixed pick face location and 'PRODUCTB' is not assigned to this location.", responseA.ErrorMessage);

			var webService5 = GetNewWebService();
			webService5.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var responseB = webService5.ValidateLocation(normalLocation.PK.ToGuid(), clientName, partB.PK.ToGuid());
			AssertNull(responseB.ErrorMessage);

			var webService6 = GetNewWebService();
			webService6.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var responseC = webService6.ValidateLocation(fixedLocation.PK.ToGuid(), clientName, partA.PK.ToGuid());
			AssertNull(responseC.ErrorMessage);
			AssertSuccessfulResponse(responseC, webService6);
		}

		#endregion
	}
}
