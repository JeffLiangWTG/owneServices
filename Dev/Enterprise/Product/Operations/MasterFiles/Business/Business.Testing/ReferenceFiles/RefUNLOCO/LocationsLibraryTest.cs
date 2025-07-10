using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LocationsLibraryTest : TestCaseWithFactory
	{
		#region CityCountry

		public void TestCityCountry()
		{
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

			AssertCountryCodeMacro("Expected to use country code only as Ukrainian states must not be entered", "UAIEV", "Kiev, UA");
			AssertCountryCodeMacro("Expected to use country code only as Colombian states must not be entered", "COBOG", "Bogota, CO");
			AssertCountryCodeMacro("Expected to use state code as American states must be entered", "USCHI", "Chicago, IL, US");
			AssertCountryCodeMacro("Expected to use state code as Australian states must be entered", "AUSYD", "Sydney, NSW, AU");
			AssertCountryCodeMacro("Expected to use country code as China doesn't mandate that states must be entered (No validation rule)", "CNSHA", "Shanghai Hongqiao International Apt, CN");

			AssertCountryCodeMacro("Expected to use state code the custom port as country requires a state", "DEINO", "Deino Port, HH, DE");
			AssertCountryCodeMacro("Expected to use country code the custom port as country does not require state", "BAGON", "Bagon Port, BA");

			AssertCountryCodeMacro("Expected non-UNLOCOs to just be returned", "Hydreigon", "Hydreigon");
			AssertCountryCodeMacro("Expected non-UNLOCOs to just be returned", "Salamence", "Salamence");
		}

		void AssertCountryCodeMacro(string message, string unlocoString, string expectedResult)
		{
			DummyBusinessObject.Z0_Description = unlocoString;

			var context = new IMacroLibrary[]
			{
				new StandardLibrary(),
				new LocationsLibrary(Factory)
			}
			.CreateContext();

			var expression = "CityCountry(Z0_Description)".With(context).CreateExpression();

			AssertMacroRunWithNoErrors(message, expression, expectedResult);
		}

		#endregion

		#region IsUnloco

		public void TestIsUnloco()
		{
			var customUnloco = Factory.NewWithValidTestData<RefUNLOCO>();
			customUnloco.RL_Code = "ARBOK";
			Factory.Save();

			AssertIsUnlocoMacro("AUSYD", true);
			AssertIsUnlocoMacro("UAIEV", true);
			AssertIsUnlocoMacro("EKANS", false);
			AssertIsUnlocoMacro("ARBOK", true);
		}

		void AssertIsUnlocoMacro(string unlocoString, bool expectedResult)
		{
			DummyBusinessObject.Z0_Code = unlocoString;

			var context = new IMacroLibrary[]
			{
				new StandardLibrary(),
				new LocationsLibrary(Factory)
			}
			.CreateContext();

			var expression = "IsUnloco(Z0_Code)".With(context).CreateExpression();

			AssertMacroRunWithNoErrors("Expected result for " + unlocoString, expression, expectedResult);
		}

		#endregion

		#region Implementation

		void AssertMacroRunWithNoErrors(string message, IMacroExpression expression, object expectedResult)
		{
			AssertEquals(message, expectedResult, expression.Evaluate(DummyBusinessObject));

			AssertMultilineASCIIEquals("no errors", "", string.Join("\r\n", expression.Errors));
		}

		DummyBusinessObject DummyBusinessObject
		{
			get { return dummyBusinessObject ?? (dummyBusinessObject = Factory.New<DummyBusinessObject>()); }
		}

		DummyBusinessObject dummyBusinessObject;

		#endregion
	}
}
