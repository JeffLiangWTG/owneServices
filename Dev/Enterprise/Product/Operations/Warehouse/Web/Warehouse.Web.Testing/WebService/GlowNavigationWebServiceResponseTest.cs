using Enterprise.Warehouse.Web.WebService.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class GlowNavigationWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestGlowSSONavigationUrl

		public void TestGlowSSONavigationUrl()
		{
			var response = new GlowNavigationWebServiceResponse();
			AssertEquals("Default response is empty.", string.Empty, response.GlowSSONavigationUrl);

			response.GlowSSONavigationUrl = "http://abc.com.au";
			AssertEquals("Updated value is correct.", "http://abc.com.au", response.GlowSSONavigationUrl);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new GlowNavigationWebServiceResponse();
		}

		#endregion
	}
}
