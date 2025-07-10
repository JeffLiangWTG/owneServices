using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AutoBillingGroupNotificationRegistryItemEditor))]
	sealed class AutoBillingGroupNotificationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new AutoBillingGroupNotificationRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override RegistryItemEditor GetEditor() => new AutoBillingGroupNotificationRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override Type GetExpectedEditorPaneType() => typeof(AutoBillingGroupNotificationUserControl);

		protected override object[] GetValidRegistryValues() => new object[] { new AutoBillingGroupNotification() };

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !(editorPane as AutoBillingGroupNotificationUserControl).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeft;
	}
}
