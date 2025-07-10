using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CommonCusReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_Code_ListValidation()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(commonCusReference.CFR_CodeInfo, "XX", "YY");
		}

		public void TestCheckCFR_Code_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(commonCusReference.CFR_CodeInfo);
		}

		public void TestCheckCFR_Reference_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(commonCusReference.CFR_ReferenceInfo);
		}

		public void TestCheckOwnerOrgPK()
		{
			commonCusReference.OwnerOrgPK = ZGuid.Invalid;
			AssertHasError("Invalid GUID", commonCusReference.OwnerOrgPKInfo, InvalidOwnerError);

			commonCusReference.OwnerOrgPK = Factory.New<OrgHeader>().PK;
			AssertNoError("Valid GUID", commonCusReference.OwnerOrgPKInfo, InvalidOwnerError);
		}

		public void TestValidateAll()
		{
			commonCusReference.OwnerOrgPK = ZGuid.Invalid;
			commonCusReference.Validation.ValidateAll();
			AssertHasError("Validation for OwnerOrgPK", commonCusReference.OwnerOrgPKInfo, InvalidOwnerError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			commonCusReference = Factory.New<CommonCusReferenceForTest>();
		}
		CommonCusReferenceForTest commonCusReference;

		const string InvalidOwnerError = "Enter a valid Owner.";
	}
}
