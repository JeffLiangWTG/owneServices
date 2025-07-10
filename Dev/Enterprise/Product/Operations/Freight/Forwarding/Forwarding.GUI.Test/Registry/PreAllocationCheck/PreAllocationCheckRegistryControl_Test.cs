using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Registry
{
	[TestedType(typeof(PreAllocationCheckRegistryControl))]
	class PreAllocationCheckRegistryControl_Test : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new Forwarding.Registry.PreAllocationCheckCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((PreAllocationCheckRegistryControl)control).ChecksGrid.ReadOnly;
		}
	}
}
