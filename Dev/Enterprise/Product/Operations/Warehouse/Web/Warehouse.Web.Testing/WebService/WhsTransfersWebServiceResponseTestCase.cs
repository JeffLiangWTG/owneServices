using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsTransfersWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestProperties

		protected override void TestPropertiesCore()
		{
			AssertNull(Response.Transfers);
			AssertNull(Response.SourceLocation);
			AssertEquals(false, Response.IsStockCommittedOrReserved);
			AssertEquals(false, Response.ShowStockOnHandWarningOnPutaway);

			var transfers = new WhsDocketInfo[] { new WhsDocketInfo() };
			Response.Transfers = transfers;
			Response.SourceLocation = "A-1";
			Response.IsStockCommittedOrReserved = true;
			Response.ShowStockOnHandWarningOnPutaway = true;
			AssertEquals(transfers, Response.Transfers);
			AssertEquals("A-1", Response.SourceLocation);
			AssertEquals(true, Response.IsStockCommittedOrReserved);
			AssertEquals(true, Response.ShowStockOnHandWarningOnPutaway);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsTransfersWebServiceResponse();
		}

		protected new WhsTransfersWebServiceResponse Response
		{
			get { return (WhsTransfersWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
