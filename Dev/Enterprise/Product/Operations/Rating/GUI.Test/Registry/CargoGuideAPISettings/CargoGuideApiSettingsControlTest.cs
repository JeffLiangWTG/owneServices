using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Registry.GUI.Testing
{
	[TestedType(typeof(CargoGuideApiSettingsControl))]
	public class CargoGuideApiSettingsControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CargoGuideApiSettings();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CargoGuideApiSettingsControl)control).ReadOnly;
		}
	}
}
