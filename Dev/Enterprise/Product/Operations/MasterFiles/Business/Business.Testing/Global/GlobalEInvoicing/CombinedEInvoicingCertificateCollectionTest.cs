using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class BranchCombinedEInvoicingCertificateCollectionTest : CombinedEInvoicingCertificateCollectionTest
	{
		public override void TestCollectionFiltering()
		{
			var master = GetMaster();
			var branchSettings = EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>().Object;
			var credentialCollection = new CombinedEInvoicingCertificateCollection(master, branchSettings);

			EInvoicingCertificateCredential GetValid()
			{
				var certificate = credentialCollection.AddNew();
				certificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				certificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				certificate.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
				certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				return certificate;
			}
			var validCertificate = GetValid();
			validCertificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			validCertificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			AssertEquals("Precondition", PasswordTypesList.Codes.EIM, validCertificate.GP_PasswordType);
			AssertEquals("Precondition", master.AsBranch.PK, validCertificate.GP_GB);
			AssertEquals("Precondition", false, validCertificate.GP_Certificate.IsEmpty);

			var invalidCertificateWithNoBranch = GetValid();
			invalidCertificateWithNoBranch.GP_GB = Factory.New<GlbBranch>().PK;

			var invalidMissingCertificate = credentialCollection.AddNew();
			AssertEquals("Precondition", true, invalidMissingCertificate.GP_Certificate.IsEmpty);

			credentialCollection.Load();

			var expectedResults = new[] { validCertificate };
			AssertContainsExactElementsInAnyOrder("Invalid credentials should be filtered out", expectedResults, credentialCollection);
		}

		public override void TestIsEditAllowed()
		{
			AssertEquals(false, CertificateCollection.IsEditAllowed);

			var companySettings = EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>().Object;
			var collectionWithBranchSettings = new CombinedEInvoicingCertificateCollection(GetMaster(), companySettings);

			AssertEquals(true, collectionWithBranchSettings.IsEditAllowed);
		}

		public override CombinedEInvoicingMaster GetMaster()
		{
			var branch = Factory.NewCompanyAndBranchWith();
			return new CombinedEInvoicingMaster(branch);
		}
	}

	public class CompanyCombinedEInvoicingCertificateCollectionTest : CombinedEInvoicingCertificateCollectionTest
	{
		public override void TestCollectionFiltering()
		{
			var master = GetMaster();
			var branchSettings = EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>().Object;
			var credentialCollection = new CombinedEInvoicingCertificateCollection(master, branchSettings);

			EInvoicingCertificateCredential GetValid()
			{
				var certificate = credentialCollection.AddNew();
				certificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				certificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				certificate.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
				certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				return certificate;
			}
			var validCertificate = GetValid();
			AssertEquals("Precondition", PasswordTypesList.Codes.EIM, validCertificate.GP_PasswordType);
			AssertEquals("Precondition", master.AsCompany.PK, validCertificate.GP_GC);
			AssertEquals("Precondition", false, validCertificate.GP_Certificate.IsEmpty);

			var invalidCertificateWithBranch = GetValid();
			invalidCertificateWithBranch.GP_GB = GlbBranch.CurrentBranch.PK;

			var invalidCertificateMissingPasswordType = GetValid();
			invalidCertificateMissingPasswordType.GP_PasswordType = ZString.Empty;

			var invalidMissingCertificate = credentialCollection.AddNew();
			AssertEquals("Precondition", true, invalidMissingCertificate.GP_Certificate.IsEmpty);

			credentialCollection.Load();

			var expectedResults = new[] { validCertificate };
			AssertContainsExactElementsInAnyOrder("Invalid credentials should be filtered out", expectedResults, credentialCollection);
		}

		public override void TestIsEditAllowed()
		{
			AssertEquals(false, CertificateCollection.IsEditAllowed);

			var companySettings = EInvoicingSettingsHelper.CreateAndHookSettingsMockForCompany<IEInvoicingCertificateCredentialSettings>().Object;
			var collectionWithCompanySettings = new CombinedEInvoicingCertificateCollection(GetMaster(), companySettings);

			AssertEquals(true, collectionWithCompanySettings.IsEditAllowed);
		}

		public override CombinedEInvoicingMaster GetMaster()
		{
			var company = Factory.NewCompany();
			return new CombinedEInvoicingMaster(company);
		}
	}

	[TestedType(typeof(CombinedEInvoicingCertificateCollection))]
	public abstract class CombinedEInvoicingCertificateCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation
		public abstract void TestCollectionFiltering();
		public abstract void TestIsEditAllowed();

		public abstract CombinedEInvoicingMaster GetMaster();

		protected override BusinessObjectCollection GetCollectionToTest() => CertificateCollection;

		CombinedEInvoicingCertificateCollection certificateCollection;
		protected CombinedEInvoicingCertificateCollection CertificateCollection => certificateCollection
			?? (certificateCollection = new CombinedEInvoicingCertificateCollection(GetMaster(), null));

		#endregion
	}
}
