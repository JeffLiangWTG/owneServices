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
	[TestedType(typeof(GlbCompanySignatureCredentialUserControl))]
	sealed class GlbCompany_SignatureCredentialUserControlTest : BasherTest
	{
		public void TestDataGridTextBoxPasswordChar_WhenLoadedWithDataEntered()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var company = GetDatabindingObject();
				var credential = Factory.NewWithValidTestData<GlbCompanySignatureCredential>();
				credential.GP_GC = company.PK;
				Factory.Save();

				company.SignatureCredentials.Load();

				using (var form = CreateFormForTest(company))
				{
					form.Show();
					var grid = form.Controls.Find("SignatureCredentialsGrid", true).FirstOrDefault() as ZGrid;
					var passwordColumn = grid.Columns[GlbCompanySignatureCredential.Schema.CurrentDecryptedPassword];
					var passwordColumnStyle = passwordColumn?.ColumnStyle as ZTextBoxColumnStyle;
					var textbox = passwordColumnStyle?.EditControl as DataGridTextBox;

					AssertEquals("PasswordChar should be '*'", '*', textbox.PasswordChar);
				}
			}
		}

		public void TestDataGridTextBoxPasswordChar_WhenLoadedWithDataEnteredAndThenCleared()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var company = GetDatabindingObject();
				var credential = Factory.NewWithValidTestData<GlbCompanySignatureCredential>();
				credential.GP_GC = company.PK;
				Factory.Save();

				company.SignatureCredentials.Load();

				using (var form = CreateFormForTest(company))
				{
					form.Show();
					Application.DoEvents();
					var grid = form.Controls.Find("SignatureCredentialsGrid", true).FirstOrDefault() as ZGrid;
					var passwordColumn = grid.Columns[GlbCompanySignatureCredential.Schema.CurrentDecryptedPassword];
					var passwordColumnStyle = passwordColumn?.ColumnStyle as ZTextBoxColumnStyle;
					var textbox = passwordColumnStyle?.EditControl as DataGridTextBox;

					ClearTextBoxUsingSendKeys(form, textbox);
					AssertEquals("PasswordChar should be null", '\0', textbox.PasswordChar);
				}
			}
		}

		[RequiresSTA]
		public void TestDataGridTextBoxPasswordChar_WhenDataEnteredAndUnsavedBizo()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var company = GetDatabindingObject();

				using (var form = CreateFormForTest(company))
				{
					form.Show();

					var grid = form.Controls.Find("SignatureCredentialsGrid", true).FirstOrDefault() as ZGrid;
					var passwordColumn = grid.Columns[GlbCompanySignatureCredential.Schema.CurrentDecryptedPassword];
					var passwordColumnStyle = passwordColumn?.ColumnStyle as ZTextBoxColumnStyle;
					var textbox = passwordColumnStyle?.EditControl as DataGridTextBox;

					ClearTextBoxUsingSendKeys(form, textbox);
					var credential = grid.GetCurrent() as GlbCompanySignatureCredential;
					credential.GP_UserID = "123";
					credential.CurrentDecryptedPassword = "123";

					AssertEquals("PasswordChar should be null when field is entered and unsaved", '\0', textbox.PasswordChar);
				}
			}
		}

		public void TestDataGridTextBoxPasswordChar_WhenDataEnteredAndBizoSaved()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var company = GetDatabindingObject();

				using (var form = CreateFormForTest(company))
				{
					form.Show();

					var grid = form.Controls.Find("SignatureCredentialsGrid", true).FirstOrDefault() as ZGrid;
					var passwordColumn = grid.Columns[GlbCompanySignatureCredential.Schema.CurrentDecryptedPassword];
					var passwordColumnStyle = passwordColumn?.ColumnStyle as ZTextBoxColumnStyle;
					var textbox = passwordColumnStyle?.EditControl as DataGridTextBox;

					ClearTextBoxUsingSendKeys(form, textbox);
					var credential = grid.GetCurrent() as GlbCompanySignatureCredential;
					credential.GP_UserID = "123";
					credential.CurrentDecryptedPassword = "123";
					credential.GP_PasswordStatus = Core.Constants.PasswordOK;
					Factory.Save();

					AssertEquals("PasswordChar should be '*' when field is entered and saved", '*', textbox.PasswordChar);
				}
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Turkey;

		public override Form GetFormToBash() => CreateFormForTest(GetDatabindingObject());

		ZChildForm CreateFormForTest(object databindingObject)
		{
			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new GlbCompanySignatureCredentialUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(databindingObject, ".");
			return form;
		}

		GlbCompany GetDatabindingObject()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();

			return company;
		}

		void ClearTextBoxUsingSendKeys(Form form, DataGridTextBox textbox)
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
	}
}
