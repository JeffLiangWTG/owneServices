using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(CargoIMPPhase2RouteMapRegistryItemEditor))]
	public class CargoIMPPhase2RouteMapRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CargoIMPPhase2RouteMapRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CargoIMPPhase2RouteMapRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CargoIMPPhase2RouteMapRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CargoIMPPhase2RouteMapRegistryItem("", null, null, null, RegistryStorageFlags.System, new CargoIMPPhase2RouteMapCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CargoIMPPhase2RouteMapCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
