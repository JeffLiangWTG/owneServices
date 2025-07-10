using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(InvoiceRemittanceConfigurationControl))]
	sealed class InvoiceRemittanceConfigurationControl_Test : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			var configuration = new InvoiceRemittanceConfiguration();
			var configurations = new InvoiceRemittanceConfigurationCollection();
			configurations.Add(configuration);
			return configurations;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((InvoiceRemittanceConfigurationControl)control).ParentGrid.ReadOnly;
		}
	}
}
