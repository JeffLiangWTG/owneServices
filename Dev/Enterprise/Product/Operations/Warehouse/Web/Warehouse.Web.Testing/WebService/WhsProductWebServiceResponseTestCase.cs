namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsProductWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestConstructor()
		{
			AssertNull(Response.Product);
			AssertNull(Response.ProductPartAttributes);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsProductWebServiceResponse();
		}

		protected new WhsProductWebServiceResponse Response
		{
			get
			{
				return (WhsProductWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
