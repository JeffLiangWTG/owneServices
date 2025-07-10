using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ConsignorUserControl))]
	sealed class ConsignorUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new ConsignorUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyConsignor", "IsModifyConsignorDetails", "IsModifyConsignorExporterScheme", "IsModifyConsignorRelationships" }; }
		}
	}
}
