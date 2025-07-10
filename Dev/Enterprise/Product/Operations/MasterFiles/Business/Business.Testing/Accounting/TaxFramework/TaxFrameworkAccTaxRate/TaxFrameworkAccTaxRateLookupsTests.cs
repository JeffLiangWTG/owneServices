using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TaxFrameworkAccTaxRateLookupsTests : AccTaxRateLookupsTest
	{
		public void TestTaxSystems()
		{
			var parent = Factory.NewWithValidTestData<TaxFrameworkAccTaxRate>();
			var lookups = new TaxFrameworkAccTaxRateLookups(parent);

			TaxSystemsConfigurationCollection collection = new TaxSystemsConfigurationCollection();
			collection.Add(NewConfiguration("PIB", "IB Percepcione", Core.Constants.CountryCodes.Argentina));
			collection.Add(NewConfiguration("RIB", "IB Retencione", Core.Constants.CountryCodes.Argentina));
			collection.Add(NewConfiguration("CESS", "State CESS", Core.Constants.CountryCodes.India));
			collection.Add(NewConfiguration("ISS", "ISS", Core.Constants.CountryCodes.Brazil));

			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			collection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			parent.AT_RN_NKCountry = Core.Constants.CountryCodes.Argentina;
			Assert("PIB and RIB Tax System should be in the collection", lookups.TaxSystems.ContainsOnly("PIB", "RIB"));

			parent.AT_RN_NKCountry = Core.Constants.CountryCodes.Brazil;
			Assert("ISS Tax System should be in the collection", lookups.TaxSystems.ContainsOnly("ISS"));

			parent.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			Assert("CESS Tax System should be in the collection", lookups.TaxSystems.ContainsOnly("CESS"));

			parent.AT_RN_NKCountry = Core.Constants.CountryCodes.Uruguay;
			AssertEquals("Should be empty as TaxSystemsConfiguration Registry does not have entry for country", 0, lookups.TaxSystems.Count);

			parent.AT_RN_NKCountry = "";
			AssertEquals("Should be empty as TaxRate Country is empty", 0, lookups.TaxSystems.Count);

			TaxSystemsConfiguration NewConfiguration(string code, string name, string country)
			{
				TaxSystemsConfiguration item = new TaxSystemsConfiguration();
				item.Code = code;
				item.Name = name;
				item.Country = country;
				item.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.State.Code;
				item.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code;
				item.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Company.Code;
				item.IncludeInInvoceTotal = true;
				item.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Positive.Code;
				item.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
				item.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
				item.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;
				item.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationOnly.Code;

				return item;
			}
		}
	}
}
