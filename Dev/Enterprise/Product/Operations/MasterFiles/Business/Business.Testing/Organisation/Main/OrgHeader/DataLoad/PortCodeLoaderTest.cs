using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PortCodeLoaderTest : TestCaseWithFactory
	{
		public void TestGetPortCodeFromState()
		{
			PortCodeLoader testUtils = new PortCodeLoader();
			string testPortCode = testUtils.GenerateRequiredPortCode(Factory, "", "", "NSW");
			AssertEquals("NSW", "AUSYD", testPortCode);

			testPortCode = testUtils.GenerateRequiredPortCode(Factory, "", "", "WESTERN AUSTRALIA");
			AssertEquals("WESTERN AUSTRALIA", "AUPER", testPortCode);

			testPortCode = testUtils.GenerateRequiredPortCode(Factory, "", "", "TAS");
			AssertEquals("TAS", "AUHBA", testPortCode);

			testPortCode = testUtils.GenerateRequiredPortCode(Factory, "", "", "NonExistingAustralianState");
			AssertEquals("NonExistingAustralianState", "ZZZZZ", testPortCode);

			testPortCode = testUtils.GenerateRequiredPortCode(Factory, "", "", "N.S.W.");
			AssertEquals("N.S.W.", "AUSYD", testPortCode);
			testPortCode = testUtils.GenerateRequiredPortCode(Factory, "", "", "TAS.");
			AssertEquals("TAS.", "AUHBA", testPortCode);
			testPortCode = testUtils.GenerateRequiredPortCode(Factory, "", "", "Vic");
			AssertEquals("Vic", "AUMEL", testPortCode);

			testPortCode = testUtils.GenerateRequiredPortCode(Factory, "AU", "DROMANA", "Vic");
			AssertEquals("Vic", "AUDNA", testPortCode);

			testPortCode = testUtils.GenerateRequiredPortCode(Factory, "AU", "DROMANA", "NonExistingAustralianState");
			AssertEquals("NonExistingAustralianState", "AUZZZ", testPortCode);
		}

		public void TestGetPortCodeFromCountry()
		{
			PortCodeLoader testUtils = new PortCodeLoader();
			string testPortCode = testUtils.GenerateRequiredPortCode(Factory, "AU", "", "");
			AssertEquals("AU", "AUZZZ", testPortCode);

			testPortCode = testUtils.GenerateRequiredPortCode(Factory, "AUS", "", "");
			AssertEquals("AUS", "ZZZZZ", testPortCode);
		}

		public void TestGetPortFromNameAndCountryCode()
		{
			PortCodeLoader testUtils = new PortCodeLoader();
			ZString testPortName = "Christchurch";
			ZString testCountryCode = "NZ";
			ZString testPort = testUtils.GenerateRequiredPortCode(Factory, testCountryCode, testPortName, "");
			AssertEquals("Christchurch port code", "NZCHC", testPort);

			testPortName = "InvalidPortName";
			testPort = testUtils.GenerateRequiredPortCode(Factory, testCountryCode, testPortName, "");
			AssertEquals("InvalidPortName - should return default value", "NZZZZ", "NZZZZ");

			testPortName = "Wollongong";
			testPort = testUtils.GenerateRequiredPortCode(Factory, testCountryCode, testPortName, "");
			AssertEquals("Wollongong port should not be found in NZ", "NZZZZ", "NZZZZ");

			testCountryCode = "AU";
			testPort = testUtils.GenerateRequiredPortCode(Factory, testCountryCode, testPortName, "");
			AssertEquals("Wollongong port code", "AUWOL", testPort);
		}

		public void TestGetPortFromNameAndCountryName()
		{
			PortCodeLoader testUtils = new PortCodeLoader();
			ZString testPortName = "Christchurch";
			ZString testCountryName = "New Zealand";
			ZString testPort = testUtils.GenerateRequiredPortCode(Factory, testCountryName, testPortName, "");
			AssertEquals("Christchurch port code", "NZCHC", testPort);

			testPortName = "InvalidPortName";
			testPort = testUtils.GenerateRequiredPortCode(Factory, testCountryName, testPortName, "");
			AssertEquals("InvalidPortName - should return default value", "NZZZZ", "NZZZZ");

			testPortName = "Wollongong";
			testPort = testUtils.GenerateRequiredPortCode(Factory, testCountryName, testPortName, "");
			AssertEquals("Wollongong should not be found in NZ - should return default value", "NZZZZ", "NZZZZ");

			testCountryName = "Australia";
			testPort = testUtils.GenerateRequiredPortCode(Factory, testCountryName, testPortName, "");
			AssertEquals("Wollongong port code", "AUWOL", testPort);

			testPortName = "Hamburg";
			testCountryName = "Germany";
			testPort = testUtils.GenerateRequiredPortCode(Factory, testCountryName, testPortName, "");
			AssertEquals("Hamburg port code", "DEHAM", testPort);
		}

		public void TestGetPortFromNameAndCountryCodeWithState()
		{
			AssertPortFromNameAndCountryNameWithState("US");
		}

		public void TestGetPortFromNameAndCountryNameWithState()
		{
			AssertPortFromNameAndCountryNameWithState("United States");
		}

		void AssertPortFromNameAndCountryNameWithState(ZString country)
		{
			PortCodeLoader testUtils = new PortCodeLoader();
			ZString testPortName = "Memphis";
			ZString state = "TX";
			ZString testPort = testUtils.GenerateRequiredPortCode(Factory, country, testPortName, "");
			AssertEquals("Memphis port code is MO state - the first one in the list", "USEJS", testPort);

			testPort = testUtils.GenerateRequiredPortCode(Factory, country, testPortName, state);
			AssertEquals("Memphis port code is TX state", "USMTX", testPort);
		}
	}
}
