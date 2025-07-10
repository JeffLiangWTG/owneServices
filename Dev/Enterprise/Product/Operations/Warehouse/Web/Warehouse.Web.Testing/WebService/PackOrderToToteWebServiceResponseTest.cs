using System;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class PackOrderToPackageWebServiceResponseTest : WebServiceResponseTestCase
	{
		public void TestNewTotePK()
		{
			var response = new PackOrderToPackageWebServiceResponse();
			AssertEquals(Guid.Empty, response.NewPackagePK);

			var newTotePK = Guid.NewGuid();
			response.NewPackagePK = newTotePK;
			AssertEquals(newTotePK, response.NewPackagePK);
		}

		protected override WebServiceResponse GetNewResponse()
		{
			return new PackOrderToPackageWebServiceResponse();
		}

		protected new PackOrderToPackageWebServiceResponse Response
		{
			get
			{
				return (PackOrderToPackageWebServiceResponse)base.Response;
			}
		}
	}
}
