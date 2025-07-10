using System;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(DefaultStatementPrintDateRegistryItemEditor))]
	sealed class DefaultStatementPrintDateRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0].Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DefaultStatementPrintDateRegistryItem("", null, null, null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DefaultStatementPrintDateRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DefaultStatementPrintDateControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			DefaultStatementPrintDate statementPrintDateData = new DefaultStatementPrintDate();

			statementPrintDateData.NumberOfDays = 0;
			statementPrintDateData.DoDefaultPrelimStatementPrintDate = false;
			return new object[] { statementPrintDateData };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((DefaultStatementPrintDateControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
