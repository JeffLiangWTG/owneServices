using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(LiquidationGroupNotificationRegistryItemEditor))]
	sealed class LiquidationGroupNotificationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new LiquidationGroupNotificationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, LiquidationGroupNotification.Default);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new LiquidationGroupNotificationRegistryItemEditor(RegistryItem.DataType, null, new BusinessObjectFactory());
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(LiquidationGroupNotificationControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { LiquidationGroupNotification.Default };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((LiquidationGroupNotificationControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
