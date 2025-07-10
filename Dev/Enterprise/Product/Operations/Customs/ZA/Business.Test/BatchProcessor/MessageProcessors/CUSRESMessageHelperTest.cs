using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	[TestedType(typeof(CUSRESMessageHelper))]
	sealed class CUSRESMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentNumber()
		{
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+963+AEF5840B836D4F71B342D6B93E0692A3'DTM+178:20170715:102'TDT+20+V345+1'LOC+22+DUR::ZZZ'GIS+6:120:ZZZ'NAD+AG+00000000'RFF+BH:BILL1'RFF+AAS:MASTERBILLLXXX'DTM+137:20170718:102'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++CON GROSS MASS / WEIGHT where CCC=00053592,MCC=SAFM,TDN=BILL1,MTD=MAST:ERBILLLXXX,value=?'null?'?: Field is required: : : 'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++BOL CONSIGNOR NAME where CCC=00053592,MCC=SAFM,TDN=BILL1,MTD=MASTERBIL:LLXXX,value=?'null?'?: Field is required: : : 'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++BOL CONSIGNOR ADDRESS LINE - 1 where value=?'null?'?: Field is required: : : : 'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++BLL GROSS MASS / WEIGHT where CCC=00053592,MCC=SAFM,TDN=BILL1,MTD=MAST:ERBILLLXXX,value=?'null?'?: Field is required: : : 'UNT+23+1'";
			var testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals("AEF5840B836D4F71B342D6B93E0692A3", testHelper.LRNNumber);
		}

		public void TestHelperForInvalidMessage()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSDEC_BGM9_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			AssertNull(testHelper);
		}

		public void TestHelperForValidMessage_GIS6()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSRES_GIS6_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			CombineAssertions("GIS 6", () =>
			{
				AssertNotNull(testHelper);
				AssertEquals("EntryStatus", "6", testHelper.EntryStatus);
				AssertEquals("OutgoingMessageNumber", "35", testHelper.OutgoingMessageNumber);
				AssertEquals("MRNNumber", string.Empty, testHelper.MRNNumber);
				AssertEquals("AssessmentDate", ZDateTime.Empty, testHelper.AssessmentDate);
				AssertEquals("PostingDate", ZDateTime.Empty, testHelper.PostingDate);
				AssertEquals("InterchangeTime", ZDateTime.Empty, testHelper.InterchangeTime);
				AssertEquals("CaseNumber", string.Empty, testHelper.CaseNumber);
				AssertEquals("CustomsPrintIndicator", "", testHelper.CustomsPrintIndicator);
				AssertEquals("AgentCode", "00505655", testHelper.AgentCode);
				AssertEquals("CustomsOfficeCode", "BBR", testHelper.CustomsOfficeCode);
				AssertEquals("CustomsOfficeDescription", "Beit Bridge", testHelper.CustomsOfficeDescription);
				AssertEquals("CustomsPrintIndicatorDescription", "", testHelper.CustomsPrintIndicatorDescription);
				AssertEquals("EntryStatusDescription", "Reject To Clearer", testHelper.EntryStatusDescription);
				AssertEquals("LRNNumber", "00505655BBR20160513000034", testHelper.LRNNumber);
				AssertEquals("TransportDocumentNumber", "HENRYMASTER1", testHelper.TransportDocumentNumber);
				AssertEquals("HouseBill", "00505655", testHelper.HouseBill);
				AssertEquals("VoyageFlightNo", "", testHelper.VoyageFlightNo);
				AssertEquals("TransportCode", "", testHelper.TransportCode);
				AssertEquals("DocumentDate", ZDateTime.Empty, testHelper.DocumentDate);
				AssertEquals("ETA", ZDateTime.Empty, testHelper.ETA);
				AssertEquals("ATA", ZDateTime.Empty, testHelper.ATA);
			});
		}

		public void TestHelperForValidMessage_GIS26()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("JSA");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("26");
			Factory.Save();
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSRES_GIS26_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			CombineAssertions("GIS 26", () =>
			{
				AssertNotNull(testHelper);
				AssertEquals("EntryStatus", "26", testHelper.EntryStatus);
				AssertEquals("OutgoingMessageNumber", "152", testHelper.OutgoingMessageNumber);
				AssertEquals("MRNNumber", "JSA201604265109719", testHelper.MRNNumber);
				AssertEquals("AssessmentDate", new ZDateTime(2016, 04, 27), testHelper.AssessmentDate);
				AssertEquals("PostingDate", new ZDateTime(2016, 04, 26), testHelper.PostingDate);
				AssertEquals("InterchangeTime", ZDateTime.Empty, testHelper.InterchangeTime);
				AssertEquals("CaseNumber", "199941811", testHelper.CaseNumber);
				AssertEquals("CustomsPrintIndicator", "N", testHelper.CustomsPrintIndicator);
				AssertEquals("AgentCode", "00469468", testHelper.AgentCode);
				AssertEquals("CustomsOfficeCode", "JSA", testHelper.CustomsOfficeCode);
				AssertEquals("CustomsOfficeDescription", "O.R. TAMBO INT AIRPORT", testHelper.CustomsOfficeDescription);
				AssertEquals("CustomsPrintIndicatorDescription", "NO CUSTOMS PRINTED RELEASE REQUIRED", testHelper.CustomsPrintIndicatorDescription);
				AssertEquals("EntryStatusDescription", "Amendment notification (Inspection report outcome)", testHelper.EntryStatusDescription);
				AssertEquals("LRNNumber", "00469468JSA20160426263869", testHelper.LRNNumber);
				AssertEquals("TransportDocumentNumber", "618-98921841", testHelper.TransportDocumentNumber);
				AssertEquals("HouseBill", "00469468MGLSHA160113", testHelper.HouseBill);
				AssertEquals("VoyageFlightNo", "SQ478", testHelper.VoyageFlightNo);
				AssertEquals("TransportCode", "4", testHelper.TransportCode);
				AssertEquals("DocumentDate", new ZDateTime(2016, 04, 27), testHelper.DocumentDate);
				AssertEquals("ETA", new ZDateTime(2016, 04, 29), testHelper.ETA);
				AssertEquals("ATA", ZDateTime.Empty, testHelper.ATA);
			});
		}

		public void TestHelperForValidMessage_GIS1()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("CLP");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSRES_GIS1_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			CombineAssertions("GIS 1", () =>
			{
				AssertNotNull(testHelper);
				AssertEquals("EntryStatus", "1", testHelper.EntryStatus);
				AssertEquals("OutgoingMessageNumber", "246", testHelper.OutgoingMessageNumber);
				AssertEquals("MRNNumber", "CLP201605145000001", testHelper.MRNNumber);
				AssertEquals("AssessmentDate", new ZDateTime(2016, 05, 14), testHelper.AssessmentDate);
				AssertEquals("PostingDate", ZDateTime.Empty, testHelper.PostingDate);
				AssertEquals("InterchangeTime", ZDateTime.Empty, testHelper.InterchangeTime);
				AssertEquals("CaseNumber", string.Empty, testHelper.CaseNumber);
				AssertEquals("CustomsPrintIndicator", "Y", testHelper.CustomsPrintIndicator);
				AssertEquals("AgentCode", "00505655", testHelper.AgentCode);
				AssertEquals("CustomsOfficeCode", "CLP", testHelper.CustomsOfficeCode);
				AssertEquals("CustomsOfficeDescription", "CALEDONSPOORT", testHelper.CustomsOfficeDescription);
				AssertEquals("CustomsPrintIndicatorDescription", "CUSTOMS PRINTED RELEASE REQUIRED", testHelper.CustomsPrintIndicatorDescription);
				AssertEquals("EntryStatusDescription", "Release", testHelper.EntryStatusDescription);
				AssertEquals("LRNNumber", "00505655CLP20160514000245", testHelper.LRNNumber);
				AssertEquals("TransportDocumentNumber", "081-99876545", testHelper.TransportDocumentNumber);
				AssertEquals("RegistrationNumber", "CARN0111123J", testHelper.RegistrationNumber);
				AssertEquals("HouseBill", "0003264", testHelper.HouseBill);
				AssertEquals("VoyageFlightNo", "QF987", testHelper.VoyageFlightNo);
				AssertEquals("TransportCode", "4", testHelper.TransportCode);
				AssertEquals("DocumentDate", new ZDateTime(2016, 05, 14), testHelper.DocumentDate);
				AssertEquals("ETA", ZDateTime.Empty, testHelper.ETA);
				AssertEquals("ATA", new ZDateTime(2016, 05, 11), testHelper.ATA);
			});
		}

		public void TestDoesEntryStatusNeedsToBeUpdated()
		{
			var refCusFactory = Factory.CreateNewFactory();
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(refCusFactory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var refCusCodeList = zaTestHelper.CreateCustomsStatusCusCodeEntry("1");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("9");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("28");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("43");
			refCusFactory.Save();
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160802:0844+1299++CUSRES+++GWWTGTEST+1'";
			var message = (CUSRESEDIMessage)interchange.ContainedMessages.AddNew(typeof(CUSRESEDIMessage));
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("1"); //Response 1 CLR
			var helper = CUSRESMessageHelper.New(message);
			AssertEquals(false, helper.DoesEntryStatusNeedsToBeUpdated(null));
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(true, helper.DoesEntryStatusNeedsToBeUpdated(entry));
			entry.CH_EntryStatus = "A";
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("9"); //Response 9 NOT (NOTification)
			helper = CUSRESMessageHelper.New(message);
			AssertEquals(false, helper.DoesEntryStatusNeedsToBeUpdated(entry));
			var testCUSDEC = Factory.New<ZAMessage>();
			testCUSDEC.EM_ReceiveTransmit = "TRX";
			testCUSDEC.EM_ApplicationCode = "ZAC";
			testCUSDEC.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			testCUSDEC.EM_Status = "QUE";
			testCUSDEC.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "");
			testCUSDEC.EM_MessageNum = "202";
			entry.Messages.Add(testCUSDEC);
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("43"); //Response 43 STD
			helper = CUSRESMessageHelper.New(message);
			AssertEquals(false, helper.DoesEntryStatusNeedsToBeUpdated(entry));
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("28"); //Response 28 CAN
			helper = CUSRESMessageHelper.New(message);
			AssertEquals(true, helper.DoesEntryStatusNeedsToBeUpdated(entry));
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("1"); //Response 1 CLR
			helper = CUSRESMessageHelper.New(message);
			AssertEquals(true, helper.DoesEntryStatusNeedsToBeUpdated(entry));
			testCUSDEC.EM_MessageNum = "203";
			AssertEquals(false, helper.DoesEntryStatusNeedsToBeUpdated(entry));
			entry.Logs.AddNew(Events.CustomsEntryStatus, "A", new ZDateTimeOffset(2016, 8, 2, 9, 44, 0));
			AssertEquals(false, helper.DoesEntryStatusNeedsToBeUpdated(entry));
			var attrib = refCusCodeList.Attributes.Find(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(RefCusCodeListAttributeTypes.Codes.IUpdateCustomsStatus)).FirstOrDefault();
			attrib.Delete();
			refCusFactory.Save();
			refCusCodeList.Reload();
			var dec2Factory = Factory.CreateNewFactory();
			var declaration2 = dec2Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("1"); //Response 1 CLR
			helper = CUSRESMessageHelper.New(message);
			AssertEquals(false, helper.DoesEntryStatusNeedsToBeUpdated(entry2));
		}

		public void TestMessageDate()
		{
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160802:0844+1299++CUSRES+++GWWTGTEST+1'";
			var message = (ZAMessage)interchange.ContainedMessages.AddNew(typeof(ZAMessage));
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("9"); //Response 9 NOT (NOTification)
			var helper = CUSRESMessageHelper.New(message);
			AssertEquals(new ZDateTime(2016, 8, 2, 8, 44, 0), helper.MessageDate);
		}

		public void TestCustomsStatusFreeTexts()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSDEC_FTX_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			CombineAssertions("FTX", () =>
			{
				AssertNotNull(testHelper.CustomsStatusFreeTexts);
				AssertEquals(3, testHelper.CustomsStatusFreeTexts.Count);
				var customsStatusFreeText = testHelper.CustomsStatusFreeTexts[0];
				AssertEquals("1", customsStatusFreeText.Line);
				AssertEquals("2108", customsStatusFreeText.Code);
				AssertEquals(" FIELD(Warehousing details) DESCR(No matching previous declaration could be found for MRN JSA201501011234567)", customsStatusFreeText.FreeText);
				customsStatusFreeText = testHelper.CustomsStatusFreeTexts[1];
				AssertEquals("2", customsStatusFreeText.Line);
				AssertEquals("9272", customsStatusFreeText.Code);
				AssertEquals(" FIELD(Warehousing B/E line no) DESCR(This field is required) FIELD(Warehousing B/E line no) DESCR(This field is required)", customsStatusFreeText.FreeText);
				customsStatusFreeText = testHelper.CustomsStatusFreeTexts[2];
				AssertEquals("2", customsStatusFreeText.Line);
				AssertEquals("9273", customsStatusFreeText.Code);
				AssertEquals(" FIELD(Warehousing B/E line no) DESCR(This field is required) FIELD(Warehousing B/E line no) DESCR(This field is required)", customsStatusFreeText.FreeText);
			});
		}

		public void TestHeaderCustomsStatusFreeTexts()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSRES_FTX_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			CombineAssertions("FTX", () =>
			{
				AssertNotNull(testHelper.HeaderCustomsStatusFreeTexts);
				AssertEquals(2, testHelper.HeaderCustomsStatusFreeTextsCount);
				var customsStatusFreeText = testHelper.HeaderCustomsStatusFreeTexts[0];
				AssertEquals("0", customsStatusFreeText.Line);
				AssertEquals("8463", customsStatusFreeText.Code);
				AssertEquals(" FIELD(Depot/Terminal Code) DESCR(Field must be empty)", customsStatusFreeText.FreeText);
				customsStatusFreeText = testHelper.HeaderCustomsStatusFreeTexts[1];
				AssertEquals("0", customsStatusFreeText.Line);
				AssertEquals("1173", customsStatusFreeText.Code);
				AssertEquals(" FIELD(Depot/Terminal Code) DESCR(Invalid Transport Code/Removal Transport Code for Depot)", customsStatusFreeText.FreeText);
			});
		}

		public void TestLineCustomsStatusFreeTexts()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSRES_FTX_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			CombineAssertions("FTX", () =>
			{
				AssertNotNull(testHelper.LineCustomsStatusFreeTexts);
				AssertEquals(2, testHelper.LineCustomsStatusFreeTextsCount);
				var customsStatusFreeText = testHelper.LineCustomsStatusFreeTexts[0];
				AssertEquals("1", customsStatusFreeText.Line);
				AssertEquals("1129", customsStatusFreeText.Code);
				AssertEquals(" FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences VAT - Calculated = 644.00 and TaxAssessed = 616.00)", customsStatusFreeText.FreeText);
				customsStatusFreeText = testHelper.LineCustomsStatusFreeTexts[1];
				AssertEquals("2", customsStatusFreeText.Line);
				AssertEquals("1129", customsStatusFreeText.Code);
				AssertEquals(" FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences 1P1 - Calculated = 200.00 and TaxAssessed = null)", customsStatusFreeText.FreeText);
			});
		}

		public void TestSummaryCustomsStatusFreeTexts()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSRES_FTX_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			CombineAssertions("FTX", () =>
			{
				AssertNotNull(testHelper.SummaryCustomsStatusFreeTexts);
				AssertEquals(1, testHelper.SummaryCustomsStatusFreeTextsCount);
				var customsStatusFreeText = testHelper.SummaryCustomsStatusFreeTexts[0];
				AssertEquals("0", customsStatusFreeText.Line);
				AssertEquals("100", customsStatusFreeText.Code);
				AssertEquals("Samples to be declared according to the export price list of identic al goods/open market value.  VOC required to declare such value plus a PP req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read", customsStatusFreeText.FreeText);
			});
		}

		public void TestAssessmentDate()
		{
			var testInterchange = Factory.New<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSRES_GIS1_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			AssertEquals(new ZDateTime(2016, 05, 14), testHelper.AssessmentDate);
		}

		public void TestIsRejectionOfSubmissionWithinAllocatedTimeframe()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("1");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();
			var testInterchange = Factory.New<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSRES_GIS1_Message;
			var testHelper = CUSRESMessageHelper.New(testMessage);
			AssertEquals(false, testHelper.IsRejectionOfSubmissionWithinAllocatedTimeframe);
			testMessage.EM_MessageText = CUSRES_GIS6_Message;
			testHelper = CUSRESMessageHelper.New(testMessage);
			AssertEquals(false, testHelper.IsRejectionOfSubmissionWithinAllocatedTimeframe);
			testMessage.EM_MessageText = CUSRES_GIS26_Message;
			testHelper = CUSRESMessageHelper.New(testMessage);
			AssertEquals(false, testHelper.IsRejectionOfSubmissionWithinAllocatedTimeframe);
			testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JHB20160504000023::00001'DTM+132:20160504:102'DTM+9:20160506050220:202'TDT+20+QF234+4'LOC+22+JHB'LOC+14+A2'GIS+6:120:ZZZ'NAD+AG+00505655'NAD+MS+TST'RFF+AAS:08122222222'DTM+137:20160424:102'RFF+ACD:24'RFF+ADE:8120060127'ERP+1:0000'ERC+0000'FTX+AAO+++Message ignored as a duplicate submission within allocated timeframe'UNT+18+1'";
			testHelper = CUSRESMessageHelper.New(testMessage);
			AssertEquals(true, testHelper.IsRejectionOfSubmissionWithinAllocatedTimeframe);
			testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JHB20160504000023::00001'DTM+132:20160504:102'DTM+9:20160506050220:202'TDT+20+QF234+4'LOC+22+JHB'LOC+14+A2'GIS+6:120:ZZZ'NAD+AG+00505655'NAD+MS+TST'RFF+AAS:08122222222'DTM+137:20160424:102'RFF+ACD:24'RFF+ADE:8120060127'ERP+1:0000'ERC+0000'FTX+AAO+++  Message ignored as a duplicate submission within allocated timeframe    'UNT+18+1'";
			testHelper = CUSRESMessageHelper.New(testMessage);
			AssertEquals(true, testHelper.IsRejectionOfSubmissionWithinAllocatedTimeframe);
			testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JHB20160504000023::00001'DTM+132:20160504:102'DTM+9:20160506050220:202'TDT+20+QF234+4'LOC+22+JHB'LOC+14+A2'GIS+6:120:ZZZ'NAD+AG+00505655'NAD+MS+TST'RFF+AAS:08122222222'DTM+137:20160424:102'RFF+ACD:24'RFF+ADE:8120060127'ERP+1:0000'ERC+0000'FTX+AAO+++  Message ignored as a duplicate submiss:ion within allocated timeframe    'UNT+18+1'";
			testHelper = CUSRESMessageHelper.New(testMessage);
			AssertEquals(true, testHelper.IsRejectionOfSubmissionWithinAllocatedTimeframe);
		}

		public void TestProvisionalPaymentInfos()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			CUSRESMessageHelper testHelper = null;
			CombineAssertions("Message With No PP Additional Info", () =>
			{
				testMessage.EM_MessageText = CUSRES_GIS6_Message;
				testHelper = CUSRESMessageHelper.New(testMessage);
				AssertNotNull(testHelper.AdditionalProvisionalPaymentInfos);
				AssertEquals(0, testHelper.AdditionalProvisionalPaymentInfos.Count());
			});
			CombineAssertions("Single PP Info", () =>
			{
				testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA20160928002719:1'DTM+178:20160928:102'TDT+20+SA123+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+B6::ZZZ'GIS+48:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-76453215'DTM+137:20160925:102'RFF+ABT:JSA201609285000615'DTM+137:20160928:102'RFF+ACD:2726'RFF+AAV:100584509'ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100662286;!DutyType=PEN;!PPAmount=56.25;'TAX+3+CUS:107:ZZZ'MOA+161:151625'CNT+7:100.00'CNT+11:1'UNT+22+1'";
				testHelper = CUSRESMessageHelper.New(testMessage);
				AssertNotNull(testHelper.AdditionalProvisionalPaymentInfos);
				AssertEquals(1, testHelper.AdditionalProvisionalPaymentInfos.Count());
				var testPick = testHelper.AdditionalProvisionalPaymentInfos.FirstOrDefault();
				AssertEquals(true, testPick.IsValid);
				AssertEquals(false, testPick.IsHeaderLevelInfo);
				AssertEquals("100662286", testPick.PPNo);
				AssertEquals("PEN", testPick.DutyType);
				AssertEquals(56.25m, testPick.PPAmount);
				AssertEquals(ZDateTime.Empty, testPick.ExpiryDate);
				AssertEquals("2", testPick.EntryLineNumber);
			});
			CombineAssertions("Multiple PP Info in a single SG41", () =>
			{
				testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA20160928002719:1'DTM+178:20160928:102'TDT+20+SA123+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+B6::ZZZ'GIS+48:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-76453215'DTM+137:20160925:102'RFF+ABT:JSA201609285000615'DTM+137:20160928:102'RFF+ACD:2726'RFF+AAV:100584509'ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100662286;!DutyType=PEN;!PPAmount=56.25;'FTX+AAO+++!PPNo=100662287;!DutyType=FOR;!PPAmount=57.25;!Expiry Date=2016/10/01;'TAX+3+CUS:107:ZZZ'MOA+161:151625'CNT+7:100.00'CNT+11:1'UNT+22+1'";
				testHelper = CUSRESMessageHelper.New(testMessage);
				AssertNotNull(testHelper.AdditionalProvisionalPaymentInfos);
				AssertEquals(2, testHelper.AdditionalProvisionalPaymentInfos.Count());
				var testPick = testHelper.AdditionalProvisionalPaymentInfos.ElementAt(0);
				AssertEquals(true, testPick.IsValid);
				AssertEquals(false, testPick.IsHeaderLevelInfo);
				AssertEquals("2", testPick.EntryLineNumber);
				AssertEquals("100662286", testPick.PPNo);
				AssertEquals("PEN", testPick.DutyType);
				AssertEquals(56.25m, testPick.PPAmount);
				AssertEquals(ZDateTime.Empty, testPick.ExpiryDate);
				testPick = testHelper.AdditionalProvisionalPaymentInfos.ElementAt(1);
				AssertEquals(true, testPick.IsValid);
				AssertEquals(false, testPick.IsHeaderLevelInfo);
				AssertEquals("2", testPick.EntryLineNumber);
				AssertEquals("100662287", testPick.PPNo);
				AssertEquals("FOR", testPick.DutyType);
				AssertEquals(57.25m, testPick.PPAmount);
				AssertEquals(new ZDateTime(2016, 10, 01), testPick.ExpiryDate);
			});
			CombineAssertions("Multiple PP Info in a single SG41 and in different SG41", () =>
			{
				testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA20160928002719:1'DTM+178:20160928:102'TDT+20+SA123+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+B6::ZZZ'GIS+48:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-76453215'DTM+137:20160925:102'RFF+ABT:JSA201609285000615'DTM+137:20160928:102'RFF+ACD:2726'RFF+AAV:100584509'ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100662286;!DutyType=PEN;!PPAmount=56.25;'FTX+AAO+++!PPNo=100662287;!DutyType=FOR;!PPAmount=57.25;!Expiry Date=2016/10/01;'ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100662285;!DutyType=PEN;!PPAmount=58.25;'TAX+3+CUS:107:ZZZ'MOA+161:151625'CNT+7:100.00'CNT+11:1'UNT+26+1'";
				testHelper = CUSRESMessageHelper.New(testMessage);
				AssertNotNull(testHelper.AdditionalProvisionalPaymentInfos);
				AssertEquals(3, testHelper.AdditionalProvisionalPaymentInfos.Count());
				var testPick = testHelper.AdditionalProvisionalPaymentInfos.ElementAt(0);
				AssertEquals(true, testPick.IsValid);
				AssertEquals(false, testPick.IsHeaderLevelInfo);
				AssertEquals("2", testPick.EntryLineNumber);
				AssertEquals("100662286", testPick.PPNo);
				AssertEquals("PEN", testPick.DutyType);
				AssertEquals(56.25m, testPick.PPAmount);
				AssertEquals(ZDateTime.Empty, testPick.ExpiryDate);
				testPick = testHelper.AdditionalProvisionalPaymentInfos.ElementAt(1);
				AssertEquals(true, testPick.IsValid);
				AssertEquals(false, testPick.IsHeaderLevelInfo);
				AssertEquals("2", testPick.EntryLineNumber);
				AssertEquals("100662287", testPick.PPNo);
				AssertEquals("FOR", testPick.DutyType);
				AssertEquals(57.25m, testPick.PPAmount);
				AssertEquals(new ZDateTime(2016, 10, 01), testPick.ExpiryDate);
				testPick = testHelper.AdditionalProvisionalPaymentInfos.ElementAt(2);
				AssertEquals(true, testPick.IsValid);
				AssertEquals(true, testPick.IsHeaderLevelInfo);
				AssertEquals("1", testPick.EntryLineNumber);
				AssertEquals("100662285", testPick.PPNo);
				AssertEquals("PEN", testPick.DutyType);
				AssertEquals(58.25m, testPick.PPAmount);
				AssertEquals(ZDateTime.Empty, testPick.ExpiryDate);
			});
			CombineAssertions("Multiple PP Info in a single SG41 and in different SG41, some invalid", () =>
			{
				testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA20160928002719:1'DTM+178:20160928:102'TDT+20+SA123+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+B6::ZZZ'GIS+48:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-76453215'DTM+137:20160925:102'RFF+ABT:JSA201609285000615'DTM+137:20160928:102'RFF+ACD:2726'RFF+AAV:100584509'ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100662286;!DutyTpe=PEN;!PPAmount=56.25;'FTX+AAO+++!PPNo=100662287;!DutyType=FOR;!PPAmount=57.25;!Expiry Date=2016/10/01;'ERP+2:3'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100662285;!DutyType=PEN;!PPAmount=58.25;'TAX+3+CUS:107:ZZZ'MOA+161:151625'CNT+7:100.00'CNT+11:1'UNT+26+1'";
				testHelper = CUSRESMessageHelper.New(testMessage);
				AssertNotNull(testHelper.AdditionalProvisionalPaymentInfos);
				AssertEquals(2, testHelper.AdditionalProvisionalPaymentInfos.Count());
				var testPick = testHelper.AdditionalProvisionalPaymentInfos.ElementAt(0);
				AssertEquals(true, testPick.IsValid);
				AssertEquals(false, testPick.IsHeaderLevelInfo);
				AssertEquals("2", testPick.EntryLineNumber);
				AssertEquals("100662287", testPick.PPNo);
				AssertEquals("FOR", testPick.DutyType);
				AssertEquals(57.25m, testPick.PPAmount);
				AssertEquals(new ZDateTime(2016, 10, 01), testPick.ExpiryDate);
				testPick = testHelper.AdditionalProvisionalPaymentInfos.ElementAt(1);
				AssertEquals(true, testPick.IsValid);
				AssertEquals(false, testPick.IsHeaderLevelInfo);
				AssertEquals("3", testPick.EntryLineNumber);
				AssertEquals("100662285", testPick.PPNo);
				AssertEquals("PEN", testPick.DutyType);
				AssertEquals(58.25m, testPick.PPAmount);
				AssertEquals(ZDateTime.Empty, testPick.ExpiryDate);
			});
			CombineAssertions("Multiple PP Info in a single SG41 and in different SG41, some invalid, with some cross fields", () =>
			{
				testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA20160928002719:1'DTM+178:20160928:102'TDT+20+SA123+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+B6::ZZZ'GIS+48:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-76453215'DTM+137:20160925:102'RFF+ABT:JSA201609285000615'DTM+137:20160928:102'RFF+ACD:2726'RFF+AAV:100584509'ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100662286;!DutyTpe=PEN;!PPAmount=56.25;'FTX+AAO+++!PPNo=100662287;!Duty:Type=FOR;!PPAmount=57.25;!Expiry: Date=2016/10/01;'ERP+2:3'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100662285;!DutyType=PEN;!PPAmount=58.25;'TAX+3+CUS:107:ZZZ'MOA+161:151625'CNT+7:100.00'CNT+11:1'UNT+26+1'";
				testHelper = CUSRESMessageHelper.New(testMessage);
				AssertNotNull(testHelper.AdditionalProvisionalPaymentInfos);
				AssertEquals(2, testHelper.AdditionalProvisionalPaymentInfos.Count());
				var testPick = testHelper.AdditionalProvisionalPaymentInfos.ElementAt(0);
				AssertEquals(true, testPick.IsValid);
				AssertEquals(false, testPick.IsHeaderLevelInfo);
				AssertEquals("2", testPick.EntryLineNumber);
				AssertEquals("100662287", testPick.PPNo);
				AssertEquals("FOR", testPick.DutyType);
				AssertEquals(57.25m, testPick.PPAmount);
				AssertEquals(new ZDateTime(2016, 10, 01), testPick.ExpiryDate);
				testPick = testHelper.AdditionalProvisionalPaymentInfos.ElementAt(1);
				AssertEquals(true, testPick.IsValid);
				AssertEquals(false, testPick.IsHeaderLevelInfo);
				AssertEquals("3", testPick.EntryLineNumber);
				AssertEquals("100662285", testPick.PPNo);
				AssertEquals("PEN", testPick.DutyType);
				AssertEquals(58.25m, testPick.PPAmount);
				AssertEquals(ZDateTime.Empty, testPick.ExpiryDate);
			});
		}

		public void TestContainers()
		{
			var interchange = Factory.NewWithValidTestData<ZACInterchange>();
			var message = (ZAMessage)interchange.ContainedMessages.AddNew(typeof(ZAMessage));
			message.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00281124JSA20161031015941:0'DTM+132:20161031:102'DTM+202:20161031:102'TDT+20+MP83+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+62::ZZZ'GIS+1:120:ZZZ:N'NAD+AG+00281124'RFF+BH:00281124AMS792936'DTM+137:20161030:102'RFF+AAS:074-50889145'DTM+137:20161030:102'RFF+ABT:JSA201610315138779'DTM+137:20161031:102'RFF+ACD:01015941'TAX+3+CUS:107:ZZZ'MOA+161:3775'CNT+7:5.20'CNT+11:1'UNT+21+1'";
			var helper = CUSRESMessageHelper.New(message);
			AssertEquals("Containers", ZString.Empty, helper.Containers);
			message = (ZAMessage)interchange.ContainedMessages.AddNew(typeof(ZAMessage));
			message.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00281124JSA20161031015941:0'DTM+132:20161031:102'DTM+202:20161031:102'TDT+20+MP83+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+62::ZZZ'GIS+1:120:ZZZ:N'EQD+CN+SUDU1769365'NAD+AG+00281124'RFF+BH:00281124AMS792936'DTM+137:20161030:102'RFF+AAS:074-50889145'DTM+137:20161030:102'RFF+ABT:JSA201610315138779'DTM+137:20161031:102'RFF+ACD:01015941'TAX+3+CUS:107:ZZZ'MOA+161:3775'CNT+7:5.20'CNT+11:1'UNT+21+1'";
			helper = CUSRESMessageHelper.New(message);
			AssertEquals("Containers", "SUDU1769365", helper.Containers);
			message = (ZAMessage)interchange.ContainedMessages.AddNew(typeof(ZAMessage));
			message.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00281124JSA20161031015941:0'DTM+132:20161031:102'DTM+202:20161031:102'TDT+20+MP83+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+62::ZZZ'GIS+1:120:ZZZ:N'EQD+CN+SUDU1769365'EQD+CN+SUDU1769366'NAD+AG+00281124'RFF+BH:00281124AMS792936'DTM+137:20161030:102'RFF+AAS:074-50889145'DTM+137:20161030:102'RFF+ABT:JSA201610315138779'DTM+137:20161031:102'RFF+ACD:01015941'TAX+3+CUS:107:ZZZ'MOA+161:3775'CNT+7:5.20'CNT+11:1'UNT+21+1'";
			helper = CUSRESMessageHelper.New(message);
			AssertEquals("Containers", "SUDU1769365,SUDU1769366", helper.Containers);
		}

		public void TestIsGateInOutCUSRESMessage()
		{
			var interchange = Factory.NewWithValidTestData<ZACInterchange>();
			interchange.EI_HeaderText = @"UNB+UNOB:4+SARSCART+ZS::WTGTESTDEPOTSEA1:WTGAS2+20180511:0653+150++CUSRES-GOVGIO'";
			var message = (ZAMessage)interchange.ContainedMessages.AddNew(typeof(ZAMessage));
			message.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00281124JSA20161031015941:0'DTM+132:20161031:102'DTM+202:20161031:102'TDT+20+MP83+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+62::ZZZ'GIS+1:120:ZZZ:N'NAD+AG+00281124'RFF+BH:00281124AMS792936'DTM+137:20161030:102'RFF+AAS:074-50889145'DTM+137:20161030:102'RFF+ABT:JSA201610315138779'DTM+137:20161031:102'RFF+ACD:01015941'TAX+3+CUS:107:ZZZ'MOA+161:3775'CNT+7:5.20'CNT+11:1'UNT+21+1'";
			var helper = CUSRESMessageHelper.New(message);
			Assert(helper.IsGateInOutCUSRESMessage);
			interchange.EI_HeaderText = @"UNB+UNOB:4+SARSCART+ZS::WTGTESTDEPOTSEA1:WTGAS2+20180511:0653+150++CUSRES-GOVGIO'";
			Assert(helper.IsGateInOutCUSRESMessage);
			interchange.EI_HeaderText = @"UNB+UNOB:4+SARSCART+ZS::WTGTESTDEPOTSEA1:WTGAS2+20180511:0653+150++CUSRES-COSCTO'";
			Assert(!helper.IsGateInOutCUSRESMessage);
		}

		public void TestShouldDiscardMessage()
		{
			var agentCode = "12345678";
			var headerText = "UNB+UNOB:4+SARSDECT+{0}::AAAAAAAAAAAAAABB:CORAS2+20160802:0844+1299++CUSRES+++GWWTGTEST+1'";
			var messageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00281124JSA20161031015941:0'DTM+132:20161031:102'DTM+202:20161031:102'TDT+20+MP83+4+++++:::'LOC+22+JSA::ZZZ'LOC+14+62::ZZZ'GIS+1:120:ZZZ:N'NAD+AG+{0}'RFF+BH:00281124AMS792936'DTM+137:20161030:102'RFF+AAS:074-50889145'DTM+137:20161030:102'RFF+ABT:JSA201610315138779'DTM+137:20161031:102'RFF+ACD:01015941'TAX+3+CUS:107:ZZZ'MOA+161:3775'CNT+7:5.20'CNT+11:1'UNT+21+1'";
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_HeaderText = string.Format(CultureInfo.InvariantCulture, headerText, agentCode);
			var message = (CUSRESEDIMessage)interchange.ContainedMessages.AddNew(typeof(CUSRESEDIMessage));
			message.EM_MessageText = string.Format(CultureInfo.InvariantCulture, messageText, agentCode);
			var helper = CUSRESMessageHelper.New(message);
			AssertEquals(false, helper.ShouldDiscardMessage);
			agentCode = "87654321";
			message.EM_MessageText = string.Format(CultureInfo.InvariantCulture, messageText, agentCode);
			helper = CUSRESMessageHelper.New(message);
			AssertEquals(true, helper.ShouldDiscardMessage);
			agentCode = "00000000";
			message.EM_MessageText = string.Format(CultureInfo.InvariantCulture, messageText, agentCode);
			helper = CUSRESMessageHelper.New(message);
			AssertEquals(false, helper.ShouldDiscardMessage);
			agentCode = "";
			message.EM_MessageText = string.Format(CultureInfo.InvariantCulture, messageText, agentCode);
			helper = CUSRESMessageHelper.New(message);
			AssertEquals(false, helper.ShouldDiscardMessage);
			message.EM_MessageText = messageText.Replace("NAD+AG+{0}'", "");
			helper = CUSRESMessageHelper.New(message);
			AssertEquals(false, helper.ShouldDiscardMessage);
		}

		public void TestProcessOtherGovernmentAgencies()
		{
			var headerText = "UNB+UNOB:4+SARSDECT+00030004B2B::XXXXXXXXXXXXXXXX:SRSAS2+20230720:1020+541495++CUSRES+++SARSTEST+1'";
			var messageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00030004MSB20230718999423:0'DTM+132:20200602:102'TDT+20+202A+2+++++:::OCENVRIS8 MAERSK LUZ'LOC+22+MSB::ZZZ'" +
				"LOC+14+RB::ZZZ'GIS+74:120:ZZZ:N'NAD+AG+00030004'RFF+BH:206245667937 7938'DTM+137:20200507:102'RFF+AAS: SAFM910721020'DTM+137:20200507:102'RFF+ABT:MSB202307185000219'" +
				"DTM+137:20230718:102'RFF+ACD:IL2023040428'RFF+AAV:151516651'" +
				"ERP+6:0'ERC+3::ZZZ'FTX+AAO+++DETAIN FOR PLANT INSPECTION'" +
				"ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Please Declare the correct amount'" +
				"{0}" +
				"TAX+3+CUS:107:ZZZ'MOA+161:3000'CNT+7:14473.00'CNT+11:2'UNT+30+1'";
			var footerText = "UNZ+1+541495'";
			var ogaSection = "ERP+6:0'ERC+0000::ZZZ'FTX+AAO+++OGA Case=151516758'";

			var interchange = Factory.NewWithValidTestData<ZACInterchange>();
			interchange.EI_HeaderText = headerText;
			interchange.EI_FooterText = footerText;
			var message = (ZAMessage)interchange.ContainedMessages.AddNew(typeof(ZAMessage));
			message.EM_MessageText = string.Format(messageText, ogaSection);
			var helper = CUSRESMessageHelper.New(message);
			AssertContainsExactElementsInAnyOrder(new[] { "151516758" }, helper.OtherGovernmentAgencies);

			ogaSection += "ERP+6:0'ERC+0000::ZZZ'FTX+AAO+++OGA Case=123456789'";
			message.EM_MessageText = string.Format(messageText, ogaSection);
			helper = CUSRESMessageHelper.New(message);
			AssertContainsExactElementsInAnyOrder(new[] { "151516758", "123456789" }, helper.OtherGovernmentAgencies);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<ZAMessage>();
			testMessage.EM_MessageText = CUSRES_GIS1_Message;
			return CUSRESMessageHelper.New(testMessage);
		}

		const string CUSRES_GIS1_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655CLP20160514000245:0'DTM+178:20160511:102'TDT+20+QF987+4+++++::: 'LOC+22+CLP::ZZZ'LOC+14+XW::ZZZ'GIS+1:120:ZZZ:Y'NAD+AG+00505655'RFF+BH:0003264'RFF+AAS:081-99876545'DTM+137:20160305:102'RFF+ABT:CLP201605145000001'DTM+137:20160514:102'RFF+UCN:6ZA01702826INV158'RFF+ACD:246'RFF+AFB:CARN0111123J'TAX+3+CUS:107:ZZZ'MOA+161:490688'CNT+7:226.79'CNT+11:5'UNT+21+1'";
		const string CUSRES_GIS6_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655BBR20160513000034::00001'DTM+9:20160513143147:202'TDT+20'LOC+22+BBR'GIS+6:120:ZZZ'NAD+AG+00505655'NAD+MS+TST'RFF+BH:00505655'RFF+AAS:HENRYMASTER1'RFF+ACD:35'ERP+1:0000'ERC+0000'FTX+AAO+++Line number may not be 0'UNT+15+1'";
		const string CUSRES_GIS26_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:152'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
		const string CUSDEC_BGM9_Message = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'RFF+VA:123321'NAD+AG+00505655'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
		const string CUSDEC_FTX_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655BBR20160708000620:0'LOC+22+BBR::ZZZ'GIS+6:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:AGTCHENRYROAD6'RFF+ACD:621'ERP+2:1'ERC+2108::ZZZ'FTX+AAO+++ FIELD(Warehousing details) DESCR(No matching previous declaration cou:ld be found for MRN JSA201501011234567)'ERP+2:2'ERC+9272::ZZZ'ERC+9273::ZZZ'FTX+AAO+++ FIELD(Warehousing B/E line no) DESCR(This field is required)'FTX+AAO+++ FIELD(Warehousing B/E line no) DESCR(This field is required)'TAX+3+CUS:107:ZZZ'MOA+161:0'CNT+7:8.00'CNT+11:1'UNT+18+1'";
		const string CUSRES_FTX_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA20160822001311:0'DTM+178:20160822:102'TDT+20+SA345+0+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A8::ZZZ'GIS+6:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-12345675'DTM+137:20160822:102'RFF+ACD:1312'ERP+1:0'ERC+8463::ZZZ'FTX+AAO+++ FIELD(Depot/Terminal Code) DESCR(Field must be empty)'ERP+1:0'ERC+1173::ZZZ'FTX+AAO+++ FIELD(Depot/Terminal Code) DESCR(Invalid Transport Code/Removal Trans:port Code for Depot)'ERP+2:1'ERC+1129::ZZZ'FTX+AAO+++ FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences VAT - Calculated = 6:44.00 and TaxAssessed = 616.00)'ERP+2:2'ERC+1129::ZZZ'FTX+AAO+++ FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences 1P1 - Calculated = 2:00.00 and TaxAssessed = null)'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:0'CNT+7:10.00'CNT+11:1'UNT+28+1'";
	}
}
