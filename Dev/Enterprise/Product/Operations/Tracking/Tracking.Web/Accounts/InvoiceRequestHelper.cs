using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web
{
	public class InvoiceRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "InvoiceRequestHandler.axd"; }
		}

		public override bool EnableCache => false;

		public override bool UseSecureQueryString
		{
			get { return true; }
		}
	}
}
