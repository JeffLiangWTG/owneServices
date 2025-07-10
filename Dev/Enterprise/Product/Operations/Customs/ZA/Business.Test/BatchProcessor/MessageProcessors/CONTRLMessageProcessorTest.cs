using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CONTRLMessageProcessorTest : TestCaseWithFactory
	{
		public void TestRecalculateAsycudaHeaderMessageStatusAfterProcessingAsycudaBillResponses()
		{
			var manifestHeader = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestHeader.FillWithValidTestData();
			var bill1 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = "HB123";
			bill1.ABL_AMA = manifestHeader.PK;
			var bill2 = manifestHeader.Bills.AddNew();
			bill2.ABL_AMA = manifestHeader.PK;
			bill2.ABL_BillNumber = "HB223";
			var bill3 = manifestHeader.Bills.AddNew();
			bill3.ABL_AMA = manifestHeader.PK;
			bill3.ABL_BillNumber = "HB323";
			Factory.Save();
			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "202";
			outgoingMessage.EM_LinkUniqueID = bill1.PK;
			outgoingMessage.EM_LinkTable = "AsycudaBill";
			outgoingMessage.MessageNumForTesting = "202";
			Factory.Save();
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "7");
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			AssertEquals("ACK", bill1.ABL_MessageStatus);
			AssertEquals("ACK", manifestHeader.AMA_MessageStatus);
			bill1.ABL_MessageStatus = "AWA";
			bill2.ABL_MessageStatus = "AWA";
			testMessage.EM_Status = "QUE";
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			AssertEquals("ACK", bill1.ABL_MessageStatus);
			AssertEquals("AWA", manifestHeader.AMA_MessageStatus);
			bill1.ABL_MessageStatus = "AWA";
			bill2.ABL_MessageStatus = "ACK";
			bill3.ABL_MessageStatus = "ERR";
			testMessage.EM_Status = "QUE";
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			AssertEquals("ACK", bill1.ABL_MessageStatus);
			AssertEquals("ERR", manifestHeader.AMA_MessageStatus);
		}

		public void TestTouchRequiredFieldsOnContrlMessage()
		{
			var elements1 = new InterchangeRecipientElements();
			elements1.InternalLocationAdress = "t1";
			elements1.InternalSubLocationAdress = "t1";
			CombineAssertions("InterchangeRecipientElements Touch", () =>
			{
				AssertEquals("InternalLocationAdress", "t1", elements1.InternalLocationAdress);
				AssertEquals("InternalSubLocationAdress", "t1", elements1.InternalSubLocationAdress);
			});
			var elements2 = new InterchangeSenderElements();
			elements2.InternalLocationAdress = "t2";
			elements2.InternalSubLocationAdress = "t2";
			CombineAssertions("InterchangeSenderElements Touch", () =>
			{
				AssertEquals("InternalLocationAdress", "t2", elements2.InternalLocationAdress);
				AssertEquals("InternalSubLocationAdress", "t2", elements2.InternalSubLocationAdress);
			});
		}

		readonly string testMessageResNo = @"UNH+1+CONTRL:D:3:UN:CONTRL'
UCI+181+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'
UCM+202+CUSDEC:D:96B:UN:ZZZ01+{0}'
UNT+4+1'";
		readonly string testMessageReqdocResponse = @"UNH+1+CONTRL:D:3:UN:CONTRL'
