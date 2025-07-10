using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Certificates.Testing
{
	[TestedType(typeof(EnterCryptokiCertificatePinForm))]
	sealed class EnterCryptokiCertificatePinFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new EnterCryptokiCertificatePinForm(new UserEnterableTokenPin());

		public void TestFormHeading()
		{
			using (var form = new EnterCryptokiCertificatePinForm(new UserEnterableTokenPin()))
			{
				form.Show();
				AssertEquals("FormHeading", "Enter Certificate PIN", form.FormHeading);
			}
		}

		public void TestPinLabelCaption()
		{
			using (var form = new EnterCryptokiCertificatePinForm(new UserEnterableTokenPin()))
			{
				form.Show();
				AssertEquals("PinLabel Caption", "Please enter the PIN for your Digital Signature Certificate", form.PinLabel.CaptionResourceString.Caption);
			}
		}

		public void TestPinTextBox()
		{
			using (var form = new EnterCryptokiCertificatePinForm(new UserEnterableTokenPin()))
			{
				form.Show();
				AssertEquals("PinTextBox BindTo", "Pin", form.PinTextBox.BindTo);
				AssertEquals("PinTextBox CharacterCasing", CharacterCasing.Normal, form.PinTextBox.CharacterCasing);
				AssertEquals("PinTextBox UseSystemPasswordChar", true, form.PinTextBox.UseSystemPasswordChar);
			}
		}

		public void TestClickOkButtonWhenBusinessEntityHasErrors()
		{
			using (var form = new EnterCryptokiCertificatePinForm(new UserEnterableTokenPin()))
			{
				form.Show();
				form.OkButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Is form visible?", true, form.Visible);
					AssertEquals("Form DialogResult", DialogResult.None, form.DialogResult);
					AssertEquals("Last message prompted to the user", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestClickOkButtonWhenBusinessEntityDoesNotHaveErrors()
		{
			var userEnterableTokenPin = new UserEnterableTokenPin();
			using (var form = new EnterCryptokiCertificatePinForm(userEnterableTokenPin))
			{
				form.Show();

				userEnterableTokenPin.Pin = "123456";
				form.OkButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Is form visible?", false, form.Visible);
					AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
				});
			}
		}

		public void TestClickAbortButton()
		{
			using (var form = new EnterCryptokiCertificatePinForm(new UserEnterableTokenPin()))
			{
				form.Show();
				form.AbortButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Is form visible?", false, form.Visible);
					AssertEquals("Form DialogResult", DialogResult.Cancel, form.DialogResult);
				});
			}
		}
	}
}
