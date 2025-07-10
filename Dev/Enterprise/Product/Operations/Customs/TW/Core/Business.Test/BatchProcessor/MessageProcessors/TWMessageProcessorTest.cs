using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWMessageProcessor))]
	sealed class TWMessageProcessorTest : TWXmlTestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestProcessLicensingAgencyMessage()
		{
			var decl = Factory.New<JobDeclaration>();
			decl.JE_CustomsOffice = "AA";
			var entryInstruction = decl.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_FunctionalReferenceId = "96944490002307040002";
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			entryNumber.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			entryNumber.CE_EntryNum = "CA  1245600093";
			Factory.Save();

			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX901.xml");
			testMessage.EM_MessageType = MessageTypeList.Codes._901;
			var logger = (LoggingInformationForTesting)TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.ProcessedOK).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkTable, NUnit.Framework.Is.EqualTo(CusTWControllingMessageHeaderSchema.Constants.TableName).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(controllingMessageHeader.PK));
			});
		}

		[ExpectNoExceptions]
		public void TestProcessCertificateOfOriginApplicatioMessage()
		{
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX101, "22099131002309220001", ZString.Empty, MessageTypeList.Codes._102, "NX102", "Y");
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX201_01, "23322708001106180001", ZString.Empty, MessageTypeList.Codes._202, "NX202", "0");
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX301, "23322708001106110001", "CA  1245600072", MessageTypeList.Codes._302, "NX302", "N");
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX301_AX, "23322708001106110002", "AA 0710823476", MessageTypeList.Codes._32A, "NX302_AX", "5");
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX301_DN, "23322709001106110009", "CA  1245600072", MessageTypeList.Codes._32D, "NX302_DN", "1");
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX401, "23322709001106120001", "CA  1245600072", MessageTypeList.Codes._402, "NX402", "NN");
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX601, "23322709001106120010", "CA  1245600072", MessageTypeList.Codes._602, "NX602", "N");
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX603, "96944490002307040002", "CA  1245600093", MessageTypeList.Codes._901, "NX901", ZString.Empty);
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX603, "52889317002307280001", "CA  1248600825", MessageTypeList.Codes._902, "NX902", ZString.Empty);
			AssertProcessCertificateOfOriginApplicatioMessage(ControllingMessageTypeList.Codes.NX603, "52889317002307280003", "CA  1248600825", MessageTypeList.Codes._903, "NX903", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void AssertProcessCertificateOfOriginApplicatioMessage(ZString controllingMessageType, ZString functionalReferenceId, ZString entryNum, ZString eMMessageType, ZString xmlFileName, ZString expectedStatus)
		{
			var licensingMessageProcessorEDIMessageResult = TestMessageFactory.GetIncomingLicensingMessageProcessorEDIMessage(Factory, controllingMessageType, functionalReferenceId, entryNum, eMMessageType, xmlFileName, expectedStatus);
			var testMessage = licensingMessageProcessorEDIMessageResult.IncomingMessage;
			var logger = (LoggingInformationForTesting)TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);

			CombineAssertions(() =>
			{
				var controllingMessageHeader = licensingMessageProcessorEDIMessageResult.ControllingMessageHeader;
				NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.ProcessedOK).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkTable, NUnit.Framework.Is.EqualTo(CusTWControllingMessageHeaderSchema.Constants.TableName).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(controllingMessageHeader.PK));
				NUnit.Framework.Assert.That(controllingMessageHeader.TW1_EntryStatus, NUnit.Framework.Is.EqualTo(expectedStatus));
				NUnit.Framework.Assert.That(controllingMessageHeader.TW1_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Acknowledged).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestProcessForwarderHouseManifestMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Voyage = "5X 0059";
			header.AMA_TransportMode = "AIR";
			header.AMA_MasterBill = "406-51754603";

			var bill = header.Bills.AddNew();
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = bill.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "FHM";
			cusNum1.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "11233527SW1812100001";
			cusNum1.CE_EntryStatus = ZString.Empty;
			Factory.Save();

			var testMessage = CreateMessage(GetTWNotification("ERR", "FHM", "11233527SW1812100001", "NUM1"), "FHM");
			var logger = (LoggingInformationForTesting)TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Error).Using(CustomComparers.TypeComparison), "Messages > Message Status");
				NUnit.Framework.Assert.That(header.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.TransmissionError).Using(CustomComparers.TypeComparison), "Header > Message Status");
				NUnit.Framework.Assert.That(bill.ABL_MessageStatus, NUnit.Framework.Is.EqualTo(MessageStatusCodeList.Codes.Error).Using(CustomComparers.TypeComparison), "Bills > Message Status");
				NUnit.Framework.Assert.That(header.Messages.Count, NUnit.Framework.Is.EqualTo(1), "Message linked to header");
			});
		}

		[ExpectNoExceptions]
		public void TestProcessN5108MessageWithNoCorrespondingHeader()
		{
			var testMessage = Generate5108Message("Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.N5108.N5108.xml");
			var logger = (LoggingInformationForTesting)TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Discarded).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(logger.LogMessages.ToString(), NUnit.Framework.Does.Contain("Can not find the corresponding Manifest Header for Message Number: TWIN1"));
			});
		}

		[ExpectNoExceptions]
		public void TestProcessN5108MessageProcessedOK()
		{
			new TestTWCreator(Factory).CreateCustomsManifestStatus();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Voyage = "5X 0059";
			header.AMA_TransportMode = "AIR";
			header.AMA_MasterBill = "406-51754603";

			var bill1 = header.Bills.AddNew();
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = bill1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "FHM";
			cusNum1.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "11233527SW1812100001";
			cusNum1.CE_EntryStatus = ZString.Empty;

			var bill2 = header.Bills.AddNew();
			var cusNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = bill2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = "FHM";
			cusNum2.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "11233527SW1812100001";
			cusNum2.CE_EntryStatus = ZString.Empty;
			Factory.Save();

			var testMessage = Generate5108Message("Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.N5108.N5108.xml");
			var logger = (LoggingInformationForTesting)TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.ProcessedOK).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkTable, NUnit.Framework.Is.EqualTo(AsycudaManifestHeaderSchema.Constants.TableName).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(header.PK));
				NUnit.Framework.Assert.That(bill1.ABL_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Acknowledged).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(bill2.ABL_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Acknowledged).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(header.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Acknowledged).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(bill1.ABL_BillStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.RE).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(bill2.ABL_BillStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.RE).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CusEntryNumber.Load(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, header.AMA_RN_NKCountry).CE_EntryStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.RE).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestProcessN5108Message_WithoutFunctionalReferenceID()
		{
			new TestTWCreator(Factory).CreateCustomsManifestStatus();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Voyage = "5X 0059";
			header.AMA_TransportMode = "AIR";
			header.AMA_MasterBill = "406-51754603";

			var bill1 = header.Bills.AddNew();
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = bill1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "FHM";
			cusNum1.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "11233527SW1812100001";
			cusNum1.CE_EntryStatus = ZString.Empty;

			var bill2 = header.Bills.AddNew();
			var cusNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = bill2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = "FHM";
			cusNum2.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = ZString.Empty;
			cusNum2.CE_EntryStatus = ZString.Empty;
			Factory.Save();

			var testMessage = Generate5108Message("Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.N5108.N5108_WithoutFunctionalReferenceID.xml");
			var logger = (LoggingInformationForTesting)TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.ProcessedOK).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkTable, NUnit.Framework.Is.EqualTo(AsycudaManifestHeaderSchema.Constants.TableName).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(header.PK));
				NUnit.Framework.Assert.That(bill1.ABL_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Acknowledged).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(bill2.ABL_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Acknowledged).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(header.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Acknowledged).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(bill1.ABL_BillStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.RE).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(bill2.ABL_BillStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.RE).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(CusEntryNumber.Load(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, header.AMA_RN_NKCountry).CE_EntryStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.RE).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestProcessN5108MessageWithInvalidNameCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Voyage = "5X 0059";
			header.AMA_TransportMode = "AIR";
			header.AMA_MasterBill = "406-51754603";

			var bill1 = header.Bills.AddNew();
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = bill1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "FHM";
			cusNum1.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "11233527SW1812100001";
			cusNum1.CE_EntryStatus = ZString.Empty;
			Factory.Save();

			var testMessage = Generate5108Message("Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.N5108.N5108_InvalidStatusNameCode.xml");
			var logger = (LoggingInformationForTesting)TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.ProcessedOK).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkTable, NUnit.Framework.Is.EqualTo(AsycudaManifestHeaderSchema.Constants.TableName).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(header.PK));
				NUnit.Framework.Assert.That(cusNum1.CE_EntryStatus.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			});
		}

		TWMessage Generate5108Message(ZString messageTextPath)
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = GetExpectedMessageXML(messageTextPath);
			testMessage.EM_MessageType = "FHR";
			return testMessage;
		}

		[ExpectNoExceptions]
		public void TestLogCustomsReadyToPayEvent_N5110()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var incomingPayResponseNo1 = "ABI31100394446";
			var fee = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 308201m, TypeCode = "B40" };
			var testMessage1 = GetN5110Message(number1, incomingPayResponseNo1, new DutyTaxFeeForTest[] { fee });
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);
			var logEntries = cusHead1.Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsReadyToPay.Code));
			var log = logEntries[0];
			NUnit.Framework.Assert.That(log.SL_Reference, NUnit.Framework.Is.EqualTo("N5110 message received").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(log.SL_EventDescription, NUnit.Framework.Is.EqualTo("Customs Ready to Pay").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLogCustomsReadyToPayEvent_N5111()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var incomingPayResponseNo1 = "ABI32100355835";
			var testMessage1 = GetN5111Message(number1, incomingPayResponseNo1, "C10");
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);
			var logEntries = cusHead1.Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsReadyToPay.Code));
			var log = logEntries[0];
			NUnit.Framework.Assert.That(log.SL_Reference, NUnit.Framework.Is.EqualTo("N5111 message received").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(log.SL_EventDescription, NUnit.Framework.Is.EqualTo("Customs Ready to Pay").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddEntryPayInfoByN5111Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var incomingPayResponseNo1 = "ABI32100355835";
			var testMessage1 = GetN5111Message(number1, incomingPayResponseNo1, "C10");
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);
			var entryPayInfo = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo1).Single();
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(2431279m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_TransactionType, NUnit.Framework.Is.EqualTo("C10").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 19)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 05)));
			NUnit.Framework.Assert.That(entryPayInfo.C9_BankAccount, NUnit.Framework.Is.EqualTo("3070500001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReasonCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));

			var incomingPayResponseNo2 = "ABI32100355836";
			var testMessage2 = GetN5111Message(number1, incomingPayResponseNo2, "D10");
			processor.ProcessMessage(testMessage2);
			var entryPayInfos = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo2);
			NUnit.Framework.Assert.That(entryPayInfos.Length, NUnit.Framework.Is.EqualTo(1));

			var incomingPayResponseNo3 = "ABI32100355837";
			var testMessage3 = GetN5111Message(number1, incomingPayResponseNo3, "B2");
			processor.ProcessMessage(testMessage3);
			entryPayInfos = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo3);
			NUnit.Framework.Assert.That(entryPayInfos.Length, NUnit.Framework.Is.EqualTo(1));

			var entryHeader = Factory.Load<CusEntryHeader>(cusHead1.PK);
			NUnit.Framework.Assert.That(entryHeader.EntryPayInfos.Count, NUnit.Framework.Is.EqualTo(3), "The EntryHeader should have 3 EntryPayInfos.");
		}

		[ExpectNoExceptions]
		public void TestUpdateEntryPayInfoByN5111Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var incomingPayResponseNo = "ABI32100355835";
			var testMessage1 = GetN5111Message(number1, incomingPayResponseNo, "C10");
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);
			var entryPayInfo = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo).Single();
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(2431279m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_TransactionType, NUnit.Framework.Is.EqualTo("C10").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 19)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 05)));

			var testMessage2 = GetN5111Message(number1, incomingPayResponseNo, "C11", "2021-08-06");
			processor.ProcessMessage(testMessage2);
			var entryPayInfos = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo);
			NUnit.Framework.Assert.That(entryPayInfos.Length, NUnit.Framework.Is.EqualTo(1));
			entryPayInfo = entryPayInfos[0];
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(2431279m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_TransactionType, NUnit.Framework.Is.EqualTo("C11").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 20)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 06)));

			var entryHeader = Factory.Load<CusEntryHeader>(cusHead1.PK);
			NUnit.Framework.Assert.That(entryHeader.EntryPayInfos.Count, NUnit.Framework.Is.EqualTo(1), "The EntryHeader should have 1 EntryPayInfo.");
		}

		[ExpectNoExceptions]
		public void TestDonotUpdateIfExistNewerEntryPayInfoByN5111Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var incomingPayResponseNo = "CGI32131301708";
			var testMessage1 = GetN5111Message(number1, incomingPayResponseNo, "C10", "2024-05-23");
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);
			var entryPayInfo = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo).Single();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(2431279m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_TransactionType, NUnit.Framework.Is.EqualTo("C10").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2024, 06, 06)).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2024, 05, 23)));
			});

			var testMessage2 = GetN5111Message(number1, incomingPayResponseNo, "C11", "2024-05-21");
			processor.ProcessMessage(testMessage2);
			var entryPayInfos = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryPayInfos.Length, NUnit.Framework.Is.EqualTo(1));
				entryPayInfo = entryPayInfos[0];
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(2431279m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_TransactionType, NUnit.Framework.Is.EqualTo("C10").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2024, 06, 06)).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2024, 05, 23)));
			});

			var entryHeader = Factory.Load<CusEntryHeader>(cusHead1.PK);
			NUnit.Framework.Assert.That(entryHeader.EntryPayInfos.Count, NUnit.Framework.Is.EqualTo(1), "The EntryHeader should have 1 EntryPayInfo.");
		}

		[ExpectNoExceptions]
		public void TestAddCusDispositionByN5107Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var statusKey = CusDispositionStatusKeyList.Codes.RFM;
			var testMessage1 = GetN5107Message("2021-08-05T14:30:56", "A01", "A02");
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);

			var dispositions = GetCusDisposition(cusHead1.PK, statusKey);
			CombineAssertions("test Message1 of dispositions", () =>
			{
				NUnit.Framework.Assert.That(dispositions.Length, NUnit.Framework.Is.EqualTo(2), "dispositions length");
				var disposition0 = dispositions[0];
				NUnit.Framework.Assert.That(disposition0.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition0.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Status, NUnit.Framework.Is.EqualTo("A01").Using(CustomComparers.TypeComparison), "disposition0.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusKey, NUnit.Framework.Is.EqualTo("RFM").Using(CustomComparers.TypeComparison), "disposition0.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2021, 8, 5, 14, 30, 56)), "disposition0.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition0.CDI_Notes should be");
				var disposition1 = dispositions[1];
				NUnit.Framework.Assert.That(disposition1.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition1.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition1.CDI_Status, NUnit.Framework.Is.EqualTo("A02").Using(CustomComparers.TypeComparison), "disposition1.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition1.CDI_StatusKey, NUnit.Framework.Is.EqualTo("RFM").Using(CustomComparers.TypeComparison), "disposition1.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition1.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2021, 8, 5, 14, 30, 56)), "disposition1.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition1.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition1.CDI_Notes should be");
			});

			var testMessage2 = GetN5107Message("2022-12-12T14:30:57", "G01", "");
			processor.ProcessMessage(testMessage2);
			dispositions = GetCusDisposition(cusHead1.PK, statusKey);
			CombineAssertions("test Message2 of dispositions", () =>
			{
				NUnit.Framework.Assert.That(dispositions.Length, NUnit.Framework.Is.EqualTo(1), "dispositions length");
				var disposition0 = dispositions[0];
				NUnit.Framework.Assert.That(disposition0.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition0.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Status, NUnit.Framework.Is.EqualTo("G01").Using(CustomComparers.TypeComparison), "disposition0.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusKey, NUnit.Framework.Is.EqualTo("RFM").Using(CustomComparers.TypeComparison), "disposition0.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2022, 12, 12, 14, 30, 57)), "disposition0.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition0.CDI_Notes should be");
			});
		}

		[ExpectNoExceptions]
		public void TestAddCusDispositionByNX5106Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var statusKey = CusDispositionStatusKeyList.Codes.ARM;
			var testMessage1 = GetNX5106Message("2019-01-25T14:30:56", "A03", "A04");
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);

			var dispositions = GetCusDisposition(cusHead1.PK, statusKey);
			CombineAssertions("test Message1 of dispositions", () =>
			{
				NUnit.Framework.Assert.That(dispositions.Length, NUnit.Framework.Is.EqualTo(2), "dispositions length");
				var disposition0 = dispositions[0];
				NUnit.Framework.Assert.That(disposition0.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition0.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Status, NUnit.Framework.Is.EqualTo("A03").Using(CustomComparers.TypeComparison), "disposition0.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusKey, NUnit.Framework.Is.EqualTo("ARM").Using(CustomComparers.TypeComparison), "disposition0.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 1, 25, 14, 30, 56)), "disposition0.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition0.CDI_Notes should be");
				var disposition1 = dispositions[1];
				NUnit.Framework.Assert.That(disposition1.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition1.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition1.CDI_Status, NUnit.Framework.Is.EqualTo("A04").Using(CustomComparers.TypeComparison), "disposition1.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition1.CDI_StatusKey, NUnit.Framework.Is.EqualTo("ARM").Using(CustomComparers.TypeComparison), "disposition1.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition1.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 1, 25, 14, 30, 56)), "disposition1.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition1.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition1.CDI_Notes should be");
			});

			var testMessage2 = GetNX5106Message("2022-12-12T14:30:57", "B46", "");
			processor.ProcessMessage(testMessage2);
			dispositions = GetCusDisposition(cusHead1.PK, statusKey);
			CombineAssertions("test Message2 of dispositions", () =>
			{
				NUnit.Framework.Assert.That(dispositions.Length, NUnit.Framework.Is.EqualTo(1), "dispositions length");
				var disposition0 = dispositions[0];
				NUnit.Framework.Assert.That(disposition0.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition0.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Status, NUnit.Framework.Is.EqualTo("B46").Using(CustomComparers.TypeComparison), "disposition0.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusKey, NUnit.Framework.Is.EqualTo("ARM").Using(CustomComparers.TypeComparison), "disposition0.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2022, 12, 12, 14, 30, 57)), "disposition0.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition0.CDI_Notes should be");
			});
		}

		[ExpectNoExceptions]
		public void TestAddCusDispositionByN5204Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var statusKey = CusDispositionStatusKeyList.Codes.CLR;
			var testMessage1 = GetN5204Message("2019-01-25T14:30:56", "2", "1");
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);

			var dispositions = GetCusDisposition(cusHead2.PK, statusKey);
			CombineAssertions("test Message1 of dispositions", () =>
			{
				NUnit.Framework.Assert.That(dispositions.Length, NUnit.Framework.Is.EqualTo(2), "dispositions length");
				var disposition0 = dispositions[0];
				NUnit.Framework.Assert.That(disposition0.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition0.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Status, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "disposition0.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusKey, NUnit.Framework.Is.EqualTo("CLR").Using(CustomComparers.TypeComparison), "disposition0.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 1, 25, 14, 30, 56)), "disposition0.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition0.CDI_Notes should be");
				var disposition1 = dispositions[1];
				NUnit.Framework.Assert.That(disposition1.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition1.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition1.CDI_Status, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "disposition1.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition1.CDI_StatusKey, NUnit.Framework.Is.EqualTo("CLR").Using(CustomComparers.TypeComparison), "disposition1.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition1.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 1, 25, 14, 30, 56)), "disposition1.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition1.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition1.CDI_Notes should be");
			});

			var testMessage2 = GetN5204Message("2022-12-12T14:30:57", "A", "");
			processor.ProcessMessage(testMessage2);
			dispositions = GetCusDisposition(cusHead2.PK, statusKey);
			CombineAssertions("test Message2 of dispositions", () =>
			{
				NUnit.Framework.Assert.That(dispositions.Length, NUnit.Framework.Is.EqualTo(1), "dispositions length");
				var disposition0 = dispositions[0];
				NUnit.Framework.Assert.That(disposition0.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition0.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Status, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "disposition0.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusKey, NUnit.Framework.Is.EqualTo("CLR").Using(CustomComparers.TypeComparison), "disposition0.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2022, 12, 12, 14, 30, 57)), "disposition0.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition0.CDI_Notes should be");
			});
		}

		[ExpectNoExceptions]
		public void TestAddCusDispositionByN5116Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var statusKey = CusDispositionStatusKeyList.Codes.CLR;
			var testMessage1 = GetN5116Message("2019-01-25T14:30:56", "4", "3");
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);

			var dispositions = GetCusDisposition(cusHead1.PK, statusKey);
			CombineAssertions("test Message1 of dispositions", () =>
			{
				NUnit.Framework.Assert.That(dispositions.Length, NUnit.Framework.Is.EqualTo(2), "dispositions length");
				var disposition0 = dispositions[0];
				NUnit.Framework.Assert.That(disposition0.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition0.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Status, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison), "disposition0.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusKey, NUnit.Framework.Is.EqualTo("CLR").Using(CustomComparers.TypeComparison), "disposition0.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 1, 25, 14, 30, 56)), "disposition0.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition0.CDI_Notes should be");
				var disposition1 = dispositions[1];
				NUnit.Framework.Assert.That(disposition1.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition1.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition1.CDI_Status, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison), "disposition1.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition1.CDI_StatusKey, NUnit.Framework.Is.EqualTo("CLR").Using(CustomComparers.TypeComparison), "disposition1.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition1.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 1, 25, 14, 30, 56)), "disposition1.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition1.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition1.CDI_Notes should be");
			});

			var testMessage2 = GetN5116Message("2022-12-12T14:30:57", "B", "");
			processor.ProcessMessage(testMessage2);
			dispositions = GetCusDisposition(cusHead1.PK, statusKey);
			CombineAssertions("test Message2 of dispositions", () =>
			{
				NUnit.Framework.Assert.That(dispositions.Length, NUnit.Framework.Is.EqualTo(1), "dispositions length");
				var disposition0 = dispositions[0];
				NUnit.Framework.Assert.That(disposition0.CDI_Type, NUnit.Framework.Is.EqualTo("CUS").Using(CustomComparers.TypeComparison), "disposition0.CDI_Type should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Status, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison), "disposition0.CDI_Status should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusKey, NUnit.Framework.Is.EqualTo("CLR").Using(CustomComparers.TypeComparison), "disposition0.CDI_StatusKey should be");
				NUnit.Framework.Assert.That(disposition0.CDI_StatusDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2022, 12, 12, 14, 30, 57)), "disposition0.CDI_StatusDate should be");
				NUnit.Framework.Assert.That(disposition0.CDI_Notes, NUnit.Framework.Is.EqualTo(ZString.Empty), "disposition0.CDI_Notes should be");
			});
		}

		CusEntryPayInfo[] GetCusEntryPayInfos(ZGuid entryHeaderPK, ZString incomingPayResponseNo)
		{
			var query = new ZQuery(CusEntryPayInfoSchema.C9_CH, entryHeaderPK);
			query.AddToFilter(CusEntryPayInfoSchema.C9_IncomingPayResponseNo, incomingPayResponseNo);
			return Factory.Load<CusEntryPayInfo>(query);
		}

		CusDisposition[] GetCusDisposition(ZGuid parentID, ZString statusKey)
		{
			var query = new ZQuery(CusDispositionSchema.CDI_ParentID, parentID);
			query.AddToFilter(CusDispositionSchema.CDI_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
			query.AddToFilter(CusDispositionSchema.CDI_Type, Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			query.AddToFilter(CusDispositionSchema.CDI_StatusKey, statusKey);
			return Factory.Load<CusDisposition>(query);
		}

		TWMessage GetN5111Message(ZString entryNumber, ZString incomingPayResponseNo, ZString depositTypeCode, string issueDateTime = "2021-08-05")
		{
			var messageText = GetN5111MessageText(entryNumber, incomingPayResponseNo, depositTypeCode, issueDateTime);
			return CreateMessage(messageText, MessageTypeList.Codes.TAD);
		}

		TWMessage GetN5107Message(ZString issueDateTime, ZString validationCode1, ZString validationCode2)
		{
			var messageText = GetN5107MessageText(issueDateTime, validationCode1, validationCode2);
			return CreateMessage(messageText, MessageTypeList.Codes.RFM);
		}

		TWMessage GetNX5106Message(ZString issueDateTime, ZString statusNameCode1, ZString statusNameCode2)
		{
			var messageText = GetNX5106MessageText(issueDateTime, statusNameCode1, statusNameCode2);
			return CreateMessage(messageText, MessageTypeList.Codes.ARM);
		}

		TWMessage GetN5204Message(ZString releaseDateTime, ZString statementCode1, ZString statementCode2)
		{
			var messageText = GetN5204MessageText(releaseDateTime, statementCode1, statementCode2);
			return CreateMessage(messageText, MessageTypeList.Codes.ERM);
		}

		TWMessage GetN5116Message(ZString releaseDateTime, ZString statementCode1, ZString statementCode2)
		{
			var messageText = GetN5116MessageText(releaseDateTime, statementCode1, statementCode2);
			return CreateMessage(messageText, MessageTypeList.Codes.IRM);
		}

		[TestDate(2021, 8, 25)]
		[ExpectNoExceptions]
		public void TestAddEntryPayInfoByN5110Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var incomingPayResponseNo1 = "ABI31100394446";
			var fee = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 308201m, TypeCode = "B40" };
			var testMessage1 = GetN5110Message(number1, incomingPayResponseNo1, new DutyTaxFeeForTest[] { fee });
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);
			var entryPayInfo = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo1).Single();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(308201m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_TransactionType, NUnit.Framework.Is.EqualTo("B40").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 03)).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 25)));
				NUnit.Framework.Assert.That(entryPayInfo.OtherChargeDeductionAmount, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
			});

			var incomingPayResponseNo2 = "ABI31100368119";
			fee = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 14608m, TypeCode = "A10" };
			var testMessage2 = GetN5110Message(number1, incomingPayResponseNo2, new DutyTaxFeeForTest[] { fee });
			processor.ProcessMessage(testMessage2);
			var entryPayInfos = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo2);
			NUnit.Framework.Assert.That(entryPayInfos.Length, NUnit.Framework.Is.EqualTo(1));

			var entryHeader = Factory.Load<CusEntryHeader>(cusHead1.PK);
			NUnit.Framework.Assert.That(entryHeader.EntryPayInfos.Count, NUnit.Framework.Is.EqualTo(2), "The EntryHeader should have 2 EntryPayInfos.");
		}

		[TestDate(2021, 8, 25)]
		[ExpectNoExceptions]
		public void TestUpdateEntryPayInfoByN5110Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var incomingPayResponseNo = "ABI31100394446";
			var fee1 = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 14608m, TypeCode = "A10" };
			var fee2 = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 409m, TypeCode = "B51" };
			var fee3 = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 51964m, TypeCode = "B40" };
			var testMessage1 = GetN5110Message(number1, incomingPayResponseNo, new DutyTaxFeeForTest[] { fee1, fee2, fee3 });
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);
			var entryPayInfos = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo).OrderBy(x => x.C9_TransactionType).ToArray();
			var entryHeader = Factory.Load<CusEntryHeader>(cusHead1.PK);
			NUnit.Framework.Assert.That(entryHeader.EntryPayInfos.Count, NUnit.Framework.Is.EqualTo(3), "The EntryHeader should have 3 EntryPayInfos.");

			var entryPayInfo1 = entryPayInfos[0];
			NUnit.Framework.Assert.That(entryPayInfo1.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(14608m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo1.C9_TransactionType, NUnit.Framework.Is.EqualTo("A10").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo1.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 03)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo1.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo1.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo1.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 25)));

			var entryPayInfo2 = entryPayInfos[1];
			NUnit.Framework.Assert.That(entryPayInfo2.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(51964m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo2.C9_TransactionType, NUnit.Framework.Is.EqualTo("B40").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo2.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 03)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo2.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo2.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo2.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 25)));

			var entryPayInfo3 = entryPayInfos[2];
			NUnit.Framework.Assert.That(entryPayInfo3.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(409m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo3.C9_TransactionType, NUnit.Framework.Is.EqualTo("B51").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo3.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 03)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo3.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo3.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo3.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 25)));

			var fee = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 409m, TypeCode = "B51" };
			var testMessage2 = GetN5110Message(number1, incomingPayResponseNo, new DutyTaxFeeForTest[] { fee }, "2021-08-04");
			processor.ProcessMessage(testMessage2);
			entryPayInfos = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo);
			NUnit.Framework.Assert.That(entryPayInfos.Length, NUnit.Framework.Is.EqualTo(1));
			var entryPayInfo = entryPayInfos[0];
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(409m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_TransactionType, NUnit.Framework.Is.EqualTo("B51").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 04)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryPayInfo.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2021, 08, 25)));

			entryHeader = Factory.Load<CusEntryHeader>(cusHead1.PK);
			NUnit.Framework.Assert.That(entryHeader.EntryPayInfos.Count, NUnit.Framework.Is.EqualTo(1), "The EntryHeader should have 1 EntryPayInfo.");
		}

		[TestDate(2024, 6, 20)]
		[ExpectNoExceptions]
		public void TestDonotUpdateIfExistNewerEntryPayInfoByN5110Message()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var incomingPayResponseNo = "ABI31100394446";
			var fee1 = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 14608m, TypeCode = "A10" };
			var fee2 = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 409m, TypeCode = "B51" };
			var fee3 = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 51964m, TypeCode = "B40" };
			var testMessage1 = GetN5110Message(number1, incomingPayResponseNo, new DutyTaxFeeForTest[] { fee1, fee2, fee3 }, "2024-06-19");
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage1);
			var entryPayInfos = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo).OrderBy(x => x.C9_TransactionType).ToArray();
			var entryHeader = Factory.Load<CusEntryHeader>(cusHead1.PK);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryHeader.EntryPayInfos.Count, NUnit.Framework.Is.EqualTo(3), "The EntryHeader should have 3 EntryPayInfos.");
				var entryPayInfo = entryPayInfos[2];
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(409m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_TransactionType, NUnit.Framework.Is.EqualTo("B51").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2024, 06, 19)).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2024, 06, 20)));
			});

			var fee = new DutyTaxFeeForTest() { AdValoremTaxBaseAmount = 539m, TypeCode = "B51" };
			var testMessage2 = GetN5110Message(number1, incomingPayResponseNo, new DutyTaxFeeForTest[] { fee }, "2024-06-18");
			processor.ProcessMessage(testMessage2);
			entryPayInfos = GetCusEntryPayInfos(cusHead1.PK, incomingPayResponseNo).OrderBy(x => x.C9_TransactionType).ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryPayInfos.Length, NUnit.Framework.Is.EqualTo(3));
				var entryPayInfo = entryPayInfos[2];
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentAmount, NUnit.Framework.Is.EqualTo(409m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_TransactionType, NUnit.Framework.Is.EqualTo("B51").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentDate, NUnit.Framework.Is.EqualTo(new ZDate(2024, 06, 19)).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentReference, NUnit.Framework.Is.EqualTo("88888888").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_PaymentParty, NUnit.Framework.Is.EqualTo("BRK").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.C9_ReceiptDate, NUnit.Framework.Is.EqualTo(new ZDate(2024, 06, 20)));
			});

			entryHeader = Factory.Load<CusEntryHeader>(cusHead1.PK);
			NUnit.Framework.Assert.That(entryHeader.EntryPayInfos.Count, NUnit.Framework.Is.EqualTo(3), "The EntryHeader should have 3 EntryPayInfos.");
		}

		TWMessage GetN5110Message(ZString entryNumber, ZString incomingPayResponseNo, DutyTaxFeeForTest[] dutyTaxFees, string dueDateTime = "2021-08-03")
		{
			var messageText = GetN5110MessageText(entryNumber, incomingPayResponseNo, dutyTaxFees, dueDateTime, 2000m, 1000m);
			return CreateMessage(messageText, MessageTypeList.Codes.TPC);
		}

		[ExpectNoExceptions]
		public void TestUpdateClearanceStatus()
		{
			var testMap = new Dictionary<string, string>();
			testMap["IRM"] = "N5116.xml";
			testMap["ERM"] = "N5204.xml";
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			foreach (var messageTypeAndFileName in testMap)
			{
				var messageType = messageTypeAndFileName.Key;
				var fileName = messageTypeAndFileName.Value;
				var cusHead = messageType == "ERM" ? cusHead2 : cusHead1;
				var number = cusHead.EntryNumber;
				var testMessageC1 = CreateMessage(GetMessageText(fileName, number, null, "C1", null, null, "2011-05-23T09:30:50"), messageType);
				var testMessageC2 = CreateMessage(GetMessageText(fileName, number, null, "C2", null, null, "2011-05-23T09:30:50"), messageType);
				var testMessageC3M = CreateMessage(GetMessageText(fileName, number, null, "C3M", null, null, "2011-05-23T09:30:50"), messageType);
				var testMessageC3X = CreateMessage(GetMessageText(fileName, number, null, "C3X", null, null, "2011-05-23T09:30:50"), messageType);
				var testMessageA18 = CreateMessage(GetMessageText(fileName, number, null, "A18", null, null, "2011-05-23T09:30:50"), messageType);
				Factory.Save();
				CombineAssertions($"Test For {messageTypeAndFileName.Key}", () =>
				{
					AssertEntryStatusAndEvent(logger, testMessageC1, cusHead, "C1");
					AssertEntryStatusAndEvent(logger, testMessageC2, cusHead, "C2");
					AssertEntryStatusAndEvent(logger, testMessageC3M, cusHead, "C3M");
					AssertEntryStatusAndEvent(logger, testMessageC3X, cusHead, "C3X");
					AssertEntryStatusAndEvent(logger, testMessageA18, cusHead, "A18");
				});
			}
		}

		[ExpectNoExceptions]
		void AssertEntryStatusAndEvent(LoggingInformation logger, TWMessage message, CusEntryHeader cusHead, ZString expectEntryStatus)
		{
			var entryNumber = GetEntryNumberAfterProcessing(logger, message);
			NUnit.Framework.Assert.That(entryNumber.CE_EntryStatus, NUnit.Framework.Is.EqualTo(expectEntryStatus));
			NUnit.Framework.Assert.That(cusHead.Declaration.Logs.Find(log => log.SL_SE_NKEvent == "CSH" && log.SL_Reference == expectEntryStatus).Count(), NUnit.Framework.Is.EqualTo(1), $"one event for {expectEntryStatus} added");
		}

		Common.CusEntryNumber GetEntryNumberAfterProcessing(LoggingInformation logger, TWMessage message)
		{
			new TWMessageProcessor(logger).ProcessMessage(message);
			var entryHeader = Factory.Load<CusEntryHeader>(message.EM_LinkUniqueID);
			return entryHeader.CusEntryNumber;
		}

		[ExpectNoExceptions]
		public void TestEM_Status()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var testMessage = CreateMessage(GetMessageText("N5204.xml", "XXXX", null, null, null, null), "ERM");
			var testMessage1 = CreateMessage(GetMessageText("N5204.xml", number2, null, null, null, null), "ERM");
			var testMessage2 = CreateMessage("<A></A>", "ERM");
			var testMessage3 = CreateMessage("XXXX", "ERM");
			var testMessage4 = CreateMessage("XXXX", "ERM");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Discarded).Using(CustomComparers.TypeComparison));
			new TWMessageProcessor(logger).ProcessMessage(testMessage1);
			NUnit.Framework.Assert.That(testMessage1.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.ProcessedOK).Using(CustomComparers.TypeComparison));
			new TWMessageProcessor(logger).ProcessMessage(testMessage2);
			NUnit.Framework.Assert.That(testMessage2.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Failed).Using(CustomComparers.TypeComparison));
			new TWMessageProcessor(logger).ProcessMessage(testMessage3);
			NUnit.Framework.Assert.That(testMessage3.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Failed).Using(CustomComparers.TypeComparison));
			new TWMessageProcessor(logger).ProcessMessage(testMessage4);
			NUnit.Framework.Assert.That(testMessage4.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Failed).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEM_LinkUniqueIDWhenUHC()
		{
			var messageType = "UHC";
			var filename = "N5168.xml";
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var testMessage = CreateMessage(GetMessageText(filename, "XXXX", "1", null, null, null), messageType);
			var testMessage1 = CreateMessage(GetMessageText(filename, number1, "1", null, null, null), messageType);
			var testMessage2 = CreateMessage(GetMessageText(filename, number2, "1", null, null, null), messageType);
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(ZGuid.Empty));
			new TWMessageProcessor(logger).ProcessMessage(testMessage1);
			NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusHead1.PK), "When Declaration/GovernmentProdecure = '1', it means that the message is for Import Customs Declarations, and not Export Customs Declarations or Transhipments. So in this case, use CE_EntryType = 'IMP' and not CE_EntryType = 'EXP");
			new TWMessageProcessor(logger).ProcessMessage(testMessage2);
			NUnit.Framework.Assert.That(testMessage2.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(ZGuid.Empty));
			testMessage = CreateMessage(GetMessageText(filename, "XXXX", "2", null, null, null), messageType);
			testMessage1 = CreateMessage(GetMessageText(filename, number1, "2", null, null, null), messageType);
			testMessage2 = CreateMessage(GetMessageText(filename, number2, "2", null, null, null), messageType);
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(ZGuid.Empty));
			new TWMessageProcessor(logger).ProcessMessage(testMessage1);
			NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(ZGuid.Empty));
			new TWMessageProcessor(logger).ProcessMessage(testMessage2);
			NUnit.Framework.Assert.That(testMessage2.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusHead2.PK), "When Declaration/GovernmentProdecure = '2', it means that the message is for Export Customs Declarations, and not Import Customs Declarations or Transhipments. So in this case, use CE_EntryType = 'EXP' and not CE_EntryType = 'IMP'");
		}

		[ExpectNoExceptions]
		public void TestEM_LinkUniqueIDWhenERMorIRMorTPCorTAD()
		{
			var testMap = new Dictionary<string, string>();
			testMap["IRM"] = "N5116.xml";
			testMap["TPC"] = "N5110.xml";
			testMap["TAD"] = "N5111.xml";
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			AssertEM_LinkUniqueID(testMap, logger, number1, number2, cusHead1);
			testMap.Clear();
			testMap["ERM"] = "N5204.xml";
			AssertEM_LinkUniqueID(testMap, logger, number2, number1, cusHead2);
		}

		[ExpectNoExceptions]
		void AssertEM_LinkUniqueID(Dictionary<string, string> testMap, LoggingInformation logger, string number1, string number2, CusEntryHeader cusHead)
		{
			foreach (var messageTypeAndFileName in testMap)
			{
				var testMessage = CreateMessage(GetMessageText(messageTypeAndFileName.Value, "XXXX", null, null, null, null), messageTypeAndFileName.Key);
				var testMessage1 = CreateMessage(GetMessageText(messageTypeAndFileName.Value, number1, null, null, null, null), messageTypeAndFileName.Key);
				var testMessage2 = CreateMessage(GetMessageText(messageTypeAndFileName.Value, number2, null, null, null, null), messageTypeAndFileName.Key);
				Factory.Save();
				new TWMessageProcessor(logger).ProcessMessage(testMessage);
				NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(ZGuid.Empty));
				new TWMessageProcessor(logger).ProcessMessage(testMessage1);
				NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusHead.PK), "When EM_MessageType =" + messageTypeAndFileName.Key + ", the message is always for Import Customs Declaration.So in this case, use CE_EntryType = 'IMP' and not CE_EntryType = 'EXP.");
				new TWMessageProcessor(logger).ProcessMessage(testMessage2);
				NUnit.Framework.Assert.That(testMessage2.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(ZGuid.Empty));
			}
		}

		[ExpectNoExceptions]
		public void TestCH_EntryStatusWhenARM()
		{
			var filename = "NX5106.xml";
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var testMessage = CreateMessage(GetMessageText(filename, number1, "1", "B61", null, null), "ARM");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
			var cusHead1 = Factory.Load<CusEntryHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo("ARM").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusHead1.EntryHeaderStatusDescription, NUnit.Framework.Is.EqualTo("單證合一核覆訊息").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCH_EntryStatusWhenTAD()
		{
			var filename = "N5111.xml";
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var testMessage1 = CreateMessage(GetMessageText(filename, number1, null, null, null, null), "TAD");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage1);
			NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
			var cusHead1 = Factory.Load<CusEntryHeader>(testMessage1.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo("TAD").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCH_StatusWhenCustomsDeliveryNotification()
		{
			var messageTypes = new string[] { "ECD", "ICD", "ADM", "IEA" };
			var testMap = new Dictionary<string, string>();
			testMap["AWO"] = "ACO";
			testMap["AWC"] = "ACC";
			testMap["AWG"] = "ACG";
			testMap["AWE"] = "ACE";
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "NUM1";
			interchange1.EI_From = "TWCustoms.TEST";
			interchange1.EI_To = "TEST";
			interchange1.EI_ApplicationCode = "TWC";
			interchange1.EI_InterchangeType = "TWC";
			interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange1.EI_Status = EDIInterchange.Status.Queued;
			interchange1.EI_IsActive = true;
			interchange1.EI_BodyText = "AAA";
			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "NUM2";
			interchange2.EI_From = "TWCustoms.TEST";
			interchange2.EI_To = "TEST";
			interchange2.EI_ApplicationCode = "TWC";
			interchange2.EI_InterchangeType = "TWC";
			interchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange2.EI_Status = EDIInterchange.Status.Queued;
			interchange2.EI_IsActive = true;
			interchange2.EI_BodyText = "AAA";
			var message1 = cusHead1.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message1.EM_Status = EDIMessage.Status.Sent;
			message1.EM_EI = interchange1.PK;
			var message2 = cusHead2.Messages.AddNew();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message2.EM_Status = EDIMessage.Status.Sent;
			message2.EM_EI = interchange2.PK;
			Factory.Save();
			AssertCH_StatusWhenCustomsDeliveryNotification(messageTypes, testMap, message1, GetTWNotification("SNT", "IMP", cusHead1.EntryNumber, "NUM1"), "SNT", cusHead1, "1");
			testMap.Clear();
			testMap["AWO"] = "ERO";
			testMap["AWC"] = "ERC";
			testMap["AWG"] = "ERG";
			testMap["AWE"] = "ERE";
			AssertCH_StatusWhenCustomsDeliveryNotification(messageTypes, testMap, message2, GetTWNotification("ERR", "EXP", cusHead2.EntryNumber, "NUM2"), "ERR", cusHead2, "2");
			testMap.Clear();
			testMap["AWO"] = "AWO";
			testMap["AWC"] = "AWC";
			testMap["AWG"] = "AWG";
			testMap["AWE"] = "AWE";
			AssertCH_StatusWhenCustomsDeliveryNotification(messageTypes, testMap, message1, GetTWNotification("SNT", "IMP", cusHead1.EntryNumber, "NUM3"), "SNT", cusHead1, "3");
			AssertCH_StatusWhenCustomsDeliveryNotification(messageTypes, testMap, message2, GetTWNotification("ERR", "EXP", cusHead2.EntryNumber, "NUM3"), "ERR", cusHead2, "3");
		}

		[ExpectNoExceptions]
		void AssertCH_StatusWhenCustomsDeliveryNotification(string[] messageTypes, Dictionary<string, string> testMap, TWMessage lastMessage, string messageText, string eventType, CusEntryHeader cusHeader, string type)
		{
			var num = 0;
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			foreach (var status in testMap)
			{
				foreach (var messageType in messageTypes)
				{
					lastMessage.EM_MessageType = messageType;
					num++;
					cusHeader.CH_Status = status.Key;
					var testMessage1 = CreateMessage(messageText, messageType);
					testMessage1.EM_MessageNum = $"{type}{eventType}{num}";
					Factory.Save();
					new TWMessageProcessor(logger).ProcessMessage(testMessage1);
					NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusHeader.PK), $"when {eventType} and {status.Key}");
					var cusHead1 = Factory.Load<CusEntryHeader>(testMessage1.EM_LinkUniqueID);
					NUnit.Framework.Assert.That(cusHeader.CH_Status, NUnit.Framework.Is.EqualTo(status.Value).Using(CustomComparers.TypeComparison), $"when {eventType} and {status.Key}");
				}
			}
		}

		[ExpectNoExceptions]
		public void TestProcessManifestDeliveryNotificationWithNoCorrespondingBill()
		{
			var testMessage = CreateMessage(GetTWNotification("SNT", "FHM", "1234567", "NUM1"), "FCF");
			var logger = new LoggingInformationForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Discarded).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(logger.LogMessages.ToString(), NUnit.Framework.Does.Contain("Can not find the corresponding Manifest Bill for Message Number: TWIN1, Entry Number: 1234567, Entry Type: FHM"));
		}

		[ExpectNoExceptions]
		public void TestProcessManifestDeliveryNotificationWithoutSupported()
		{
			var testMessage = CreateMessage(GetTWNotification("SNN", "FHM", "1234567", "NUM1"), "FCF");
			var logger = new LoggingInformationForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Discarded).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(logger.LogMessages.ToString(), NUnit.Framework.Does.Contain("Event Type or Entry Type is not supported for Message Number: TWIN1, Entry Number: 1234567, Entry Type: FHM"));

			testMessage = CreateMessage(GetTWNotification("SNT", "FHH", "1234567", "NUM1"), "FCF");
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Discarded).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(logger.LogMessages.ToString(), NUnit.Framework.Does.Contain("Event Type or Entry Type is not supported for Message Number: TWIN1, Entry Number: 1234567, Entry Type: FHM"));
		}

		[ExpectNoExceptions]
		public void TestProcessManifestDeliveryNotificationWithNoCorrespondingControllingMessageHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_FunctionalReferenceId = "22099131002309220001";
			Factory.Save();

			var testMessage = CreateMessage(GetTWNotification("SNT", "NXM", "22099131002309220002", "00000000001305020580", "101"), "101");
			var logger = new LoggingInformationForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Discarded).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(logger.LogMessages.ToString(), NUnit.Framework.Does.Contain("Can not find the corresponding Controlling Message Header for Message Number: TWIN1, Functional Reference ID: 22099131002309220002, Message Type: 101"));
			});
		}

		[ExpectNoExceptions]
		public void TestProcessControllingAgencyNotification()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_FunctionalReferenceId = "22099131002309220001";
			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader2.TW1_FunctionalReferenceId = "23322708001106180001";
			Factory.Save();

			var testMessage = CreateMessage(GetTWNotification("SNT", "NXM", "22099131002309220001", "00000000001305020580", "101"), "101");
			var logger = new LoggingInformationForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader1.TW1_MessageStatus, NUnit.Framework.Is.EqualTo("SNT").Using(CustomComparers.TypeComparison), "messageHeader1 of MessageStatus should be SNT.");
				NUnit.Framework.Assert.That(messageHeader2.TW1_MessageStatus, NUnit.Framework.Is.EqualTo("NOT").Using(CustomComparers.TypeComparison), "messageHeader2 of MessageStatus should be NOT.");
			});

			testMessage = CreateMessage(GetTWNotification("ERR", "NXM", "23322708001106180001", "00000000001305020581", "201"), "201");
			processor.ProcessMessage(testMessage);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageHeader1.TW1_MessageStatus, NUnit.Framework.Is.EqualTo("SNT").Using(CustomComparers.TypeComparison), "messageHeader1 of MessageStatus should be SNT.");
				NUnit.Framework.Assert.That(messageHeader2.TW1_MessageStatus, NUnit.Framework.Is.EqualTo("ERR").Using(CustomComparers.TypeComparison), "messageHeader2 of MessageStatus should be ERR.");
			});
		}

		[ExpectNoExceptions]
		public void TestABL_MessageStatusWhenProcessManifestDeliveryNotification()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bills = header.Bills;
			var bill1 = bills.AddNew();
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = bill1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "FHM";
			cusNum1.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "1234567";
			cusNum1.CE_EntryStatus = ZString.Empty;

			var bill2 = bills.AddNew();
			var cusNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = bill2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = "FHM";
			cusNum2.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "1234567";
			cusNum2.CE_EntryStatus = ZString.Empty;
			Factory.Save();

			var testMessage = CreateMessage(GetTWNotification("SNT", "FHM", "1234567", "NUM1"), "FCF");
			var logger = new LoggingInformationForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(bills.Cast<AsycudaBill>().All(c => c.ABL_MessageStatus == TWMessageStatusCodeList.Codes.Sent), NUnit.Framework.Is.True, "all bill of ABL_MessageStatus should be SNT");
			NUnit.Framework.Assert.That(header.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Sent).Using(CustomComparers.TypeComparison), "header of AMA_MessageStatus should be SNT");

			testMessage = CreateMessage(GetTWNotification("ERR", "FHM", "1234567", "NUM1"), "FCF");
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(bills.Cast<AsycudaBill>().All(c => c.ABL_MessageStatus == TWMessageStatusCodeList.Codes.TransmissionError), NUnit.Framework.Is.True, "all bill of ABL_MessageStatus should be ERR");
			NUnit.Framework.Assert.That(header.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.TransmissionError).Using(CustomComparers.TypeComparison), "header of AMA_MessageStatus should be ERR");

			testMessage = CreateMessage(GetTWNotification("SNT", "FHH", "1234567", "NUM1"), "FCF");
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(bills.Cast<AsycudaBill>().All(c => c.ABL_MessageStatus == "ERR"), NUnit.Framework.Is.True, "all bill of ABL_MessageStatus should be ERR");
			NUnit.Framework.Assert.That(header.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.TransmissionError).Using(CustomComparers.TypeComparison), "header of AMA_MessageStatus should be ERR");

			testMessage = CreateMessage(GetTWNotification("SNT", "FHM", "123456", "NUM1"), "FCF");
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(bills.Cast<AsycudaBill>().All(c => c.ABL_MessageStatus == "ERR"), NUnit.Framework.Is.True, "all bill of ABL_MessageStatus should be ERR");
			NUnit.Framework.Assert.That(header.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.TransmissionError).Using(CustomComparers.TypeComparison), "header of AMA_MessageStatus should be ERR");

			testMessage = CreateMessage(GetTWNotification("SNT", "FHM", "1234567", "NUM1"), "FCC");
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(bills.Cast<AsycudaBill>().All(c => c.ABL_MessageStatus == "ERR"), NUnit.Framework.Is.True, "all bill of ABL_MessageStatus should be ERR");
			NUnit.Framework.Assert.That(header.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.TransmissionError).Using(CustomComparers.TypeComparison), "header of AMA_MessageStatus should be ERR");

			cusNum2.CE_EntryNum = "1234566";
			Factory.Save();
			testMessage = CreateMessage(GetTWNotification("SNT", "FHM", "1234567", "NUM1"), "FCF");
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(bill1.ABL_MessageStatus, NUnit.Framework.Is.EqualTo("SNT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(bill2.ABL_MessageStatus, NUnit.Framework.Is.EqualTo("ERR").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(header.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.TransmissionError).Using(CustomComparers.TypeComparison), "header of AMA_MessageStatus should be ERR");

			var logs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageReceived.Code));
			AssertEquals(3, logs.Length);
			AssertEquals("FCF", logs[0].SL_Reference);
		}

		[ExpectNoExceptions]
		public void TestCH_EntryStatusWhenTPC()
		{
			var filename = "N5110.xml";
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var testMessage1 = CreateMessage(GetMessageText(filename, number1, null, null, null, null), "TPC");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage1);
			NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
			var cusHead1 = Factory.Load<CusEntryHeader>(testMessage1.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo("TPC").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUpdateEntryStatusToTPCAndTAD()
		{
			var filename = "N5116.xml";
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var testMessage1 = CreateMessage(GetMessageText(filename, number1, null, null, null, null), "IRM");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage1);
			var cusHead1 = Factory.Load<CusEntryHeader>(testMessage1.EM_LinkUniqueID);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
				NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo("IRM").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(cusHead1.Declaration.Logs.Find(log => log.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && log.SL_Reference == "IRM").Count(), NUnit.Framework.Is.EqualTo(1), "One event for IRM added");
			});

			cusHead1.CH_EntryStatus = "CAA";
			Factory.Save();

			filename = "N5110.xml";
			testMessage1 = CreateMessage(GetMessageText(filename, number1, null, null, null, null), "TPC");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage1);
			cusHead1 = Factory.Load<CusEntryHeader>(testMessage1.EM_LinkUniqueID);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
				NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo("CAA").Using(CustomComparers.TypeComparison));
				var logs = cusHead1.Declaration.Logs;
				NUnit.Framework.Assert.That(logs.Find(log => log.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && log.SL_Reference == "TPC").Count(), NUnit.Framework.Is.EqualTo(0), "No TPC event logged");
				NUnit.Framework.Assert.That(logs.Find(log => log.SL_SE_NKEvent == Events.CustomsReadyToPay.Code && log.SL_Reference == "N5110 message received").Count(), NUnit.Framework.Is.EqualTo(1), "One event for N5110 CRP added");
			});
		}

		[ExpectNoExceptions]
		public void TestCH_EntryStatusAndCH_EntryReleaseDateWhenERMorIRM()
		{
			var testMap = new Dictionary<string, string>();
			testMap["IRM"] = "N5116.xml";
			testMap["ERM"] = "N5204.xml";
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var releaseDateTime = new ZDateTime(2011, 05, 23, 9, 30, 50);
			foreach (var messageTypeAndFileName in testMap)
			{
				var messageType = messageTypeAndFileName.Key;
				var fileName = messageTypeAndFileName.Value;
				var cusHead = messageType == "ERM" ? cusHead2 : this.cusHead1;
				var number = cusHead.EntryNumber;
				var testMessage = CreateMessage(GetMessageText(fileName, number, null, "C1", null, null, "2011-05-23T09:30:50"), messageType);
				var testMessage1 = CreateMessage(GetMessageText(fileName, number, null, "C2", null, null, "2011-05-23T09:30:50"), messageType);
				var testMessage2 = CreateMessage(GetMessageText(fileName, number, null, "XX", null, null, "2011-05-23T09:30:50"), messageType);
				Factory.Save();
				new TWMessageProcessor(logger).ProcessMessage(testMessage);
				NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusHead.PK), messageType);
				var cusHead1 = Factory.Load<CusEntryHeader>(testMessage.EM_LinkUniqueID);
				NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo(messageType).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(cusHead1.CH_EntryReleaseDate.CompareTo(releaseDateTime), NUnit.Framework.Is.EqualTo(0));
				cusHead1.CH_EntryReleaseDate = ZDateTime.Empty;
				new TWMessageProcessor(logger).ProcessMessage(testMessage1);
				NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusHead.PK));
				cusHead1 = Factory.Load<CusEntryHeader>(testMessage1.EM_LinkUniqueID);
				NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo(messageType).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(cusHead1.CH_EntryReleaseDate.CompareTo(releaseDateTime), NUnit.Framework.Is.EqualTo(0));
				cusHead1.CH_EntryReleaseDate = ZDateTime.Empty;
				new TWMessageProcessor(logger).ProcessMessage(testMessage2);
				NUnit.Framework.Assert.That(testMessage2.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusHead.PK));
				cusHead1 = Factory.Load<CusEntryHeader>(testMessage2.EM_LinkUniqueID);
				NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo(messageType).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(cusHead1.CH_EntryReleaseDate.CompareTo(releaseDateTime), NUnit.Framework.Is.EqualTo(0));
			}
		}

		[ExpectNoExceptions]
		public void TestCH_DutyDueDateWhenTPC()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var dutyDueDate = new ZDateTime(2011, 05, 23);
			var testMessage = CreateMessage(GetMessageText("N5110.xml", number1, null, "C1", null, null, null, true, "2011-05-23"), "TPC");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
			var cusHead1 = Factory.Load<CusEntryHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.CH_DutyDueDate, NUnit.Framework.Is.EqualTo(dutyDueDate));
			testMessage = CreateMessage(GetMessageText("N5110.xml", number1, null, "C1", null, null, null, true, ""), "TPC");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
			cusHead1 = Factory.Load<CusEntryHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.CH_DutyDueDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
		}

		[ExpectNoExceptions]
		public void TestCH_EntryStatusWhenRFM()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var filename = "N5107.xml";
			var messageType = "RFM";
			var testMessage = CreateMessage(GetMessageText(filename, number1, "1", "C3M", "1", null), messageType);
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
			var cusHead1 = Factory.Load<CusEntryHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo("RFM").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCH_EntryStatusWhenIEM()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var filename = "N5109.xml";
			var testMessage = CreateMessage(GetMessageText(filename, number1, null, null, null, null), "IEM");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
			var cusHead1 = Factory.Load<CusEntryHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo("IEM").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCH_EntryStatusWhenUHC()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var filename = "N5168.xml";
			var testMessage = CreateMessage(GetMessageText(filename, number1, null, null, null, null), "UHC");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
			var cusHead1 = Factory.Load<CusEntryHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.CH_EntryStatus, NUnit.Framework.Is.EqualTo("UHC").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMessageIsTranshipment()
		{
			var testMap = new Dictionary<string, string>();
			testMap["ARM"] = "NX5106.xml";
			testMap["IEM"] = "N5109.xml";
			testMap["RFM"] = "N5107.xml";
			testMap["TRN"] = "N5302.xml";
			testMap["UHC"] = "N5168.xml";
			foreach (var messageTypeAndFileName in testMap)
			{
				var fileName = messageTypeAndFileName.Value;
				var messageType = messageTypeAndFileName.Key;
				var procedure = messageType == "TRN" ? null : "3";
				var testMessage = CreateMessage(GetMessageText(fileName, inNumber1, procedure, null, null, null), messageType);
				NUnit.Framework.Assert.That(testMessage.IsTranshipment, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), string.Format(CultureInfo.InvariantCulture, "{0}({1})  is Transhipment", fileName, messageType));
			}
		}

		[ExpectNoExceptions]
		public void TestBH_ReleaseStatusWhenARM()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var filename = "NX5106.xml";
			var testMessage = CreateMessage(GetMessageText(filename, cusInBondHead1.EntryNumber, "3", null, null, null), "ARM");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusInBondHead1.PK));
			var cusHead1 = Factory.Load<CusInBondHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.BH_ReleaseStatus, NUnit.Framework.Is.EqualTo("ARM").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBH_ReleaseStatusWhenRFM()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var filename = "N5107.xml";
			var testMessage = CreateMessage(GetMessageText(filename, cusInBondHead1.EntryNumber, "3", null, null, null), "RFM");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusInBondHead1.PK));
			var cusHead1 = Factory.Load<CusInBondHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.BH_ReleaseStatus, NUnit.Framework.Is.EqualTo("RFM").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBH_ReleaseStatusWhenTRN()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var filename = "N5302.xml";
			var testMessage = CreateMessage(GetMessageText(filename, cusInBondHead1.EntryNumber, null, "C1", null, null), "TRN");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusInBondHead1.PK));
			var cusHead1 = Factory.Load<CusInBondHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead1.BH_ReleaseStatus, NUnit.Framework.Is.EqualTo("C1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLinkCusInBondHeader()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var messageType = "TRN";
			var filename = "N5302.xml";
			var testMessage1 = CreateMessage(GetMessageText(filename, cusInBondHead1.EntryNumber, null, "C1", null, null), messageType);
			var testMessage2 = CreateMessage(GetMessageText(filename, cusInBondHead2.EntryNumber, null, "C2", null, null), messageType);
			var testMessage3 = CreateMessage(GetMessageText(filename, "66666", null, "C2", null, null), messageType);
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage1);
			NUnit.Framework.Assert.That(testMessage1.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusInBondHead1.PK));
			var cusHead = Factory.Load<CusInBondHeader>(testMessage1.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead.BH_ReleaseStatus, NUnit.Framework.Is.EqualTo("C1").Using(CustomComparers.TypeComparison));
			new TWMessageProcessor(logger).ProcessMessage(testMessage2);
			NUnit.Framework.Assert.That(testMessage2.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusInBondHead2.PK));
			cusHead = Factory.Load<CusInBondHeader>(testMessage2.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(cusHead.BH_ReleaseStatus, NUnit.Framework.Is.EqualTo("C2").Using(CustomComparers.TypeComparison));
			new TWMessageProcessor(logger).ProcessMessage(testMessage3);
			NUnit.Framework.Assert.That(testMessage3.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(ZGuid.Empty));
		}

		[ExpectNoExceptions]
		public void TestSetEM_MessageInterpretationWhenProcessTranshipmentMessage()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var messageType = MessageTypeList.Codes.TRN;
			var filename = "N5302.xml";
			var testMessage = CreateMessage(GetMessageText(filename, cusInBondHead1.EntryNumber, null, "C1", null, null), messageType);
			Factory.Save();

			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			var expectedString = TWMessageHelper.NewIncomingHelper(testMessage).ToHtml();
			NUnit.Framework.Assert.That(testMessage.EM_MessageInterpretation, NUnit.Framework.Is.EqualTo(expectedString).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUpdateBH_MessageStatusWhenProcessTranshipmentMessage()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var filename = "N5302.xml";
			var testMessage = CreateMessage(GetMessageText(filename, cusInBondHead1.EntryNumber, null, "C1", null, null), MessageTypeList.Codes.TRN);
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(cusInBondHead1.PK));
			var inBondHeader = Factory.Load<CusInBondHeader>(testMessage.EM_LinkUniqueID);
			NUnit.Framework.Assert.That(inBondHeader.BH_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Acknowledged).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestProcessTransferApplicationFromEHubNotificationWithoutCorrespondingTranshipment()
		{
			var testMessage = CreateMessage(GetTWNotification("SNT", "TRS", "AWDT1112300004", "00000000001004230159"), MessageTypeList.Codes.TRA);
			var logger = new LoggingInformationForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Discarded).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(logger.LogMessages.ToString(), NUnit.Framework.Does.Contain("Can not find the corresponding Transhipment Entry Header for Message Number: TWIN1, Entry Number: AWDT1112300004, Entry Type: TRS"));
		}

		[ExpectNoExceptions]
		public void TestProcessTransferApplicationFromEHubNotificationWhenEventTypeOrEntryTypeIsNotSupported()
		{
			var testMessage = CreateMessage(GetTWNotification("SNN", "TRS", "AWDT1112300004", "00000000001004230159"), MessageTypeList.Codes.TRA);
			var logger = new LoggingInformationForTesting();
			var processor = new TWMessageProcessor(logger);
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Discarded).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(logger.LogMessages.ToString(), NUnit.Framework.Does.Contain("Event Type or Entry Type is not supported for Message Number: TWIN1, Entry Number: AWDT1112300004, Entry Type: TRS"));

			testMessage = CreateMessage(GetTWNotification("SNT", "TRP", "AWDT1112300004", "00000000001004230159"), MessageTypeList.Codes.TRA);
			processor.ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_Status, NUnit.Framework.Is.EqualTo(EDIMessageStatusList.Codes.Discarded).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(logger.LogMessages.ToString(), NUnit.Framework.Does.Contain("Event Type or Entry Type is not supported for Message Number: TWIN1, Entry Number: AWDT1112300004, Entry Type: TRP"));
		}

		[ExpectNoExceptions]
		public void TestProcessTransferApplicationFromEHubNotification()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var testMessage = CreateMessage(GetTWNotification("SNT", "TRS", cusInBondHead1.EntryNumber, "00000000001004230159"), MessageTypeList.Codes.TRA);
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(cusInBondHead1.BH_MessageStatus, NUnit.Framework.Is.EqualTo("SNT").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddCESEventWhenUpdateEntryStatus()
		{
			var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var testMessage = CreateMessage(GetMessageText("N5107.xml", number1, "1", "C3M", "1", null), "RFM");
			Factory.Save();
			new TWMessageProcessor(logger).ProcessMessage(testMessage);
			NUnit.Framework.Assert.That(testMessage.EM_LinkUniqueID, NUnit.Framework.Is.EqualTo(this.cusHead1.PK));
			var cusHead1 = Factory.Load<CusEntryHeader>(testMessage.EM_LinkUniqueID);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(cusHead1.Declaration.Logs.Find(log => log.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && log.SL_Reference == "RFM").Count(), NUnit.Framework.Is.EqualTo(1), "one event for RFM added");

				var testMessage2 = CreateMessage(GetMessageText("N5107.xml", number1, "2", "C3M", "1", null), "RFM");
				Factory.Save();
				new TWMessageProcessor(logger).ProcessMessage(testMessage);
				cusHead1 = Factory.Load<CusEntryHeader>(testMessage.EM_LinkUniqueID);
				NUnit.Framework.Assert.That(cusHead1.Declaration.Logs.Find(log => log.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && log.SL_Reference == "RFM").Count(), NUnit.Framework.Is.EqualTo(2), "one event for RFM added again");
			});
		}

		TWMessage CreateMessage(string messageText, string messageType)
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = messageText;
			testMessage.EM_MessageType = messageType;
			return testMessage;
		}

		string number1 = "AAB1082348";
		string number2 = "AAB1092348";
		const string inNumber1 = "INNO1";
		const string inNumber2 = "INNO2";
		CusEntryHeader cusHead1;
		CusEntryHeader cusHead2;
		CusInBondHeader cusInBondHead1;
		CusInBondHeader cusInBondHead2;
		protected override void SetUp()
		{
			base.SetUp();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CompanyData.OB_CusPaidBy = PaidByCodeList.Codes.BRK;
			var org2Code = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "AA";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			var entryInstruction1 = declaration.CusEntryInstruction;
			entryInstruction1.CEI_Style = "B1";
			entryInstruction1.CEI_CustomsOffice = "AA";
			entryInstruction1.CEI_DateForDuty = new ZDateTime(2019, 01, 01);
			entryInstruction1.CEI_BoxNumber = "123";
			cusHead1 = declaration.CustomsEntryHeaders.AddNew();
			cusHead1.CH_CEI_Instruction = entryInstruction1.PK;
			cusHead1.CH_JE = declaration.PK;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "IMP";
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = number1;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = "EXP";
			declaration2.JE_CustomsOffice = "AA";
			declaration2.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			var entryInstruction2 = declaration2.CusEntryInstruction;
			entryInstruction2.CEI_Style = "B1";
			entryInstruction2.CEI_CustomsOffice = "AA";
			entryInstruction2.CEI_DateForDuty = new ZDateTime(2020, 01, 01);
			entryInstruction2.CEI_BoxNumber = "123";
			cusHead2 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead2.CH_CEI_Instruction = entryInstruction2.PK;
			cusHead2.CH_JE = declaration2.PK;
			var cusNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = cusHead2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = "EXP";
			cusNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = number2;
			cusInBondHead1 = Factory.NewWithValidTestData<CusInBondHeader>();
			cusInBondHead1.ReceiptOffice = "AA";
			cusInBondHead1.UnladingOffice = "BB";
			cusInBondHead1.TW_BoxNumber = "123";
			var cusNum3 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum3.CE_ParentID = cusInBondHead1.PK;
			cusNum3.CE_EntryType = "TRS";
			cusNum3.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum3.CE_EntryNum = inNumber1;
			cusInBondHead2 = Factory.NewWithValidTestData<CusInBondHeader>();
			cusInBondHead2.ReceiptOffice = "AA";
			cusInBondHead2.UnladingOffice = "BB";
			cusInBondHead2.TW_BoxNumber = "123";
			var cusNum4 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum4.CE_ParentID = cusInBondHead2.PK;
			cusNum4.CE_EntryType = "TRS";
			cusNum4.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum4.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum4.CE_EntryNum = inNumber2;
			Factory.Save();
			number1 = cusHead1.EntryNumber;
			number2 = cusHead2.EntryNumber;
			AssertNotNullOrEmpty(number1);
			AssertNotNullOrEmpty(number2);
		}
	}
}
