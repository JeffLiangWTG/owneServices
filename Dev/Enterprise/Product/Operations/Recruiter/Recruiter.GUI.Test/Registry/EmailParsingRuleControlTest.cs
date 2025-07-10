using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(EmailParsingRuleControl))]
	public class EmailParsingRuleControlTest : RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl() => new EmailParsingRuleControl();
		protected override IBusiness GetNewBusinessEntity() => new EmailParsingRuleCollection();
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly || businessEntity.IsReadOnly;
		}
	}
}
