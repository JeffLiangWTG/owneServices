namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsStocktakeWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestStocktakeHeader()
		{
			AssertNull(Response.Stocktake);
			AssertNotNull(Response.LinesToCount);
			AssertNotNull(Response.LocationsToCount);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsStocktakeWebServiceResponse();
		}

		protected new WhsStocktakeWebServiceResponse Response
		{
			get
			{
				return (WhsStocktakeWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
