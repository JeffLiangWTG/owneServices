using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.Edifact;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	public class CUSDECTest : TestCaseWithFactory
	{
		public virtual void TestMessageSubType()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals(CUSDECEDIMessage.Declaration, MessageBuilder.MessageSubType);
		}

		public void TestGenerateUNH()
		{
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNH+WTG+CUSDEC:D:05B:UN:040+INPDEC'", msg.UNH.ToString(new UNOASGCharacterSet()));
		}

		public void TestUNHMessageReferenceMaxSizeIsNotExceeded()
		{
			DataProvider.JobNumber = "B00001001"; //standard enterprise generated job number
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNH+WTGB00001001+CUSDEC:D:05B:UN:040+INPDEC'", msg.UNH.ToString(new UNOASGCharacterSet()));
			UNHSegment uNH = msg.UNH[0];
			Assert("Mesasge Reference Number max size should not be exceeded", uNH.MessageReferenceNumber.Length < 15);
			ResetMessageBuilder();
			DataProvider.JobNumber = "SSIN090000168"; //client option generated job number format
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNH+GSSIN090000168+CUSDEC:D:05B:UN:040+INPDEC'", msg.UNH.ToString(new UNOASGCharacterSet()));
			uNH = msg.UNH[0];
			Assert("Mesasge Reference Number max size should not be exceeded", uNH.MessageReferenceNumber.Length < 15);
			ResetMessageBuilder();
			DataProvider.JobNumber = "DSSSIN09A0000168003T"; //shipment generated job number format - max 20 chars
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("UNH+09A0000168003T+CUSDEC:D:05B:UN:040+INPDEC'", msg.UNH.ToString(new UNOASGCharacterSet()));
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
			DataProvider.CargoPackingType = CargoPackingTypeCodeList.Codes.PackingType3;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("CST++3'", msg.CST.ToString(new UNOASGCharacterSet()));
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
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BKT;
			DataProvider.PortOfLoading = "";
			DataProvider.PortOfDischarge = "";
			DataProvider.HasInwardTransport = false;
			DataProvider.HasOutwardTransport = false;
			DataProvider.ArrivalDate = new ZDate(2007, 08, 28);
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base DTM Segments expected", 1, msg.DTM.Count);
			AssertEquals("Arrival Date is mandatory for Blanket declarations even with no transport details", "DTM+178:20070828:102'", msg.DTM[0].ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.DUT;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base DTM Segments expected", 0, msg.DTM.Count);
			Assert("Arrival Date should not be sent when no transport details if not BKT dec", !msg.ToString(new UNOASGCharacterSet()).Contains("DTM+178"));
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
			AssertEquals("OuterPack & Weight", "MEA+ABK++BOX:100.0000'MEA+AAH++TNE:111.0000'", msg.MEA.ToString(new UNOASGCharacterSet()));
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
			AssertEquals("OuterPack, Weight & Tonnage of Outward Vessel", "MEA+ABK++CTN:50.0000'MEA+AAH++TNE:1.7500'MEA+AAN++:35000.00'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.IsSea = false;
			DataProvider.OutwardTransportCode = 0;
			DataProvider.TotalOuterPack = 244;
			DataProvider.TotalOuterPackUnitOfQty = "PLT";
			DataProvider.TotalGrossWeight = 1750;
			DataProvider.TotalGrossWeightUnitOfQty = "KG";
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Non Sea Transport - OuterPack & Weight", "MEA+ABK++PLT:244.0000'MEA+AAH++KGM:1750.0000'", msg.MEA.ToString(new UNOASGCharacterSet()));
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
			AssertEquals("Inward Air Declaration - Weight must be in KGM", "MEA+ABK++BOX:100.0000'MEA+AAH++KGM:1300.0000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Inward Sea Declaration - Weight must be in TNE", "MEA+ABK++BOX:100.0000'MEA+AAH++TNE:1.3000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.DRT;
			DataProvider.InwardTransportCode = 0;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.IsInwardDeclaration = false;
			DataProvider.IsOutwardDeclaration = true;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Outward Air Declaration - Weight must be in KGM", "MEA+ABK++BOX:100.0000'MEA+AAH++KGM:1300.0000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Outward Sea Declaration - Weight must be in TNE", "MEA+ABK++BOX:100.0000'MEA+AAH++TNE:1.3000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TTI;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.IsInwardDeclaration = false;
			DataProvider.IsOutwardDeclaration = false;
			DataProvider.IsTranshipmentDeclaration = true;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Transhipment Declaration with Inward Leg being Air - Weight must be in KGM", "MEA+ABK++BOX:100.0000'MEA+AAH++KGM:1300.0000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Transhipment Declaration with Inward Leg being Sea - Weight must be in TNE", "MEA+ABK++BOX:100.0000'MEA+AAH++TNE:1.3000'", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.TotalGrossWeightUnitOfQty = "XXX";
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("Weight unit should not be converted if invalid.", "MEA+ABK++BOX:100.0000'MEA+AAH++XXX:1.3000'", msg.MEA.ToString(new UNOASGCharacterSet()));
		}

		public void TestOnlyRelevantTransportDetailsAreSent()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
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
			Assert("Inward but no outward transport - Inward Berth should be sent", msgString.Contains("LOC+164+KW"));
			Assert("Inward but no outward transport - Outward Berth should not be sent", !msgString.Contains("LOC+234"));
			Assert("Inward but no outward transport - Arrival Date should be sent", msgString.Contains("DTM+178:20070807:102"));
			Assert("Inward but no outward transport - Departure Date should not be sent", !msgString.Contains("DTM+136"));
			Assert("Inward but no outward transport - Inward Transport should be sent", msgString.Contains("TDT+3++1+++++22N:::IN-VSSL"));
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
			Assert("Outward with no inward transport - Outward Berth should be sent", msgString.Contains("LOC+234+JW"));
			Assert("Outward with no inward transport - Arrival Date should not be sent", !msgString.Contains("DTM+178"));
			Assert("Outward with no inward transport - Departure Date should be sent", msgString.Contains("DTM+136:20070810:102"));
			Assert("Outward with no inward transport - Inward Transport should not be sent", !msgString.Contains("TDT+3"));
			Assert("Outward with no inward transport - Outward Transport should be sent", msgString.Contains("TDT+12++1+++++28E:::OUT-VSSL"));
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
			Assert("Inward & Outward transport - Inward Berth should be sent", msgString.Contains("LOC+164+KW"));
			Assert("Inward & Outward transport - Outward Berth should be sent", msgString.Contains("LOC+234+JW"));
			Assert("Inward & Outward transport - Arrival Date should be sent", msgString.Contains("DTM+178:20070807:102"));
			Assert("Inward & Outward transport - Departure Date should be sent", msgString.Contains("DTM+136:20070810:102"));
			Assert("Inward & Outward transport - Inward Transport should be sent", msgString.Contains("TDT+3++1+++++22N:::IN-VSSL"));
			Assert("Inward & Outward transport - Outward Transport should be sent", msgString.Contains("TDT+12++1+++++28E:::OUT-VSSL"));
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
			AssertEquals("Carrier Agent", "NAD+CG+197201770G++SINGAPORE AIRPORT TERMINAL SERVICES:LTD'", carrierAgent.ToString(new UNOASGCharacterSet()));
			NADSegment exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name fits in first segment element", "NAD+EX+12345678S++SMITH JONES CONSTRUCTION PTE LTD'", exporter.ToString(new UNOASGCharacterSet()));
			ResetINPDECMessageBuilder();
			testExporter.Name = "WILLIAM-SMITH JONES CONSTRUCTION PTE LTD";
			msg = INPDECMessageBuilder.CusdecMessage;
			exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name should split elegantly accross elements", "NAD+EX+12345678S++WILLIAM-SMITH JONES CONSTRUCTION:PTE LTD'", exporter.ToString(new UNOASGCharacterSet()));
			ResetINPDECMessageBuilder();
			testExporter.Name = "ANGLO-EASTERN FORWARDING AGENCY (SINGAPORE) PTE LTD";
			msg = INPDECMessageBuilder.CusdecMessage;
			exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name should split elegantly accross elements", "NAD+EX+12345678S++ANGLO-EASTERN FORWARDING AGENCY:(SINGAPORE) PTE LTD'", exporter.ToString(new UNOASGCharacterSet()));
			ResetINPDECMessageBuilder();
			testCarriersAgent.Name = "ANGLO-EASTERN INTERNATIONAL ENGINEERING AND MANUFACTURING SERVICES PTE. LTD";
			testExporter.Name = "ANGLO-EASTERN INTERNATIONAL ENGINEERING AND MANUFACTURING SERVICES PTE. LTD";
			msg = INPDECMessageBuilder.CusdecMessage;
			carrierAgent = msg.Group6[2].NAD[0];
			AssertEquals("Carrier Agent should split elegantly accross 3 elements", "NAD+CG+197201770G++ANGLO-EASTERN INTERNATIONAL:ENGINEERING AND MANUFACTURING:SERVICES PTE. LTD'", carrierAgent.ToString(new UNOASGCharacterSet()));
			exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name should only split accross 2 elements - should show all 70 characters if cannot split", "NAD+EX+12345678S++ANGLO-EASTERN INTERNATIONAL ENGINEE:RING AND MANUFACTURING SERVICES PTE'", exporter.ToString(new UNOASGCharacterSet()));
			ResetINPDECMessageBuilder();
			testCarriersAgent.Name = "ANGLO-EASTERN AND NORTH AMERICAN INTERNATIONAL ENGINEERING MANUFACTURING AND PROJECT MANAGEMENT SERVICES PTE. LIMITED";
			testExporter.Name = "ANGLO-EASTERN AND NORTH AMERICAN INTERNATIONAL ENGINEERING MANUFACTURING AND PROJECT MANAGEMENT SERVICES PTE. LIMITED";
			msg = INPDECMessageBuilder.CusdecMessage;
			carrierAgent = msg.Group6[2].NAD[0];
			AssertEquals("Carrier Agent should show in full when it cannot split elegantly accross elements", "NAD+CG+197201770G++ANGLO-EASTERN AND NORTH AMERICAN IN:TERNATIONAL ENGINEERING MANUFACTURI:NG AND PROJECT MANAGEMENT SERVICES '", carrierAgent.ToString(new UNOASGCharacterSet()));
			exporter = msg.Group6[5].NAD[0];
			AssertEquals("Exporter Name should show in full when it cannot split elegantly accross elements", "NAD+EX+12345678S++ANGLO-EASTERN AND NORTH AMERICAN IN:TERNATIONAL ENGINEERING MANUFACTURI'", exporter.ToString(new UNOASGCharacterSet()));
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
			AssertEquals("Full Importer Name override must fit in first two segment elements when it cannot be split elegantly", "NAD+IM+++HYDRO ALUMINIUM MALAYSIA SDN BHD C/:O OIA GLOBAL LOGISTICS (S) PTE LTD'", importer.ToString(new UNOASGCharacterSet()));
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
			AssertEquals("EQD+CN+ABCU0039477:1+:::FCL20035'EQD+CN+NYKU2188343:2+:::LCL40120'EQD+CN+NYKU4798228:3+:::LCL40025'", msg.EQD.ToString(new UNOASGCharacterSet()));
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
			AssertEquals("EQD+CN+ABCU0039477:1+:::FCL20999'EQD+CN+NYKU2188343:2+:::LCL40001'EQD+CN+NYKU4798228:3+:::LCL40001'", msg.EQD.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateFTX()
		{
			DataProvider.TradersRemarksForMessage = new ZString[] { "This is one string remark", "This is another string remark" };

			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("FTX+AAI+++THIS IS ONE STRING REMARK:THIS IS ANOTHER STRING REMARK'", msg.FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestBaseGenerateSG6()
		{
			AgentInfoTestClass testDeclarant = new AgentInfoTestClass();
			testDeclarant.Name = "Test SG Broker";
			testDeclarant.Phone = "64 85721111";
			testDeclarant.Code = "v13t001";
			DataProvider.Declarant = testDeclarant;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("Base message should generate the Declaring Agent Segment & Declarant Segments (Group 6)", 2, msg.Group6.Count);
			AssertEquals("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL'", msg.Group6[0].NAD.ToString(new UNOASGCharacterSet()));
			SegmentGroup6 declarantSG6 = msg.Group6[1];
			AssertEquals("Declarant NAD", 1, declarantSG6.NAD.Count);
			AssertEquals("Declarant CTA", 1, declarantSG6.CTA.Count);
			AssertEquals("Declarant COM", 1, declarantSG6.COM.Count);
			AssertEquals("NAD+DT++TEST SG BROKER'", declarantSG6.NAD.ToString(new UNOASGCharacterSet()));
			AssertEquals("CTA+IC+:V13T001'", declarantSG6.CTA.ToString(new UNOASGCharacterSet()));
			AssertEquals("COM+64 85721111:TE'", declarantSG6.COM.ToString(new UNOASGCharacterSet()));
		}

		public void TestDeclaringAgentSegment()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertEquals("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL'", msg.Group6[0].NAD.ToString(new UNOASGCharacterSet()));
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
			AssertEquals("Foreign Broker", "NAD+DT++TEST FOREIGN BROKER'", broker.ToString(new UNOASGCharacterSet()));
			AssertEquals("Declarant Code requires trailing space", "CTA+IC+:P0657777 '", brokerID.ToString(new UNOASGCharacterSet()));
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
			AssertEquals("DOC+704+081-003948578'DOC+741+OB883747'DOC+:::DOCUMENT ATTACHMENT+001::ATTDOC_0001'DOC+:::DOCUMENT ATTACHMENT+002::ATTDOC_0002'", msg.Group5.ToString(new UNOASGCharacterSet()));
		}

		public void TestConvertToSingaporeCustomsRequiredWeightUnit()
		{
			CUSDECTestClass cUSDECTestClass = new CUSDECTestClass(DataProvider);
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

		#region Group 33
		public void TestUnitPriceOnlyWhenGoodsSubjectToAdValoremDuties()
		{
			CUSDECTestClass cUSDECTestClass = new CUSDECTestClass(DataProvider);
			ItemsTestClass[] items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].ExcisePercentageRate = 20;
			items[0].DutyPercentageRate = 20;
			DataProvider.Items = items;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 33", "MOA+63:0.00'MOA+146:0.0000", msg.Group30[0].Group33.ToString(new UNOASGCharacterSet()), '\'');
			items[0].ExcisePercentageRate = 0;
			items[0].DutyPercentageRate = 20;
			fMessageBuilder = null;
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 33", "MOA+63:0.00'MOA+146:0.0000", msg.Group30[0].Group33.ToString(new UNOASGCharacterSet()), '\'');
			items[0].ExcisePercentageRate = 20;
			items[0].DutyPercentageRate = 0;
			fMessageBuilder = null;
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 33", "MOA+63:0.00'MOA+146:0.0000", msg.Group30[0].Group33.ToString(new UNOASGCharacterSet()), '\'');
			items[0].ExcisePercentageRate = 0;
			fMessageBuilder = null;
			msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 33", "MOA+63:0.00", msg.Group30[0].Group33.ToString(new UNOASGCharacterSet()), '\'');
		}

		#endregion
		#region Group 35 Tests
		public void TestGenerateSegmentGroup35()
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
			ZString resultExpected = "RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'IMD++8'FTX+PRD+++4800.00:CC'RFF+AEA:SEASTORE'GIN+AV+8+15'";
			AssertMultilineEquals("Group 35 - Seastores / Motor Vehicle", resultExpected, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35Strategic()
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
			AssertMultilineEquals("Group 35 - Base Should not generate Strategic Goods", "", MessageBuilder.CusdecMessage.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35MotorVehicles()
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
			ZString resultExpected = "RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'IMD++8'FTX+PRD+++4800.00:CC'";
			AssertMultilineEquals("Group 35 - Motor Vehicle", resultExpected, msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35Seastores()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			DataProvider.Items = items;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NumberOfCrew = 8;
			DataProvider.VoyageDuration = 15;
			AssertMultilineEquals("Group 35 - Seastores", "RFF+AEA:SEASTORE'GIN+AV+8+15'", MessageBuilder.CusdecMessage.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35NoProductCode()
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
			AssertMultilineEquals("Group 35 - Chemical Concentration limit", "RFF+AVM'GIN+AV+5+50'", msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup35NoProductCodeButOtherG35()
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
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NumberOfCrew = 8;
			DataProvider.VoyageDuration = 15;
			CUSDECMessage msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("Group 35 - No Product code but with other CASC values & Seastores", "RFF+AVM'GIN+AV+5+50'RFF+AEA:SEASTORE'GIN+AV+8+15'", msg.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestInvoiceNumberIsNotIncluded()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			DataProvider.Items = items;
			AssertMultilineEquals("Group 35 - should be empty", "", MessageBuilder.CusdecMessage.Group30[0].Group35.ToString(new UNOASGCharacterSet()), '\'');
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
			string actual = msg.Group30[0].Group41[0].ToString(new UNOASGCharacterSet());
			AssertEquals("TAX+5++++LPA:::350.0000'MOA+161:100.00'", actual);
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
		CUSDEC MessageBuilder
		{
			get
			{
				return fMessageBuilder ?? (fMessageBuilder = new CUSDECTestClass(DataProvider));
			}
		}

		CUSDEC fMessageBuilder;
		INPDECTestClass INPDECDataProvider
		{
			get
			{
				return fINPDECDataProvider ?? (fINPDECDataProvider = new INPDECTestClass());
			}
		}

		INPDECTestClass fINPDECDataProvider;
		INPDEC INPDECMessageBuilder
		{
			get
			{
				return fINPDECMessageBuilder ?? (fINPDECMessageBuilder = new INPDEC(fINPDECDataProvider));
			}
		}

		INPDEC fINPDECMessageBuilder;
		protected class CUSDECTestClass : CUSDEC
		{
			public CUSDECTestClass(ISGCUSDEC customsDec) : base(customsDec)
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
		#endregion
	}
}
