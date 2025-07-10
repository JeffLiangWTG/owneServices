using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsCartonSizeWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestCartonSizeInfo()
		{
			var response = new WhsCartonSizeWebServiceResponse();
			AssertNull(response.CartonSizeInfo);

			var cartonSizeInfo = new WhsCartonSizeInfo();
			response.CartonSizeInfo = cartonSizeInfo;
			AssertEquals(cartonSizeInfo, response.CartonSizeInfo);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsCartonSizeWebServiceResponse();
		}

		protected new WhsCartonSizeWebServiceResponse Response
		{
			get
			{
				return (WhsCartonSizeWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
