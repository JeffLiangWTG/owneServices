using System.Web.Services.Protocols;

namespace Enterprise.Tracking.Web.WebService
{
	/// <summary>
	/// Header for SOAP Messages that includes authentication information
	/// </summary>
	public class WebTrackerSOAPHeader : SoapHeader
	{
		public string CompanyCode;
		public string UserName;
		public string Password;
	}
}
