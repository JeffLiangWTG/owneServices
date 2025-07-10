using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(OrderManagerRequestMappingRegistryItemEditor))]
	sealed class OrderManagerRequestMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new OrderManagerRequestMappingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OrderManagerRequestMappingControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OrderManagerRequestMappingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var item = new OrderManagerRequestMappingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new OrderManagerRequestMappingCollection());
			var dataType = item.DataType as OrderManagerRequestMappingDataType;
			return item;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new OrderManagerRequestMappingCollection()
			{
				new OrderManagerRequestMapping()
				{
					Request = "CargoDateVsShipmentWindow",
					RequestType = "",
				}
			};

			return [collection];
		}

		#endregion
	}
}
