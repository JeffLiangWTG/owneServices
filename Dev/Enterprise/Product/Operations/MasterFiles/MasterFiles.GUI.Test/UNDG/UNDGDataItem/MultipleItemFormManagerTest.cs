using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class MultipleItemFormManagerTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestInitializeWithUnknownColumnToUpdate()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var bizObj1 = Factory.New<DummyBusinessObject>();
			collection.Add(bizObj1);
			var grid = new ZGrid();
			grid.GridId = Guid.NewGuid().ToString();
			grid.Columns.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));

			using (var form = new ZForm())
			{
				form.Controls.Add(grid);
				new MultipleItemFormManagerForTest(grid).Initialize();
				AssertNoExceptionThrown(() => grid.SetDataBinding(collection, "Collection"));
			}
		}

		sealed class MultipleItemFormManagerForTest : MultipleItemFormManager<string>
		{
			public MultipleItemFormManagerForTest(ZGrid grid) : base(grid, "")
			{
			}

			protected override void AddColumnsCore(ZGrid gridToAddTo, string bindingPrefixWithPlus)
			{
			}

			protected override void AddCountChangedEventHandlerToItemsCollection(string parent, EventHandler eventHandler)
			{
			}

			protected override int CollectionCount(string parent)
			{
				return 0;
			}

			protected override void ShowMultipleItemFormCore(string parent, Form parentForm)
			{
			}

			protected override ZString[] ColumnsToUpdate
			{
				get
				{
					return new ZString[] { "ABC", "XYZZ" };
				}
			}
		}
	}
}
