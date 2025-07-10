namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsInventoryHeldCodesWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertNull(Response.WhsInventoryHeldCodes);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsInventoryHeldCodesWebServiceResponse();
		}

		protected new WhsInventoryHeldCodesWebServiceResponse Response
		{
			get { return (WhsInventoryHeldCodesWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
