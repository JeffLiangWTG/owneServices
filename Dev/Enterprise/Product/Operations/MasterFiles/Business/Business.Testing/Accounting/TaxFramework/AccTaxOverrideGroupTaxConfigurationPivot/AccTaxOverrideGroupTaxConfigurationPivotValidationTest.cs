using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTaxOverrideGroupTaxConfigurationPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAXP_ETC_TaxConfiguration()
		{
			TaxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = ZGuid.Empty;
			TaxOverrideGroupTaxConfigurationPivot.RunPreSaveValidation();
			AssertHasError(TaxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfigurationInfo, "Please enter a Tax Configuration.");

			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			TaxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			TaxOverrideGroupTaxConfigurationPivot.RunPreSaveValidation();
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfigurationInfo);
		}

		public void TestAXP_RateDenominator()
		{
			TaxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = 0;
			TaxOverrideGroupTaxConfigurationPivot.RunPreSaveValidation();
			AssertHasError(TaxOverrideGroupTaxConfigurationPivot.AXP_RateDenominatorInfo, "Please enter a 'Rate Denominator' greater than or equal to 1.");

			TaxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = 1;
			TaxOverrideGroupTaxConfigurationPivot.RunPreSaveValidation();
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_RateDenominatorInfo);
		}

		public void TestAXP_RateNumerator()
		{
			TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = -1;
			TaxOverrideGroupTaxConfigurationPivot.RunPreSaveValidation();
			AssertHasError(TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumeratorInfo, "Please enter a 'Rate Numerator' greater than or equal to 0.");

			TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 1;
			TaxOverrideGroupTaxConfigurationPivot.RunPreSaveValidation();
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumeratorInfo);
		}

		public void TestAXP_TaxAuthorityServiceCode()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var taxConfiguration = Factory.New<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = company.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			var taxSystemAU = AccountingTestObjectCreator.CreateTaxSystem("TAXSYSAU", country: Core.Constants.CountryCodes.Australia);
			var taxSystemBRNotISS = AccountingTestObjectCreator.CreateTaxSystem("TAXSYSBR", country: Core.Constants.CountryCodes.Brazil);
			var taxSystemBRISS = AccountingTestObjectCreator.CreateTaxSystem("ISS", country: Core.Constants.CountryCodes.Brazil);

			var serviceCodeWarning = @"You have entered a separator (space + | + space). The system will split this value into two codes when a transaction that only contains ISS Tax is queued for E-Reporting.
