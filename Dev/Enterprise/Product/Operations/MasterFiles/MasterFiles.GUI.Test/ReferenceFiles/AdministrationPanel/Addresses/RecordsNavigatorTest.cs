using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RecordsNavigatorTest : TestCaseWithFactory
	{
		public void TestUpdateLayouts()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			using (var grid = new ZGrid())
			using (var form = new ZForm())
			using (var navigator = new RecordsNavigator())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
				form.Controls.Add(grid);
				form.Controls.Add(navigator);
				form.Show();
				var moveToLeftButton = navigator.Controls.Find("MoveLeftButton", true)[0];
				var moveToRightButton = navigator.Controls.Find("MoveRightButton", true)[0];
				var moveToFirstButton = navigator.Controls.Find("MoveToFirstButton", true)[0];
				var moveToLastButton = navigator.Controls.Find("MoveToLastButton", true)[0];
				var totalNumberTextBox = navigator.Controls.Find("TotalNumberTextBox", true)[0];
				var currentNumberTextBox = navigator.Controls.Find("CurrentNumberTextBox", true)[0];
				grid.SetDataBinding(collection, "");
				navigator.BindToCurrencyManager(grid.ListManager);
				AssertEquals("Total records", "3", totalNumberTextBox.Text);
				AssertEquals("Current number", "1", currentNumberTextBox.Text);
				Assert("Move left is readonly", !moveToLeftButton.Enabled);
				Assert("Move to first is readonly", !moveToFirstButton.Enabled);
				Assert("Move right is enabled", moveToRightButton.Enabled);
				Assert("Move to last is enabled", moveToLastButton.Enabled);
				Assert("Control is enabled", navigator.Enabled);
				grid.ListManager.Position++;
				AssertEquals("Current number", "2", currentNumberTextBox.Text);
				Assert("Move left is enabled", moveToLeftButton.Enabled);
				Assert("Move to first is enabled", moveToFirstButton.Enabled);
				grid.ListManager.Position++;
				AssertEquals("Current number", "3", currentNumberTextBox.Text);
				Assert("Move right is readonly", !moveToRightButton.Enabled);
				Assert("Move to last is readonly", !moveToLastButton.Enabled);
				collection.RemoveAndDeleteAll();
				Assert("Control is readonly", !navigator.Enabled);
			}
		}

		public void TestNavigationButtons()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			using (var grid = new ZGrid())
			using (var form = new ZForm())
			using (var navigator = new RecordsNavigator())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
				form.Controls.Add(grid);
				form.Controls.Add(navigator);
				form.Show();
				ZTextBox currentNumberTextBox = navigator.Controls.Find("CurrentNumberTextBox", true)[0] as ZTextBox;
				grid.SetDataBinding(collection, "");
				navigator.BindToCurrencyManager(grid.ListManager);
				currentNumberTextBox.Focus();
				currentNumberTextBox.Text = "Not decimal";
				grid.Focus();
				AssertEquals("currentNumberTextBox's text doesn't change", "1", currentNumberTextBox.Text);
				AssertEquals("Position doesn't change", 0, grid.ListManager.Position);

				currentNumberTextBox.Focus();
				currentNumberTextBox.Text = "0";
				grid.Focus();
				AssertEquals("currentNumberTextBox's text doesn't change", "1", currentNumberTextBox.Text);
				AssertEquals("Position doesn't change", 0, grid.ListManager.Position);

				currentNumberTextBox.Focus();
				currentNumberTextBox.Text = "4";
				grid.Focus();
				AssertEquals("currentNumberTextBox's text doesn't change", "1", currentNumberTextBox.Text);
				AssertEquals("Position doesn't change", 0, grid.ListManager.Position);

				currentNumberTextBox.Focus();
				currentNumberTextBox.Text = "2";
				grid.Focus();
				AssertEquals("currentNumberTextBox's text changed to 2", "2", currentNumberTextBox.Text);
				AssertEquals("Position changed to 1", 1, grid.ListManager.Position);
			}
		}

		public void TestManuallyInputCurrentNumber()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			using (var grid = new ZGrid())
			using (var form = new ZForm())
			using (var navigator = new RecordsNavigator())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
				form.Controls.Add(grid);
				form.Controls.Add(navigator);
				form.Show();
				ZButton moveToLeftButton = navigator.Controls.Find("MoveLeftButton", true)[0] as ZButton;
				ZButton moveToRightButton = navigator.Controls.Find("MoveRightButton", true)[0] as ZButton;
				ZButton moveToFirstButton = navigator.Controls.Find("MoveToFirstButton", true)[0] as ZButton;
				ZButton moveToLastButton = navigator.Controls.Find("MoveToLastButton", true)[0] as ZButton;
				grid.SetDataBinding(collection, "");
				navigator.BindToCurrencyManager(grid.ListManager);
				moveToRightButton.PerformClick();
				AssertEquals("List current position is 1", 1, grid.ListManager.Position);
				moveToLeftButton.PerformClick();
				AssertEquals("List current position is 0", 0, grid.ListManager.Position);
				moveToLastButton.PerformClick();
				AssertEquals("List current position is 2", 2, grid.ListManager.Position);
				moveToFirstButton.PerformClick();
				AssertEquals("List current position is 0", 0, grid.ListManager.Position);
			}
		}

		public void TestSyncLayoutWithGrid()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.AddNew();
			collection.AddNew();
			var testDummy = collection.AddNew();
			using (var grid = new ZGrid())
			using (var form = new ZForm())
			using (var navigator = new RecordsNavigator())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
				form.Controls.Add(grid);
				form.Controls.Add(navigator);
				form.Show();
				ZTextBox currentNumberTextBox = navigator.Controls.Find("CurrentNumberTextBox", true)[0] as ZTextBox;
				ZTextBox totalNumberTextBox = navigator.Controls.Find("TotalNumberTextBox", true)[0] as ZTextBox;
				grid.SetDataBinding(collection, "");
				navigator.BindToCurrencyManager(grid.ListManager);
				grid.PerformMouseDownForTest(1, 1);
				AssertEquals("Current number changed to 2", "2", currentNumberTextBox.Text);

				collection.Remove(testDummy);
				AssertEquals("Current number changed to 2", "2", totalNumberTextBox.Text);

				collection.AddNew();
				AssertEquals("Current number changed to 3", "3", totalNumberTextBox.Text);
			}
		}
	}
}
