using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.Customs.ZA.Business.MessageManagers.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;
using InboundInterchangeProcessor = Enterprise.Customs.ZA.Business.BatchProcessor.InboundInterchangeProcessor;

namespace Enterprise.Customs.ZA.Business.Testing
{
	internal class InboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestGenerateMessageFromInterchange_EXPORT()
		{
			var expectedText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00505655CLP20160514000245:0'
DTM+178:20160511:102'
TDT+20+QF987+4+++++::: '
LOC+22+CLP::ZZZ'
LOC+14+XW::ZZZ'
GIS+1:120:ZZZ:Y'
NAD+AG+00505655'
RFF+BH:0003264'
RFF+AAS:081-99876545'
DTM+137:20160305:102'
RFF+ABT:CLP201605145000001'
DTM+137:20160514:102'
RFF+UCN:6ZA01702826INV158'
RFF+ACD:246'
TAX+3+CUS:107:ZZZ'
MOA+161:490688'
CNT+7:226.79'
CNT+11:5'
UNT+20+1'";

			var messageText = @"UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160514:1405+247++EXPORT+++GWWTGTEST+1'
" + expectedText + "UNZ+1+247'";
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "ZAC";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = messageText;
			Factory.Save();

			CombineAssertions(() =>
			{
				var log = GetNewLoggerForTesting();
				new InboundInterchangeProcessor(log).ExecuteBatch();
				interchange.Reload();

				AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

				var createdMessage = interchange.ContainedMessages[0];
				AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
				AssertEquals("EM_ApplicationCode", "ZAC", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "RES", createdMessage.EM_MessageType);
				AssertEquals("EM_MessageNum", "1", createdMessage.EM_MessageNum);
				AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
				AssertEquals("EM_MessageText", expectedText.Replace("\r\n", ""), createdMessage.EM_MessageText);
				AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
				AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
			});
		}

