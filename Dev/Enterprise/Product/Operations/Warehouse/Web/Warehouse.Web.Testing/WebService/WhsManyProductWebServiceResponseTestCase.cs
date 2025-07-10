namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsManyProductWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertNull(Response.Products);
			AssertNull(Response.ProductPartAttributes);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsManyProductWebServiceResponse();
		}

		protected new WhsManyProductWebServiceResponse Response
		{
			get
			{
				return (WhsManyProductWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
