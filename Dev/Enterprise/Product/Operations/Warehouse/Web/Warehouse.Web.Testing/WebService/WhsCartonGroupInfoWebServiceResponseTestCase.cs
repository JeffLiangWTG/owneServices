using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsCartonGroupInfoWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestCartonGroup

		public void TestCartonGroup()
		{
			var response = new WhsCartonGroupInfoWebServiceResponse();
			AssertNull(response.CartonGroups);

			var cartonGroups = new WhsCartonGroupInfoCollection();
			response.CartonGroups = cartonGroups;
			AssertEquals(cartonGroups, response.CartonGroups);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsCartonGroupInfoWebServiceResponse();
		}

		protected new WhsCartonGroupInfoWebServiceResponse Response
		{
			get { return (WhsCartonGroupInfoWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
