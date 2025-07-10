using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(InvoiceRollupOrGroupRegistryItemEditor))]
	sealed class InvoiceRollupOrGroupRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new InvoiceRollupOrGroupRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((InvoiceRollupOrGroupControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(InvoiceRollupOrGroupControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new InvoiceRollupOrGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new InvoiceRollupOrGroupCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new InvoiceRollupOrGroupCollection() };
		}
	}
}
