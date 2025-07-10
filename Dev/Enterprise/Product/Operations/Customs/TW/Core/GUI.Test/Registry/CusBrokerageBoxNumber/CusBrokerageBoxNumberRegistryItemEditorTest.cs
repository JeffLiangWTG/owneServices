using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(CusBrokerageBoxNumberRegistryItemEditor))]
	public sealed class CusBrokerageBoxNumberRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new CusBrokerageBoxNumberRegistryItemEditor(RegistryItem.DataType, null, null);
		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((CusBrokerageBoxNumberRegistryItemUserControl)editorPane).ReadOnly;
		protected override Type GetExpectedEditorPaneType() => typeof(CusBrokerageBoxNumberRegistryItemUserControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new CusBrokerageBoxNumberRegistryItem("", null, null, null, RegistryStorageFlags.System);
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
		protected override object[] GetValidRegistryValues()
		{
			var collection = new CusBrokerageBoxNumberCollection();
			var element = collection.AddNew();
			element.BoxNumber = "600";
			element.CustomsOfficeArea = TaiwanCustomsDistrictList.Codes.A;
			element.IsDefaultBoxNumber = ZBool.True;
			Factory.Save();
			return new object[] { collection };
		}
	}
}
