using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(StatementsStampDutyForm))]
	public class StatementsStampDutyFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var statement = Factory.New<CusStatementHeader>();
			return new StatementsStampDutyForm(statement) { ControllerID = ControllerIDs.Customs.CustomsStatement };
		}

		public void TestFormCaption()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = "M";

			using (StatementsStampDutyForm form = new StatementsStampDutyForm(statement))
			{
				AssertEquals("Statements / Stamp Duty", form.FormCaption);
			}
		}

		public void TestDocDataPlugIn()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = "M";

			using (var form = new StatementsStampDutyForm(statement))
			{
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);
				AssertNotNull(plugIn);
			}
		}

		public void TestExportRADeclarationFile_Click()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (var form = new StatementsStampDutyForm(statement))
			{
				form.Show();

				AssertNotNull(form.FindMenuItem_ForTest("Export R.A. Declaration File (TXT)"));

				var exportRADeclarationFileMenu = form.FindMenuItem_ForTest("Export R.A. Declaration File (TXT)");
				exportRADeclarationFileMenu.PerformClick();

				AssertEquals("Do you want to create Revenue Administration Stamp Duty Declaration TXT file?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestExportRADeclarationFile_ForYes()
		{
			using (var tempFile = TempFile.New(Env.TempPath, "txt"))
			{
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
				var fileInfo = new FileInfo(tempFile.Filename);

				var statementHeader = Factory.New<CusStatementHeader>();
				var statementLine1 = statementHeader.StatementLines.AddNew();

				statementLine1.B3_BrokerReference = "21-04315";
				statementLine1.B3_EntryDate = (ZDate)new ZDateTime(2022, 08, 01, 00, 00, 00);
				statementLine1.B3_AssociatedEntry = "231";

				var statementLineCharge1 = statementLine1.Charges.AddNew();

				statementLineCharge1.B4_ChargeType = "SBS";
				statementLineCharge1.B4_ReferenceNumber = "ULU000001886";
				statementLineCharge1.B4_ChargeAmount = 50;

				var statementLineCharge2 = statementLine1.Charges.AddNew();

				statementLineCharge2.B4_ChargeType = "ABS";
				statementLineCharge2.B4_ReferenceNumber = "ULU000001786";
				statementLineCharge2.B4_ChargeAmount = 50;

				var statementLine2 = statementHeader.StatementLines.AddNew();

				statementLine2.B3_BrokerReference = "21-04316";
				statementLine2.B3_EntryDate = (ZDate)new ZDateTime(2022, 08, 01, 00, 00, 00);
				statementLine2.B3_AssociatedEntry = "232";

				var statementLineCharge3 = statementLine2.Charges.AddNew();

				statementLineCharge3.B4_ChargeType = "";
				statementLineCharge3.B4_ReferenceNumber = "ULU000001890";
				statementLineCharge3.B4_ChargeAmount = 25;

				var statementLineCharge4 = statementLine2.Charges.AddNew();

				statementLineCharge4.B4_ChargeType = "";
				statementLineCharge4.B4_ReferenceNumber = "ULU000001670";
				statementLineCharge4.B4_ChargeAmount = 25;

				using (var form = new StatementsStampDutyForm(statementHeader))
				{
					form.Show();

					AssertNotNull(form.FindMenuItem_ForTest("Export R.A. Declaration File (TXT)"));

					var exportRADeclarationFileMenu = form.FindMenuItem_ForTest("Export R.A. Declaration File (TXT)");

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					exportRADeclarationFileMenu.PerformClick();

					var expectedMessage = $"Saved it successfully to '{tempFile.Filename}'";

					CombineAssertions("Test for the last message and the existance of the temp file", () =>
					{
						AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						Assert("Export File should Exist", fileInfo.Exists);
					});

					var tempData = File.ReadAllText(tempFile.Filename);

					var messageText = Enterprise.Customs.TR.Business.Testing.NumberFountainHelperTest.GetTxtFile("StampDuty.RevenueAdministration.txt");

					AssertEquals("File Content", tempData, messageText);

					fileInfo.Delete();
				}
			}
		}

		public void TestExportRADeclarationFile_ForNo()
		{
			using (var tempFile = TempFile.New(Env.TempPath, "txt"))
			{
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
				var fileInfo = new FileInfo(tempFile.Filename);

				var statement = Factory.New<CusStatementHeader>();

				using (var form = new StatementsStampDutyForm(statement))
				{
					form.Show();

					AssertNotNull(form.FindMenuItem_ForTest("Export R.A. Declaration File (TXT)"));

					var exportRADeclarationFileMenu = form.FindMenuItem_ForTest("Export R.A. Declaration File (TXT)");

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					exportRADeclarationFileMenu.PerformClick();

					var expectedMessage = $"Saved it successfully to '{tempFile.Filename}'";
					AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestExportRADeclarationFile_ForCancel()
		{
			using (var tempFile = TempFile.New(Env.TempPath, "txt"))
			{
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
				var fileInfo = new FileInfo(tempFile.Filename);

				var statement = Factory.New<CusStatementHeader>();

				using (var form = new StatementsStampDutyForm(statement))
				{
					form.Show();

					AssertNotNull(form.FindMenuItem_ForTest("Export R.A. Declaration File (TXT)"));

					var exportRADeclarationFileMenu = form.FindMenuItem_ForTest("Export R.A. Declaration File (TXT)");

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

					exportRADeclarationFileMenu.PerformClick();

					var expectedMessage = $"Saved it successfully to '{tempFile.Filename}'";
					AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestControlVisible()
		{
			var statement = Factory.New<CusStatementHeader>();

			using (var form = new StatementsStampDutyForm(statement))
			{
				form.Show();

				var statementNumberZTextBox = form.Controls.Find("StatementNumberZTextBox", true).First();
				var paymentTypeZDropEdit = form.Controls.Find("PaymentTypeZDropEdit", true).First();
				var printDateDateEdit = form.Controls.Find("PrintDateZDateEdit", true).First();
				var statementStatusDropEdit = form.Controls.Find("StatementStatusZDropEdit", true).First();
				var paymentStatusDropEdit = form.Controls.Find("PaymentStatusZDropEdit", true).First();
				var paymentPartyDropEdit = form.Controls.Find("PaymentPartyZDropEdit", true).First();
				var statementAmountCalcEdit = form.Controls.Find("TotalChargeAmountZCalcEdit", true).First();

				CombineAssertions("Statements Stamp Duty Header Fields Visible", () =>
				{
					AssertEquals("StatementNumberZTextBox", true, statementNumberZTextBox.Visible);
					AssertEquals("PaymentTypeZDropEdit", true, paymentTypeZDropEdit.Visible);
					AssertEquals("PrintDateZDateEdit", true, printDateDateEdit.Visible);
					AssertEquals("StatementStatusZDropEdit", true, statementStatusDropEdit.Visible);
					AssertEquals("PaymentStatusZDropEdit", true, paymentStatusDropEdit.Visible);
					AssertEquals("PaymentPartyZDropEdit", true, paymentPartyDropEdit.Visible);
					AssertEquals("TotalChargeAmountZCalcEdit", true, statementAmountCalcEdit.Visible);
				});
			}
		}

		public void TestDeactivateButtonAndMenu()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (var form = new StatementsStampDutyForm(statement))
			{
				form.Show();

				AssertNotNull(form.FindMenuItem_ForTest("Make Inactive"));

				var activeMenu = form.FindMenuItem_ForTest("Make Inactive");
				activeMenu.PerformClick();

				AssertNotEquals("You cannot deactivate a statement with the entries.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var statement2 = Factory.New<CusStatementHeader>();
			var statementLine2 = statement2.StatementLines.AddNew();
			using (var form2 = new StatementsStampDutyForm(statement2))
			{
				form2.Show();

				AssertNotNull(form2.FindMenuItem_ForTest("Make Inactive"));

				var activeMenu2 = form2.FindMenuItem_ForTest("Make Inactive");
				activeMenu2.PerformClick();

				AssertEquals("You cannot deactivate a statement with the entries.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
