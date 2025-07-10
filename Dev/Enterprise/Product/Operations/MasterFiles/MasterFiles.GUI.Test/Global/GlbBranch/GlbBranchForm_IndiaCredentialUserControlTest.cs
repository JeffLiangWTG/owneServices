using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbBranchForm_IndiaCredentialUserControl))]
	public class GlbBranchForm_IndiaCredentialUserControlTest : UserAndClientCredentialsUserControlTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.India;

		[RequiresSTA]
		public void TestPasswordStatusVisibility_WhenDataSourceIsGlbBranch()
		{
			using (var form = GetFormToBash() as ZChildForm)
			{
				form.SetDataBinding(GetBranchForTest(), "BranchCredentialsIndia");
				form.Show();

				var userControl = GetFormControl<GlbBranchForm_IndiaCredentialUserControl>(form);
				Assert("Precondition", userControl.BindingSource.DataSource is GlbBranch);

				var textBox1 = form.Controls.Find("TextBox_UserCredentialPasswordStatusReason", true).FirstOrDefault();
				var textBox2 = form.Controls.Find("TextBox_ClientCredentialPasswordStatusReason", true).FirstOrDefault();
				AssertNotNull("Should find control", textBox1);
				AssertNotNull("Should find control", textBox2);
				Assert("Should be hidden when there is no error message.", !textBox1.Visible);
				Assert("Should be hidden when there is no error message.", !textBox2.Visible);
			}

			using (var form = GetFormToBash() as ZChildForm)
			{
				form.SetDataBinding(GetBranchForTestWithStatusReasonError(), "BranchCredentialsIndia");
				form.Show();

				var userControl = GetFormControl<GlbBranchForm_IndiaCredentialUserControl>(form);
				Assert("Precondition", userControl.BindingSource.DataSource is GlbBranch);

				var textBox1 = form.Controls.Find("TextBox_UserCredentialPasswordStatusReason", true).FirstOrDefault();
				var textBox2 = form.Controls.Find("TextBox_ClientCredentialPasswordStatusReason", true).FirstOrDefault();
				Assert("Should be visible when there is an error message.", textBox1.Visible);
				Assert("Should be visible when there is an error message.", textBox2.Visible);
			}
		}

		#region Implementation

		GlbBranchCredentialsForIndia GetDatabindingObject(Func<GlbBranch> branchCreator) => GlbBranchCredentialsForIndia.New(branchCreator());

		GlbBranch GetBranchForTest()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.India;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			return branch;
		}

		GlbBranch GetBranchForTestWithStatusReasonError()
		{
			var branch = GetBranchForTest();

			var serviceProviderCredential = Factory.NewWithValidTestData<GlbBranchExternalPasswordINS>();
			serviceProviderCredential.GP_StatusReason = "service provider error";
			serviceProviderCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			serviceProviderCredential.GP_GB = branch.PK;

			var taxpayerCredential = Factory.NewWithValidTestData<GlbBranchExternalPasswordINT>();
			taxpayerCredential.GP_StatusReason = "taxpayer error";
			taxpayerCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			taxpayerCredential.GP_GB = branch.PK;

			return branch;
		}

		protected override UserAndClientCredentials GetDatabindingObject()
			=> GetDatabindingObject(GetBranchForTest);

		protected override UserAndClientCredentials GetDatabindingObjectWithStatusReasonError()
			=> GetDatabindingObject(GetBranchForTestWithStatusReasonError);

		protected override T GetFormControl<T>(ZChildForm form)
			=> form.Controls.OfType<GlbBranchForm_IndiaCredentialUserControl>().FirstOrDefault() as T;

		#endregion
	}
}
