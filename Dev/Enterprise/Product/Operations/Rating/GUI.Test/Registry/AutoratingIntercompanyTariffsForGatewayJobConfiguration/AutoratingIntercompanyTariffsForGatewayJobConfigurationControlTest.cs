using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Registry.Testing
{
	[TestedType(typeof(AutoratingIntercompanyTariffsForGatewayJobConfigurationControl))]
	public class AutoratingIntercompanyTariffsForGatewayJobConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AutoratingIntercompanyTariffsForGatewayJobConfigurationControl)control).AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.ReadOnly;
		}
	}
}
