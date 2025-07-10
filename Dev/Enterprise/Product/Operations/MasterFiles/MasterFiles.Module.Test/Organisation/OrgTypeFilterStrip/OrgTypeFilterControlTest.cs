using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgTypeFilterControlTest : TestCaseWithFactory
	{
		public void TestControllingAgentAndCustomerCheckboxesAreHiddenIfRelatedRegistrySettingsAreOff()
		{
			AssertControllingAgentAndCustomerCheckboxesAreHiddenIfRelatedRegistrySettingsAreOff(false, false);
			AssertControllingAgentAndCustomerCheckboxesAreHiddenIfRelatedRegistrySettingsAreOff(false, true);
			AssertControllingAgentAndCustomerCheckboxesAreHiddenIfRelatedRegistrySettingsAreOff(true, false);
			AssertControllingAgentAndCustomerCheckboxesAreHiddenIfRelatedRegistrySettingsAreOff(true, true);
		}

		void AssertControllingAgentAndCustomerCheckboxesAreHiddenIfRelatedRegistrySettingsAreOff(bool isControllingAgentFunctionalityEnabled, bool isControllingCustomerFunctionalityEnabled)
		{
			using (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isControllingAgentFunctionalityEnabled))
			using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isControllingCustomerFunctionalityEnabled))
			using (var control = new OrgTypeFilterControl())
			{
				AssertEquals(isControllingAgentFunctionalityEnabled, control.ControllingAgentCheckBox.Visible);
				AssertEquals(isControllingCustomerFunctionalityEnabled, control.ControllingCustomerCheckBox.Visible);
			}
		}
	}
}
