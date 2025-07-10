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
	[TestedType(typeof(JiraCustomFieldMappingRegistryEditor))]
	class JiraCustomFieldMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new JiraCustomFieldMappingRegistryEditor(new JiraCustomFieldMappingRegistryDataType(new JiraCustomFieldMap()), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !editorPane.FindSingle<ZGrid>().ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(JiraCustomFieldMappingControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new JiraCustomFieldMappingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new JiraCustomFieldMap());

		protected override object[] GetValidRegistryValues() => new[] { new JiraCustomFieldMap() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
