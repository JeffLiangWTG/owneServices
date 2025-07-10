using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class OrgHeadersWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestProperties

		protected override void TestPropertiesCore()
		{
			AssertNull(Response.Organisations);

			Response.Organisations = new OrgHeaderInfo[] { new OrgHeaderInfo() };
			AssertNotNull(Response.Organisations);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new OrgHeadersWebServiceResponse();
		}

		protected new OrgHeadersWebServiceResponse Response
		{
			get { return (OrgHeadersWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
