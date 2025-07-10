using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(ReferringPartyConfigurationControl))]
	public class OrgSecurityProfileControlTest : RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl() => new ReferringPartyConfigurationControl();
		protected override IBusiness GetNewBusinessEntity() => new ReferringPartyConfigurationCollection();
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly || businessEntity.IsReadOnly;
		}
	}
}
