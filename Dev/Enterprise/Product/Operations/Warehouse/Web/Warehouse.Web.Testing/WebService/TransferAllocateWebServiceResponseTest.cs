using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class TransferAllocateWebServiceResponseTest : WebServiceResponseTestCase
	{
		protected override void TestPropertiesCore()
		{
			var response = new TransferAllocateWebServiceResponse();
			AssertEquals(false, response.ShowStockOnHandWarningOnPutaway);

			response.Transfer = new WhsDocketInfo();
			response.NewTransferLines = new WhsDocketLineInfoCollection();
			response.IsValidLocation = true;
			response.IsValidPalletID = true;
			response.IsValidProduct = true;
			response.IsValidInventoryHeldCode = true;
			response.IsValidAttributes = true;
			response.ErrorMessage = "ERROR";
			response.TotalQuantityAvailableToPick = 5.123m;
			response.TotalPickLineQuantity = 6.234m;
			response.ShowStockOnHandWarningOnPutaway = true;

			AssertNotNull(response.Transfer);
			AssertNotNull(response.NewTransferLines);
			AssertEquals(true, response.IsValidLocation);
			AssertEquals(true, response.IsValidPalletID);
			AssertEquals(true, response.IsValidProduct);
			AssertEquals(true, response.IsValidInventoryHeldCode);
			AssertEquals(true, response.IsValidAttributes);
			AssertEquals("ERROR", response.ErrorMessage);
			AssertEquals(5.123m, response.TotalQuantityAvailableToPick);
			AssertEquals(6.234m, response.TotalPickLineQuantity);
			AssertEquals(true, response.ShowStockOnHandWarningOnPutaway);
		}

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new TransferAllocateWebServiceResponse();
		}

		protected new TransferAllocateWebServiceResponse Response
		{
			get { return (TransferAllocateWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
