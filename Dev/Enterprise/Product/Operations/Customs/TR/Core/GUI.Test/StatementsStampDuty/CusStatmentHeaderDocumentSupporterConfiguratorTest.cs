using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	public class CusStatmentHeaderDocumentSupporterConfiguratorTest : TestCaseWithFactory
	{
		public void TestPrintStampDutyLedgerConfig()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var documentSupporter = cusStatementHeader.DocumentSupporter;
			var menuItemName = Factory.New<IStmMenuItem>();
			menuItemName.SU_MenuName = "Print Stamp Duty Ledger";

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ShowDialogsInTest = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemName);

			CombineAssertions(() =>
			{
				AssertEquals("Do you want to print The Stamp Duty Ledger?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Print process for Stamp Duty Ledger is Canceled", documentSupporterDataState.ErrorMessage);
				AssertEquals("IsValid", false, documentSupporterDataState.IsValid);
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ShowDialogsInTest = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemName);

			CombineAssertions(() =>
			{
				AssertEquals("Do you want to print The Stamp Duty Ledger?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
				AssertEquals(typeof(StampDutyLedgerDocumentEntryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ShowDialogsInTest = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			cusStatementHeader.B2_PrintDate = ZDateTime.Today;
			documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemName);

			CombineAssertions(() =>
			{
				AssertEquals($"Do you want to print The Stamp Duty Ledger? It was printed on {cusStatementHeader.B2_PrintDate.ToShortDateString()}", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
				AssertEquals(typeof(StampDutyLedgerDocumentEntryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			});
		}

		public void TestDialogForEmptyPrintDate()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var documentSupporter = cusStatementHeader.DocumentSupporter;
			var menuItemName = Factory.New<IStmMenuItem>();
			menuItemName.SU_MenuName = "Print Stamp Duty Ledger";

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ShowDialogsInTest = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var stampDutyLedgerDocumentEntryForm = dialog as ZChildForm;
				if (stampDutyLedgerDocumentEntryForm != null)
				{
					stampDutyLedgerDocumentEntryForm.Shown += (s, e) =>
					{
						var okButton = stampDutyLedgerDocumentEntryForm.FindAll<ZButton>(b => b.Name == "btnOk").Single();
						okButton.PerformClick();
					};
				}
			});

			var documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemName);

			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.Today, cusStatementHeader.B2_PrintDate);
				AssertEquals("Do you want to print The Stamp Duty Ledger?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
				AssertEquals(typeof(StampDutyLedgerDocumentEntryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			});
		}

		public void TestDialogForNotEmptyPrintDate()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var documentSupporter = cusStatementHeader.DocumentSupporter;
			var menuItemName = Factory.New<IStmMenuItem>();
			menuItemName.SU_MenuName = "Print Stamp Duty Ledger";

			var testDate = ZDateTime.Today.AddDays(1);
			var testDate1 = ZDateTime.Today.AddDays(2);
			cusStatementHeader.B2_PrintDate = testDate;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ShowDialogsInTest = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var stampDutyLedgerDocumentEntryForm = dialog as ZChildForm;
				if (stampDutyLedgerDocumentEntryForm != null)
				{
					stampDutyLedgerDocumentEntryForm.Shown += (s, e) =>
					{
						var form = dialog as StampDutyLedgerDocumentEntryForm;
						form.PrintDateDateEdit.DateTimeValue = testDate1;
						var okButton = stampDutyLedgerDocumentEntryForm.FindAll<ZButton>(b => b.Name == "btnOk").Single();
						okButton.PerformClick();
					};
				}
			});

			var documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemName);

			CombineAssertions(() =>
			{
				AssertEquals($"Do you want to print The Stamp Duty Ledger? It was printed on {testDate.ToShortDateString()}", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(testDate1, cusStatementHeader.B2_PrintDate);
				AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
				AssertEquals(typeof(StampDutyLedgerDocumentEntryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			});
		}

		public void TestDialogForPrintCancel()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var documentSupporter = cusStatementHeader.DocumentSupporter;
			var menuItemName = Factory.New<IStmMenuItem>();
			menuItemName.SU_MenuName = "Print Stamp Duty Ledger";

			var testDate = ZDateTime.Today.AddDays(1);
			var testDate1 = ZDateTime.Today.AddDays(2);
			cusStatementHeader.B2_PrintDate = testDate;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ShowDialogsInTest = false;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var stampDutyLedgerDocumentEntryForm = dialog as ZChildForm;
				if (stampDutyLedgerDocumentEntryForm != null)
				{
					stampDutyLedgerDocumentEntryForm.Shown += (s, e) =>
					{
						var form = dialog as StampDutyLedgerDocumentEntryForm;
						form.PrintDateDateEdit.DateTimeValue = testDate1;
						var cancelButton = stampDutyLedgerDocumentEntryForm.FindAll<ZButton>(b => b.Name == "btnClose").Single();
						cancelButton.PerformClick();
					};
				}
			});

			var documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemName);

			CombineAssertions(() =>
			{
				AssertEquals("IsValid", false, documentSupporterDataState.IsValid);
				AssertEquals("ErrorMessage", "Print process for Stamp Duty Ledger is Canceled", documentSupporterDataState.ErrorMessage);
				AssertEquals("Print Date", testDate, cusStatementHeader.B2_PrintDate);
				AssertEquals(typeof(StampDutyLedgerDocumentEntryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			});
		}
	}
}
