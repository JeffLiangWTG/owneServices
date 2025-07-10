using System;
using System.Windows.Forms;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationRegistryItemEditor))]
	public sealed class CusGoodsLocationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new CusGoodsLocationRegistryItemEditor(RegistryItem.DataType, null, null);
		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((CusGoodsLocationRegistryItemUserControl)editorPane).ReadOnly;
		protected override Type GetExpectedEditorPaneType() => typeof(CusGoodsLocationRegistryItemUserControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new CusGoodsLocationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
		protected override object[] GetValidRegistryValues()
		{
			var collection = new CusGoodsLocationCollection();
			var element = collection.AddNew();
			element.MessageType = "IMP";
			element.CustomsOffice = "CC";
			element.GoodsLocation = "ANP0060D";
			element = collection.AddNew();
			element.MessageType = "EXP";
			element.CustomsOffice = "DD";
			element.GoodsLocation = "ANP0061D";
			Factory.Save();
			return new object[] { collection };
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
		}
	}
}
