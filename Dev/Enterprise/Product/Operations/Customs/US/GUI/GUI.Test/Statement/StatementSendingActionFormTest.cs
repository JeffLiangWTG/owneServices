using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(StatementSendingActionForm))]
	sealed class StatementSendingActionFormTest : ZFormBasherTest
	{
		public void TestClickSendButton()
		{
			using (StatementSendingActionForm form = CreateNewForm())
			{
				form.actions[0].US_SendMessage = false;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				AssertEquals(StatementSendingActionForm.YouHaveNotSelectedAnythingToSendMessagesFor, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsCancelled", false, form.actions.SendMessage);
				form.CancelButton_Click(form.CancelButton, EventArgs.Empty);
				AssertEquals("IsCancelled", false, form.actions.SendMessage);
			}
		}

		public void TestMessageErrorWithNoSecurityRight()
		{
			using (StatementSendingActionForm form = CreateNewForm())
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.actions[0].US_SendMessage = true;
				form.actions[0].US_PaymentType = "";
				Assert("PreCondition", form.actions.HasMessageErrors());
				Env.Security.USCustomsImportStatementSendWithMessageErrors.IsAllowed = false;
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				AssertContains(Env.Security.USCustomsImportStatementSendWithMessageErrors.DisplayTextPathToSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(form.actions.IsCancelled);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Env.Security.USCustomsImportStatementSendWithMessageErrors.IsAllowed = true;
				form.OKButton_Click(form.OKButton, EventArgs.Empty);
				Assert(!form.actions.IsCancelled);
			}
		}

		public void TestCreateFormForReconciliation()
		{
			StatementDeleteAndSendingActionCollection actions = new StatementDeleteAndSendingActionCollection(ReconDeclaration);
			using (StatementSendingActionForm form = new StatementSendingActionForm(actions))
			{
				ZGridColumnInfo column = form.EntriesGrid.GetColumnStyle(StatementDeleteAndSendingAction.Schema.US_PeriodicStatementMonth);
				AssertEquals("Periodic Statement Month column is unavailable", true, column.IsUnavailable);
			}
		}

		protected override Form GetFormToBashCore() => new StatementSendingActionForm(new StatementDeleteAndSendingActionCollection(StatementHeader));

		protected override IEnumerable<Form> FormsToBash
		{
			get
			{
				yield return GetFormToBash();
				yield return CreateNewForm();
			}
		}

		StatementSendingActionForm CreateNewForm()
		{
			StatementHeader.StatementLines.AddNew();
			StatementHeader.StatementLines[0].B3_EntryNum = "000044";
			StatementHeader.StatementLines[0].B3_Status = StatementLineStatusList.Codes.Active;
			StatementHeader.StatementLines.AddNew();
			StatementHeader.StatementLines[1].B3_EntryNum = "000055";
			StatementHeader.StatementLines[1].B3_Status = StatementLineStatusList.Codes.Active;
			StatementDeleteAndSendingActionCollection collection = new StatementDeleteAndSendingActionCollection(StatementHeader);
			return new StatementSendingActionForm(collection);
		}

		CusStatementHeader statementHeader;
		CusStatementHeader StatementHeader => statementHeader ?? (statementHeader = Factory.New<CusStatementHeader>());

		ReconDeclaration reconDeclaration;
		ReconDeclaration ReconDeclaration
		{
			get
			{
				if (reconDeclaration == null)
				{
					Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("XJ5");
					reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
					reconDeclaration.OriginalEntries.AddNew();
					var invoice = reconDeclaration.Invoices.AddNew();
					invoice.US_CH_ReconEntry = reconDeclaration.OriginalEntries[0].CH_PK;
					reconDeclaration.InvoiceLines.AddNew();
					Factory.Save();
				}

				return reconDeclaration;
			}
		}
	}
}
