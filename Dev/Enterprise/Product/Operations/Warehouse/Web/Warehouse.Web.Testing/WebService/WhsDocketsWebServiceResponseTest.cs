using System;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsDocketsWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestDockets

		public void TestDockets()
		{
			var response = new WhsDocketsWebServiceResponse();
			AssertNull(response.Dockets);

			var docket = new WhsDocketInfo();
			response.Dockets = new[] { docket };
			AssertContainsExactElementsInAnyOrder(new[] { docket }, response.Dockets);
		}

		#endregion

		#region TestHeldCodes

		public void TestHeldCodes()
		{
			AssertNull(Response.HeldCodes);

			var heldCode = new CodeDescriptionPairInfo();
			Response.HeldCodes = new[] { heldCode };
			AssertContainsExactElementsInAnyOrder(new[] { heldCode }, Response.HeldCodes);
		}

		#endregion

		#region TestProperties_DockDoorLocation

		public void TestProperties_DockDoorLocation()
		{
			AssertEquals(false, Response.WarehouseHasSingleDockDoorLocation);
			AssertNull(Response.SingleDockDoorLocation);
			AssertEquals(Guid.Empty, Response.SingleDockDoorLocationPK);

			Response.WarehouseHasSingleDockDoorLocation = true;
			Response.SingleDockDoorLocation = "ABC";
			var locationPK = Guid.NewGuid();
			Response.SingleDockDoorLocationPK = locationPK;

			AssertEquals(true, Response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("ABC", Response.SingleDockDoorLocation);
			AssertEquals(locationPK, Response.SingleDockDoorLocationPK);
		}

		#endregion

		#region TestShowStockOnHandWarningOnPutaway

		public void TestShowStockOnHandWarningOnPutaway()
		{
			AssertEquals(false, Response.ShowStockOnHandWarningOnPutaway);

			Response.ShowStockOnHandWarningOnPutaway = true;
			AssertEquals(true, Response.ShowStockOnHandWarningOnPutaway);

			Response.ShowStockOnHandWarningOnPutaway = false;
			AssertEquals(false, Response.ShowStockOnHandWarningOnPutaway);
		}

		#endregion

		#region TestCanDuplicatePreviousLine

		public void TestCanDuplicatePreviousLine()
		{
			AssertEquals(false, Response.CanDuplicatePreviousLine);

			Response.CanDuplicatePreviousLine = true;
			AssertEquals(true, Response.CanDuplicatePreviousLine);

			Response.CanDuplicatePreviousLine = false;
			AssertEquals(false, Response.CanDuplicatePreviousLine);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsDocketsWebServiceResponse();
		}

		protected new WhsDocketsWebServiceResponse Response
		{
			get { return (WhsDocketsWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
