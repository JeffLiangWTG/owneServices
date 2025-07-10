using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCountryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRN_Code()
		{
			Country.RN_Code = ZString.Empty;
			Assert("Expecting RN_Code to be empty and have errors.", Country.RN_CodeInfo.HasErrors());
			Country.RN_Code = new ZString("1");
			Assert("Expecting RN_Code to be 1 char long and have errors.", Country.RN_CodeInfo.HasErrors());
			Country.RN_Code = new ZString("er");
			Assert("RN_Code should be correct, not expecting errors.", !Country.RN_CodeInfo.HasNotifications());
		}

		public void TestValidateRN_Desc()
		{
			Country.RN_Desc = ZString.Empty;
			Assert("Expecting RN_Desc to be empty and have errors.", Country.RN_DescInfo.HasErrors());
			Country.RN_Desc = new ZString("asd");
			Assert("Expection RN_Desc to have too few characters and have errors.", Country.RN_DescInfo.HasErrors());
			Country.RN_Desc = new ZString("Australia, Dollars");
			Assert("RN_Desc should be correct, not expecting errors.", !Country.RN_DescInfo.HasNotifications());
		}

		public void TestValidateRN_DescForSystemCountries()
		{
			RefCountry systemDefinedCountry = Factory.New<RefCountry>();
			systemDefinedCountry.RN_Code = "ZU";
			systemDefinedCountry.RN_Desc = "Zublavia";
			systemDefinedCountry.RN_IsSystem = true;
			Factory.Save();

			Assert("No warnings on RN_Desc", !systemDefinedCountry.RN_DescInfo.HasWarnings());

			systemDefinedCountry.RN_Desc = "Changed Value";
			Assert("Warning on RN_Desc as it was changed for a system defined country", systemDefinedCountry.RN_DescInfo.HasWarnings());

			systemDefinedCountry.RN_Desc = "Zublavia";
			Assert("Warning removed as value is original value", !systemDefinedCountry.RN_DescInfo.HasWarnings());

			systemDefinedCountry.RN_IsSystem = false;
			systemDefinedCountry.RN_Desc = "Changed Value";
			Assert("No warning as country is not system defined", !systemDefinedCountry.RN_DescInfo.HasWarnings());
		}

		public void TestValidateRN_RX()
		{
			Country.RN_RX_NKLocalCurrency = ZString.Empty;
			AssertHasErrors("Expecting RN_RX_NKLocalCurrency to be empty and have errors.", Country.RN_RX_NKLocalCurrencyInfo);
		}

		public void TestRN_EconomicGrouping()
		{
			RefCountry testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.RN_EconomicGrouping = ZString.Empty;
			AssertNoErrors(testCountry.RN_EconomicGroupingInfo);

			testCountry.RN_EconomicGrouping = "XXX";
			AssertHasErrors(testCountry.RN_EconomicGroupingInfo);

			testCountry.RN_EconomicGrouping = testCountry.Lookups.ListOfEconomicGroups[0].Code;
			AssertNoErrors(testCountry.RN_EconomicGroupingInfo);
		}

		public void TestRN_AddressFormattingRule()
		{
			RefCountry testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.RN_AddressFormattingRule = ZString.Empty;
			AssertHasErrors(testCountry.RN_AddressFormattingRuleInfo);

			testCountry.RN_AddressFormattingRule = testCountry.Lookups.CountryAddressFormattingRules[0].Code;
			AssertNoErrors(testCountry.RN_AddressFormattingRuleInfo);

			testCountry.RN_AddressFormattingRule = "XXX";
			AssertHasErrors(testCountry.RN_AddressFormattingRuleInfo);
		}

		public void TestRN_StateProvinceValidationRule()
		{
			RefCountry testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.RN_StateProvinceValidationRule = ZString.Empty;
			AssertHasErrors(testCountry.RN_StateProvinceValidationRuleInfo);

			testCountry.RN_StateProvinceValidationRule = testCountry.Lookups.StateAndProvinceValidationRules[0].Code;
			AssertNoErrors(testCountry.RN_StateProvinceValidationRuleInfo);

			testCountry.RN_StateProvinceValidationRule = "XXX";
			AssertHasErrors(testCountry.RN_StateProvinceValidationRuleInfo);
		}

		public void TestRN_PostcodeValidationRule()
		{
			RefCountry testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.RN_PostcodeValidationRule = ZString.Empty;
			AssertHasErrors(testCountry.RN_PostcodeValidationRuleInfo);

			testCountry.RN_PostcodeValidationRule = testCountry.Lookups.PostCodeValidationRules[0].Code;
			AssertNoErrors(testCountry.RN_PostcodeValidationRuleInfo);

			testCountry.RN_PostcodeValidationRule = "XXX";
			AssertHasErrors(testCountry.RN_PostcodeValidationRuleInfo);
		}

		public void TestValidationPassesOnAllSystemDefinedCountries()
		{
			Country.Delete();
			CombineAssertions(delegate
			{
				foreach (var country in Factory.Load<RefCountry>(new ZQuery()))
				{
					country.RunPreSaveValidation();
					Assert("Country " + country.Code + " has validation errors: " + country.GetErrors().ToMessageListString(), !country.HasErrors);
				}
			});
		}

		#region Implementation

		RefCountry Country;

		protected override void SetUp()
		{
			base.SetUp();
			Country = Factory.New<RefCountry>();
			Country.RN_Code = "XY";
		}

		#endregion
	}
}
