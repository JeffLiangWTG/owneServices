using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(CargoIMPPhase2MSUEventsMappingRegistryItemEditor))]
	public class CargoIMPPhase2MSUEventsMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CargoIMPPhase2MSUEventsMappingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CargoIMPPhase2MSUEventsMappingRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CargoIMPPhase2MSUEventsMappingRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CargoIMPPhase2MSUEventsMappingRegistryItem("", null, null, null, RegistryStorageFlags.System, new CargoIMPPhase2MSUEventsMappingCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { CargoIMPPhase2MSUEventsMappingCollection.GetDefault() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
