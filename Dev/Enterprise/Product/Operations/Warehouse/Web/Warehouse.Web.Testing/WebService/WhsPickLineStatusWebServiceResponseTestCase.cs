namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPickLineStatusWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		protected override void TestPropertiesCore()
		{
			base.TestPropertiesCore();

			var response = new WhsPickLineStatusWebServiceResponse();
			AssertEquals(false, response.IsPickLineAlive);
			response.IsPickLineAlive = true;
			AssertEquals(true, response.IsPickLineAlive);
			response.IsPickLineAlive = false;
			AssertEquals(false, response.IsPickLineAlive);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsPickLineStatusWebServiceResponse();
		}

		protected new WhsPickLineStatusWebServiceResponse Response
		{
			get { return (WhsPickLineStatusWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
