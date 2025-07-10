using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class PackageForPackingWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestToteInfo()
		{
			var response = new PackageForPackingWebServiceResponse();
			AssertEquals(0, response.PackagesForPackingInfo.Count);

			var packageInfo = new PackageForPackingInfo();
			response.PackagesForPackingInfo = new PackageForPackingInfoCollection(new[] { packageInfo });
			AssertEquals(1, response.PackagesForPackingInfo.Count);
			AssertEquals(packageInfo, response.PackagesForPackingInfo[0]);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new PackageForPackingWebServiceResponse();
		}

		protected new PackageForPackingWebServiceResponse Response
		{
			get
			{
				return (PackageForPackingWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
