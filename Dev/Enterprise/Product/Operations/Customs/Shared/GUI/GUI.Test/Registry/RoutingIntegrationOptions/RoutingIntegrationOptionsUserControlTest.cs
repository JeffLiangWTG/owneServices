using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(RoutingIntegrationOptionsUserControl))]
	class RoutingIntegrationOptionsUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new RoutingIntegrationOptions();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var optionUserControl = (RoutingIntegrationOptionsUserControl)control;
			return optionUserControl.AlwaysLinkRadioButton.ReadOnly;
		}
	}
}
