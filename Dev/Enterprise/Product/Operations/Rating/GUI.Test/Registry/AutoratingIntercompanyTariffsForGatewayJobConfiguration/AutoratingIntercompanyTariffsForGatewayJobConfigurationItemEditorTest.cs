using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.Registry.Testing
{
	[TestedType(typeof(AutoratingIntercompanyTariffsForGatewayJobConfigurationItemEditor))]
	public class AutoratingIntercompanyTariffsForGatewayJobConfigurationItemEditorTest : RegistryItemEditorTestCase
	{
		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AutoratingIntercompanyTariffsForGatewayJobConfigurationItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AutoratingIntercompanyTariffsForGatewayJobConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AutoratingIntercompanyTariffsForGatewayJobConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AutoratingIntercompanyTariffsForGatewayJobConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection();
			var configuration = collection.AddNew();
			configuration.LoginAgentRole = AgentStatusList.Codes.GatewayAgentWithTariff;
			configuration.ShipmentDirection = FreightShipmentDirection.Code.Export;
			configuration.AutoratingJob = JobInvoicingConsumerTypes.ShipmentCode;
			configuration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			configuration.ICTServiceProvider = GatewayICTServiceProvider.Code.CurrentGatewayOrganization;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
