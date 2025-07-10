using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	[TestedType(typeof(AWBDisplayOptionRegistryControl))]
	class AWBDisplayOptionRegistryControl_Test : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AWBDisplayOptionCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AWBDisplayOptionRegistryControl)control).AWBDisplayOptionsGrid.ReadOnly &&
					((AWBDisplayOptionRegistryControl)control).HideAllCarrierButton.ReadOnly &&
					((AWBDisplayOptionRegistryControl)control).HideAllButton.ReadOnly &&
					((AWBDisplayOptionRegistryControl)control).HideAllAgentButton.ReadOnly &&
					((AWBDisplayOptionRegistryControl)control).ShowAllCarrierButton.ReadOnly &&
					((AWBDisplayOptionRegistryControl)control).ShowAllButton.ReadOnly &&
					((AWBDisplayOptionRegistryControl)control).ShowAllAgentButton.ReadOnly;
		}
	}
}
