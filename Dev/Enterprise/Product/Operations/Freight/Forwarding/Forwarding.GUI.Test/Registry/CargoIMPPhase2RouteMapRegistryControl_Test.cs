using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI
{
	[TestedType(typeof(CargoIMPPhase2RouteMapRegistryControl))]
	class CargoIMPPhase2RouteMapRegistryControl_Test : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CargoIMPPhase2RouteMapCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CargoIMPPhase2RouteMapRegistryControl)control).CargoIMPPhase2RouteMapGrid.ReadOnly;
		}
	}
}
