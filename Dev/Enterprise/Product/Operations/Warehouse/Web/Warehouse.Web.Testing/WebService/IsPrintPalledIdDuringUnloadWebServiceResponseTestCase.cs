namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class IsPrintPalledIdDuringUnloadWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestProperties

		protected override void TestPropertiesCore()
		{
			var response = new IsPrintPalletIdDuringUnloadWebServiceResponse();
			AssertEquals(false, response.IsPrintPalletIDDuringUnload);

			response.IsPrintPalletIDDuringUnload = true;
			AssertEquals(true, response.IsPrintPalletIDDuringUnload);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse() => new IsPrintPalletIdDuringUnloadWebServiceResponse();

		protected new IsPrintPalletIdDuringUnloadWebServiceResponse Response => (IsPrintPalletIdDuringUnloadWebServiceResponse)base.Response;

		#endregion
	}
}
