using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbCompanyForm_CredentialUserControl))]
	sealed class GlbCompanyForm_CredentialUserControlTest : BasherTest
	{
		public override Form GetFormToBash()
		{
			var form = new ZChildForm { CaptionRenderingEnabled = true };
			form.Controls.Add(new GlbCompanyForm_CredentialUserControl { Dock = DockStyle.Fill });
			form.SetDataBinding(Company.EInvoicingCertificateCredentials, string.Empty);
			return form;
		}

		public void TestGlbCompanyForm_CredentialUserControlVisibilityColumns()
		{
			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			var settingsMock = new Mock<ICountryEInvoicingObjectFactorySettings>();
			var credentialsMock = new Mock<IEInvoicingCertificateCredentialSettings>();

			globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns(settingsMock.Object);
			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			settingsMock.Setup(x => x.Credentials).Returns(credentialsMock.Object);

			ObjectFactory.Substitute(globalFactoryMock.Object);

			var expectedListOfAllVisibleColumns = new[]
			{
				$"{GlbBranchEInvoicingCertificateCredential.Schema.GP_MailBoxID} (ZTextBoxColumnStyleInfo) IsUnavailable:False",
				$"{GlbBranchEInvoicingCertificateCredential.Schema.CurrentDecryptedCertificatePassphrase} (ZTextBoxColumnStyleInfo) IsUnavailable:False",
				$"SerialNumber (ZTextBoxColumnStyleInfo) IsUnavailable:False",
				$"IssuerNameCommonName (ZTextBoxColumnStyleInfo) IsUnavailable:False",
				$"{GlbBranchEInvoicingCertificateCredential.Schema.GP_IssueDate} (ZDateEditColumnStyleInfo) IsUnavailable:False",
				$"{GlbBranchEInvoicingCertificateCredential.Schema.GP_ExpiryDate} (ZDateEditColumnStyleInfo) IsUnavailable:False",
				$"PasswordStatus (ZDropEditColumnStyleInfo) IsUnavailable:False",
			};

			AssertVisibilityColumns("None of the columns should be hidden.", expectedListOfAllVisibleColumns, System.Array.Empty<string>());

			var expectedListOfAnyVisibleColumns = new[]
			{
				$"{GlbBranchEInvoicingCertificateCredential.Schema.GP_MailBoxID} (ZTextBoxColumnStyleInfo) IsUnavailable:True",
				$"{GlbBranchEInvoicingCertificateCredential.Schema.CurrentDecryptedCertificatePassphrase} (ZTextBoxColumnStyleInfo) IsUnavailable:False",
				$"SerialNumber (ZTextBoxColumnStyleInfo) IsUnavailable:True",
				$"IssuerNameCommonName (ZTextBoxColumnStyleInfo) IsUnavailable:True",
				$"{GlbBranchEInvoicingCertificateCredential.Schema.GP_IssueDate} (ZDateEditColumnStyleInfo) IsUnavailable:False",
				$"{GlbBranchEInvoicingCertificateCredential.Schema.GP_ExpiryDate} (ZDateEditColumnStyleInfo) IsUnavailable:True",
				$"PasswordStatus (ZDropEditColumnStyleInfo) IsUnavailable:True",
			};

			AssertVisibilityColumns("Some columns must be hidden.", expectedListOfAnyVisibleColumns, new string[] { "GP_MailBoxID", "SerialNumber", "GP_ExpiryDate", "PasswordStatus", "IssuerNameCommonName" });

			var expectedListOfAnyVisibleColumns1 = new[]
			{
				$"{GlbBranchEInvoicingCertificateCredential.Schema.GP_MailBoxID} (ZTextBoxColumnStyleInfo) IsUnavailable:True",
				$"{GlbBranchEInvoicingCertificateCredential.Schema.CurrentDecryptedCertificatePassphrase} (ZTextBoxColumnStyleInfo) IsUnavailable:False",
				$"SerialNumber (ZTextBoxColumnStyleInfo) IsUnavailable:False",
				$"IssuerNameCommonName (ZTextBoxColumnStyleInfo) IsUnavailable:False",
				$"{GlbBranchEInvoicingCertificateCredential.Schema.GP_IssueDate} (ZDateEditColumnStyleInfo) IsUnavailable:False",
				$"{GlbBranchEInvoicingCertificateCredential.Schema.GP_ExpiryDate} (ZDateEditColumnStyleInfo) IsUnavailable:False",
				$"PasswordStatus (ZDropEditColumnStyleInfo) IsUnavailable:False",
			};

			AssertVisibilityColumns("A single column must be hidden.", expectedListOfAnyVisibleColumns1, new string[] { "GP_MailBoxID" });

			void AssertVisibilityColumns(string message, string[] expectedListOfColumns, string[] credentialsGridListOfHiddenColumns)
			{
				credentialsMock.Setup(x => x.HiddenColumns).Returns(credentialsGridListOfHiddenColumns);

				var company = GetDatabindingObject();
				company.SignatureCredentials.Load();

				using (var companyForm = GetFormToBash())
				{
					companyForm.Show();

					var grid = companyForm.Controls.Find("BranchCredentialsGrid", true).FirstOrDefault() as ZGrid;

					var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsUnavailable:{x.IsUnavailable}").ToArray();

					AssertArrayEqualsByElements(message, expectedListOfColumns, realListOfColumns);
				}
			}
		}

		#region Implementations

		GlbCompany GetDatabindingObject()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var credential = Factory.New<GlbCompanyEInvoicingCertificateCredential>();
			credential.GP_GC = company.PK;
			Factory.Save();

			return company;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			Company.Factory.Save();
		}

		GlbCompany Company => glbCompany ?? (glbCompany = Factory.NewWithValidTestData<GlbCompany>());
		GlbCompany glbCompany;
	}
}
