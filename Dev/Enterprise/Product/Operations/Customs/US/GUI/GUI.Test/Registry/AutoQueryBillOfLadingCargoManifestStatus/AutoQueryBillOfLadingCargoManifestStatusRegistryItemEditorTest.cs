using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AutoQueryBillOfLadingCargoManifestStatusRegistryItemEditor))]
	sealed class AutoQueryBillOfLadingCargoManifestStatusRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new AutoQueryBillOfLadingCargoManifestStatusItem("", null, null, null, RegistryStorageFlags.System);

		protected override RegistryItemEditor GetEditor() => new AutoQueryBillOfLadingCargoManifestStatusRegistryItemEditor(RegistryItem.DataType, null, new BusinessObjectFactory());

		protected override Type GetExpectedEditorPaneType() => typeof(AutoQueryBillOfLadingCargoManifestStatusUserControl);

		protected override object[] GetValidRegistryValues()
		{
			var data = new AutoQueryBillOfLadingCargoManifestStatus();
			data.FillWithValidTestData();
			return new object[] { data };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !((AutoQueryBillOfLadingCargoManifestStatusUserControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeft;
	}
}
