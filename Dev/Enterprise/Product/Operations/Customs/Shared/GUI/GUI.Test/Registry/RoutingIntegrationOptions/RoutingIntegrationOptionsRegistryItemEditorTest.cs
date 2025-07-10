using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(RoutingIntegrationOptionsRegistryItemEditor))]
	sealed class RoutingIntegrationOptionsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var options = new RoutingIntegrationOptions();
			options.AlwaysLink = true;
			return new RoutingIntegrationOptionsRegistryItem("", null, null, null, RegistryStorageFlags.System, options);
		}

		protected override RegistryItemEditor GetEditor() => new RoutingIntegrationOptionsRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override Type GetExpectedEditorPaneType() => typeof(RoutingIntegrationOptionsUserControl);

		protected override object[] GetValidRegistryValues() => new object[] { new RoutingIntegrationOptions() };

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !((RoutingIntegrationOptionsUserControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeft;
	}
}
