using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WarehouseInfoWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestWarehouseInfos

		public void TestWarehouseInfos()
		{
			var response = new WarehouseInfoWebServiceResponse();
			AssertNull(response.WarehouseInfos);

			var warehouseInfos = new WarehouseInfoCollection();
			warehouseInfos.Add(new WarehouseInfo { Code = "WHS" });
			warehouseInfos.Add(new WarehouseInfo { Code = "TST" });

			response.WarehouseInfos = warehouseInfos;
			AssertContainsExactElementsInAnyOrder(warehouseInfos, response.WarehouseInfos);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WarehouseInfoWebServiceResponse();
		}

		#endregion
	}
}
