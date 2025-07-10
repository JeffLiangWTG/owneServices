using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondHeaderSynchroniserTest : AMSSynchroniserTestCase
	{
		public void TestSynchroniseBH_CarrierSCAC()
		{
			AssertEquals("OTT1", orgProxyCarrierCode.OK_CustomsRegNo);
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo.ToUpper(), header.BH_CarrierSCAC);
		}

		public void TestSynchroniseBH_ImportTransportMode()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(ZString.Empty, header.BH_ImportTransportMode);

			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(TransportTypeList.Codes.Rail, header.BH_ImportTransportMode);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(TransportTypeList.Codes.VesselNonContainer, header.BH_ImportTransportMode);

			var container1 = consol.Containers.AddNew();
			AssertEquals(TransportTypeList.Codes.VesselContainer, header.BH_ImportTransportMode);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(TransportTypeList.Codes.VesselContainer, header.BH_ImportTransportMode);

			var container2 = consol.Containers.AddNew();
			AssertEquals(TransportTypeList.Codes.VesselContainer, header.BH_ImportTransportMode);

			container2.JC_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(TransportTypeList.Codes.VesselContainer, header.BH_ImportTransportMode);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(TransportTypeList.Codes.VesselNonContainer, header.BH_ImportTransportMode);
		}

		public void TestSynchroniseVesselDepartureFields()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			AssertEquals("Load Port", "AUSYD", header.BH_RL_NKImportLoadPort);
			AssertEquals("ETD", ZDateTime.Empty, header.BH_FirstExportDate);

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_ETD = new ZDateTime(2016, 10, 09, 15, 30, 00);

			AssertEquals("Load Port", "AUSYD", header.BH_RL_NKImportLoadPort);
			AssertEquals("ETD", new ZDateTime(2016, 10, 09, 15, 30, 00), header.BH_FirstExportDate);
		}

		public void TestSynchroniseVesselDepartureFieldsOnDifferentCountries()
		{
			AssertSynchroniseVesselDepartureFields_ShouldNotVaryFromCountries(Core.Constants.CountryCodes.Australia);
			AssertSynchroniseVesselDepartureFields_ShouldNotVaryFromCountries(Core.Constants.CountryCodes.UnitedStates);
		}

		void AssertSynchroniseVesselDepartureFields_ShouldNotVaryFromCountries(string countryCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var date20180101 = new ZDateTime(2018, 01, 01);
				var date20180201 = new ZDateTime(2018, 02, 01);

				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				var transport1 = consol.Transports[0];
				SetTransport(transport1, 1, Core.Constants.TransportModes.Sea, AUSYD.RL_Code, SGSIN.RL_Code, date20180101);

				var transport2 = consol.Transports.AddNew();
				SetTransport(transport2, 2, Core.Constants.TransportModes.Sea, SGSIN.RL_Code, USCHI.RL_Code, date20180201);

				AssertEquals("Load Port", SGSIN.RL_Code, header.BH_RL_NKImportLoadPort);
				AssertEquals("ETD", date20180201, header.BH_FirstExportDate);
			}
		}

		public void TestMostInterestingTransportShouldSyncToAMS()
		{
			var date20180101 = new ZDateTime(2018, 01, 01);
			var date20180201 = new ZDateTime(2018, 02, 01);
			var date20180301 = new ZDateTime(2018, 03, 01);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var transport1 = consol.Transports[0];
			SetTransport(transport1, 1, Core.Constants.TransportModes.Sea, AUSYD.RL_Code, USLAX.RL_Code, date20180101);

			var transport2 = consol.Transports.AddNew();
			SetTransport(transport2, 2, Core.Constants.TransportModes.Sea, SGSIN.RL_Code, USCHI.RL_Code, date20180201);

			AssertEquals("Load Port", SGSIN.RL_Code, header.BH_RL_NKImportLoadPort);
			AssertEquals("ETD", date20180201, header.BH_FirstExportDate);

			transport1.JW_LegOrder = 3;
			AssertEquals("The most interesting transport should be the last one.", AUSYD.RL_Code, header.BH_RL_NKImportLoadPort);
			AssertEquals("The most interesting transport should be the last one.", date20180101, header.BH_FirstExportDate);

			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("The most interesting transport should be the same transport mode as consol.", SGSIN.RL_Code, header.BH_RL_NKImportLoadPort);
			AssertEquals("The most interesting transport should be the same transport mode as consol.", date20180201, header.BH_FirstExportDate);

			transport2.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport2.JW_ETD = date20180301;
			AssertEquals("AMS should be synchronized with the most interesting transport.", AUMEL.RL_Code, header.BH_RL_NKImportLoadPort);
			AssertEquals("AMS should be synchronized with the most interesting transport.", date20180301, header.BH_FirstExportDate);
		}

		public void TestMostInterestingTransportShouldBeLastForeignToUSLeg()
		{
			var date20180101 = new ZDateTime(2018, 01, 01);
			var date20180102 = new ZDateTime(2018, 01, 02);
			var date20180103 = new ZDateTime(2018, 01, 03);
			var date20180104 = new ZDateTime(2018, 01, 04);
			var date20180105 = new ZDateTime(2018, 01, 05);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			// We are looking for the last interesting transport, not first.
			var transport1 = consol.Transports[0];
			SetTransport(transport1, 1, Core.Constants.TransportModes.Sea, AUSYD.RL_Code, USCHI.RL_Code, date20180101);

			// This is the last one which is from foreign to US, yep, this is the one we are looking for.
			var transport2 = consol.Transports.AddNew();
			SetTransport(transport2, 2, Core.Constants.TransportModes.Sea, AUMEL.RL_Code, USCHI.RL_Code, date20180102);

			// This is from foreign to US, we are not interested in it.
			var transport3 = consol.Transports.AddNew();
			SetTransport(transport3, 3, Core.Constants.TransportModes.Sea, SGSIN.RL_Code, AUMEL.RL_Code, date20180103);

			// This is from US to US, we are not interested it.
			var transport4 = consol.Transports.AddNew();
			SetTransport(transport4, 4, Core.Constants.TransportModes.Sea, USCHI.RL_Code, USLAX.RL_Code, date20180104);

			// This is the last one, but it is from US to foreign.
			var transport5 = consol.Transports.AddNew();
			SetTransport(transport5, 5, Core.Constants.TransportModes.Sea, USLAX.RL_Code, SGSIN.RL_Code, date20180105);

			AssertEquals("Load Port", AUMEL.RL_Code, header.BH_RL_NKImportLoadPort);
			AssertEquals("ETD", date20180102, header.BH_FirstExportDate);
		}

		public void TestBH_LloydsNumberCapitalLetters()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "vessel";
			vessel.RV_LloydsNumber = "1234a";
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "vessel";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals("VESSEL", header.BH_ImportConveyanceName);
			AssertEquals("1234A", header.BH_LloydsNumber);
		}

		public void TestSynchroniseBH_ImportConveyanceName()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL 1";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals("VESSEL 1", header.BH_ImportConveyanceName);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL 2";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = USLAX.RL_Code;
			AssertEquals("VESSEL 2", header.BH_ImportConveyanceName);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_Vessel = "VESSEL 3";
			transport3.JW_RL_NKLoadPort = USLAX.RL_Code;
			transport3.JW_RL_NKDiscPort = USCHI.RL_Code;
			AssertEquals("VESSEL 2", header.BH_ImportConveyanceName);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_Vessel = "VESSEL 4";
			transport4.JW_RL_NKLoadPort = USCHI.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals("VESSEL 2", header.BH_ImportConveyanceName);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("VESSEL 1", header.BH_ImportConveyanceName);

			transport1.JW_Vessel = "VESSEL 1A";
			AssertEquals("VESSEL 1A", header.BH_ImportConveyanceName);

			transport3.JW_RL_NKLoadPort = SGSIN.RL_Code;
			AssertEquals("VESSEL 3", header.BH_ImportConveyanceName);

			consol.JK_RL_NKPortOfFirstArrival = USCHI.RL_Code;
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2024, 03, 13);
			transport2.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport2.JW_ETD = new ZDateTime(2024, 03, 10);
			transport2.JW_ETA = new ZDateTime(2024, 03, 12);
			transport3.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_ETD = new ZDateTime(2024, 03, 12);
			transport3.JW_ETA = new ZDateTime(2024, 03, 14);
			transport4.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport4.JW_ETD = new ZDateTime(2024, 03, 12);
			transport4.JW_ETA = new ZDateTime(2024, 03, 14);
			transport4.JW_Vessel = "VESSEL 5";
			AssertEquals("VESSEL 5", header.BH_ImportConveyanceName);
		}

		public void TestSynchroniseBH_VoyageNumber()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_VoyageFlight = "V1";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals("V1", header.BH_VoyageNumber);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_VoyageFlight = "V2";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = USLAX.RL_Code;
			AssertEquals("V2", header.BH_VoyageNumber);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_VoyageFlight = "V3";
			transport3.JW_RL_NKLoadPort = USLAX.RL_Code;
			transport3.JW_RL_NKDiscPort = USCHI.RL_Code;
			AssertEquals("V2", header.BH_VoyageNumber);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_VoyageFlight = "V4";
			transport4.JW_RL_NKLoadPort = USCHI.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals("V2", header.BH_VoyageNumber);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("V1", header.BH_VoyageNumber);

			transport1.JW_VoyageFlight = "V1A";
			AssertEquals("V1A", header.BH_VoyageNumber);

			transport3.JW_RL_NKLoadPort = SGSIN.RL_Code;
			AssertEquals("V3", header.BH_VoyageNumber);

			consol.JK_RL_NKPortOfFirstArrival = USCHI.RL_Code;
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2024, 03, 13);
			transport2.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport2.JW_ETD = new ZDateTime(2024, 03, 10);
			transport2.JW_ETA = new ZDateTime(2024, 03, 12);
			transport3.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_ETD = new ZDateTime(2024, 03, 12);
			transport3.JW_ETA = new ZDateTime(2024, 03, 14);
			transport4.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport4.JW_ETD = new ZDateTime(2024, 03, 12);
			transport4.JW_ETA = new ZDateTime(2024, 03, 14);
			transport4.JW_VoyageFlight = "V5";
			AssertEquals("V5", header.BH_VoyageNumber);
		}

		public void TestSynchroniseBH_VoyageNumberWhenMoreThanFiveDigits()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_VoyageFlight = "abc123";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals("ABC12", header.BH_VoyageNumber);
		}

		public void TestSynchroniseBH_VoyageNumberCapitalLetters()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_VoyageFlight = "abc";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals("ABC", header.BH_VoyageNumber);
		}

		public void TestSynchroniseBH_RL_NKPortUnlading()
		{
			AssertEquals(ZString.Empty, header.BH_RL_NKPortUnlading);
			consol.JK_RL_NKPortOfFirstArrival = USLAX.RL_Code;
			AssertEquals(USLAX.RL_Code, header.BH_RL_NKPortUnlading);

			consol.JK_DatePortOfFirstArrival = new ZDateTime(2011, 4, 10);
			AssertEquals(USLAX.RL_Code, header.BH_RL_NKPortUnlading);

			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport1.JW_ETA = new ZDateTime(2011, 4, 11);
			AssertEquals("Consol's PortOfFirstArrival is earlier", USLAX.RL_Code, header.BH_RL_NKPortUnlading);

			transport1.JW_RL_NKDiscPort = USNYC.RL_Code;
			transport1.JW_ETA = new ZDateTime(2011, 4, 11);
			AssertEquals("Consol's PortOfFirstArrival is earlier", USNYC.RL_Code, header.BH_RL_NKPortUnlading);

			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport1.JW_ATA = new ZDateTime(2011, 4, 8);
			AssertEquals(USLAX.RL_Code, header.BH_RL_NKPortUnlading);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = USCHI.RL_Code;
			transport2.JW_ATA = new ZDateTime(2011, 4, 7);

			AssertEquals("First Leg", USCHI.RL_Code, header.BH_RL_NKPortUnlading);

			transport1.JW_RL_NKLoadPort = USNYC.RL_Code;
			AssertEquals("First Leg is not US Bound", USCHI.RL_Code, header.BH_RL_NKPortUnlading);
		}

		public void TestSynchroniseBH_RL_NKPortUnladingWithLowercaseLetters()
		{
			AssertEquals(ZString.Empty, header.BH_RL_NKPortUnlading);
			USLAX.RL_Code = "abc";
			consol.JK_RL_NKPortOfFirstArrival = USLAX.RL_Code;
			AssertEquals(USLAX.RL_Code.ToUpper(), header.BH_RL_NKPortUnlading);
		}

		public void TestSynchroniseBH_ETAForPRProt()
		{
			AssertEquals(ZDateTime.Empty, header.BH_ETA);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "ABCD1234";
			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport1.JW_RL_NKDiscPort = PRADJ.RL_Code;
			transport1.JW_ETA = new ZDateTime(2011, 4, 11);
			AssertEquals("US ports and PR ports should be treated the same, add the default of the Est Unloading date for PR ports", new ZDateTime(2011, 4, 11), header.BH_ETA);
			AssertEquals(PRADJ.RL_Code, header.BH_RL_NKPortUnlading);

			AssertEquals(1, header.Bills.Count);
			var bill1 = header.Bills[0];
			AssertEquals("N", bill1.B0_BillStatus);
		}

		protected RefUNLOCO PRADJ
		{
			get { return pradj ?? (pradj = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "PRADJ")); }
		}
		RefUNLOCO pradj;

		public void TestSynchroniseBH_ETAorUSProt()
		{
			AssertEquals(ZDateTime.Empty, header.BH_ETA);
			consol.JK_RL_NKPortOfFirstArrival = USLAX.RL_Code;
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2011, 4, 10);
			AssertEquals(new ZDateTime(2011, 4, 10), header.BH_ETA);

			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport1.JW_ETA = new ZDateTime(2011, 4, 11);
			AssertEquals("Consol's PortOfFirstArrival is earlier", new ZDateTime(2011, 4, 10), header.BH_ETA);

			transport1.JW_RL_NKDiscPort = USNYC.RL_Code;
			AssertEquals("Consol's PortOfFirstArrival is earlier", new ZDateTime(2011, 4, 11), header.BH_ETA);

			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport1.JW_ATA = new ZDateTime(2011, 4, 8);
			AssertEquals(new ZDateTime(2011, 4, 10), header.BH_ETA);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = USCHI.RL_Code;
			transport2.JW_ATA = new ZDateTime(2011, 4, 7);
			AssertEquals("First Leg", new ZDateTime(2011, 4, 10), header.BH_ETA);

			transport1.JW_RL_NKLoadPort = USNYC.RL_Code;
			AssertEquals("First Leg is not US Bound", new ZDateTime(2011, 4, 7), header.BH_ETA);
		}

		public void TestSynchroniseBills()
		{
			AssertEquals(0, header.Bills.Count);
			AssertEquals("OTT1", orgProxyCarrierCode.OK_CustomsRegNo);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "AB1D1234";

			AssertEquals(1, header.Bills.Count);
			var bill1 = header.Bills[0];
			AssertBill(bill1, "OTT1", "AB1D1234");

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "AB1D5678";

			AssertEquals(2, header.Bills.Count);
			AssertEquals(bill1, header.Bills["OTT1", "AB1D1234"]);
			var bill2 = header.Bills["OTT1", "AB1D5678"];
			AssertNotNull(bill2);

			shipment1.Delete();
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], bill2.PK, "OTT1", "AB1D5678");
		}

		void SetTransport(Transport transport, ZByte legOrder, ZString transportMode, ZString loadPort, ZString discPort, ZDateTime etd)
		{
			transport.JW_LegOrder = legOrder;
			transport.JW_TransportMode = transportMode;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_ETD = etd;
		}

		CusInBondHeaderSynchroniser synchroniser;

		protected override void SetUp()
		{
			base.SetUp();

			orgProxyCarrierCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "Ott1", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			synchroniser = new CusInBondHeaderSynchroniser(header);
			synchroniser.Synchronise(true);
		}
	}
}
