using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(AmendmentReasonForm))]
	public class AmendmentReasonFormTest : ZFormBasherTest
	{
		public void TestFormText()
		{
			using (var testForm = (AmendmentReasonForm)GetFormToBash())
			{
				testForm.Show();
				AssertEquals("Please enter a reason", testForm.Text);
			}
		}

		public void TestOKButton_ClickWhenThereAreErrorsInBizO()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var testForm = (AmendmentReasonForm)GetFormToBash())
			{
				AmendmentReason.ReasonText = "";
				AssertEquals("PreCondition: Change reason should be there", true, AmendmentReason.ReasonTextInfo.HasErrors());
				testForm.OKButton_Click(testForm.OKButton, EventArgs.Empty);
				ZString warningMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Error should have shown", true, warningMessage.Contains("Please enter a reason for the amendment or withdrawal."));
				AssertEquals("No result, should not proceed.", DialogResult.None, testForm.DialogResult);
			}
		}

		public void TestOKButton_ClickWhenThereAreNoErrors()
		{
			using (var testForm = (AmendmentReasonForm)GetFormToBash())
			{
				AmendmentReason.ReasonText = "Blah";
				AssertEquals("No errrors expected at this point", false, AmendmentReason.HasErrors);
				testForm.OKButton_Click(testForm.OKButton, EventArgs.Empty);
				AssertEquals("IsOKToSendAMessage", false, AmendmentReason.IsCancelled);
				AssertEquals("If this fails, please amend MYCustomsCargoManifestPlugInToConsol.DeclareManifest. It expects DialogResult.OK to proceed and declare", DialogResult.OK, testForm.DialogResult);
			}
		}

		public void TestCancelButton_Click()
		{
			using (var testForm = (AmendmentReasonForm)GetFormToBash())
			{
				AmendmentReason.IsCancelled = false;
				testForm.CancelButton_Click(testForm.CancelButton, EventArgs.Empty);
				AssertEquals("IsOKToSendAMessage", true, AmendmentReason.IsCancelled);
				AssertEquals("Dialog Result is 'Cancelled'", DialogResult.Cancel, testForm.DialogResult);
			}
		}

		protected override Form GetFormToBashCore() => (Form)Activator.CreateInstance(FormToBashType, new object[] { AmendmentReason });

		protected virtual AmendmentWithdrawalReason GetNewAmendmentWithdrawalReason() => new AmendmentWithdrawalReason();

		AmendmentWithdrawalReason amendmentReason;
		protected AmendmentWithdrawalReason AmendmentReason => amendmentReason ?? (amendmentReason = GetNewAmendmentWithdrawalReason());
	}
}
