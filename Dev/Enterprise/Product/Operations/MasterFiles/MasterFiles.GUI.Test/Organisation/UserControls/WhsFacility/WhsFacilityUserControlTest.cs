using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing.Organisation.UserControls.WhsFacility
{
	[TestedType(typeof(WhsFacilityUserControl))]
	sealed class WhsFacilityUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new WhsFacilityUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyWarehouse" }; }
		}
	}
}
