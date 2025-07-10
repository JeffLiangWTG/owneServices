using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web
{
	public class FreightLabelRequestHandler : BookingDocumentRequestHandler<FreightLabelRequestHelper>
	{
		protected override TrackingDocumentTypes GetTrackingDocumentType() => TrackingDocumentTypes.FreightLabels;
	}
}
