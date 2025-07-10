using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondBillSynchroniserTest : AMSSynchroniserTestCase
	{
		public void TestSynchroniseB0_IssuerCode()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ORG1";
			carrier.UI_ModeOfTransportation = "11";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "ORG2";
			carrier2.UI_ModeOfTransportation = "10";
			var carrier3 = Factory.New<USCarrierCombined>();
			carrier3.UI_Code = "SCAC";
			carrier3.UI_ModeOfTransportation = "10";
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			AssertEquals("OTT1", bill.B0_IssuerCode);
			AssertNotEquals("ABCD", org1CarrierCode.OK_CustomsRegNo);
			shipment.JS_HouseBill = "1234";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;
			synchroniser.Synchronise(true);
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, bill.B0_IssuerCode);
			bill.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org2.PK;
			synchroniser.Synchronise(true);
			shipment.JS_HouseBill = "OTT41234";
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, bill.B0_IssuerCode);
			bill.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
			synchroniser.Synchronise(true);
			AssertEquals(org2CarrierCode.OK_CustomsRegNo, bill.B0_IssuerCode);
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var orgProxyCarrierCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			shipment.JS_HouseBill = "SCAC1234"; //SCAC is valid carrier code.
			synchroniser.Synchronise(true);
			AssertEquals("SCAC", bill.B0_IssuerCode);
		}

		public void TestSynchroniseB0_MasterbillNumberWithoutSCAC()
		{
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			bill.ShipmentReferenceDetails.DeleteAll();
			consol.JK_MasterBillNum = "MB1";
			shipment.JS_HouseBill = "OTT1 HB1234";
			bill.B0_IssuerSCAC = "OTT1";
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, bill.B0_IssuerSCAC);
			synchroniser = new CusInBondBillSynchroniser(bill, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals("HB1234", bill.B0_MasterBillNumber);
		}

		public void TestSynchroniseB0_MasterBillNumber()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SCAC";
			carrier.UI_ModeOfTransportation = "11";
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var orgProxyCarrierCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("", bill.B0_MasterBillNumber);
			AssertNotEquals("ABCD", org1CarrierCode.OK_CustomsRegNo);
			shipment.JS_HouseBill = "BN1234";
			AssertEquals("BN1234", bill.B0_MasterBillNumber);
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("BN1234", bill.B0_MasterBillNumber);
			synchroniser.Synchronise(true);
			AssertEquals("BN1234", bill.B0_MasterBillNumber);
			bill.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			shipment.JS_HouseBill = "SCAC6 789";
			AssertEquals("BN1234", bill.B0_MasterBillNumber);
			bill.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
			AssertEquals("6789", bill.B0_MasterBillNumber);
		}

		public void TestSynchroniseB0_BillStatus()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard, bill.B0_BillStatus);
			transport1.JW_RL_NKDiscPort = USCHI.RL_Code;
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = USLAX.RL_Code;
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.HouseBill, bill.B0_BillStatus);
		}

		public void TestSynchroniseB0_RL_NKPortOfLading()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(AUSYD.RL_Code, bill.B0_RL_NKPortOfLading);
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = USLAX.RL_Code;
			AssertEquals(AUSYD.RL_Code, bill.B0_RL_NKPortOfLading);
			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_Vessel = "VESSEL";
			transport3.JW_RL_NKLoadPort = USLAX.RL_Code;
			transport3.JW_RL_NKDiscPort = USCHI.RL_Code;
			AssertEquals(AUSYD.RL_Code, bill.B0_RL_NKPortOfLading);
			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_RL_NKLoadPort = USCHI.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals(AUSYD.RL_Code, bill.B0_RL_NKPortOfLading);
			transport1.JW_Vessel = "VESSEL1";
			AssertEquals(SGSIN.RL_Code, bill.B0_RL_NKPortOfLading);
			transport3.JW_RL_NKLoadPort = AUMEL.RL_Code;
			AssertEquals(SGSIN.RL_Code, bill.B0_RL_NKPortOfLading);
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(AUMEL.RL_Code, bill.B0_RL_NKPortOfLading);
			var transport5 = consol.Transports.AddNew();
			transport5.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport5.JW_LegOrder = 1;
			transport5.JW_Vessel = "VESSEL";
			transport5.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport5.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(AUSYD.RL_Code, bill.B0_RL_NKPortOfLading);
		}

		public void TestSynchroniseB0_RLNKForeignPortOFContractureWithSameCarrier()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL1";
			transport1.JW_IsLinked = true;
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport1.JW_OA_CarrierAddress = ZGuid.Empty;
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL2";
			transport2.JW_IsLinked = true;
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = AUMEL.RL_Code;
			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_Vessel = "VESSEL3";
			transport3.JW_IsLinked = true;
			transport3.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport3.JW_RL_NKDiscPort = USCHI.RL_Code;
			transport3.JW_OA_CarrierAddress = org2.MainAddress.PK;
			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_Vessel = "VESSEL4";
			transport4.JW_RL_NKLoadPort = USCHI.RL_Code;
			transport4.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport4.JW_OA_CarrierAddress = ZGuid.Empty;
			AssertEquals("Melbourne", bill.B0_PlaceOfReceipt);
			AssertEquals(AUMEL.RL_Code, bill.B0_RL_NKForeignPortOfContract);
			transport2.JW_OA_CarrierAddress = org2.MainAddress.PK;
			synchroniser.Synchronise(true);
			AssertEquals("Singapore", bill.B0_PlaceOfReceipt);
			AssertEquals(SGSIN.RL_Code, bill.B0_RL_NKForeignPortOfContract);
		}

		public void TestSynchroniseB0_PlaceOfReceipt()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(AUSYD.RL_PortName, bill.B0_PlaceOfReceipt);
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = USLAX.RL_Code;
			AssertEquals(AUSYD.RL_PortName, bill.B0_PlaceOfReceipt);
			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_Vessel = "VESSEL";
			transport3.JW_RL_NKLoadPort = USLAX.RL_Code;
			transport3.JW_RL_NKDiscPort = USCHI.RL_Code;
			AssertEquals(AUSYD.RL_PortName, bill.B0_PlaceOfReceipt);
			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_RL_NKLoadPort = USCHI.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals(AUSYD.RL_PortName, bill.B0_PlaceOfReceipt);
			transport1.JW_Vessel = "VESSEL1";
			AssertEquals(SGSIN.RL_PortName, bill.B0_PlaceOfReceipt);
			transport3.JW_RL_NKLoadPort = AUMEL.RL_Code;
			AssertEquals(SGSIN.RL_PortName, bill.B0_PlaceOfReceipt);
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(AUMEL.RL_PortName, bill.B0_PlaceOfReceipt);
			var transport5 = consol.Transports.AddNew();
			transport5.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport5.JW_LegOrder = 1;
			transport5.JW_Vessel = "VESSEL";
			transport5.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport5.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(AUSYD.RL_PortName, bill.B0_PlaceOfReceipt);
			AUSYD.RL_PortName = "A".PadRight(bill.B0_PlaceOfReceiptInfo.MaxLength + 1, 'B');
			transport5.JW_RL_NKLoadPort = "ZDS!!";
			AssertEquals("ZDS!!", bill.B0_PlaceOfReceipt);
			transport5.JW_RL_NKLoadPort = AUSYD.RL_Code;
			AssertEquals("A".PadRight(bill.B0_PlaceOfReceiptInfo.MaxLength, 'B'), bill.B0_PlaceOfReceipt);
		}

		public void TestSynchroniseB0_RL_NKLastForeignPort()
		{
			AssertEquals(ZString.Empty, bill.B0_RL_NKLastForeignPort);
			consol.JK_RL_NKLastForeignPort = AUSYD.RL_Code;
			consol.JK_DateLastForeignPort = new ZDateTime(2011, 4, 10);
			AssertEquals(AUSYD.RL_Code, bill.B0_RL_NKLastForeignPort);
			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport1.JW_RL_NKDiscPort = USLAX.RL_Code;
			transport1.JW_ETD = new ZDateTime(2011, 4, 9);
			transport1.JW_ATD = new ZDateTime(2011, 4, 11);
			AssertEquals(AUMEL.RL_Code, bill.B0_RL_NKLastForeignPort);
			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = USLAX.RL_Code;
			transport2.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport2.JW_ATD = new ZDateTime(2011, 4, 12);
			AssertEquals(AUMEL.RL_Code, bill.B0_RL_NKLastForeignPort);
			transport1.JW_ATD = ZDateTime.Empty;
			AssertEquals(AUSYD.RL_Code, bill.B0_RL_NKLastForeignPort);
			consol.JK_RL_NKLastForeignPort = ZString.Empty;
			AssertEquals(AUMEL.RL_Code, bill.B0_RL_NKLastForeignPort);
			consol.JK_RL_NKLastForeignPort = USLAX.RL_Code;
			AssertEquals(AUMEL.RL_Code, bill.B0_RL_NKLastForeignPort);
			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 0;
			transport3.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport3.JW_RL_NKDiscPort = USLAX.RL_Code;
			transport3.JW_ATD = new ZDateTime(2011, 4, 11);
			AssertEquals(SGSIN.RL_Code, bill.B0_RL_NKLastForeignPort);
		}

		public void TestSynchroniseB0_Weight()
		{
			shipment.JS_ActualWeight = 3m;
			AssertEquals(3m, bill.B0_Weight);
			shipment.JS_ActualWeight = 0m;
			AssertEquals(0m, bill.B0_Weight);
		}

		public void TestSynchroniseB0_WeightUQ()
		{
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilotonnes;
			AssertEquals(Core.Constants.Weight.Kilotonnes, bill.B0_WeightUQ);
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Hectograms;
			AssertEquals(Core.Constants.Weight.Hectograms, bill.B0_WeightUQ);
		}

		public void TestConvertPackUQB0_ManifestUQ()
		{
			var packUQ = Factory.New<CusRefPacks>();
			packUQ.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			packUQ.RP_CustomsPack = "LUG";
			packUQ.RP_CommercialPack = "XX";
			packUQ.RP_ConversionFactor = 1.0m;
			packUQ.RP_Type = RPTypeList.Codes.AMSManifest;
			Factory.Save();

			shipment.JS_F3_NKPackType = "XX";
			synchroniser = new CusInBondBillSynchroniser(bill, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals("Converted UQ", "LUG", bill.B0_ManifestUQ);

			packUQ.RP_CustomsPack = "PK";
			Factory.Save();
			synchroniser = new CusInBondBillSynchroniser(bill, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals("XX", bill.B0_ManifestUQ);

			shipment.JS_F3_NKPackType = "CAS";
			synchroniser = new CusInBondBillSynchroniser(bill, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals("CAS", bill.B0_ManifestUQ);
		}

		public void TestSynchroniseB0_Volume()
		{
			shipment.JS_ActualVolume = 3m;
			AssertEquals(3m, bill.B0_Volume);
			shipment.JS_ActualVolume = 0m;
			AssertEquals(0m, bill.B0_Volume);
		}

		public void TestSynchroniseB0_VolumeUQ()
		{
			shipment.JS_UnitOfVolume = Core.Constants.Weight.Kilotonnes;
			AssertEquals(Core.Constants.Weight.Kilotonnes, bill.B0_VolumeUQ);
			shipment.JS_UnitOfVolume = Core.Constants.Weight.Hectograms;
			AssertEquals(Core.Constants.Weight.Hectograms, bill.B0_VolumeUQ);
		}

		public void TestSynchroniseB0_ManifestQtyAndUQ()
		{
			var outerpackLine1 = shipment.OuterPackLines.AddNew();
			outerpackLine1.JL_PackageCount = 25;
			outerpackLine1.JL_F3_NKPackType = ManifestUnitList.Codes.Bag;
			outerpackLine1.JL_ContainerPackingOrder = 2;
			var outerpackLine2 = shipment.OuterPackLines.AddNew();
			outerpackLine2.JL_PackageCount = 40;
			outerpackLine2.JL_F3_NKPackType = ManifestUnitList.Codes.Bag;
			outerpackLine2.JL_ContainerPackingOrder = 1;
			AssertEquals(65, bill.B0_ManifestQty);
			AssertEquals(ManifestUnitList.Codes.Bag, bill.B0_ManifestUQ);
			outerpackLine2.JL_PackageCount = 100;
			outerpackLine2.JL_F3_NKPackType = ManifestUnitList.Codes.Bin;
			AssertEquals(125, bill.B0_ManifestQty);
			AssertEquals(ManifestUnitList.Codes.Package, bill.B0_ManifestUQ);
			outerpackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Piece;
			outerpackLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Piece;
			AssertEquals(ManifestUnitList.Codes.Pieces, bill.B0_ManifestUQ);
			outerpackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
			outerpackLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
			AssertEquals(ManifestUnitList.Codes.Bundle, bill.B0_ManifestUQ);
			outerpackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Roll;
			outerpackLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Roll;
			AssertEquals(ManifestUnitList.Codes.Roll, bill.B0_ManifestUQ);
			outerpackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			outerpackLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			AssertEquals(ManifestUnitList.Codes.Pail, bill.B0_ManifestUQ);
		}

		public void TestSynchroniseB0_ManifestQtyAndUQ_InnerPacklinesEnteredOuterPackID_SyncFromInnerPackLines()
		{
			var outerpackLine1 = shipment.OuterPackLines.AddNew();
			outerpackLine1.JL_PackageCount = 10;
			outerpackLine1.JL_F3_NKPackType = ManifestUnitList.Codes.Bag;
			outerpackLine1.JL_PackLineId = "WTLDNZ00002269";

			var outerpackLine2 = shipment.OuterPackLines.AddNew();
			outerpackLine2.JL_PackageCount = 20;
			outerpackLine2.JL_F3_NKPackType = ManifestUnitList.Codes.Bag;
			Factory.Save();

			var synchroniserForInner = new CusInBondBillSynchroniser(bill, shipment);
			synchroniserForInner.Synchronise(true);
			AssertEquals("Qty sync from all outer pack lines", 30, bill.B0_ManifestQty);
			AssertEquals("UQ sync from all outer pack lines, single UQ", ManifestUnitList.Codes.Bag, bill.B0_ManifestUQ);

			var innerPackLine1 = shipment.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 1;
			innerPackLine1.JL_F3_NKPackType = ManifestUnitList.Codes.Can;
			innerPackLine1.JL_JL_OuterPackLine = outerpackLine1.PK;
			synchroniserForInner.Synchronise(true);
			AssertEquals("Qty sync from outer pack line 2 and inner pack line 1", 21, bill.B0_ManifestQty);
			AssertEquals("UQ sync from outer pack line 2 and inner pack line 1, multiple UQ", ManifestUnitList.Codes.Package, bill.B0_ManifestUQ);

			var innerPackLine2 = shipment.InnerPackLines.AddNew();
			innerPackLine2.JL_PackageCount = 2;
			innerPackLine2.JL_F3_NKPackType = ManifestUnitList.Codes.Can;
			innerPackLine2.JL_JL_OuterPackLine = outerpackLine1.PK;
			synchroniserForInner.Synchronise(true);
			AssertEquals("Qty sync from outer pack line 2, inner pack line 1&2", 23, bill.B0_ManifestQty);
			AssertEquals("UQ sync from outer pack line 2, inner pack line 1&2, multiple UQ", ManifestUnitList.Codes.Package, bill.B0_ManifestUQ);

			innerPackLine2.JL_PackageCount = 4;
			innerPackLine2.JL_F3_NKPackType = ManifestUnitList.Codes.AmmoPack;
			AssertEquals("Qty sync from outer pack line 2, inner pack line 1&2 update", 25, bill.B0_ManifestQty);
			AssertEquals("UQ sync from outer pack line 2, inner pack line 1&2 update, multiple UQ", ManifestUnitList.Codes.Package, bill.B0_ManifestUQ);

			innerPackLine1.JL_PackageCount = 0;
			AssertEquals("Qty change cuz one innerpack line has no qty", 24, bill.B0_ManifestQty);
			AssertEquals("UQ change cuz one innerpack line has no qty, multiple UQ", ManifestUnitList.Codes.Package, bill.B0_ManifestUQ);

			innerPackLine2.JL_PackageCount = 0;
			AssertEquals("Qty from outerpack lines cuz they all have no qty", 30, bill.B0_ManifestQty);
			AssertEquals("UQ from outerpack lines cuz they all have no qty, single UQ", ManifestUnitList.Codes.Bag, bill.B0_ManifestUQ);

			innerPackLine1.JL_PackageCount = 1;
			innerPackLine2.JL_PackageCount = 2;
			var innerPackLine3 = shipment.InnerPackLines.AddNew();
			innerPackLine3.JL_PackageCount = 3;
			innerPackLine3.JL_F3_NKPackType = ManifestUnitList.Codes.Can;
			innerPackLine3.JL_JL_OuterPackLine = outerpackLine2.PK;
			AssertEquals("Qty from all inner pack lines", 6, bill.B0_ManifestQty);
			AssertEquals("UQ from all inner pack lines, multiple UQ", ManifestUnitList.Codes.Package, bill.B0_ManifestUQ);
		}

		public void TestSynchroniseB0_IssuerSCAC()
		{
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, bill.B0_IssuerSCAC);
		}

		public void TestSynchroniseSecondNotifyParty1()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ORG1";
			carrier.UI_ModeOfTransportation = "11";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "ORG2";
			carrier2.UI_ModeOfTransportation = "11";
			var snp1 = bill.SecondaryNotifyParties.FirstOrDefault(x => x.CY_Order == 1);
			AssertNotNull("A SNP1 should have been added", snp1);
			AssertEquals(ZString.Empty, snp1.CY_Data);
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, snp1.CY_Data);
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org2.PK;
			AssertEquals(org2CarrierCode.OK_CustomsRegNo, snp1.CY_Data);
		}

		public void TestSynchroniseSecondNotifyParty2()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "OTT2";
			carrier.UI_ModeOfTransportation = "11";
			var snp2 = bill.SecondaryNotifyParties.FirstOrDefault(x => x.CY_Order == 2);
			AssertNotNull("A SNP2 should have been added", snp2);
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, snp2.CY_Data);
			header.BH_GB = otherCompanyBranch.PK;
			AssertEquals(otherBranchOrgProxyCarrierCode.OK_CustomsRegNo, snp2.CY_Data);
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, snp2.CY_Data);
		}

		public void TestSynchroniseConsignee()
		{
			AssertSynchroniseJobDocAddress(shipment.ConsigneeDocumentaryAddress, bill.Consignee);
		}

		public void TestSynchroniseForeignShipper()
		{
			AssertSynchroniseJobDocAddress(shipment.ConsignorDocumentaryAddress, bill.ForeignShipper);
		}

		public void TestSynchroniseNotifyParty1()
		{
			AssertSynchroniseJobDocAddress(shipment.NotifyPartyDocumentaryAddress, bill.NotifyParty1);
		}

		public void TestSynchroniseContainers()
		{
			AssertEquals(0, bill.MovementDetail.Containers.Count);
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol, container1);
			AssertEquals(1, bill.MovementDetail.Containers.Count);
			var billContainer1 = bill.MovementDetail.Containers["CONT1"];
			AssertNotNull(billContainer1);
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";
			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(consol, container2);
			AssertEquals(2, bill.MovementDetail.Containers.Count);
			AssertEquals(billContainer1, bill.MovementDetail.Containers["CONT1"]);
			var billContainer2 = bill.MovementDetail.Containers["CONT2"];
			AssertNotNull(billContainer2);
			container1.Delete();
			AssertEquals(2, bill.MovementDetail.Containers.Count);
			AssertEquals(billContainer2, bill.MovementDetail.Containers["CONT2"]);
			AssertNotNull(bill.MovementDetail.Containers[CusInBondContainer.NonContainerizedNumber]);
			AssertEquals(true, billContainer1.IsDeleted);
		}

		public void TestOceanBillIsNotSynchronisedForNVOCCShipmentReference()
		{
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			bill.ShipmentReferenceDetails.DeleteAll();
			synchroniser.SetEnabled(false, false);
			consol.JK_MasterBillNum = "MB1";
			synchroniser = new CusInBondBillSynchroniser(bill, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals(0, bill.ShipmentReferenceDetails.Count);
		}

		public void TestSynchroniseOceanBill()
		{
			AssertEquals(1, bill.ShipmentReferenceDetails.Count);
			AssertEquals(ZString.Empty, bill.ShipmentReferenceDetails[BillReferenceList.Codes.OB].BR_ReferenceNum);
			consol.JK_MasterBillNum = "MB1";
			AssertEquals(1, bill.ShipmentReferenceDetails.Count);
			AssertEquals("MB1", bill.ShipmentReferenceDetails[BillReferenceList.Codes.OB].BR_ReferenceNum);
			synchroniser.SetEnabled(false, false);
			consol.JK_MasterBillNum = "MB2";
			AssertEquals(1, bill.ShipmentReferenceDetails.Count);
			var billRef = bill.ShipmentReferenceDetails[BillReferenceList.Codes.OB];
			AssertEquals("MB1", billRef.BR_ReferenceNum);
			synchroniser = new CusInBondBillSynchroniser(bill, shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals(1, bill.ShipmentReferenceDetails.Count);
			AssertEquals(billRef.PK, bill.ShipmentReferenceDetails[BillReferenceList.Codes.OB].PK);
			AssertEquals("MB2", billRef.BR_ReferenceNum);
		}

		void AssertSynchroniseJobDocAddress(JobDocAddress source, JobDocAddress destination)
		{
			AssertNotEquals(source, destination);
			source.E2_OA_Address = org1.MainAddress.PK;
			AssertEquals(org1.MainAddress.PK, destination.E2_OA_Address);
			source.E2_OA_Address = org2.MainAddress.PK;
			AssertEquals(org2.MainAddress.PK, destination.E2_OA_Address);
			source.E2_AddressOverride = true;
			source.E2_CompanyName = "BOB THE BUILDER";
			AssertEquals(true, destination.E2_AddressOverride);
			AssertEquals("BOB THE BUILDER", destination.E2_CompanyName);
		}

		ForwardingShipment shipment;
		CusInBondBill bill;
		CusInBondBillSynchroniser synchroniser;
		protected override void SetUp()
		{
			base.SetUp();
			shipment = consol.Shipments.AddNew();
			bill = header.Bills.AddNew();
			synchroniser = new CusInBondBillSynchroniser(bill, shipment);
			synchroniser.Synchronise(true);
		}
	}
}
