namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class PalletIDWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestProperties

		protected override void TestPropertiesCore()
		{
			var response = new PalletIDWebServiceResponse();
			AssertEquals("", response.PalletID);
			AssertEquals(0, response.UpdatedCountToBuildFrom);

			response.PalletID = "ABC-1";
			response.UpdatedCountToBuildFrom = 1;
			AssertEquals("ABC-1", response.PalletID);
			AssertEquals(1, response.UpdatedCountToBuildFrom);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse() => new PalletIDWebServiceResponse();

		protected new PalletIDWebServiceResponse Response => (PalletIDWebServiceResponse)base.Response;

		#endregion
	}
}
