using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI
{
	[TestedType(typeof(PhaseSecurityRegistryControl))]
	class PhaseSecurityRegistryControl_Test : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new Forwarding.Registry.PhaseSecurity();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((PhaseSecurityRegistryControl)control).IsEnabledCheckBox.ReadOnly
				&& ((PhaseSecurityRegistryControl)control).PhasesGrid.ReadOnly
				&& ((PhaseSecurityRegistryControl)control).RulesGrid.ReadOnly;
		}
	}
}
