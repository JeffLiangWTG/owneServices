using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ReceivablesUserControl))]
	sealed class ReceivablesUserControlSecurityTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new ReceivablesUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyReceivables", "IsModifyReceivablesConfig", "IsModifyReceivablesInvoicing" }; }
		}
	}
}
