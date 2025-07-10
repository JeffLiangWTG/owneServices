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
	[TestedType(typeof(RecipientSourceFallbackRegistryEditor))]
	class RecipientSourceFallbackRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new RecipientSourceFallbackRegistryEditor(new RecipientSourceFallbackRegistryDataType(), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !editorPane.FindSingle<ZGrid>().ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(RecipientSourceFallbackControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new RecipientSourceFallbackRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override object[] GetValidRegistryValues() => new[] { new RecipientSourceFallbackHeader() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
