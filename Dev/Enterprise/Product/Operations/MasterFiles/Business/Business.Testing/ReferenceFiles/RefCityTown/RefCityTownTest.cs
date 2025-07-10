using CargoWise.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCityTown))]
	sealed class RefCityTownTest : EnterpriseBusinessObjectTestCase
	{
		public void TestState()
		{
			RefCityTown cityTown = Factory.New<RefCityTown>();

			cityTown.R9_RN_NKCountry = "SE";
			cityTown.R9_RW_NKState = "AB";

			AssertEquals("Stockholms län", cityTown.State.RW_Description);

			cityTown.R9_RN_NKCountry = "CA";
			cityTown.R9_RW_NKState = "AB";

			AssertEquals("Alberta", cityTown.State.RW_Description);

			cityTown.R9_RW_NKState = "WA";
			cityTown.R9_RN_NKCountry = string.Empty;
			ErrorReporter.Clear();
			var obj = cityTown.State;
			AssertEquals("There should be no error reports when getting cityTown.State.", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestR9_RN_NKCountrySetter()
		{
			RefCityTown cityTown = Factory.New<RefCityTown>();

			cityTown.R9_RN_NKCountry = "SE";
			cityTown.R9_RW_NKState = "AB";

			AssertEquals("AB", cityTown.R9_RW_NKState);

			cityTown.R9_RN_NKCountry = "CA";

			Assert(cityTown.R9_RW_NKState.IsEmpty);
		}

		public void TestDescriptionIsEmpty()
		{
			RefCityTown cityTown = Factory.New<RefCityTown>();
			cityTown.R9_LocalLanguageName = "Local";
			cityTown.R9_InternationalName = "SYDNEY";
			AssertEquals("Local", cityTown.Description);

			cityTown.R9_LocalLanguageName = "";
			AssertEquals("SYDNEY", cityTown.Description);
		}

		#region System Defined CityTowns

		public void TestR9InternationalNameReadOnly()
		{
			RefCityTown cityTown = Factory.New<RefCityTown>();
			cityTown.R9_IsSystem = false;
			cityTown.OnLoaded();
			Assert("R9_InternationalName should not be read only", !cityTown.R9_InternationalNameInfo.ReadOnly);
			cityTown.R9_IsSystem = true;
			cityTown.OnLoaded();
			Assert("R9_InternationalName should be read only", cityTown.R9_InternationalNameInfo.ReadOnly);
		}

		public void TestR9InternationalNameDoesNotHaveLeadingSpaces()
		{
			RefCityTown cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = " Mordor";
			AssertEquals("R9_InternationalName should not contain leading spaces", "Mordor", cityTown.R9_InternationalName);
		}

		public void TestR9RNNKCountryReadOnly()
		{
			RefCityTown cityTown = Factory.New<RefCityTown>();
			cityTown.R9_IsSystem = false;
			cityTown.OnLoaded();
			Assert("R9_RN_NKCountry should not be read only", !cityTown.R9_RN_NKCountryInfo.ReadOnly);
			cityTown.R9_IsSystem = true;
			cityTown.OnLoaded();
			Assert("R9_RN_NKCountry should be read only", cityTown.R9_RN_NKCountryInfo.ReadOnly);
		}

		public void TestR9RWNKStateReadOnly()
		{
			RefCityTown cityTown = Factory.New<RefCityTown>();
			cityTown.R9_IsSystem = false;
			cityTown.OnLoaded();
			Assert("R9_RW_NKState should not be read only", !cityTown.R9_RW_NKStateInfo.ReadOnly);
			cityTown.R9_IsSystem = true;
			cityTown.OnLoaded();
			Assert("R9_RW_NKState should be read only", cityTown.R9_RW_NKStateInfo.ReadOnly);
		}

		#endregion
	}
}
