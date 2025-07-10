using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	abstract class ChargeGroupSettingControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ChargeGroupSettingControl)control).ChargeGroupSetupGrid.ReadOnly;
		}
	}
}