The value before the separator will be treated as the City Code.
The value after the separator will be treated as the Federal Code.
When a transaction contains a PIS tax record the value after the separator will be ignored.";

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode = ZString.Empty;
			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescription = "Air Agency";
			Assert("Precondition: HasErrors", TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo.HasErrors());
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode = "123456 | 1234";
			Assert("Precondition: HasErrors", !TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo.HasErrors());

			TaxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = ZGuid.Empty;
			AssertNull("Precondition: TaxConfiguration", TaxOverrideGroupTaxConfigurationPivot.TaxConfiguration);
			TaxOverrideGroupTaxConfigurationPivot.Validation.ValidateAll();
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			AssertNotNull("Precondition: TaxConfiguration", TaxOverrideGroupTaxConfigurationPivot.TaxConfiguration);
			AssertNull("Precondition: TaxSystem", TaxOverrideGroupTaxConfigurationPivot.TaxConfiguration.TaxSystem);
			TaxOverrideGroupTaxConfigurationPivot.Validation.ValidateAll();
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper().WithGetTaxSystem(Factory, taxSystemAU).WithGetTaxSystems(taxSystemAU)
																			.WithGetTaxSystem(Factory, taxSystemBRNotISS).WithGetTaxSystems(taxSystemBRNotISS)
																			.WithGetTaxSystem(Factory, taxSystemBRISS).WithGetTaxSystems(taxSystemBRISS);

			taxConfiguration.ETC_RN_NKCountry = taxSystemAU.Country;
			taxConfiguration.ETC_TaxSystemCode = taxSystemAU.Code;
			AssertNotNull("Precondition: TaxSystem", TaxOverrideGroupTaxConfigurationPivot.TaxConfiguration.TaxSystem);

			AssertNotEquals("Precondition: Tax System-Country", Core.Constants.CountryCodes.Brazil, taxSystemAU.Country);
			AssertNotEquals("Precondition: Tax System-Code", "ISS", taxSystemAU.Code);
			TaxOverrideGroupTaxConfigurationPivot.Validation.ValidateAll();
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			taxConfiguration.ETC_RN_NKCountry = taxSystemBRNotISS.Country;
			taxConfiguration.ETC_TaxSystemCode = taxSystemBRNotISS.Code;
			AssertEquals("Precondition: Tax System-Country", Core.Constants.CountryCodes.Brazil, taxSystemBRNotISS.Country);
			AssertNotEquals("Precondition: Tax System-Code", "ISS", taxSystemBRNotISS.Code);
			TaxOverrideGroupTaxConfigurationPivot.Validation.ValidateAll();
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			taxConfiguration.ETC_RN_NKCountry = taxSystemBRISS.Country;
			taxConfiguration.ETC_TaxSystemCode = taxSystemBRISS.Code;
			AssertEquals("Precondition: Tax System-Country", Core.Constants.CountryCodes.Brazil, taxSystemBRNotISS.Country);
			AssertEquals("Precondition: Tax System-Code", "ISS", taxSystemBRISS.Code);
			AssertNotEquals("Precondition: ", Core.Constants.CountryCodes.Brazil, TaxOverrideGroupTaxConfigurationPivot.TaxConfiguration.Company.GC_RN_NKCountryCode);
			TaxOverrideGroupTaxConfigurationPivot.Validation.ValidateAll();
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			AssertEquals("Precondition: ", Core.Constants.CountryCodes.Brazil, TaxOverrideGroupTaxConfigurationPivot.TaxConfiguration.Company.GC_RN_NKCountryCode);
			Assert("Precondition: ", TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode.Contains(" | "));
			TaxOverrideGroupTaxConfigurationPivot.Validation.ValidateAll();
			AssertHasWarning(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo, serviceCodeWarning);

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode = "123456 |1234";
			Assert("Precondition: ", !TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode.Contains(" | "));
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode = "123456| 1234";
			Assert("Precondition: ", !TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode.Contains(" | "));
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode = "123456|1234";
			Assert("Precondition: ", !TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode.Contains(" | "));
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode = "123456";
			Assert("Precondition: ", !TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode.Contains(" | "));
			AssertNoWarnings(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);
		}

		public void TestAXP_TaxAuthorityServiceCodeAndDescription()
		{
			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode = ZString.Empty;
			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescription = ZString.Empty;
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescriptionInfo);
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescription = "Air Agency";
			AssertHasError(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo, "Please enter a Service Code.");
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescriptionInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescription = ZString.Empty;
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescriptionInfo);
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCode = "6297";
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);
			AssertHasError(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescriptionInfo, "Please enter a Service Code Description.");

			TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescription = "Air Agency";
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeInfo);
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.AXP_TaxAuthorityServiceCodeDescriptionInfo);
		}

		public void TestAX_TaxAuthorityServiceCodeDescriptionBeforeSave()
		{
			var taxOverrideGroupCode1 = "CODE1";
			var taxOverrideGroupCode2 = "CODE2";
			var taxOverrideGroupDescription1 = "Tax Override Group 1";
			var taxOverrideGroupDescription2 = "Tax Override Group 2";
			var taxAuthorityServiceCode1 = "6297";
			var taxAuthorityServiceCode2 = "6298";
			var taxAuthorityServiceCodeDescription1 = "Air Agency";
			var taxAuthorityServiceCodeDescription2 = "Different Air Agency";

			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			var taxOverrideGroup1 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup1.AX_Code = taxOverrideGroupCode1;
			taxOverrideGroup1.AX_Description = taxOverrideGroupDescription1;

			var taxOverrideGroupTaxConfigurationPivot1 = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot1.AXP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_TaxAuthorityServiceCode = taxAuthorityServiceCode1;
			taxOverrideGroupTaxConfigurationPivot1.AXP_TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription1;

			taxOverrideGroupTaxConfigurationPivot1.Factory.Save();

			var taxOverrideGroup2 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup2.AX_Code = taxOverrideGroupCode2;
			taxOverrideGroup2.AX_Description = taxOverrideGroupDescription2;

			var taxOverrideGroupTaxConfigurationPivot2 = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot2.AXP_AX_TaxOverrideGroup = taxOverrideGroup2.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCode = taxAuthorityServiceCode1;
			taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription2;

			string serviceCodeDescriptionWarning = "Please note: The Service Code Description recorded here differs" +
				" to the description used on other Tax Configuration Override Groups with the same Service Code";

			AssertHasWarning(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo, serviceCodeDescriptionWarning);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);

			taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription1;
			AssertNoWarnings(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);

			taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription2;
			taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCode = taxAuthorityServiceCode2;
			AssertNoWarnings(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);

			taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCode = taxAuthorityServiceCode1;
			AssertHasWarning(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo, serviceCodeDescriptionWarning);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);

			var anotherTaxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			anotherTaxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			anotherTaxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = anotherTaxConfiguration.PK;
			taxOverrideGroupTaxConfigurationPivot2.Validation.ValidateAll();
			AssertNoWarnings(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);

			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			taxOverrideGroupTaxConfigurationPivot2.Validation.ValidateAll();
			AssertHasWarning(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo, serviceCodeDescriptionWarning);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);
		}

		public void TestAX_TaxAuthorityServiceCodeDescriptionAfterSave()
		{
			var taxOverrideGroupCode1 = "CODE1";
			var taxOverrideGroupCode2 = "CODE2";
			var taxOverrideGroupDescription1 = "Tax Override Group 1";
			var taxOverrideGroupDescription2 = "Tax Override Group 2";
			var taxAuthorityServiceCode = "6297";
			var taxAuthorityServiceCodeDescription1 = "Air Agency";
			var taxAuthorityServiceCodeDescription2 = "Different Air Agency";

			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			var taxOverrideGroup1 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup1.AX_Code = taxOverrideGroupCode1;
			taxOverrideGroup1.AX_Description = taxOverrideGroupDescription1;

			var taxOverrideGroupTaxConfigurationPivot1 = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot1.AXP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_TaxAuthorityServiceCode = taxAuthorityServiceCode;
			taxOverrideGroupTaxConfigurationPivot1.AXP_TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription1;

			taxOverrideGroupTaxConfigurationPivot1.Factory.Save();

			var taxOverrideGroup2 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup2.AX_Code = taxOverrideGroupCode2;
			taxOverrideGroup2.AX_Description = taxOverrideGroupDescription2;

			var taxOverrideGroupTaxConfigurationPivot2 = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot2.AXP_AX_TaxOverrideGroup = taxOverrideGroup2.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCode = taxAuthorityServiceCode;
			taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription2;

			string serviceCodeDescriptionWarning = "Please note: The Service Code Description recorded here differs" +
				" to the description used on other Tax Configuration Override Groups with the same Service Code";

			taxOverrideGroupTaxConfigurationPivot2.Factory.Save();

			taxOverrideGroupTaxConfigurationPivot1.Validation.ValidateAll();
			AssertHasWarning(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo, serviceCodeDescriptionWarning);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);

			taxOverrideGroupTaxConfigurationPivot2.Validation.ValidateAll();
			AssertHasWarning(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo, serviceCodeDescriptionWarning);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);

			taxOverrideGroupTaxConfigurationPivot1.AXP_TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription2;
			taxOverrideGroupTaxConfigurationPivot1.Factory.Save();

			taxOverrideGroupTaxConfigurationPivot1.Validation.ValidateAll();
			AssertNoWarnings(taxOverrideGroupTaxConfigurationPivot1.AXP_TaxAuthorityServiceCodeDescriptionInfo);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot1.AXP_TaxAuthorityServiceCodeDescriptionInfo);

			taxOverrideGroupTaxConfigurationPivot2.Validation.ValidateAll();
			AssertNoWarnings(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_TaxAuthorityServiceCodeDescriptionInfo);
		}

		public void TestAXP_AT_TaxID()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_TaxSystemCode = "TEST";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_TaxSystemCode = "TEST";

			var taxOverrideGroup = Factory.New<AccTaxOverrideGroup>();
			AccChargeTaxOverride taxOverride = taxOverrideGroup.TaxOverrides.AddNew();
			var taxOverrideGroupTaxConfigurationPivot1 = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();

			var pivotCollection = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots;
			AssertEquals("Precondition", 1, pivotCollection.Count);
			AssertEquals("Precondition", ZGuid.Empty, taxOverride.AO_AT);
			AssertEquals("Precondition", ZGuid.Empty, taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxID);
			taxOverrideGroupTaxConfigurationPivot1.Validation.ValidateAll();
			AssertHasError(taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxIDInfo, "Please enter a Tax ID.");

			taxOverride.AO_AT = taxRate.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			AssertNoErrors("Tax ID is Null and Tax Configuration NOT Null", taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxIDInfo);

			taxOverride.AO_AT = ZGuid.Empty;
			taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxID = taxRate.PK;
			AssertNoErrors("Tax ID NOT Null, Tax Configuration NOT Null and Tax system codes are same", taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxIDInfo);

			taxRate.AT_TaxSystemCode = "TESTTS";
			taxOverrideGroupTaxConfigurationPivot1.Validation.ValidateAll();
			AssertHasError("Tax ID NOT Null, Tax Configuration NOT Null and Tax system codes are different", taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxIDInfo, "Enter a valid Tax ID.");

			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = ZGuid.Empty;
			AssertNoErrors("Tax ID NOT Null and Tax Configuration is Null", taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxIDInfo);

			taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxID = Guid.NewGuid();
			AssertHasError(taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxIDInfo, "Enter a valid Tax ID.");

			taxOverride.AO_AT = taxRate.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxID = ZGuid.Empty;
			AssertNoErrors("Tax ID is Null and Tax Configuration is Null", taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxIDInfo);

			var taxOverrideGroupTaxConfigurationPivot2 = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			AssertEquals("Precondition: pivots collection", 2, pivotCollection.Count);
			AssertEquals(ZGuid.Empty, taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxID);
			AssertEquals(ZGuid.Empty, taxOverrideGroupTaxConfigurationPivot2.AXP_AT_TaxID);

			taxOverrideGroupTaxConfigurationPivot1.Validation.ValidateAll();
			taxOverrideGroupTaxConfigurationPivot2.Validation.ValidateAll();
			AssertHasError(taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxIDInfo, "Please enter a Tax ID.");
			AssertHasError(taxOverrideGroupTaxConfigurationPivot2.AXP_AT_TaxIDInfo, "Please enter a Tax ID.");

			taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxID = taxRate.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_AT_TaxID = taxRate.PK;

			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxIDInfo);
			AssertNoErrors(taxOverrideGroupTaxConfigurationPivot2.AXP_AT_TaxIDInfo);
		}

		public void TestAXP_A9_DefaultVATClass()
		{
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			var taxOverrideGroup = Factory.New<AccTaxOverrideGroup>();
			var taxOverrideGroupTaxConfigurationPivot = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();

			taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClass = ZGuid.Empty;
			AssertNoErrors("Tax ID is Empty and Tax Message is Empty", taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClassInfo);

			taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClass = taxMessage.PK;
			AssertHasError("Tax ID is Empty and Tax Message NOT Empty", taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClassInfo, "You must have a Tax ID before you can choose a Tax Message");

			taxOverrideGroupTaxConfigurationPivot.AXP_AT_TaxID = taxRate.PK;
			AssertNoErrors("Tax ID NOT Empty and Tax Message NOT Empty", taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClassInfo);

			taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClass = Guid.NewGuid();
			AssertHasError(taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClassInfo, "Enter a valid Tax Message.");

			taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClass = ZGuid.Empty;
			AssertNoErrors("Tax ID NOT Empty and Tax Message is Empty", taxOverrideGroupTaxConfigurationPivot.AXP_A9_DefaultVATClassInfo);
		}

		public void TestCallingTaxRateValidationOnRateNumeratorSetter()
		{
			var expectedError = "Rate must be less than 100%. Please correct the Numerator and Denominator entered.";
			TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 0;
			AssertEquals("Precondition", 0m, TaxOverrideGroupTaxConfigurationPivot.RateInfo.Value);

			TaxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = 2;
			TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 100;
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.RateInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 200;
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.RateInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 230;
			AssertHasError(TaxOverrideGroupTaxConfigurationPivot.RateInfo, expectedError);
		}

		public void TestCallingTaxRateValidationOnRateDenominatorSetter()
		{
			var expectedError = "Rate must be less than 100%. Please correct the Numerator and Denominator entered.";
			TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 0;
			AssertEquals("Precondition", 0m, TaxOverrideGroupTaxConfigurationPivot.RateInfo.Value);

			TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 220;

			TaxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = 3;
			AssertNoErrors(TaxOverrideGroupTaxConfigurationPivot.RateInfo);

			TaxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = 2;
			AssertHasError(TaxOverrideGroupTaxConfigurationPivot.RateInfo, expectedError);

			TaxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = 1;
			AssertHasError(TaxOverrideGroupTaxConfigurationPivot.RateInfo, expectedError);
		}

		public void TestCheckCalculatedRateOnCallingValidateAll()
		{
			var expectedError = "Rate must be less than 100%. Please correct the Numerator and Denominator entered.";

			using (TaxOverrideGroupTaxConfigurationPivot.GetValidationSuspender())
			{
				TaxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 2550;
				TaxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = 3;
			}
			AssertNoErrors("Precondition", TaxOverrideGroupTaxConfigurationPivot.RateInfo);
			TaxOverrideGroupTaxConfigurationPivot.RunPreSaveValidation();

			AssertHasErrors(expectedError, TaxOverrideGroupTaxConfigurationPivot.RateInfo);
		}

		AccTaxOverrideGroupTaxConfigurationPivot TaxOverrideGroupTaxConfigurationPivot
		{
			get
			{
				if (taxOverrideGroupTaxConfigurationPivot == null)
				{
					var taxOverrideGroup = Factory.New<AccTaxOverrideGroup>();
					taxOverrideGroupTaxConfigurationPivot = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
					taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
				}

				return taxOverrideGroupTaxConfigurationPivot;
			}
		}

		AccTaxOverrideGroupTaxConfigurationPivot taxOverrideGroupTaxConfigurationPivot;
		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
