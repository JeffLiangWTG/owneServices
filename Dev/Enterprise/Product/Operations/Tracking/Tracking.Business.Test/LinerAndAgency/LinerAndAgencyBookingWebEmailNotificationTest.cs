using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyBookingWebInterfacesHelper))]
	sealed class LinerAndAgencyBookingWebEmailNotificationTest : LinerAndAgencyBaseWebEmailNotificationTest<LinerAndAgencyBookingWebInterfacesHelper>
	{
		#region Implementation

		protected override ControllerID ExpectedControllerID
		{
			get { return ControllerIDs.AgencyBooking; }
		}

		protected override CodePairRegistryItem ExpectedNotificationSendingRule
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBookingNotificationOptions; }
		}

		protected override CodeDescriptionBoolRegistryItem ExpectedStaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBookingNotificationStaffRoles; }
		}

		protected override GuidRegistryItem ExpectedEmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBookingNotificationEmailGroup; }
		}

		protected override AgencyShipment GetBusinessObjectForHelper()
		{
			return Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
		}

		protected override LinerAndAgencyBookingWebInterfacesHelper GetHelper(AgencyShipment shipment)
		{
			return new LinerAndAgencyBookingWebInterfacesHelper(shipment as TrackingLinerAndAgencyBooking);
		}

		#endregion

	}
}
