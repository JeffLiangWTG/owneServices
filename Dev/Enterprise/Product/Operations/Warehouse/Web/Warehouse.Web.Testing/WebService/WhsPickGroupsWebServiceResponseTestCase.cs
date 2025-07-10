using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPickGroupsWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		protected override void TestPropertiesCore()
		{
			base.TestPropertiesCore();

			var response = new WhsPickGroupsWebServiceResponse();
			response.PickGroups = new WhsPickGroupInfoCollection();
			AssertNotNull(response.PickGroups);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsPickGroupsWebServiceResponse();
		}

		protected new WhsPickGroupsWebServiceResponse Response
		{
			get { return (WhsPickGroupsWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
