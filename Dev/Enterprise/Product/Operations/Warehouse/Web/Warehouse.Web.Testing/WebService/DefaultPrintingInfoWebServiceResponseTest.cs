using System;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class DefaultPrintingInfoWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestDefaultPrinter

		public void TestDefaultPrinter()
		{
			var response = new DefaultPrintingInfoWebServiceResponse();
			AssertEquals(Guid.Empty, response.DefaultPrinter);

			var guid = Guid.NewGuid();
			response.DefaultPrinter = guid;
			AssertEquals(guid, response.DefaultPrinter);
		}

		#endregion

		#region TestNumberOfLabelsToPrintOnClose

		public void TestNumberOfLabelsToPrintOnClose()
		{
			var response = new DefaultPrintingInfoWebServiceResponse();
			AssertEquals(0, response.NumberOfLabelsToPrintOnClose);

			response.NumberOfLabelsToPrintOnClose = 1;
			AssertEquals(1, response.NumberOfLabelsToPrintOnClose);
		}

		#endregion

		#region TestNumberOfLabelsToPrintOnNew

		public void TestNumberOfLabelsToPrintOnNew()
		{
			var response = new DefaultPrintingInfoWebServiceResponse();
			AssertEquals(0, response.NumberOfLabelsToPrintOnNew);

			response.NumberOfLabelsToPrintOnNew = 1;
			AssertEquals(1, response.NumberOfLabelsToPrintOnNew);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new DefaultPrintingInfoWebServiceResponse();
		}

		#endregion
	}
}
