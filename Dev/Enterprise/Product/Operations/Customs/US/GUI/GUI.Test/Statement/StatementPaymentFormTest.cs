using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(StatementPaymentForm))]
	sealed class StatementPaymentFormBasherTest : ZFormBasherTest
	{
		public void TestIsCancelled_FromCancelButton()
		{
			using (RegistrySetup regSetupper = new RegistrySetup(TargetInRegistry.AllowMessageErrors))
			{
				Action.PayerUnitNo = "123456";
				Form.Show();
				Form.CancelButton_Click(Form.CancelButton, EventArgs.Empty);
				AssertEquals(true, Form.IsCancelled);
			}
		}

		public void TestIsCancelled_FromOKButton()
		{
			using (RegistrySetup regSetupper = new RegistrySetup(TargetInRegistry.AllowMessageErrors))
			{
				Action.PayerUnitNo = "123456";
				Form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Form.OKButton_Click(Form.OKButton, EventArgs.Empty);
				AssertEquals(false, Form.IsCancelled);
			}
		}

		public void TestIsCancelled_FromOKButtonWhenMessageError()
		{
			using (RegistrySetup regSetupper = new RegistrySetup(TargetInRegistry.AllowMessageErrors))
			{
				Action.PayerUnitNo = "123456";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.US_EntryFilerCode = "XJ5";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = "ENS";
				entry.EntryNumber = "1";
				Factory.Save();
				declaration.ReleaseStatus = "CAN";
				var statementLine = Action.StatementHeader.StatementLines.AddNew();
				statementLine.B3_EntryFilerCode = "XJ5";
				statementLine.B3_EntryNum = "1";
				Form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Form.OKButton_Click(Form.OKButton, EventArgs.Empty);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Release Status: This entry has not been released yet"));
				AssertEquals(true, Form.IsCancelled);
			}
		}

		public void TestShowMessageErrors_WhenUserChoosesNo()
		{
			using (StatementPaymentForm form = new StatementPaymentForm(Action))
			{
				form.Show();
				Env.Security.USCustomsImportStatementSendWithMessageErrors.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.OKButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Payer Unit No: Please enter the payer's unit number"));
				AssertEquals(true, form.IsCancelled);
			}

			using (StatementPaymentForm form = new StatementPaymentForm(Action))
			{
				form.Show();
				Env.Security.USCustomsImportStatementSendWithMessageErrors.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.OKButton.PerformClick();
				AssertContains(Env.Security.USCustomsImportStatementSendWithMessageErrors.DisplayTextPathToSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowMessageErrors_WhenUserChoosesYes()
		{
			Form.Show();
			Env.Security.USCustomsImportStatementSendWithMessageErrors.IsAllowed = true;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			form.OKButton.PerformClick();
			Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Payer Unit No: Please enter the payer's unit number"));
			AssertEquals(false, form.IsCancelled);
		}

		protected override Form GetFormToBashCore() => new StatementPaymentForm(Action);

		protected override void TearDown()
		{
			base.TearDown();
			form?.Dispose();
		}

		StatementPaymentAction action;
		StatementPaymentAction Action
		{
			get
			{
				if (action == null)
				{
					CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
					statementHeader.B2_StatementNumber = "1234";
					action = new StatementPaymentAction(statementHeader);
				}

				return action;
			}
		}

		StatementPaymentForm form;
		StatementPaymentForm Form => form ?? (form = (StatementPaymentForm)GetFormToBashCore());
	}
}
