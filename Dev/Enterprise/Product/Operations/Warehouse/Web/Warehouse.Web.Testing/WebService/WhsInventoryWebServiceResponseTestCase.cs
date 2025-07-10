using System;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsInventoryWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestInventoryPK

		public void TestInventoryPK()
		{
			var response = new WhsInventoryWebServiceResponse();
			AssertEquals(Guid.Empty, response.InventoryLinePK);

			var inventoryPK = Guid.NewGuid();
			response.InventoryLinePK = inventoryPK;
			AssertEquals(inventoryPK, response.InventoryLinePK);
		}

		#endregion
	}
}
