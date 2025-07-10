using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AccountingIntegrationOptionsRegistryItemRegistryItemEditor))]
	sealed class AccountingIntegrationOptionsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new AccountingIntegrationOptionsRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override RegistryItemEditor GetEditor() => new AccountingIntegrationOptionsRegistryItemRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override Type GetExpectedEditorPaneType() => typeof(AccountingIntegrationOptionsUserControl);

		protected override object[] GetValidRegistryValues() => new object[] { new AccountingIntegrationOptions() };

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !((AccountingIntegrationOptionsUserControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeft;
	}
}
