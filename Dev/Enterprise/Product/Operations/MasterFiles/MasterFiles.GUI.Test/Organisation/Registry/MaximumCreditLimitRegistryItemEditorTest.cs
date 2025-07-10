using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(MaximumCreditLimitRegistryItemEditor))]
	public class MaximumCreditLimitRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new MaximumCreditLimitRegistryItemEditor(new MaximumCreditLimitDataType(), null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((MaximumCreditLimitControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(MaximumCreditLimitControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new MaximumCreditLimitCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new MaximumCreditLimitCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new MaximumCreditLimitCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
