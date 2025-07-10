using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class PackageProductInfosWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		#region TestPackageID

		public void TestPackageID()
		{
			AssertNotNull(Response.PackageID);
			AssertEquals("", Response.PackageID);

			Response.PackageID = "1234";
			AssertEquals("1234", Response.PackageID);

			Response.PackageID = "4321";
			AssertEquals("4321", Response.PackageID);
		}

		#endregion

		#region TestProductInfos

		public void TestProductInfos()
		{
			AssertNull(Response.ProductInfos);

			Response.ProductInfos = new[] { new WhsPackageProductInfo() };
			AssertNotNull(Response.ProductInfos);
		}

		#endregion

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new PackageProductInfosWebServiceResponse();
		}

		protected new PackageProductInfosWebServiceResponse Response
		{
			get { return (PackageProductInfosWebServiceResponse)base.Response; }
		}

		#endregion
	}
}
