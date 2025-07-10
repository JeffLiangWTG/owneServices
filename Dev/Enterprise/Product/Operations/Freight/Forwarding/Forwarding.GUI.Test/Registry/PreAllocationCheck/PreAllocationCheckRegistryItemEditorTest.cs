using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Registry.Testing
{
	[TestedType(typeof(PreAllocationCheckRegistryItemEditor))]
	public class PreAllocationCheckRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new PreAllocationCheckRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PreAllocationCheckRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PreAllocationCheckRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PreAllocationCheckRegistryItem("", null, null, null, RegistryStorageFlags.System, new PreAllocationCheckCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new PreAllocationCheckCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
