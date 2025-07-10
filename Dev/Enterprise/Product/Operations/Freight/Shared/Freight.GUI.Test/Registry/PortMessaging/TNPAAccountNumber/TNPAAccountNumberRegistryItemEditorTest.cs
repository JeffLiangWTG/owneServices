using System;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(TNPAAccountNumberRegistryItemEditor))]
	sealed class TNPAAccountNumberRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new TNPAAccountNumberRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TNPAAccountNumberControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TNPAAccountNumberControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TNPAAccountNumberRegistryItem("", null, null, null, RegistryStorageFlags.System, new TNPAAccountNumberCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new TNPAAccountNumberCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
