using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Guarantees;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(GuaranteeForm))]
	sealed class GuaranteeFormBaseOnlyTest : ZFormBasherTest
	{
		public void TestMenuItems()
		{
			using (var form = new GuaranteeForm(Factory.NewWithValidTestData<GuaranteeHeaderWithMessageSupportForTest>()))
			{
				var menuItems = form.Menu.MenuItems;
				AssertSequencesEqual(new[] { "&File", "&Edit", "Actio&ns", "&Messaging", "&Help" }, menuItems.Cast<MenuItem>().Select(x => x.Text));
			}
		}

		public void TestMessagingMenuVisibility()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				var menuItem = form.FindMenuItem_ForTest("Messaging");
				AssertNull("Messaging menu item should be hidden", menuItem);
			}

			using (var form = new GuaranteeForm(Factory.NewWithValidTestData<GuaranteeHeaderWithMessageSupportForTest>()))
			{
				var menuItem = form.FindMenuItem_ForTest("Messaging");
				AssertNotNull("Messaging menu item should be visible", menuItem);
			}
		}

		public void TestGuaranteeMessagesTabVisibility()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				var ctrl = form.FindSingleOrDefault<MessagesUserControl>("GuaranteeMessagesUserControl");
				AssertNull("Messages tab should not be visible", ctrl);
			}

			using (var form = new GuaranteeForm(Factory.NewWithValidTestData<GuaranteeHeaderWithMessageSupportForTest>()))
			{
				form.Show();
				var ctrl = form.FindSingleOrDefault<MessagesUserControl>("GuaranteeMessagesUserControl");
				AssertNotNull("Messages tab should be visible", ctrl);
			}
		}

		public void TestPendingBalanceAfterAddNewTransaction()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				var pendingBalanceZTextBox = form.Controls.Find("PendingBalanceZTextBox", true).FirstOrDefault();

				CombineAssertions(() =>
				{
					form.SetNewTransactionLineValues(750m, PermitTransactionTypeList.Codes.OBL, "", "BBBB");
					ClickNewTransactionAddButton(form);
					AssertEquals("CPH_Calc_OpeningBalance after OBL", 750m, guaranteeHeader.CPH_Calc_OpeningBalance);
					AssertEquals("CPH_Calc_TotalBalanceIncludingPending after OBL", 750m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPending.Amount);
					AssertEquals("CPH_Calc_UsedBalance after OBL", 0m, guaranteeHeader.CPH_Calc_UsedBalance);
					AssertEquals("CPH_Calc_PendingBalanceDecimal after OBL", 0m, guaranteeHeader.CPH_Calc_PendingBalanceDecimal);
					AssertEquals("pendingBalanceZTextBox after OBL", "0.00", pendingBalanceZTextBox.Text);

					form.SetNewTransactionLineValues(-100m, PermitTransactionTypeList.Codes.ADJ, "Some comment", "CCCC");
					ClickNewTransactionAddButton(form);
					AssertEquals("CPH_Calc_OpeningBalance after ADJ", 750m, guaranteeHeader.CPH_Calc_OpeningBalance);
					AssertEquals("CPH_Calc_TotalBalanceIncludingPending after ADJ", 650m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPending.Amount);
					AssertEquals("CPH_Calc_UsedBalance after ADJ", 100m, guaranteeHeader.CPH_Calc_UsedBalance);
					AssertEquals("CPH_Calc_PendingBalanceDecimal after ADJ", 0m, guaranteeHeader.CPH_Calc_PendingBalanceDecimal);
					AssertEquals("pendingBalanceZTextBox after ADJ", "0.00", pendingBalanceZTextBox.Text);

					form.SetNewTransactionLineValues(25m, GuaranteeTransactionTypeList.Codes.OBA, "Some comment", "CCCC");
					ClickNewTransactionAddButton(form);

					var bgcTask = new CalculateGuaranteeBalanceServiceTask();
					bgcTask.ServiceLogger = new Mock<ILogger>().Object;
					bgcTask.RunTask();

					AssertEquals("CPH_Calc_OpeningBalance after OBA", 775m, guaranteeHeader.CPH_Calc_OpeningBalance);
					AssertEquals("CPH_Calc_TotalBalanceIncludingPending after OBA", 675m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPending.Amount);
					AssertEquals("CPH_Calc_UsedBalance after OBA", 100m, guaranteeHeader.CPH_Calc_UsedBalance);
					AssertEquals("CPH_Calc_PendingBalanceDecimal after OBA", 0m, guaranteeHeader.CPH_Calc_PendingBalanceDecimal);
					AssertEquals("pendingBalanceZTextBox after OBA", "0.00", pendingBalanceZTextBox.Text);
				});
			}
		}

		public void TestNewTransactionAddButton_Mutex()
		{
			Factory.RefreshEnabled = false;
			var guaranteeHeader = GetGuaranteeHeader();
			var guaranteeHeaderInNewFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<BaseCusGuaranteeHeader>(guaranteeHeader.PK);
			_ = guaranteeHeaderInNewFactory.LockMutex;
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				form.SetNewTransactionLineValues(200m, PermitTransactionTypeList.Codes.OBL, "", "BBBB");
				ClickNewTransactionAddButton(form);
				AssertStartsWith("Warning", "Guarantee transactions are currently being edited by user", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			guaranteeHeaderInNewFactory.UnlockMutex();
		}

		public void TestGetGuaranteeTransactionFilterStripBusinessObject()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				var subForm = form.FindSingle<GuaranteeTransactionFilterControl>("GuaranteeTransactionFilterControl");
				var filterBusiness = subForm.FilterBusinessObject;
				CombineAssertions(() =>
				{
					AssertNotNull("The filter object for shared exist", filterBusiness);
					AssertType<GuaranteeTransactionFilterStripBusinessObject>(filterBusiness);
				});
			}
		}

		public void TestGetGuaranteeTransactionFilterStripBusinessObjectByParameter()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader, new GuaranteeTransactionFilterStripBusinessObjectForTest()))
			{
				form.Show();
				var subForm = form.FindSingle<GuaranteeTransactionFilterControl>("GuaranteeTransactionFilterControl");
				var filterBusiness = subForm.FilterBusinessObject;
				CombineAssertions(() =>
				{
					AssertNotNull("The filter object for shared exist", filterBusiness);
					AssertType<GuaranteeTransactionFilterStripBusinessObjectForTest>(filterBusiness);
				});
			}
		}

		public void TestzPanel1_Anchor()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				AssertEquals(AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, form.FindSingle<ZPanel>("zPanel1").Anchor);
			}
		}

		public void TestzPanel3_Anchor()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				AssertEquals(AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, form.FindSingle<ZPanel>("zPanel3").Anchor);
			}
		}

		public void TestzPanel4_Anchor()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				AssertEquals(AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, form.FindSingle<ZPanel>("zPanel4").Anchor);
			}
		}

		public void TestGridColumnsReadOnlyStatus()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				form.SetNewTransactionLineValues(100m, PermitTransactionTypeList.Codes.OBL, "XXXX", "YYYY");
				form.FindSingle<ZButton>("NewTransactionAddButton").PerformClick();
				var transactionLinesGrid = GetGuaranteeTransactionsGrid(form);
				var transactionDateColumnReadOnly = transactionLinesGrid.GetColumnStyle(BaseCusGuaranteeLineTransaction.Schema.CPL_TransactionDate).IsReadOnly;
				AssertEquals($"{BaseCusGuaranteeLineTransaction.Schema.CPL_TransactionDate} grid column should be read-only", true, transactionDateColumnReadOnly);
				var transactionTypeColumnReadOnly = transactionLinesGrid.GetColumnStyle(BaseCusGuaranteeLineTransaction.Schema.CPL_TransactionType).IsReadOnly;
				AssertEquals($"{BaseCusGuaranteeLineTransaction.Schema.CPL_TransactionType} grid column should be read-only", true, transactionTypeColumnReadOnly);
				var transactionTypeDescriptionColumnReadOnly = transactionLinesGrid.GetColumnStyle("TransactionTypeDescription").IsReadOnly;
				AssertEquals("TypeDescription grid column should be read-only", true, transactionTypeDescriptionColumnReadOnly);
				var transactionRefrenceColumnReadOnly = transactionLinesGrid.GetColumnStyle(BaseCusGuaranteeLineTransaction.Schema.CPL_Reference).IsReadOnly;
				AssertEquals($"{BaseCusGuaranteeLineTransaction.Schema.CPL_Reference} grid column should be read-only", true, transactionRefrenceColumnReadOnly);
				var transactionValueColumnReadOnly = transactionLinesGrid.GetColumnStyle(BaseCusGuaranteeLineTransaction.Schema.CPL_TranValue).IsReadOnly;
				AssertEquals($"{BaseCusGuaranteeLineTransaction.Schema.CPL_TranValue} grid column should be read-only", true, transactionValueColumnReadOnly);
				var transactionCommentColumnReadOnly = transactionLinesGrid.GetColumnStyle(BaseCusGuaranteeLineTransaction.Schema.CPL_Comment).IsReadOnly;
				AssertEquals($"{BaseCusGuaranteeLineTransaction.Schema.CPL_Comment} grid column should be read-only", true, transactionCommentColumnReadOnly);
				var transactionAppIDColumnReadOnly = transactionLinesGrid.GetColumnStyle(BaseCusGuaranteeLineTransaction.Schema.CPL_AppId).IsReadOnly;
				AssertEquals($"{BaseCusGuaranteeLineTransaction.Schema.CPL_AppId} grid column should be read-only", true, transactionAppIDColumnReadOnly);
				var transactionStatusColumnReadOnly = transactionLinesGrid.GetColumnStyle(BaseCusGuaranteeLineTransaction.Schema.CPL_TransactionStatus).IsReadOnly;
				AssertEquals($"{BaseCusGuaranteeLineTransaction.Schema.CPL_TransactionStatus} grid column should be read-only", true, transactionStatusColumnReadOnly);
			}
		}

		public void TestNewOBLTransactionMustBePositive()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();

				form.SetNewTransactionLineValues(0m, PermitTransactionTypeList.Codes.OBL, "", "");
				ClickNewTransactionAddButton(form);
				AssertHasErrorContaining(form.GetNewTransactionLineNPBO().CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);

				form.SetNewTransactionLineValues(-50m, PermitTransactionTypeList.Codes.OBL, "", "");
				ClickNewTransactionAddButton(form);
				AssertHasErrorContaining(form.GetNewTransactionLineNPBO().CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);

				form.SetNewTransactionLineValues(100m, PermitTransactionTypeList.Codes.OBL, "", "");
				ClickNewTransactionAddButton(form);
				AssertNoErrorContaining(form.GetNewTransactionLineNPBO().CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);
			}
		}

		public void TestNewADJTransactionCantBeZero()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();

				form.SetNewTransactionLineValues(0m, PermitTransactionTypeList.Codes.ADJ, "XXX", "YYY");
				ClickNewTransactionAddButton(form);
				AssertHasErrorContaining(form.GetNewTransactionLineNPBO().CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.AdjustmentTransactionValueCannotBeZero);

				form.SetNewTransactionLineValues(50m, PermitTransactionTypeList.Codes.ADJ, "XXX", "YYY");
				ClickNewTransactionAddButton(form);
				AssertNoErrorContaining(form.GetNewTransactionLineNPBO().CPL_TranValueInfo, CusGuaranteeLineTransactionValidation.AdjustmentTransactionValueCannotBeZero);
			}
		}

		public void TestNoMultipleOBLAllowed()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();

				form.SetNewTransactionLineValues(100m, PermitTransactionTypeList.Codes.OBL, "", "AAAA");
				ClickNewTransactionAddButton(form);
				AssertNoErrorContaining(form.GetNewTransactionLineNPBO().CPL_TransactionTypeInfo, CusGuaranteeLineTransactionValidation.OnlyOneOBLAllowed);

				form.SetNewTransactionLineValues(200m, PermitTransactionTypeList.Codes.OBL, "", "BBBB");
				ClickNewTransactionAddButton(form);
				AssertHasErrorContaining(form.GetNewTransactionLineNPBO().CPL_TransactionTypeInfo, CusGuaranteeLineTransactionValidation.OnlyOneOBLAllowed);
			}
		}

		public void TestNewTransactionShowsInGrid()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				form.SetNewTransactionLineValues(100m, PermitTransactionTypeList.Codes.OBL, "XXXX", "YYYY");
				ClickNewTransactionAddButton(form);
				var transactionLines = form.GetFormGuaranteeHeader().CusGuaranteeLineTransactions;
				AssertEquals("CusGuaranteeLineTransactions business object should feature the transaction that was just added", 1, transactionLines.Count);
				AssertEquals(100m, transactionLines[0].CPL_TranValue);
				AssertEquals(PermitTransactionTypeList.Codes.OBL, transactionLines[0].CPL_TransactionType);
				AssertEquals("XXXX", transactionLines[0].CPL_Comment);
				AssertEquals("YYYY", transactionLines[0].CPL_Reference);
				var transactionLinesGrid = GetGuaranteeTransactionsGrid(form);
				AssertEquals("Transaction grid should show the transaction that was just added", 1, transactionLinesGrid.List.Count);
			}
		}

		public void TestGridAutomaticPopulation()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				var subForm = form.FindSingle<GuaranteeTransactionFilterControl>("GuaranteeTransactionFilterControl");
				AssertEquals("The transaction grid should populate automatically. Check the GuaranteeTransactionFilterControl.ShouldRunSearchOnStripsInitialized property.", true, subForm.ShouldRunSearchOnStripsInitialized);
			}
		}

		public void TestNewTransactionGroupBoxFields()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				var newTransactionGroupBox = form.FindSingle<ZGroupBox>("NewTransactionGroupBox");
				AssertNotNull("The form should be showing a New Transaction Group Box", newTransactionGroupBox);
				AssertEquals("NewTransactionAmountZCalcEdit TabIndex", 21, newTransactionGroupBox.FindSingle<ZCalcEdit>("NewTransactionAmountZCalcEdit").TabIndex);
				AssertEquals("NewTransactionDateZDateEdit TabIndex", 22, newTransactionGroupBox.FindSingle<ZDateEdit>("NewTransactionDateZDateEdit").TabIndex);
				AssertEquals("NewTransactionTypeZDropEdit TabIndex", 20, newTransactionGroupBox.FindSingle<ZDropEdit>("NewTransactionTypeZDropEdit").TabIndex);
				AssertEquals("NewTransactionCommentZTextBox TabIndex", 23, newTransactionGroupBox.FindSingle<ZTextBox>("NewTransactionCommentZTextBox").TabIndex);
				AssertEquals("NewTransactionReferenceZTextBox TabIndex", 24, newTransactionGroupBox.FindSingle<ZTextBox>("NewTransactionReferenceZTextBox").TabIndex);
				AssertEquals("NewTransactionAddButton TabIndex", 25, newTransactionGroupBox.FindSingle<ZButton>("NewTransactionAddButton").TabIndex);
			}
		}

		public void TestNoGridCustomContextMenuAnymore()
		{
			int customContextMenuItemsCount = 0;
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				var filteredGrid = GetGuaranteeTransactionsGrid(form);
				foreach (MenuItem item in filteredGrid.ContextMenu.MenuItems)
				{
					if ("Add Opening Balance".Equals(item.Text))
					{
						customContextMenuItemsCount += 1;
					}
					if ("Add Manual Adjustment".Equals(item.Text))
					{
						customContextMenuItemsCount += 1;
					}
				}
				AssertEquals("Transaction grid should not show any custom context menu", 0, customContextMenuItemsCount);
			}
		}

		public void TestGridType()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				var filteredGrid = GetGuaranteeTransactionsGrid(form);
				AssertNotNull("The filtered grid should be a GuaranteeTransactionLinesGrid which is a ZDisplayGrid + internal notifications and standard tabbing properties", filteredGrid);
			}
		}

		public void TestPercentValueCustomLabelText()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			var guaranteeLineTransactions = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransactions.CPL_Reference = "REF";
			guaranteeLineTransactions.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			guaranteeLineTransactions.CPL_TranValue = 300000;
			var guaranteeLineTransactions2 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransactions2.CPL_Reference = "REF";
			guaranteeLineTransactions2.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			guaranteeLineTransactions2.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			guaranteeLineTransactions2.CPL_TranValue = 420;
			guaranteeHeader.CPH_Balance = 5;
			Factory.Save();

			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();

				var percentValueCustomLabel = form.Controls.Find("PercentValueCustomLabel", true).Single();
				AssertEquals("Percentage is 99.858333 but is must be truncated to 99.85", "99.85 %", percentValueCustomLabel.Text);
			}
		}

		public void TestProgressBar()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			var guaranteeLineTransactions = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransactions.CPL_Reference = "REF";
			guaranteeLineTransactions.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			guaranteeLineTransactions.CPL_TranValue = 100;
			var guaranteeLineTransactions2 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransactions2.CPL_Reference = "REF";
			guaranteeLineTransactions2.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			guaranteeLineTransactions2.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			guaranteeLineTransactions2.CPL_TranValue = 5;
			guaranteeHeader.CPH_Balance = 5;
			Factory.Save();
			AssertEquals(10m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPending.Amount.Round(0));
			AssertProgressBar(guaranteeHeader, 90, Color.Red);

			var guaranteeLineTransactions3 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransactions3.CPL_Reference = "REF";
			guaranteeLineTransactions3.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			guaranteeLineTransactions3.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			guaranteeLineTransactions3.CPL_TranValue = 35;
			Factory.Save();
			AssertEquals(45m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPending.Amount.Round(0));
			AssertProgressBar(guaranteeHeader, 55, Color.Orange);

			var guaranteeLineTransactions4 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransactions4.CPL_Reference = "REF";
			guaranteeLineTransactions4.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			guaranteeLineTransactions4.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			guaranteeLineTransactions4.CPL_TranValue = 45;
			Factory.Save();
			AssertEquals(90m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPending.Amount.Round(0));
			AssertProgressBar(guaranteeHeader, 10, Color.Green);
		}

		public void TestGuaranteeFormGroupsAlignment()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();
				var guaranteeDetailsGroupbox = form.FindSingle<ZGroupBox>("GuaranteeDetailsGroupBox");
				var guaranteeBalanceStatusGroupBox = form.FindSingle<ZGroupBox>("GuaranteBalanceStatusGroupBox");
				var guaranteeTransactionsGroupBox = form.FindSingle<ZGroupBox>("GuaranteeTransactionsGroupBox");
				var filteredGrid = GetGuaranteeTransactionsGrid(form);
				AssertEquals("Guarantee Details group box and Guarantee Balance group box should share same horizontal location", guaranteeBalanceStatusGroupBox.Location.X, guaranteeDetailsGroupbox.Location.X);
				AssertEquals("Guarantee Details group box and Guarantee Balance group box should share same horizontal location", guaranteeTransactionsGroupBox.Location.X, guaranteeDetailsGroupbox.Location.X);
				AssertEquals("Guarantee Balance group box and Guarantee Transaction group box should share same width.", guaranteeTransactionsGroupBox.Size.Width, guaranteeDetailsGroupbox.Size.Width);
				AssertEquals("Guarantee Transaction grid origin should be the same as Guarantee Transaction group box.", filteredGrid.Location.X, guaranteeTransactionsGroupBox.Location.X);
				AssertEquals("Guarantee Transaction grid should fit in Guarantee Transaction group box in width", filteredGrid.Size.Width + 6, guaranteeTransactionsGroupBox.Size.Width);
			}
		}

		public void TestGuaranteeDetailsNewTabBalanceRulesAccess()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var guaranteeHeader = GetGuaranteeHeader();
				using (var form = new GuaranteeForm(guaranteeHeader))
				{
					form.Show();

					var balanceRulesAccessTabControl = form.FindSingleOrDefault<ZTabControl>("BalanceRulesAccessTabControl");
					var balanceTabPage = form.FindSingleOrDefault<ZTabPage>("BalanceTabPage");
					var rulesTabPage = form.FindSingleOrDefault<ZTabPage>("RulesTabPage");
					var additionalReferencesTabPage = form.FindSingleOrDefault<ZTabPage>("AdditionalReferencesTabPage");
					var accessTabPage = form.FindSingle<ZTabPage>("AccessTabPage");
					AssertNotNull(accessTabPage);
					AssertNotNull(balanceRulesAccessTabControl);
					AssertNotNull(rulesTabPage);
					AssertNotNull(balanceTabPage);
					AssertNull(additionalReferencesTabPage);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var guaranteeHeader = GetGuaranteeHeader();
				using (var form = new GuaranteeForm(guaranteeHeader))
				{
					form.Show();
					var additionalReferencesTabPage = form.FindSingleOrDefault<ZTabPage>("AdditionalReferencesTabPage");
					AssertNull(additionalReferencesTabPage);
				}

				guaranteeHeader.CPH_Type = "COD";
				using (var form = new GuaranteeForm(guaranteeHeader))
				{
					form.Show();
					var additionalReferencesTabPage = form.FindSingleOrDefault<ZTabPage>("AdditionalReferencesTabPage");
					AssertNotNull(additionalReferencesTabPage);
					Assert(additionalReferencesTabPage.TabVisible);
				}
			}
		}

		public void TestCPH_BalanceAfterAddNewTransaction()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();

				form.SetNewTransactionLineValues(100m, PermitTransactionTypeList.Codes.ADJ, "", "AAAA");
				ClickNewTransactionAddButton(form);
				AssertEquals(0m, guaranteeHeader.CPH_Balance);

				form.SetNewTransactionLineValues(200m, PermitTransactionTypeList.Codes.OBL, "", "BBBB");
				ClickNewTransactionAddButton(form);
				AssertEquals(200m, guaranteeHeader.CPH_Balance);
			}
		}

		public void TestCPH_Calc_OpeningBalanceAfterAddNewTransaction()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();

				var tranValueInfo = form.GetNewTransactionLineNPBO().CPL_TranValueInfo;
				form.SetNewTransactionLineValues(200m, PermitTransactionTypeList.Codes.OBL, "", "BBBB");
				ClickNewTransactionAddButton(form);
				AssertEquals(200m, guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertNoErrors(tranValueInfo);

				form.SetNewTransactionLineValues(-300m, GuaranteeTransactionTypeList.Codes.OBA, "Some comment", "CCCC");
				ClickNewTransactionAddButton(form);
				AssertEquals(200m, guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertHasErrors("Remaining Balance mustn't be below zero after adjustment transaction is added.", tranValueInfo);

				form.SetNewTransactionLineValues(300m, GuaranteeTransactionTypeList.Codes.OBA, "Some comment", "CCCC");
				ClickNewTransactionAddButton(form);
				AssertEquals(500m, guaranteeHeader.CPH_Calc_OpeningBalance);
				AssertNoErrors(tranValueInfo);
			}
		}

		public void TestCPH_Calc_TotalBalanceIncludingPendingDecimal_ADJ()
		{
			var guaranteeHeader = GetGuaranteeHeader();
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				form.Show();

				form.SetNewTransactionLineValues(200m, PermitTransactionTypeList.Codes.OBL, "", "BBBB");
				ClickNewTransactionAddButton(form);
				form.SetNewTransactionLineValues(-1m, PermitTransactionTypeList.Codes.ADJ, "ADJ by -1", "AAAA");
				ClickNewTransactionAddButton(form);
				CombineAssertions(() =>
				{
					AssertEquals($"{nameof(guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal)}", 199m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal);
					AssertEquals("ADJ Transaction CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, guaranteeHeader.GetTransactions().Single(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.ADJ).CPL_TransactionStatus);
				});
			}
		}

		protected override Form GetFormToBashCore() => new GuaranteeForm(GetGuaranteeHeader());

		protected override bool AllowHasChangesOnFormOpen => true;

		static ZFilterGrid GetGuaranteeTransactionsGrid(GuaranteeForm form)
		{
			return form.FindSingle<GuaranteeTransactionFilterControl>("GuaranteeTransactionFilterControl").Grid;
		}

		BaseCusGuaranteeHeader GetGuaranteeHeader()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
			helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "@#$_Basher_Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = "ZZZ";
			guaranteeHeader.CPH_StartDate = ZDate.Today;
			Factory.Save();
			return guaranteeHeader;
		}

		static void ClickNewTransactionAddButton(GuaranteeForm form)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddYesAnswer();
			form.FindSingle<ZButton>("NewTransactionAddButton").PerformClick();
		}

		static void AssertProgressBar(BaseCusGuaranteeHeader guaranteeHeader, int percentage, Color color)
		{
			using (var form = new GuaranteeForm(guaranteeHeader))
			{
				AssertEquals("Value should be " + percentage, percentage, form.ProgressBar.Value);
				AssertEquals("Color should be " + color.Name, color, ((SolidBrush)form.ProgressBar.GetForeGroundColor()).Color);
			}
		}

		sealed class GuaranteeHeaderWithMessageSupportForTest : BaseCusGuaranteeHeader
		{
			public GuaranteeHeaderWithMessageSupportForTest(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			protected override bool SupportsMessagesCore => true;
		}

		sealed class GuaranteeTransactionFilterStripBusinessObjectForTest : GuaranteeTransactionFilterStripBusinessObject
		{
			public GuaranteeTransactionFilterStripBusinessObjectForTest()
				: base()
			{
			}
		}
	}
}
