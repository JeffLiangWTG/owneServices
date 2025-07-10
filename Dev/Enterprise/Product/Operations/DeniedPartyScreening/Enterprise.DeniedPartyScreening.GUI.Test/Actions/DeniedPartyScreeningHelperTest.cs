using System.Globalization;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class DeniedPartyScreeningHelperTest : TestCaseWithFactory
	{
		public void TestSystemDefinedUnmatchedOrganisation_ShouldShowMessage()
		{
			var header = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);
			Assert("Precondition : UNMATCHED is system Defined", header.IsSystemDefinedOrganisation);

			DeniedPartyScreeningHelper.IsSystemDefinedUnmatchedOrganizationWithShowMessage(header);
			AssertEquals("Unmatched Organizations are not included in Denied Party Screening and will not be screened. Please create a new Organization or replace with an existing Organization. Unmatched Organizations will remain Not Screened", UnitTestUserNotification.Instance.LastMessage.Text);

			DeniedPartyScreeningHelper.IsSystemDefinedUnmatchedOrganizationWithShowMessage((IDpsEntityProvider)header);
			AssertEquals("The selected organization is a system defined organization and its screening status cannot be modified.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestInactiveEntityWithShowMessage()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsActive = false;

			DeniedPartyScreeningHelper.IsInactiveEntityWithShowMessage(header, header.OH_IsActive);
			AssertEquals(string.Format(CultureInfo.InvariantCulture, "The selected {0} is inactive and its screening status cannot be modified.", header.HumanReadableName), UnitTestUserNotification.Instance.LastMessage.Text);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_IsActive = false;

			DeniedPartyScreeningHelper.IsInactiveEntityWithShowMessage(vessel, vessel.RV_IsActive);
			AssertEquals(string.Format(CultureInfo.InvariantCulture, "The selected {0} is inactive and its screening status cannot be modified.", vessel.HumanReadableName), UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
