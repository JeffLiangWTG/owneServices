namespace Enterprise.Tracking.Web
{
	public class FreightLabelRequestHelper : DocumentRequestHelper
	{
		public override string BaseUrl => "FreightLabelRequestHandler.axd";

		public override bool EnableCache => false;

		public override bool UseSecureQueryString => true;
	}
}
