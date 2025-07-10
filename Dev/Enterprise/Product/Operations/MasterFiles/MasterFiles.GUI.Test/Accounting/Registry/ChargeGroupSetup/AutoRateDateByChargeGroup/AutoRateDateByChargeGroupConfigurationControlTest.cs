using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AutoRateDateByChargeGroupConfigurationControl))]
	sealed class AutoRateDateByChargeGroupConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AutoRateDateByChargeGroupConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AutoRateDateByChargeGroupConfigurationControl)control).filterTypeDropEdit.ReadOnly;
		}
	}
}
