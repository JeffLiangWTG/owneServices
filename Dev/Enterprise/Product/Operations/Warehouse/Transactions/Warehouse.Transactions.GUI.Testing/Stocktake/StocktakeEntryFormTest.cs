using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI.Stocktake;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class StocktakeEmailMenuItemTest : WhsDocketFormTestCase
	{
		#region Implementation

		protected override ZForm GetNewDocketForm()
		{
			return new StocktakeEntryForm(Factory.New<WhsStocktake>(), new NotificationSubscriberGuiHelper());
		}

		#endregion
	}

	[TestedType(typeof(StocktakeEntryForm))]
	class StocktakeEntryFormTest : ZFormBasherTest
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var stocktake = Factory.New<WhsStocktake>();
			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Posting not setup", true, form.SetupPostingCalled);
				AssertNotNull("Billing not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull("EDocs not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull("Documents not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown if null INotificationSubscriberQueryUser is passed in.",
				() => new TestStocktakeEntryForm(stocktake, null));
		}

		#endregion

		#region TestEDocsModifySetToOffNotDisablePlugin

		public void TestEDocsModifySetToOffNotDisablePlugin()
		{
			Env.Security.eDocs.IsAllowed = true;
			Env.Security.eDocsModify.IsAllowed = false;

			var stocktake = Factory.New<WhsStocktake>();
			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var eDocPlugin = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				Assert("Plugin should not be disabled.", eDocPlugin.SecurityCheckpoint.IsAllowed);
			}
		}

		#endregion

		#region TestOnLoad

		public void TestOnLoad()
		{
			var stocktake = Factory.New<WhsStocktake>();
			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("FilterTabPage not selected", form.LinesTabControl.GetTabPage("FilterTabPage"), form.LinesTabControl.SelectedTab);
			}

			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Loaded;
				form.Show();
				AssertEquals("LinesTabPage not selected", form.LinesTabControl.GetTabPage("LinesTabPage"), form.LinesTabControl.SelectedTab);
			}
		}

		#endregion

		#region TestFormCaption

		public void TestFormCaption()
		{
			var stocktake = Factory.New<WhsStocktake>();
			using (var form = new StocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Stocktake/Cycle Count", form.FormCaption.Trim());
				stocktake.WS_StocktakeNumber = "W00000001";
				AssertEquals("Stocktake/Cycle Count W00000001", form.FormCaption);
			}
		}

		#endregion

		#region TestLoadButton

		public void TestLoadButton()
		{
			var stocktake = Factory.New<WhsStocktake>();
			var org1 = Helper.CreateClient("ORG1");
			var org2 = Helper.CreateClient("ORG2");
			var whs = Helper.CreateWarehouse("AAAA");
			var part = Helper.CreateProduct(org2, "AAA");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			row.WR_Columns = 2;
			row.WR_Levels = 2;

			org2.MiscServ.OM_IMPartAttrib1Name = "Batch #";
			org2.MiscServ.OM_IMPartAttrib1Type = Enterprise.MasterFiles.Business.PartAttributeTypeList.Codes.BatchNumber;
			org2.MiscServ.OM_IMPartAttrib2Name = "Vehicle #";
			org2.MiscServ.OM_IMPartAttrib2Type = Enterprise.MasterFiles.Business.PartAttributeTypeList.Codes.VIN;
			org2.MiscServ.OM_IMPartAttrib3Name = "Colour";
			org2.MiscServ.OM_IMPartAttrib3Type = Enterprise.MasterFiles.Business.PartAttributeTypeList.Codes.Mandatory;
			org2.MiscServ.OM_IMUseExpiryDate = true;
			org2.MiscServ.OM_IMUsePackingDate = true;

			var receive = Helper.CreateWhsReceive(org2, whs, "1000", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, 10m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.LoadButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter a Warehouse"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				stocktake.WS_WW_Whs = whs.PK;
				stocktake.WS_OH_Client = org1.PK;
				form.LoadButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("There is no inventory which matches this criteria"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				stocktake.WS_OH_Client = org2.PK;
				form.LoadButton.PerformClick();
				AssertEquals("Must load 1 line", 1, stocktake.Lines.Count);
				AssertAttributeTitles(form.StocktakeFilterStipUserControl.Grid, "Batch #", "Vehicle #", "Colour");
			}
		}

		void AssertAttributeTitles(ZGrid grid, string part1, string part2, string part3)
		{
			if (!string.IsNullOrEmpty(part1))
			{
				AssertEquals("PartAttrib1 title incorrect", part1, grid.Columns[WhsStocktakeLineSchema.WU_PartAttrib1.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part2))
			{
				AssertEquals("PartAttrib2 title incorrect", part2, grid.Columns[WhsStocktakeLineSchema.WU_PartAttrib2.Name].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part3))
			{
				AssertEquals("PartAttrib3 title incorrect", part3, grid.Columns[WhsStocktakeLineSchema.WU_PartAttrib3.Name].ColumnStyle.HeaderText);
			}
		}

		#endregion

		#region TestCloseLinesButton

		public void TestCloseLinesButton()
		{
			// Setup test data

			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part2);

			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.One, true);
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(stocktake.Client, AttributeNumber.ExpiryDate, true);

			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(stocktake.Client, data.Part1, AttributeNumber.ExpiryDate, true);

			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition: Stocktake is not saved", false, stocktake.IsInDatabase);
				form.LinesTabControl.SelectedTab = form.LinesTabControl.GetTabPage("LinesTabPage");
				form.CloseLinesButton.PerformClick();
				AssertEquals("You must save the Stocktake/Cycle Count before any Stocktake/Cycle Count lines can be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save();

				form.CloseLinesButton.PerformClick();
				AssertEquals("Please select the lines you wish to close.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var line = stocktake.StocktakeLinesForFilter.AddNew(); // Since there are no product code or location for this line. There are validation errors.
				AssertEquals("Precondition", false, line.HasErrors);
				form.StocktakeFilterStipUserControl.Grid.Select(0);
				form.CloseLinesButton.PerformClick();
				AssertEquals("You must correct validation errors in selected lines.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Stocktake line should still be opened.", StocktakeLineStatus.Codes.Open, stocktake.StocktakeLinesForFilter[0].WU_Status);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				line.WU_OP = data.Part1.PK;
				line.LocationString = "A";
				AssertEquals("Precondition", false, line.HasErrors);
				form.StocktakeFilterStipUserControl.Grid.Select(0);
				form.CloseLinesButton.PerformClick();
				AssertEquals("You must correct validation errors in selected lines.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Stocktake line should still be opened.", StocktakeLineStatus.Codes.Open, stocktake.StocktakeLinesForFilter[0].WU_Status);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				line.WU_PartAttrib1 = "1";
				line.WU_PartAttrib2 = "2";
				line.WU_PartAttrib3 = "3";
				line.WU_ExpiryDate = ZDate.Today;
				line.WU_PackingDate = ZDate.Today;
				AssertEquals("Precondition", false, line.HasErrors);
				form.StocktakeFilterStipUserControl.Grid.Select(0);
				form.CloseLinesButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Stocktake line should be closed", StocktakeLineStatus.Codes.Closed, stocktake.StocktakeLinesForFilter[0].WU_Status);
			}
		}

		#endregion

		#region TestNewCountMenuItem

		public void TestNewCountMenuItem()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup a receive and an inventory line
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1000", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			// setup a stocktake without loading it

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			Factory.Save();

			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				// Test New count action menu item

				AssertEquals("Since stocktake is not loaded, New count menu item should not be enabled.", false, form.NewCountMenuItem.Enabled);

				// Stocktake is loaded but not saved

				form.LoadButton.PerformClick(); // load the stocktake
				AssertEquals(2, stocktake.Lines.Count);
				AssertEquals((byte)1, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals((byte)1, stocktake.Lines[1].WU_TotalCounts);
				AssertEquals("Since stocktake is loaded, new count menu item is enabled.", true, form.NewCountMenuItem.Enabled);

				// Grid should only show 1st count column

				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_LastCount));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_DateVerified));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy));

				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count2));
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count2DateVerified));
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy));

				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count3));
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count3DateVerified));
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy));

				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy].ColumnStyle.ReadOnly);
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_LastCount].ColumnStyle.ReadOnly);
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_DateVerified].ColumnStyle.ReadOnly);

				form.NewCountMenuItem.PerformClick();
				AssertEquals("Since newly loaded stocktake is not saved, we should show an error message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please save all changes before adding a new count column.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save(); // Save newly loaded stocktake

				// Test Stocktake is loaded and saved.

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.NewCountMenuItem.PerformClick(); // Adding second new count column
				AssertEquals("All open lines will require count data to be entered, this action cannot be undone.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Since user clicks on cancel button it should not add the new count column.", (byte)1, stocktake.CurrentCountColumnNumber);
				AssertEquals((byte)1, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals((byte)1, stocktake.Lines[1].WU_TotalCounts);

				stocktake.CloseLines(new[] { stocktake.Lines[1] });
				AssertEquals("Precondition: stocktake has one adjustment", 1, stocktake.Adjustments.Count);
				stocktake.Adjustments[0].RunPreSaveValidation(); //creates picklines and allocates stock
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.NewCountMenuItem.PerformClick(); // Adding second new count column
				AssertEquals("All open lines will require count data to be entered, this action cannot be undone.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals((byte)2, stocktake.CurrentCountColumnNumber);
				AssertEquals((byte)2, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals((byte)1, stocktake.Lines[1].WU_TotalCounts);

				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_LastCount));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_DateVerified));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy));

				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count2));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count2DateVerified));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy));

				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count3));
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count3DateVerified));
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy));

				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy].ColumnStyle.ReadOnly);
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_LastCount].ColumnStyle.ReadOnly);
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_DateVerified].ColumnStyle.ReadOnly);

				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_Count2].ColumnStyle.ReadOnly);
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_Count2DateVerified].ColumnStyle.ReadOnly);
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy].ColumnStyle.ReadOnly);

				// Test stocktake line is modified but trying to add a new count column

				stocktake.Lines[0].WU_Count2 = 2; // Stocktake line is using second count
				form.NewCountMenuItem.PerformClick();
				AssertEquals("Since stocktake is having changes, user can't add a new count column until those changes are saved.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please save all changes before adding a new count column.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals((byte)2, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals((byte)1, stocktake.Lines[1].WU_TotalCounts);

				Factory.Save(); // Save all changes

				// Test stocktake line changes has been saved to the database

				UnitTestUserNotification.Instance.AddOKAnswer();
				form.NewCountMenuItem.PerformClick();
				AssertEquals("All open lines will require count data to be entered, this action cannot be undone.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals((byte)3, stocktake.CurrentCountColumnNumber);
				AssertEquals((byte)3, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals((byte)1, stocktake.Lines[1].WU_TotalCounts);

				stocktake.Lines[0].WU_TotalCounts = WhsStocktake.MaximumNumberOfColumns - 1; // Maximum - 1 last count

				Factory.Save();

				UnitTestUserNotification.Instance.AddOKAnswer();
				form.NewCountMenuItem.PerformClick(); // Add last new count column
				AssertEquals("All open lines will require count data to be entered, this action cannot be undone.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, stocktake.IsMaximumAmountOfColumnsAdded);
				AssertEquals((byte)3, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals((byte)1, stocktake.Lines[1].WU_TotalCounts);

				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_LastCount));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_DateVerified));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy));

				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count2));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count2DateVerified));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy));

				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count3));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count3DateVerified));
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns.Contains(WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy));

				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy].ColumnStyle.ReadOnly);
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_LastCount].ColumnStyle.ReadOnly);
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_DateVerified].ColumnStyle.ReadOnly);

				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_Count2].ColumnStyle.ReadOnly);
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_Count2DateVerified].ColumnStyle.ReadOnly);
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy].ColumnStyle.ReadOnly);

				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_Count3].ColumnStyle.ReadOnly);
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_Count3DateVerified].ColumnStyle.ReadOnly);
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.Columns[WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy].ColumnStyle.ReadOnly);

				// stocktake.WS_LastCount is now maximum
				AssertEquals(false, form.NewCountMenuItem.Enabled);

				form.NewCountMenuItem.Enabled = true; // Reset it back to one so that we can test whether the menuitem is disabled after the stocktake is finalised

				stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
				AssertEquals("After finalising the stocktake, new count menu item should be disabled.", false, form.NewCountMenuItem.Enabled);
			}
		}

		public void TestNewCountMenuItem_CurrentCountIsMaxValue()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup a receive and an inventory line
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1000", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			// setup a stocktake without loading it

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			Factory.Save();

			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.LoadButton.PerformClick(); // load the stocktake
				AssertEquals(2, stocktake.Lines.Count);
				AssertEquals("Precondition: CurrentCountColumnNumber", (byte)1, stocktake.CurrentCountColumnNumber);
				AssertEquals("Precondition: WU_TotalCounts", (byte)1, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals("Precondition: WU_TotalCounts", (byte)1, stocktake.Lines[1].WU_TotalCounts);
				AssertEquals("Since stocktake is loaded, new count menu item is enabled.", true, form.NewCountMenuItem.Enabled);
				Factory.Save(); // Save newly loaded stocktake

				UnitTestUserNotification.Instance.AddOKAnswer();
				form.NewCountMenuItem.PerformClick(); // Adding second new count column
				AssertEquals("All open lines will require count data to be entered, this action cannot be undone.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Precondition: CurrentCountColumnNumber", (byte)2, stocktake.CurrentCountColumnNumber);
				AssertEquals("Precondition: WU_TotalCounts", (byte)2, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals("Precondition: WU_TotalCounts", (byte)2, stocktake.Lines[1].WU_TotalCounts);

				// updating stocktake in a new factory
				var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
				var stockTakeInNewFactory = newFactory.Load<WhsStocktake>(stocktake.PK);
				foreach (var stockTakeLine in stockTakeInNewFactory.Lines)
				{
					stockTakeLine.WU_TotalCounts++;
				}

				newFactory.Save();

				AssertEquals("Precondition: Maximum Amount of columns is added.", true, stocktake.IsMaximumAmountOfColumnsAdded);
				AssertEquals("Precondition: CurrentCountColumnNumber is updated.", (byte)3, stocktake.CurrentCountColumnNumber);
				AssertEquals("Precondition: WU_TotalCounts is updated.", (byte)3, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals("Precondition: WU_TotalCounts is updated.", (byte)3, stocktake.Lines[1].WU_TotalCounts);
				AssertEquals("Precondition: New Count Menu Item is still enabled.", true, form.NewCountMenuItem.Enabled);

				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertNoExceptionThrown("No exception is thrown.", form.NewCountMenuItem.PerformClick);

				AssertEquals("CurrentCountColumnNumber is not updated.", (byte)3, stocktake.CurrentCountColumnNumber);
				AssertEquals("WU_TotalCounts is not updated.", (byte)3, stocktake.Lines[0].WU_TotalCounts);
				AssertEquals("WU_TotalCounts is not updated.", (byte)3, stocktake.Lines[1].WU_TotalCounts);
				AssertEquals("New Count Menu Item is disabled.", false, form.NewCountMenuItem.Enabled);
				AssertEquals("Maximum number of count columns has already been added.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Maximum number of count columns has already been added.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region TestAssignAllLinesToUsersMenuItem

		[TestDate(2012, 05, 05)]
		public void TestAssignAllLinesToUsersMenuItem_Count1()
		{
			AssertAssignAllLinesToUserMenuItem(1);
		}

		[TestDate(2012, 05, 05)]
		public void TestAssignAllLinesToUsersMenuItem_Count2()
		{
			AssertAssignAllLinesToUserMenuItem(2);
		}

		[TestDate(2012, 05, 05)]
		public void TestAssignAllLinesToUsersMenuItem_Count3()
		{
			AssertAssignAllLinesToUserMenuItem(3);
		}

		void AssertAssignAllLinesToUserMenuItem(ZByte numberOfCounts)
		{
			// Setup test data

			var data = new TestDataSimpleEnvironment(Factory);
			var activeStaff1 = Helper.CreateGlbStaff("01", "T1", true);
			var activeStaff2 = Helper.CreateGlbStaff("02", "T2", true);
			var activeStaff3 = Helper.CreateGlbStaff("03", "T3", true);
			var inactiveStaff = Helper.CreateGlbStaff("04", "T4", false);

			// Setup a receive and an inventory line

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1000", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			// setup a stocktake without loading it
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.New);

			Factory.Save();

			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				AssertEquals("Since stocktake is not loaded, Assign All lines to users menu item should not be enabled.", false, form.AssignAllLinesToUser.Enabled);

				form.LoadButton.PerformClick(); // load the stocktake

				Factory.Save();

				form.StocktakeFilterStipUserControl.FirePerformSearch();

				AssertEquals("Pre-condition", 2, stocktake.StocktakeLinesForFilter.Count);
				AssertEquals("Since stocktake is loaded, Assign All lines to users menu item is enabled.", true, form.AssignAllLinesToUser.Enabled);

				form.AssignAllLinesToUser.PerformClick();

				var staffCollectionPropertyInfo = typeof(ZRecordAttacher).GetField("originalFindBoxList", BindingFlags.NonPublic | BindingFlags.Instance);
				var staffCollection = (GlbStaffCollection)staffCollectionPropertyInfo.GetValue(form.StocktakeLineAttacherForTesting);

				AssertCollectionContains("Collection should contain active staff.", activeStaff1, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff2, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff3, staffCollection);
				AssertCollectionNotContains("Collection should not contain inactive staff.", inactiveStaff, staffCollection);

				var embeddedModulePopup = form.LastShownStocktakeLineAttachPopupForTesting;
				var line1 = stocktake.StocktakeLinesForFilter[0];
				var line2 = stocktake.StocktakeLinesForFilter[1];
				line1.WU_TotalCounts = numberOfCounts;
				line2.WU_TotalCounts = numberOfCounts;

				// All unassigned stocktake lines.

				embeddedModulePopup.SelectStaffForEmbeddedModuleSelection(activeStaff1);
				AssertEquals(activeStaff1.GS_Code, line1.CurrentCountVerifiedBy);
				AssertEquals(activeStaff1.GS_Code, line2.CurrentCountVerifiedBy);

				// One unassigned and one assigned line.

				line1.CurrentCountVerifiedDate = ZDateTime.Now;
				Factory.Save();
				form.AssignAllLinesToUser.PerformClick();

				embeddedModulePopup.SelectStaffForEmbeddedModuleSelection(activeStaff2);
				AssertEquals(activeStaff1.GS_Code, line1.CurrentCountVerifiedBy);
				AssertEquals(activeStaff2.GS_Code, line2.CurrentCountVerifiedBy);

				// All assigned lines.

				line1.CurrentCountVerifiedDate = ZDateTime.Now;
				line2.CurrentCountVerifiedDate = ZDateTime.Now;
				form.LastShownStocktakeLineAttachPopupForTesting = null;
				Factory.Save();
				form.AssignAllLinesToUser.PerformClick();

				AssertNull("No popup should be shown.", form.LastShownStocktakeLineAttachPopupForTesting);
				AssertEquals("Since there are no un-assigned lines, it should show an error message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(form.NoUnAssignedLinesErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(activeStaff1.GS_Code, line1.CurrentCountVerifiedBy);
				AssertEquals(activeStaff2.GS_Code, line2.CurrentCountVerifiedBy);

				// With Closed and open line.

				line2.CurrentCountVerifiedDate = ZDateTime.Empty;
				stocktake.CloseLines(new[] { line1 });
				AssertEquals("Precondition: stocktake has one adjustment", 1, stocktake.Adjustments.Count);
				stocktake.Adjustments[0].RunPreSaveValidation(); //creates picklines and allocates stock
				Factory.Save();
				form.AssignAllLinesToUser.PerformClick();

				embeddedModulePopup.SelectStaffForEmbeddedModuleSelection(activeStaff3);
				AssertEquals(activeStaff1.GS_Code, line1.CurrentCountVerifiedBy);
				AssertEquals(activeStaff3.GS_Code, line2.CurrentCountVerifiedBy);

				stocktake.CloseLines(new[] { line2 });
				AssertEquals(StocktakeStatus.Codes.Finalised, stocktake.WS_StocktakeStatus);
				AssertEquals("After finalising the stocktake, new count menu item should be disabled.", false, form.AssignAllLinesToUser.Enabled);
			}
		}

		#endregion

		#region TestReadOnlyGrid

		public void TestReadOnlyGrid()
		{
			var stocktake = Factory.New<WhsStocktake>();

			using (var form = new StocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.New;
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.ReadOnly);

				stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Loaded;
				AssertEquals(false, form.StocktakeFilterStipUserControl.Grid.ReadOnly);

				stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
				AssertEquals(true, form.StocktakeFilterStipUserControl.Grid.ReadOnly);
			}
		}

		#endregion

		#region TestLoadButton_FinalisedStocktake

		public void TestLoadButton_FinalisedStocktake()
		{
			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.New;

			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.LoadButton.Enabled);

				stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Loaded;
				AssertEquals(true, form.LoadButton.Enabled);

				stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
				AssertEquals(false, form.LoadButton.Enabled);
			}
		}

		#endregion

		#region TestAddAndRemoveManualLine_UpdateStocktakeStatus

		public void TestAddAndRemoveManualLine_UpdateStocktakeStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.New);
			Factory.Save();

			using (var form = new TestStocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.LoadButton.PerformClick(); // load the stocktake
				AssertEquals("Precondition", StocktakeStatus.Codes.Loaded, stocktake.WS_StocktakeStatus);
				Factory.Save();

				form.StocktakeFilterStipUserControl.FirePerformSearch();
				AssertEquals("Precondition", 2, stocktake.StocktakeLinesForFilter.Count);

				var grid = form.StocktakeFilterStipUserControl.Grid;
				var lines = (WhsStocktakeLineCollection)grid.List;
				var line1 = lines[0];
				var line2 = lines[1];
				lines.AddNew();
				AssertEquals("There should be three lines along with manually added line.", 3, grid.List.Count);

				stocktake.CloseLines(new[] { line1, line2 });
				AssertEquals(StocktakeStatus.Codes.Loaded, stocktake.WS_StocktakeStatus);

				grid.Select(2);
				grid.DeleteMenuItem.PerformClick();
				AssertEquals(StocktakeStatus.Codes.Finalised, stocktake.WS_StocktakeStatus);

				form.Close();
			}
		}

		#endregion

		#region TestDetachAndSaveWithoutLoadStocktake

		[ExpectNoExceptions]
		public void TestDetachAndSaveWithoutLoadStocktake()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			using (var form = new StocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				WhsStocktakeProductFilterGrid grid = form.Controls.Find("whsStocktakeProductFilterGrid1", true)[0] as WhsStocktakeProductFilterGrid;
				AssertNotNull("There should be a WhsStocktakeProductFilterGrid on the form.", grid);
				form.Show();
				Helper.CreateWhsStocktakeProductFilter(stocktake, data.Part1);
				Factory.Save();
				grid.DetachMessage = null;
				grid.DetachSelectedElement();
				Factory.Save();
			}
		}

		#endregion

		#region TestLocationAutoComplete

		public void TestLocationAutoCompleteDisabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			using (var form = new StocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				var locationFindBox = (IFindBoxUserControl)form.Controls.Find("LocationFindBox", true)[0];
				Assert("Location find box should have auto complete disabled", locationFindBox.AutoCompleteDisabled);
			}
		}

		public void TestNonLocationAutoCompleteEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			using (var form = new StocktakeEntryForm(stocktake, new NotificationSubscriberGuiHelper()))
			{
				var commodityFindBox = (IFindBoxUserControl)form.Controls.Find("CommodityCodeFindBox", true)[0];
				Assert("Commodity find box should have auto complete enabled", !commodityFindBox.AutoCompleteDisabled);
			}
		}

		#endregion

		#region TestQueryUser

		public void TestQueryUser()
		{
			var helperMock = new Mock<INotificationSubscriberQueryUser>();
			var stocktake = Factory.New<WhsStocktake>();
			using (var form = new StocktakeEntryForm(stocktake, helperMock.Object))
			{
				GUITestHelper.VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(form, helperMock);
			}
		}

		#endregion

		#region Implementation

		class TestStocktakeEntryForm : StocktakeEntryForm
		{
			public TestStocktakeEntryForm(WhsStocktake stocktake, NotificationSubscriberGuiHelper whsNotificationSubscriberGuiHelper)
				: base(stocktake, whsNotificationSubscriberGuiHelper)
			{
			}

			public new bool SetupPostingCalled => base.SetupPostingCalled;
			public new ZTemplateTabControl LinesTabControl => base.LinesTabControl;
			public new ZButton CloseLinesButton => base.CloseLinesButton;
			public new ZButton LoadButton => base.LoadButton;
			public new MenuItem NewCountMenuItem => base.NewCountMenuItem;
			public new MenuItem AssignAllLinesToUser => base.AssignAllLinesToUser;
			public new string NoUnAssignedLinesErrorMessage => StocktakeEntryForm.NoUnAssignedLinesErrorMessage;
		}

		protected override Form GetFormToBashCore()
		{
			return new StocktakeEntryForm(Factory.New<WhsStocktake>(), new NotificationSubscriberGuiHelper()) { ControllerID = ControllerIDs.WhsReceive };
		}

		TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());
		TestNotificationBuffer notify;

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
