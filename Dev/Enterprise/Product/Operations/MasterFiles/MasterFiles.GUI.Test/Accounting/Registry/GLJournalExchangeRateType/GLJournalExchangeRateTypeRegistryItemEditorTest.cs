using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GLJournalExchangeRateTypeRegistryItemEditor))]
	public class GLJournalExchangeRateTypeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new GLJournalExchangeRateTypeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((GLJournalExchangeRateTypeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GLJournalExchangeRateTypeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GLJournalExchangeRateTypeRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = "PER", ProfitAndLossAccountTypeExchangeRateType = "PER" });
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = "PER", ProfitAndLossAccountTypeExchangeRateType = "PER" } };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		#endregion
	}
}