		public void TestGenerateMessageFromInterchange_CUSCAR_AND()
		{
			var expectedText = @"UNH+6266+CUSCAR:D:16A:UN:RCG001'
BGM+85:::AND+669E4647053C4C18+9'
DTM+136:20180120:102'
DTM+137:20180219:102'
RFF+LO:OGM0000194'
NAD+RL+MSC:172:ZZZ'
NAD+FZ+00101566::ZZZ'
NAD+MS+00505655TST::ZZZ'
TDT+20+S123+4++MSC:172:20+++3FRF8:103:::ZA'
LOC+60+ZADUR:139:6'
DTM+132:20180216:102'
EQD+CN+MSCU1234566+2000:102:5++3+5'
MEA+AAE+VGM+KGM:100'
SEL+SEAL1+TO'
SEL+SEAL2+TO'
CNT+16:1'
CNI+1+MSCU09874ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ:BOL:::20180216'
CNT+16:1'
RFF+BM:HB2'
LOC+8+ZADUR:139:6'
LOC+9+DEHAM:139:6'
NAD+CN++SHELDONS IMPORTER ZA WITH A LONG NA:198 WEST STREET:JOHANNESBURG::1619'
NAD+CZ++TIM EXPORT CO:1 BAYERN STREET:HAMBURG:HH:22765'
GID+1+12:NO'
FTX+AAA+++DESC2'
MEA+AAE+AAW+MTQ:0'
MEA+AAE+AAB+KGM:300'
SGP+MSCU1234566+12'
PCI+24+MAND2'
CST++COMM2'
UNT+31+6266'";

			var messageText = @"UNB+UNOB:4+SARSDECT+00626166COR::BBAAAAAAAAAAAAAA:CORAS2+20160331:1530+1983++CUSCAR+++UTITEST2+1'
" + expectedText + "UNZ+1+1983'";
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "ZAC";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = messageText;
			Factory.Save();

			var log = GetNewLoggerForTesting();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();

			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "ZAC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "CAR", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageNum", "6266", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", expectedText.Replace("\r\n", ""), createdMessage.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", string.Empty, createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateMessageFromInterchange_CUSRES()
		{
			var expectedText = @"UNH+10+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166JSA20160331008480:0'
DTM+132:20160431:102'
DTM+202:20160431:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+1:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA20xx03315000938'
DTM+137:20160401:102'
RFF+ACD:201'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";
			var messageText = @"UNB+UNOB:4+SARSDECT+00626166COR::BBAAAAAAAAAAAAAA:CORAS2+20160331:1530+1983++CUSRES+++UTITEST2+1'
" + expectedText + "UNZ+1+1983'";
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "ZAC";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = messageText;
			Factory.Save();

			var log = GetNewLoggerForTesting();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();

			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "ZAC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "RES", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageNum", "10", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", expectedText.Replace("\r\n", ""), createdMessage.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateMessageFromInterchange_CUSRES_COSTCO()
		{
			var headerText = @"UNB+UNOB:4+SARSCART+ZS::WTGTESTDEPOTSEA1:WTGAS2+20180426:1421+13++CUSRES-COSTCO++++'";
			var footerText = @"UNZ+1+13'";
			var bodyText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+963+BA6C5224C92C42798F0810154F91D1EF'
DTM+132:20180425:102'
TDT+20+V0987+1'
LOC+22+DUR::ZZZ'
GIS+6:120:ZZZ'
NAD+AG+00000000'
RFF+BH:HOUSEDORSEA'
RFF+AAS:MASTERDORSEA'
ERP+6:0'
ERC+0001::ZZZ'
FTX+AAO+++BLL DESCRIPTION OF GOODS Field is required: : : '
CNT+11:10'
UNT+14+1'";

			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "ZAC";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_HeaderText = headerText;
			interchange.EI_BodyText = bodyText;
			interchange.EI_FooterText = footerText;
			Factory.Save();

			var log = GetNewLoggerForTesting();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();

			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "ZAC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "RES", createdMessage.EM_MessageType);
			AssertMultilineASCIIEquals("EM_MessageText", bodyText.Replace("\r\n", ""), createdMessage.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateMessageFromInterchange_CUSRES_CALINF()
		{
			var headerText = @"UNB+UNOB:4+SARSCART+ZS::WTGTESTDEPOTSEA1:WTGAS2+20180426:1421+13++CUSRES-CALINF++++'";
			var footerText = @"UNZ+1+13'";
			var bodyText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+963+BA6C5224C92C42798F0810154F91D1EF'
DTM+132:20180425:102'
TDT+20+V0987+1'
LOC+22+DUR::ZZZ'
GIS+6:120:ZZZ'
NAD+AG+00000000'
RFF+BH:HOUSEDORSEA'
RFF+AAS:MASTERDORSEA'
ERP+6:0'
ERC+0001::ZZZ'
FTX+AAO+++BLL DESCRIPTION OF GOODS Field is required: : : '
CNT+11:10'
UNT+14+1'";

			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "ZAC";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_HeaderText = headerText;
			interchange.EI_BodyText = bodyText;
			interchange.EI_FooterText = footerText;
			Factory.Save();

			var log = GetNewLoggerForTesting();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();

			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "ZAC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "RES", createdMessage.EM_MessageType);
			AssertMultilineASCIIEquals("EM_MessageText", bodyText.Replace("\r\n", ""), createdMessage.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateMessageFromInterchange_CUSRES_GOVGIO()
		{
			AssertGenerateMessageFromInterchangeForGateInOut(SARSEDIMessage.MessageTypeNames.CUSRES_GOVGIO);
		}

		public void TestGenerateMessageFromInterchange_CUSRES_GIO()
		{
			AssertGenerateMessageFromInterchangeForGateInOut(SARSEDIMessage.MessageTypeNames.CUSRES_GIO);
		}

		public void TestGenerateMessageFromInterchange_CONTROL()
		{
			CombineAssertions(() =>
			{
				var expectedText = @"UNH+1+CONTRL:D:3:UN:CONTRL'
UCI+00000000027997+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSREQT+7'
UCM+00000000065627+REQDOC:D:99B:UN:ZZZ01+7'
UNT+4+1'
";
				var messageText = @"UNB+UNOB:4+SARSREQT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160217:0633+39++CONTRL+++GWWTGTEST+1'
" + expectedText + "UNZ+1+39'";
				var interchange = Factory.New<ZACInterchange>();
				interchange.EI_From = "SARS";
				interchange.EI_To = "TEST";
				interchange.EI_ApplicationCode = "ZAC";
				interchange.EI_InterchangeType = "ZAC";
				interchange.EI_InterchangeNum = "00000000000000000031";
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_IsActive = true;
				interchange.EI_BodyText = messageText;
				Factory.Save();

				var log = GetNewLoggerForTesting();
				new InboundInterchangeProcessor(log).ExecuteBatch();
				interchange.Reload();

				AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

				var createdMessage = interchange.ContainedMessages[0];
				AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
				AssertEquals("EM_ApplicationCode", "ZAC", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "CTL", createdMessage.EM_MessageType);
				AssertEquals("EM_MessageNum", "1", createdMessage.EM_MessageNum);
				AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
				AssertEquals("EM_MessageText", expectedText.Replace("\r\n", ""), createdMessage.EM_MessageText);
				AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
				AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
			});
		}

		public void TestEntryStatusAndMessageStatus()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("6");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ProcedureCodes._11;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV001";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = ProcedureCodes._00 + "00";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var entry = declaration.ActiveEntryHeaders[0];
			entry.CH_BGMReference = "00505655KFN20160801000996";

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManager(objectParent, notification);
			AssertEquals("entry.Messages.Count", 0, entry.Messages.Count);
			AssertEquals("entry.CH_Status", "", entry.CH_Status);
			AssertEquals("entry.CH_EntryStatus", "", entry.CH_EntryStatus);
			Factory.Save();
			messageManager.SendMessages();
			AssertEquals("entry.Messages.Count", 1, entry.Messages.Count);
			var originalMessage = (CUSDECEDIMessage)entry.Messages[0];
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
			AssertEquals("entry.CH_EntryStatus", "", entry.CH_EntryStatus);

			var controlInterchange1 = Factory.New<ZACInterchange>();
			controlInterchange1.EI_From = "SARS";
			controlInterchange1.EI_To = "TEST";
			controlInterchange1.EI_InterchangeType = "ZAC";
			controlInterchange1.EI_InterchangeNum = "00000000000000000031";
			controlInterchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			controlInterchange1.EI_Status = EDIInterchange.Status.Queued;
			controlInterchange1.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160801:0655+1175++CONTRL+++GWWTGTEST+1'";
			controlInterchange1.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+460+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+997+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			controlInterchange1.EI_FooterText = "UNZ +1+1175'";
			originalMessage.EM_MessageNum = "997";
			Factory.Save();
			var logger = GetNewLoggerForTesting();
			var processor = new InboundInterchangeProcessor(logger);
			processor.ExecuteBatch();
			controlInterchange1.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, controlInterchange1.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, controlInterchange1.ContainedMessages.Count);
			var controlMessage1 = controlInterchange1.ContainedMessages[0];
			var messageProcessor = new ZACIncomingMessageProcessor();
			messageProcessor.Logger = new LoggingInformation();
			messageProcessor.ExecuteBatch();
			entry.Reload();
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entry.PK);
			query.ReLoadExistingRows = true;
			Factory.Load<EDIMessage>(query);
			entry.Messages.Load();
			AssertEquals("entry.Messages.Count", 2, entry.Messages.Count);
			AssertCollectionContains(controlMessage1, entry.Messages);
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
			AssertEquals("entry.CH_EntryStatus", "", entry.CH_EntryStatus);
			Factory.Save();

			var cusresInterchange1 = Factory.New<ZACInterchange>();
			cusresInterchange1.EI_From = "SARS";
			cusresInterchange1.EI_To = "TEST";
			cusresInterchange1.EI_InterchangeType = "ZAC";
			cusresInterchange1.EI_InterchangeNum = "00000000000000000032";
			cusresInterchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			cusresInterchange1.EI_Status = EDIInterchange.Status.Queued;
			cusresInterchange1.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160801:0655+1176++EXPORT+++GWWTGTEST+1'";
			cusresInterchange1.EI_BodyText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655KFN20160801000996:0'DTM+178:20160801:102'TDT+20+DE1455+3+++++:::DE1455'LOC+22+KFN::ZZZ'GIS+6:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:BOL1608011455'DTM+137:20160801:102'RFF+UCN:6ZA011432336ZA01143233000000000004'RFF+ACD:997'ERP+2:1'ERC+2108::ZZZ'FTX+AAO+++ FIELD(Warehousing details) DESCR(No matching previous declaration cou:ld be found for MRN KFN201606025000111)'TAX+3+CUS:107:ZZZ'MOA+161:0'CNT+7:100.00'CNT+11:10'UNT+19+1'";
			cusresInterchange1.EI_FooterText = "UNZ+1+1176'";
			Factory.Save();
			processor.ExecuteBatch();
			cusresInterchange1.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, cusresInterchange1.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, cusresInterchange1.ContainedMessages.Count);
			var cusresMessage1 = cusresInterchange1.ContainedMessages[0];
			messageProcessor.ExecuteBatch();
			entry.Reload();
			Factory.Load<EDIMessage>(query);
			entry.Messages.Load();
			AssertEquals("entry.Messages.Count", 3, entry.Messages.Count);
			AssertCollectionContains(cusresMessage1, entry.Messages);
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
			AssertEquals("entry.CH_EntryStatus", "6", entry.CH_EntryStatus);

			objectParent.SendingObjectsCollection[0].MessageType = MessageSubTypeCodes.Codes.Change;
			Factory.Save();
			messageManager.SendMessages();
			AssertEquals("entry.Messages.Count", 4, entry.Messages.Count);
			var changeMessage = (CUSDECEDIMessage)entry.Messages[3];
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
			AssertEquals("entry.CH_EntryStatus", "", entry.CH_EntryStatus);

			var controlInterchange2 = Factory.New<ZACInterchange>();
			controlInterchange2.EI_From = "SARS";
			controlInterchange2.EI_To = "TEST";
			controlInterchange2.EI_InterchangeType = "ZAC";
			controlInterchange2.EI_InterchangeNum = "00000000000000000033";
			controlInterchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			controlInterchange2.EI_Status = EDIInterchange.Status.Queued;
			controlInterchange2.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160802:0118+1214++CONTRL+++GWWTGTEST+1'";
			controlInterchange2.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+482+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+1026+CUSDEC:D:96B:UN:ZZZ01+4'UNT+4+1'";
			controlInterchange2.EI_FooterText = "UNZ+1+1214'";
			changeMessage.EM_MessageNum = "1026";
			Factory.Save();
			processor.ExecuteBatch();
			controlInterchange2.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, controlInterchange2.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, controlInterchange2.ContainedMessages.Count);

			var controlMessage2 = controlInterchange2.ContainedMessages[0];
			messageProcessor.ExecuteBatch();
			entry.Reload();
			Factory.Load<EDIMessage>(query);
			entry.Messages.Load();
			AssertEquals("entry.Messages.Count", 5, entry.Messages.Count);
			AssertCollectionContains(controlMessage2, entry.Messages);
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Error, entry.CH_Status);
			AssertEquals("entry.CH_EntryStatus", "6", entry.CH_EntryStatus);

			Factory.Save();
			messageManager.SendMessages();
			AssertEquals("entry.Messages.Count", 6, entry.Messages.Count);
			changeMessage = (CUSDECEDIMessage)entry.Messages[5];
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
			AssertEquals("entry.CH_EntryStatus", "", entry.CH_EntryStatus);

			var controlInterchange3 = Factory.New<ZACInterchange>();
			controlInterchange3.EI_From = "SARS";
			controlInterchange3.EI_To = "TEST";
			controlInterchange3.EI_InterchangeType = "ZAC";
			controlInterchange3.EI_InterchangeNum = "00000000000000000034";
			controlInterchange3.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			controlInterchange3.EI_Status = EDIInterchange.Status.Queued;
			controlInterchange3.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160802:0118+1215++CONTRL+++GWWTGTEST+1'";
			controlInterchange3.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+482+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+1027+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			controlInterchange3.EI_FooterText = "UNZ+1+1215'";
			changeMessage.EM_MessageNum = "1027";
			Factory.Save();
			processor.ExecuteBatch();
			controlInterchange3.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, controlInterchange3.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, controlInterchange3.ContainedMessages.Count);

