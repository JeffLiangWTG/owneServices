using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPickByLabelWebServiceResponseTestCase : WhsPickJobWebServiceResponseTestCase<WhsPickByLabelWebServiceResponse, PickByLabelInfo>
	{
		#region Implementation

		protected override WhsPickByLabelWebServiceResponse GetNewPickJobWebServiceResponse()
		{
			return new WhsPickByLabelWebServiceResponse();
		}

		#endregion
	}
}
