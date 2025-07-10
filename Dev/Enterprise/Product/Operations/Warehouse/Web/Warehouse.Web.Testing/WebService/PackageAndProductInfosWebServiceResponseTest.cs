using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class PackageAndProductInfosWebServiceResponseTest : WebServiceResponseTestCase
	{
		public void TestInfos()
		{
			var response = new PackageAndProductInfosWebServiceResponse();
			AssertNull(response.Package);
			AssertNull(response.ProductInfos);
			AssertEquals(false, response.IsInvalidToteId);

			var packageInfo = new PackageForPackingInfo();
			response.Package = packageInfo;
			AssertEquals(packageInfo, response.Package);

			var productInfo = new WhsPackageProductInfo();
			response.ProductInfos = new[] { productInfo };
			AssertEquals(1, response.ProductInfos.Length);
			AssertEquals(productInfo, response.ProductInfos[0]);

			response.IsInvalidToteId = true;
			AssertEquals(true, response.IsInvalidToteId);
		}

		protected override WebServiceResponse GetNewResponse()
		{
			return new PackageAndProductInfosWebServiceResponse();
		}

		protected new PackageAndProductInfosWebServiceResponse Response
		{
			get
			{
				return (PackageAndProductInfosWebServiceResponse)base.Response;
			}
		}
	}
}
