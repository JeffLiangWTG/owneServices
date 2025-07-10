namespace Enterprise.Tracking.Web
{
	public class HouseBillRequestHelper : DocumentRequestHelper
	{
		public override string BaseUrl => "HouseBillRequestHandler.axd";

		public override bool EnableCache => false;

		public override bool UseSecureQueryString => true;
	}
}
