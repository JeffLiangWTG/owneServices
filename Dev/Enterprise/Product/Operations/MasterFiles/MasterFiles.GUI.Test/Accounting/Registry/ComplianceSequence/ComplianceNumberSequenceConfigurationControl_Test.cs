using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ComplianceNumberSequenceConfigurationControl))]
	sealed class ComplianceNumberSequenceConfigurationControl_Test : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			var configuration = new ComplianceNumberSequenceConfiguration();
			var configurations = new ComplianceNumberSequenceConfigurationCollection();
			configurations.Add(configuration);
			return configurations;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ComplianceNumberSequenceConfigurationControl)control).ParentGrid.ReadOnly;
		}
	}
}
