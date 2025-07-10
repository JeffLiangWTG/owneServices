using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RevenueRecognitionByChargeGroupRegistryItemEditor))]
	sealed class RevenueRecognitionByChargeGroupRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new RevenueRecognitionByChargeGroupRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((RevenueRecognitionByChargeGroupControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RevenueRecognitionByChargeGroupControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RevenueRecognitionByChargeGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, new RevenueRecognitionByChargeGroupCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new RevenueRecognitionByChargeGroupCollection();
			var copy = collection.AddNew();

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
