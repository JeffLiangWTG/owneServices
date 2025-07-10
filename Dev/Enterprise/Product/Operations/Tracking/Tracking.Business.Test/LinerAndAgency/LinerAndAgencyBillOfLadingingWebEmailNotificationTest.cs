using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyBillOfLadingWebInterfacesHelper))]
	sealed class LinerAndAgencyBillOfLadingingWebEmailNotificationTest : LinerAndAgencyBaseWebEmailNotificationTest<LinerAndAgencyBillOfLadingWebInterfacesHelper>
	{
		#region Implementation

		protected override ControllerID ExpectedControllerID
		{
			get { return ControllerIDs.AgencyBillOfLading; }
		}

		protected override CodePairRegistryItem ExpectedNotificationSendingRule
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBillOfLadingNotificationOptions; }
		}

		protected override CodeDescriptionBoolRegistryItem ExpectedStaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBillOfLadingNotificationStaffRoles; }
		}

		protected override GuidRegistryItem ExpectedEmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBillOfLadingNotificationEmailGroup; }
		}

		protected override AgencyShipment GetBusinessObjectForHelper()
		{
			return Factory.NewWithValidTestData<BillOfLading>();
		}

		protected override LinerAndAgencyBillOfLadingWebInterfacesHelper GetHelper(AgencyShipment shipment)
		{
			return new LinerAndAgencyBillOfLadingWebInterfacesHelper(shipment as BillOfLading);
		}

		#endregion

	}
}
