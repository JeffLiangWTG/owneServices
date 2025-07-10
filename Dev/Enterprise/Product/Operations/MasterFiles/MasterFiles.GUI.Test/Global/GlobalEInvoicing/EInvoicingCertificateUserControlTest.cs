using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EInvoicingCertificateUserControl))]
	sealed class EInvoicingCertificateUserControlTest : BasherTest
	{
		public override Form GetFormToBash()
		{
			var branch = Factory.NewCompanyAndBranchWith(countryCode: Constants.CountryCodes.SaudiArabia);
			Factory.Save();

			var form = new ZChildForm { CaptionRenderingEnabled = true };
			form.Controls.Add(new EInvoicingCertificateUserControl { Dock = DockStyle.Fill });
			form.SetDataBinding(branch.EInvoicingCertificateCredentials, string.Empty);
			return form;
		}

		public void TestLoadWithSaudiArabiaCompanyAndItsOwnBranch()
		{
			var saBranch = CreateBranch(Constants.CountryCodes.SaudiArabia);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, saBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var form = new ZChildForm { CaptionRenderingEnabled = true };
				var control = new EInvoicingCertificateUserControl { Dock = DockStyle.Fill };
				control.SetDataSourceForX509CertificatesGrid_TestOnly(saBranch);
				form.Controls.Add(control);
				using (form)
				{
					form.Show();
					var registerButton = control.FindSingle<ZButton>("RegisterButton");
					Assert(registerButton.Visible);
					AssertEquals("Register", registerButton.CaptionResourceString.Caption);
					var loadButton = control.FindSingle<ZButton>("LoadButton");
					AssertEquals("Load", loadButton.CaptionResourceString.Caption);
				}
			}
		}

		[RequiresSTA]
		public void TestLoadWithNonCurrentCompanyBranch()
		{
			var branch = CreateBranch(Constants.CountryCodes.SaudiArabia);
			branch.GB_RN_NKCountryCode = Constants.CountryCodes.Australia;

			var form = new ZChildForm { CaptionRenderingEnabled = true };
			var control = new EInvoicingCertificateUserControl { Dock = DockStyle.Fill };
			control.SetDataSourceForX509CertificatesGrid_TestOnly(branch);
			form.Controls.Add(control);
			using (form)
			{
				form.Show();
				var registerButton = control.FindSingle<ZButton>("RegisterButton");
				Assert(!registerButton.Visible);
				AssertEquals("Register", registerButton.CaptionResourceString.Caption);
				var loadButton = control.FindSingle<ZButton>("LoadButton");
				AssertEquals("Add", loadButton.CaptionResourceString.Caption);
			}
		}

		public void TestLoadWithNonSaudiArabiaCompany()
		{
			var branch = CreateBranch(Constants.CountryCodes.Australia);

			var form = new ZChildForm { CaptionRenderingEnabled = true };
			var control = new EInvoicingCertificateUserControl { Dock = DockStyle.Fill };
			control.SetDataSourceForX509CertificatesGrid_TestOnly(branch);
			form.Controls.Add(control);
			using (form)
			{
				form.Show();
				var registerButton = control.FindSingle<ZButton>("RegisterButton");
				Assert(!registerButton.Visible);
				AssertEquals("Register", registerButton.CaptionResourceString.Caption);
				var loadButton = control.FindSingle<ZButton>("LoadButton");
				AssertEquals("Add", loadButton.CaptionResourceString.Caption);
			}
		}

		public void TestLoadWithSaudiArabiaCompanyAndBranchFromAnotherSACompany()
		{
			var branchA = CreateBranch(Constants.CountryCodes.SaudiArabia);
			branchA.GB_Code = "TST";

			var branchB = CreateBranch(Constants.CountryCodes.SaudiArabia);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var form = new ZChildForm { CaptionRenderingEnabled = true };
				var control = new EInvoicingCertificateUserControl { Dock = DockStyle.Fill };
				control.SetDataSourceForX509CertificatesGrid_TestOnly(branchB);
				form.Controls.Add(control);
				using (form)
				{
					form.Show();
					var registerButton = control.FindSingle<ZButton>("RegisterButton");
					Assert(!registerButton.Visible);
					AssertEquals("Register", registerButton.CaptionResourceString.Caption);
					var loadButton = control.FindSingle<ZButton>("LoadButton");
					AssertEquals("Add", loadButton.CaptionResourceString.Caption);
				}
			}
		}

		#region Helpers

		GlbBranch CreateBranch(ZString companyCountryCode)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = companyCountryCode;
			var branch = company.Branches.AddNew();
			Factory.Save();

			return branch;
		}

		#endregion
	}
}
