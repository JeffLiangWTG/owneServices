namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsSecurityAccessWebServiceResponse : WebServiceResponse
	{
		#region Properties

		public bool HasAccess { get; set; }
		public string Message { get; set; }

		#endregion
	}
}
