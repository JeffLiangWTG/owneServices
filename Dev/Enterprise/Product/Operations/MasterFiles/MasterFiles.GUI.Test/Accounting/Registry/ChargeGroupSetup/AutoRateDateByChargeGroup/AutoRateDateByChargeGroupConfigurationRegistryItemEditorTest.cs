using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AutoRateDateByChargeGroupConfigurationRegistryItemEditor))]
	sealed class AutoRateDateByChargeGroupConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AutoRateDateByChargeGroupConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AutoRateDateByChargeGroupConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AutoRateDateByChargeGroupConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AutoRateDateByChargeGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, new AutoRateDateByChargeGroupConfiguration());
		}

		protected override object[] GetValidRegistryValues()
		{
			var copy = new AutoRateDateByChargeGroupConfiguration();
			return new object[] { copy };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
