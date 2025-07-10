using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(ACECargoReleaseTypePortMappingItemEditor))]
	sealed class ACECargoReleaseTypePortMappingItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new ACECargoReleaseTypePortRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override RegistryItemEditor GetEditor() => new ACECargoReleaseTypePortMappingItemEditor(RegistryItem.DataType, null, new BusinessObjectFactory());

		protected override Type GetExpectedEditorPaneType() => typeof(ACECargoReleaseTypePortMappingControl);

		protected override object[] GetValidRegistryValues()
		{
			var data = new ACECargoReleaseTypePortMapping();
			data.FillWithValidTestData();
			return new object[] { data };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !((ACECargoReleaseTypePortMappingControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
