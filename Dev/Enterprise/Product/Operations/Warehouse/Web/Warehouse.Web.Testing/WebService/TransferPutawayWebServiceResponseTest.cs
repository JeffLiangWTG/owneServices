namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class TransferPutawayWebServiceResponseTest : WebServiceResponseTestCase
	{
		protected override void TestPropertiesCore()
		{
			var response = new TransferPutawayWebServiceResponse();
			response.IsFullPalletIDTransferred = true;
			response.IsSingleProductTransferred = true;
			response.IsValidSourcePalletID = true;
			response.IsValidDestPalletID = true;
			response.IsValidProduct = true;
			response.IsValidDestLocation = true;
			response.IsAllTransferLinesTransferredOrFinalised = true;
			response.IsValidInventoryHeldCode = true;
			response.IsValidAttributes = true;
			response.ErrorMessage = "ERROR";
			response.TotalQuantityAvailableForPutaway = 5.123m;
			response.TotalQuantityTransferred = 6.234m;

			AssertEquals(true, response.IsFullPalletIDTransferred);
			AssertEquals(true, response.IsSingleProductTransferred);
			AssertEquals(true, response.IsValidSourcePalletID);
			AssertEquals(true, response.IsValidDestPalletID);
			AssertEquals(true, response.IsValidProduct);
			AssertEquals(true, response.IsValidDestLocation);
			AssertEquals(true, response.IsAllTransferLinesTransferredOrFinalised);
			AssertEquals(true, response.IsValidInventoryHeldCode);
			AssertEquals(true, response.IsValidAttributes);
			AssertEquals("ERROR", response.ErrorMessage);
			AssertEquals(5.123m, response.TotalQuantityAvailableForPutaway);
			AssertEquals(6.234m, response.TotalQuantityTransferred);
		}

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new TransferPutawayWebServiceResponse();
		}

		protected new TransferPutawayWebServiceResponse Response
		{
			get { return (TransferPutawayWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
