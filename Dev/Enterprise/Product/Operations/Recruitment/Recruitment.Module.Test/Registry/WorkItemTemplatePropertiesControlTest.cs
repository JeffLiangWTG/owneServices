using CargoWise.EntityFramework;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Module;

[TestedType(typeof(WorkItemTemplatePropertiesControl))]
sealed class WorkItemTemplatePropertiesControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity()
		=> new WorkItemTemplatePropertiesCollection();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		=> ((WorkItemTemplatePropertiesControl)control).ReadOnly;
}
