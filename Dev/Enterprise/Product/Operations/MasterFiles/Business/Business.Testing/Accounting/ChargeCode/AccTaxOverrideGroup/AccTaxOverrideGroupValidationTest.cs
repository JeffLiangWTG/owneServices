using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTaxOverrideGroupValidation02Test : BusinessObjectValidationTestCase
	{
		public void TestCheckAX_Code()
		{
			string expectedError = "Tax Override Group with the same code already exists.";
			AccTaxOverrideGroup taxOverrideGroup_inDB = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup_inDB.AX_Code = "AAA";
			taxOverrideGroup_inDB.AX_RN_NKCountry = "AU";
			AccTaxOverrideGroup taxOverrideGroup_OtherCountry = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup_OtherCountry.AX_Code = "XXX";
			taxOverrideGroup_OtherCountry.AX_RN_NKCountry = "US";
			Factory.Save();

			AccTaxOverrideGroup taxOverrideGroup_notInDB = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup_notInDB.AX_Code = "BBB";
			taxOverrideGroup_notInDB.AX_RN_NKCountry = "AU";
			Factory.Save();
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_RN_NKCountry = "AU";

			taxOverrideGroup.AX_Code = "AAA";
			AssertHasError(taxOverrideGroup.AX_CodeInfo, expectedError);

			taxOverrideGroup.AX_Code = "XXX";
			AssertNoErrors(taxOverrideGroup.AX_CodeInfo);

			taxOverrideGroup.AX_Code = "BBB";
			AssertHasError(taxOverrideGroup.AX_CodeInfo, expectedError);

			taxOverrideGroup.AX_Code = "";
			AssertHasError(taxOverrideGroup.AX_CodeInfo, "Please enter a Code.");
		}

		public void TestCheckAX_Description()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_RN_NKCountry = "AU";
			taxOverrideGroup.AX_Description = "";
			AssertHasError(taxOverrideGroup.AX_DescriptionInfo, "Please enter a Description.");
		}

		public void TestValidateDuplicateTaxOverrides()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			AccChargeTaxOverride taxOverride1 = taxOverrideGroup.TaxOverrides.AddNew();
			AccChargeTaxOverride taxOverride2 = taxOverrideGroup.TaxOverrides.AddNew();

			taxOverride1.AO_Direction = "ALL";
			taxOverride1.AO_IncoTerm = "ALL";
			taxOverride1.AO_JobType = "ALL";
			taxOverride1.AO_Origin = "ALL";
			taxOverride1.AO_Destination = "ALL";
			taxOverride1.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride1.AO_CustomsStatus = "ALL";

			taxOverride2.AO_Direction = "ALL";
			taxOverride2.AO_IncoTerm = "ALL";
			taxOverride2.AO_JobType = "ALL";
			taxOverride2.AO_Origin = "ALL";
			taxOverride2.AO_Destination = "ALL";
			taxOverride2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride2.AO_CustomsStatus = "ALL";

			taxOverrideGroup.Validation.ValidateDuplicateTaxOverrides();
			AssertNoRowErrors(taxOverride1);
			AssertHasRowError(taxOverride2, "You cannot have identical tax overrides.");

			taxOverride2.AO_JobType = "CST";
			taxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(taxOverride1);
			AssertNoRowErrors(taxOverride2);
		}
	}
}
