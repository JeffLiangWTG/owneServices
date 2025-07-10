using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CarrierUserControl))]
	sealed class CarrierUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new CarrierUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyCarrier" }; }
		}
	}
}
