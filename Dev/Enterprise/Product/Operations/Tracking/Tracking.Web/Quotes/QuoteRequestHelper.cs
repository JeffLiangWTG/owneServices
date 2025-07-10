using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web
{
	public class QuoteRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "QuoteRequestHandler.axd"; }
		}

		public override bool EnableCache
		{
			get { return true; }
		}

		public override bool UseSecureQueryString
		{
			get { return true; }
		}
	}
}
