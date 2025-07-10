using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbCompany_EInvoicingOAuthAuthorizationUserControl))]
	sealed class GlbCompany_EInvoicingOAuthAuthorizationUserControlTest : BasherTest
	{
		[RequiresSTA]
		public void TestReloadButtonClick()
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceInfoBase.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.ShouldShowCredentialsTab(It.IsAny<ICompany>()))
				.Returns(true);

			var credentials = new EInvoiceOAuthCredentialCollection();
			credentials.Add(new EInvoiceOAuthCredential() { Status = "Pending" });

			mockICountryComplianceInfoBase.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.CreateOAuthData(It.IsAny<ICompany>()))
				.Returns(new EInvoiceOAuthData(Company, credentials));

			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
				.Returns(mockICountryComplianceInfoBase.Object);

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			using (var form = GetFormToBash())
			{
				form.Show();

				var eInvoicingOAuthAuthorizationUserControl = GetGlbCompany_EInvoicingOAuthAuthorizationUserControl(form);
				var grid = (ZGrid)eInvoicingOAuthAuthorizationUserControl.Controls.Find("TokenManagementGrid", true)[0];
				AssertEquals("Prerequisite", "Pending", ((EInvoiceOAuthCredential)grid.GetFirstBizOInList()).Status);

				credentials = new EInvoiceOAuthCredentialCollection();
				credentials.Add(new EInvoiceOAuthCredential() { Status = "Valid" });
				mockICountryComplianceInfoBase.As<IEInvoiceCredentialsProvider>()
					.Setup(x => x.CreateOAuthData(It.IsAny<ICompany>()))
					.Returns(new EInvoiceOAuthData(Company, credentials));

				var reloadButton = (ZButton)eInvoicingOAuthAuthorizationUserControl.Controls.Find("ReloadButton", true)[0];
				reloadButton.PerformClick();

				AssertEquals("Credential should be refreshed.", "Valid", ((EInvoiceOAuthCredential)grid.GetFirstBizOInList()).Status);
			}
		}

		public void TestAuthorizeButtonClick()
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceInfoBase.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.GetAuthorizationURL(It.IsAny<ICompany>()))
				.Returns("https://example.com");

			mockICountryComplianceInfoBase.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.ShouldShowCredentialsTab(It.IsAny<ICompany>()))
				.Returns(true);

			mockICountryComplianceInfoBase.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.CreateOAuthData(It.IsAny<ICompany>()))
				.Returns(new EInvoiceOAuthData(Company, new EInvoiceOAuthCredentialCollection()));

			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
				.Returns(mockICountryComplianceInfoBase.Object);

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			using (var form = GetFormToBash())
			{
				form.Show();

				var eInvoicingOAuthAuthorizationUserControl = GetGlbCompany_EInvoicingOAuthAuthorizationUserControl(form);

				var authorizeButton = (ZButton)eInvoicingOAuthAuthorizationUserControl.Controls.Find("AuthorizeButton", true)[0];
				authorizeButton.PerformClick();
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				AssertEquals("https://example.com", WebUrlLauncher.LastUrlLaunched);

				mockICountryComplianceInfoBase.As<IEInvoiceCredentialsProvider>()
					.Verify(x => x.CreateOrUpdateEInvoicingCredential(It.IsAny<ICompany>()), Times.Once);
			}
		}

		public override Form GetFormToBash()
		{
			var form = new GlbCompanyForm(Company);
			form.SetDataBinding(Company, string.Empty);

			return form;
		}

		GlbCompany_EInvoicingOAuthAuthorizationUserControl GetGlbCompany_EInvoicingOAuthAuthorizationUserControl(Form form)
		{
			var companyTabControl = (ZTemplateTabControl)form.Controls.Find("CompanyTabControl", true)[0];
			companyTabControl.SelectTab("AccountingConfigurationTabPage");

			var accConfigTabControl = (ZTemplateTabControl)companyTabControl.Controls.Find("AccConfigTabControl", true)[0];
			accConfigTabControl.SelectTab("EInvoiceOAuthAuthorizationTabPage");

			var eInvoicingOAuthAuthorizationUserControl = (GlbCompany_EInvoicingOAuthAuthorizationUserControl)accConfigTabControl.Controls.Find("EInvoicingOAuthAuthorizationUserControl", true)[0];
			AssertNotNull(eInvoicingOAuthAuthorizationUserControl);
			return eInvoicingOAuthAuthorizationUserControl;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Company.Factory.Save();
		}

		GlbCompany Company => glbCompany ?? (glbCompany = Factory.NewWithValidTestData<GlbCompany>());
		GlbCompany glbCompany;
	}
}
