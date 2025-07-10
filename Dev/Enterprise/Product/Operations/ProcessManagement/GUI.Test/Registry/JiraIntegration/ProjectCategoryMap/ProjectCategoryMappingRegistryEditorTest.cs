using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(ProjectCategoryMappingRegistryEditor))]
	class ProjectCategoryMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ProjectCategoryMappingRegistryEditor(new ProjectCategoryMappingRegistryDataType(new ProjectCategoryMap()), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !editorPane.FindSingle<ZGrid>().ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(ProjectCategoryMappingControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new ProjectCategoryMappingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ProjectCategoryMap());

		protected override object[] GetValidRegistryValues() => new[] { new ProjectCategoryMap() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
