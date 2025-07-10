using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DistanceCalculationProviderConfigurationRegistryControl))]
	sealed class DistanceCalculationProviderConfigurationRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DistanceCalculationProviderConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			bool allControlsReadonly = true;
			foreach (Control control in control1.Controls)
			{
				if (control.Enabled)
				{
					allControlsReadonly = false;
					break;
				}
			}
			return ((DistanceCalculationProviderConfigurationRegistryControl)control1).ReadOnly && allControlsReadonly;
		}
	}
}
