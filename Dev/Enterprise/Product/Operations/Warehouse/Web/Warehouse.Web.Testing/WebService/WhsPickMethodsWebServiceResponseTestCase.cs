using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPickMethodsWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestPickMethods()
		{
			AssertNotNull(Response.PickMethods);
			AssertEquals(0, Response.PickMethods.Count);

			WhsPickMethodInfo pickMethod1 = new WhsPickMethodInfo();
			WhsPickMethodInfo pickMethod2 = new WhsPickMethodInfo();
			AssertNotEquals(pickMethod1, pickMethod2);
			Response.PickMethods.Add(pickMethod1);
			Response.PickMethods.Add(pickMethod2);
			AssertCollectionContains(pickMethod1, Response.PickMethods);
			AssertCollectionContains(pickMethod2, Response.PickMethods);

			WhsPickMethodInfoCollection pickMethods = new WhsPickMethodInfoCollection();
			pickMethods.Add(new WhsPickMethodInfo());
			pickMethods.Add(new WhsPickMethodInfo());
			AssertNotEquals(pickMethods, Response.PickMethods);
			Response.PickMethods = pickMethods;
			AssertEquals(pickMethods, Response.PickMethods);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsPickMethodsWebServiceResponse();
		}

		protected new WhsPickMethodsWebServiceResponse Response
		{
			get
			{
				return (WhsPickMethodsWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
