using System;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class BeginRFPickByLabelTaskWebServiceResponseTest : WebServiceResponseTestCase
	{
		public void TestBeginRFPickByLabelTaskWebServiceResponse()
		{
			var response = new BeginRFPickByLabelTaskWebServiceResponse();
			Assert("Response should implement appropriate response type.", response is WhsPickByLabelActiveJobWebServiceResponse);

			AssertEquals(Guid.Empty, response.WhsPickPK);

			var pickPK = Guid.NewGuid();

			response.WhsPickPK = pickPK;

			AssertEquals(pickPK, response.WhsPickPK);
		}
	}
}