UCI+181+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSREQT+7'
UCM+202+REQDOC:D:99B:UN:ZZZ01+7'
UNT+4+1'";
		[TestDate(2016, 6, 01)]
		public void TestRollbackOfPermitTransactionsWhenRejected()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MainAddress.Address1 = "IMPORTERADDR1";
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			testHeader.CH_BGMReference = "DBN201609231234567";
			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "202";
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.MessageNumForTesting = "202";
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m);
			var transaction = permitHelper.CreatePermitLineTransaction(permit, testHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, Customs.Business.PermitTransactionStatusList.Codes.Pending);
			Factory.Save();
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			var query = permitHelper.GetPermitLineTransactionQuery(testHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			AssertNotNull("Transaction exists", new BusinessObjectFactory().LoadTop1<Customs.Business.BaseCusPermitLineTransaction>(query));
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "4"); //Response 4 - Message Rejected
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			CombineAssertions("Transaction Rolledback", () =>
			{
				AssertEquals("PRS", testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("ERR", testHeader.CH_Status);
				query = permitHelper.GetPermitLineTransactionQuery(testHeader, testMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<Customs.Business.BaseCusPermitLineTransaction>(query);
				AssertEquals("One transactions for message flagged as deleted", 1, transactions.Length);
				AssertEquals("DTI2014/7656 Outgoing Value = -50m", -50m, transactions[0].CPL_TranValue);
				AssertEquals("Status should be DEL", "DEL", transactions[0].CPL_TransactionStatus);
			});
		}

		public void TestContrlMessageProcessingForDuplicateOutgoingMessages()
		{
			CreateOutgoingMessageAndHeader();
			testHeader.CH_BGMReference = "TESTHeader1";
			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "202";
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.MessageNumForTesting = "202";
			outgoingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 6, 15);
			Factory.Save();
			AssertEquals("", testHeader.CH_Status);
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "4"); //Response 4 - Message Rejected
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			CombineAssertions("Rejected", () =>
			{
				AssertEquals("PRS", testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("ERR", testHeader.CH_Status);
				AssertEquals("Error", testHeader.MessageStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CONTRL Message: #1/ to job: TESTHeader1
Information: 	Message Status of job:TESTHeader1 has been updated to 'ERR'.", logger.LogMessages.ToString());
			});
			logger.ClearLogs();
			testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "7"); //Response 7 - Message Acknowledged
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			CombineAssertions("Acknowledged", () =>
			{
				AssertEquals("PRS", testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("ACK", testHeader.CH_Status);
				AssertEquals("Acknowledged", testHeader.MessageStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CONTRL Message: #1/ to job: TESTHeader1
Information: 	Message Status of job:TESTHeader1 has been updated to 'ACK'.", logger.LogMessages.ToString());
			});
		}

		public void TestContrlMessageProcessing()
		{
			testHeader.CH_BGMReference = "TESTHeader1";
			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "202";
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.MessageNumForTesting = "202";
			Factory.Save();
			AssertEquals("", testHeader.CH_Status);
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "4"); //Response 4 - Message Rejected
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			CombineAssertions("Rejected", () =>
			{
				AssertEquals("PRS", testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("ERR", testHeader.CH_Status);
				AssertEquals("Error", testHeader.MessageStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CONTRL Message: #1/ to job: TESTHeader1
Information: 	Message Status of job:TESTHeader1 has been updated to 'ERR'.", logger.LogMessages.ToString());
			});
			logger.ClearLogs();
			testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "7"); //Response 7 - Message Acknowledged
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			CombineAssertions("Acknowledged", () =>
			{
				AssertEquals("PRS", testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("ACK", testHeader.CH_Status);
				AssertEquals("Acknowledged", testHeader.MessageStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CONTRL Message: #1/ to job: TESTHeader1
Information: 	Message Status of job:TESTHeader1 has been updated to 'ACK'.", logger.LogMessages.ToString());
			});
		}

		public void TestContrlMessageProcessingGOVGIO()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = "OUT";
			manifestHeader.AMA_MasterBill = "MAN0123";
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "HB234";
			Factory.Save();
			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "202";
			outgoingMessage.EM_LinkUniqueID = manifestHeader.PK;
			outgoingMessage.EM_LinkTable = manifestHeader.TableName;
			outgoingMessage.MessageNumForTesting = "202";
			Factory.Save();
			AssertEquals("", manifestHeader.AMA_MessageStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "4"); //Response 4 - Message Rejected
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			CombineAssertions("Rejected", () =>
			{
				AssertEquals("PRS", testMessage.EM_Status);
				AssertEquals(manifestHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("ERR", manifestHeader.AMA_MessageStatus);
				var allMessages = logger.LogMessages.ToString();
				AssertNotContains("Unable to find the linked job", allMessages);
				AssertContains("Linking CONTRL Message: #1/ to job: MAN0123", allMessages);
				AssertContains("Message Status of job:MAN0123 has been updated to 'ERR'.", allMessages);
			});
			logger.ClearLogs();
			testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "7"); //Response 7 - Message Acknowledged
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			CombineAssertions("Acknowledged", () =>
			{
				AssertEquals("PRS", testMessage.EM_Status);
				AssertEquals(manifestHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("ACK", manifestHeader.AMA_MessageStatus);
				var allMessages = logger.LogMessages.ToString();
				AssertNotContains("Unable to find the linked job", allMessages);
				AssertContains("Linking CONTRL Message: #1/ to job: MAN0123", allMessages);
				AssertContains("Message Status of job:MAN0123 has been updated to 'ACK'.", allMessages);
			});
		}

		public void TestDontProcessForReqdocResponses()
		{
			testHeader.CH_BGMReference = "TESTHeader1";
			var newFactory = new BusinessObjectFactory();
			var outgoingMessage = newFactory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "REQ";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "202";
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.MessageNumForTesting = "202";
			newFactory.Save();
			testHeader.Messages.Add(Factory.Load<CONTRLEDIMessage>(outgoingMessage.PK));
			Factory.Save();
			AssertEquals("", testHeader.CH_Status);
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "ACK";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = testMessageReqdocResponse.Replace("\r\n", "");
			Factory.Save();
			PreProcessAndProcess(logger, testMessage);
			Factory.Save();
			CombineAssertions("Process CONTRL message For REQDOC, but won't update job MessageStatus", () =>
			{
				AssertEquals("PRS", testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("", testHeader.CH_Status);
				AssertEquals("Not Sent", testHeader.MessageStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CONTRL Message: #1/ to job: TESTHeader1", logger.LogMessages.ToString());
			});
		}

		public void TestPreProcessMessage_CusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testHeader = declaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_BGMReference = "TESTHeader1";
			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "202";
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.MessageNumForTesting = "202";
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "4"); //Response 4 - Message Rejected
			Factory.Save();
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			new CONTRLMessageProcessor(logger).PreProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				AssertEquals(CusEntryHeader.Schema.TableName, testMessage.EM_LinkTable);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals(Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK, testMessage.EM_Status);
			});
		}

		public void TestPreProcessMessage_AsycudaManifestHeader()
		{
			var testHeader = Factory.New<AsycudaManifestHeader>();
			testHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			testHeader.AMA_JobReference = "MAN0000315";
			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "901";
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.MessageNumForTesting = "901";
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+181+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+901+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			Factory.Save();
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			new CONTRLMessageProcessor(logger).PreProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				AssertEquals(AsycudaManifestHeader.Schema.TableName, testMessage.EM_LinkTable);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals(Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK, testMessage.EM_Status);
			});
		}

		public void TestPreProcessMessage_NoMatch()
		{
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = string.Format(testMessageResNo.Replace("\r\n", ""), "4"); //Response 4 - Message Rejected
			Factory.Save();
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			new CONTRLMessageProcessor(logger).PreProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				AssertNull(testMessage.EM_LinkedObject);
				AssertContains("Unable to find the linked job for CONTRL Message: #1/", logger.LogMessages.ToString());
			});
		}

		public void TestPreProcessingRequired()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var processor = new CONTRLMessageProcessor(logger);
			AssertEquals(true, processor.RequiresPreProcessing);
		}

		public void TestProcessBatch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testHeader = declaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_BGMReference = "TESTHeader1";
			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "911";
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.MessageNumForTesting = "911";
			var testMessage = Factory.NewWithValidTestData<CONTRLMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+181+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+911+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var proc = new ZACIncomingMessageProcessor(logger);
			proc.ExecuteBatch();
			testMessage.Reload();
			AssertEquals("PRS", testMessage.EM_Status);
			AssertEquals("CusEntryHeader", testMessage.EM_LinkTable);
			AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
		}

		void PreProcessAndProcess(LoggingInformation logger, EDIMessage ctrlMessage)
		{
			var proc = new CONTRLMessageProcessor(logger);
			proc.PreProcessMessage(ctrlMessage);
			proc.ProcessMessage(ctrlMessage);
		}

		CusEntryHeader testHeader;
		CusEntryHeader oldHeader;
		protected override void SetUp()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_MessageType = "IMP";
				var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_Style = "11";
				var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = "20";
				var invHeader1 = declaration.Invoices.AddNew();
				invHeader1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				var invLine11 = invHeader1.JobComInvoiceLines.AddNew();
				invLine11.JI_CEI = entryInstruction1.PK;
				invLine11.JI_Procedure = invLine11.EntryInstruction.CEI_Style + "00";
				invLine11.JI_NewUsed = GoodsTypeList.Codes.S;
				var invHeader2 = declaration.Invoices.AddNew();
				invHeader2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				var invLine21 = invHeader2.JobComInvoiceLines.AddNew();
				invLine21.JI_CEI = entryInstruction1.PK;
				invLine21.JI_Procedure = invLine21.EntryInstruction.CEI_Style + "21";
				var invLine22 = invHeader2.JobComInvoiceLines.AddNew();
				invLine22.JI_CEI = entryInstruction1.PK;
				invLine22.JI_Procedure = invLine22.EntryInstruction.CEI_Style + "00";
				invLine22.JI_NewUsed = GoodsTypeList.Codes.S;
				Factory.Save();
				new LineMerger(declaration).DoMerge();
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				Factory.Save();
				testHeader = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			}
		}

		void CreateOutgoingMessageAndHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = "IMP";
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "11";
			var invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invLine11 = invHeader1.JobComInvoiceLines.AddNew();
			invLine11.JI_CEI = entryInstruction1.PK;
			invLine11.JI_Procedure = invLine11.EntryInstruction.CEI_Style + "00";
			invLine11.JI_NewUsed = GoodsTypeList.Codes.S;
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			Factory.Save();
			oldHeader = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			oldHeader.CH_BGMReference = "Old Header";

			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "202";
			outgoingMessage.EM_LinkUniqueID = oldHeader.PK;
			outgoingMessage.EM_LinkTable = oldHeader.TableName;
			outgoingMessage.MessageNumForTesting = "202";
			outgoingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 6, 15);
			Factory.Save();
		}

		sealed class CONTRLMessageForTest : CONTRLEDIMessage
		{
			internal string MessageNumForTesting { get; set; }

			public CONTRLMessageForTest(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber()
			{
				return MessageNumForTesting;
			}
		}
	}
}