			var controlMessage3 = controlInterchange3.ContainedMessages[0];
			messageProcessor.ExecuteBatch();
			entry.Reload();
			Factory.Load<EDIMessage>(query);
			entry.Messages.Load();
			AssertEquals("entry.Messages.Count", 7, entry.Messages.Count);
			AssertCollectionContains(controlMessage3, entry.Messages);
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
			AssertEquals("entry.CH_EntryStatus", "", entry.CH_EntryStatus);

			var cusresInterchange2 = Factory.New<ZACInterchange>();
			cusresInterchange2.EI_From = "SARS";
			cusresInterchange2.EI_To = "TEST";
			cusresInterchange2.EI_InterchangeType = "ZAC";
			cusresInterchange2.EI_InterchangeNum = "00000000000000000035";
			cusresInterchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			cusresInterchange2.EI_Status = EDIInterchange.Status.Queued;
			cusresInterchange2.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160802:0122+1216++EXPORT+++GWWTGTEST+1'";
			cusresInterchange2.EI_BodyText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655KFN20160801000996'DTM+178:20160801:102'TDT+20+DE1455+3+++++:::DE1455'LOC+22+KFN::ZZZ'GIS+39:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:BOL1608011455'DTM+137:20160801:102'RFF+UCN:6ZA011432336ZA01143233000000000004'RFF+ACD:1027'TAX+3+CUS:107:ZZZ'MOA+161:290'CNT+7:100.00'CNT+11:10'UNT+16+1'";
			cusresInterchange2.EI_FooterText = "UNZ+1+1216'";

