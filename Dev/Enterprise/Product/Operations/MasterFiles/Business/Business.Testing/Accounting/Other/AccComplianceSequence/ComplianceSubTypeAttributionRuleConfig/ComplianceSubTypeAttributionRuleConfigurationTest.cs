using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleConfiguration))]
	class ComplianceSubTypeAttributionRuleConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestIsTaxRegistrationTypeSupported()
		{
			var config = new ComplianceSubTypeAttributionRuleConfiguration();
			var allCountries = new RefCountryCollection(Factory);

			var countriesNotImplementInterface = new ZString[] {
				Constants.CountryCodes.Peru
			};

			CombineAssertions(() =>
			{
				foreach (var country in allCountries)
				{
					config.Country = country.Code;

					if (CountryComplianceFactory.GetIComplianceSubTypeTaxRegistrationTypeRuleProvider(config.Country) != null || countriesNotImplementInterface.Contains(config.Country))
					{
						Assert(country.Description + ": " + "Must be added to TaxRegistrationTypeSupported property", config.IsTaxRegistrationTypeSupported);
					}
					else
					{
						Assert(country.Description + ": " + "We must implement IComplianceSubTypeTaxRegistrationTypeRuleProvider in " + country.Description + "ComplianceInfo.cs", !config.IsTaxRegistrationTypeSupported);
					}
				}
			});
		}

		public void TestLedgerTypeList()
		{
			AssertEquals("LedgerTypeList.Count", 2, BizObj.Lookups.LedgerTypeList.Count);
			AssertEquals("should contain AccountsReceivable", true, BizObj.Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsReceivable));
			AssertEquals("should contain AccountsPayable", true, BizObj.Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsPayable));
		}

		public void TestInvoiceTypeList()
		{
			AssertEquals("InvoiceTypeList.Count", 3, BizObj.Lookups.InvoiceTypeList.Count);
			AssertEquals("should contain Invoice", true, BizObj.Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Invoice));
			AssertEquals("should contain CreditNote", true, BizObj.Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.CreditNote));
			AssertEquals("should contain AdjustmentNote", true, BizObj.Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.AdjustmentNote));
		}

		public void TestClearTaxRegistrationTypeIfNotTaxRegistrationTypeSupported()
		{
			ComplianceSubTypeAttributionRuleConfiguration config = SetUpArgentinaConfiguration();
			AssertEquals(true, config.IsTaxRegistrationTypeSupported);
			AssertEquals("REC", config.TaxRegistrationType);
			Assert("expect TaxRegistrationType is not readonly when country is Argentina", !config.TaxRegistrationTypeInfo.ReadOnly);

			config.Country = "AU";
			AssertEquals("expect empty string when country is not Argentina", "", config.TaxRegistrationType);
			Assert("expect TaxRegistrationType is readonly when country is not Argentina", config.TaxRegistrationTypeInfo.ReadOnly);

			//Peru
			config = SetUpPeruConfiguration();
			AssertEquals(true, config.IsTaxRegistrationTypeSupported);
			AssertEquals("DNI", config.TaxRegistrationType);
			Assert("expect TaxRegistrationType is not readonly when country is Peru", !config.TaxRegistrationTypeInfo.ReadOnly);

			config.Country = "AU";
			AssertEquals("expect empty string when country is not Peru", "", config.TaxRegistrationType);
			Assert("expect TaxRegistrationType is readonly when country is not Peru", config.TaxRegistrationTypeInfo.ReadOnly);

			// Turkey
			config = SetUpTurkeyConfiguration();
			AssertEquals(true, config.IsTaxRegistrationTypeSupported);
			AssertEquals("ERO", config.TaxRegistrationType);
			Assert("expect TaxRegistrationType is not readonly when country is Turkey", !config.TaxRegistrationTypeInfo.ReadOnly);

			config.Country = "AU";
			AssertEquals("expect empty string when country is not Turkey", "", config.TaxRegistrationType);
			Assert("expect TaxRegistrationType is readonly when country is not Turkey", config.TaxRegistrationTypeInfo.ReadOnly);

			//Dominican Republic
			config = SetUpDominicanRepublicConfiguration();
			AssertEquals(true, config.IsTaxRegistrationTypeSupported);
			AssertEquals("RCS", config.TaxRegistrationType);
			Assert("expect TaxRegistrationType is not readonly when country is Dominican Republic", !config.TaxRegistrationTypeInfo.ReadOnly);

			config.Country = "AU";
			AssertEquals("expect empty string when country is not Dominican Republic", "", config.TaxRegistrationType);
			Assert("expect TaxRegistrationType is readonly when country is not Dominican Republic", config.TaxRegistrationTypeInfo.ReadOnly);
		}

		public void TestValidateTaxTaxRegistrationType()
		{
			ComplianceSubTypeAttributionRuleConfiguration config = SetUpArgentinaConfiguration();
			AssertNoErrors("Precondition: TaxRegistrationType should not have errors.", configuration.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "";
			AssertHasErrors("TaxRegistrationType can't be blank", config.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "ABC";
			AssertHasErrors("TaxRegistrationType has invalid code", config.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "NOT";
			Assert(!config.TaxRegistrationTypeInfo.HasErrors());

			config.Country = "CN";
			AssertNoErrors("Precondition: TaxRegistrationType should not have errors.", configuration.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "";
			AssertHasErrors("TaxRegistrationType can't be blank", config.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "ABC";
			AssertHasErrors("TaxRegistrationType has invalid code", config.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "NOT";
			Assert(!config.TaxRegistrationTypeInfo.HasErrors());

			//Peru
			config = SetUpPeruConfiguration();
			AssertNoErrors("Precondition: TaxRegistrationType should not have errors.", configuration.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "";
			AssertNoErrors("TaxRegistrationType can be blank", configuration.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "ABC";
			AssertHasErrors("TaxRegistrationType has invalid code", config.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "DNI";
			Assert(!config.TaxRegistrationTypeInfo.HasErrors());

			//Dominican Republic
			config = SetUpDominicanRepublicConfiguration();
			AssertNoErrors("Precondition: TaxRegistrationType should not have errors.", configuration.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "";
			AssertNoErrors("TaxRegistrationType can be blank", configuration.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = "ABC";
			AssertHasErrors("TaxRegistrationType has invalid code", config.TaxRegistrationTypeInfo);

			config.TaxRegistrationType = DominicanRepublicComplianceInfo.TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			Assert(!config.TaxRegistrationTypeInfo.HasErrors());

			config.TaxRegistrationType = DominicanRepublicComplianceInfo.TaxRegistrationTypeCodes.GovernmentOrganizations;
			Assert(!config.TaxRegistrationTypeInfo.HasErrors());
		}

		public void TestValidateSubType()
		{
			AssertNoErrors("Precondition: SubType should not have errors.", configuration.SubTypeInfo);

			configuration.SubType = "";
			AssertHasErrors("SubType can't be blank", configuration.SubTypeInfo);

			configuration.SubType = "AAA";
			AssertHasErrors("SubType has invalid code", configuration.SubTypeInfo);

			configuration.SubType = "TXI";
			Assert(!configuration.SubTypeInfo.HasErrors());
		}

		public void TestValidateParentTransactionComplianceSubType()
		{
			AssertNoErrors("Precondition: ParentSubType should not have errors.", configuration.ParentTransactionSubTypeInfo);

			configuration.ParentTransactionSubType = "AAA";
			AssertHasErrors("ParentSubType has invalid code", configuration.ParentTransactionSubTypeInfo);

			configuration.ParentTransactionSubType = "TXI";
			Assert(!configuration.ParentTransactionSubTypeInfo.HasErrors());
		}

		public void TestValidateLedgerType()
		{
			AssertNoErrors("Precondition: LedgerType should not have errors.", configuration.LedgerTypeInfo);

			configuration.LedgerType = "";
			AssertHasErrors("LedgerType can't be blank", configuration.LedgerTypeInfo);

			configuration.LedgerType = "AA";
			AssertHasErrors("LedgerType has invalid code", configuration.LedgerTypeInfo);

			configuration.LedgerType = "AR";
			Assert(!configuration.LedgerTypeInfo.HasErrors());
		}

		public void TestValidateInvoiceType()
		{
			AssertNoErrors("Precondition: InvoiceType should not have errors.", configuration.InvoiceTypeInfo);

			configuration.InvoiceType = "";
			AssertHasErrors("InvoiceType can't be blank", configuration.InvoiceTypeInfo);

			configuration.InvoiceType = "AAA";
			AssertHasErrors("InvoiceType has invalid code", configuration.InvoiceTypeInfo);

			configuration.InvoiceType = "INV";
			Assert(!configuration.InvoiceTypeInfo.HasErrors());
		}

		public void TestValidateTaxInvoiceRule()
		{
			AssertNoErrors("Precondition: TaxInvoiceRule should not have errors.", configuration.TaxInvoiceRuleInfo);

			configuration.TaxInvoiceRule = "";
			AssertHasErrors("TaxInvoiceRule can't be blank", configuration.TaxInvoiceRuleInfo);

			configuration.TaxInvoiceRule = "AAA";
			AssertHasErrors("InvoiceType has invalid code", configuration.TaxInvoiceRuleInfo);

			configuration.TaxInvoiceRule = "AMT";
			Assert(!configuration.TaxInvoiceRuleInfo.HasErrors());
		}

		public void TestValidateTaxIDCode()
		{
			AssertNoErrors("Precondition: TaxIDCode should not have errors.", configuration.TaxIDCodeInfo);

			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.SpecificTaxIDs;
			configuration.TaxIDCode = "";
			AssertHasError("TaxIDCode can't be blank when TaxInvoiceRule is specified as 'STI'", configuration.TaxIDCodeInfo, "Please enter a Tax ID Code.");

			configuration.TaxIDCode = "AAA";
			AssertHasError("TaxIDCode has invalid code", configuration.TaxIDCodeInfo, "Invalid Tax ID : AAA");

			configuration.TaxIDCode = "AAA, BBB";
			AssertHasError("TaxIDCode has invalid code", configuration.TaxIDCodeInfo, "Invalid Tax ID : AAA, BBB");

			configuration.Country = GlbCompany.CurrentCompany.Country.Code;
			var taxRates = Factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, configuration.Country));
			configuration.TaxIDCode = $"{taxRates[0].AT_Code}, {taxRates[1].AT_Code}";
			Assert(!configuration.TaxInvoiceRuleInfo.HasErrors());

			var configuration1 = GetBusinessObjectToClone() as ComplianceSubTypeAttributionRuleConfiguration;
			configurations.Add(configuration1);
			configuration1.Country = GlbCompany.CurrentCompany.Country.Code;
			configuration1.TaxInvoiceRule = TaxInvoiceRuleCodes.SpecificTaxIDs;
			configuration1.TaxIDCode = $"{taxRates[2].AT_Code}, {taxRates[0].AT_Code}";
			AssertHasError("TaxIDCode has overlapping codes", configuration1.TaxIDCodeInfo, "overlapped configurations detected");
			AssertNoError("TaxIDCode is not equal to existing", configuration1.TaxIDCodeInfo, "configuration with identical criteria already exists!");

			configuration1.TaxIDCode = configuration.TaxIDCode;
			AssertHasError("TaxIDCode is equal to existing", configuration1.TaxIDCodeInfo, "configuration with identical criteria already exists!");
			AssertNoError("TaxIDCode is overlapping but equals message is shown first", configuration1.TaxIDCodeInfo, "overlapped configurations detected");
		}

		public void TestValidateOriginalRule()
		{
			AssertNoErrors("Precondition: OriginalRule should not have errors.", configuration.OriginalRuleInfo);

			configuration.OriginalRule = "";
			AssertHasErrors("OriginalRule can't be blank", configuration.OriginalRuleInfo);

			configuration.OriginalRule = "AAA";
			AssertHasErrors("OriginalRule has invalid code", configuration.OriginalRuleInfo);

			configuration.OriginalRule = "ALL";
			Assert(!configuration.OriginalRuleInfo.HasErrors());
		}

		public void TestValidateDisbursementRule()
		{
			AssertNoErrors("Precondition: DisbursementRule should not have errors.", configuration.DisbursementRuleInfo);

			configuration.DisbursementRule = "";
			AssertHasErrors("DisbursementRule can't be blank", configuration.DisbursementRuleInfo);

			configuration.DisbursementRule = "AAA";
			AssertHasErrors("DisbursementRule has invalid code", configuration.DisbursementRuleInfo);

			configuration.DisbursementRule = "ALL";
			Assert(!configuration.DisbursementRuleInfo.HasErrors());
		}

		public void TestValidateSelfBillingRule()
		{
			AssertNoErrors("Precondition: Self Billing Rule should not have errors.", configuration.SelfBillingRuleInfo);

			configuration.SelfBillingRule = "";
			Assert("Self Billing Rule can be blank", !configuration.SelfBillingRuleInfo.HasErrors());

			configuration.SelfBillingRule = "AAA";
			AssertHasErrors("DisbursementRule has invalid code", configuration.SelfBillingRuleInfo);

			configuration.SelfBillingRule = "SBI";
			Assert(!configuration.SelfBillingRuleInfo.HasErrors());

			configuration.SelfBillingRule = "AAA";
			AssertHasErrors("DisbursementRule has invalid code", configuration.SelfBillingRuleInfo);

			configuration.SelfBillingRule = "STD";
			Assert(!configuration.SelfBillingRuleInfo.HasErrors());

			configuration.SelfBillingRule = "AAA";
			AssertHasErrors("DisbursementRule has invalid code", configuration.SelfBillingRuleInfo);
		}

		public void TestValidateVatGroupRule()
		{
			AssertNoErrors("Precondition: Self Billing Rule should not have errors.", configuration.VATGroupRuleInfo);

			configuration.VATGroupRule = "";
			Assert("VAT Group Rule can be blank", !configuration.VATGroupRuleInfo.HasErrors());

			configuration.VATGroupRule = "AAA";
			AssertHasErrors("VAT Group Rule has invalid code", configuration.VATGroupRuleInfo);

			configuration.VATGroupRule = "VGM";
			Assert(!configuration.VATGroupRuleInfo.HasErrors());

			configuration.VATGroupRule = "XXX";
			AssertHasErrors("VAT Group Rule has invalid code", configuration.VATGroupRuleInfo);

			configuration.VATGroupRule = "EVG";
			Assert(!configuration.VATGroupRuleInfo.HasErrors());
		}

		public void TestValidateTaxRegistrationLocationRule()
		{
			AssertNoErrors("Precondition: Tax Registration Location Rule should not have errors.", configuration.TaxRegistrationLocationRuleInfo);

			configuration.Country = Core.Constants.CountryCodes.Italy; // Set to EU country

			configuration.TaxRegistrationLocationRule = "";
			Assert("Tax Registration Location Rule can be blank", !configuration.TaxRegistrationLocationRuleInfo.HasErrors());

			configuration.TaxRegistrationLocationRule = "AAA";
			AssertHasErrors("Tax Registration Location Rule has invalid code", configuration.TaxRegistrationLocationRuleInfo);

			configuration.TaxRegistrationLocationRule = "NEU";
			Assert(!configuration.TaxRegistrationLocationRuleInfo.HasErrors());

			configuration.TaxRegistrationLocationRule = "AAA";
			AssertHasErrors("DisbursementRule has invalid code", configuration.TaxRegistrationLocationRuleInfo);

			configuration.TaxRegistrationLocationRule = "IT";
			Assert(!configuration.TaxRegistrationLocationRuleInfo.HasErrors());

			configuration.TaxRegistrationLocationRule = "AAA";
			AssertHasErrors("DisbursementRule has invalid code", configuration.TaxRegistrationLocationRuleInfo);

			configuration.TaxRegistrationLocationRule = "EUX";
			Assert(!configuration.TaxRegistrationLocationRuleInfo.HasErrors());

			configuration.Country = Core.Constants.CountryCodes.KoreaSouth; // Set to Non EU Country

			configuration.TaxRegistrationLocationRule = "";
			Assert("Tax Registration Location Rule can be blank", !configuration.TaxRegistrationLocationRuleInfo.HasErrors());

			configuration.TaxRegistrationLocationRule = "AAA";
			AssertHasErrors("Tax Registration Location Rule has invalid code", configuration.TaxRegistrationLocationRuleInfo);

			configuration.TaxRegistrationLocationRule = "NEU";
			AssertHasErrors("Tax Registration Location Rule has invalid code", configuration.TaxRegistrationLocationRuleInfo);

			configuration.TaxRegistrationLocationRule = "AAA";
			AssertHasErrors("Tax Registration Location Rule has invalid code", configuration.TaxRegistrationLocationRuleInfo);

			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Australia;
			AssertHasErrors("Tax Registration Location Rule has invalid code", configuration.TaxRegistrationLocationRuleInfo);

			configuration.TaxRegistrationLocationRule = "AAA";
			AssertHasErrors("DisbursementRule has invalid code", configuration.TaxRegistrationLocationRuleInfo);

			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.KoreaSouth;
			Assert("Non EU Countries can nominate their own code", !configuration.TaxRegistrationLocationRuleInfo.HasErrors());
		}

		public void TestTaxSystemList()
		{
			BizObj.Country = Constants.CountryCodes.Brazil;
			CreateTaxSystemConfigurationForBrazil(Factory);
			AssertEquals("TaxSystemList.Count", 3, BizObj.Lookups.TaxSystemList.Count);
			AssertEquals("should contain empty", true, BizObj.Lookups.TaxSystemList.ContainsCode(string.Empty));
			AssertEquals("should contain ISS", true, BizObj.Lookups.TaxSystemList.ContainsCode("ISS"));
			AssertEquals("should contain INSS", true, BizObj.Lookups.TaxSystemList.ContainsCode("INSS"));
		}

		public void TestValidateRequiredTaxSystem()
		{
			AssertNoErrors("Precondition: Required Tax System should not have errors.", configuration.RequiredTaxSystemInfo);

			configuration.Country = Core.Constants.CountryCodes.Brazil;
			CreateTaxSystemConfigurationForBrazil(Factory);

			configuration.RequiredTaxSystem = "";
			Assert("Required Tax System can be blank", !configuration.TaxRegistrationLocationRuleInfo.HasErrors());

			configuration.RequiredTaxSystem = "AAA";
			AssertHasErrors("Required Tax System has invalid code", configuration.RequiredTaxSystemInfo);

			configuration.RequiredTaxSystem = "ISS";
			AssertNoErrors(configuration.RequiredTaxSystemInfo);

			configuration.RequiredTaxSystem = "";
			configuration.ExcludedTaxSystem = "ISS";

			configuration.RequiredTaxSystem = "ISS";
			AssertHasErrors("Cannot require and exclude same Tax System", configuration.RequiredTaxSystemInfo);
		}

		public void TestValidateExcludedTaxSystem()
		{
			AssertNoErrors("Precondition: Excluded Tax System should not have errors.", configuration.ExcludedTaxSystemInfo);

			configuration.Country = Core.Constants.CountryCodes.Brazil;
			CreateTaxSystemConfigurationForBrazil(Factory);
			configuration.ExcludedTaxSystem = "";
			Assert("Excluded Tax System can be blank", !configuration.TaxRegistrationLocationRuleInfo.HasErrors());

			configuration.ExcludedTaxSystem = "AAA";
			AssertHasErrors("Excluded Tax System has invalid code", configuration.ExcludedTaxSystemInfo);

			configuration.ExcludedTaxSystem = "ISS";
			AssertNoErrors(configuration.ExcludedTaxSystemInfo);

			configuration.ExcludedTaxSystem = "";
			configuration.RequiredTaxSystem = "ISS";

			configuration.ExcludedTaxSystem = "ISS";
			AssertHasErrors("Cannot require and exclude same Tax System", configuration.ExcludedTaxSystemInfo);
		}

		public void TestRegistrationCodeList()
		{
			BizObj.Country = Constants.CountryCodes.Argentina;
			AssertEquals("RegistrationCodeList.Count", 70, BizObj.Lookups.RegistrationCodeList.Count);
		}

		public void TestValidateRequiredRegistrationCode()
		{
			AssertNoErrors("Precondition: Required Registration should not have errors.", configuration.RequiredRegistrationCodeInfo);

			configuration.Country = Core.Constants.CountryCodes.Argentina;

			configuration.RequiredRegistrationCode = "";
			AssertNoErrors("Required Registration can be blank", configuration.RequiredRegistrationCodeInfo);

			configuration.RequiredRegistrationCode = "XXX";
			AssertHasError("Required Registration has invalid code", configuration.RequiredRegistrationCodeInfo, "Enter a valid selection.");

			configuration.RequiredRegistrationCode = "MIP";
			AssertNoErrors(configuration.RequiredRegistrationCodeInfo);

			configuration.RequiredRegistrationCode = "";
			configuration.ExcludedRegistrationCode = "MIP";
			AssertNoErrors(configuration.RequiredRegistrationCodeInfo);

			configuration.RequiredRegistrationCode = "MIP";
			AssertHasError("Cannot require and exclude same Registration", configuration.RequiredRegistrationCodeInfo,
				"The same code cannot be selected for RequiredRegistrationCode and ExcludedRegistrationCode");
		}

		public void TestValidateExcludedRegistrationCode()
		{
			AssertNoErrors("Precondition: Required Registration should not have errors.", configuration.ExcludedRegistrationCodeInfo);

			configuration.Country = Core.Constants.CountryCodes.Argentina;

			configuration.ExcludedRegistrationCode = "";
			AssertNoErrors("Excluded Registration can be blank", configuration.ExcludedRegistrationCodeInfo);

			configuration.ExcludedRegistrationCode = "XXX";
			AssertHasError("Excluded Registration has invalid code", configuration.ExcludedRegistrationCodeInfo, "Enter a valid selection.");

			configuration.ExcludedRegistrationCode = "MIP";
			AssertNoErrors(configuration.ExcludedRegistrationCodeInfo);

			configuration.ExcludedRegistrationCode = "";
			configuration.RequiredRegistrationCode = "MIP";
			AssertNoErrors(configuration.ExcludedRegistrationCodeInfo);

			configuration.ExcludedRegistrationCode = "MIP";
			AssertHasError("Cannot require and exclude same Registration", configuration.ExcludedRegistrationCodeInfo,
				"The same code cannot be selected for RequiredRegistrationCode and ExcludedRegistrationCode");
		}

		public void TestValidateOrganisationCategory()
		{
			AssertNoErrors("Precondition: Organisation Category should not have errors.", configuration.OrganisationCategoryInfo);

			configuration.OrganisationCategory = "";
			AssertNoErrors("Organisation Category can be blank", configuration.OrganisationCategoryInfo);

			configuration.OrganisationCategory = "AAA";
			AssertHasErrors("Organisation Category has invalid code", configuration.OrganisationCategoryInfo);

			configuration.OrganisationCategory = OrgConstants.Category.Government;
			AssertNoErrors(configuration.OrganisationCategoryInfo);
		}

		public void TestCheckIdenticalConfiguration_ExcludedRegistrationCode()
		{
			var config1 = SetUpArgentinaConfiguration();
			config1.ExcludedRegistrationCode = "MIP";
			configurations.Add(config1);

			var config2 = SetUpArgentinaConfiguration();
			configurations.Add(config2);
			AssertNoErrors("Precondition: OriginalRule should not have errors.", config2.ExcludedRegistrationCodeInfo);

			config2.ExcludedRegistrationCode = "MIP";
			AssertHasError(config2.ExcludedRegistrationCodeInfo, "configuration with identical criteria already exists!");

			config2.ExcludedRegistrationCode = "DNI";
			AssertNoErrors(config2.ExcludedRegistrationCodeInfo);

			config2.RequiredRegistrationCode = "";
			AssertNoErrors(config2.ExcludedRegistrationCodeInfo);
		}

		public void TestCheckIdenticalConfiguration_RequiredRegistrationCode()
		{
			var config1 = SetUpArgentinaConfiguration();
			config1.RequiredRegistrationCode = "MIP";
			configurations.Add(config1);

			var config2 = SetUpArgentinaConfiguration();
			configurations.Add(config2);
			AssertNoErrors("Precondition: OriginalRule should not have errors.", config2.RequiredRegistrationCodeInfo);

			config2.RequiredRegistrationCode = "MIP";
			AssertHasError(config2.RequiredRegistrationCodeInfo, "configuration with identical criteria already exists!");

			config2.RequiredRegistrationCode = "DNI";
			AssertNoErrors(config2.RequiredRegistrationCodeInfo);

			config2.RequiredRegistrationCode = "";
			AssertNoErrors(config2.RequiredRegistrationCodeInfo);
		}

		public void TestCheckOverlappedConfigurationExist_RequiredRegistrationCode()
		{
			var config1 = SetUpArgentinaConfiguration();
			config1.RequiredRegistrationCode = "MIP";
			configurations.Add(config1);

			var config2 = SetUpArgentinaConfiguration();
			config2.OriginalRule = "ARO";
			configurations.Add(config2);

			AssertNoErrors("Precondition: OriginalRule should not have errors.", config2.RequiredRegistrationCodeInfo);

			config2.RequiredRegistrationCode = "MIP";
			AssertHasError(config2.RequiredRegistrationCodeInfo, "overlapped configurations detected");
		}

		public void TestCheckOverlappedConfigurationExist_ExcludeRegistrationCode()
		{
			var config1 = SetUpArgentinaConfiguration();
			config1.ExcludedRegistrationCode = "MIP";
			configurations.Add(config1);

			var config2 = SetUpArgentinaConfiguration();
			config2.OriginalRule = "ARO";
			configurations.Add(config2);

			AssertNoErrors("Precondition: OriginalRule should not have errors.", config2.ExcludedRegistrationCodeInfo);

			config2.ExcludedRegistrationCode = "MIP";
			AssertHasError(config2.ExcludedRegistrationCodeInfo, "overlapped configurations detected");
		}

		public void TestDuplicateConfiurationValidationForSelfBilling()
		{
			ComplianceSubTypeAttributionRuleConfigurationCollection configurations = new ComplianceSubTypeAttributionRuleConfigurationCollection();

			ComplianceSubTypeAttributionRuleConfiguration newConfig = configurations.AddNew();
			newConfig.Country = "PE";
			newConfig.SubType = "TXI";
			newConfig.LedgerType = "AR";
			newConfig.InvoiceType = "INV";
			newConfig.TaxInvoiceRule = "TID";
			newConfig.DisbursementRule = "DSB";
			newConfig.OriginalRule = "ARO";
			newConfig.OrganisationLocation = "PE";
			newConfig.SelfBillingRule = "";

			ComplianceSubTypeAttributionRuleConfiguration newConfig2 = configurations.AddNew();
			newConfig2.Country = "PE";
			newConfig2.SubType = "TXI";
			newConfig2.LedgerType = "AR";
			newConfig2.InvoiceType = "INV";
			newConfig2.TaxInvoiceRule = "TID";
			newConfig2.DisbursementRule = "DSB";
			newConfig2.OriginalRule = "ARO";
			newConfig2.OrganisationLocation = "PE";
			newConfig2.SelfBillingRule = "SBI";

			newConfig.RunPreSaveValidation();
			AssertNoErrors("Shouldn't be any errors because self billing rule is different", newConfig);

			newConfig2.SelfBillingRule = "";
			newConfig.RunPreSaveValidation();
			AssertHasErrors(newConfig.SelfBillingRuleInfo);
		}

		public void TestCheckIdenticalConfigurationExist()
		{
			ComplianceSubTypeAttributionRuleConfiguration newConfig = configurations.AddNew();
			newConfig.Country = "PE";
			newConfig.SubType = "TXI";
			newConfig.LedgerType = "AR";
			newConfig.InvoiceType = "INV";
			newConfig.TaxInvoiceRule = "TID";
			newConfig.DisbursementRule = "DSB";
			newConfig.OriginalRule = "ARO";
			newConfig.OrganisationLocation = "PE";
			AssertNoErrors("Precondition: OriginalRule should not have errors.", newConfig.OriginalRuleInfo);

			newConfig.DisbursementRule = "NDB";
			newConfig.OriginalRule = "OTO";
			AssertHasError(newConfig.OriginalRuleInfo, "configuration with identical criteria already exists!");
			newConfig.OriginalRule = "ARO";
			AssertNoErrors("OriginalRule should not have errors.", newConfig.OriginalRuleInfo);

			newConfig.OriginalRule = "OTO";
			AssertHasError(newConfig.OriginalRuleInfo, "configuration with identical criteria already exists!");
			newConfig.OrganisationLocation = "VN";
			AssertNoErrors("OrganisationLocation should not have errors.", newConfig.OrganisationLocationInfo);

			newConfig.OriginalRule = "ARO";
			AssertNoErrors("OriginalRule should not have errors.", newConfig.OriginalRuleInfo);

			newConfig.OriginalRule = "OTO";
			AssertNoErrors("OriginalRule should not have errors.", newConfig.OriginalRuleInfo);

			CreateTaxSystemConfigurationForBrazil(Factory);

			newConfig.OrganisationLocation = "PE";
			newConfig.ValidateOriginalRule();
			AssertHasError(newConfig.OriginalRuleInfo, "configuration with identical criteria already exists!");

			configuration.Country = Core.Constants.CountryCodes.Brazil;
			newConfig.Country = Core.Constants.CountryCodes.Brazil;
			configuration.RequiredTaxSystem = "ISS";
			AssertNoErrors("RequiredTaxSystem should not have errors.", configuration.RequiredTaxSystemInfo);

			newConfig.RequiredTaxSystem = "ISS";
			AssertHasError(newConfig.RequiredTaxSystemInfo, "configuration with identical criteria already exists!");
			newConfig.RequiredTaxSystem = "INSS";
			AssertNoErrors("RequiredTaxSystem should not have errors.", newConfig.RequiredTaxSystemInfo);

			configuration.RequiredTaxSystem = "";
			AssertNoErrors("RequiredTaxSystem should not have errors.", configuration.RequiredTaxSystemInfo);
			newConfig.RequiredTaxSystem = "";
			AssertHasError(newConfig.RequiredTaxSystemInfo, "configuration with identical criteria already exists!");

			configuration.ExcludedTaxSystem = "ISS";
			AssertNoErrors("RequiredTaxSystem should not have errors.", configuration.ExcludedTaxSystemInfo);

			newConfig.ExcludedTaxSystem = "ISS";
			AssertHasError(newConfig.ExcludedTaxSystemInfo, "configuration with identical criteria already exists!");
			newConfig.ExcludedTaxSystem = "INSS";
			AssertNoErrors("ExcludedTaxSystem should not have errors.", newConfig.ExcludedTaxSystemInfo);
		}

		public void TestCheckIdenticalConfigurationExporterExemption()
		{
			var config1 = configurations.AddNew();
			config1.Country = "PE";
			config1.SubType = "TXI";
			config1.LedgerType = "AR";
			config1.InvoiceType = "INV";
			config1.TaxInvoiceRule = "TID";
			config1.DisbursementRule = "DSB";
			config1.OriginalRule = "ARO";
			config1.OrganisationLocation = "PE";

			AssertNoErrors(config1.ExporterExemptionInfo);

			var config2 = configurations.AddNew();
			config2.Country = "PE";
			config2.SubType = "TXI";
			config2.LedgerType = "AR";
			config2.InvoiceType = "INV";
			config2.TaxInvoiceRule = "TID";
			config2.DisbursementRule = "DSB";
			config2.OriginalRule = "ARO";
			config2.OrganisationLocation = "PE";

			config1.ExporterExemption = "XXX";

			AssertHasError(config1.ExporterExemptionInfo, "Enter a valid selection.");

			config1.ExporterExemption = "EXV";
			AssertHasError(config1.ExporterExemptionInfo, "overlapped configurations detected");

			config2.ExporterExemption = "EXV";
			AssertHasError(config2.ExporterExemptionInfo, "configuration with identical criteria already exists!");

			config2.ExporterExemption = "NON";
			AssertNoErrors(config2.ExporterExemptionInfo);

			config2.ExporterExemption = "";
			AssertHasError(config2.ExporterExemptionInfo, "overlapped configurations detected");
		}

		public void TestCheckIdenticalConfigurationRequiredTaxSystem()
		{
			CreateTaxSystemConfigurationForBrazil(Factory);

			var config1 = configurations.AddNew();
			config1.Country = Core.Constants.CountryCodes.Brazil;
			config1.SubType = "TXI";
			config1.LedgerType = "AR";
			config1.InvoiceType = "INV";
			config1.TaxInvoiceRule = "TID";
			config1.DisbursementRule = "DSB";
			config1.OriginalRule = "ARO";
			config1.OrganisationLocation = "PE";
			config1.ExporterExemption = "EXV";

			AssertNoErrors(config1.RequiredTaxSystemInfo);

			var config2 = configurations.AddNew();
			config2.Country = Core.Constants.CountryCodes.Brazil;
			config2.SubType = "TXI";
			config2.LedgerType = "AR";
			config2.InvoiceType = "INV";
			config2.TaxInvoiceRule = "TID";
			config2.DisbursementRule = "DSB";
			config2.OriginalRule = "ARO";
			config2.OrganisationLocation = "PE";
			config2.ExporterExemption = "EXV";

			config1.RequiredTaxSystem = "XXX";

			AssertHasError(config1.RequiredTaxSystemInfo, "Enter a valid selection.");

			config1.RequiredTaxSystem = "ISS";
			config2.RequiredTaxSystem = "ISS";
			AssertHasError(config2.RequiredTaxSystemInfo, "configuration with identical criteria already exists!");

			config2.RequiredTaxSystem = "INSS";
			AssertNoErrors(config2.RequiredTaxSystemInfo);

			config2.RequiredTaxSystem = "";
			AssertNoErrors(config2.RequiredTaxSystemInfo);
		}

		public void TestCheckIdenticalConfigurationExcludedTaxSystem()
		{
			CreateTaxSystemConfigurationForBrazil(Factory);

			var config1 = configurations.AddNew();
			config1.Country = Core.Constants.CountryCodes.Brazil;
			config1.SubType = "TXI";
			config1.LedgerType = "AR";
			config1.InvoiceType = "INV";
			config1.TaxInvoiceRule = "TID";
			config1.DisbursementRule = "DSB";
			config1.OriginalRule = "ARO";
			config1.OrganisationLocation = "PE";
			config1.ExporterExemption = "EXV";

			AssertNoErrors(config1.ExcludedTaxSystemInfo);

			var config2 = configurations.AddNew();
			config2.Country = Core.Constants.CountryCodes.Brazil;
			config2.SubType = "TXI";
			config2.LedgerType = "AR";
			config2.InvoiceType = "INV";
			config2.TaxInvoiceRule = "TID";
			config2.DisbursementRule = "DSB";
			config2.OriginalRule = "ARO";
			config2.OrganisationLocation = "PE";
			config2.ExporterExemption = "EXV";

			config1.ExcludedTaxSystem = "XXX";

			AssertHasError(config1.ExcludedTaxSystemInfo, "Enter a valid selection.");

			config1.ExcludedTaxSystem = "ISS";
			config2.ExcludedTaxSystem = "ISS";
			AssertHasError(config2.ExcludedTaxSystemInfo, "configuration with identical criteria already exists!");

			config2.ExcludedTaxSystem = "INSS";
			AssertNoErrors(config2.ExcludedTaxSystemInfo);

			config2.ExcludedTaxSystem = "";
			AssertNoErrors(config2.ExcludedTaxSystemInfo);
		}

		public void TestCheckOverlappedConfigurationExist()
		{
			var newConfig = configurations.AddNew();
			newConfig.Country = "PE";
			newConfig.SubType = "TXI";
			newConfig.LedgerType = "AR";
			newConfig.InvoiceType = "INV";
			newConfig.TaxInvoiceRule = "TID";
			newConfig.DisbursementRule = "DSB";
			newConfig.OriginalRule = "ARO";
			newConfig.OrganisationLocation = "PE";
			AssertNoErrors("Precondition: OriginalRule should not have errors.", newConfig.OriginalRuleInfo);

			newConfig.DisbursementRule = "ALL";
			newConfig.OriginalRule = "ALL";
			AssertHasError(newConfig.OriginalRuleInfo, "overlapped configurations detected");
			newConfig.OriginalRule = "ARO";
			AssertNoErrors("OriginalRule should not have errors.", newConfig.OriginalRuleInfo);

			newConfig.TaxInvoiceRule = "TID";
			newConfig.DisbursementRule = "ALL";
			newConfig.OriginalRule = "OTO";
			AssertHasError(newConfig.OriginalRuleInfo, "overlapped configurations detected");

			newConfig.TaxInvoiceRule = "TXX";
			newConfig.OriginalRule = "OTO";
			AssertNoErrors("OriginalRule should not have errors.", newConfig.OriginalRuleInfo);

			newConfig.TaxInvoiceRule = "TID";
			newConfig.OriginalRule = "OTO";
			AssertHasError(newConfig.OriginalRuleInfo, "overlapped configurations detected");

			newConfig.OrganisationLocation = "VN";
			newConfig.OriginalRule = "OTO";
			AssertNoErrors("OriginalRule should not have errors.", newConfig.OriginalRuleInfo);

			configuration.TaxInvoiceRule = "STI";
			configuration.TaxIDCode = "CAPP, CAPP2";

			newConfig.OrganisationLocation = "PE";
			newConfig.TaxInvoiceRule = "STI";
			newConfig.TaxIDCode = "CAPP3, CAPP";
			newConfig.OriginalRule = "OTO";
			AssertHasError(newConfig.OriginalRuleInfo, "overlapped configurations detected");

			newConfig.TaxIDCode = "AAA, CAPP3";
			newConfig.OriginalRule = "OTO";
			AssertNoErrors("OriginalRule should not have errors.", newConfig.OriginalRuleInfo);
		}

		#region SubTypeThresholdNotMet

		public void TestSubTypeThresholdNotMet_Validations()
		{
			configuration.SubTypeThresholdNotMet = string.Empty;
			AssertNoErrors("Precondition: SubTypeThresholdNotMet should not have errors.", configuration.SubTypeThresholdNotMetInfo);

			configuration.ThresholdApplies = true;
			configuration.SubTypeThresholdNotMet = string.Empty;
			AssertHasError(configuration.SubTypeThresholdNotMetInfo, "Please enter a value.");

			configuration.SubTypeThresholdNotMet = "XXX";
			AssertHasError("SubTypeThresholdNotMet has invalid code", configuration.SubTypeThresholdNotMetInfo, "Enter a valid selection.");

			configuration.SubType = "BOL";
			configuration.SubTypeThresholdNotMet = "Bol";
			AssertHasError("SubTypeThresholdNotMet should not be equals to SubType ignoring case", configuration.SubTypeThresholdNotMetInfo, "The same code cannot be selected for SubType and SubTypeThresholdNotMet");

			configuration.SubTypeThresholdNotMet = "BOL";
			AssertHasError(configuration.SubTypeThresholdNotMetInfo, "The same code cannot be selected for SubType and SubTypeThresholdNotMet");

			foreach (ICodeDescription code in ComplianceSubTypeCodesAndLists.Lists.GetSubTypeList(configuration.Country))
			{
				configuration.SubTypeThresholdNotMet = code.Code;
				AssertNoErrors(configuration.SubTypeThresholdNotMetInfo);
			}
		}

		public void TestSubTypeThresholdNotMet_Readonly()
		{
			Assert(configuration.SubTypeThresholdNotMetInfo.ReadOnly);
			configuration.ThresholdApplies = true;
			Assert(!configuration.SubTypeThresholdNotMetInfo.ReadOnly);
		}

		public void TestSubTypeThresholdNotMet_CleanValue()
		{
			configuration.ThresholdApplies = true;
			configuration.SubTypeThresholdNotMet = "TXI";
			AssertNotNullOrEmpty("Precondition: SubTypeThresholdNotMet should not be empty", configuration.SubTypeThresholdNotMet);

			configuration.ThresholdApplies = false;
			Assert("SubTypeThresholdNotMet should be empty", configuration.SubTypeThresholdNotMet.IsEmpty);
		}

		public void TestSubTypeThresholdNotMet_RunPreSaveValidation()
		{
			configuration.ThresholdApplies = true;
			configuration.SubTypeThresholdNotMet = string.Empty;
			AssertHasErrors("SubTypeThresholdNotMet should not be empty", configuration.SubTypeThresholdNotMetInfo);

			using (configuration.GetValidationSuspender())
			{
				configuration.SubTypeThresholdNotMet = "TCD";
				AssertHasErrors("SubTypeThresholdNotMet should not be empty", configuration.SubTypeThresholdNotMetInfo);

				configuration.RunPreSaveValidation();
				AssertNoErrors(configuration);
			}
		}

		public void TestSubTypeThresholdNotMet_SuspendValidation()
		{
			using (configuration.GetValidationSuspender())
			{
				configuration.ThresholdApplies = true;
				configuration.SubTypeThresholdNotMet = string.Empty;
				AssertNoErrors("SubTypeThresholdNotMet should not have errors.", configuration.SubTypeThresholdNotMetInfo);
			}
			configuration.SubTypeThresholdNotMet = string.Empty;
			AssertHasErrors("SubTypeThresholdNotMet should have errors.", configuration.SubTypeThresholdNotMetInfo);
		}

		#endregion

		public void TestCheckIdenticalConfigurationOrganisationCategory()
		{
			var newSetting1 = configurations.AddNew();
			newSetting1.Country = Constants.CountryCodes.France;
			newSetting1.LedgerType = LedgerTypes.AccountsReceivable;
			newSetting1.InvoiceType = "INV";
			newSetting1.TaxInvoiceRule = "TID";
			newSetting1.OriginalRule = "ALL";
			newSetting1.DisbursementRule = "ALL";
			newSetting1.OrganisationLocation = Constants.CountryCodes.France;
			newSetting1.OrganisationCategory = OrgConstants.Category.Business;

			var newSetting2 = configurations.AddNew();
			newSetting2.Country = Constants.CountryCodes.France;
			newSetting2.LedgerType = LedgerTypes.AccountsReceivable;
			newSetting2.InvoiceType = "INV";
			newSetting2.TaxInvoiceRule = "TID";
			newSetting2.OriginalRule = "ALL";
			newSetting2.DisbursementRule = "ALL";
			newSetting2.OrganisationLocation = Constants.CountryCodes.France;
			newSetting2.OrganisationCategory = OrgConstants.Category.Government;

			// Both configurations differ in OrgCategory
			Assert("Pre-requisite: There are no errors", !newSetting1.HasErrors);
			Assert("Pre-requisite: There are no errors", !newSetting2.HasErrors);

			// Now both configurations are identical
			newSetting2.OrganisationCategory = OrgConstants.Category.Business;
			AssertHasError(newSetting2.OrganisationCategoryInfo, "configuration with identical criteria already exists!");

			// Now both configurations differ again
			newSetting2.OrganisationCategory = OrgConstants.Category.NaturalPersonIndividual;
			AssertNoErrors(newSetting2.OrganisationCategoryInfo);
			Assert(!newSetting2.HasErrors);
		}

		public void TestCheckOverlappedConfigurationOrganisationCategory()
		{
			var newSetting1 = configurations.AddNew();
			newSetting1.Country = "FR";
			newSetting1.LedgerType = "AR";
			newSetting1.InvoiceType = "INV";
			newSetting1.TaxInvoiceRule = "TID";
			newSetting1.OriginalRule = "OTO";
			newSetting1.DisbursementRule = "ALL";
			newSetting1.OrganisationLocation = "FR";
			newSetting1.OrganisationCategory = "BUS";

			var newSetting2 = configurations.AddNew();
			newSetting2.Country = "FR";
			newSetting2.LedgerType = "AR";
			newSetting2.InvoiceType = "INV";
			newSetting2.TaxInvoiceRule = "TID";
			newSetting2.OriginalRule = "ALL";
			newSetting2.DisbursementRule = "ALL";
			newSetting2.OrganisationLocation = "FR";
			newSetting2.OrganisationCategory = "GOV";

			// Both configurations differ in OrganisationCategory, selecting disjunct records
			AssertNoErrors(newSetting1.OrganisationCategoryInfo);
			AssertNoErrors(newSetting2.OrganisationCategoryInfo);

			// Now both configurations are overlapping (OriginalRule ALL includes OTO)
			newSetting2.OrganisationCategory = "BUS";
			AssertHasError(newSetting2.OrganisationCategoryInfo, "overlapped configurations detected");

			// Now both configurations are disjunct (different OrgCategory)
			newSetting2.OrganisationCategory = "GOV";
			AssertNoErrors(newSetting1.OrganisationCategoryInfo);
			AssertNoErrors(newSetting2.OrganisationCategoryInfo);
		}

		public void TestXmlSerializationRoundTrip()
		{
			var expected = configurations.AddNew();
			expected.Country = "FR";
			expected.SubType = "TXI";
			expected.DocumentTitle = "";
			expected.LedgerType = "AR";
			expected.InvoiceType = "INV";
			expected.TaxInvoiceRule = "TID";
			expected.TaxIDCode = "CAPP, CAPP2";
			expected.DisbursementRule = "ALL";
			expected.OriginalRule = "ALL";
			expected.OrganisationLocation = "FR";
			expected.TaxRegistrationType = "REC";
			expected.SelfBillingRule = "SBI";
			expected.TaxRegistrationLocationRule = "IT";
			expected.VATGroupRule = "VGM";
			expected.ExporterExemption = "EXV";
			expected.ParentTransactionSubType = "TBO";
			expected.RequiredTaxSystem = "ISS";
			expected.ExcludedTaxSystem = "INSS";
			expected.RequiredRegistrationCode = "MIP";
			expected.ExcludedRegistrationCode = "DNI";
			expected.ThresholdApplies = true;
			expected.SubTypeThresholdNotMet = "TXI";
			expected.OrganisationCategory = "BUS";

			// Serialize the setting to XML and back
			ComplianceSubTypeAttributionRuleConfiguration actual;
			using (var writer = new StringWriter())
			{
				var serializer = new XmlSerializer(typeof(ComplianceSubTypeAttributionRuleConfiguration));
				serializer.Serialize(writer, expected);
				var serializedXml = writer.ToString();

				using (var reader = new StringReader(serializedXml))
				{
					actual = (ComplianceSubTypeAttributionRuleConfiguration)serializer.Deserialize(reader);
				}
			}

			AssertNotNull(actual);
			AssertEquals(expected.Country, actual.Country);
			AssertEquals(expected.SubType, actual.SubType);
			AssertEquals(expected.DocumentTitle, actual.DocumentTitle);
			AssertEquals(expected.LedgerType, actual.LedgerType);
			AssertEquals(expected.InvoiceType, actual.InvoiceType);
			AssertEquals(expected.TaxInvoiceRule, actual.TaxInvoiceRule);
			AssertEquals(expected.TaxIDCode, actual.TaxIDCode);
			AssertEquals(expected.DisbursementRule, actual.DisbursementRule);
			AssertEquals(expected.OriginalRule, actual.OriginalRule);
			AssertEquals(expected.OrganisationLocation, actual.OrganisationLocation);
			AssertEquals(expected.TaxRegistrationType, actual.TaxRegistrationType);
			AssertEquals(expected.SelfBillingRule, actual.SelfBillingRule);
			AssertEquals(expected.TaxRegistrationLocationRule, actual.TaxRegistrationLocationRule);
			AssertEquals(expected.VATGroupRule, actual.VATGroupRule);
			AssertEquals(expected.ExporterExemption, actual.ExporterExemption);
			AssertEquals(expected.ParentTransactionSubType, actual.ParentTransactionSubType);
			AssertEquals(expected.RequiredTaxSystem, actual.RequiredTaxSystem);
			AssertEquals(expected.ExcludedTaxSystem, actual.ExcludedTaxSystem);
			AssertEquals(expected.RequiredRegistrationCode, actual.RequiredRegistrationCode);
			AssertEquals(expected.ExcludedRegistrationCode, actual.ExcludedRegistrationCode);
			AssertEquals(expected.ThresholdApplies, actual.ThresholdApplies);
			AssertEquals(expected.SubTypeThresholdNotMet, actual.SubTypeThresholdNotMet);
			AssertEquals(expected.OrganisationCategory, actual.OrganisationCategory);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var item = new ComplianceSubTypeAttributionRuleConfiguration();
			item.Country = "PE";
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = "TID";
			item.DisbursementRule = "NDB";
			item.OriginalRule = "OTO";
			item.OrganisationLocation = "PE";
			return item;
		}

		protected override void SetUp()
		{
			base.SetUp();

			configuration = GetBusinessObjectToClone() as ComplianceSubTypeAttributionRuleConfiguration;
			configurations = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			configurations.Add(configuration);
		}

		ComplianceSubTypeAttributionRuleConfiguration SetUpArgentinaConfiguration()
		{
			var config = new ComplianceSubTypeAttributionRuleConfiguration();
			config.Country = "AR";
			config.SubType = "TXA";
			config.LedgerType = "AR";
			config.InvoiceType = "INV";
			config.TaxInvoiceRule = "TID";
			config.DisbursementRule = "NDB";
			config.OriginalRule = "ALL";
			config.TaxRegistrationType = "REC";
			return config;
		}

		ComplianceSubTypeAttributionRuleConfiguration SetUpPeruConfiguration()
		{
			var config = new ComplianceSubTypeAttributionRuleConfiguration();
			config.Country = "PE";
			config.SubType = "TBO";
			config.LedgerType = "AR";
			config.InvoiceType = "INV";
			config.TaxInvoiceRule = "TID";
			config.DisbursementRule = "NDB";
			config.OriginalRule = "OTO";
			config.TaxRegistrationType = "DNI";
			return config;
		}

		ComplianceSubTypeAttributionRuleConfiguration SetUpDominicanRepublicConfiguration()
		{
			var config = new ComplianceSubTypeAttributionRuleConfiguration();
			config.Country = Core.Constants.CountryCodes.DominicanRepublic;
			config.SubType = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXI;
			config.LedgerType = LedgerTypes.AccountsReceivable;
			config.InvoiceType = TransactionTypes.Invoice;
			config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			config.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			config.TaxRegistrationType = DominicanRepublicComplianceInfo.TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			return config;
		}

		ComplianceSubTypeAttributionRuleConfiguration SetUpTurkeyConfiguration()
		{
			var config = new ComplianceSubTypeAttributionRuleConfiguration();
			config.Country = Core.Constants.CountryCodes.Turkey;
			config.LedgerType = LedgerTypes.AccountsReceivable;
			config.SubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
			config.InvoiceType = TransactionTypes.Invoice;
			config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			config.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			config.TaxRegistrationType = TaxRegistrationTypeCodes.BasicEInvoiceRegisteredOrganizationTurkey;
			return config;
		}

		internal static void CreateTaxSystemConfigurationForBrazil(BusinessObjectFactory factory)
		{
			var helper = new AccountingTestObjectCreator(factory);
			helper.CreateTaxAuthority("TA", country: Constants.CountryCodes.Brazil);

			helper.CreateTaxSystem("ISS", country: Constants.CountryCodes.Brazil, name: "PROVISÃO DE ISS");
			helper.CreateTaxSystem("INSS", country: Constants.CountryCodes.Brazil, name: "RETENÇÃO DE INSS");

			var taxSystemCodeDesc_1 = new CodeDescriptionPair("ISS", "PROVISÃO DE ISS");
			var taxSystemCodeDesc_2 = new CodeDescriptionPair("INSS", "RETENÇÃO DE INSS");
			TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper()
								.WithGetTaxSystems(Constants.CountryCodes.Brazil, ZString.Empty, new CodeDescriptionPairList { taxSystemCodeDesc_1, taxSystemCodeDesc_2 });
		}

		ComplianceSubTypeAttributionRuleConfiguration configuration;
		ComplianceSubTypeAttributionRuleConfigurationCollection configurations;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new ComplianceSubTypeAttributionRuleConfiguration BizObj
		{
			get { return (ComplianceSubTypeAttributionRuleConfiguration)base.BizObj; }
		}

		protected virtual ComplianceSubTypeAttributionRuleConfigurationCollection GetAuthorisationSettingsCollection()
		{
			return new ComplianceSubTypeAttributionRuleConfigurationCollection();
		}

		#endregion
	}
}
