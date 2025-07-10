using System;
using System.Windows.Forms;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.GUI.Testing
{
	[TestedType(typeof(CPCAcquitByDateRegistryItemEditor))]
	sealed class CPCAcquitByDateRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new CPCAcquitByDateRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((CPCAcquitByDateUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(CPCAcquitByDateUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new CPCAcquitByDateRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CPCAcquitByDate();
			Factory.Save();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