			Factory.Save();
			processor.ExecuteBatch();
			cusresInterchange2.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, cusresInterchange2.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, cusresInterchange2.ContainedMessages.Count);
			var cusresMessage2 = cusresInterchange2.ContainedMessages[0];
			messageProcessor.ExecuteBatch();
			entry.Reload();
			Factory.Load<EDIMessage>(query);
			entry.Messages.Load();
			AssertEquals("entry.Messages.Count", 8, entry.Messages.Count);
			AssertCollectionContains(cusresMessage2, entry.Messages);
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
			AssertEquals("entry.CH_EntryStatus", "39", entry.CH_EntryStatus);
		}

		public void TestGenerateMessageFromInterchange_STATAC_DAILY()
		{
			var expectedText = @"UNH+1+STATAC:D:96B:UN:ZZZ01'
BGM+342:::DAILY'
DTM+137:201607110120:203'
DTM+90:20160710:102'
DTM+91:20160710:102'
RFF+ADE:8120050169'
NAD+CM+CTN'
NAD+AG+21044566'
DOC+914:::HC+01862282JSA20160710360193::CASH'
MOA+9:1500'
DTM+353:20160710:102'
DTM+140:20160710:102'
UNS+S'
MOA+86:1500'
UNT+15+1'";
			var messageText = @"UNB+UNOB:4+SARSDECT+00626166COR::BBAAAAAAAAAAAAAA:CORAS2+20160331:1530+1983++STATAC-DAILY+++UTITEST2+1'
" + expectedText + "UNZ+1+1983'";
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "ZAC";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = messageText;
			Factory.Save();

			var log = GetNewLoggerForTesting();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();

			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "ZAC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "STA", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageNum", "1", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", expectedText.Replace("\r\n", ""), createdMessage.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateMessageFromInterchange_STATAC_DETAIL()
		{
			var expectedText = @"UNH+1+STATAC:D:96B:UN:ZZZ01'
BGM+342:::DETAIL+333596'
DTM+137:201607100725:203'
DTM+90:20160613:102'
DTM+91:20160707:102'
RFF+ADE:8120223246'
NAD+CM+LBA'
NAD+AG+01862282'
DOC+914:::H+01862282LBA20160613355310::DEFERMENT DECLARATION'
MOA+9:13694.44'
DTM+353:20160613:102'
DOC+914:::IV+01862282LBA20160613355310::VAT'
MOA+9:6251.84'
DTM+353:20160613:102'
DTM+140:20160620:102'
DOC+914:::ID+01862282LBA20160613355310::DUTIES'
MOA+9:7442.6'
DTM+353:20160613:102'
DTM+140:20160620:102'
DOC+914:::I+8120223246CF0000697::PAYMENT'
MOA+9:-13694.44'
DTM+353:20160617:102'
UNS+S'
MOA+86:443443.42'
UNT+25+1'";
			var messageText = @"UNB+UNOB:4+SARSDECT+00626166COR::BBAAAAAAAAAAAAAA:CORAS2+20160331:1530+1983++STATAC-DAILY+++UTITEST2+1'
" + expectedText + "UNZ+1+1983'";
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "ZAC";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = messageText;
			Factory.Save();

			var log = GetNewLoggerForTesting();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();

			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "ZAC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "STA", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageNum", "1", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", expectedText.Replace("\r\n", ""), createdMessage.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestSupportsEnvironmentSwitch()
		{
			var interchangeProc = new InboundInterchangeProcessorForTest(GetNewLoggerForTesting());
			AssertEquals(true, interchangeProc.SupportEnvironmentSwitch);
		}

		public void TestIsNoBranchFilter()
		{
			var interchangeProc = new InboundInterchangeProcessorForTest(GetNewLoggerForTesting());
			AssertEquals(true, interchangeProc.IsNoBranchFilter);
		}

		public void TestInterchangesProcessedToCorrectBranches()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ZC1";
			company.GC_Name = "TEST COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;

			Factory.Save();

			var expectedText = @"UNH+1+CONTRL:D:3:UN:CONTRL'
UCI+00000000027997+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSREQT+7'
UCM+00000000065627+REQDOC:D:99B:UN:ZZZ01+7'
UNT+4+1'
";
			var messageText = @"UNB+UNOB:4+SARSREQT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160217:0633+39++CONTRL+++GWWTGTEST+1'
" + expectedText + "UNZ+1+39'";

			var interchange1 = Factory.New<ZACInterchange>();
			interchange1.EI_From = "SARS";
			interchange1.EI_To = "TEST";
			interchange1.EI_ApplicationCode = "ZAC";
			interchange1.EI_InterchangeType = "ZAC";
			interchange1.EI_InterchangeNum = "00000000000000000031";
			interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange1.EI_Status = EDIInterchange.Status.Queued;
			interchange1.EI_IsActive = true;
			interchange1.EI_BodyText = messageText;
			interchange1.EI_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var interchangeProc = new InboundInterchangeProcessorForTest(GetNewLoggerForTesting());
			interchangeProc.ExecuteBatch();

			var newBranch2 = company.Branches.AddNew();
			newBranch2.GB_Code = "BZ2";
			newBranch2.GB_BranchName = "B2 NAME";
			newBranch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			var interchange2 = Factory.New<ZACInterchange>();
			interchange2.EI_From = "SARS";
			interchange2.EI_To = "TEST";
			interchange2.EI_ApplicationCode = "ZAC";
			interchange2.EI_InterchangeType = "ZAC";
			interchange2.EI_InterchangeNum = "00000000000000000032";
			interchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange2.EI_Status = EDIInterchange.Status.Queued;
			interchange2.EI_IsActive = true;
			interchange2.EI_BodyText = messageText;
			interchange2.EI_GB = newBranch2.PK;
			Factory.Save();

			interchangeProc.ExecuteBatch();

			interchange1.Reload();
			interchange2.Reload();

			CombineAssertions("Interchange1 Pre-reqs", () =>
			{
				AssertEquals("Interchange1 Received", EDIInterchange.Status.Received, interchange1.EI_Status);
				AssertEquals("Interchange1 has 1 message", 1, interchange1.ContainedMessages.Count);
			});

			CombineAssertions("Message1 details", () =>
			{
				var msg1 = interchange1.ContainedMessages[0];
				AssertEquals("Msg1", GlbBranch.CurrentBranch.PK, msg1.EM_GB);
			});

			CombineAssertions("Interchange2 Pre-reqs", () =>
			{
				AssertEquals("Interchange2 Received", EDIInterchange.Status.Received, interchange2.EI_Status);
				AssertEquals("Interchange2 has 1 message", 1, interchange2.ContainedMessages.Count);
			});

			CombineAssertions("Message2 details", () =>
			{
				var msg2 = interchange2.ContainedMessages[0];
				AssertEquals("Msg2", newBranch2.PK, msg2.EM_GB);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		}

		void AssertGenerateMessageFromInterchangeForGateInOut(string applicationReference)
		{
			var headerText = $@"UNB+UNOB:4+SARSCART+ZS::WTGTESTDEPOTSEA1:WTGAS2+20180426:1421+13++{applicationReference}++++'";
			var footerText = @"UNZ+1+13'";
			var bodyText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+963+BA6C5224C92C42798F0810154F91D1EF'
DTM+132:20180425:102'
TDT+20+V0987+1'
LOC+22+DUR::ZZZ'
GIS+6:120:ZZZ'
NAD+AG+00000000'
RFF+BH:HOUSEDORSEA'
RFF+AAS:MASTERDORSEA'
ERP+6:0'
ERC+0001::ZZZ'
FTX+AAO+++BLL DESCRIPTION OF GOODS Field is required: : : '
CNT+11:10'
UNT+14+1'";

			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "ZAC";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_HeaderText = headerText;
			interchange.EI_BodyText = bodyText;
			interchange.EI_FooterText = footerText;
			Factory.Save();

			var log = GetNewLoggerForTesting();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();

			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "ZAC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "RES", createdMessage.EM_MessageType);
			AssertMultilineASCIIEquals("EM_MessageText", bodyText.Replace("\r\n", ""), createdMessage.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		internal static LoggingInformation GetNewLoggerForTesting() => new LoggingInformationForTesting();
	}

	sealed class InboundInterchangeProcessorForTest : InboundInterchangeProcessor
	{
		public InboundInterchangeProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public new bool IsNoBranchFilter => base.IsNoBranchFilter;

		public new bool SupportEnvironmentSwitch => base.SupportEnvironmentSwitch;
	}

	sealed class LoggingInformationForTesting : LoggingInformation
	{
		public LoggingInformationForTesting() : base()
		{
			LogMessages = new ZStringBuilder();
			OnLogInfoAdded += new LogInfoAdded((string log, LogType logType) => LogMessages.AppendLine($"{logType}: {log}"));
		}

		public override void ClearLogs()
		{
			base.ClearLogs();
			LogMessages = new ZStringBuilder();
		}
		internal ZStringBuilder LogMessages;
	}
}
