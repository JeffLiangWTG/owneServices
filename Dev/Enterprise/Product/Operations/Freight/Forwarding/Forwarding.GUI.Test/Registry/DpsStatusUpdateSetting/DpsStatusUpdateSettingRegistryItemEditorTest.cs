using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(DpsStatusUpdateSettingRegistryItemEditor))]
	public class DpsStatusUpdateSettingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DpsStatusUpdateSettingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DpsStatusUpdateSettingRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DpsStatusUpdateSettingRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DpsStatusUpdateSettingRegistryItem("", null, null, null, RegistryStorageFlags.System, null);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new DpsStatusUpdateSetting() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		#endregion
	}
}
