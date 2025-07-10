using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web
{
	public class HouseBillRequestHandler : BookingDocumentRequestHandler<HouseBillRequestHelper>
	{
		protected override TrackingDocumentTypes GetTrackingDocumentType() => TrackingDocumentTypes.HouseBills;
	}
}
