namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsReleaseCapturedSerialsWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		public void TestReleaseCapturedSerials()
		{
			AssertNull(Response.ReleaseCapturedSerials);

			var releaseCapturedSerials = new[] { "ABC", "DEF" };
			Response.ReleaseCapturedSerials = releaseCapturedSerials;
			AssertContainsExactElementsInAnyOrder(new[] { "ABC", "DEF" }, Response.ReleaseCapturedSerials);
		}

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsReleaseCapturedSerialsWebServiceResponse();
		}

		protected new WhsReleaseCapturedSerialsWebServiceResponse Response => (WhsReleaseCapturedSerialsWebServiceResponse)base.Response;
	}
}
