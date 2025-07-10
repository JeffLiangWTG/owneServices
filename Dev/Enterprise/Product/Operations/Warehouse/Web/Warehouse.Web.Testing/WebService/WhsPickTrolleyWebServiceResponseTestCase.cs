using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPickTrolleyWebServiceResponseTestCase : WhsPickJobWebServiceResponseTestCase<WhsPickTrolleyWebServiceResponse, TrolleyJobInfo>
	{
		#region Implementation

		protected override WhsPickTrolleyWebServiceResponse GetNewPickJobWebServiceResponse()
		{
			return new WhsPickTrolleyWebServiceResponse();
		}

		#endregion
	}
}
