using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EUTaxIDDefaultingRegistryItemEditor))]
	sealed class EUTaxIDDefaultingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new EUTaxIDDefaultingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EUTaxIDDefaultingControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EUTaxIDDefaultingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EUTaxIDDefaultingRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			EUTaxIDDefaultingRuleCollection copy = new EUTaxIDDefaultingRuleCollection();

			return new object[] { copy };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
