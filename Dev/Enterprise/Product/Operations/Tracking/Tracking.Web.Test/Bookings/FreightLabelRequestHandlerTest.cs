using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(FreightLabelRequestHandler))]
	sealed class FreightLabelRequestHandlerTest : BookingDocumentRequestHandlerTest<FreightLabelRequestHelper>
	{
		protected override DocumentRequestHandler<FreightLabelRequestHelper> GetDocumentRequestHandler() => new FreightLabelRequestHandler();

		protected override TrackingDocumentTypes GetExpectedTrackingDocumentType() => TrackingDocumentTypes.FreightLabels;

		protected override ForwardingShipment GetNewBooking()
		{
			var booking = base.GetNewBooking();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			booking.ConsigneePK = consignee.PK;
			booking.ConsignorPK = consignor.PK;
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USORD";
			booking.JS_BookingReference = "Test Booking";

			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			var line = booking.OuterPackLines.AddNew();
			line.FillWithValidTestData();
			line.JL_PackageCount = 1;

			return booking;
		}
	}
}
