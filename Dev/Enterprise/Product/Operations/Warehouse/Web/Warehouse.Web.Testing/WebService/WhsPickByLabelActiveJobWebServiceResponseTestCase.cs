namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPickByLabelActiveJobWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Implementation

		protected override WebServiceResponse GetNewResponse() => new WhsPickByLabelActiveJobWebServiceResponse();

		#endregion
	}
}
