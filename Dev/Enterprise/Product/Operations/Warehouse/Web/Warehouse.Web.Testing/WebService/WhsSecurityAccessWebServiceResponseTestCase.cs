namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsSecurityAccessWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		protected override void TestPropertiesCore()
		{
			var response = new WhsSecurityAccessWebServiceResponse();
			response.HasAccess = true;
			response.Message = "BLABLA";

			AssertEquals("HasAccess", true, response.HasAccess);
			AssertEquals("Message", "BLABLA", response.Message);
		}

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsSecurityAccessWebServiceResponse();
		}

		protected new WhsSecurityAccessWebServiceResponse Response
		{
			get
			{
				return (WhsSecurityAccessWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
