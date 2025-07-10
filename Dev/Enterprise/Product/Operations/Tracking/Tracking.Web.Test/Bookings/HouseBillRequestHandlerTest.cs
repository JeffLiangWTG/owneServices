using Enterprise.Tracking.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(HouseBillRequestHandler))]
	sealed class HouseBillRequestHandlerTest : BookingDocumentRequestHandlerTest<HouseBillRequestHelper>
	{
		protected override DocumentRequestHandler<HouseBillRequestHelper> GetDocumentRequestHandler() => new HouseBillRequestHandler();

		protected override TrackingDocumentTypes GetExpectedTrackingDocumentType() => TrackingDocumentTypes.HouseBills;
	}
}
