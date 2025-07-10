using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(SameChargeCodeDifferentProviderRegistryItemEditor))]
	public class SameChargeCodeDifferentProviderRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new SameChargeCodeDifferentProviderRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SameChargeCodeDifferentProviderControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SameChargeCodeDifferentProviderControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SameChargeCodeDifferentProviderRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new SameChargeCodeDifferentProviderCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
