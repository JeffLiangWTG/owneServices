using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(MaximumAllowedTransactionAmountRegistryItemEditor))]
	public class MaximumAllowedTransactionAmountRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new MaximumAllowedTransactionAmountRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((MaximumAllowedTransactionAmountControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(MaximumAllowedTransactionAmountControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new MaximumAllowedTransactionAmountRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new MaximumAllowedTransactionAmount() { MaximumAllowedHeaderAmount = 1M, MaximumAllowedLineAmount = 2M });
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new MaximumAllowedTransactionAmount() { MaximumAllowedHeaderAmount = 1M, MaximumAllowedLineAmount = 2M } };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
