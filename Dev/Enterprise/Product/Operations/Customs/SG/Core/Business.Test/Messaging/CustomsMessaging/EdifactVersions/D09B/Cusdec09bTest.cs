using System;
using System.Collections;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B.Testing
{
	public class Cusdec09bTest : TestCaseWithFactory
	{
		public virtual void TestMessageSubType()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals(CUSDECEDIMessage.Declaration, MessageBuilder.MessageSubType);
		}

		public void TestGenerateUNH()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Vendor prefix included as requested by SG Customs for their ease of identification", "UNH+WTG+CUSDEC:D:09B:UN:041+INPDEC'", msg.UNH.ToString(new UNOASGCharacterSet()));
		}

		public void TestUNHMessageReferenceMaxSizeIsNotExceeded()
		{
			DataProvider.JobNumber = "B00001001"; //standard enterprise generated job number
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNH+WTGB00001001+CUSDEC:D:09B:UN:041+INPDEC'", msg.UNH.ToString(new UNOASGCharacterSet()));
			UNHSegment uNH = msg.UNH[0];
			Assert("Mesasge Reference Number max size should not be exceeded", uNH.MessageReferenceNumber.Length < 15);
			ResetMessageBuilder();
			DataProvider.JobNumber = "SSIN090000168"; //client option generated job number format
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNH+GSSIN090000168+CUSDEC:D:09B:UN:041+INPDEC'", msg.UNH.ToString(new UNOASGCharacterSet()));
			uNH = msg.UNH[0];
			Assert("Mesasge Reference Number max size should not be exceeded", uNH.MessageReferenceNumber.Length < 15);
			ResetMessageBuilder();
			DataProvider.JobNumber = "DSSSIN09A0000168003T"; //shipment generated job number format - max 20 chars
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNH+09A0000168003T+CUSDEC:D:09B:UN:041+INPDEC'", msg.UNH.ToString(new UNOASGCharacterSet()));
			uNH = msg.UNH[0];
			Assert("Mesasge Reference Number max size should not be exceeded", uNH.MessageReferenceNumber.Length < 15);
		}

		public void TestGenerateBGM()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("BGM+914:::REX+<<MSGNO PLACEHOLDER>>+9'", msg.BGM.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateCST()
		{
			DataProvider.CargoPackingType = CargoPackingCodeList.Codes.PackingType9;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("CST++9'", msg.CST.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateLOC()
		{
			DataProvider.PortOfLoading = "AUSYD";
			DataProvider.PortOfDischarge = "JPTYO";
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = true;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base LOC Segments expected", 2, msg.LOC.Count);
			Assert("Port of Loading", msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+9+AUSYD'"));
			Assert("Port of Discharge", msg.LOC.ToString(new UNOASGCharacterSet()).Contains("LOC+12+JPTYO'"));
		}

		public void TestGenerateLicensedWarehouseLOCSegment()
		{
			DataProvider.PlaceOfRelease = TestPlace("VEN001", SGCPlaces.Constants.PremiseType.LicensedWarehouse, "DUTIABLE LIQUOR LIC. WAREHOUSE LIC W/H ADDR1");
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base LOC Segments expected", 1, msg.LOC.Count);
			AssertEquals("Place of Release when LW", "LOC+11+VEN001:::DUTIABLE LIQUOR LIC. WAREHOUSE LIC W/H ADDR1'", msg.LOC.ToString(new UNOASGCharacterSet()));
			DataProvider.PlaceOfRelease = TestPlace("RSYC", SGCPlaces.Constants.PremiseType.SailingClub, "Royal Singapore Yacht Club addr1", true);
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base LOC Segments expected", 1, msg.LOC.Count);
			AssertEquals("Place of Release when other NonSystemPlaces", "LOC+11+SC:::ROYAL SINGAPORE YACHT CLUB ADDR1'", msg.LOC.ToString(new UNOASGCharacterSet()));
			DataProvider.PlaceOfRelease = TestPlace("AT1B", "AT1B", "");
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base LOC Segments expected", 1, msg.LOC.Count);
			AssertEquals("Place of Release when Non LW", "LOC+11+AT1B'", msg.LOC.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateBondedWarehouseAndBWCY_LOCSegments()
		{
			DataProvider.PlaceOfRelease = TestPlace("BW-001", SGCPlaces.Constants.PremiseType.BondedWarehouse, "BONDED WAREHOUSE W/H ADDR1 ADDR2");
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base LOC Segments expected", 1, msg.LOC.Count);
			AssertEquals("Place of Release when BW", "LOC+11+BW-001:::BONDED WAREHOUSE W/H ADDR1 ADDR2'", msg.LOC.ToString(new UNOASGCharacterSet()));
			DataProvider.PlaceOfRelease = TestPlace("HGH19", SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard, "BONDED WAREHOUSE CLASS 2 YARD ADDRESS1 ADDRESS2");
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base LOC Segments expected", 1, msg.LOC.Count);
			AssertEquals("Place of Release when BWCY", "LOC+11+HGH19:::BONDED WAREHOUSE CLASS 2 YARD ADDRESS1 ADDRESS2'", msg.LOC.ToString(new UNOASGCharacterSet()));
			DataProvider.PlaceOfRelease = TestPlace("O15", SGCPlaces.Constants.PremiseType.Others, "Other Location address1 address2", true);
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base LOC Segments expected", 1, msg.LOC.Count);
			AssertEquals("Place of Release when other NonSystemPlaces", "LOC+11+O:::OTHER LOCATION ADDRESS1 ADDRESS2'", msg.LOC.ToString(new UNOASGCharacterSet()));
			DataProvider.PlaceOfRelease = TestPlace("AT1B", "AT1B", "");
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base LOC Segments expected", 1, msg.LOC.Count);
			AssertEquals("Place of Release when NonSystemNonLicencedPlaces", "LOC+11+AT1B'", msg.LOC.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateArrivalDate()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GST;
			DataProvider.PortOfLoading = "";
			DataProvider.PortOfDischarge = "";
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = false;
			DataProvider.ArrivalDate = new ZDate(2011, 05, 10);
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base DTM Segments expected", 1, msg.DTM.Count);
			AssertEquals("Arrival Date is mandatory for with Inward transport details", "DTM+178:20110510:102'", msg.DTM[0].ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.DUT;
			DataProvider.HasInwardTransport = false;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base DTM Segments expected", 0, msg.DTM.Count);
			Assert("Arrival Date should not be sent when no transport details", !msg.ToString(new UNOASGCharacterSet()).Contains("DTM+178"));
		}

		public void TestGenerateStartDate()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BKT;
			DataProvider.PortOfLoading = "";
			DataProvider.PortOfDischarge = "";
			DataProvider.HasInwardTransport = false;
			DataProvider.HasOutwardTransport = false;
			DataProvider.StartDateOfBlanket = new ZDate(2011, 05, 19);
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base DTM Segments expected", 1, msg.DTM.Count);
			AssertEquals("Start Date is mandatory for Blanket declarations", "DTM+194:20110519:102'", msg.DTM[0].ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.DUT;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base DTM Segments expected", 0, msg.DTM.Count);
			Assert("Start Date should not be sent when not a Blanket Declaration", !msg.ToString(new UNOASGCharacterSet()).Contains("DTM+194"));
		}

		public void TestGenerateMEA()
		{
			DataProvider.IsSea = true;
			DataProvider.IsInwardDeclaration = true;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.TotalOuterPack = 100;
			DataProvider.TotalOuterPackUnitOfQty = "BOX";
			DataProvider.TotalGrossWeight = 111;
			DataProvider.TotalGrossWeightUnitOfQty = "T";
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("OuterPack & Weight", "MEA+ABK++BOX:100'MEA+AAH++TNE:111.000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.IsInwardDeclaration = false;
			DataProvider.IsOutwardDeclaration = true;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.TotalOuterPack = 50;
			DataProvider.TotalOuterPackUnitOfQty = "CTN";
			DataProvider.TotalGrossWeight = 1750;
			DataProvider.TotalGrossWeightUnitOfQty = "KG";
			DataProvider.HasOutwardTransport = true;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardVesselNRT = 35000;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("OuterPack, Weight & Tonnage of Outward Vessel", "MEA+ABK++CTN:50'MEA+AAH++TNE:1.750'MEA+AAN++TNE:35000.000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.IsSea = false;
			DataProvider.OutwardTransportCode = 0;
			DataProvider.TotalOuterPack = 244;
			DataProvider.TotalOuterPackUnitOfQty = "PLT";
			DataProvider.TotalGrossWeight = 1750;
			DataProvider.TotalGrossWeightUnitOfQty = "KG";
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Non Sea Transport - OuterPack & Weight", "MEA+ABK++PLT:244'MEA+AAH++KGM:1750.000'", msg.MEA.ToString(new UNOASGCharacterSet()));
		}

		public void TestWeightInRequiredUQ()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.DUT;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.IsInwardDeclaration = true;
			DataProvider.TotalOuterPack = 100;
			DataProvider.TotalOuterPackUnitOfQty = "BOX";
			DataProvider.TotalGrossWeight = 1.3;
			DataProvider.TotalGrossWeightUnitOfQty = "T";
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Inward Air Declaration - Weight must be in KGM", "MEA+ABK++BOX:100'MEA+AAH++KGM:1300.000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Inward Sea Declaration - Weight must be in TNE", "MEA+ABK++BOX:100'MEA+AAH++TNE:1.300'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.DRT;
			DataProvider.InwardTransportCode = 0;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.IsInwardDeclaration = false;
			DataProvider.IsOutwardDeclaration = true;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Outward Air Declaration - Weight must be in KGM", "MEA+ABK++BOX:100'MEA+AAH++KGM:1300.000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Outward Sea Declaration - Weight must be in TNE", "MEA+ABK++BOX:100'MEA+AAH++TNE:1.300'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TTI;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.IsInwardDeclaration = false;
			DataProvider.IsOutwardDeclaration = false;
			DataProvider.IsTranshipmentDeclaration = true;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Transhipment Declaration with Inward Leg being Air - Weight must be in KGM", "MEA+ABK++BOX:100'MEA+AAH++KGM:1300.000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Transhipment Declaration with Inward Leg being Sea - Weight must be in TNE", "MEA+ABK++BOX:100'MEA+AAH++TNE:1.300'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.TotalGrossWeightUnitOfQty = "XXX";
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Weight unit should not be converted if invalid.", "MEA+ABK++BOX:100'MEA+AAH++XXX:1.300'", msg.MEA.ToString(new UNOASGCharacterSet()));
		}

		public void TestOnlyRelevantTransportDetailsAreSent()
		{
			INPDECDataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GTR;
			INPDECDataProvider.PortOfLoading = "AUSYD";
			INPDECDataProvider.PortOfDischarge = "JPTYO";
			INPDECDataProvider.InwardTransportCode = 0;
			INPDECDataProvider.InwardMasterBill = "IN-MB";
			INPDECDataProvider.InwardVesselBerth = TestPlace("KW", "KW", "");
			INPDECDataProvider.InwardTransportIdentifier = "IN-VSSL";
			INPDECDataProvider.InwardJourneyIdentifier = "22N";
			INPDECDataProvider.OutwardTransportCode = 0;
			INPDECDataProvider.OutwardMasterBill = "OUT-MB";
			INPDECDataProvider.OutwardVesselBerth = TestPlace("JW", "JW", "");
			INPDECDataProvider.OutwardJourneyIdentifier = "28E";
			INPDECDataProvider.OutwardTransportIdentifier = "OUT-VSSL";
			INPDECDataProvider.ArrivalDate = new ZDate(2007, 08, 07);
			INPDECDataProvider.DepartureDate = new ZDate(2007, 08, 10);
			INPDECDataProvider.TotalOuterPack = 100;
			INPDECDataProvider.TotalOuterPackUnitOfQty = "BOX";
			INPDECDataProvider.TotalGrossWeight = 1.3;
			INPDECDataProvider.TotalGrossWeightUnitOfQty = "T";
			ItemsTestClass[] invoices = new ItemsTestClass[1];
			invoices[0] = new ItemsTestClass();
			invoices[0].InvoicePK = ZGuid.NewZGuid();
			invoices[0].InvoiceCurrency = Core.Constants.CurrencyCodes.Singapore;
			invoices[0].InvoiceDate = new ZDate(2006, 11, 29);
			invoices[0].InvoiceNumber = "INV-TST";
			invoices[0].InvoiceCurrExchangeRate = 1;
			invoices[0].InvoiceTotalAmount = 2000;
			invoices[0].IncoTerm = "FOB";
			invoices[0].FreightCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 100m, 0m);
			invoices[0].InsuranceCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 200m, 0m);
			invoices[0].OtherCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.UnitedStates, 110m, 100m, 10m);
			INPDECDataProvider.Invoices = invoices;
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceCurrency = Core.Constants.CurrencyCodes.Singapore;
			items[0].BrandName = "Sonic";
			items[0].HSCode = "01011000";
			items[0].HSQuantity = 5;
			items[0].HSQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			items[0].InwardHAWB = "IN-HB";
			items[0].OutwardHAWB = "OUT-HB";
			INPDECDataProvider.Items = items;
			CUSDECMessage msg = INPDECMessageBuilder.CusdecMessage;
			string msgString = msg.ToString(new UNOASGCharacterSet());
			Assert("No inward or outward transport - Port of Loading should not be sent", !msgString.Contains("LOC+9"));
			Assert("No inward or outward transport - Port of Discharge should not be sent", !msgString.Contains("LOC+12"));
			Assert("No inward or outward transport - Inward Berth should not be sent", !msgString.Contains("LOC+164"));
			Assert("No inward or outward transport - Outward Berth should not be sent", !msgString.Contains("LOC+234"));
			Assert("No inward or outward transport - Arrival Date should not be sent", !msgString.Contains("DTM+178"));
			Assert("No inward or outward transport - Departure Date should not be sent", !msgString.Contains("DTM+136"));
			Assert("No inward or outward transport - Inward Transport should not be sent", !msgString.Contains("TDT+3"));
			Assert("No inward or outward transport - Outward Transport should not be sent", !msgString.Contains("TDT+12"));
			Assert("No inward or outward transport - Inward MasterBill should not be sent", !msgString.Contains("DOC+704"));
			Assert("No inward or outward transport - Outward MasterBill should not be sent", !msgString.Contains("DOC+741"));
			Assert("No inward or outward transport - Inward HouseBill should not be sent", !msgString.Contains("DOC+703"));
			Assert("No inward or outward transport - Outward HouseBill should not be sent", !msgString.Contains("DOC+714"));
			INPDECDataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			INPDECDataProvider.IsInwardDeclaration = true;
			INPDECDataProvider.HasInwardTransport = true;
			ResetINPDECMessageBuilder();
			msg = INPDECMessageBuilder.CusdecMessage;
			msgString = msg.ToString(new UNOASGCharacterSet());
			Assert("Inward but no outward transport - Port of Loading should be sent", msgString.Contains("LOC+9+AUSYD"));
			Assert("Inward but no outward transport - Port of Discharge should not be sent", !msgString.Contains("LOC+12"));
			Assert("Inward but no outward transport - Outward Berth should not be sent", !msgString.Contains("LOC+234"));
			Assert("Inward but no outward transport - Arrival Date should be sent", msgString.Contains("DTM+178:20070807:102"));
			Assert("Inward but no outward transport - Departure Date should not be sent", !msgString.Contains("DTM+136"));
			Assert("Inward but no outward transport - Inward Transport should be sent", msgString.Contains("TDT+3+22N+1+++++:::IN-VSSL"));
			Assert("Inward but no outward transport - Outward Transport should not be sent", !msgString.Contains("TDT+12"));
			Assert("Inward but no outward transport - Inward MasterBill should be sent", msgString.Contains("DOC+704+IN-MB"));
			Assert("Inward but no outward transport - Outward MasterBill should not be sent", !msgString.Contains("DOC+741"));
			Assert("Inward but no outward transport - Inward HouseBill should be sent", msgString.Contains("DOC+703+IN-HB"));
			Assert("Inward but no outward transport - Outward HouseBill should not be sent", !msgString.Contains("DOC+714"));
			INPDECDataProvider.DeclarationType = DeclarationTypeCodeList.Codes.SFZ;
			INPDECDataProvider.InwardTransportCode = 0;
			INPDECDataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			INPDECDataProvider.IsInwardDeclaration = false;
			INPDECDataProvider.IsOutwardDeclaration = true;
			INPDECDataProvider.HasInwardTransport = false;
			INPDECDataProvider.HasOutwardTransport = true;
			INPDECDataProvider.PlaceOfRelease = TestPlace("KZ", "KZ", "");
			INPDECDataProvider.PlaceOfReceipt = TestPlace("OTHER", SGCPlaces.Constants.PremiseType.Others, "");
			ResetINPDECMessageBuilder();
			msg = INPDECMessageBuilder.CusdecMessage;
			msgString = msg.ToString(new UNOASGCharacterSet());
			Assert("Outward with no inward transport - Port of Loading should not be sent", !msgString.Contains("LOC+9"));
			Assert("Outward with no inward transport - Port of Discharge should be sent", msgString.Contains("LOC+12+JPTYO"));
			Assert("Outward with no inward transport - Inward Berth should not be sent", !msgString.Contains("LOC+164"));
			Assert("Outward with no inward transport - Arrival Date should not be sent", !msgString.Contains("DTM+178"));
			Assert("Outward with no inward transport - Departure Date should be sent", msgString.Contains("DTM+136:20070810:102"));
			Assert("Outward with no inward transport - Inward Transport should not be sent", !msgString.Contains("TDT+3"));
			Assert("Outward with no inward transport - Outward Transport should be sent", msgString.Contains("TDT+12+28E+1+++++:::OUT-VSSL"));
			Assert("Outward with no inward transport - Inward MasterBill should not be sent", !msgString.Contains("DOC+704"));
			Assert("Outward with no inward transport - Outward MasterBill should be sent", msgString.Contains("DOC+741+OUT-MB"));
			Assert("Outward with no inward transport - Inward HouseBill should not be sent", !msgString.Contains("DOC+703"));
			Assert("Outward with no inward transport - Outward HouseBill should be sent", msgString.Contains("DOC+714+OUT-HB"));
			INPDECDataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			INPDECDataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			INPDECDataProvider.IsInwardDeclaration = true;
			INPDECDataProvider.IsOutwardDeclaration = true;
			INPDECDataProvider.HasInwardTransport = true;
			INPDECDataProvider.HasOutwardTransport = true;
			ResetINPDECMessageBuilder();
			msg = INPDECMessageBuilder.CusdecMessage;
			msgString = msg.ToString(new UNOASGCharacterSet());
			Assert("Inward & Outward transport - Port of Loading should be sent", msgString.Contains("LOC+9+AUSYD"));
			Assert("Inward & Outward transport - Port of Discharge should be sent", msgString.Contains("LOC+12+JPTYO"));
			Assert("Inward & Outward transport - Arrival Date should be sent", msgString.Contains("DTM+178:20070807:102"));
			Assert("Inward & Outward transport - Departure Date should be sent", msgString.Contains("DTM+136:20070810:102"));
			Assert("Inward & Outward transport - Inward Transport should be sent", msgString.Contains("TDT+3+22N+1+++++:::IN-VSSL"));
			Assert("Inward & Outward transport - Outward Transport should be sent", msgString.Contains("TDT+12+28E+1+++++:::OUT-VSSL"));
			Assert("Inward & Outward transport - Inward MasterBill should be sent", msgString.Contains("DOC+704+IN-MB"));
			Assert("Inward & Outward transport - Outward MasterBill should be sent", msgString.Contains("DOC+741+OUT-MB"));
			Assert("Inward & Outward transport - Inward HouseBill should be sent", msgString.Contains("DOC+703+IN-HB"));
			Assert("Inward & Outward transport - Outward HouseBill should be sent", msgString.Contains("DOC+714+OUT-HB"));
			INPDECDataProvider.IsSeaStoreDeclaration = true;
			ResetINPDECMessageBuilder();
			msg = INPDECMessageBuilder.CusdecMessage;
			msgString = msg.ToString(new UNOASGCharacterSet());
			Assert("Outward seastore declaration with no vessel nationality should not generate TPL segment", !msgString.Contains("TPL"));
			INPDECDataProvider.OutwardVesselNationality = "PA";
			ResetINPDECMessageBuilder();
			msg = INPDECMessageBuilder.CusdecMessage;
			msgString = msg.ToString(new UNOASGCharacterSet());
			Assert("Outward seastore declaration should generate TPL segment", msgString.Contains("TPL+::::PA'"));
		}

		public void TestGenerateSegmentGroups1_2and3forSeastores()
		{
			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var cpc = Declaration.CPCs.AddNew();
			cpc.SG_APCCodeDescription = SGConstants.SeaStore;
			cpc.SG_CPCCode = "4203000";
			cpc.SG_PC1 = "8";
			cpc.SG_PC2 = "15";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.APS;
			DataProvider.CPCs = CusEntry.CPCs;
			AssertMultilineEquals("Group 1 - Seastores", "RFF+ABE:4203000'PAC+++++ZZZ'PCI+ZZZ'FTX+AAX+++8:15'", MessageBuilder.CusdecMessage.Group1[1].ToString(new UNOASGCharacterSet()), '\'');
			AssertMultilineEquals("Group 2 - Seastores", "PAC+++++ZZZ'PCI+ZZZ'FTX+AAX+++8:15'", MessageBuilder.CusdecMessage.Group1[1].Group2[0].ToString(new UNOASGCharacterSet()), '\'');
			AssertMultilineEquals("Group 3 - Seastores", "PCI+ZZZ'FTX+AAX+++8:15'", MessageBuilder.CusdecMessage.Group1[1].Group2[0].Group3[0].ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestNADSegmentSplitsNameAtAppropriatePlace()
		{
			// CN, IM, EX & MF all only have 2 segments (70 chars) to split into.
			// All other names can split across 3 segments (105 chars total).
			OrganisationTestClass testExporter = new OrganisationTestClass();
			testExporter.Address = new OrganisationAddressTestClass("Org Address");
			testExporter.Name = "SMITH JONES CONSTRUCTION PTE LTD";
			testExporter.UEN = "12345678S";
			OrganisationTestClass testCarriersAgent = new OrganisationTestClass();
			testCarriersAgent.Address = new OrganisationAddressTestClass("Org Address");
			testCarriersAgent.Name = "SINGAPORE AIRPORT TERMINAL SERVICES LTD";
			testCarriersAgent.UEN = "197201770G";
			INPDECDataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TCO;
			INPDECDataProvider.PortOfLoading = "AUSYD";
			INPDECDataProvider.PortOfDischarge = "JPTYO";
			INPDECDataProvider.InwardTransportCode = 0;
			INPDECDataProvider.InwardMasterBill = "IN-MB";
			INPDECDataProvider.InwardVesselBerth = TestPlace("KW", "KW", "");
			INPDECDataProvider.InwardTransportIdentifier = "IN-VSSL";
			INPDECDataProvider.InwardJourneyIdentifier = "22N";
			INPDECDataProvider.InwardCarrierAgent = testCarriersAgent;
			INPDECDataProvider.OutwardTransportCode = 0;
			INPDECDataProvider.OutwardMasterBill = "OUT-MB";
			INPDECDataProvider.OutwardVesselBerth = TestPlace("JW", "JW", "");
			INPDECDataProvider.OutwardJourneyIdentifier = "28E";
			INPDECDataProvider.OutwardTransportIdentifier = "OUT-VSSL";
			INPDECDataProvider.Exporter = testExporter;
			CUSDECMessage msg = INPDECMessageBuilder.CusdecMessage;
			AssertEquals("Base message should generate the Declaring Agent Segment & Exporter Segments (Group 6)", 8, msg.Group6.Count);
			NADSegment carrierAgent = msg.Group6[2].NAD[0];
			AssertEquals("Carrier Agent", "NAD+CG+197201770G++SINGAPORE AIRPORT TERMINAL SERVICES LTD:::::1'", carrierAgent.ToString(new UNOASGCharacterSet()));
			NADSegment exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name fits in first segment element", "NAD+EX+12345678S++SMITH JONES CONSTRUCTION PTE LTD:::::2'", exporter.ToString(new UNOASGCharacterSet()));
			ResetINPDECMessageBuilder();
			testExporter.Name = "MAXIMILIAN-EDGARD-HERALDSON-SMITH JONES CONSTRUCTION PTE LTD";
			msg = INPDECMessageBuilder.CusdecMessage;
			exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name should split elegantly accross elements", "NAD+EX+12345678S++MAXIMILIAN-EDGARD-HERALDSON-SMITH:JONES CONSTRUCTION PTE LTD::::2'", exporter.ToString(new UNOASGCharacterSet()));
			ResetINPDECMessageBuilder();
			testExporter.Name = "ANGLO-EASTERN FREIGHT FORWARDING AGENCY (SINGAPORE) PTE LTD";
			msg = INPDECMessageBuilder.CusdecMessage;
			exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name should split elegantly accross elements", "NAD+EX+12345678S++ANGLO-EASTERN FREIGHT FORWARDING:AGENCY (SINGAPORE) PTE LTD::::2'", exporter.ToString(new UNOASGCharacterSet()));
			ResetINPDECMessageBuilder();
			testCarriersAgent.Name = "ANGLO-EASTERN INTERNATIONAL ENGINEERING AND MANUFACTURING SERVICES PTE. LTD";
			testExporter.Name = "ANGLO-EASTERN-SINGAPORE INTERNATIONAL DESIGN ENGINEERING AND MANUFACTURING PRODUCTION SERVICES PTE. LTD";
			msg = INPDECMessageBuilder.CusdecMessage;
			carrierAgent = msg.Group6[2].NAD[0];
			AssertEquals("Carrier Agent should split elegantly accross 2 elements", "NAD+CG+197201770G++ANGLO-EASTERN INTERNATIONAL ENGINEERING AND:MANUFACTURING SERVICES PTE. LTD::::1'", carrierAgent.ToString(new UNOASGCharacterSet()));
			exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name should only split accross 2 elements - should show 100 characters if cannot split", "NAD+EX+12345678S++ANGLO-EASTERN-SINGAPORE INTERNATION:AL DESIGN ENGINEERING AND MANUFACTU::::2'", exporter.ToString(new UNOASGCharacterSet()));
			ResetINPDECMessageBuilder();
			testCarriersAgent.Name = "ANGLO-EASTERN AND NORTH AMERICAN INTERNATIONAL ENGINEERING MANUFACTURING AND PROJECT MANAGEMENT SERVICES PTE. LIMITED";
			testExporter.Name = "ANGLO-EASTERN AND NORTH AMERICAN INTERNATIONAL ENGINEERING MANUFACTURING AND PROJECT MANAGEMENT SERVICES PTE. LIMITED";
			msg = INPDECMessageBuilder.CusdecMessage;
			carrierAgent = msg.Group6[2].NAD[0];
			AssertEquals("Carrier Agent should show in full when it cannot split elegantly accross elements", "NAD+CG+197201770G++ANGLO-EASTERN AND NORTH AMERICAN INTERNATIONAL ENG:INEERING MANUFACTURING AND PROJECT MANAGEMENT SERV::::1'", carrierAgent.ToString(new UNOASGCharacterSet()));
			exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name should show in full when it cannot split elegantly accross elements", "NAD+EX+12345678S++ANGLO-EASTERN AND NORTH AMERICAN IN:TERNATIONAL ENGINEERING MANUFACTURI::::2'", exporter.ToString(new UNOASGCharacterSet()));
		}

		public void TestClaimantSegmentsSG6()
		{
			var testClaimant = new OrganisationTestClass();
			testClaimant.Address = new OrganisationAddressTestClass("Org Address");
			testClaimant.Name = "CLAIMANTCOMPANYNAME";
			testClaimant.UEN = "197201770G";
			INPDECDataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TCO;
			INPDECDataProvider.ClaimantCode = "CLAIMANTCODE";
			INPDECDataProvider.ClaimantName = "CLAIMANTNAME";
			INPDECDataProvider.Claimant = testClaimant;
			var msg = INPDECMessageBuilder.CusdecMessage;
			SegmentGroup6 claimant = null;
			foreach (SegmentGroup6 group6 in msg.Group6)
			{
				if (group6.NAD[0].PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Claimant)
				{
					claimant = group6;
					break;
				}
			}

			AssertEquals("Claimant Segment Format", "NAD+CC+197201770G++CLAIMANTCOMPANYNAME:::::1'CTA+IC+CLAIMANTCODE:CLAIMANTNAME'", claimant.ToString(new UNOASGCharacterSet()));
		}

		public void TestSG6PostCode()
		{
			var testExporter = new OrganisationTestClass();
			testExporter.Name = "C & A MEXICO S. DE R. L.";
			testExporter.Address = new OrganisationAddressTestClass("AV.CAMINO AL ITESO No.8350", "MEXICO R.F.C CME-", "961203-360", "MX", "");
			testExporter.UEN = "197201770G";
			OUTDECDataProvider.IsOutwardDeclaration = true;
			OUTDECDataProvider.DeclarationType = DeclarationTypeCodeList.Codes.APS;
			OUTDECDataProvider.Exporter = testExporter;
			var msg = OUTDECMessageBuilder.CusdecMessage;
			SegmentGroup6 exporter = null;
			foreach (SegmentGroup6 group6 in msg.Group6)
			{
				if (group6.NAD[0].PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Exporter)
				{
					exporter = group6;
					break;
				}
			}

			AssertEquals("Claimant Segment Format", "NAD+EX+197201770G++C & A MEXICO S. DE R. L.:::::2+AV.CAMINO AL ITESO NO.8350+MEXICO R.F.C CME-++961203360+MX'", exporter.ToString(new UNOASGCharacterSet()));
		}

		public void TestImporterNameOverrideWithLongName()
		{
			OrgHeader testImporter1 = Factory.NewWithValidTestData<OrgHeader>();
			testImporter1.OH_FullName = "HYDRO ALUMINIUM MALAYSIA SDN BHD";
			string importerNameOverride = "HYDRO ALUMINIUM MALAYSIA SDN BHD C/O OIA GLOBAL LOGISTICS (S) PTE LTD";
			IOrganisation testImporter = new EntryOrganisationsInfo(testImporter1, importerNameOverride);
			INPDECDataProvider.DeclarationType = DeclarationTypeCodeList.Codes.SFZ;
			INPDECDataProvider.Importer = testImporter;
			CUSDECMessage msg = INPDECMessageBuilder.CusdecMessage;
			AssertEquals("Base message should generate the base NAD segments (Group 6)", 8, msg.Group6.Count);
			NADSegment importer = msg.Group6[4].NAD[0];
			AssertEquals("Full Importer Name override must fit in first two segment elements when it cannot be split elegantly", "NAD+IM+++HYDRO ALUMINIUM MALAYSIA SDN BHD C/:O OIA GLOBAL LOGISTICS (S) PTE LTD::::2'", importer.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateGEI()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("GEI+5+:Y'", msg.GEI.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateEQDAndSELSegments()
		{
			ContainersTestClass[] containers = new ContainersTestClass[3];
			containers[0] = new ContainersTestClass();
			containers[0].ContainerNumber = "ABCU0039477";
			containers[0].ContainerType = "FCL";
			containers[0].ContainerSize = 20;
			containers[0].ContainerWeight = 35;
			containers[0].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[1] = new ContainersTestClass();
			containers[1].ContainerNumber = "NYKU2188343";
			containers[1].ContainerType = "LCL";
			containers[1].ContainerSize = 40;
			containers[1].ContainerWeight = 120;
			containers[1].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[1].SealNumber = "SHP1233445";
			containers[2] = new ContainersTestClass();
			containers[2].ContainerNumber = "NYKU4798228";
			containers[2].ContainerType = "LCL";
			containers[2].ContainerSize = 40;
			containers[2].ContainerWeight = 24780;
			containers[2].ContainerWeightUnit = Core.Constants.Weight.Kilograms;
			containers[2].SealNumber = "XX-03948";
			DataProvider.Containers = containers;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Container EQD Segments expected", 3, msg.EQD.Count);
			AssertEquals("Container SEL Segments expected", 3, msg.SEL.Count);
			AssertEquals("EQD+CN+ABCU0039477:1+35:::FCL20'EQD+CN+NYKU2188343:2+120:::LCL40'EQD+CN+NYKU4798228:3+25:::LCL40'", msg.EQD.ToString(new UNOASGCharacterSet()));
			AssertEquals("SEL+NA'SEL+SHP1233445'SEL+XX-03948'", msg.SEL.ToString(new UNOASGCharacterSet()));
		}

		public void TestMinAndMaxContainerWeight()
		{
			ContainersTestClass[] containers = new ContainersTestClass[3];
			containers[0] = new ContainersTestClass();
			containers[0].ContainerNumber = "ABCU0039477";
			containers[0].ContainerType = "FCL";
			containers[0].ContainerSize = 20;
			containers[0].ContainerWeight = 3500;
			containers[0].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[1] = new ContainersTestClass();
			containers[1].ContainerNumber = "NYKU2188343";
			containers[1].ContainerType = "LCL";
			containers[1].ContainerSize = 40;
			containers[1].ContainerWeight = .58;
			containers[1].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[2] = new ContainersTestClass();
			containers[2].ContainerNumber = "NYKU4798228";
			containers[2].ContainerType = "LCL";
			containers[2].ContainerSize = 40;
			containers[2].ContainerWeight = 247;
			containers[2].ContainerWeightUnit = Core.Constants.Weight.Kilograms;
			DataProvider.Containers = containers;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Container EQD Segments expected", 3, msg.EQD.Count);
			AssertEquals("EQD+CN+ABCU0039477:1+999:::FCL20'EQD+CN+NYKU2188343:2+1:::LCL40'EQD+CN+NYKU4798228:3+1:::LCL40'", msg.EQD.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateFTX()
		{
			DataProvider.TradersRemarksForMessage = new ZString[] { "This is one string remark", "This is another string remark" };

			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("FTX+AAI+++THIS IS ONE STRING REMARK:THIS IS ANOTHER STRING REMARK'", msg.FTX.ToString(new UNOASGCharacterSet()));

			DataProvider.TradersRemarksForMessage = new ZString[] { "This is one string remark", "This is another string remark", "This is a third string remark" };
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			var result = "FTX+AAI+++THIS IS ONE STRING REMARK:THIS IS ANOTHER STRING REMARK'";
			AssertEquals("Trader Remarks send 2 lines max for non Out with CO declarations, even when excess data is stored in db field.", result, msg.FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestMessageWillIncludeHashtagCharacterIfUsed()
		{
			DataProvider.TradersRemarksForMessage = new ZString[] { "This remark contains a hashtag - MicroController R7F701331AEAFP#KA2" };

			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Segment element generated should retain the # character", "FTX+AAI+++THIS REMARK CONTAINS A HASHTAG - MICROCONTROLLER R7F701331AEAFP#KA2'", msg.FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestBaseGenerateSG6()
		{
			var testDeclarant = new AgentInfoTestClass();
			testDeclarant.Name = "Test SG Broker";
			testDeclarant.Phone = "64 85721111";
			testDeclarant.Code = "v13t001";
			DataProvider.Declarant = testDeclarant;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base message should generate the Declaring Agent Segment & Declarant Segments (Group 6)", 2, msg.Group6.Count);
			AssertEquals("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL:::::1'", msg.Group6[0].NAD.ToString(new UNOASGCharacterSet()));
			// PopulateDeclaringAgentSegment
			// DeclarantsAgentRepresentative = AE
			var declaringAgentSG6 = msg.Group6[0];
			AssertEquals("Declarant NAD", 1, declaringAgentSG6.NAD.Count);
			AssertEquals("Declarant CTA", 0, declaringAgentSG6.CTA.Count);
			AssertEquals("Declarant COM", 0, declaringAgentSG6.COM.Count);
			AssertEquals("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL:::::1'", declaringAgentSG6.NAD.ToString(new UNOASGCharacterSet()));
			// Declarant = DT
			var declarantSG6 = msg.Group6[1];
			AssertEquals("Declarant NAD", 1, declarantSG6.NAD.Count);
			AssertEquals("Declarant CTA", 1, declarantSG6.CTA.Count);
			AssertEquals("Declarant COM", 1, declarantSG6.COM.Count);
			AssertEquals("NAD+DT'", declarantSG6.NAD.ToString(new UNOASGCharacterSet()));
			AssertEquals("CTA+IC+V13T001:TEST SG BROKER'", declarantSG6.CTA.ToString(new UNOASGCharacterSet()));
			AssertEquals("COM+64 85721111:TE'", declarantSG6.COM.ToString(new UNOASGCharacterSet()));
		}

		public void TestDeclaringAgentSegment()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL:::::1'", msg.Group6[0].NAD.ToString(new UNOASGCharacterSet()));
		}

		public void TestForeignerDeclarantCodeWithTrailingSpace()
		{
			AgentInfoTestClass testDeclarant = new AgentInfoTestClass();
			testDeclarant.Name = "Test Foreign Broker";
			testDeclarant.Phone = "+64 85721111";
			testDeclarant.Code = "P0657777@"; // Format of Foreigner/Passport code: P followed by last 7 digits of their passport followed by a blank space always in 9th position (currently substitutued with @)
			DataProvider.Declarant = testDeclarant;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base message should generate the Declaring Agent Segment & Declarant Segments (Group 6)", 2, msg.Group6.Count);
			NADSegment broker = msg.Group6[1].NAD[0];
			CTASegment brokerID = msg.Group6[1].CTA[0];
			COMSegment brokerPhone = msg.Group6[1].COM[0];
			AssertEquals("Foreign Broker", "NAD+DT'", broker.ToString(new UNOASGCharacterSet()));
			AssertEquals("Declarant Code requires trailing space", "CTA+IC+P0657777 :TEST FOREIGN BROKER'", brokerID.ToString(new UNOASGCharacterSet()));
			AssertEquals("Broker contact details", "COM+?+64 85721111:TE'", brokerPhone.ToString(new UNOASGCharacterSet()));
		}

		public void TestDetailsSectionUNS()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNS+D'", msg.UNS1.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMessageUNS()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNS+S'", msg.UNS2.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMessageCNT()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("CNT+5:0'", msg.CNT.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMessageUNT()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNT+15+WTG'", msg.UNT.ToString(new UNOASGCharacterSet()));
		}

		public void TestBaseGenerateSG1()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base should generate the Message Sender Segment Group 1", 1, msg.Group1.Count);
			AssertEquals("RFF+MS:.'", msg.Group1[0].RFF.ToString(new UNOASGCharacterSet()));
		}

		public void TestSegmentGroup5()
		{
			AttachmentsTestClass[] attachments = new AttachmentsTestClass[2];
			attachments[0] = new AttachmentsTestClass();
			attachments[0].FileName = "ATTDOC_0001";
			attachments[0].DocType = SupportingDocumentTypeCodeList.Codes.DocType001;
			attachments[1] = new AttachmentsTestClass();
			attachments[1].FileName = "ATTDOC_0002";
			attachments[1].DocType = SupportingDocumentTypeCodeList.Codes.DocType002;
			SGCUSDECTestClass.ImplementsAdditionalMessageInformation addInfo = (SGCUSDECTestClass.ImplementsAdditionalMessageInformation)DataProvider.AdditionalMessageInformation;
			addInfo.SupportingDocuments = attachments;
			DataProvider.InwardMasterBill = "081-003948578";
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.OutwardMasterBill = "OB883747";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("DOC+704+081-003948578'DOC+741+OB883747'DOC+916+001::ATTDOC_0001'DOC+916+002::ATTDOC_0002'", msg.Group5.ToString(new UNOASGCharacterSet()));
		}

		public void TestAttachedDocumentFilenameStripsInvalidCharacters()
		{
			var docs = new eDocs();
			var doc = new eDoc();
			doc.FileName = "test?File*Name.doc";
			doc.DocType = SupportingDocumentTypeCodeList.Codes.DocType005;
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "xl<>/|sdoc<.xls";
			doc.DocType = SupportingDocumentTypeCodeList.Codes.DocType024;
			docs.Add(doc);
			var supportingDocumentCollection = new SupportingDocumentCollection(Factory);
			AssertNoExceptionThrown("No exception should be thrown", () => supportingDocumentCollection.SetStorageDocs(docs));
			AttachmentsTestClass[] attachments = new AttachmentsTestClass[2];
			attachments[0] = new AttachmentsTestClass();
			attachments[0].FileName = supportingDocumentCollection.StorageDocs[0].Code;
			attachments[0].DocType = SupportingDocumentTypeCodeList.Codes.DocType005;
			attachments[1] = new AttachmentsTestClass();
			attachments[1].FileName = supportingDocumentCollection.StorageDocs[1].Code;
			attachments[1].DocType = SupportingDocumentTypeCodeList.Codes.DocType024;
			SGCUSDECTestClass.ImplementsAdditionalMessageInformation addInfo = (SGCUSDECTestClass.ImplementsAdditionalMessageInformation)DataProvider.AdditionalMessageInformation;
			addInfo.SupportingDocuments = attachments;
			DataProvider.InwardMasterBill = "081-003948578";
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.OutwardMasterBill = "OB883747";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("DOC+704+081-003948578'DOC+741+OB883747'DOC+916+005::005 - TESTFILENAME.DOC'DOC+916+024::024 - XLSDOC.XLS'", msg.Group5.ToString(new UNOASGCharacterSet()));
		}

		public void TestConvertToSingaporeCustomsRequiredWeightUnit()
		{
			Cusdec09bTestClass cUSDECTestClass = new Cusdec09bTestClass(DataProvider);
			decimal sourceWeightInPounds = 10;
			var (resultWeight, resultWeightUnit) = cUSDECTestClass.ConvertToSingaporeCustomsRequiredWeightUnit(sourceWeightInPounds, Core.Constants.Weight.Pounds, SGConstants.Weight.Kilograms);
			decimal expectedWeight = Core.Constants.Weight.Convert(sourceWeightInPounds, Core.Constants.Weight.Pounds, Core.Constants.Weight.Kilograms);
			AssertEquals(expectedWeight, resultWeight);
			AssertEquals(SGConstants.Weight.Kilograms, resultWeightUnit);
			decimal sourceWeightInShortTonnes = 100;
			(resultWeight, resultWeightUnit) = cUSDECTestClass.ConvertToSingaporeCustomsRequiredWeightUnit(sourceWeightInShortTonnes, Core.Constants.Weight.ShortTons, SGConstants.Weight.Tonnes);
			expectedWeight = Core.Constants.Weight.Convert(sourceWeightInShortTonnes, Core.Constants.Weight.ShortTons, Core.Constants.Weight.Tonnes);
			AssertEquals(expectedWeight, resultWeight);
			AssertEquals(SGConstants.Weight.Tonnes, resultWeightUnit);
			decimal sourceWeightInInvalidUnit = 1000;
			(resultWeight, resultWeightUnit) = cUSDECTestClass.ConvertToSingaporeCustomsRequiredWeightUnit(sourceWeightInInvalidUnit, "XXX", SGConstants.Weight.Kilograms);
			AssertEquals(sourceWeightInInvalidUnit, resultWeight);
			AssertEquals("XXX", resultWeightUnit);
		}

		public void TestBrandNameDefaultsToUNBRANDEDForImportDecs()
		{
			var cUSDECTestClass = new Cusdec09bTestClass(DataProvider);
			DataProvider.IsImport = true;
			ItemsTestClass[] items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].BrandName = ZString.Empty;
			DataProvider.Items = items;
			var expectedResult = @"FTX+PRD+++UNBRANDED";
			var msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("For import jobs UNBRANDED must be sent if no value entered for Brand Name", expectedResult, msg.Group32[0].FTX[0].ToString(new UNOASGCharacterSet()), '\'');
		}

		#region Group 32
		public void TestGoodsDescription()
		{
			var cUSDECTestClass = new Cusdec09bTestClass(DataProvider);
			ItemsTestClass[] items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].GoodsDescription = @"HANKOOK HORIZONTAL CNC LATHE
PROTURN 60BX1500 WITH ACCESSORIES";
			DataProvider.Items = items;
			var expectedResult = @"FTX+AAA+++HANKOOK HORIZONTAL CNC LATHE PROTURN 60BX1500 WITH ACCESSORIES";
			var msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 32 - FTX 'Goods Description'", expectedResult, msg.Group32[0].FTX[0].ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGoodsDescriptionMaxWithMultipleCarriageReturns()
		{
			var cUSDECTestClass = new Cusdec09bTestClass(DataProvider);
			ItemsTestClass[] items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].GoodsDescription = @"GOODS DESCRIPTION
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB
CCCCCCCCCCCCCCCCCCCCCCCCCC
DDDDDDDDDDDDDDDDDDDDDD
EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG
HHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH
IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII
JJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJ
KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK
LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL
FINISH HERE";
			DataProvider.Items = items;
			var expectedResult = @"FTX+AAA+++GOODS DESCRIPTION AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB CCCCCCCCCCCCCCCCCCCCCCCCCC DDDDDDDDDDDDDDDDDDDDDD EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG HHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII JJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJ KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL FINISH HERE";
			var msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 32 - FTX 'Goods Description'", expectedResult, msg.Group32[0].FTX[0].ToString(new UNOASGCharacterSet()), '\'');
		}

		#endregion
		#region Group 34
		public void TestMarksAndNumbers()
		{
			var cUSDECTestClass = new Cusdec09bTestClass(DataProvider);
			ItemsTestClass[] items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].MarksAndNumbers = @"Note 2: Specify markings on cargo for marks & numbers, if any. Repeat at most 4 times and specify: 
1st occurrence: 10 lines x 17 = 170 characters
2nd occurrence: 10 lines x 17 = 170 characters
3rd occurrence: 8 lines x 17 = 136 characters
4th occurrence: 3 lines x 12 = 36 characters.
This specification provides the definition of the In-Non-Payment/Customs Declaration ie. CUSDEC (INPDEC) message to be used in Electronic Data Interchange (EDI) between trading partners involved in Administration, commerc 
";
			DataProvider.Items = items;
			var expectedResult = @"PCI+28+NOTE 2?: SPECIFY M:ARKINGS ON CARGO :FOR MARKS & NUMBE:RS, IF ANY. REPEA:T AT MOST 4 TIMES: AND SPECIFY?:  1S:T OCCURRENCE?: 10 :LINES X 17 = 170 :CHARACTERS 2ND OC:CURRENCE?: 10 LI'PCI+28+NES X 17 = 170 CH:ARACTERS 3RD OCCU:RRENCE?: 8 LINES X: 17 = 136 CHARACT:ERS 4TH OCCURRENC:E?: 3 LINES X 12 =: 36 CHARACTERS. T:HIS SPECIFICATION: PROVIDES THE DEF:INITION OF THE'PCI+28+ IN-NON-PAYMENT/C:USTOMS DECLARATIO:N IE. CUSDEC (INP:DEC) MESSAGE TO B:E USED IN ELECTRO:NIC DATA INTERCHA:NGE (EDI) BETWEEN: TRADING PARTNERS'PCI+28+ INVOLVED IN:ADMINISTRATI:ON, COMMERC";
			var msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 34", expectedResult, msg.Group32[0].Group33[0].Group34.ToString(new UNOASGCharacterSet()), '\'');
		}

		#endregion
		#region Group 35
		public void TestUnitPrice()
		{
			Cusdec09bTestClass cUSDECTestClass = new Cusdec09bTestClass(DataProvider);
			ItemsTestClass[] items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].CustomsValue = 17500m;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 35 - Unit price not entered/calculated", "MOA+63:17500.00'", msg.Group32[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
			items[0].UnitPrice = 15000m;
			AssertMultilineEquals("Group 35 - item is not a dutiable motor vehicle - do not send Unit price segement", "MOA+63:17500.00'", msg.Group32[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
			items[0].IsMotorVehicle = true;
			items[0].ExciseAmount = 3000m;
			items[0].UnitPrice = 15750m;
			fMessageBuilder = null;
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 35 with Unit price segment", "MOA+63:17500.00'MOA+146:15750.0000", msg.Group32[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		#endregion
		#region Group 37 Tests
		public void TestGenerateSegmentGroup37()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			//Sea Stores
			DataProvider.Items = items;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NumberOfCrew = 8;
			DataProvider.VoyageDuration = 15;
			//MV
			items[0].IsMotorVehicle = true;
			items[0].DateOfFirstRegistration = new ZDate(2005, 11, 23);
			items[0].EngineCapacity = 4800;
			items[0].EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			items[0].RegistrationNumberSG = "SG12345";
			ProductCodesTestClass[] cASC = new ProductCodesTestClass[1];
			cASC[0] = new ProductCodesTestClass();
			cASC[0].ProductCode = "ProdCode_00001";
			cASC[0].ProductCodeQty = 3.5;
			cASC[0].ProductCodeUnitType = UnitOfQuantityCodeList.Codes.TNE;
			items[0].ProductCodes = cASC;
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 engineNumber = Factory.New<CASCCode1>();
			engineNumber.CY_Data = "jkue903jrm3499emdfjf93eikr30rff";
			testCodes1.Add(engineNumber);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 chassisNumber = Factory.New<CASCCode2>();
			chassisNumber.CY_Data = "v83945030eif9gt8d903kdfmrk93ik";
			testCodes2.Add(chassisNumber);
			items[0].CASCCodes2 = testCodes2;
			//strategic goods - Base should not generate these segments
			items[0].IsStrategic = true;
			items[0].CategoryCode = "XY09348TFZ";
			items[0].EndUseDescription = "Commercial application";
			items[0].EndUseCode1 = CA_SC1CodeList.Codes.NMU;
			items[0].EndUseCode2 = "GOV";
			items[0].EndUseCode3 = "NMD";
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'IMD++8+:::4800.00:CC'";
			AssertMultilineEquals("Group 37 - Motor Vehicle", resultExpected, msg.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup37Strategic()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].IsStrategic = true;
			items[0].CategoryCode = "XY09348TFZ";
			items[0].EndUseDescription = "Commercial aircraft spares";
			items[0].EndUseCode1 = CA_SC1CodeList.Codes.NMU;
			items[0].EndUseCode2 = CA_SC2CodeList.Codes.NGU;
			items[0].EndUseCode3 = CA_SC3CodeList.Codes.NMD;
			DataProvider.Items = items;
			AssertMultilineEquals("Group 37 - Base Should not generate Strategic Goods", "", MessageBuilder.CusdecMessage.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup37MotorVehicles()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].IsMotorVehicle = true;
			ProductCodesTestClass[] cASC = new ProductCodesTestClass[1];
			cASC[0] = new ProductCodesTestClass();
			cASC[0].ProductCode = "ProdCode_00001";
			cASC[0].ProductCodeQty = 3.5;
			cASC[0].ProductCodeUnitType = UnitOfQuantityCodeList.Codes.TNE;
			items[0].ProductCodes = cASC;
			items[0].DateOfFirstRegistration = new ZDate(2005, 11, 23);
			items[0].EngineCapacity = 4800;
			items[0].EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			items[0].RegistrationNumberSG = "SG12345";
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 code1_1 = Factory.New<CASCCode1>();
			code1_1.CY_Data = "jkue903jrm3499emdfjf93eikr30rff";
			testCodes1.Add(code1_1);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 code2_1 = Factory.New<CASCCode2>();
			code2_1.CY_Data = "v83945030eif9gt8d903kdfmrk93ik";
			testCodes2.Add(code2_1);
			items[0].CASCCodes2 = testCodes2;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'IMD++8+:::4800.00:CC'";
			AssertMultilineEquals("Group 37 - Motor Vehicle", resultExpected, msg.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup37NoProductCode()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 chemicalPurity = Factory.New<CASCCode1>();
			chemicalPurity.CY_Data = "5";
			testCodes1.Add(chemicalPurity);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 maxQty = Factory.New<CASCCode2>();
			maxQty.CY_Data = "50";
			testCodes2.Add(maxQty);
			items[0].CASCCodes2 = testCodes2;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 37 - Chemical Concentration limit", "RFF+AVM'GIN+AV+5+50'", msg.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup37NoProductCodeButOtherG37()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			CASCCode1Collection testCodes1 = new CASCCode1Collection(InvoiceLine);
			CASCCode1 chemicalPurity = Factory.New<CASCCode1>();
			chemicalPurity.CY_Data = "5";
			testCodes1.Add(chemicalPurity);
			items[0].CASCCodes1 = testCodes1;
			CASCCode2Collection testCodes2 = new CASCCode2Collection(InvoiceLine);
			CASCCode2 maxQty = Factory.New<CASCCode2>();
			maxQty.CY_Data = "50";
			testCodes2.Add(maxQty);
			items[0].CASCCodes2 = testCodes2;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 37 - No Product code but with other CA/CS values", "RFF+AVM'GIN+AV+5+50'", msg.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestInvoiceNumberIsNotIncluded()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			DataProvider.Items = items;
			AssertMultilineEquals("Group 37 - should be empty", "", MessageBuilder.CusdecMessage.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		#endregion
		public void TestMessageText()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals(msg.ToString(new UNOASGCharacterSet()), MessageBuilder.MessageText);
		}

		public void TestMessageSegmentCount()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals(msg.CountIncludingUNT, MessageBuilder.MessageSegmentCount);
		}

		public void TestEscapeCharacterHandling()
		{
			DataProvider.TradersRemarksForMessage = new ZString[] { "Edifact should handle characters needing the escape character, eg: colon's and apostrophe's" };
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("FTX+AAI+++EDIFACT SHOULD HANDLE CHARACTERS NEEDING THE ESCAPE CHARACTER, EG?: COLON?'S AND APOSTROPHE?'S'", msg.FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestExciseUnitSegments()
		{
			ItemsTestClass item = new ItemsTestClass();
			ItemsTestClass[] items = new ItemsTestClass[] { item };
			item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.LTR;
			item.DutyRateUnit = SGConstants.LPA;
			item.ExciseUnitRate = 350m;
			item.ExciseAmount = 100;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			string actual = msg.Group32[0].Group43.ToString(new UNOASGCharacterSet());
			AssertEquals("TAX+5++++LPA:::350.0000'MOA+161:100.00'TAX+7'MOA+369:0.00'", actual);
		}

		public void TestExciseUnitSegments_Preference()
		{
			ItemsTestClass item = new ItemsTestClass();
			ItemsTestClass[] items = new ItemsTestClass[] { item };
			item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.LTR;
			item.DutyRateUnit = SGConstants.LPA;
			item.ExciseUnitRate = 48m;
			item.ExciseAmount = 100;
			item.PreferenceIndicator = PreferentialIndicatorCodeList.Codes.PRF;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			string actual = msg.Group32[0].Group43.ToString(new UNOASGCharacterSet());
			AssertEquals("TAX+5+:::PRF'TAX+5+:::PRF+++LPA:::48.0000'MOA+161:100.00'TAX+7'MOA+369:0.00'", actual);
		}

		public void TestDutyUnitSegments_Preference()
		{
			ItemsTestClass item = new ItemsTestClass();
			ItemsTestClass[] items = new ItemsTestClass[] { item };
			item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.LTR;
			item.DutyUnitRateUnit = SGConstants.LPA;
			item.DutyUnitRate = 48m;
			item.DutyAmount = 0;
			item.PreferenceIndicator = PreferentialIndicatorCodeList.Codes.PRF;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			string actual = msg.Group32[0].Group43.ToString(new UNOASGCharacterSet());
			AssertEquals("TAX+5+:::PRF+++:::48.0000'TAX+7'MOA+369:0.00'", actual);
		}

		public void TestPreferenceSTDShouldNotOutput()
		{
			ItemsTestClass item = new ItemsTestClass();
			ItemsTestClass[] items = new ItemsTestClass[] { item };
			item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.LTR;
			item.DutyRateUnit = SGConstants.LPA;
			item.ExciseUnitRate = 48m;
			item.ExciseAmount = 100;
			item.PreferenceIndicator = PreferentialIndicatorCodeList.Codes.STD;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			string actual = msg.Group32[0].Group43.ToString(new UNOASGCharacterSet());
			AssertEquals("TAX+5++++LPA:::48.0000'MOA+161:100.00'TAX+7'MOA+369:0.00'", actual);
		}

		#region Implementation
		protected SGCUSDECTestClass DataProvider
		{
			get
			{
				return fDataProvider ?? (fDataProvider = new SGCUSDECTestClass());
			}
		}

		SGCUSDECTestClass fDataProvider;
		Cusdec09b MessageBuilder
		{
			get
			{
				return fMessageBuilder ?? (fMessageBuilder = new Cusdec09bTestClass(DataProvider));
			}
		}

		Cusdec09b fMessageBuilder;
		INPDECTestClass INPDECDataProvider
		{
			get
			{
				return fINPDECDataProvider ?? (fINPDECDataProvider = new INPDECTestClass());
			}
		}

		INPDECTestClass fINPDECDataProvider;
		IOUTDECTestClass OUTDECDataProvider
		{
			get
			{
				return fIOUTDECDataProvider ?? (fIOUTDECDataProvider = new IOUTDECTestClass());
			}
		}

		IOUTDECTestClass fIOUTDECDataProvider;
		Inpdec09b INPDECMessageBuilder
		{
			get
			{
				return fINPDECMessageBuilder ?? (fINPDECMessageBuilder = new Inpdec09b(INPDECDataProvider));
			}
		}

		Inpdec09b fINPDECMessageBuilder;
		Outdec09b OUTDECMessageBuilder
		{
			get
			{
				return fOUTDECMessageBuilder ?? (fOUTDECMessageBuilder = new Outdec09b(OUTDECDataProvider));
			}
		}

		Outdec09b fOUTDECMessageBuilder;
		protected class Cusdec09bTestClass : Cusdec09b
		{
			public Cusdec09bTestClass(ISGCUSDEC customsDec) : base(customsDec)
			{
			}

			public override string MessageType
			{
				get
				{
					return CommonAccessReferenceCodeList.Codes.INPDEC;
				}
			}

			public override string MessageSubType
			{
				get
				{
					return CUSDECEDIMessage.Declaration;
				}
			}

			protected override string CommonAccessReferenceCode
			{
				get
				{
					return "2";
				}
			}

			public new (decimal, string) ConvertToSingaporeCustomsRequiredWeightUnit(decimal sourceWeight, string sourceWeightUnit, string targetWeightUnit)
			{
				return base.ConvertToSingaporeCustomsRequiredWeightUnit(sourceWeight, sourceWeightUnit, targetWeightUnit);
			}
		}

		SGCPlaceTestClass TestPlace(string code, string type, string nameAndAddress, bool isNonSystemNonLicenced = false)
		{
			SGCPlaceTestClass locationPlace = new SGCPlaceTestClass();
			locationPlace.AddressRequired = nameAndAddress.Length > 0;
			locationPlace.Code = code;
			locationPlace.NameAndAddress = nameAndAddress;
			locationPlace.Type = type;
			locationPlace.IsNonSystemNonLicenced = isNonSystemNonLicenced;
			return locationPlace;
		}

		void ResetMessageBuilder()
		{
			fMessageBuilder = null;
		}

		void ResetINPDECMessageBuilder()
		{
			fINPDECMessageBuilder = null;
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>());
			}
		}

		JobComInvoiceLine invoiceLine;
		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion
		#region EntryHeader
		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
					fEntryHeader.Declaration.AdditionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
				}

				return fEntryHeader;
			}
		}

		CusEntryHeader fEntryHeader;
		#endregion
		#region Interface Objects
		ISGCUSDEC CusEntry
		{
			get
			{
				return EntryHeader;
			}
		}

		#endregion
		#region TestClasses
		public class eDocs : IStorageDocsBaseCollection
		{
			readonly ArrayList docs = new ArrayList();
			#region IStorageDocsBaseCollection Members
			public void Add(IeDoc elementToAdd)
			{
				docs.Add(elementToAdd);
			}

			public bool Contains(IeDoc element)
			{
				return docs.Contains(element);
			}

			public int Count
			{
				get
				{
					return docs.Count;
				}
			}

			public IeDoc GetFromUniqueKey(Guid uniqueKey)
			{
				foreach (IeDoc doc in docs)
				{
					if (doc.UniqueKey == uniqueKey)
					{
						return doc;
					}
				}

				return null;
			}

			public IeDoc GetMostRecentEDoc(string docType)
			{
				return null;
			}

			public void Remove(IeDoc elementToRemove)
			{
			}

			public bool ContainsDocType(ZString docType)
			{
				return false;
			}

			public IeDoc this[int index]
			{
				get
				{
					return (IeDoc)docs[index];
				}
			}

			#endregion
			#region IEnumerable Members
			public IEnumerator GetEnumerator()
			{
				return docs.GetEnumerator();
			}
			#endregion
		}

		sealed public class eDoc : IeDoc
		{
			#region IeDoc Members
			public ZDateTime DateAdded
			{
				get
				{
					return ZDateTime.Now;
				}

				set
				{ /* Why oh why is this code duplicated here and everywhere else???*/
				}
			}

			public ZString Description
			{
				get
				{
					return "Description";
				}

				set
				{
				}
			}

			public ZString DocType
			{
				get
				{
					return docType;
				}

				set
				{
					docType = value;
				}
			}

			ZString docType;
			public ZString DocSourceDescription
			{
				get
				{
					return "Description";
				}

				set
				{
				}
			}

			public ZString DocSource
			{
				get;
				set;
			}

			public CodeDescriptionPairList DocType_List
			{
				get
				{
					return new CodeDescriptionPairList();
				}
			}

			public ZString FileName
			{
				get
				{
					return fileName;
				}

				set
				{
					fileName = value;
				}
			}

			ZString fileName;
			public ZBlob ImageData
			{
				get
				{
					return null;
				}

				set
				{
				}
			}

			public ZBool IsDeleted
			{
				get
				{
					return false;
				}

				set
				{
				}
			}

			public ZBool IsPublished
			{
				get
				{
					return false;
				}

				set
				{
				}
			}

			public ZBool IsSystemGenerated
			{
				get
				{
					return false;
				}
			}

			public ZString VisibleCompanyCode
			{
				get
				{
					return string.Empty;
				}
			}

			public ZString VisibleBranchCode
			{
				get
				{
					return string.Empty;
				}
			}

			public ZString VisibleDepartmentCode
			{
				get
				{
					return string.Empty;
				}
			}

			public ZDateTime LastEdited
			{
				get
				{
					return ZDateTime.Now.AddDays(-1);
				}
			}

			public ZString LastEditedUser
			{
				get
				{
					return GlbStaff.CurrentUser.GS_Code;
				}
			}

			public ZBool IsCustomisableDocTypes
			{
				get
				{
					return false;
				}
			}

			public void NotifyReadByUser()
			{
			}

			public ZGuid UniqueKey
			{
				get
				{
					return uniqueKey;
				}
			}

			readonly ZGuid uniqueKey = ZGuid.NewZGuid();
			public BusinessObject ParentMain
			{
				get
				{
					return null;
				}
			}

			public void SetValuesForTest(ZDateTime dateTime, ZString dataType)
			{
			}

			public ZString DataType
			{
				get
				{
					return null;
				}
			}

			public ZString FileNameOnly
			{
				get
				{
					return null;
				}
			}

			public ZDecimal FileSizeInMB
			{
				get
				{
					return ZDecimal.Zero;
				}
			}

			public IDisposable OpenForEdit()
			{
				throw new NotImplementedException();
			}

			public Stream GetImageDataReader() => new CargoWise.IO.Shim.SubStreamableStream();
			public void SetImageDataStream(Stream stream)
			{
			}

			public string CreateReference()
			{
				throw new NotImplementedException();
			}

			public void Delete()
			{
				throw new NotImplementedException();
			}

			#endregion
		}
		#endregion
		#endregion
	}
}
