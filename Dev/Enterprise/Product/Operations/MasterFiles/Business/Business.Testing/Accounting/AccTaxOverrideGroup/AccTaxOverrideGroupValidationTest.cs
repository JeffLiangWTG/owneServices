using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccTaxOverrideGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAX_CodeWithCompanies()
		{
			string expectedError = "Tax Override Group with the same code already exists.";
			var company1 = GlbCompany.CurrentCompany;
			company1.SetCountry(Core.Constants.CountryCodes.Argentina);
			var branch1C1 = company1.Branches.AddNew();
			branch1C1.GB_Code = "B1";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.SetCountry(Core.Constants.CountryCodes.Argentina);
			Factory.Save();

			var ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			var taxConfigurationC1 = AccountingTestObjectCreator.CreateTaxConfiguration(company1, ledger);
			var taxConfigurationB1C1 = AccountingTestObjectCreator.CreateTaxConfiguration(branch1C1, ledger);

			var taxOverrideGroup1C1 = AccountingTestObjectCreator.CreateTaxOverrideGroup(company1, taxConfigurationC1);
			taxOverrideGroup1C1.AX_Code = "TG1C1";
			AssertNoErrors(taxOverrideGroup1C1.AX_CodeInfo);

			var taxOverrideGroupB1C1 = AccountingTestObjectCreator.CreateTaxOverrideGroup(company1, taxConfigurationB1C1);
			taxOverrideGroupB1C1.AX_Code = "TGB1C1";
			AssertNoErrors(taxOverrideGroupB1C1.AX_CodeInfo);

			Factory.Save();

			var taxConfigurationC2 = AccountingTestObjectCreator.CreateTaxConfiguration(company2, ledger);
			var taxOverrideGroup1C2 = AccountingTestObjectCreator.CreateTaxOverrideGroup(company2, taxConfigurationC2);

			taxOverrideGroup1C2.AX_Code = "TG1C2";
			AssertNoErrors(taxOverrideGroup1C2.AX_CodeInfo);
			Factory.Save();
			taxOverrideGroup1C2.AX_Code = "TG1C1";
			AssertHasError(taxOverrideGroup1C2.AX_CodeInfo, expectedError);
		}

		public void TestValidationOnDuplicateTaxOverrides()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var chargeCode = taxOverrideGroup.ChargeCodes.AddNew();
			var taxOverrideGroupTaxOverride1 = taxOverrideGroup.TaxOverrides.AddNew();
			var taxOverrideGroupTaxOverride2 = taxOverrideGroup.TaxOverrides.AddNew();
			taxOverrideGroupTaxOverride1.AO_Direction = "ALL";
			taxOverrideGroupTaxOverride1.AO_IncoTerm = "ALL";
			taxOverrideGroupTaxOverride1.AO_JobType = "ALL";
			taxOverrideGroupTaxOverride1.AO_Origin = "ALL";
			taxOverrideGroupTaxOverride1.AO_Destination = "ALL";
			taxOverrideGroupTaxOverride1.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideGroupTaxOverride1.AO_CustomsStatus = "ALL";

			taxOverrideGroupTaxOverride2.AO_Direction = "ALL";
			taxOverrideGroupTaxOverride2.AO_IncoTerm = "ALL";
			taxOverrideGroupTaxOverride2.AO_JobType = "ALL";
			taxOverrideGroupTaxOverride2.AO_Origin = "ALL";
			taxOverrideGroupTaxOverride2.AO_Destination = "ALL";
			taxOverrideGroupTaxOverride2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideGroupTaxOverride2.AO_CustomsStatus = "ALL";

			taxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(taxOverrideGroupTaxOverride1);
			AssertHasRowError(taxOverrideGroupTaxOverride2, "You cannot have identical tax overrides.");

			taxOverrideGroupTaxOverride2.AO_JobType = "CST";
			taxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(taxOverrideGroupTaxOverride1);
			AssertNoRowErrors(taxOverrideGroupTaxOverride2);

			var taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_CustomsStatus = "ALL";

			taxOverrideGroup.Validation.ValidateAll();
			AssertHasRowError(chargeCode, "You cannot have identical tax overrides with tax override group.");

			taxOverride.AO_JobType = "CST";
			taxOverrideGroup.Validation.ValidateAll();
			AssertHasRowError(chargeCode, "You cannot have identical tax overrides with tax override group.");

			taxOverride.AO_JobType = "SHP";
			taxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(chargeCode);

			taxOverrideGroupTaxOverride2.AO_JobType = "BRK";
			taxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(chargeCode);

			taxOverride.AO_JobType = "BRK";
			taxOverrideGroup.Validation.ValidateAll();
			AssertHasRowError(chargeCode, "You cannot have identical tax overrides with tax override group.");
		}

		protected AccountingTestObjectCreator AccountingTestObjectCreator
		{
			get
			{
				return accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
			}
		}
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
