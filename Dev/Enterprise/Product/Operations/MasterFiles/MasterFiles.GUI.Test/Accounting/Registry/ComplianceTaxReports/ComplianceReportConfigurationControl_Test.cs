using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ComplianceReportConfigurationControl))]
	sealed class ComplianceReportConfigurationControl_Test : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			var configuration = new ComplianceReportConfiguration();
			var configurations = new ComplianceReportConfigurationCollection();
			configurations.Add(configuration);
			return configurations;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ComplianceReportConfigurationControl)control).ParentGrid.ReadOnly;
		}
	}
}
