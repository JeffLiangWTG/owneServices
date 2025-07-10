using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Moq;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(CodeDescriptionBoolTreeRegistryEditor))]
	public class CodeDescriptionBoolTreeRegistryEditorTest : RegistryItemEditorTestCase
	{
		public override void TestEditorPaneLayout()
		{
			RegistryItemEditor editor = GetEditor();
			using (Control control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 666, 333);
				AssertEquals("should have the correct width", 666, control.Width);
				AssertEquals("should have the correct height", 333, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, control.Anchor);
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CodeDescriptionBoolTreeRegistryEditor(RegistryItem.DataType, RegistryItem.EditorInfo, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionBoolTreeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionBoolTreeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var editorInfo = new CodeDescriptionBoolTreeRegistryEditorInfo(
				new MultilingualString[] { (NoResString)"1", (NoResString)"1", (NoResString)"1", (NoResString)"1", (NoResString)"1" }
				, (NoResString)"B", null, true, false);
			return new CodeDescriptionBoolTreeRegistryItem("", null, null, null, RegistryStorageFlags.System, editorInfo, new CodeDescriptionBoolTreeNodeCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var result = new CodeDescriptionBoolTreeNodeCollection();
			return new object[] { result };
		}

		public void TestCodeDescriptionBoolTreeControlKeepStates()
		{
			using (var testForm = new ZForm() { Width = 450, Height = 700 })
			{
				RegistryItemEditor editor = GetEditor();
				using (Control control = editor.NewWinFormsEditorPane())
				{
					editor.SetEditorPaneLayout(control, 450, 700);
					AssertEquals("should have the correct width", 450, control.Width);
					AssertEquals("should have the correct height", 700, control.Height);

					if (control is not CodeDescriptionBoolTreeControl)
					{
						return;
					}
					var treeControl = (CodeDescriptionBoolTreeControl)control;
					var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree3();
					treeControl.SetDataBinding(tree, null);

					testForm.Controls.Add(treeControl);
					testForm.Show();

					AssertEquals("Initial: total nodes", 12, tree.Count);

					SelectRowInGrid(treeControl, 1, 1);
					AssertEquals("Initial: Grid 1: row at index 1 is selected", 1, GetCurrentRowInGrid(treeControl, 1));
					SelectRowInGrid(treeControl, 2, 2);
					AssertEquals("Initial: Grid 2: row at index 2 is selected", 2, GetCurrentRowInGrid(treeControl, 2));
					SelectRowInGrid(treeControl, 3, 1);
					AssertEquals("Initial: Grid 3: row at index 1 is selected", 1, GetCurrentRowInGrid(treeControl, 3));
					SelectRowInGrid(treeControl, 4, 0);
					AssertEquals("Initial: Grid 4: row at index 0 is selected", 0, GetCurrentRowInGrid(treeControl, 4));

					// Delete a row in Grid4
					DeleteCurrentRowInGrid(treeControl, 4);
					AssertEquals("After adding: total nodes", 11, tree.Count);
					AssertEquals("After deleting: Grid 1: row at index 1 is selected", 1, GetCurrentRowInGrid(treeControl, 1));
					AssertEquals("After deleting: Grid 2: row at index 2 is selected", 2, GetCurrentRowInGrid(treeControl, 2));
					AssertEquals("After deleting: Grid 3: row at index 1 is selected", 1, GetCurrentRowInGrid(treeControl, 3));
					AssertEquals("After deleting: Grid 4: row count equals 0", 0, GetRowCountInGrid(treeControl, 4));

					// Add more rows in Grid4
					CodeDescriptionBoolTreeTestHelper.Add(tree, "1A", "2B", "3A", "4B");
					CodeDescriptionBoolTreeTestHelper.Add(tree, "1A", "2B", "3A", "4C");
					CodeDescriptionBoolTreeTestHelper.Add(tree, "1A", "2B", "3A", "4D");

					AssertEquals("After adding: total nodes", 14, tree.Count);
					AssertEquals("After adding: Grid 1: row at index 1 is selected", 1, GetCurrentRowInGrid(treeControl, 1));
					AssertEquals("After adding: Grid 2: row at index 2 is selected", 2, GetCurrentRowInGrid(treeControl, 2));
					AssertEquals("After adding: Grid 3: row at index 1 is selected", 1, GetCurrentRowInGrid(treeControl, 3));
					AssertEquals("After adding: Grid 4: row count equals 3", 3, GetRowCountInGrid(treeControl, 4));

					// Import 1 record in Grid4
					ImportDataInGrid(treeControl, 4, "4E");
					AssertEquals("After importing: total nodes", 15, tree.Count);
					AssertEquals("After importing: Grid 1: row at index 1 is selected", 1, GetCurrentRowInGrid(treeControl, 1));
					AssertEquals("After importing: Grid 2: row at index 2 is selected", 2, GetCurrentRowInGrid(treeControl, 2));
					AssertEquals("After importing: Grid 3: row at index 1 is selected", 1, GetCurrentRowInGrid(treeControl, 3));
					AssertEquals("After importing: Grid 4: row count equals 4", 4, GetRowCountInGrid(treeControl, 4));
				}
			}
		}

		ZGrid GetZGridFromTreeCode(CodeDescriptionBoolTreeControl treeControl, int index)
		{
			var codeDescriptionBoolTreeGridControl = (CodeDescriptionBoolTreeGridControl)typeof(CodeDescriptionBoolTreeControl).GetField($"Grid{index}", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(treeControl);
			var grid = (ZGrid)typeof(CodeDescriptionBoolTreeGridControl).GetField("CodeDescriptionBoolGrid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(codeDescriptionBoolTreeGridControl);
			return grid;
		}

		void SelectRowInGrid(CodeDescriptionBoolTreeControl treeControl, int gridIndex, int rowIndex)
		{
			var grid = GetZGridFromTreeCode(treeControl, gridIndex);
			var rowRectangle = (Rectangle)typeof(DataGrid).InvokeMember("GetRowRect", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { rowIndex });
			Application.DoEvents();
			typeof(ZGrid).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rowRectangle.X, rowRectangle.Y, 0) });
			Application.DoEvents();
		}

		int GetCurrentRowInGrid(CodeDescriptionBoolTreeControl treeControl, int gridIndex)
		{
			var grid = GetZGridFromTreeCode(treeControl, gridIndex);
			return grid.CurrentRowIndex;
		}

		int GetRowCountInGrid(CodeDescriptionBoolTreeControl treeControl, int gridIndex)
		{
			var grid = GetZGridFromTreeCode(treeControl, gridIndex);
			return grid.ListManager.Count;
		}

		void DeleteCurrentRowInGrid(CodeDescriptionBoolTreeControl treeControl, int gridIndex)
		{
			var grid = GetZGridFromTreeCode(treeControl, gridIndex);
			typeof(ZGrid).InvokeMember("DeleteMenuItem_Click", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { null, new EventArgs() });
			Application.DoEvents();
		}

		void ImportDataInGrid(CodeDescriptionBoolTreeControl treeControl, int gridIndex, string code)
		{
			var grid4 = (CodeDescriptionBoolTreeGridControl)typeof(CodeDescriptionBoolTreeControl).GetField($"Grid{gridIndex}", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(treeControl);
			var collection = grid4.BindingSource.DataSource as CodeDescriptionBoolTreeView;
			IImportCollectionInfo collectionInfo = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<CodeDescriptionBoolTreeNode>("Code"),
				new ImportPropertyInfoImpl<CodeDescriptionBoolTreeNode>("Description"),
				new ImportPropertyInfoImpl<CodeDescriptionBoolTreeNode>("Bool")
			};
			using (var importForm = new DataImportWizardForm(collectionInfo, ""))
			{
				var wizardMock = new Mock<ImportWizard>(collectionInfo, null, null) { CallBase = true };
				wizardMock
					.Setup(m => m.LoadFile(1, -1, false))
					.Returns(new List<string[]>() { new string[] { code, code, "1" } });
				wizardMock.Object.Mapping[0].AddFileColumnIndex(0);
				wizardMock.Object.Mapping[1].AddFileColumnIndex(1);
				wizardMock.Object.Mapping[2].AddFileColumnIndex(2);

				((KForm)importForm).DataSource = wizardMock.Object;
				typeof(DataImportWizardForm).InvokeMember("DoImport", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, importForm, Array.Empty<object>());
				Application.DoEvents();
			}
		}
	}
}
