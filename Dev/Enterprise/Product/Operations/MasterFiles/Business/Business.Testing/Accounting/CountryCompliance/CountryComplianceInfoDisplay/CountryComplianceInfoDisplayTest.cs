using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay.Testing
{
	[TestedType(typeof(CountryComplianceInfoDisplay))]
	sealed class CountryComplianceInfoDisplayTest : NonPersistentBusinessObjectTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestComplianceSubTypesRules()
		{
			var testObject = new CountryComplianceInfoDisplay();
			var countries = Factory.Load<RefCountry>(new ZQuery());
			foreach (RefCountry country in countries)
			{
				testObject.CountryCode = country.Code;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
				{
					var registryRules = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value;
					if (registryRules.Count > 0 && (CountryComplianceFactory.GetIComplianceSubTypeRuleProvider(testObject.CountryCode) != null || CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(testObject.CountryCode) != null))
					{
						Assert(country.Description + " have Records in the Registry but not in the interface.", testObject.ComplianceSubTypeAttributionRules.Count > 0);
					}
					else
					{
						AssertEquals(country.Description + " doesn't implement the new architecture yet.", 0, testObject.ComplianceSubTypeAttributionRules.Count);
					}
				}
			}
		}

		public void TestComplianceSubTypeAttributionRulesValidation()
		{
			var testObject = new CountryComplianceInfoDisplay();
			var countries = Factory.Load<RefCountry>(new ZQuery());
			foreach (RefCountry country in countries)
			{
				testObject.CountryCode = country.Code;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
				{
					var collection = testObject.ComplianceSubTypeAttributionRules;
					foreach (var ruleSet in collection)
					{
						Assert("RuleSet should not have validation error", !ruleSet.HasErrors);
					}
				}
			}
		}

		public void TestComplianceSubTypesAttributingRulesShowAllCountry()
		{
			ComplianceSubTypesAttributingRulesShowAllCountry(Core.Constants.CountryCodes.ElSalvador, Core.Constants.CountryCodes.Brazil, 8);
		}

		public void TestComplianceSubTypesAttributingRulesShowAllCountry_WithCountryHavingMultipleRuleSet()
		{
			ComplianceSubTypesAttributingRulesShowAllCountry(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.ElSalvador, 13);
		}

		void ComplianceSubTypesAttributingRulesShowAllCountry(string countryCode, string changeCountryCode, int complianceSubTypeAttributionRulesCount)
		{
			var testObject = new CountryComplianceInfoDisplay();
			testObject.CountryCode = countryCode;
			Assert(testObject.ComplianceSubTypeAttributionRules.Cast<ComplianceSubTypeAttributionRuleConfiguration>().All(x => x.Country == countryCode));
			Assert(testObject.ComplianceSubTypeAttributionRules.Count == complianceSubTypeAttributionRulesCount);

			testObject.ComplianceSubTypeAttributionRulesForAllCountries = true;
			Assert(testObject.ComplianceSubTypeAttributionRules.Cast<ComplianceSubTypeAttributionRuleConfiguration>().Any(x => x.Country == countryCode));
			Assert(testObject.ComplianceSubTypeAttributionRules.Cast<ComplianceSubTypeAttributionRuleConfiguration>().Any(x => x.Country != countryCode));
			Assert(testObject.ComplianceSubTypeAttributionRules.Count > complianceSubTypeAttributionRulesCount);

			var totalRules = testObject.ComplianceSubTypeAttributionRules.Count;

			testObject.CountryCode = changeCountryCode;
			AssertEquals("Total number of rules should not change since tick box is ticked", totalRules, testObject.ComplianceSubTypeAttributionRules.Count);

			testObject.CountryCode = countryCode;

			testObject.ComplianceSubTypeAttributionRulesForAllCountries = false;
			Assert(testObject.ComplianceSubTypeAttributionRules.Cast<ComplianceSubTypeAttributionRuleConfiguration>().All(x => x.Country == countryCode));
			Assert(testObject.ComplianceSubTypeAttributionRules.Count == complianceSubTypeAttributionRulesCount);
		}

		public void TestTaxSystems()
		{
			var testObject = new CountryComplianceInfoDisplay();
			var registryDefaultValue = AccountingMasterFilesRegistry.Instance.TaxSystems.DefaultValue.OfType<TaxSystemsConfiguration>();
			Assert("Pre-condition: Should have some defaults", registryDefaultValue.Any());
			AssertEquals("Pre-Condition: do not show TaxSystems for all countries.", false, testObject.ShowTaxSystemsForAllCountries);

			CombineAssertions(() =>
			{
				var countries = Factory.Load<RefCountry>(new ZQuery());
				foreach (RefCountry country in countries)
				{
					var countryCode = testObject.CountryCode = country.Code;
					var expectedValue = registryDefaultValue.Where(x => x.Country == countryCode).Select(x => x.Code).ToArray();
					if (expectedValue.Length > 0)
					{
						AssertContainsExactElementsInAnyOrder(country.Code + " have different records in the TaxSystems Registry DefaultValue and CountryComplianceInfoDisplay.TaxSystems.",
							expectedValue, testObject.TaxSystems.OfType<TaxSystemsConfiguration>().Select(x => x.Code).ToArray());
					}
					else
					{
						AssertEquals(country.Code + " should not contain any records in the CountryComplianceInfoDisplay.TaxSystems.", 0, testObject.TaxSystems.Count);
					}
				}

				testObject.ShowTaxSystemsForAllCountries = true;
				AssertContainsExactElementsInAnyOrder("Records for all countries", registryDefaultValue.Select(x => x.Code).ToArray(), testObject.TaxSystems.OfType<TaxSystemsConfiguration>().Select(x => x.Code));
			});
		}

		public void TestTaxTaxAuthorities()
		{
			var testObject = new CountryComplianceInfoDisplay();
			var registryDefaultValue = AccountingMasterFilesRegistry.Instance.TaxAuthorities.DefaultValue.OfType<TaxAuthoritiesConfiguration>();
			Assert("Pre-condition: Should have some defaults", registryDefaultValue.Any());
			AssertEquals("Pre-Condition: do not show TaxAuthorities for all countries.", false, testObject.ShowTaxAuthoritiesForAllCountries);

			CombineAssertions(() =>
			{
				var countries = Factory.Load<RefCountry>(new ZQuery());
				foreach (RefCountry country in countries)
				{
					var countryCode = testObject.CountryCode = country.Code;
					var expectedValue = registryDefaultValue.Where(x => x.Country == countryCode).Select(x => x.Code).ToArray();
					if (expectedValue.Length > 0)
					{
						AssertContainsExactElementsInAnyOrder(country.Code + " have different records in the TaxAuthorities Registry DefaultValue and CountryComplianceInfoDisplay.TaxAuthorities.",
							expectedValue, testObject.TaxAuthorities.OfType<TaxAuthoritiesConfiguration>().Select(x => x.Code).ToArray());
					}
					else
					{
						AssertEquals(country.Code + " should not contain any records in the CountryComplianceInfoDisplay.TaxAuthorities.", 0, testObject.TaxAuthorities.Count);
					}
				}

				testObject.ShowTaxAuthoritiesForAllCountries = true;
				AssertContainsExactElementsInAnyOrder("Records for all countries", registryDefaultValue.Select(x => x.Code).ToArray(), testObject.TaxAuthorities.OfType<TaxAuthoritiesConfiguration>().Select(x => x.Code));
			});
		}
	}
}
