using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	[TestedType(typeof(PalletProviderRegistryItemEditor))]
	sealed class PalletProviderRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new PalletProviderRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PalletProviderRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PalletProviderRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PalletProviderRegistryItem("", null, null, null, new PalletTypeParent());
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override object[] GetValidRegistryValues()
		{
			var result = new PalletTypeParent();

			var type = result.Types.AddNew();
			type.Code = "ABC";
			type.Description = (NoResString)"Aleera";
			type.ProviderCode = "ZAY";
			type.EquipmentCode = "RY";

			var type2 = result.Types.AddNew();
			type2.Code = "XYZ";
			type2.Description = (NoResString)"Lola";
			type2.ProviderCode = "APP";
			type2.EquipmentCode = "RAK";

			return new object[] { result };
		}

		#endregion
	}
}
