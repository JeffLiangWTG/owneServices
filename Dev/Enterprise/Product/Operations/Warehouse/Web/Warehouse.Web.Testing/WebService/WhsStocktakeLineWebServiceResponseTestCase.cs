namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsStocktakeLineWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestStocktakeLine()
		{
			AssertNull(Response.StocktakeLine);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsStocktakeLineWebServiceResponse();
		}

		protected new WhsStocktakeLineWebServiceResponse Response
		{
			get
			{
				return (WhsStocktakeLineWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
