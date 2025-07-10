using System;
using System.Windows.Forms;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(UCMEDIMessageTestTypesRegistryItemEditor))]
	sealed class UCMEDIMessageTestTypesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new UCMEDIMessageTestTypesRegistryItemEditor((UCMEDIMessageTestTypeRegistryItemDataType)RegistryItem.DataType, null, null);
		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((UCMEDIMessageTestTypeUserControl)editorPane).ReadOnly;
		protected override Type GetExpectedEditorPaneType() => typeof(UCMEDIMessageTestTypeUserControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new UCMEDIMessageTestTypeRegistryItem("", null, null, null);
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override object[] GetValidRegistryValues()
		{
			var collection = new UCMEDIMessageTestTypeCollection();
			var item = collection.AddNew();
			item.ApplicationCode = "_T1";
			item.UCKDelayTimeInMilliseconds = 10;
			item.UCQDelayTimeInMilliseconds = 15;
			item.UCQDelayTimeInMilliseconds = 26;
			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var collection1 = (UCMEDIMessageTestTypeCollection)setValue;
			var collection2 = (UCMEDIMessageTestTypeCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].ApplicationCode", i.ToString()), collection1[i].ApplicationCode, collection2[i].ApplicationCode);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].UCKDelayTimeInMilliseconds", i.ToString()), collection1[i].UCKDelayTimeInMilliseconds, collection2[i].UCKDelayTimeInMilliseconds);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].UCQDelayTimeInMilliseconds", i.ToString()), collection1[i].UCQDelayTimeInMilliseconds, collection2[i].UCQDelayTimeInMilliseconds);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].UCUDelayTimeInMilliseconds", i.ToString()), collection1[i].UCUDelayTimeInMilliseconds, collection2[i].UCUDelayTimeInMilliseconds);
			}
		}
	}
}
