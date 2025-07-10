using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(SupervisorOverridesControl))]
	sealed class SupervisorOverridesControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SupervisorOverrideData();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			SupervisorOverridesControl supervisorOverridesControl = (SupervisorOverridesControl)control;
			return supervisorOverridesControl.NominErrorsGrid.ReadOnly;
		}
	}
}
