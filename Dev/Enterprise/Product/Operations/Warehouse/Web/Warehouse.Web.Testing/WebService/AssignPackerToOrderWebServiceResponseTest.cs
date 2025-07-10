using Enterprise.Warehouse.Web.WebService;
using Enterprise.Warehouse.Web.WebService.Testing;

namespace Enterprise.Warehouse.Web.Testing.WebService
{
	public class AssignPackerToOrderWebServiceResponseTest : WebServiceResponseTestCase
	{
		protected override void TestPropertiesCore()
		{
			var response = new AssignPackerToOrderWebServiceResponse();
			AssertEquals(false, response.IsAssignedToAnotherPacker);
			AssertNull(response.AssignedPackerCode);

			response.IsAssignedToAnotherPacker = true;
			AssertEquals(true, response.IsAssignedToAnotherPacker);

			response.AssignedPackerCode = "ABC";
			AssertEquals("ABC", response.AssignedPackerCode);
		}

		protected override WebServiceResponse GetNewResponse() => new AssignPackerToOrderWebServiceResponse();

		protected new AssignPackerToOrderWebServiceResponse Response => (AssignPackerToOrderWebServiceResponse)base.Response;
	}
}
