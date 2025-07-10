using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(RatesServiceSettingsControl))]
	class RatesServiceSettingsControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new RatesServiceRegistrySettingsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((RatesServiceSettingsControl)control).IsControlOrBusinessEntityReadOnly;
		}
	}
}
