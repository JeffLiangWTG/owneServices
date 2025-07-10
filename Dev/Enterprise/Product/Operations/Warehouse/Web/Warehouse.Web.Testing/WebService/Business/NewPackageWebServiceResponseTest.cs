using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class NewPackageWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestNewPackage

		public void TestNewPackage()
		{
			var response = new NewPackageWebServiceResponse();
			AssertNull(response.NewPackage);

			response.NewPackage = new PackageInfo();
			AssertNotNull(response.NewPackage);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new NewPackageWebServiceResponse();
		}

		#endregion
	}
}
