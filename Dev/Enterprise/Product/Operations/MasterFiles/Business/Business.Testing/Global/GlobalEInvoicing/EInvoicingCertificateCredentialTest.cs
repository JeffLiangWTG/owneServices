using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingCertificateCredential))]
	sealed class EInvoicingCertificateCredentialTest : EInvoicingCredentialTest<EInvoicingCertificateCredential>
	{
		#region Implementation

		protected override EInvoicingCertificateCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			var credential = base.CreateNewGlbExternalPassword(factory);
			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			return credential;
		}

		#endregion

		#region LoadBestCertificate() - Branch

		public void TestLoadBestCertificate_Branch_ReturnsNull_WhenNoCertificates()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Branch_ReturnsNull_WhenWrongPasswordType()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22), passwordType: "ZZZ");
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Branch_ReturnsNull_WhenCertificateHasExpired()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory, currentTime: new ZDateTime(2023, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Branch_ReturnsNull_WhenCertificateIssuedInFuture()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory, currentTime: new ZDateTime(2019, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Branch_ReturnsNull_WhenCertificateFieldIsNull()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = null;
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Branch_ReturnsNull_WhenCredentialIsNotValid()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = null;
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_ReturnsNull_WhenDifferentBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var otherBranch = Factory.CreateNewFactory().NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = null;
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			Factory.Save();

			var result = EInvoicingCertificateCredential.LoadBestCertificate(otherBranch, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Branch_ReturnsCredential_WhenValidCertificate()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNotNull(result);
			AssertEquals(certificate.PK, result.PK);
		}

		[TestDate(2021, 02, 12, 11, 05, 22)]
		public void TestLoadBestCertificate_Branch_UsesZDateNow_WhenDateNotSpecified()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory);
			AssertNotNull(result);
			AssertEquals(certificate.PK, result.PK);
		}

		public void TestLoadBestCertificate_Branch_SelectsCredential_BasedOnIssueDate()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);         // 2 year range
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var certificate2 = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate2.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate2.GP_GB = branch.PK;
			certificate2.GP_IssueDate = new ZDateTime(2021, 01, 01);        // 3 month range
			certificate2.GP_ExpiryDate = new ZDateTime(2021, 04, 01);
			certificate2.GP_Certificate = new byte[4];
			certificate2.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22), orderByColumn: nameof(GlbExternalPassword.GP_IssueDate));
			AssertNotNull(result);
			AssertEquals("Second certificate should be selected as it was issued more recently", certificate2.PK, result.PK);
		}

		public void TestLoadBestCertificate_Branch_SelectsCredential_BasedOnExpiryDate()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GB = branch.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);         // 2 year range
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var certificate2 = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate2.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate2.GP_GB = branch.PK;
			certificate2.GP_IssueDate = new ZDateTime(2021, 01, 01);        // 3 month range
			certificate2.GP_ExpiryDate = new ZDateTime(2021, 04, 01);
			certificate2.GP_Certificate = new byte[4];
			certificate2.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var branchInDifferentFactory = Factory.CreateNewFactory().Load<GlbBranch>(branch.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(branchInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22), orderByColumn: nameof(GlbExternalPassword.GP_ExpiryDate));
			AssertNotNull(result);
			AssertEquals("First certificate should be selected as it will expire later", certificate.PK, result.PK);
		}

		#endregion

		#region LoadBestCertificate() - Company

		public void TestLoadBestCertificate_Company_ReturnsNull_WhenNoCertificates()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Company_ReturnsNull_WhenWrongPasswordType()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22), passwordType: "ZZZ");
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Company_ReturnsNull_WhenCertificateHasExpired()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory, currentTime: new ZDateTime(2023, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Company_ReturnsNull_WhenCertificateIssuedInFuture()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory, currentTime: new ZDateTime(2019, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Company_ReturnsNull_WhenCertificateFieldIsNull()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = null;
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Company_ReturnsNull_WhenCredentialIsNotValid()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = null;
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_ReturnsNull_WhenDifferentCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var otherCompany = Factory.CreateNewFactory().NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = null;
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			Factory.Save();

			var result = EInvoicingCertificateCredential.LoadBestCertificate(otherCompany, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNull(result);
		}

		public void TestLoadBestCertificate_Company_ReturnsCredential_WhenValidCertificate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22));
			AssertNotNull(result);
			AssertEquals(certificate.PK, result.PK);
		}

		[TestDate(2021, 02, 12, 11, 05, 22)]
		public void TestLoadBestCertificate_Company_UsesZDateNow_WhenDateNotSpecified()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory);
			AssertNotNull(result);
			AssertEquals(certificate.PK, result.PK);
		}

		public void TestLoadBestCertificate_Company_SelectsCredential_BasedOnIssueDate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);         // 2 year range
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var certificate2 = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate2.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate2.GP_GC = company.PK;
			certificate2.GP_IssueDate = new ZDateTime(2021, 01, 01);        // 3 month range
			certificate2.GP_ExpiryDate = new ZDateTime(2021, 04, 01);
			certificate2.GP_Certificate = new byte[4];
			certificate2.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22), orderByColumn: nameof(GlbExternalPassword.GP_IssueDate));
			AssertNotNull(result);
			AssertEquals("Second certificate should be selected as it was issued more recently", certificate2.PK, result.PK);
		}

		public void TestLoadBestCertificate_Company_SelectsCredential_BasedOnExpiryDate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var certificate = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate.GP_GC = company.PK;
			certificate.GP_IssueDate = new ZDateTime(2020, 07, 15);         // 2 year range
			certificate.GP_ExpiryDate = new ZDateTime(2022, 07, 19);
			certificate.GP_Certificate = new byte[4];
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var certificate2 = Factory.NewWithValidTestData<GlbExternalPassword>();
			certificate2.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificate2.GP_GC = company.PK;
			certificate2.GP_IssueDate = new ZDateTime(2021, 01, 01);        // 3 month range
			certificate2.GP_ExpiryDate = new ZDateTime(2021, 04, 01);
			certificate2.GP_Certificate = new byte[4];
			certificate2.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			var companyInDifferentFactory = Factory.CreateNewFactory().Load<GlbCompany>(company.PK);
			var result = EInvoicingCertificateCredential.LoadBestCertificate(companyInDifferentFactory, currentTime: new ZDateTime(2021, 02, 12, 11, 05, 22), orderByColumn: nameof(GlbExternalPassword.GP_ExpiryDate));
			AssertNotNull(result);
			AssertEquals("First certificate should be selected as it will expire later", certificate.PK, result.PK);
		}

		#endregion
	}
}
