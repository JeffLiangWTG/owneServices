using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Edifact;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B.Testing
{
	public class Iptdec09bTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGenerateIPTDEC()
		{
			var testMessage = TestMessageBuilder.CusdecMessage;
			AssertNotNull(testMessage);
		}

		public void TestMandatorySegmentsGenerated()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetTestData();
			var testMessage = TestMessageBuilder.CusdecMessage;
			string messageTextResult = TestMessageBuilder.MessageText;
			Assert("Message requires correct version UNH segment", messageTextResult.StartsWith("UNH+WTG+CUSDEC:D:09B:UN:041+IPTDEC"));
			AssertEquals("Message should be IPTDEC message", "IPTDEC", testMessage.UNH[0].CommonAccessReference);
			Assert("Message must have BGM segment", messageTextResult.Contains("BGM+914"));
			Assert("Message requires Cargo Packing Type", messageTextResult.Contains("CST++1'"));
			Assert("Message requires Port of Loading", messageTextResult.Contains("LOC+9+HKHKG'"));
			Assert("Message requires Place of Release", messageTextResult.Contains("LOC+11+CZ'"));
			Assert("Message requires Place of Receipt", messageTextResult.Contains("LOC+88+CZ'"));
			Assert("Message requires Arrival Date", messageTextResult.Contains("DTM+178:20061212:102'"));
			Assert("Message requires Declaration segment", messageTextResult.Contains("GEI+5+:Y'"));
			Assert("Message requires Total Outer Pack", messageTextResult.Contains("MEA+ABK++CTN:18'"));
			Assert("Message requires Total Gross Weight", messageTextResult.Contains("MEA+AAH++KGM:85.750'"));
			Assert("Message must have First Section Control Segment", testMessage.UNS1.Count > 0);
			Assert("Message must have Segment Group 11", testMessage.Group11.Count > 0);
			Assert("Message must have Second Section Control Segment", testMessage.UNS2.Count > 0);
		}

		public void TestMessageStructure()
		{
			var testMessage = TestMessageBuilder.CusdecMessage;
			string messageTextResult = TestMessageBuilder.MessageText;
			Assert("Message requires correct version UNH segment", messageTextResult.StartsWith("UNH+WTG+CUSDEC:D:09B:UN:041+IPTDEC"));
			AssertEquals("Message should be IPTDEC message", "IPTDEC", testMessage.UNH[0].CommonAccessReference);
			Assert("Message requires BGM segment", messageTextResult.Contains("BGM+914"));
			Assert("Message must have First Section Control Segment", testMessage.UNS1.Count > 0);
			Assert("Message must have Second Section Control Segment", testMessage.UNS2.Count > 0);
		}

		[TestDate(2008, 10, 28)]
		public void TestSegmentsGenerated()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetFullTestData(DeclarationTypeCodeList.Codes.DUT);
			var testMessage = TestMessageBuilder.CusdecMessage;
			string messageTextResult = TestMessageBuilder.MessageText;
			Assert("Port of Loading", messageTextResult.Contains("LOC+9+HKHKG'"));
			Assert("Place of Receipt", messageTextResult.Contains("LOC+88+CW'"));
			Assert("Place of Release", messageTextResult.Contains("LOC+11+CCJ'"));
			Assert("Arrival Date", messageTextResult.Contains("DTM+178:20061213:102'"));
			AssertEquals("Container EQD segments expected", 3, testMessage.EQD.Count);
			AssertEquals("Container SEL segments expected", 3, testMessage.SEL.Count);
			Assert("Container segments", messageTextResult.Contains("EQD+CN+FLMU0039485:1+16:::FCL40'EQD+CN+FLMU8528974:2+15:::FCL40'EQD+CN+JJYU497621:3+4:::LCL20'SEL+AK0038-92'SEL+NA'SEL+127ST5'"));
			AssertEquals("SegmentGroup5 count expected", 1, testMessage.Group5.Count);
			AssertEquals("DOC segments expected", 1, testMessage.Group5[0].DOC.Count);
			Assert("Master Bill", messageTextResult.Contains("DOC+704+OBL00384758'"));
			AssertEquals("SegmentGroup6 count expected", 6, testMessage.Group6.Count);
			Assert("Declarant Agent", messageTextResult.Contains("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL:::::1'"));
			Assert("Declarant segments", messageTextResult.Contains("NAD+DT'CTA+IC+V13T001:CHOW LEW WEE'COM+?+64 9760 2847:TE'"));
			AssertEquals("Detail Section: SG11 count expected", 1, testMessage.Group11.Count);
			AssertEquals("Detail Section: SG12 count expected", 1, testMessage.Group11[0].Group12.Count);
			Assert("Invoice Total", messageTextResult.Contains("MOA+39:23012.00:SGD'"));
			AssertEquals("Detail Section: no SG13 expected as currency in SGD", 0, testMessage.Group11[0].Group12[0].Group13.Count);
			AssertEquals("Detail Section: SG14 count expected", 1, testMessage.Group11[0].Group14.Count);
			Assert("Invoice Terms", messageTextResult.Contains("TOD+++FOB'"));
			AssertEquals("Detail Section: SG15 count expected", 1, testMessage.Group11[0].Group15.Count);
			Assert("Supplier", messageTextResult.Contains("NAD+SU++14242880000R+PACECO INDUSTRIAL SUPPLIES PTE LTD'"));
			AssertEquals("Detail Section: SG16 count expected", 1, testMessage.Group11[0].Group15[0].Group16.Count);
			Assert("Invoice Number", messageTextResult.Contains("DOC+380+INV0103101.9'"));
			Assert("Invoice Date", messageTextResult.Contains("DTM+3:20061220:102'"));
			AssertEquals("Detail Section: SG20 count expected", 2, testMessage.Group11[0].Group20.Count);
			Assert("Freight Charges", messageTextResult.Contains("ALC+C'MOA+64:100.00:SGD'PCD+5:1.500'"));
			Assert("Insurance Charges", messageTextResult.Contains("ALC+C'MOA+70:178.90:USD'PCD+5:1.380'CUX+++1.540000'"));
			AssertEquals("Line Section: SG32 count expected", 3, testMessage.Group32.Count);
			var line1 = testMessage.Group32[0];
			AssertEquals("Line 1 - CST segment", "CST+1+48063000'", line1.CST.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line Section: Line 1 FTX count expected", 3, line1.FTX.Count);
			AssertEquals("Line 1 - goods description segment", "FTX+AAA+++TRACING PAPERS'", line1.FTX[0].ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 1 - brand and model", "FTX+PRD+++SPICERS'", line1.FTX[1].ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 1 - dangerous goods indicator", "FTX+AAC+++N'", line1.FTX[2].ToString(new UNOASGCharacterSet()));
			var line2 = testMessage.Group32[1];
			AssertEquals("Line 2 - CST segment", "CST+2+48083000'", line2.CST.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 2 - FTX segments", "FTX+AAA+++OTHER KRAFT PAPER CREPED OR CRINKLED'FTX+PRD+++SPICERS'FTX+AAC+++N'", line2.FTX.ToString(new UNOASGCharacterSet()));
			var line3 = testMessage.Group32[2];
			AssertEquals("Line 3 - CST segment", "CST+3+48091010'", line3.CST.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 3 - FTX segments", "FTX+AAA+++CARBON PAPER IN ROLLS OR SHEETS'FTX+PRD+++SPICERS'FTX+AAC+++N'", line3.FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestLiqourDeclaration()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetLiqourTestData(DeclarationTypeCodeList.Codes.DUT);
			var testMessage = TestMessageBuilder.CusdecMessage;
			string messageTextResult = TestMessageBuilder.MessageText;
			Assert("Port of Loading", messageTextResult.Contains("LOC+9+USLAX'"));
			Assert("Place of Release", messageTextResult.Contains("LOC+11+CCJ'"));
			Assert("Place of Receipt", messageTextResult.Contains("LOC+88+CW'"));
			Assert("Arrival Date", messageTextResult.Contains("DTM+178:20070108:102'"));
			AssertEquals("Container EQD segments expected", 1, testMessage.EQD.Count);
			AssertEquals("Container SEL segments expected", 1, testMessage.SEL.Count);
			Assert("Container segments", messageTextResult.Contains("EQD+CN+FLMU0039485:1+16:::FCL40'SEL+AK0038-92'"));
			AssertEquals("SegmentGroup5 count expected", 1, testMessage.Group5.Count);
			AssertEquals("DOC segments expected", 1, testMessage.Group5[0].DOC.Count);
			Assert("Master Bill", messageTextResult.Contains("DOC+704+OBL00384758'"));
			AssertEquals("SegmentGroup6 count expected", 6, testMessage.Group6.Count);
			Assert("Declarant Agent", messageTextResult.Contains("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL:::::1'"));
			Assert("Declarant segments", messageTextResult.Contains("NAD+DT'CTA+IC+V13T001:CHOW LEW WEE'COM+?+64 9760 2847:TE'"));
			AssertEquals("Detail Section: SG11 count expected", 1, testMessage.Group11.Count);
			AssertEquals("Detail Section: SG12 count expected", 1, testMessage.Group11[0].Group12.Count);
			Assert("Invoice Total", messageTextResult.Contains("MOA+39:570000.00:USD'"));
			AssertEquals("Detail Section: 1 SG13 expected as currency not in SGD", 1, testMessage.Group11[0].Group12[0].Group13.Count);
			Assert("Invoice Currency Exch Rate", messageTextResult.Contains("CUX+++1.540000'"));
			AssertEquals("Detail Section: SG14 count expected", 1, testMessage.Group11[0].Group14.Count);
			Assert("Invoice Terms", messageTextResult.Contains("TOD+++FOB'"));
			AssertEquals("Detail Section: SG15 count expected", 1, testMessage.Group11[0].Group15.Count);
			Assert("Supplier", messageTextResult.Contains("NAD+SU++14242880000R+JIM BEAM ASIA PACIFIC DISTRIBUTION'"));
			AssertEquals("Detail Section: SG16 count expected", 1, testMessage.Group11[0].Group15[0].Group16.Count);
			Assert("Invoice Number", messageTextResult.Contains("DOC+380+JB200629837'"));
			Assert("Invoice Date", messageTextResult.Contains("DTM+3:20061220:102'"));
			AssertEquals("Detail Section: SG20 count expected", 2, testMessage.Group11[0].Group20.Count);
			Assert("Freight Charges", messageTextResult.Contains("ALC+C'MOA+64:100.00:SGD'PCD+5:1.500'"));
			Assert("Insurance Charges", messageTextResult.Contains("ALC+C'MOA+70:178.90:USD'PCD+5:1.380'CUX+++1.540000'"));
			AssertEquals("Line Section: SG32 count expected", 1, testMessage.Group32.Count);
			var line1 = testMessage.Group32[0];
			AssertEquals("Line 1 - CST segment", "CST+1+22083020'", line1.CST.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line Section: Line 1 FTX count expected", 3, line1.FTX.Count);
			AssertEquals("Line 1 - goods description segment", "FTX+AAA+++WHISKIES OVER 46% VOL'", line1.FTX[0].ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 1 - brand", "FTX+PRD+++JIM BEAM TEXAS BOURBON'", line1.FTX[1].ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 1 - dangerous goods indicator", "FTX+AAC+++N'", line1.FTX[2].ToString(new UNOASGCharacterSet()));
			AssertEquals("Line Section: SG33 count expected", 4, line1.Group33.Count);
			AssertEquals("Line 1 - Outer Pack", "PAC+1+3+CTN'", line1.Group33[0].PAC.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 1 - In Pack", "PAC+25+2+CRT'", line1.Group33[1].PAC.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 1 - Inner Pack", "PAC+1650+1+BOX'", line1.Group33[2].PAC.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 1 - Inmost Pack", "PAC+19800+5+BOT'", line1.Group33[3].PAC.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line Section: SG35 count expected", 2, line1.Group35.Count);
			AssertEquals("Line 1 - CIF/FOB Value in SGD", "MOA+63:79950.00'", line1.Group35[0].MOA.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line 1 - LSP", "MOA+41:57.38'", line1.Group35[1].MOA.ToString(new UNOASGCharacterSet()));
			AssertEquals("Line Section: SG39 count expected", 1, line1.Group39.Count);
			AssertEquals("House Bill", "DOC+703+INWARDHAWB'", line1.Group39[0].DOC.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup37()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			items[0].IsMotorVehicle = true;
			ProductCodesTestClass[] cASC = new ProductCodesTestClass[1];
			cASC[0] = new ProductCodesTestClass();
			cASC[0].ProductCode = "ProdCode_00001";
			cASC[0].ProductCodeQty = 3.5;
			cASC[0].ProductCodeUnitType = UnitOfQuantityCodeList.Codes.TNE;
			items[0].ProductCodes = cASC;
			items[0].EngineCapacity = 4800;
			items[0].EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			items[0].RegistrationNumberSG = "SG12345";
			var testCodes1 = new CASCCode1Collection(InvoiceLine);
			var engineNo = Factory.New<CASCCode1>();
			engineNo.CY_Data = "jkue903jrm3499emdfjf93eikr30rff";
			testCodes1.Add(engineNo);
			items[0].CASCCodes1 = testCodes1;
			var testCodes2 = new CASCCode2Collection(InvoiceLine);
			var chassisNo = Factory.New<CASCCode2>();
			chassisNo.CY_Data = "v83945030eif9gt8d903kdfmrk93ik";
			testCodes2.Add(chassisNo);
			items[0].CASCCodes2 = testCodes2;
			items[0].IsStrategic = true;
			items[0].CategoryCode = "XY09348TFZ";
			items[0].EndUseDescription = "Commercial application";
			items[0].EndUseCode1 = CA_SC1CodeList.Codes.NMU;
			items[0].EndUseCode2 = CA_SC2CodeList.Codes.GOV;
			items[0].EndUseCode3 = CA_SC3CodeList.Codes.NMD;
			items[0].DateOfFirstRegistration = new ZDate(2005, 11, 23);
			dataProvider.Items = items;
			dataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GST;
			dataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			var msg = TestMessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+IV:INV100023'RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'DTM+375:20051123:102'IMD++8+:::4800.00:CC'RFF+VT:SG12345'";
			AssertMultilineEquals("Group 37 - Strategic Motor Vehicle", resultExpected, msg.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup37Strategic()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			items[0].HSCode = "88033000";
			items[0].HSQuantity = 3.5;
			items[0].HSQuantityUnitType = UnitOfQuantityCodeList.Codes.TNE;
			items[0].IsStrategic = true;
			items[0].CategoryCode = "XY09348TFZ";
			items[0].EndUseDescription = "Commercial aircraft spares";
			items[0].EndUseCode1 = CA_SC1CodeList.Codes.NMU;
			items[0].EndUseCode2 = CA_SC2CodeList.Codes.NGU;
			items[0].EndUseCode3 = CA_SC3CodeList.Codes.NMD;
			dataProvider.Items = items;
			dataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GST;
			dataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			var msg = TestMessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+IV:INV100023'"; // no strategic goods for IPT
			AssertMultilineEquals("Group 37 - s/b NO Strategic Goods segments", resultExpected, msg.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup37MotorVehicles()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
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
			var testCodes1 = new CASCCode1Collection(InvoiceLine);
			var vinNo = Factory.New<CASCCode1>();
			vinNo.CY_Data = "jkue903jrm3499emdfjf93eikr30rff";
			testCodes1.Add(vinNo);
			items[0].CASCCodes1 = testCodes1;
			var testCodes2 = new CASCCode2Collection(InvoiceLine);
			var chassisNo = Factory.New<CASCCode2>();
			chassisNo.CY_Data = "v83945030eif9gt8d903kdfmrk93ik";
			testCodes2.Add(chassisNo);
			items[0].CASCCodes2 = testCodes2;
			dataProvider.Items = items;
			dataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GST;
			dataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			var msg = TestMessageBuilder.CusdecMessage;
			var resultExpected = "RFF+IV:INV100023'RFF+AEA:PRODCODE_00001'GIN+AV+JKUE903JRM3499EMDFJF93EIKR30RFF+V83945030EIF9GT8D903KDFMRK93IK'RFF+SE'DTM+375:20051123:102'IMD++8+:::4800.00:CC'RFF+VT:SG12345'";
			AssertMultilineEquals("Group 37 - Motor Vehicle", resultExpected, msg.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup37Seastores()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			items[0].HSCode = "24022090";
			items[0].HSQuantity = 25;
			items[0].HSQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			dataProvider.Items = items;
			dataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GST;
			dataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			dataProvider.IsSeaStoreDeclaration = true;
			dataProvider.NumberOfCrew = 8;
			dataProvider.VoyageDuration = 15;
			CUSDECMessage msg = TestMessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+IV:INV100023'"; // no seastores for IPT
			AssertMultilineEquals("Group 37 - s/b NO Seastores segments", resultExpected, msg.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup37NoProductCode()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].InvoiceNumber = "INV100023";
			var testCodes1 = new CASCCode1Collection(InvoiceLine);
			var chemicalPurity = Factory.New<CASCCode1>();
			chemicalPurity.CY_Data = "5";
			testCodes1.Add(chemicalPurity);
			items[0].CASCCodes1 = testCodes1;
			var testCodes2 = new CASCCode2Collection(InvoiceLine);
			var maximumQty = Factory.New<CASCCode2>();
			maximumQty.CY_Data = "50";
			testCodes2.Add(maximumQty);
			items[0].CASCCodes2 = testCodes2;
			dataProvider.Items = items;
			var msg = TestMessageBuilder.CusdecMessage;
			ZString resultExpected = "RFF+IV:INV100023'RFF+AVM'GIN+AV+5+50'";
			AssertMultilineEquals("Group 37 - Chemical Concentration limit", resultExpected, msg.Group32[0].Group37.ToString(new UNOASGCharacterSet()), '\'');
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			dataProvider = new SGCUSDECTestClass();
		}

		SGCUSDECTestClass dataProvider;
		Iptdec09b TestMessageBuilder
		{
			get
			{
				if (fTestMessageBuilder == null)
				{
					fTestMessageBuilder = new Iptdec09b(dataProvider);
				}

				return fTestMessageBuilder;
			}
		}

		Iptdec09b fTestMessageBuilder;
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
