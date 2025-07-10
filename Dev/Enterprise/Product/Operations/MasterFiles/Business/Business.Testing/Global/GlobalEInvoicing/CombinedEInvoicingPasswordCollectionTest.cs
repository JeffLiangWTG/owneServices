using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class BranchCombinedEInvoicingPasswordCollectionTest : CombinedEInvoicingPasswordCollectionTest
	{
		public override void TestCollectionFiltering()
		{
			var master = GetMaster();
			var branchSettings = EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingPasswordCredentialSettings>().Object;
			var passwordCollection = new CombinedEInvoicingPasswordCollection(master, branchSettings);

			var validPassword = passwordCollection.AddNew();
			AssertEquals("Precondition", PasswordTypesList.Codes.EIM, validPassword.GP_PasswordType);
			AssertEquals("Precondition", master.AsBranch.PK, validPassword.GP_GB);

			var invalidPasswordForAnotherBranch = passwordCollection.AddNew();
			invalidPasswordForAnotherBranch.GP_GB = Factory.NewWithValidTestData<GlbBranch>().PK;

			var invalidPasswordWithCertificate = passwordCollection.AddNew();
			invalidPasswordWithCertificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

			var invalidPasswordBadPasswordType = passwordCollection.AddNew();
			invalidPasswordBadPasswordType.GP_PasswordType = "BAD";

			passwordCollection.Load();

			var expectedResults = new[] { validPassword };
			AssertContainsExactElementsInAnyOrder("Invalid Passwords should be filtered out", expectedResults, passwordCollection);
		}

		public override void TestIsEditAllowed()
		{
			AssertEquals(false, PasswordCollection.IsEditAllowed);

			var companySettings = EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingPasswordCredentialSettings>().Object;
			var passwordCollectionWithBranchSettings = new CombinedEInvoicingPasswordCollection(GetMaster(), companySettings);

			AssertEquals(true, passwordCollectionWithBranchSettings.IsEditAllowed);
		}

		public override CombinedEInvoicingMaster GetMaster()
		{
			var branch = Factory.NewCompanyAndBranchWith();
			return new CombinedEInvoicingMaster(branch);
		}
	}

	public class CompanyCombinedEInvoicingPasswordCollectionTest : CombinedEInvoicingPasswordCollectionTest
	{
		public override void TestCollectionFiltering()
		{
			var companySettings = EInvoicingSettingsHelper.CreateAndHookSettingsMockForCompany<IEInvoicingPasswordCredentialSettings>().Object;
			var passwordCollection = new CombinedEInvoicingPasswordCollection(GetMaster(), companySettings);

			var validPassword = passwordCollection.AddNew();
			AssertEquals("Precondition", PasswordTypesList.Codes.EIM, validPassword.GP_PasswordType);
			AssertEquals("Precondition", ZGuid.Empty, validPassword.GP_GB);

			var invalidPasswordWithBranch = passwordCollection.AddNew();
			invalidPasswordWithBranch.GP_GB = GlbBranch.CurrentBranch.PK;

			var invalidPasswordWithCertificate = passwordCollection.AddNew();
			invalidPasswordWithCertificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

			var invalidPasswordBadPasswordType = passwordCollection.AddNew();
			invalidPasswordBadPasswordType.GP_PasswordType = "BAD";

			passwordCollection.Load();

			var expectedResults = new[] { validPassword };
			AssertContainsExactElementsInAnyOrder("Invalid Passwords should be filtered out", expectedResults, passwordCollection);
		}

		public override void TestIsEditAllowed()
		{
			AssertEquals(false, PasswordCollection.IsEditAllowed);

			var companySettings = EInvoicingSettingsHelper.CreateAndHookSettingsMockForCompany<IEInvoicingPasswordCredentialSettings>().Object;
			var passwordCollectionWithCompanySettings = new CombinedEInvoicingPasswordCollection(GetMaster(), companySettings);

			AssertEquals(true, passwordCollectionWithCompanySettings.IsEditAllowed);
		}

		public override CombinedEInvoicingMaster GetMaster()
		{
			var company = Factory.NewCompany();
			return new CombinedEInvoicingMaster(company);
		}
	}

	[TestedType(typeof(CombinedEInvoicingPasswordCollection))]
	public abstract class CombinedEInvoicingPasswordCollectionTest : BusinessObjectCollectionTestCase
	{
		public abstract void TestCollectionFiltering();
		public abstract void TestIsEditAllowed();

		#region Implementation

		public abstract CombinedEInvoicingMaster GetMaster();

		protected override BusinessObjectCollection GetCollectionToTest() => PasswordCollection;

		CombinedEInvoicingPasswordCollection passwordCollection;
		protected CombinedEInvoicingPasswordCollection PasswordCollection => passwordCollection
			?? (passwordCollection = new CombinedEInvoicingPasswordCollection(GetMaster(), null));

		#endregion
	}
}
