using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SupplyTypeConfigurationByChargeGroupRegistryItemEditor))]
	sealed class SupplyTypeConfigurationByChargeGroupRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new SupplyTypeConfigurationByChargeGroupRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SupplyTypeConfigurationByChargeGroupControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SupplyTypeConfigurationByChargeGroupControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SupplyTypeConfigurationByChargeGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value ? RegistryOptions.Default : RegistryOptions.IsHidden, new SupplyTypeConfigurationByChargeGroupCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new SupplyTypeConfigurationByChargeGroupCollection();
			var copy = collection.AddNew();

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
