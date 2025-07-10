using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HouseBillsNumberValidationRegistryItemEditor))]
	public class HouseBillsNumberValidationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new HouseBillsNumberValidationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((HouseBillsNumberValidationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(HouseBillsNumberValidationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new HouseBillsNumberValidationRegistryItem("", null, null, null, RegistryStorageFlags.System, new HouseBillsNumberValidationCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new HouseBillsNumberValidationCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
