using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Module;

[TestedType(typeof(WorkItemTemplatePropertiesRegistryItemEditor))]
sealed class WorkItemTemplatePropertiesRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		=> new WorkItemTemplatePropertiesRegistryItem("", null, null, null, RegistryStorageFlags.System);

	protected override RegistryItemEditor GetEditor()
		=> new WorkItemTemplatePropertiesRegistryItemEditor(RegistryItem.DataType, null, null);

	protected override Type GetExpectedEditorPaneType()
		=> typeof(WorkItemTemplatePropertiesControl);

	protected override object[] GetValidRegistryValues()
		=> new object[] { new WorkItemTemplatePropertiesCollection() };

	protected override bool GetEditorPaneEnabledState(Control editorPane)
		=> !((WorkItemTemplatePropertiesControl)editorPane).ReadOnly;

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		=> RegistryItemEditor.EditorPaneAnchor.All;
}
