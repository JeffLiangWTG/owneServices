using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsAreasWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestAreas()
		{
			AssertNotNull(Response.Areas);
			AssertEquals(0, Response.Areas.Count);

			WhsAreaInfo area1 = new WhsAreaInfo();
			WhsAreaInfo area2 = new WhsAreaInfo();
			AssertNotEquals(area1, area2);
			Response.Areas.Add(area1);
			Response.Areas.Add(area2);
			AssertCollectionContains(area1, Response.Areas);
			AssertCollectionContains(area2, Response.Areas);

			WhsAreaInfoCollection areas = new WhsAreaInfoCollection();
			areas.Add(new WhsAreaInfo());
			areas.Add(new WhsAreaInfo());
			AssertNotEquals(areas, Response.Areas);
			Response.Areas = areas;
			AssertEquals(areas, Response.Areas);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsAreasWebServiceResponse();
		}

		protected new WhsAreasWebServiceResponse Response
		{
			get
			{
				return (WhsAreasWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
