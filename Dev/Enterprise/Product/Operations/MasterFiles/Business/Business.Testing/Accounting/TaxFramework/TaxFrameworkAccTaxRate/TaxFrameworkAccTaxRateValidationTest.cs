using System;

using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TaxFrameworkAccTaxRateValidationTest : AccTaxRateValidationTest
	{
		#region Implementation

		protected override AutoAccTaxRate TaxRate
		{
			get
			{
				if (taxRate == null)
				{
					taxRate = Factory.New<TaxFrameworkAccTaxRate>();
				}
				return taxRate;
			}
		}

		#endregion

		#region Validation

		public override void TestValidateAT_Code()
		{
			TaxRate.AT_Code = "";
			AssertHasError(TaxRate.AT_CodeInfo, "Please enter a Tax Code.");

			TaxRate.AT_Code = "A!C";
			AssertHasError(TaxRate.AT_CodeInfo, "Please enter alphanumeric characters only.");

			TaxRate.AT_Code = "ABC";
			AssertNoErrors(TaxRate.AT_CodeInfo);
		}

		public override void TestValidateAT_Description()
		{
			TaxRate.AT_Description = "";
			AssertHasError(TaxRate.AT_DescriptionInfo, "Please enter a Description.");

			TaxRate.AT_Description = "###";
			AssertNoErrors(TaxRate.AT_DescriptionInfo);
		}

		public void TestValidateAT_RN_NKCountry()
		{
			TaxRate.AT_RN_NKCountry = "";
			AssertHasError(TaxRate.AT_RN_NKCountryInfo, "Please enter a Country/Region.");

			TaxRate.AT_RN_NKCountry = "AA";
			AssertHasError(TaxRate.AT_RN_NKCountryInfo, "Enter a valid Country/Region.");

			TaxRate.AT_RN_NKCountry = CountryCodes.Australia;
			AssertNoErrors(TaxRate.AT_RN_NKCountryInfo);
		}

		public void TestValidateAT_TaxSystemCode()
		{
			TaxRate.AT_TaxSystemCode = "";
			AssertHasError(TaxRate.AT_TaxSystemCodeInfo, "Please enter a Tax System.");

			TaxRate.AT_TaxSystemCode = "AA";
			AssertHasError(TaxRate.AT_TaxSystemCodeInfo, "Enter a valid Tax System.");

			TaxSystemsConfigurationCollection collection = new TaxSystemsConfigurationCollection();
			TaxSystemsConfiguration item = collection.AddNew();
			item.Code = "PIB";
			item.Name = "IB Percepcione";
			item.Country = CountryCodes.Argentina;
			item.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.State.Code;
			item.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code;
			item.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Company.Code;
			item.IncludeInInvoceTotal = true;
			item.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Positive.Code;
			item.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
			item.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
			item.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;
			item.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationOnly.Code;

			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			collection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			TaxRate.AT_RN_NKCountry = CountryCodes.Argentina;
			TaxRate.AT_TaxSystemCode = "PIB";
			AssertNoErrors(TaxRate.AT_TaxSystemCodeInfo);
		}

		public void TestValidateAT_RateSource()
		{
			TaxRate.AT_RateSource = "";
			AssertHasError(TaxRate.AT_RateSourceInfo, "Please enter a Rate Source.");

			TaxRate.AT_RateSource = "AA";
			AssertHasError(TaxRate.AT_RateSourceInfo, "Enter a valid Rate Source.");

			TaxRate.AT_RateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxIDOnly.Code;
			AssertNoErrors(TaxRate.AT_RateSourceInfo);

			TaxRate.AT_RateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationOnly.Code;
			AssertNoErrors(TaxRate.AT_RateSourceInfo);
			TaxRate.AT_RateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationFallbackToTaxGroup.Code;
			AssertNoErrors(TaxRate.AT_RateSourceInfo);
			TaxRate.AT_RateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxGroupOnly.Code;
			AssertNoErrors(TaxRate.AT_RateSourceInfo);
		}

		#endregion

	}
}
