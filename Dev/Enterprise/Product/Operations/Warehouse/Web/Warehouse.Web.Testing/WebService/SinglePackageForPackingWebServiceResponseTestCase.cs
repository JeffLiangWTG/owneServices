using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class SinglePackageForPackingWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestPackageForPackingInfo()
		{
			var response = new SinglePackageForPackingWebServiceResponse();
			AssertNull(response.PackageForPackingInfo);

			var packageInfo = new PackageForPackingInfo();
			response.PackageForPackingInfo = packageInfo;
			AssertEquals(packageInfo, response.PackageForPackingInfo);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new SinglePackageForPackingWebServiceResponse();
		}

		protected new SinglePackageForPackingWebServiceResponse Response => (SinglePackageForPackingWebServiceResponse)base.Response;

		#endregion
	}
}
