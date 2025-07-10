using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbCompany_HungaryCredentialUserControl))]
	sealed class GlbCompany_HungaryCredentialUserControlTest : BasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Hungary;

		public override Form GetFormToBash()
			=> CreateFormForTest(GetDatabindingObject());

		public void TestFieldPasswordStyle_WhenBlankAndUnsavedBizo()
		{
			var company = GetDatabindingObject();
			using (var form = CreateFormForTest(company))
			{
				form.Show();
				var (loginTextbox, passwordHashTextbox, signatureKeyTextbox, replacementKeyTextbox) = AssertTextboxesNotNull(form);

				AssertEquals("Login textbox PasswordChar should be null in all cases", '\0', loginTextbox.PasswordChar);
				AssertEquals("Password Hash textbox PasswordChar should be null when field is empty and unsaved", '\0', passwordHashTextbox.PasswordChar);
				AssertEquals("Signature Key textbox PasswordChar should be null when field is empty and unsaved", '\0', signatureKeyTextbox.PasswordChar);
				AssertEquals("Replacement Key textbox PasswordChar should be null when field is empty and unsaved", '\0', replacementKeyTextbox.PasswordChar);
			}
		}

		public void TestFieldPasswordStyle_WhenDataEnteredAndUnsavedBizo()
		{
			var company = GetDatabindingObject();
			using (var form = CreateFormForTest(company))
			{
				form.Show();
				var (loginTextbox, passwordHashTextbox, signatureKeyTextbox, replacementKeyTextbox) = AssertTextboxesNotNull(form);
				company.HungaryEInvoicingCredentials.Login = "h9nupbpmgi8yhet";
				company.HungaryEInvoicingCredentials.PasswordHash = "0123456789ABCDEF";
				company.HungaryEInvoicingCredentials.SignatureKey = "8a042DA4ZSW17HY6";
				company.HungaryEInvoicingCredentials.ReplacementKey = "fb-8530-0ead7916e5432DA4ZSW1UKW";

				AssertEquals("Login textbox PasswordChar should be null in all cases", '\0', loginTextbox.PasswordChar);
				AssertEquals("Password Hash textbox PasswordChar should be null when field is entered and unsaved", '\0', passwordHashTextbox.PasswordChar);
				AssertEquals("Signature Key textbox PasswordChar should be null when field is entered and unsaved", '\0', signatureKeyTextbox.PasswordChar);
				AssertEquals("Replacement Key textbox PasswordChar should be null when field is entered and unsaved", '\0', replacementKeyTextbox.PasswordChar);
			}
		}

		[RequiresSTA]
		public void TestFieldPasswordStyle_WhenDataEnteredAndBizoSaved()
		{
			var company = GetDatabindingObject();
			using (var form = CreateFormForTest(company))
			{
				form.Show();
				var (loginTextbox, passwordHashTextbox, signatureKeyTextbox, replacementKeyTextbox) = AssertTextboxesNotNull(form);
				company.HungaryEInvoicingCredentials.Login = "h9nupbpmgi8yhet";
				company.HungaryEInvoicingCredentials.PasswordHash = "0123456789ABCDEF";
				company.HungaryEInvoicingCredentials.SignatureKey = "8a042DA4ZSW17HY6";
				company.HungaryEInvoicingCredentials.ReplacementKey = "fb-8530-0ead7916e5432DA4ZSW1UKW";
				var saveResult = form.FireSaveButton();
				AssertEquals("Form should save without error", ContinueWithSave.Yes, saveResult);

				AssertEquals("Login textbox PasswordChar should be null in all cases", '\0', loginTextbox.PasswordChar);
				AssertEquals("Password Hash textbox PasswordChar should be '*' when field is entered and saved", '*', passwordHashTextbox.PasswordChar);
				AssertEquals("Signature Key textbox PasswordChar should be '*' when field is entered and saved", '*', signatureKeyTextbox.PasswordChar);
				AssertEquals("Replacement Key textbox PasswordChar should be '*' when field is entered and saved", '*', replacementKeyTextbox.PasswordChar);
			}
		}

		[RequiresSTA]
		public void TestFieldPasswordStyle_WhenLoadedWithDataEntered()
		{
			var company = GetDatabindingObject();
			company.HungaryEInvoicingCredentials.Login = "h9nupbpmgi8yhet";
			company.HungaryEInvoicingCredentials.PasswordHash = "0123456789ABCDEF";
			company.HungaryEInvoicingCredentials.SignatureKey = "8a042DA4ZSW17HY6";
			company.HungaryEInvoicingCredentials.ReplacementKey = "fb-8530-0ead7916e5432DA4ZSW1UKW";
			Factory.Save();

			using (var form = CreateFormForTest(company))
			{
				form.Show();
				var (loginTextbox, passwordHashTextbox, signatureKeyTextbox, replacementKeyTextbox) = AssertTextboxesNotNull(form);

				AssertEquals("Login textbox PasswordChar should be null in all cases", '\0', loginTextbox.PasswordChar);
				AssertEquals("Password Hash textbox PasswordChar should be '*' when field is entered and loaded", '*', passwordHashTextbox.PasswordChar);
				AssertEquals("Signature Key textbox PasswordChar should be '*' when field is entered and loaded", '*', signatureKeyTextbox.PasswordChar);
				AssertEquals("Replacement Key textbox PasswordChar should be '*' when field is entered and loaded", '*', replacementKeyTextbox.PasswordChar);
			}
		}

		[RequiresSTA]
		public void TestFieldPasswordStyle_WhenLoadedWithDataEnteredAndThenCleared()
		{
			var company = GetDatabindingObject();
			company.HungaryEInvoicingCredentials.Login = "h9nupbpmgi8yhet";
			company.HungaryEInvoicingCredentials.PasswordHash = "0123456789ABCDEF";
			company.HungaryEInvoicingCredentials.SignatureKey = "8a042DA4ZSW17HY6";
			company.HungaryEInvoicingCredentials.ReplacementKey = "fb-8530-0ead7916e5432DA4ZSW1UKW";
			Factory.Save();

			using (var form = CreateFormForTest(company))
			{
				form.Show();
				Application.DoEvents();
				var (loginTextbox, passwordHashTextbox, signatureKeyTextbox, replacementKeyTextbox) = AssertTextboxesNotNull(form);
				ClearTextBoxUsingSendKeys(form, loginTextbox);
				ClearTextBoxUsingSendKeys(form, passwordHashTextbox);
				ClearTextBoxUsingSendKeys(form, signatureKeyTextbox);
				ClearTextBoxUsingSendKeys(form, replacementKeyTextbox);

				AssertEquals("Login textbox PasswordChar should be null in all cases", '\0', loginTextbox.PasswordChar);
				AssertEquals("Password Hash textbox PasswordChar should be null when field is empty after cleared", '\0', passwordHashTextbox.PasswordChar);
				AssertEquals("Signature Key textbox PasswordChar should be null when field is empty after cleared", '\0', signatureKeyTextbox.PasswordChar);
				AssertEquals("Replacement Key textbox PasswordChar should be null when field is empty after cleared", '\0', replacementKeyTextbox.PasswordChar);
			}
		}

		#region Implementation

		GlbCompany GetDatabindingObject()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Hungary;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";

			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();
			credential.GP_GC = company.PK;
			Factory.Save();     // Required to prevent basher test failures due to HasChanges = true

			return company;
		}

		ZChildForm CreateFormForTest(object databindingObject)
		{
			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new GlbCompany_HungaryCredentialUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(databindingObject, nameof(GlbCompany.HungaryEInvoicingCredentials));
			return form;
		}

		(ZTextBox loginTextbox, ZTextBox passwordHashTextbox, ZTextBox signatureKeyTextbox, ZTextBox replacementKeyTextbox) AssertTextboxesNotNull(ZChildForm form)
		{
			var loginTextbox = form.Controls.Find("TextBox_Login", true).FirstOrDefault() as ZTextBox;
			AssertNotNull("Should find Login textbox", loginTextbox);
			var passwordHashTextbox = form.Controls.Find("TextBox_PasswordHash", true).FirstOrDefault() as ZTextBox;
			AssertNotNull("Should find Password Hash textbox", passwordHashTextbox);
			var signatureKeyTextbox = form.Controls.Find("TextBox_SignatureKey", true).FirstOrDefault() as ZTextBox;
			AssertNotNull("Should find Signature Key textbox", signatureKeyTextbox);
			var replacementKeyTextbox = form.Controls.Find("TextBox_ReplacementKey", true).FirstOrDefault() as ZTextBox;
			AssertNotNull("Should find Replacement Key textbox", replacementKeyTextbox);
			return (loginTextbox, passwordHashTextbox, signatureKeyTextbox, replacementKeyTextbox);
		}

		void ClearTextBoxUsingSendKeys(Form form, ZTextBox textbox)
		{
			for (int i = 0; i < 5; i++)
			{
				form.Activate();
				textbox.Focus();
				textbox.SelectAll();
				Application.DoEvents();
				KeySender.PostKeyDown(textbox, textbox.Handle, Keys.Delete);
				KeySender.PostKeyUp(textbox, textbox.Handle, Keys.Delete);
				Application.DoEvents();

				if (string.IsNullOrEmpty(textbox.Text))
				{
					break;
				}
			}

			if (!string.IsNullOrEmpty(textbox.Text))
			{
				throw new Exception($"Clearing textbox '{textbox.Name}' using SendKeys failed.");
			}
		}

		#endregion
	}
}
