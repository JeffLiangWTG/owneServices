namespace Enterprise.Warehouse.Web.WebService
{
	public class GlowNavigationWebServiceResponse : WebServiceResponse
	{
		public GlowNavigationWebServiceResponse()
			: base()
		{
			GlowSSONavigationUrl = string.Empty;
		}

		public string GlowSSONavigationUrl { get; set; }
	}
}
