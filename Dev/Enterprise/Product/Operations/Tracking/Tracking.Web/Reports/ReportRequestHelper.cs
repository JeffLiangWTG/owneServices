using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web
{
	public class ReportRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "ReportRequestHandler.axd"; }
		}

		public override bool EnableCache
		{
			get { return false; }
		}

		public override bool UseSecureQueryString
		{
			get { return false; }
		}
	}
}
