using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AutomatedTariffDescriptionPopulationControl))]
	sealed class AutomatedTariffDescriptionPopulationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AutomatedTariffDescriptionPopulation();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AutomatedTariffDescriptionPopulationControl)control).ReadOnly;
		}
	}
}
