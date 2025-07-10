using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI
{
	[TestedType(typeof(CargoIMPPhase2MSUEventsMappingRegistryControl))]
	class CargoIMPPhase2MSUEventsMappingRegistryControl_Test : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CargoIMPPhase2MSUEventsMappingCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CargoIMPPhase2MSUEventsMappingRegistryControl)control).CargoIMPPhase2MSUEventsMappingGrid.ReadOnly;
		}
	}
}
