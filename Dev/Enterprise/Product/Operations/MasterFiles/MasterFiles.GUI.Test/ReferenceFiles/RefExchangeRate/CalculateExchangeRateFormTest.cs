using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CalculateExchangeRateForm))]
	sealed class CalculateExchangeRateFormTest : ZFormBasherTest
	{
		public void TestFormVerb()
		{
			using var form = (CalculateExchangeRateForm)GetFormToBashCore();
			AssertEquals(string.Empty, form.FormVerb);
		}

		public void TestButtonOK_Click()
		{
			var exRate = Factory.New<RefExchangeRate>();
			var calculator = new RefExchangeRateCalculator(exRate);
			ZFormModaliser.ShowDialogsInTest = true;
			using var form = new CalculateExchangeRateForm(calculator);
			var formClosedFlag = false;
			form.FormClosed += (sender, e) => formClosedFlag = true;
			form.Show();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			form.ButtonOK.PerformClick();
			Application.DoEvents();

			calculator.BaseCurrencyValue = 0m;
			CombineAssertions(() =>
			{
				AssertEquals("Error message", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Form not closed", !formClosedFlag);
				AssertNotEquals("DialogResult OK", DialogResult.OK, form.DialogResult);
			});

			UnitTestUserNotification.Instance.ClearMessages();
			calculator.BaseCurrencyValue = 1m;
			calculator.QuoteCurrencyValue = 1m;

			form.ButtonOK.PerformClick();
			Application.DoEvents();

			CombineAssertions(() =>
			{
				AssertNull("No errors", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DialogResult OK", DialogResult.OK, form.DialogResult);
				Assert("Form closed", formClosedFlag);
			});
		}

		protected override Form GetFormToBashCore()
		{
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_StartDate = ZDateTime.BrettsBirthday;
			exRate.RE_ExpiryDate = ZDateTime.BrettsBirthday;
			exRate.HasChanges = false;
			var calculator = new RefExchangeRateCalculator(exRate);
			calculator.HasChanges = false;
			var form = new CalculateExchangeRateForm(calculator);
			MissingResourceStringChecker.ExcludeFromTest(form.QuoteCurrencyLabel);
			MissingResourceStringChecker.ExcludeFromTest(form.QuoteCurrencyValueCalcEdit);
			MissingResourceStringChecker.ExcludeFromTest(form.BaseCurrencyLabel);
			MissingResourceStringChecker.ExcludeFromTest(form.BaseCurrencyValueCalcEdit);
			return form;
		}
	}
}
