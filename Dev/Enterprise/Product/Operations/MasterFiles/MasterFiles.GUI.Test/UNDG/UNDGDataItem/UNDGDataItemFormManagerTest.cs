using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class UNDGDataItemFormManagerTest : TestCaseWithFactory
	{
		public void TestDoubleBindingShouldNotAddDuplicateColumnStyles()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_Description = ZString.Empty;
			collection.Add(bizObj1);

			var grid = new ZGrid();
			grid.GridId = Guid.NewGuid().ToString();
			grid.Columns.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));

			new UNDGDataItemFormManager(grid).Initialize();

			using (ZForm form = new ZForm())
			{
				form.Controls.Add(grid);
				grid.ReOrderColumns(grid.Columns.Select(column => column.ColumnName).ToArray());
				new UNDGDataItemFormManager(grid).Initialize(); //double initialization from a rare UI scenario
				AssertNoExceptionThrown(() => grid.SetDataBinding(collection, "Collection"));
			}
		}

		public void TestDoNotShowUnwantedColumns()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_Description = ZString.Empty;
			collection.Add(bizObj1);

			var grid1 = new ZGrid();
			grid1.GridId = Guid.NewGuid().ToString();
			grid1.Columns.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			grid1.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			new UNDGDataItemFormManager(grid1).Initialize();
			var columnsCountWithStandardContructor = 0;
			using (ZForm form1 = new ZForm())
			{
				form1.Controls.Add(grid1);
				grid1.SetDataBinding(collection, "Collection");
				columnsCountWithStandardContructor = grid1.ColumnStyles.Count;
			}

			var grid2 = new ZGrid();
			grid2.GridId = Guid.NewGuid().ToString();
			grid2.Columns.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			grid2.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			new UNDGDataItemFormManager(grid2, "", UNDGDataItemFormManagerConfig.IMOHideProperties()).Initialize();
			var columnsCountWithSlimLineCtor = 0;
			using (ZForm form2 = new ZForm())
			{
				form2.Controls.Add(grid2);
				grid2.SetDataBinding(collection, "Collection");
				columnsCountWithSlimLineCtor = grid2.ColumnStyles.Count;
			}

			AssertEquals(columnsCountWithStandardContructor - 9, columnsCountWithSlimLineCtor);
		}

		public void TestShouldHidePredicate()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_Description = ZString.Empty;
			collection.Add(bizObj1);

			var grid1 = new ZGrid();
			grid1.GridId = Guid.NewGuid().ToString();
			grid1.Columns.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			grid1.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			new UNDGDataItemFormManager(grid1, "", UNDGDataItemFormManagerConfig.ShowSubstanceProperties()).Initialize();
			var columnsCountWithStandardContructor = 0;
			using (ZForm form1 = new ZForm())
			{
				form1.Controls.Add(grid1);
				grid1.SetDataBinding(collection, "Collection");
				columnsCountWithStandardContructor = grid1.ColumnStyles.Count;
			}

			var grid2 = new ZGrid();
			grid2.GridId = Guid.NewGuid().ToString();
			grid2.Columns.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			grid2.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			new UNDGDataItemFormManager(grid2, "", UNDGDataItemFormManagerConfig.ShowSubstanceProperties()) { ShouldHidePredicate = style => style.ColumnName != "UNDGs+UNDGTechnicalNameManager+Value" }.Initialize();
			var columnsCountWithSlimLineCtor = 0;
			using (ZForm form2 = new ZForm())
			{
				form2.Controls.Add(grid2);
				grid2.SetDataBinding(collection, "Collection");
				columnsCountWithSlimLineCtor = grid2.ColumnStyles.Count;
			}

			AssertEquals(columnsCountWithStandardContructor - 11, columnsCountWithSlimLineCtor);
		}
	}
}
