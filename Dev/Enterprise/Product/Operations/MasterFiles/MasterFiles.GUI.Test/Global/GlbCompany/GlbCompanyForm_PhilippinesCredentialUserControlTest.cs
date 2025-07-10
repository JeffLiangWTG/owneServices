using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(GlbCompany_PhilippinesCredentialUserControl))]
	public class GlbCompany_PhilippinesCredentialUserControlTest : UserAndClientCredentialsUserControlTest
	{
		public void TestPasswordStatusVisibility_WhenDataSourceIsGlbCompany()
		{
			using (var form = GetFormToBash() as ZChildForm)
			{
				form.SetDataBinding(GetCompanyForTest(), "PhilippinesEInvoicingCredentials");
				form.Show();

				var userControl = form.Controls.OfType<GlbCompany_PhilippinesCredentialUserControl>().FirstOrDefault();
				Assert("Precondition", userControl.BindingSource.DataSource is GlbCompany);

				var textBox1 = form.Controls.Find("TextBox_UserCredentialPasswordStatusReason", true).FirstOrDefault();
				var textBox2 = form.Controls.Find("TextBox_ClientCredentialPasswordStatusReason", true).FirstOrDefault();
				AssertNotNull("Should find control", textBox1);
				AssertNotNull("Should find control", textBox2);
				Assert("Should be hidden when there is no error message.", !textBox1.Visible);
				Assert("Should be hidden when there is no error message.", !textBox2.Visible);
			}

			using (var form = GetFormToBash() as ZChildForm)
			{
				form.SetDataBinding(GetCompanyForTestWithStatusReasonError(), "PhilippinesEInvoicingCredentials");
				form.Show();

				var userControl = form.Controls.OfType<GlbCompany_PhilippinesCredentialUserControl>().FirstOrDefault();
				Assert("Precondition", userControl.BindingSource.DataSource is GlbCompany);

				var textBox1 = form.Controls.Find("TextBox_UserCredentialPasswordStatusReason", true).FirstOrDefault();
				var textBox2 = form.Controls.Find("TextBox_ClientCredentialPasswordStatusReason", true).FirstOrDefault();
				Assert("Should be visible when there is an error message.", textBox1.Visible);
				Assert("Should be visible when there is an error message.", textBox2.Visible);
			}
		}

		#region Implementation

		protected override string CountryCode => CountryCodes.Philippines;

		protected GlbCompanyExternalPasswordForPhilippines GetDatabindingObject(Func<GlbCompany> companyCreator)
			=> GlbCompanyExternalPasswordForPhilippines.New(companyCreator());

		protected override T GetFormControl<T>(ZChildForm form)
			=> form.Controls.OfType<GlbCompany_PhilippinesCredentialUserControl>().FirstOrDefault() as T;

		protected override UserAndClientCredentials GetDatabindingObject()
			=> GetDatabindingObject(GetCompanyForTest);

		protected override UserAndClientCredentials GetDatabindingObjectWithStatusReasonError()
			=> GetDatabindingObject(GetCompanyForTestWithStatusReasonError);

		GlbCompany GetCompanyForTest()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.Philippines;

			return company;
		}

		GlbCompany GetCompanyForTestWithStatusReasonError()
		{
			var company = GetCompanyForTest();

			var serviceProviderCredential = Factory.NewWithValidTestData<GlbCompanyExternalPasswordPHU>();
			serviceProviderCredential.GP_StatusReason = "service provider error";
			serviceProviderCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			serviceProviderCredential.GP_GC = company.PK;

			var taxpayerCredential = Factory.NewWithValidTestData<GlbCompanyExternalPasswordPHA>();
			taxpayerCredential.GP_StatusReason = "application error";
			taxpayerCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			taxpayerCredential.GP_GC = company.PK;

			return company;
		}

		#endregion
	}
}
