using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingPasswordValidation))]
	public class EInvoicingPasswordValidationTest : GlbExternalPasswordValidationTest<EInvoicingPasswordCredential, EInvoicingPasswordValidation>
	{
		public void TestValidateDuplicateConstraint()
		{
			var branch = Factory.NewCompanyAndBranchWith();

			var savedCredentialWithCompany = Factory.New<EInvoicingPasswordCredential>();
			savedCredentialWithCompany.GP_GC = branch.GB_GC;

			var savedCredentialWithBranch = Factory.New<EInvoicingPasswordCredential>();
			savedCredentialWithBranch.GP_GB = branch.PK;
			Factory.Save();

			var expectedRowError = "Duplicate credential found; there is already another GlbExternalPassword with the same data.";

			var newCredential = Factory.New<EInvoicingPasswordCredential>();
			newCredential.Validation.ValidateDuplicateConstraint();
			AssertNoRowError(newCredential, expectedRowError);

			newCredential.GP_GC = branch.GB_GC;
			newCredential.Validation.ValidateDuplicateConstraint();
			AssertHasRowErrorContaining(newCredential, expectedRowError);

			newCredential = Factory.New<EInvoicingPasswordCredential>();
			newCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;
			newCredential.Validation.ValidateDuplicateConstraint();
			AssertNoRowError(newCredential, expectedRowError);

			newCredential = Factory.New<EInvoicingPasswordCredential>();
			newCredential.GP_GB = branch.PK;
			newCredential.Validation.ValidateDuplicateConstraint();
			AssertHasRowError(newCredential, expectedRowError);

			newCredential = Factory.New<EInvoicingPasswordCredential>();
			newCredential.GP_GB = branch.PK;
			newCredential.GP_MailBoxID = "Letterbox";
			newCredential.Validation.ValidateDuplicateConstraint();
			AssertNoRowError(newCredential, expectedRowError);
		}
	}
}
