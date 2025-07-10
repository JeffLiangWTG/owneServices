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
	[TestedType(typeof(IssueTypeMappingRegistryEditor))]
	class IssueTypeMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new IssueTypeMappingRegistryEditor(new IssueTypeMappingRegistryDataType(new IssueTypeMap()), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !editorPane.FindSingle<ZGrid>().ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(IssueTypeMappingControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new IssueTypeMappingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new IssueTypeMap());

		protected override object[] GetValidRegistryValues() => new[] { new IssueTypeMap() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
