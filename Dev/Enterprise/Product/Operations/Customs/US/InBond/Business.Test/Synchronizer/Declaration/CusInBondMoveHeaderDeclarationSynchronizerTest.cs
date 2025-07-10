using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest
	{
		public void TestSynchronizeCusInBondMoveHeaderFor61()
		{
			var transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USPHL";
			transport.JW_ETD = new ZDateTime(2012, 1, 13);
			transport.JW_ETA = new ZDateTime(2012, 1, 14);

			transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "ROA";
			transport.JW_RL_NKLoadPort = "USPHL";
			transport.JW_RL_NKDiscPort = "USCHI";
			transport.JW_ETD = new ZDateTime(2012, 1, 15);
			transport.JW_ETA = new ZDateTime(2012, 1, 16);
			declaration.US_SchDEntry = "";

			AssertEquals(1, header.MovementHeaders.Count);
			var moveHeader = header.MovementHeaders[0];
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("US Destination", "3901", moveHeader.BM_DestinationPortCode);
			AssertEquals("moveHeader.BM_DestinationPortCodeInfo.ReadOnly", true, moveHeader.BM_DestinationPortCodeInfo.ReadOnly);
			AssertEquals("Foreing Dest", "", moveHeader.BM_ForeignDestPortKCode);
			AssertEquals("moveHeader.BM_ForeignDestPortKCodeInfo.ReadOnly", true, moveHeader.BM_ForeignDestPortKCodeInfo.ReadOnly);

			declaration.JE_RL_NKFinalDestination = "USLAX";
			transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "AIR";
			transport.JW_RL_NKLoadPort = "USCHI";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2012, 1, 17);
			transport.JW_ETA = new ZDateTime(2012, 1, 18);
			AssertEquals("US Destination - multiple AIR mapping", ZString.Empty, moveHeader.BM_DestinationPortCode);
			AssertEquals("Foreing Dest", "", moveHeader.BM_ForeignDestPortKCode);
			declaration.US_SchDEntry = "3902";
			AssertEquals("US Destination", "3902", moveHeader.BM_DestinationPortCode);
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
		}

		public void TestBM_DestinationPortcodeINDeclarationSynchroniser()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "UST~";

			var map = Factory.NewWithValidTestData<RefLocoMap>();
			map.RY_SystemUsage = "SEA";
			map.RY_LocalPortCode = "88991";
			map.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			map.RY_RL_NKLocoPort = port.RL_Code;
			Factory.Save();

			declaration.US_SchDEntry = "";
			declaration.Transports.RemoveAndDeleteAll();
			var transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2021, 06, 13);
			transport.JW_ETA = new ZDateTime(2021, 06, 14);
			transport.JW_RL_NKDiscPort = "UST~";

			AssertEquals(1, header.MovementHeaders.Count);
			var moveHeader = header.MovementHeaders[0];
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("US Destination should be 4 digits.", "8899", moveHeader.BM_DestinationPortCode);
		}

		public void TestSynchronizeCusInBondMoveHeaderFor62()
		{
			var transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USPHL";
			transport.JW_ETD = new ZDateTime(2012, 1, 13);
			transport.JW_ETA = new ZDateTime(2012, 1, 14);

			transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "ROA";
			transport.JW_RL_NKLoadPort = "USPHL";
			transport.JW_RL_NKDiscPort = "USCHI";
			transport.JW_ETD = new ZDateTime(2012, 1, 15);
			transport.JW_ETA = new ZDateTime(2012, 1, 16);

			transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "AIR";
			transport.JW_RL_NKLoadPort = "USCHI";
			transport.JW_RL_NKDiscPort = "GBTIL";
			transport.JW_ETD = new ZDateTime(2012, 1, 17);
			transport.JW_ETA = new ZDateTime(2012, 1, 18);

			AssertEquals(1, header.MovementHeaders.Count);
			var moveHeader = header.MovementHeaders[0];
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("US Destination", "3901", moveHeader.BM_DestinationPortCode);
			AssertEquals("moveHeader.BM_DestinationPortCodeInfo.ReadOnly", true, moveHeader.BM_DestinationPortCodeInfo.ReadOnly);
			AssertEquals("Foreing Dest", "41380", moveHeader.BM_ForeignDestPortKCode);
			AssertEquals("moveHeader.BM_ForeignDestPortKCodeInfo.ReadOnly", true, moveHeader.BM_ForeignDestPortKCodeInfo.ReadOnly);
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
		}

		public void TestSynchronizeCusInBondMoveHeaderFor63()
		{
			var transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USPHL";
			transport.JW_ETD = new ZDateTime(2012, 1, 13);
			transport.JW_ETA = new ZDateTime(2012, 1, 14);

			transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "USPHL";
			transport.JW_RL_NKDiscPort = "GBTIL";
			transport.JW_ETD = new ZDateTime(2012, 1, 15);
			transport.JW_ETA = new ZDateTime(2012, 1, 16);

			AssertEquals(1, header.MovementHeaders.Count);
			var moveHeader = header.MovementHeaders[0];
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals("US Destination", "", moveHeader.BM_DestinationPortCode);
			AssertEquals("moveHeader.BM_DestinationPortCodeInfo.ReadOnly", true, moveHeader.BM_DestinationPortCodeInfo.ReadOnly);
			AssertEquals("Foreing Dest", "41380", moveHeader.BM_ForeignDestPortKCode);
			AssertEquals("moveHeader.BM_ForeignDestPortKCodeInfo.ReadOnly", true, moveHeader.BM_ForeignDestPortKCodeInfo.ReadOnly);

			transport.JW_RL_NKDiscPort = "USLAX";

			transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2012, 1, 17);
			transport.JW_ETA = new ZDateTime(2012, 1, 18);

			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "ITAA~";

			var map = Factory.NewWithValidTestData<RefLocoMap>();
			map.RY_SystemUsage = "SCK";
			map.RY_LocalPortCode = "89991";
			map.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			map.RY_RL_NKLocoPort = port.RL_Code;

			transport.JW_RL_NKDiscPort = port.RL_Code;
			AssertEquals("US Destination", "", moveHeader.BM_DestinationPortCode);
			AssertEquals("Foreing Dest", "89991", moveHeader.BM_ForeignDestPortKCode);
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
		}
	}
}