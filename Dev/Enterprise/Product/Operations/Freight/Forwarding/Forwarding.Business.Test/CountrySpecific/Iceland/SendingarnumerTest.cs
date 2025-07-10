using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SendingarnumerTest : TestCase
	{
		public void TestProperties()
		{
			Sendingarnumer sen = new Sendingarnumer("");

			#region Code Properties
			sen.CarrierCode = "";
			AssertEquals("Should be empty if not correct length", "", sen.CarrierCode);
			sen.CarrierCode = "F";
			AssertEquals("Should be the same as the value set", "F", sen.CarrierCode);

			sen.VesselCodeFlightNumber = "3D";
			AssertEquals("Should be empty if not correct length", "", sen.VesselCodeFlightNumber);
			sen.VesselCodeFlightNumber = "F33";
			AssertEquals("Should be the same as the value set", "F33", sen.VesselCodeFlightNumber);

			sen.ArrivalDepartureDayMonth = "309";
			AssertEquals("Should be empty if not correct length", "", sen.ArrivalDepartureDayMonth);
			sen.ArrivalDepartureDayMonth = "3009";
			AssertEquals("Should be the same as the value set", "3009", sen.ArrivalDepartureDayMonth);

			sen.ArrivalDepartureYear = "";
			AssertEquals("Should be empty if not correct length", "", sen.ArrivalDepartureYear);
			sen.ArrivalDepartureYear = "8";
			AssertEquals("Should be the same as the value set", "8", sen.ArrivalDepartureYear);

			sen.PortOfLoadingCountryCode = "A";
			AssertEquals("Should be empty if not correct length", "", sen.PortOfLoadingCountryCode);
			sen.PortOfLoadingCountryCode = "AU";
			AssertEquals("Should be the same as the value set", "AU", sen.PortOfLoadingCountryCode);

			sen.PortOfLoadingPortCode = "SY";
			AssertEquals("Should be empty if not correct length", "", sen.PortOfLoadingPortCode);
			sen.PortOfLoadingPortCode = "SYD";
			AssertEquals("Should be the same as the value set", "SYD", sen.PortOfLoadingPortCode);

			sen.CarrierNumber = "W12";
			AssertEquals("Should be empty if not correct length", "", sen.CarrierNumber);
			sen.CarrierNumber = "W123";
			AssertEquals("Should be the same as the value set", "W123", sen.CarrierNumber);

			sen.CheckDigit = "";
			AssertEquals("Should be empty if not correct length", "", sen.CheckDigit);
			sen.CheckDigit = "3";
			AssertEquals("Should be the same as the value set", "3", sen.CheckDigit);
			#endregion

			sen.VesselCodeFlightNumber = "111";
			AssertEquals("should be air", true, sen.IsAir);

			sen.VesselCodeFlightNumber = "A23";
			AssertEquals("should be sea", true, sen.IsSea);

			sen.PortOfLoadingCountryCode = Core.Constants.CountryCodes.Iceland;
			AssertEquals("should be export", true, sen.IsExport);

			sen.PortOfLoadingCountryCode = Core.Constants.CountryCodes.Hungary;
			AssertEquals("should be import", true, sen.IsImport);

			sen.FormatCode("SD6187358DFRTLU539");
			AssertEquals("G", sen.GenerateCheckDigitFromCode());

			sen.FormatCode("A-BBB-CCCC-D-EE-FFF-GGGG-H");
			AssertEquals("Has valid length", true, sen.HasValidLength);

			sen.FormatCode("A-BBB-CCCC-D-EE-FFF-GGGG-");
			AssertEquals("Has valid length", false, sen.HasValidLength);
		}

		public void TestCode()
		{
			Sendingarnumer sen = new Sendingarnumer("");
			AssertEquals("code should be empty", "", sen.Code);

			sen.FormatCode("G");
			AssertEquals("code should be G", "G", sen.Code);

			sen.FormatCode("G12");
			AssertEquals("code should be", "G-12", sen.Code);

			sen.FormatCode("G123");
			AssertEquals("code should be", "G-123", sen.Code);

			sen.FormatCode("G123456");
			AssertEquals("code should be", "G-123-456", sen.Code);

			sen.FormatCode("G1234567");
			AssertEquals("code should be", "G-123-4567", sen.Code);

			sen.FormatCode("G12345678");
			AssertEquals("code should be", "G-123-4567-8", sen.Code);

			sen.FormatCode("G12345678A");
			AssertEquals("code should be", "G-123-4567-8-A", sen.Code);

			sen.FormatCode("G12345678AU");
			AssertEquals("code should be", "G-123-4567-8-AU", sen.Code);

			sen.FormatCode("G12345678AUSY");
			AssertEquals("code should be", "G-123-4567-8-AU-SY", sen.Code);

			sen.FormatCode("G12345678AUSYD");
			AssertEquals("code should be", "G-123-4567-8-AU-SYD", sen.Code);

			sen.FormatCode("G12345678AUSYDW12");
			AssertEquals("code should be", "G-123-4567-8-AU-SYD-W12", sen.Code);

			sen.FormatCode("G12345678AUSYDW123");
			AssertEquals("code should be", "G-123-4567-8-AU-SYD-W123", sen.Code);

			sen.FormatCode("G12345678AUSYDW123P");
			AssertEquals("code should be", "G-123-4567-8-AU-SYD-W123-P", sen.Code);

			sen.FormatCode("G12__--3!!45+==678&&A(U*SY&DW12$#3--@P");
			AssertEquals("code should be", "G-123-4567-8-AU-SYD-W123-P", sen.Code);
		}
	}
}
