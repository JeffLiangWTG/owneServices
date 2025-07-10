using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPalletWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		#region TestPalletID

		public void TestPalletIDs()
		{
			AssertNotNull(Response.PalletID);
			AssertEquals("", Response.PalletID);

			Response.PalletID = "1234";
			AssertEquals("1234", Response.PalletID);

			Response.PalletID = "4321";
			AssertEquals("4321", Response.PalletID);
		}

		public void TestPalletInfo()
		{
			AssertNotNull(Response.PalletInfo);

			Response.PalletInfo.ProductCode = "P1";
			AssertEquals("P1", Response.PalletInfo.ProductCode);

			Response.PalletInfo.PalletID = "1234";
			AssertEquals("1234", Response.PalletInfo.PalletID);
			AssertEquals("1234", Response.PalletID);
		}

		#endregion

		#region TestInventory

		public void TestInventory()
		{
			AssertNotNull(Response.Inventory);
			AssertEquals(0, Response.Inventory.InventoryLineInfos.Count);

			WhsInventoryLineInfoCollection collection = Response.Inventory;
			AssertEquals(collection, Response.Inventory);

			Response.Inventory.InventoryLineInfos.Add(new WhsInventoryLineInfo());
			AssertEquals(1, Response.Inventory.InventoryLineInfos.Count);

			collection = new WhsInventoryLineInfoCollection();
			collection.InventoryLineInfos.Add(new WhsInventoryLineInfo());
			collection.InventoryLineInfos.Add(new WhsInventoryLineInfo());
			AssertNotEquals(collection, Response.Inventory);
			Response.Inventory = collection;
			AssertEquals(collection, Response.Inventory);
			AssertEquals(2, Response.Inventory.InventoryLineInfos.Count);
		}

		#endregion

		#region TestReferencesOfReceiveThatCouldBeAutoFinalised

		public void TestReferencesOfReceiveThatCouldBeAutoFinalised()
		{
			AssertNotNull(Response);

			List<string> list = new List<string>();
			Response.ReferencesOfReceiveThatCouldBeAutoFinalised = list;
			AssertEquals(list, Response.ReferencesOfReceiveThatCouldBeAutoFinalised);
		}

		#endregion

		#region TestAllowToOverrideLocation

		public void TestAllowToOverrideLocation()
		{
			AssertEquals(false, Response.AllowToOverrideLocation);

			Response.AllowToOverrideLocation = true;
			AssertEquals(true, Response.AllowToOverrideLocation);
		}

		#endregion

		#region TestShowStockOnHandWarningOnPutaway

		public void TestShowStockOnHandWarningOnPutaway()
		{
			var response = new WhsPalletWebServiceResponse();
			AssertEquals(false, response.ShowStockOnHandWarningOnPutaway);

			response.ShowStockOnHandWarningOnPutaway = true;
			AssertEquals(true, response.ShowStockOnHandWarningOnPutaway);

			response.ShowStockOnHandWarningOnPutaway = false;
			AssertEquals(false, response.ShowStockOnHandWarningOnPutaway);
		}

		#endregion

		#region TestTaskPK

		public void TestTaskPK()
		{
			var response = new WhsPalletWebServiceResponse();
			AssertEquals("TaskPK correct", Guid.Empty, response.TaskPK);

			var testPK = Guid.NewGuid();
			response.TaskPK = testPK;
			AssertEquals("TaskPK correct", testPK, response.TaskPK);
		}

		#endregion

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsPalletWebServiceResponse();
		}

		protected new WhsPalletWebServiceResponse Response
		{
			get { return (WhsPalletWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
