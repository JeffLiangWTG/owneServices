using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(GlbCompany_ICS2CredentialUserControl))]
	class GlbCompany_ICS2CredentialUserControlTest : BasherTest
	{
		protected string CountryToTestAgainst => Core.Constants.CountryCodes.Ireland;

		public void TestCaptions()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				AssertEquals("ICS2 Company Reporting Details", userControl.ICS2CertificateGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestCertificateLoadAndClear()
		{
			using (var form = GetFormToBash())
			{
				userControl = form.Controls[1] as GlbCompany_ICS2CredentialUserControl;
				var companyWrapper = Provider.GetWrapper(Company);
				userControl.SetDataBinding(companyWrapper, "");
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull("Initial ICS2 certificate is null", companyWrapper.GlbCompanyCredentialICS2);
					var loaderControl = userControl.FindSingle<DigitalCertificateControl_p12>("CertificateLoaderUserControl");

					(typeof(DigitalCertificateControl_p12).GetField("fDataLoaded", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(loaderControl) as Delegate).DynamicInvoke(null, null);
					AssertNotNull("Load and then a certificate is created", companyWrapper.GlbCompanyCredentialICS2);
					companyWrapper.GlbCompanyCredentialICS2.CurrentDecryptedCertificatePassphrase = "!!!";
					(typeof(DigitalCertificateControl_p12).GetField("fDataLoaded", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(loaderControl) as Delegate).DynamicInvoke(null, null);
					AssertEquals("The loaded certificate can be edited", "!!!", userControl.FindSingle<ZTextBox>("PrivateKey").Text);

					var clearedICS2 = companyWrapper.GlbCompanyCredentialICS2;
					(typeof(DigitalCertificateControl_p12).GetField("fDataCleared", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(loaderControl) as Delegate).DynamicInvoke(null, null);
					AssertNull("ICS2 certificate is null after clearing", companyWrapper.GlbCompanyCredentialICS2);
					AssertEquals("Cleared certificate is deleted", true, clearedICS2.IsDeleted);
				});
			}
		}

		public override Form GetFormToBash()
		{
			var form = new ZForm();
			form.CaptionRenderingEnabled = true;

			userControl = new GlbCompany_ICS2CredentialUserControl();
			userControl.Dock = DockStyle.Fill;

			form.Controls.Add(userControl);
			userControl.SetDataBinding(Provider.GetWrapper(Company), "");

			return form;
		}

		protected GlbCompany Company
		{
			get
			{
				if (glbCompany == null)
				{
					glbCompany = Factory.New<GlbCompany>();
					glbCompany.GC_Code = "ZAC";
				}
				return glbCompany;
			}
		}
		GlbCompany glbCompany;

		protected GlbCompanyWrapperProvider Provider => provider ?? (provider = GlbCompanyWrapperProvider.GetProvider(CountryToTestAgainst));
		GlbCompanyWrapperProvider provider;

		GlbCompany_ICS2CredentialUserControl userControl;
	}
}
