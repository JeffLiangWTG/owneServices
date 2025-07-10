using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class VesselRoutingLineRecordTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			var csvLines = new string[][]
			{
				new string[] { "TermID", "TerminalName", "ShipName", "1234548", "999S", "DischargeCountry", "DischargePortName", "DPCde", "DischargePortState" },
				new string[] { "LongTerminalCode", "TerminalName", "ShipName", "1234548", "999S", "DischargeCountry", "DischargePortName", "DPCde", "DischargePortState" },
				new string[] { "TermID", "TerminalName", "ShipName", "123454812345481", "999S", "DischargeCountry", "DischargePortName", "DPCde", "DischargePortState" },
				new string[] { "TermID", "TerminalName", "ShipName", "1234548", "LongVoyageNum", "DischargeCountry", "DischargePortName", "DPCde", "DischargePortState" },
				new string[] { "TermID", "TerminalName", "ShipName", "1234548", "999S", "DischargeCountry", "DischargePortName", "LongDischargePortCode", "DischargePortState" }
			};

			AssertIsValid("Valid", csvLines[0], true);
			AssertIsValid("Terminal Code too long", csvLines[1], false);
			AssertIsValid("Lloyds ID too long", csvLines[2], false);
			AssertIsValid("Voyage Number too long", csvLines[3], false);
			AssertIsValid("Discharge port code too long", csvLines[4], false);
		}

		void AssertIsValid(string message, string[] csvLine, bool expectedIsValid)
		{
			var record = new VesselRoutingLineRecord(csvLine);
			AssertEquals(message, expectedIsValid, record.IsValid());
		}
	}
}
