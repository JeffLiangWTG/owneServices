using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TriggerConditionValueColumnStyleTest : TestCaseWithFactory
	{
		public void TestContextMenu_PreviewAndInsertMacroItemsAreNotVisible_NoCondition()
		{
			var bizo = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			CheckContextMenu_PreviewAndInsertMacroItemsAreVisible(bizo, string.Empty, false, false);
		}

		public void TestContextMenu_PreviewAndInsertMacroItemsAreNotVisible_MCR()
		{
			var bizo = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			CheckContextMenu_PreviewAndInsertMacroItemsAreVisible(bizo, EventReferenceConditionList.Codes.ConditionWithMacros, false, false);
		}

		[RequiresSTA]
		public void TestContextMenu_PreviewAndInsertMacroItemsAreVisible_UDF()
		{
			var bizo = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			CheckContextMenu_PreviewAndInsertMacroItemsAreVisible(bizo, EventReferenceConditionList.Codes.UserDefined, true, false);
		}

		public void TestContextMenu_PreviewAndInsertMacroItemsAreVisible_UDF_RealJob()
		{
			var bizo = Factory.NewWithValidTestData<DummyWithWorkflow>();
			CheckContextMenu_PreviewAndInsertMacroItemsAreVisible(bizo, EventReferenceConditionList.Codes.UserDefined, true, true);
		}

		void CheckContextMenu_PreviewAndInsertMacroItemsAreVisible(IWorkflowProvider bizo, string condition, bool shouldSupportMacroInsert, bool shouldSupportMacroPreview)
		{
			var templateTrigger = bizo.WorkflowItems.AddNew();
			templateTrigger.TriggerConditions.TriggerCondition = condition;

			using (var form = new ZForm(bizo))
			{
				var grid = new ZGrid();
				grid.BindTo = nameof(bizo.WorkflowItems);

				var columnStyleInfo = new TriggerConditionValueColumnStyleInfo();
				columnStyleInfo.ColumnName = "TriggerConditions+TriggerConditionValue";
				columnStyleInfo.FieldTypeColumnName = "TriggerConditions+TriggerConditionValueFieldType";

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;

				form.Controls.Add(grid);
				form.Show();

				var columnStyle = (TriggerConditionValueColumnStyle)grid.Columns[0].ColumnStyle;

				ZTextBox textBox;
				if (shouldSupportMacroInsert || condition == EventReferenceConditionList.Codes.ConditionWithMacros)
				{
					textBox = ((ZMacrosFindBox)columnStyle.EditControl.CurrentEditor).CodeBox;
				}
				else
				{
					textBox = (ZTextBox)columnStyle.EditControl.CurrentEditor;
				}

				textBox.InitializeContextMenu_ForTest();
				textBox.ContextMenuStrip.Show();

				var insertMacroMenuItem = GetContextMenuItemFromText(textBox, "Insert Macro");
				var previewMenuItem = GetContextMenuItemFromText(textBox, "Preview");

				AssertEquals(shouldSupportMacroInsert, insertMacroMenuItem.Visible);
				AssertEquals(shouldSupportMacroPreview, previewMenuItem.Visible);
			}
		}

		ToolStripItem GetContextMenuItemFromText(ZTextBox box, string text)
		{
			return box.ContextMenuStrip.Items
				.Cast<ToolStripItem>().Where(t => t is ZToolStripMenuItem)
				.First(t => (t as ZToolStripMenuItem).Text == text);
		}
	}
}
