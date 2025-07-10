using System;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AutomaticDeferredSelectionRegistryItemEditor))]
	sealed class AutomaticDeferredSelectionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AutomaticDeferredSelectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new AutomaticDeferredSelectionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AutomaticDeferredSelectionUserControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new AutomaticDeferredSelection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((AutomaticDeferredSelectionUserControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.TopLeft;
			}
		}
	}
}
