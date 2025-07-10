using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web
{
	public class StatementRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "StatementRequestHandler.axd"; }
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
