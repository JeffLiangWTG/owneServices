using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class USConsolDataCalculatorTest : AMSSynchroniserTestCase
	{
		public void TestGetSCAC()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ORG1";
			carrier.UI_ModeOfTransportation = "11";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "ORG2";
			carrier2.UI_ModeOfTransportation = "11";
			var carrier3 = Factory.New<USCarrierCombined>();
			carrier3.UI_Code = "ORGP";
			carrier3.UI_ModeOfTransportation = "11";
			AssertEquals(ZString.Empty, calculator.GetSCAC(null));
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, calculator.GetSCAC(orgProxy));
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, calculator.GetSCAC(org1));
			AssertEquals(org2PrefixCarrierCode.OK_CustomsRegNo, calculator.GetSCAC(org2));
			AssertEquals(ZString.Empty, calculator.GetSCAC(Factory.New<OrgHeader>()));
		}

		public void TestOrgProxySCAC()
		{
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "OTT2";
			carrier2.UI_ModeOfTransportation = "11";
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, calculator.OrgProxySCAC);
			header.BH_GB = otherCompanyBranch.PK;
			AssertEquals(otherBranchOrgProxyCarrierCode.OK_CustomsRegNo, calculator.OrgProxySCAC);
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, calculator.OrgProxySCAC);
		}

		public void TestGetInfosAffectingOrgProxySCAC()
		{
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingOrgProxySCAC());
			AssertEquals(1, list.Count);
			AssertCollectionContains(header.BH_GBInfo, list);
		}

		public void TestSendingForwarderSCAC()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ORG1";
			carrier.UI_ModeOfTransportation = "11";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "ORG2";
			carrier2.UI_ModeOfTransportation = "11";
			var carrier3 = Factory.New<USCarrierCombined>();
			carrier3.UI_Code = "ORGP";
			carrier3.UI_ModeOfTransportation = "11";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, calculator.SendingForwarderSCAC);
			consol.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			AssertEquals(org2PrefixCarrierCode.OK_CustomsRegNo, calculator.SendingForwarderSCAC);
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			AssertEquals(ZString.Empty, calculator.SendingForwarderSCAC);
		}

		public void TestCarrierSCAC()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ORG1";
			carrier.UI_ModeOfTransportation = "11";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, calculator.CarrierSCAC);
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, calculator.CarrierSCAC);
		}

		public void TestLastForeignPortOfLoading()
		{
			AssertNull(calculator.LastForeignPortOfLoading);
			consol.JK_RL_NKLastForeignPort = AUSYD.RL_Code;
			consol.JK_DateLastForeignPort = new ZDateTime(2011, 4, 10);
			AssertEquals(AUSYD, calculator.LastForeignPortOfLoading);
			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport1.JW_RL_NKDiscPort = USLAX.RL_Code;
			transport1.JW_ETD = new ZDateTime(2011, 4, 9);
			transport1.JW_ATD = new ZDateTime(2011, 4, 11);
			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = USLAX.RL_Code;
			transport2.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport2.JW_ATD = new ZDateTime(2011, 4, 12);
			AssertEquals(AUMEL, calculator.LastForeignPortOfLoading);
			transport1.JW_ATD = ZDateTime.Empty;
			AssertEquals(AUSYD, calculator.LastForeignPortOfLoading);
			consol.JK_RL_NKLastForeignPort = ZString.Empty;
			AssertEquals(AUMEL, calculator.LastForeignPortOfLoading);
			consol.JK_RL_NKLastForeignPort = USLAX.RL_Code;
			AssertEquals(AUMEL, calculator.LastForeignPortOfLoading);
		}

		public void TestGetInfosAffectingLastForeignPortOfLoading()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingLastForeignPortOfLoading());
			AssertEquals(15, list.Count);
			AssertCollectionContains(consol.JK_RL_NKLastForeignPortInfo, list);
			AssertCollectionContains(consol.JK_DateLastForeignPortInfo, list);
			AssertCollectionContains(header.BH_ImportTransportModeInfo, list);
			AssertCollectionContains(transport1.JW_LegOrderInfo, list);
			AssertCollectionContains(transport1.JW_TransportModeInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport1.JW_ETDInfo, list);
			AssertCollectionContains(transport1.JW_ATDInfo, list);
			AssertCollectionContains(transport2.JW_LegOrderInfo, list);
			AssertCollectionContains(transport2.JW_TransportModeInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport2.JW_ETDInfo, list);
			AssertCollectionContains(transport2.JW_ATDInfo, list);
		}

		public void TestGetInfosAffectingFirstUSPortOfDischarge()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingFirstCountryPortOfDischarge());
			AssertEquals(15, list.Count);
			AssertCollectionContains(header.BH_ImportTransportModeInfo, list);
		}

		public void TestGetInfosAffectingFirstUSDischargeDate()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingFirstCountryDischargeDate());
			AssertEquals(15, list.Count);
			AssertCollectionContains(header.BH_ImportTransportModeInfo, list);
		}

		public void TestGetInfosAffectingFirstUSBoundTransportOrDepartureTransport()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingTransportsOrder());
			AssertEquals(9, list.Count);
			AssertCollectionContains(header.BH_ImportTransportModeInfo, list);
		}

		public void TestGetInfosAffectingFirstCarrierContractualTransport()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingFirstCarrierContractualTransport());
			AssertEquals(13, list.Count);
			AssertCollectionContains(header.BH_ImportTransportModeInfo, list);
		}

		public void TestBillOfLadingStatusCode()
		{
			AssertEquals("MVOCC", false, header.IsNVOCCHeader);

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.HouseBill, calculator.BillOfLadingStatusCode);
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard, calculator.BillOfLadingStatusCode);

			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			AssertEquals("NVOCC leg 1", true, header.IsNVOCCHeader);
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.FROB, calculator.BillOfLadingStatusCode);

			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = USLAX.RL_Code;
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.HouseBill, calculator.BillOfLadingStatusCode);

			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			AssertEquals("NVOCC leg 2", true, header.IsNVOCCHeader);
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.HouseBill, calculator.BillOfLadingStatusCode);

			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_Vessel = "VESSEL";
			transport3.JW_RL_NKLoadPort = USLAX.RL_Code;
			transport3.JW_RL_NKDiscPort = USCHI.RL_Code;
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.HouseBill, calculator.BillOfLadingStatusCode);

			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport4.JW_LegOrder = 4;
			transport4.JW_Vessel = "VESSEL";
			transport4.JW_RL_NKLoadPort = USCHI.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard, calculator.BillOfLadingStatusCode);

			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			AssertEquals("NVOCC leg 4", true, header.IsNVOCCHeader);
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.FROB, calculator.BillOfLadingStatusCode);
		}

		public void TestGetInfosAffectingBillOfLadingStatusCode()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingBillOfLadingStatusCode());
			AssertEquals(11, list.Count);
			AssertCollectionContains(header.BH_ImportTransportModeInfo, list);
			AssertCollectionContains(transport1.JW_LegOrderInfo, list);
			AssertCollectionContains(transport1.JW_TransportModeInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport1.JW_VesselInfo, list);
			AssertCollectionContains(transport2.JW_LegOrderInfo, list);
			AssertCollectionContains(transport2.JW_TransportModeInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport2.JW_VesselInfo, list);
		}

		public void TestLoadTransportForUSBoundVessel()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(transport1, calculator.LoadTransportForUSBoundVessel);
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = USLAX.RL_Code;
			AssertEquals(transport1, calculator.LoadTransportForUSBoundVessel);
			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_Vessel = "VESSEL";
			transport3.JW_RL_NKLoadPort = USLAX.RL_Code;
			transport3.JW_RL_NKDiscPort = USCHI.RL_Code;
			AssertEquals(transport1, calculator.LoadTransportForUSBoundVessel);
			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_RL_NKLoadPort = USCHI.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals(transport1, calculator.LoadTransportForUSBoundVessel);
			transport1.JW_Vessel = "VESSEL1";
			AssertEquals(transport2, calculator.LoadTransportForUSBoundVessel);
		}

		public void TestGetInfosAffectingLoadTransportForUSBoundVessel()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingLoadTransportForUSBoundVessel());
			AssertEquals(11, list.Count);
			AssertCollectionContains(header.BH_ImportTransportModeInfo, list);
			AssertCollectionContains(transport1.JW_LegOrderInfo, list);
			AssertCollectionContains(transport1.JW_TransportModeInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport1.JW_VesselInfo, list);
			AssertCollectionContains(transport2.JW_LegOrderInfo, list);
			AssertCollectionContains(transport2.JW_TransportModeInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport2.JW_VesselInfo, list);
		}

		ConsolDataCalculator calculator;
		protected override void SetUp()
		{
			base.SetUp();
			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselNonContainer;
			calculator = new ConsolDataCalculator(consol, header);
		}

		protected override void TearDown()
		{
			calculator.Dispose();
			calculator = null;
			base.TearDown();
		}
	}
}
