using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxSystemsConfiguration))]
	class TaxSystemsConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestIsSingleRecordPerARTransactionRequired()
		{
			var config = new TaxSystemsConfiguration();
			config.Code = ZString.Empty;
			config.Country = CountryCodes.Brazil;
			Assert("tax code is empty", !config.IsSingleRecordPerARTransactionRequired(CountryCodes.Brazil));

			config.Code = "ISS";
			Assert(config.IsSingleRecordPerARTransactionRequired(CountryCodes.Brazil));

			config.Code = "ISS";
			Assert("country passed is not Brazil", !config.IsSingleRecordPerARTransactionRequired(CountryCodes.Australia));

			config.Code = "ISS";
			Assert(config.IsSingleRecordPerARTransactionRequired(CountryCodes.Brazil));

			config.Country = CountryCodes.Venezuela;
			Assert("country for ISS tax is not Brazil", !config.IsSingleRecordPerARTransactionRequired(CountryCodes.Brazil));

			config.Code = "ISS";
			config.Country = CountryCodes.Brazil;
			Assert(config.IsSingleRecordPerARTransactionRequired(CountryCodes.Brazil));

			config.Code = "ISSS";
			Assert("tax code is not ISS", !config.IsSingleRecordPerARTransactionRequired(CountryCodes.Brazil));
		}

		public void TestValidateCode()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 10, config.CodeInfo.MaxLength);

			config.RunPreSaveValidation();
			AssertHasError(config.CodeInfo, "Please enter a Code.");

			config.Code = "C.1";
			config.RunPreSaveValidation();
			AssertHasError(config.CodeInfo, "Please enter alphanumeric characters only.");

			config.Code = "CḈ1";
			config.RunPreSaveValidation();
			AssertHasError(config.CodeInfo, "Code only accepts Western European languages characters.");

			config.Code = "Cw1Code";
			config.RunPreSaveValidation();
			AssertNoErrors(config.CodeInfo);
			AssertEquals("Assignment in uppercase", "CW1CODE", config.Code);
		}

		public void TestName()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 80, config.NameInfo.MaxLength);

			config.Name = ZString.Empty;
			config.RunPreSaveValidation();
			AssertNoErrors(config.NameInfo);

			config.Name = "C.1";
			config.RunPreSaveValidation();
			AssertNoErrors(config.NameInfo);
		}

		public void TestValidateCountry()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 2, config.CountryInfo.MaxLength);

			config.Name = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.CountryInfo, "Please enter a Country/Region.");

			config.Country = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.CountryInfo, "Enter a valid Country/Region.");

			config.Country = CountryCodes.Australia;
			config.RunPreSaveValidation();
			AssertNoErrors(config.CountryInfo);
		}

		public void TestValidateTaxSuperType()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 3, config.TaxSuperTypeInfo.MaxLength);

			config.TaxSuperType = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.TaxSuperTypeInfo, "Please enter a Tax Super Type.");

			config.TaxSuperType = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.TaxSuperTypeInfo, "Enter a valid Tax Super Type.");

			config.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxSuperTypeInfo);

			config.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.RetentionInInvoice.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxSuperTypeInfo);

			config.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.StandardPaymentRetention.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxSuperTypeInfo);

			config.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.TurnoverTax.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxSuperTypeInfo);

			config.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.SalesTax.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxSuperTypeInfo);
		}

		public void TestValidateRegistrationLevel()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 3, config.RegistrationLevelInfo.MaxLength);

			config.RegistrationLevel = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.RegistrationLevelInfo, "Please enter a Registration Level.");

			config.RegistrationLevel = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.RegistrationLevelInfo, "Enter a valid Registration Level.");

			config.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Company.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.RegistrationLevelInfo);

			config.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Branch.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.RegistrationLevelInfo);
		}

		public void TestValidateAdjustmentSign()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 3, config.AdjustmentSignInfo.MaxLength);

			config.AdjustmentSign = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.AdjustmentSignInfo, "Please enter an Adjustment Sign.");

			config.AdjustmentSign = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.AdjustmentSignInfo, "Enter a valid Adjustment Sign.");

			config.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Positive.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.AdjustmentSignInfo);

			config.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Negative.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.AdjustmentSignInfo);
		}

		public void TestValidateTaxBaseCalculationMethod()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 3, config.TaxBaseCalculationMethodInfo.MaxLength);

			config.TaxBaseCalculationMethod = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.TaxBaseCalculationMethodInfo, "Please enter a Tax Base Calculation Method.");

			config.TaxBaseCalculationMethod = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.TaxBaseCalculationMethodInfo, "Enter a valid Tax Base Calculation Method.");

			config.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxBaseCalculationMethodInfo);
		}

		public void TestValidateTaxAmountCalculationMethod()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 3, config.TaxAmountCalculationMethodInfo.MaxLength);

			config.TaxAmountCalculationMethod = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.TaxAmountCalculationMethodInfo, "Please enter a Tax Amount Calculation Method.");

			config.TaxAmountCalculationMethod = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.TaxAmountCalculationMethodInfo, "Enter a valid Tax Amount Calculation Method.");

			config.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxAmountCalculationMethodInfo);
		}

		public void TestValidateThresholdRule()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 3, config.ThresholdRuleInfo.MaxLength);

			config.ThresholdRule = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.ThresholdRuleInfo, "Please enter a Threshold Rule.");

			config.ThresholdRule = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.ThresholdRuleInfo, "Enter a valid Threshold Rule.");

			config.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.ThresholdRuleInfo);
		}

		public void TestValidateTaxRateSource()
		{
			var config = new TaxSystemsConfiguration();
			AssertEquals("Max length", 3, config.TaxRateSourceInfo.MaxLength);

			config.TaxRateSource = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.TaxRateSourceInfo, "Please enter a Tax Rate Source.");

			config.TaxRateSource = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.TaxRateSourceInfo, "Enter a valid Tax Rate Source.");

			config.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationOnly.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxRateSourceInfo);

			config.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationFallbackToTaxGroup.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxRateSourceInfo);

			config.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxGroupOnly.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxRateSourceInfo);

			config.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxIDOnly.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxRateSourceInfo);

			config.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationFallbackToTaxID.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxRateSourceInfo);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject() => new TaxSystemsConfiguration();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => (TaxSystemsConfiguration)GetNewBusinessObject();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => (TaxSystemsConfiguration)GetNewBusinessObject();

		protected new TaxSystemsConfiguration BizObj => (TaxSystemsConfiguration)base.BizObj;

		#endregion
	}
}
