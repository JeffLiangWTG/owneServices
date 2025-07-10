using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsOrdersWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestProperties

		protected override void TestPropertiesCore()
		{
			AssertNull(Response.Orders);

			var orders = new WhsDocketInfo[] { new WhsDocketInfo() };
			Response.Orders = orders;
			AssertEquals(orders, Response.Orders);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse() => new WhsOrdersWebServiceResponse();

		protected new WhsOrdersWebServiceResponse Response => (WhsOrdersWebServiceResponse)base.Response;

		#endregion
	}
}
