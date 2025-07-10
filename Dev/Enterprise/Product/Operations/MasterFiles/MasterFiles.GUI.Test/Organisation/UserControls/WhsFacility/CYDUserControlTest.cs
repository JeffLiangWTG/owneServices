using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing.Organisation.UserControls.WhsFacility
{
	[TestedType(typeof(CYDUserControl))]
	sealed class CYDUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new CYDUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyWarehouse" }; }
		}
	}
}
