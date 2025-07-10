using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UnlocoHelperTest : TestCaseWithFactory
	{
		#region CityCountry

		public void TestCityCountry()
		{
			var helper = new UnlocoHelper(Factory);

			var germany = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Germany);
			AssertEquals("Pre-condition", CountryAddressValidationRuleList.Codes.MustBeEntered, germany.RN_StateProvinceValidationRule);

			var deino = Factory.NewWithValidTestData<RefUNLOCO>();
			deino.RL_Code = "DEINO";
			deino.RL_PortName = "Deino Port";
			deino.RL_RN_NKCountryCode = germany.Code;
			deino.RL_RW = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, "HH")).PK;

			var bosniaAndHerzegovina = Factory.Load<RefCountry>(Core.Constants.CountryGuids.BosniaandHerzegovina);

			var bagon = Factory.NewWithValidTestData<RefUNLOCO>();
			bagon.RL_Code = "BAGON";
			bagon.RL_PortName = "Bagon Port";
			bagon.RL_RN_NKCountryCode = bosniaAndHerzegovina.Code;

			AssertEquals("Expected to use country code only as Ukrainian states must not be entered", "Kiev, UA", helper.GetCityCountry("UAIEV"));
			AssertEquals("Expected to use country code only as Colombian states must not be entered", "Bogota, CO", helper.GetCityCountry("COBOG"));
			AssertEquals("Expected to use state code as American states must be entered", "Chicago, IL, US", helper.GetCityCountry("USCHI"));
			AssertEquals("Expected to use state code as Australian states must be entered", "Sydney, NSW, AU", helper.GetCityCountry("AUSYD"));
			AssertEquals("Expected to use country code as China doesn't mandate that states must be entered (No validation rule)", "Shanghai Hongqiao International Apt, CN", helper.GetCityCountry("CNSHA"));

			AssertEquals("Expected to use state code the custom port as country requires a state", "Deino Port, HH, DE", helper.GetCityCountry("DEINO"));
			AssertEquals("Expected to use country code the custom port as country does not require state", "Bagon Port, BA", helper.GetCityCountry("BAGON"));

			AssertEquals("Expected non-UNLOCOs to just be returned", "Hydreigon", helper.GetCityCountry("Hydreigon"));
			AssertEquals("Expected non-UNLOCOs to just be returned", "Salamence", helper.GetCityCountry("Salamence"));
		}

		#endregion

		#region IsUnloco

		public void TestIsUnloco()
		{
			var customUnloco = Factory.NewWithValidTestData<RefUNLOCO>();
			customUnloco.RL_Code = "ARBOK";

			Factory.Save();

			var helper = new UnlocoHelper(Factory);

			AssertEquals("AUSYD", true, helper.IsUnloco("AUSYD"));
			AssertEquals("UAIEV", true, helper.IsUnloco("UAIEV"));
			AssertEquals("EKANS", false, helper.IsUnloco("EKANS"));
			AssertEquals("ARBOK", true, helper.IsUnloco("ARBOK"));
		}

		#endregion
	}
}
