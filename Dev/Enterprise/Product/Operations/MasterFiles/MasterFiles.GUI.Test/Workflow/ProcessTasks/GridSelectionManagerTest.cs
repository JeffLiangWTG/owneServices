using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GridSelectionManagerTest : TestCaseWithDummy
	{
		public void TestListChanged_WhenParentFormIsDisposing_ShouldDoNothing()
		{
			var managerListChangedExecutedCount = 0;
			var gridListChangedExecutedCount = 0;

			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			using (var form = new ZForm_ForGridSelectionManagerTest(Dummy, grid))
			{
				form.Show();
				Application.DoEvents();

				var manager = new GridSelectionManager_ForTest(grid, () => grid.FindForm() as ZForm, false);
				manager.ListChangedExecuted += (_, __) => managerListChangedExecutedCount++;
				grid.ListManager.ListChanged += (_, __) => gridListChangedExecutedCount++;
				grid.ListManager.ListChanged += manager.ListChanged;

				AssertEquals(0, managerListChangedExecutedCount);
				AssertEquals(0, gridListChangedExecutedCount);
				AssertEquals("If we don't add the two bizos at the start of the test, it causes extra calls to ListChanged in some places. So please leave them where they are!", 2, grid.ListManager.Count);

				grid.ListManager.AddNew();
				Application.DoEvents();

				AssertEquals(1, managerListChangedExecutedCount);
				AssertEquals(1, gridListChangedExecutedCount);
			}

			Application.DoEvents();

			AssertEquals("The form was disposing, so the manager should do nothing with the grid's ListChanged events, because they don't matter and because it could otherwise cause difficult-to-track-down errors. SAD!", 1, managerListChangedExecutedCount);
			AssertEquals("This verifies that the grid did still try to execute its ListChanged event, but that the manager ignored it. If this stayed at 1, it means the test isn't really proving what it's meant to prove. SAD!", 3, gridListChangedExecutedCount);
		}

		[RequiresSTA]
		public void TestReselectItemSynchronously()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			using (var form = new ZForm_ForGridSelectionManagerTest(Dummy, grid))
			{
				form.Show();
				Application.DoEvents();

				grid.ListManager.Position = 1;

				var manager = new GridSelectionManager_ForTest(grid, () => form, false);
				manager.SetItemToSelectOnBinding();

				grid.ListManager.Position = 0;

				AssertEquals("Precondition: should be in position 0", 0, grid.ListManager.Position);

				manager.ReselectItem();

				AssertEquals("Should restore position to 1", 1, grid.ListManager.Position);
			}
		}

		public void TestReselectItem_AlwaysInvoke()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			using (var form = new ZForm_ForGridSelectionManagerTest(Dummy, grid))
			{
				form.Show();
				Application.DoEvents();

				grid.ListManager.Position = 1;

				var manager = new GridSelectionManager_ForTest(grid, () => form, true);
				manager.SetItemToSelectOnBinding();

				grid.ListManager.Position = 0;

				AssertEquals("Precondition: should be in position 0", 0, grid.ListManager.Position);

				manager.ReselectItem();

				AssertEquals("Still 0", 0, grid.ListManager.Position);

				Application.DoEvents();

				AssertEquals("Should restore position to 1", 1, grid.ListManager.Position);
			}
		}

		public void TestReselectItemAsynchronously()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			using (var form = new ZForm_ForGridSelectionManagerTest(Dummy, grid))
			{
				form.Show();
				Application.DoEvents();

				grid.ListManager.Position = 1;

				var manager = new GridSelectionManager_ForTest(grid, () => form, false);
				manager.SetItemToSelectOnBinding();

				grid.ListManager.Position = 0;

				AssertEquals("Precondition: should be in position 0", 0, grid.ListManager.Position);

				Task.Run(manager.ReselectItem).Wait();

				AssertEquals("Should schedule restoring position but not do it yet", 0, grid.ListManager.Position);

				Application.DoEvents();

				AssertEquals("Should restore position to 1", 1, grid.ListManager.Position);
			}
		}

		[RequiresSTA]
		public void TestReselectItemAsynchronouslyWhileSaving()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			using (var form = new ZForm_ForGridSelectionManagerTest(Dummy, grid))
			{
				form.Show();
				Application.DoEvents();

				grid.ListManager.Position = 1;

				var manager = new GridSelectionManager_ForTest(grid, () => form, false);
				manager.SetItemToSelectOnBinding();

				grid.ListManager.Position = 0;

				AssertEquals("Precondition: should be in position 0", 0, grid.ListManager.Position);

				using (form.SetSavingInProgressExposedForTest())
				{
					manager.ReselectItem();
				}

				AssertEquals("Should schedule restoring position but not do it yet", 0, grid.ListManager.Position);

				Application.DoEvents();

				AssertEquals("Should restore position to 1", 1, grid.ListManager.Position);
			}
		}

		class GridSelectionManager_ForTest : GridSelectionManager
		{
			public GridSelectionManager_ForTest(ZGrid grid, Func<ZForm> getParentForm, bool alwaysInvoke)
				: base(grid, getParentForm, alwaysInvoke)
			{
			}

			protected override void ListChangedCore(ListChangedType listChangedType)
			{
				ListChangedExecuted?.Invoke(this, EventArgs.Empty);
			}

			public event EventHandler ListChangedExecuted;
		}

		class ZForm_ForGridSelectionManagerTest : ZForm
		{
			public ZForm_ForGridSelectionManagerTest(DummyBusinessObject dummy, ZGrid grid)
				: base(dummy)
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				grid.BindTo = "Collection";
				grid.GridId = Guid.NewGuid().ToString();
				grid.Dock = DockStyle.Fill;
				Controls.Add(grid);
				this.grid = grid;
			}

			readonly ZGrid grid;

			protected override void UnbindControls()
			{
				grid.ListManager?.AddNew(); // this will get called on dispose, but before the ListManager is null.
				base.UnbindControls();
			}
		}
	}
}
