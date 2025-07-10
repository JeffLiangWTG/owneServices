using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class ViewVesselRoutingPortPairsTestCase : TestCaseWithFactory
	{
		public void TestPortPairs_OneDomesticPortNoForeignPorts()
		{
			NewJobVesselSchedule("AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), "Voyage", "Voyage");
			Factory.Save();

			AssertPortPairCount("Voyage", 2);
			AssertPortPair("Voyage", "", "AUSYD", ZDateTime.Empty, new ZDateTime(2000, 1, 1));
			AssertPortPair("Voyage", "AUSYD", "", new ZDateTime(2000, 1, 2), ZDateTime.Empty);
		}

		public void TestPortPairs_TwoDomesticPortsNoForeignPorts()
		{
			NewJobVesselSchedule("AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), "Voyage", "Voyage");
			NewJobVesselSchedule("AUMEL", new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4), "Voyage", "Voyage");
			Factory.Save();

			AssertPortPairCount("Voyage", 5);
			AssertPortPair("Voyage", "", "AUSYD", ZDateTime.Empty, new ZDateTime(2000, 1, 1));
			AssertPortPair("Voyage", "", "AUMEL", ZDateTime.Empty, new ZDateTime(2000, 1, 3));
			AssertPortPair("Voyage", "AUSYD", "AUMEL", new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3));
			AssertPortPair("Voyage", "AUSYD", "", new ZDateTime(2000, 1, 2), ZDateTime.Empty);
			AssertPortPair("Voyage", "AUMEL", "", new ZDateTime(2000, 1, 4), ZDateTime.Empty);
		}

		public void TestPortPairs_OneDomesticPortOneForeignPort()
		{
			NewJobVesselRouting("MYPKG", "Voyage");
			NewJobVesselSchedule("AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), "Voyage", "Voyage");
			Factory.Save();

			AssertPortPairCount("Voyage", 2);
			AssertPortPair("Voyage", "MYPKG", "AUSYD", ZDateTime.Empty, new ZDateTime(2000, 1, 1));
			AssertPortPair("Voyage", "AUSYD", "MYPKG", new ZDateTime(2000, 1, 2), ZDateTime.Empty);
		}

		public void TestPortPairs_TwoDomesticPortsTwoForeignPorts()
		{
			NewJobVesselRouting("MYPKG", "Voyage");
			NewJobVesselRouting("SGSIN", "Voyage");
			NewJobVesselSchedule("AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), "Voyage", "Voyage");
			NewJobVesselSchedule("AUMEL", new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4), "Voyage", "Voyage");
			Factory.Save();

			AssertPortPairCount("Voyage", 9);
			AssertPortPair("Voyage", "MYPKG", "AUSYD", ZDateTime.Empty, new ZDateTime(2000, 1, 1));
			AssertPortPair("Voyage", "SGSIN", "AUSYD", ZDateTime.Empty, new ZDateTime(2000, 1, 1));
			AssertPortPair("Voyage", "MYPKG", "AUMEL", ZDateTime.Empty, new ZDateTime(2000, 1, 3));
			AssertPortPair("Voyage", "SGSIN", "AUMEL", ZDateTime.Empty, new ZDateTime(2000, 1, 3));
			AssertPortPair("Voyage", "AUSYD", "AUMEL", new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3));
			AssertPortPair("Voyage", "AUSYD", "MYPKG", new ZDateTime(2000, 1, 2), ZDateTime.Empty);
			AssertPortPair("Voyage", "AUSYD", "SGSIN", new ZDateTime(2000, 1, 2), ZDateTime.Empty);
			AssertPortPair("Voyage", "AUMEL", "MYPKG", new ZDateTime(2000, 1, 4), ZDateTime.Empty);
			AssertPortPair("Voyage", "AUMEL", "SGSIN", new ZDateTime(2000, 1, 4), ZDateTime.Empty);
		}

		public void TestPortPairs_TwoDomesticPortsOneArrivedFromForeignPort()
		{
			NewJobVesselRouting("MYPKG", "Voyage");
			NewJobVesselSchedule("AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), "Voyage", "Voyage");
			NewJobVesselSchedule("AUMEL", new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4), "Voyage", "VoyageOut");
			Factory.Save();

			AssertPortPairCount("Voyage", 5);
			AssertPortPair("Voyage", "MYPKG", "AUSYD", ZDateTime.Empty, new ZDateTime(2000, 1, 1));
			AssertPortPair("Voyage", "MYPKG", "AUMEL", ZDateTime.Empty, new ZDateTime(2000, 1, 3));
			AssertPortPair("Voyage", "AUSYD", "AUMEL", new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3));
			AssertPortPair("Voyage", "AUSYD", "MYPKG", new ZDateTime(2000, 1, 2), ZDateTime.Empty);
			AssertPortPair("Voyage", "AUMEL", "", new ZDateTime(2000, 1, 4), ZDateTime.Empty);
		}

		public void TestPortPairs_TwoDomesticPortsOneDepartingToForeignPort()
		{
			NewJobVesselSchedule("AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), "Voyage", "Voyage");
			NewJobVesselSchedule("AUMEL", new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4), "Voyage", "VoyageOut");
			NewJobVesselRouting("MYPKG", "VoyageOut");
			Factory.Save();

			AssertPortPairCount("VoyageOut", 2);
			AssertPortPair("VoyageOut", "", "AUMEL", ZDateTime.Empty, new ZDateTime(2000, 1, 3));
			AssertPortPair("VoyageOut", "AUMEL", "MYPKG", new ZDateTime(2000, 1, 4), ZDateTime.Empty);
		}

		#region Implementation

		JobVesselRouting NewJobVesselRouting(ZString portCode, ZString voyage)
		{
			JobVesselRouting result = Factory.New<JobVesselRouting>();
			result.E1_RL_NKDischargePortCode = portCode;
			result.E1_LloydsID = "Lloyds";
			result.E1_VoyageNumber = voyage;
			return result;
		}

		JobVesselSchedule NewJobVesselSchedule(ZString portCode, ZDateTime eTA, ZDateTime eTD, ZString voyageIn, ZString voyageOut)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portCode;
			result.EV_ETD = eTD;
			result.EV_ETA = eTA;
			result.EV_IMOLloydsNumber = "Lloyds";
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			return result;
		}

		void AssertPortPairCount(ZString voyage, int expectedCount)
		{
			VesselRoutingPortPair[] portPairs = LoadPortPairs(voyage);
			AssertEquals("Port Pair Count; all port pairs:\r\n\r\n" + GetPortPairsAsString(voyage), expectedCount, portPairs.Length);
		}

		void AssertPortPair(ZString voyage, ZString expectedLoadPort, ZString expectedDischargePort, ZDateTime eTD, ZDateTime eTA)
		{
			VesselRoutingPortPair match = null;
			foreach (VesselRoutingPortPair portPair in LoadPortPairs(voyage))
			{
				if (portPair.E9_RL_NKLoadPort == expectedLoadPort &&
					portPair.E9_RL_NKDischargePort == expectedDischargePort)
				{
					match = portPair;
				}
			}
			AssertNotNull("Expected to find LoadPort='" + expectedLoadPort + "' DischargePort='" + expectedDischargePort + "'; all port pairs:\r\n\r\n" + GetPortPairsAsString(voyage), match);
			AssertEquals("ETD", eTD, match.E9_ETD);
			AssertEquals("ETA", eTA, match.E9_ETA);
		}

		ZString GetPortPairsAsString(ZString voyage)
		{
			ZString result = "Load   Discharge\r\n";
			VesselRoutingPortPair[] portPairs = LoadPortPairs(voyage);
			foreach (VesselRoutingPortPair portPair in portPairs)
			{
				result += portPair.E9_RL_NKLoadPort.PadRight(5) + "  " + portPair.E9_RL_NKDischargePort + "\r\n";
			}
			return result;
		}

		VesselRoutingPortPair[] LoadPortPairs(ZString voyage)
		{
			return Factory.Load<VesselRoutingPortPair>(new ZQuery(ViewVesselRoutingPortPairsSchema.E9_Voyage, voyage));
		}

		#endregion
	}
}
