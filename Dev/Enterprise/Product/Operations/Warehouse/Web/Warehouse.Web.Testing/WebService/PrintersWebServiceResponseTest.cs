using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PrintersWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestPrinters

		public void TestPrinters()
		{
			var response = new PrintersWebServiceResponse();
			AssertNull(response.Printers);

			response.Printers = new[] { new PrinterInfo() };
			AssertNotNull(response.Printers);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new PrintersWebServiceResponse();
		}

		#endregion
	}
}
