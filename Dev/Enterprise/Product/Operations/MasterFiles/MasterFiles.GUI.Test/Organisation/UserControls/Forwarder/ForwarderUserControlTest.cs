using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ForwarderUserControl))]
	sealed class ForwarderUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new ForwarderUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyForwarder", "IsModifyForwarderDetails", "IsModifyForwarderProfitShare" }; }
		}
	}
}
