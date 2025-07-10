using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ReferenceNumberFormManagerTest : TestCaseWithFactory
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

			new ReferenceNumberFormManager(grid).Initialize();

			using (var form = new ZForm())
			{
				form.Controls.Add(grid);
				grid.ReOrderColumns(grid.Columns.Select(column => column.ColumnName).ToArray());
				new ReferenceNumberFormManager(grid).Initialize(); //double initialization from a rare UI scenario
				AssertNoExceptionThrown(() => grid.SetDataBinding(collection, "Collection"));
			}
		}
	}
}
