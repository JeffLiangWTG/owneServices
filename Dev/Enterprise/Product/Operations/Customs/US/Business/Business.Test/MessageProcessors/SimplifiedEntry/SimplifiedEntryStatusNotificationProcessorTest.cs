using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	sealed class SimplifiedEntryStatusNotificationProcessorTest : ABIProcessorTest<SimplifiedEntryStatusNotificationProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestGetKeysForBlockingParallelProcessing_FoundMatchingData()
		{
			var declaration = GetDeclaration("71002057");
			declaration.JE_MasterBill = "FSDFS324234";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "71005415";
			Factory.Save();

			var message = CreateStatusMessage(
				"B001101SV9SO                                                                    " +
				"SO101101SV9  71005415 01123-45-6789                                    1        " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000425     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1025141512    " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000261     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1026141512    " +
				"SO60093013012597ADMISSIBLE                                                      " +
				"SO60093013014298RELEASED                                09301301                " +
				"SO70ACE000123013143912EPA DATA REVIEW                         2  0101000001     " +
				"SO70ACE000123013143901PGA DATA ACCEPTED                       2  0101000001     " +
				"Y  1101SV9SO00000");
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new SimplifiedEntryStatusNotificationProcessor();
			var provider = processor as IKeysForBlockingParallelProcessingProvider;
			var actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			var actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions(() =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(entry.TableName, declaration.ActiveEntryHeaders.SimplifiedEntry.PK, GlbBranch.CurrentBranch.PK, declaration.JE_DeclarationReference)), actualMetaData);
				AssertArrayEqualsByElements("Keys", new[] { $"Entry:SV9-71005415|{GlbCompany.CurrentCompany.PK}" }, actualKeys.ReturnValue.Keys.ToArray());
			});
		}

		public void TestGetKeysForBlockingParallelProcessing_MissingMatchingData()
		{
			var message = CreateStatusMessage(
				"B001101SV9SO                                                                    " +
				"SO101101SV9  71005415 01123-45-6789                                    1        " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000425     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1025141512    " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000261     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1026141512    " +
				"SO60093013012597ADMISSIBLE                                                      " +
				"SO60093013014298RELEASED                                09301301                " +
				"SO70ACE000123013143912EPA DATA REVIEW                         2  0101000001     " +
				"SO70ACE000123013143901PGA DATA ACCEPTED                       2  0101000001     " +
				"Y  1101SV9SO00000");
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new SimplifiedEntryStatusNotificationProcessor();
			var provider = processor as IKeysForBlockingParallelProcessingProvider;
			var actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			var actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions(() =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.Empty), actualMetaData);
				AssertEquals(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder, actualKeys.ReturnValue.ResultType);
			});
		}
		public void TestEmailDoesNotContainSSNNumber()
		{
			var declaration = GetDeclaration("71002057");

			declaration.JE_MasterBill = "FSDFS324234";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "71005415";

			Factory.Save();

			var message = CreateStatusMessage(
				"B001101SV9SO                                                                    " +
				"SO101101SV9  71005415 01123-45-6789                                    1        " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000425     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1025141512    " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000261     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1026141512    " +
				"SO60093013012597ADMISSIBLE                                                      " +
				"SO60093013014298RELEASED                                09301301                " +
				"SO70ACE000123013143912EPA DATA REVIEW                         2  0101000001     " +
				"SO70ACE000123013143901PGA DATA ACCEPTED                       2  0101000001     " +
				"Y  1101SV9SO00000");

			Factory.Save();

			message = CreateStatusMessage(
				"B001101SV9SO                                                                    " +
				"SO101101SV9  71005415 0123-456789012                                   1        " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000425     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1025141512    " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000261     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1026141512    " +
				"SO60093013012597ADMISSIBLE                                                      " +
				"SO60093013014298RELEASED                                09301301                " +
				"SO70ACE000123013143912EPA DATA REVIEW                         2  0101000001     " +
				"SO70ACE000123013143901PGA DATA ACCEPTED                       2  0101000001     " +
				"Y  1101SV9SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(2, emails.Count);
			Assert("Email does not contain SSN Number", !emails[0].Body.Contains("<td>Importer of Record Number</td><td>123-45-6789</td>"));
			Assert("EIN Number or CBP Number included in the mail", emails[1].Body.Contains("<td>Importer of Record Number</td><td>23-456789012</td>"));
		}

		public void TestHLDOrEXMStatusWhenBillStatus4CClearedAfter1A()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);

			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates, allowDuplicates: true);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, "IsExamDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, "IsHoldExamRemovedDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1A = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1A", "1A DESC", startDate, endDate);
			var code4C = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "4C", "4C DESC", startDate, endDate);

			var attribute1 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1A.PK, attributeName1.ZXE_Name, "4C");
			var attribute2 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1A.PK, attributeName1.ZXE_Name, "1B");
			var attribute3 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1A.PK, attributeName2.ZXE_Name, "Y");
			var attribute4 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4C.PK, attributeName3.ZXE_Name, "Y");
			Factory.Save();

			var declaration = GetDeclaration("71002057");
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "00591325206";
			bill1.CU_BillType = "HB";
			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1                            MOLU13700107330                        YY2       Y  3901SV9CQ00001";
			entry.Messages.Add(outgoing);

			var c1message = Factory.New<MQEDIMessage>();
			c1message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message.EM_MessageNum = "HYEDUSCMT_159673";
			c1message.EM_Status = EDIMessage.Status.Queued;
			c1message.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
										 "WR12704            01                     APLUAA TEST1            001T 121414   " +
										 "WR4            13700107330 00591325206             00000034KG   MOLUMOLUMN1     " +
										 "WR5022924105572 CBPA INSPECTION/DOC REVIEW HOLD                  5000     001   " +
										 "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("", reLoadBill1.CU_MessageStatus);
			AssertEquals("N", reLoadBill1.HLDOrEXMStatus);

			var message = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO5010011604471AENTERED - INTENSIVE EXAM REQUIRED       YQTR 727  1201161108    " +
				"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("1A", reLoadBill1.CU_MessageStatus);

			var message3 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000009     00000011" +
				"SO5011011607474COVERRIDE TO GENERAL                     YQTR 727  1201161108    " +
				"SO60110915080990UNDER CBP REVIEW                                                " +
				"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadBill2 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("", reLoadBill2.CU_MessageStatus);
			AssertEquals("N", reLoadBill2.HLDOrEXMStatus);
		}

		public void TestGetEntryHeaderWithPGADetailFromEntrySummaryMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = false;
			dec.US_CertifyCargoRelease = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.ImportEntryNumber = "71002057";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";
			dec.JE_OH_Importer = importer.PK;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_FDAIndicator = "D";

			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = "FOO";
			fdaLine.US_LineNo = 1;

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new ImportMessageSendingActionCollection(dec, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(ensEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			var ensMessageBuilder = new ACEEntrySummaryMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Add);
			var outgoingMessage = ensMessageBuilder.PopulateMessage();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			responseMessage.EM_Status = EDIMessage.Status.Received;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";
			responseMessage.EM_MessageText =
					"B001101SV9AX                                               HYEDUSCMT_148588     " +
					"E0 SUMMRY 000001 REF ID: SV9 71002057 B00155595                                 " +
					"E0 LINITM 000001 REF ID: 001                                                    " +
					"E0 TARIFF 000001 REF ID: 4703110000                                             " +
					"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  71002057     B00155595   " +
					"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  71002057     B00155595   " +
					"Y  1101SV9AX00005";

			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.Messages.Add(outgoingMessage);
			ensEntry.Messages.Add(responseMessage);
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_148588";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(ensEntry.PK, reLoadJob.GetEntryHeaderWithPGADetail().PK);
		}

		public void TestGetEntryHeaderWithPGADetailFromCargoReleaseMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = false;
			dec.US_EnableCRL = true;
			dec.US_CertifyCargoRelease = false;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.ImportEntryNumber = "71002057";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";
			dec.JE_OH_Importer = importer.PK;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_FDAIndicator = "D";

			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = "FOO";
			fdaLine.US_LineNo = 1;

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var seEntry = dec.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(seEntry);
			var collection = new ImportMessageSendingActionCollection(dec, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var outgoingMessage = builder.PopulateMessage();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Received;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148191";
			responseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_164692     SE10ASV9  71002057 01EI 58-12345678911800000100001101  1101                     SE15RAPLUMST081515A                                        00000100CS   N       SE20CR B00164172                                                                SE40001HK CYCLOHEXYLDIAZENIUMDIOXY                                              SE6029021100000000010000                                                        OI        CYCLOHEXYLDIAZENIUMDIOXY                                              PG01001EPAPS1   Y                        130.027                                SE9013PH6   MISSING DIS DOCUMENTATION                                           PG02PCAS 1234                                                                   PG04YCYCLOHEXYLDIAZENIUMDIOXY                                            1000000PG07ACME                                                                        PG14 EP8241-420                                                                 PG19EPN   012345CHN002                                                          PG19CB                   U.S. DEMO COMPANY               184 TEST STREET        PG20                                     CHICAGO              IL US60056        PG21CB CRAIG SEELIG           2154441234     CRAIG.SEELIG@TEST.COM              PG55NP                                                                          PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21IM CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG55CI                                                                          PG19OV                   MARITIME INDUSTRIAL SHIPPERS    600 PORT STREET        PG20                                     NEWARK               NJ US20034        PG21OV JAY SMITH              14101123456    JAY.SMITH@MARITIMESHIPPERS.COM     PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROADPG20                                     HONG KONG               HK             PG21DEQJOHN SMITH             5555555555     JOHN.SMITH@COMPANY.COM             PG19LG                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21LG CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG22 944         CI EP3                                                         PG261000000100000DR                                                             PG262                                                                           PG29KG 000000100000                                                             SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00044                                                               ";
			seEntry.Messages.Add(responseMessage);
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_148191";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(seEntry.PK, reLoadJob.GetEntryHeaderWithPGADetail().PK);
		}

		public void TestGetEntryHeaderWithPGADetailFromCargoReleaseCertifiedFromEntrySummary()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = false;
			dec.US_CertifyCargoRelease = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.ImportEntryNumber = "71002057";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";
			dec.JE_OH_Importer = importer.PK;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_FDAIndicator = "D";

			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = "FOO";
			fdaLine.US_LineNo = 1;

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new ImportMessageSendingActionCollection(dec, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(ensEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			var ensMessageBuilder = new ACEEntrySummaryMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Add);
			var outgoingMessage = ensMessageBuilder.PopulateMessage();
			var entrySummaryResponseMessage = Factory.New<MQEDIMessage>();
			entrySummaryResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			entrySummaryResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryResponseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			entrySummaryResponseMessage.EM_Status = EDIMessage.Status.Received;
			entrySummaryResponseMessage.EM_MessageNum = "HYEDUSCMT_148588";
			entrySummaryResponseMessage.EM_MessageText =
					"B001101SV9AX                                               HYEDUSCMT_148588     " +
					"E0 SUMMRY 000001 REF ID: SV9 71002057 B00155595                                 " +
					"E0 LINITM 000001 REF ID: 001                                                    " +
					"E0 TARIFF 000001 REF ID: 4703110000                                             " +
					"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  71002057     B00155595   " +
					"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  71002057     B00155595   " +
					"Y  1101SV9AX00005";

			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.Messages.Add(outgoingMessage);
			ensEntry.Messages.Add(entrySummaryResponseMessage);

			var seEntry = dec.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(seEntry);
			var cargoReleaseResponseMessage = Factory.New<MQEDIMessage>();
			cargoReleaseResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cargoReleaseResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cargoReleaseResponseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			cargoReleaseResponseMessage.EM_Status = EDIMessage.Status.Received;
			cargoReleaseResponseMessage.EM_MessageNum = "HYEDUSCMT_148588";
			cargoReleaseResponseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_148588     SE10ASV9  71002057 01EI 58-12345678911800000100001101  1101                     SE15RAPLUMST081515A                                        00000100CS   N       SE20CR B00164172                                                                SE40001HK CYCLOHEXYLDIAZENIUMDIOXY                                              SE6029021100000000010000                                                        OI        CYCLOHEXYLDIAZENIUMDIOXY                                              PG01001EPAPS1   Y                        130.027                                SE9013PH6   MISSING DIS DOCUMENTATION                                           PG02PCAS 1234                                                                   PG04YCYCLOHEXYLDIAZENIUMDIOXY                                            1000000PG07ACME                                                                        PG14 EP8241-420                                                                 PG19EPN   012345CHN002                                                          PG19CB                   U.S. DEMO COMPANY               184 TEST STREET        PG20                                     CHICAGO              IL US60056        PG21CB CRAIG SEELIG           2154441234     CRAIG.SEELIG@TEST.COM              PG55NP                                                                          PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21IM CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG55CI                                                                          PG19OV                   MARITIME INDUSTRIAL SHIPPERS    600 PORT STREET        PG20                                     NEWARK               NJ US20034        PG21OV JAY SMITH              14101123456    JAY.SMITH@MARITIMESHIPPERS.COM     PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROADPG20                                     HONG KONG               HK             PG21DEQJOHN SMITH             5555555555     JOHN.SMITH@COMPANY.COM             PG19LG                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21LG CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG22 944         CI EP3                                                         PG261000000100000DR                                                             PG262                                                                           PG29KG 000000100000                                                             SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00044                                                               ";
			seEntry.Messages.Add(cargoReleaseResponseMessage);
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_148588";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(ensEntry.PK, reLoadJob.GetEntryHeaderWithPGADetail().PK);
		}

		public void TestGetEntryHeaderForPGACorrectionFromEntrySummaryMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = false;
			dec.US_CertifyCargoRelease = false;
			dec.US_PGAExpeditedRelease = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.ImportEntryNumber = "71002057";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";
			dec.JE_OH_Importer = importer.PK;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_FDAIndicator = "D";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new ImportMessageSendingActionCollection(dec, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(ensEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = false;
			action.US_AcknowledgeAndSign = true;
			var ensMessageBuilder = new ACEEntrySummaryMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Add);
			var outgoingMessage = ensMessageBuilder.PopulateMessage();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			responseMessage.EM_Status = EDIMessage.Status.Received;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";
			responseMessage.EM_MessageText =
					"B001101SV9AX                                               HYEDUSCMT_148588     " +
					"E0 SUMMRY 000001 REF ID: SV9 71002057 B00155595                                 " +
					"E0 LINITM 000001 REF ID: 001                                                    " +
					"E0 TARIFF 000001 REF ID: 4703110000                                             " +
					"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  71002057     B00155595   " +
					"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  71002057     B00155595   " +
					"Y  1101SV9AX00005";

			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.Messages.Add(outgoingMessage);
			ensEntry.Messages.Add(responseMessage);
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_148588";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(ensEntry.PK, reLoadJob.GetEntryHeaderForPGACorrection().PK);
		}

		public void TestGetEntryHeaderForPGACorrectionFromCargoReleaseMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = false;
			dec.US_EnableCRL = true;
			dec.US_CertifyCargoRelease = false;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.ImportEntryNumber = "71002057";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";
			dec.JE_OH_Importer = importer.PK;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_FDAIndicator = "D";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var seEntry = dec.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(seEntry);
			var collection = new ImportMessageSendingActionCollection(dec, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var outgoingMessage = builder.PopulateMessage();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Received;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148191";
			responseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_164692     SE10ASV9  71002057 01EI 58-12345678911800000100001101  1101                     SE15RAPLUMST081515A                                        00000100CS   N       SE20CR B00164172                                                                SE40001HK CYCLOHEXYLDIAZENIUMDIOXY                                              SE6029021100000000010000                                                        OI        CYCLOHEXYLDIAZENIUMDIOXY                                              PG01001EPAPS1   Y                        130.027                                SE9013PH6   MISSING DIS DOCUMENTATION                                           PG02PCAS 1234                                                                   PG04YCYCLOHEXYLDIAZENIUMDIOXY                                            1000000PG07ACME                                                                        PG14 EP8241-420                                                                 PG19EPN   012345CHN002                                                          PG19CB                   U.S. DEMO COMPANY               184 TEST STREET        PG20                                     CHICAGO              IL US60056        PG21CB CRAIG SEELIG           2154441234     CRAIG.SEELIG@TEST.COM              PG55NP                                                                          PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21IM CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG55CI                                                                          PG19OV                   MARITIME INDUSTRIAL SHIPPERS    600 PORT STREET        PG20                                     NEWARK               NJ US20034        PG21OV JAY SMITH              14101123456    JAY.SMITH@MARITIMESHIPPERS.COM     PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROADPG20                                     HONG KONG               HK             PG21DEQJOHN SMITH             5555555555     JOHN.SMITH@COMPANY.COM             PG19LG                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21LG CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG22 944         CI EP3                                                         PG261000000100000DR                                                             PG262                                                                           PG29KG 000000100000                                                             SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00044                                                               ";
			seEntry.Messages.Add(responseMessage);
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_148191";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(seEntry.PK, reLoadJob.GetEntryHeaderForPGACorrection().PK);
		}

		public void TestPGAMessageStatusAfterPGALineStatusAdded()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = false;
			dec.US_EnableCRL = true;
			dec.ImportEntryNumber = "71002958";
			dec.US_EntryFilerCode = "SV9";

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			invoiceLine.US_FDAIndicator = "D";
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = "FOO";
			fdaLine.US_LineNo = 1;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var seEntry = dec.ActiveEntryHeaders.SimplifiedEntry;
			var collection = new ImportMessageSendingActionCollection(dec, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder2 = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var msg2 = builder2.PopulateMessage();
			Factory.Save();

			AssertEquals("The PGA Message Status should be 'Adding' after send Original message", fdaLine.US_TrackingStatus, PGATrackingStatusList.Codes.Adding);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148784";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148784     SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE16COA 6020 09091300000450                                                     SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE20CR B00160864                                                                Y  1101SV9SE00010";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			seEntry.Messages.Add(message);
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148784";
			responseMessage.EM_Status = EDIMessage.Status.Queued;

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00010";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			dec.Reload();
			fdaLine.Reload();

			AssertEquals(fdaLine.US_TrackingStatus, PGATrackingStatusList.Codes.Added);

			var message2 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002958 0123-456789012CO                      3021 0909131        " +
				"SO20CR B00160703                                                                " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO70FDAFOO090416063001DATA UNDER PGA REVIEW         01  00011001              02" +
				"SO70DTCDTC090416063001DATA UNDER PGA REVIEW         01  00011001              02" +
				"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			dec.Reload();
			fdaLine.Reload();
			AssertEquals(fdaLine.US_TrackingStatus, PGATrackingStatusList.Codes.Added);

			var lineCusDispositions = ((IPGALineStatus)fdaLine).PGALineCusDispositions;
			AssertEquals(1, lineCusDispositions.Count);
			AssertEquals("B7", lineCusDispositions[0].CDI_ParentTableCode);
		}

		public void TestMatchCorrectionEntryHeader()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.ImportEntryNumber = "71002057";

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			dec.US_CertifyCargoRelease = false;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var builder = new ACEEntrySummaryMessageBuilderForTesting(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add);
			var msg1 = builder.PopulateMessage(); // send entry summary message without PGA data
			var formalEntry = dec.FormalEntry;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			responseMessage.EM_Status = EDIMessage.Status.Received;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";

			responseMessage.EM_MessageText =
					"B001101SV9AX                                               HYEDUSCMT_148588     " +
					"E0 SUMMRY 000001 REF ID: SV9 71002057 B00155595                                 " +
					"E0 LINITM 000001 REF ID: 001                                                    " +
					"E0 TARIFF 000001 REF ID: 4703110000                                             " +
					"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  71002057     B00155595   " +
					"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  71002057     B00155595   " +
					"Y  1101SV9AX00005";
			formalEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			formalEntry.Messages.Add(msg1);
			formalEntry.Messages.Add(responseMessage);
			Factory.Save();
			msg1.EM_MessageNum = "HYEDUSCMT_148588";
			Factory.Save();

			dec.US_EnableCRL = true;
			invoiceLine.US_FDAIndicator = "D";
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = "FOO";
			fdaLine.US_LineNo = 1;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var seEntry = dec.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(seEntry);
			var collection = new ImportMessageSendingActionCollection(dec, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder2 = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var msg2 = builder2.PopulateMessage(); // send cargo release message with PGA data

			var responseMessage2 = Factory.New<MQEDIMessage>();
			responseMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage2.EM_Status = EDIMessage.Status.Received;
			responseMessage2.EM_MessageNum = "HYEDUSCMT_148191";
			responseMessage2.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_164692     SE10ASV9  71002057 01EI 58-12345678911800000100001101  1101                     SE15RAPLUMST081515A                                        00000100CS   N       SE20CR B00164172                                                                SE40001HK CYCLOHEXYLDIAZENIUMDIOXY                                              SE6029021100000000010000                                                        OI        CYCLOHEXYLDIAZENIUMDIOXY                                              PG01001EPAPS1   Y                        130.027                                SE9013PH6   MISSING DIS DOCUMENTATION                                           PG02PCAS 1234                                                                   PG04YCYCLOHEXYLDIAZENIUMDIOXY                                            1000000PG07ACME                                                                        PG14 EP8241-420                                                                 PG19EPN   012345CHN002                                                          PG19CB                   U.S. DEMO COMPANY               184 TEST STREET        PG20                                     CHICAGO              IL US60056        PG21CB CRAIG SEELIG           2154441234     CRAIG.SEELIG@TEST.COM              PG55NP                                                                          PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21IM CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG55CI                                                                          PG19OV                   MARITIME INDUSTRIAL SHIPPERS    600 PORT STREET        PG20                                     NEWARK               NJ US20034        PG21OV JAY SMITH              14101123456    JAY.SMITH@MARITIMESHIPPERS.COM     PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROADPG20                                     HONG KONG               HK             PG21DEQJOHN SMITH             5555555555     JOHN.SMITH@COMPANY.COM             PG19LG                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21LG CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG22 944         CI EP3                                                         PG261000000100000DR                                                             PG262                                                                           PG29KG 000000100000                                                             SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00044                                                               ";
			seEntry.Messages.Add(responseMessage2);
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			msg2.EM_MessageNum = "HYEDUSCMT_148191";
			Factory.Save();

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDAFOO090416063001DATA UNDER PGA REVIEW         01  00011001              02" + // BEGINNING TARIFF POSITION (61-61)        :2
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			var fdaLineLoad = reLoadJob.InvoiceLines[0].ACE_FDALines[0];

			var lineCusDispositions = ((IPGALineStatus)fdaLineLoad).PGALineCusDispositions;
			AssertEquals(1, lineCusDispositions.Count);

			var dispositionData = lineCusDispositions[0];

			AssertEquals("B7", dispositionData.CDI_ParentTableCode);
			AssertEquals("PLS", dispositionData.CDI_Type);
			AssertEquals("FDA", dispositionData.CDI_StatusKey);
			AssertEquals("01", dispositionData.CDI_Status);
		}

		public void TestSecondaryLinePGALines()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.US_EnableENS = false;
			dec.ImportEntryNumber = "71002057";

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.US_SupTariff = "7001001000"; //Parent Line
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = "FOO";
			fdaLine.US_LineNo = 1;

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDAFOO090416063001DATA UNDER PGA REVIEW         01  00012001              02" + // BEGINNING TARIFF POSITION (61-61)        :2
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			dec.US_EnableENS = false;

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			var fdaLineLoad = reLoadJob.InvoiceLines[0].ACE_FDALines[0];

			var lineCusDispositions = ((IPGALineStatus)fdaLineLoad).PGALineCusDispositions;
			AssertEquals(1, lineCusDispositions.Count);

			var dispositionData = lineCusDispositions[0];

			AssertEquals("B7", dispositionData.CDI_ParentTableCode);
			AssertEquals("PLS", dispositionData.CDI_Type);
			AssertEquals("FDA", dispositionData.CDI_StatusKey);
			AssertEquals("01", dispositionData.CDI_Status);
		}

		public void TestEndToEndForPGAStatusOnPGALine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "01", "DATA UNDER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var declaration = GetDeclaration("71002057");
			var invoiceLine = declaration.InvoiceLines[0];
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = "FOO";
			fdaLine.US_LineNo = 1;

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDAFOO090416063001DATA UNDER PGA REVIEW         01  00011001              02" +
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			declaration.US_EnableENS = false;

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var fdaLineLoad = reLoadJob.InvoiceLines[0].ACE_FDALines[0];

			var lineCusDispositions = ((IPGALineStatus)fdaLineLoad).PGALineCusDispositions;
			AssertEquals(1, lineCusDispositions.Count);

			var dispositionData = lineCusDispositions[0];

			AssertEquals("B7", dispositionData.CDI_ParentTableCode);
			AssertEquals("PLS", dispositionData.CDI_Type);
			AssertEquals("FDA", dispositionData.CDI_StatusKey);
			AssertEquals("01", dispositionData.CDI_Status);
			AssertEquals("DATA UNDER PGA REVIEW", dispositionData.StatusDescription);
		}

		public void TestPGALineStatusForLineNumberRange()
		{
			var declaration = GetDeclaration("71002057");
			var invoiceLine = declaration.InvoiceLines[0];
			var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine1.US_ProgramCode = "FOO";
			fdaLine1.US_LineNo = 1;

			var fdaLine2 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine2.US_ProgramCode = "FOO";
			fdaLine2.US_LineNo = 2;

			var fdaLine3 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine3.US_ProgramCode = "FOO";
			fdaLine3.US_LineNo = 3;

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDAFOO090416063001DATA UNDER PGA REVIEW         01  00011001    003       02" +
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			declaration.US_EnableENS = false;
			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(3, reLoadJob.InvoiceLines[0].ACE_FDALines.Count);
			foreach (var fdaLineLoad in reLoadJob.InvoiceLines[0].ACE_FDALines)
			{
				var lineCusDispositions = ((IPGALineStatus)fdaLineLoad).PGALineCusDispositions;
				AssertEquals(1, lineCusDispositions.Count);

				var dispositionData = lineCusDispositions[0];

				AssertEquals("B7", dispositionData.CDI_ParentTableCode);
				AssertEquals("PLS", dispositionData.CDI_Type);
				AssertEquals("FDA", dispositionData.CDI_StatusKey);
				AssertEquals("01", dispositionData.CDI_Status);
			}
		}

		/*
		Make the entry lines has the same line number, Match the invoice line by Traiff
		*/
		public void TestMultipleEntryLinesWithTheSameEntryLineNumber()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_OH_Importer = importer.PK;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = false;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.ImportEntryNumber = "71002057";

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = "FOO";
			fda.US_LineNo = 1;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_Tariff = "5001000000";
			invoiceLine2.JI_CustomsQuantity = 5m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "CN";
			var fws = invoiceLine2.FWSHeaders.AddNew();
			fws.US_LineNo = 1;

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 100m;
			invoiceLine3.JI_Tariff = "7001001000";
			invoiceLine3.JI_CustomsQuantity = 5m;
			invoiceLine3.US_UC_NKCountryOfOrigin = "CA";
			var omc = invoiceLine3.OMCHeaders.AddNew();
			omc.US_LineNo = 1;

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = dec.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entry);

			var entryLines = entry.MergedLines.Cast<CusEntryLine>().ToList();
			AssertEquals(3, entryLines.Count);
			AssertEquals(2, entryLines.Count(x => x.CL_LineNumber == new ZShort(1)));// has the same entry lines
			AssertEquals(1, entryLines.Count(x => x.CL_LineNumber == new ZShort(2)));

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDAFOO090416063001DATA UNDER PGA REVIEW         01  00011001              02" +
					"SO70FWSFWS090416063007DATA UNDER PGA REVIEW         07  00012001              02" +
					"SO70OMCOMC090416063004DATA UNDER PGA REVIEW         04  00021001              02" +
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			dec.US_EnableENS = false;

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			var line1 = reLoadJob.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.PK == invoiceLine.PK);
			var fdaReLoad = line1.ACE_FDALines[0];
			var lineCusDispositions1 = ((IPGALineStatus)fdaReLoad).PGALineCusDispositions;
			AssertEquals(1, lineCusDispositions1.Count);
			var dispositionData1 = lineCusDispositions1[0];
			AssertEquals("FDA", dispositionData1.CDI_StatusKey);
			AssertEquals("01", dispositionData1.CDI_Status);

			var line2 = reLoadJob.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.PK == invoiceLine2.PK);
			var fwsReLoad = line2.FWSHeaders[0];
			var lineCusDispositions2 = ((IPGALineStatus)fwsReLoad).PGALineCusDispositions;
			AssertEquals(1, lineCusDispositions2.Count);
			var dispositionData2 = lineCusDispositions2[0];
			AssertEquals("FWS", dispositionData2.CDI_StatusKey);
			AssertEquals("07", dispositionData2.CDI_Status);

			var line3 = reLoadJob.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.PK == invoiceLine3.PK);
			var omcReLoad = line3.OMCHeaders[0];
			var lineCusDispositions3 = ((IPGALineStatus)omcReLoad).PGALineCusDispositions;
			AssertEquals(1, lineCusDispositions3.Count);
			var dispositionData3 = lineCusDispositions3[0];
			AssertEquals("OMC", dispositionData3.CDI_StatusKey);
			AssertEquals("04", dispositionData3.CDI_Status);
		}

		public void TestTwoOrderMessagesForPGALineStatus()
		{
			var declaration = GetDeclaration("71002057");
			var invoiceLine = declaration.InvoiceLines[0];
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = "FOO";
			fdaLine.US_LineNo = 1;

			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_LineNo = 1;

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDAFOO012216122801DATA UNDER PGA REVIEW         01  00011001              02" +
					"SO70CPSCPS012216122801DATA UNDER PGA REVIEW         01  00011001              02" +
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var message2 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO20CR B00160703                                                                " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO70FDAFOO012216122901DATA UNDER PGA REVIEW         01  00011001              02" +
				"SO70CPSCPS012216122907MAY PROCEED                   07  00011001              02" +
				"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			declaration.US_EnableENS = false;
			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			invoiceLine = reLoadJob.InvoiceLines[0];
			var fdaReLoad = invoiceLine.ACE_FDALines[0];
			var fdaCusDispositions = ((IPGALineStatus)fdaReLoad).PGALineCusDispositions;
			AssertEquals(fdaCusDispositions.Count, 1);
			Assert(fdaCusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "FDA" && x.CDI_Status == "01"));

			var cpscReLoad = invoiceLine.CPSCHeaders[0];
			var cpscCusDispositions = ((IPGALineStatus)cpscReLoad).PGALineCusDispositions;
			AssertEquals(cpscCusDispositions.Count, 1);
			Assert(cpscCusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "CPS" && x.CDI_Status == "07"));
		}

		public void TestPGALineStatusOnInvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "01", "DATA UNDER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var declaration = GetDeclaration("71002057");
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.US_DDTCInd = "D";

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70DTCDTC090416063001DATA UNDER PGA REVIEW         01  00011001              02" +
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			declaration.US_EnableENS = false;

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var invoiceLineLoad = reLoadJob.InvoiceLines[0];

			var lineCusDispositions = invoiceLineLoad.PGALineCusDispositions;
			AssertEquals(1, lineCusDispositions.Count);

			var dispositionData = lineCusDispositions[0];

			AssertEquals("JI", dispositionData.CDI_ParentTableCode);
			AssertEquals("PLS", dispositionData.CDI_Type);
			AssertEquals("DTC", dispositionData.CDI_StatusKey);
			AssertEquals("DATA UNDER PGA REVIEW", dispositionData.StatusDescription);
		}

		public void TestShouldNotUpdatePGALineStatus()
		{
			var declaration = GetDeclaration("71002057");
			var invoiceLine = declaration.InvoiceLines[0];
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = "RAD";
			fdaLine.US_LineNo = 1;

			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_LineNo = 1;

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDARAD012216122801DATA UNDER PGA REVIEW         01  00011001              02" +
					"SO70CPSCPS012216122801DATA UNDER PGA REVIEW         01  00011001              02" +

					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var message2 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO20CR B00160703                                                                " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO70FDARAD012216122701DATA UNDER PGA REVIEW         01  00011001              02" +
				"SO70CPSCPS012216122707MAY PROCEED                   07  00011001              02" +
				"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			declaration.US_EnableENS = false;

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			invoiceLine = reLoadJob.InvoiceLines[0];
			var fdaReLoad = invoiceLine.ACE_FDALines[0];
			var fdaCusDispositions = ((IPGALineStatus)fdaReLoad).PGALineCusDispositions;
			AssertEquals(fdaCusDispositions.Count, 1);
			Assert(fdaCusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "FDA" && x.CDI_Status == "01"));

			var cpscReLoad = invoiceLine.CPSCHeaders[0];
			var cpscCusDispositions = ((IPGALineStatus)cpscReLoad).PGALineCusDispositions;
			AssertEquals(cpscCusDispositions.Count, 1);
			Assert(cpscCusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "CPS" && x.CDI_Status == "01"));
		}

		public void TestSummaryEntryHasPGADataCorrectionResponseMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = false;
			dec.US_CertifyCargoRelease = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.ImportEntryNumber = "71002057";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";
			dec.JE_OH_Importer = importer.PK;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new ImportMessageSendingActionCollection(dec, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(ensEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			var ensMessageBuilder = new ACEEntrySummaryMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Add);
			var outgoingMessage = ensMessageBuilder.PopulateMessage();
			var entrySummaryResponseMessage = Factory.New<MQEDIMessage>();
			entrySummaryResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			entrySummaryResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryResponseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			entrySummaryResponseMessage.EM_Status = EDIMessage.Status.Received;
			entrySummaryResponseMessage.EM_MessageNum = "HYEDUSCMT_148588";
			entrySummaryResponseMessage.EM_MessageText =
					"B001101SV9AX                                               HYEDUSCMT_148588     " +
					"E0 SUMMRY 000001 REF ID: SV9 71002057 B00155595                                 " +
					"E0 LINITM 000001 REF ID: 001                                                    " +
					"E0 TARIFF 000001 REF ID: 4703110000                                             " +
					"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  71002057     B00155595   " +
					"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  71002057     B00155595   " +
					"Y  1101SV9AX00005";

			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.Messages.Add(outgoingMessage);
			ensEntry.Messages.Add(entrySummaryResponseMessage);

			var seEntry = dec.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(seEntry);
			var cargoReleaseResponseMessage = Factory.New<MQEDIMessage>();
			cargoReleaseResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cargoReleaseResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cargoReleaseResponseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			cargoReleaseResponseMessage.EM_Status = EDIMessage.Status.Received;
			cargoReleaseResponseMessage.EM_MessageNum = "HYEDUSCMT_148588";
			cargoReleaseResponseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_148588     SE10ASV9  71002057 01EI 58-12345678911800000100001101  1101                     SE15RAPLUMST081515A                                        00000100CS   N       SE20CR B00164172                                                                SE40001HK CYCLOHEXYLDIAZENIUMDIOXY                                              SE6029021100000000010000                                                        OI        CYCLOHEXYLDIAZENIUMDIOXY                                              PG01001EPAPS1   Y                        130.027                                SE9013PH6   MISSING DIS DOCUMENTATION                                           PG02PCAS 1234                                                                   PG04YCYCLOHEXYLDIAZENIUMDIOXY                                            1000000PG07ACME                                                                        PG14 EP8241-420                                                                 PG19EPN   012345CHN002                                                          PG19CB                   U.S. DEMO COMPANY               184 TEST STREET        PG20                                     CHICAGO              IL US60056        PG21CB CRAIG SEELIG           2154441234     CRAIG.SEELIG@TEST.COM              PG55NP                                                                          PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21IM CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG55CI                                                                          PG19OV                   MARITIME INDUSTRIAL SHIPPERS    600 PORT STREET        PG20                                     NEWARK               NJ US20034        PG21OV JAY SMITH              14101123456    JAY.SMITH@MARITIMESHIPPERS.COM     PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROADPG20                                     HONG KONG               HK             PG21DEQJOHN SMITH             5555555555     JOHN.SMITH@COMPANY.COM             PG19LG                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21LG CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG22 944         CI EP3                                                         PG261000000100000DR                                                             PG262                                                                           PG29KG 000000100000                                                             SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00044                                                               ";
			seEntry.Messages.Add(cargoReleaseResponseMessage);
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_148588";
			Factory.Save();

			var pgaDataCorrectionMessage = Factory.New<MQEDIMessage>();
			pgaDataCorrectionMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			pgaDataCorrectionMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			pgaDataCorrectionMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PGADataCorrection;
			pgaDataCorrectionMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.PGADataCorrection;
			pgaDataCorrectionMessage.EM_Status = EDIMessage.Status.Sent;
			pgaDataCorrectionMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 10, 15);
			pgaDataCorrectionMessage.EM_LinkedObject = ensEntry;
			Factory.Save();

			var pgaDataCorrectionResponseMessage = Factory.New<MQEDIMessage>();
			pgaDataCorrectionResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			pgaDataCorrectionResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			pgaDataCorrectionResponseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse;
			pgaDataCorrectionResponseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.PGADataCorrection;
			pgaDataCorrectionResponseMessage.EM_Status = EDIMessage.Status.Received;
			pgaDataCorrectionResponseMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 10, 16);
			pgaDataCorrectionResponseMessage.EM_LinkedObject = ensEntry;
			pgaDataCorrectionResponseMessage.EM_MessageNum = pgaDataCorrectionMessage.EM_MessageNum;
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(ensEntry.PK, reLoadJob.GetEntryHeaderWithPGADetail().PK);
		}

		public void TestCargoReleaseEntryHasPGADataCorrectionResponseMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = false;
			dec.US_CertifyCargoRelease = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.ImportEntryNumber = "71002057";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";
			dec.JE_OH_Importer = importer.PK;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "5001000000";
			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new ImportMessageSendingActionCollection(dec, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(ensEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			var ensMessageBuilder = new ACEEntrySummaryMessageBuilder(ensEntry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Add);
			var outgoingMessage = ensMessageBuilder.PopulateMessage();
			var entrySummaryResponseMessage = Factory.New<MQEDIMessage>();
			entrySummaryResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			entrySummaryResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryResponseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			entrySummaryResponseMessage.EM_Status = EDIMessage.Status.Received;
			entrySummaryResponseMessage.EM_MessageNum = "HYEDUSCMT_148588";
			entrySummaryResponseMessage.EM_MessageText =
					"B001101SV9AX                                               HYEDUSCMT_148588     " +
					"E0 SUMMRY 000001 REF ID: SV9 71002057 B00155595                                 " +
					"E0 LINITM 000001 REF ID: 001                                                    " +
					"E0 TARIFF 000001 REF ID: 4703110000                                             " +
					"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  71002057     B00155595   " +
					"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  71002057     B00155595   " +
					"Y  1101SV9AX00005";

			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.Messages.Add(outgoingMessage);
			ensEntry.Messages.Add(entrySummaryResponseMessage);

			var seEntry = dec.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(seEntry);
			var cargoReleaseResponseMessage = Factory.New<MQEDIMessage>();
			cargoReleaseResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cargoReleaseResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cargoReleaseResponseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			cargoReleaseResponseMessage.EM_Status = EDIMessage.Status.Received;
			cargoReleaseResponseMessage.EM_MessageNum = "HYEDUSCMT_148588";
			cargoReleaseResponseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_148588     SE10ASV9  71002057 01EI 58-12345678911800000100001101  1101                     SE15RAPLUMST081515A                                        00000100CS   N       SE20CR B00164172                                                                SE40001HK CYCLOHEXYLDIAZENIUMDIOXY                                              SE6029021100000000010000                                                        OI        CYCLOHEXYLDIAZENIUMDIOXY                                              PG01001EPAPS1   Y                        130.027                                SE9013PH6   MISSING DIS DOCUMENTATION                                           PG02PCAS 1234                                                                   PG04YCYCLOHEXYLDIAZENIUMDIOXY                                            1000000PG07ACME                                                                        PG14 EP8241-420                                                                 PG19EPN   012345CHN002                                                          PG19CB                   U.S. DEMO COMPANY               184 TEST STREET        PG20                                     CHICAGO              IL US60056        PG21CB CRAIG SEELIG           2154441234     CRAIG.SEELIG@TEST.COM              PG55NP                                                                          PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21IM CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG55CI                                                                          PG19OV                   MARITIME INDUSTRIAL SHIPPERS    600 PORT STREET        PG20                                     NEWARK               NJ US20034        PG21OV JAY SMITH              14101123456    JAY.SMITH@MARITIMESHIPPERS.COM     PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROADPG20                                     HONG KONG               HK             PG21DEQJOHN SMITH             5555555555     JOHN.SMITH@COMPANY.COM             PG19LG                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21LG CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG22 944         CI EP3                                                         PG261000000100000DR                                                             PG262                                                                           PG29KG 000000100000                                                             SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00044                                                               ";
			seEntry.Messages.Add(cargoReleaseResponseMessage);
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_148588";
			Factory.Save();

			var pgaDataCorrectionMessage = Factory.New<MQEDIMessage>();
			pgaDataCorrectionMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			pgaDataCorrectionMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			pgaDataCorrectionMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PGADataCorrection;
			pgaDataCorrectionMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.PGADataCorrection;
			pgaDataCorrectionMessage.EM_Status = EDIMessage.Status.Sent;
			pgaDataCorrectionMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 10, 15);
			pgaDataCorrectionMessage.EM_LinkedObject = seEntry;
			Factory.Save();

			var pgaDataCorrectionResponseMessage = Factory.New<MQEDIMessage>();
			pgaDataCorrectionResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			pgaDataCorrectionResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			pgaDataCorrectionResponseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse;
			pgaDataCorrectionResponseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.PGADataCorrection;
			pgaDataCorrectionResponseMessage.EM_Status = EDIMessage.Status.Received;
			pgaDataCorrectionResponseMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 10, 16);
			pgaDataCorrectionResponseMessage.EM_LinkedObject = seEntry;
			pgaDataCorrectionResponseMessage.EM_MessageNum = pgaDataCorrectionMessage.EM_MessageNum;
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(seEntry.PK, reLoadJob.GetEntryHeaderWithPGADetail().PK);
		}

		public void TestUpdateDeclarationFromSOMessages()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = GetDeclaration("71002057");
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = "TRK";

			var bill1 = declaration.Bills.AddNew();
			bill1.US_UI_NKBillIssuerSCAC = "C2";
			bill1.CU_BillNum = "PI3151202078";
			bill1.CU_BillType = "HB";
			bill1.CU_NoOfPacks = 10;
			bill1.CU_PackType = "AE";

			var bill2 = declaration.Bills.AddNew();
			bill2.US_UI_NKBillIssuerSCAC = "C2";
			bill2.CU_BillNum = "PI3151202079";
			bill2.CU_BillType = "HB";

			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.US_EntryDate = ZDateTime.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			bill1 = reLoadJob.Bills.Cast<Bill>().FirstOrDefault(x => x.CU_BillNum == "PI3151202078");
			bill2 = reLoadJob.Bills.Cast<Bill>().FirstOrDefault(x => x.CU_BillNum == "PI3151202079");
			CombineAssertions(() =>
			{
				AssertEquals("US_SchDEntry", "4601", reLoadJob.US_SchDEntry);
				AssertEquals("US_UI_NKCarrierSCAC", "HLCU", reLoadJob.US_UI_NKCarrierSCAC);
				AssertEquals("JE_VoyageFlightNo", "40W", reLoadJob.JE_VoyageFlightNo);
				AssertEquals("JE_TotalNoOfPacks", 9913, reLoadJob.JE_TotalNoOfPacks);
				AssertEquals("JE_TotalNoOfPacksPackType", "BO", reLoadJob.JE_TotalNoOfPacksPackType);
				AssertEquals("US_EntryDate", new ZDateTime(2016, 1, 22), reLoadJob.US_EntryDate);
				AssertEquals("JE_DateOfArrival", new ZDateTime(2016, 12, 1), reLoadJob.JE_DateOfArrival);
				AssertEquals("US_SchDArrival", "1108", reLoadJob.US_SchDArrival);

				AssertEquals("bill1.CU_NoOfPacks is not updated for the original value is not zero", 10m, bill1.CU_NoOfPacks);
				AssertEquals("bill1.CU_PackType is not updated for the original value is not empty", "AE", bill1.CU_PackType);

				AssertEquals("bill2.CU_NoOfPacks is updated for the original value is zero", 4957m, bill2.CU_NoOfPacks);
				AssertEquals("bill2.CU_PackType is updated for the original value is empty", "BO", bill2.CU_PackType);
			});
		}

		public void TestUpdateDeletedDeclarationFromSOMessages()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = GetDeclaration("71002057");
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = "TRK";

			var bill1 = declaration.Bills.AddNew();
			bill1.US_UI_NKBillIssuerSCAC = "C2";
			bill1.CU_BillNum = "PI3151202078";
			bill1.CU_BillType = "HB";
			bill1.CU_NoOfPacks = 10;
			bill1.CU_PackType = "AE";

			var bill2 = declaration.Bills.AddNew();
			bill2.US_UI_NKBillIssuerSCAC = "C2";
			bill2.CU_BillNum = "PI3151202079";
			bill2.CU_BillType = "HB";

			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.US_EntryDate = ZDateTime.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			declaration.Delete();
			Factory.Save();
			AssertNoExceptionThrown(() =>
			{
				new ABIIncomingMessageProcessor().ExecuteBatch();
			});
		}

		[TestDate(2021, 06, 16)]
		public void TestSuppressSendingSTUOncePaymentAuthorizedSO()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var declaration = GetDeclaration("71002777");
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = "TRK";
			declaration.JE_DeclarationReference = "JE02777";

			var request = new AutoSendStatementDateChangeRequest();
			request.OverrideAllOrByOrganisation = "ALL";
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, request);

			var bill1 = declaration.Bills.AddNew();
			bill1.US_UI_NKBillIssuerSCAC = "C2";
			bill1.CU_BillNum = "PI3151202078";
			bill1.CU_BillType = "MB";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_CU_ParentBill = bill1.PK;
			bill2.US_UI_NKBillIssuerSCAC = "C2";
			bill2.CU_BillNum = "PI3151202079";
			bill2.CU_BillType = "HB";

			SetupStatement("71002777", "112288777", declaration.JE_DeclarationReference);

			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.US_EntryDate = ZDateTime.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(2);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;

			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.EntryNumber = "71002777";

			var message = CreateStatusMessage(@"
B004601267SO                                                                    
SO104601SV9  71002777 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                                   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                17062101                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var logs = reLoadJob.Logs.GetAllLogs().ToArray<StmALog>();
			AssertEquals("STU message suppressed because the payment has been authorized.", true, logs.Any(x => x.SL_Reference == AutomaticSTUConditionChecker.NotificationSuppressSTUPaymentAuthorized));
		}

		[TestDate(2021, 06, 16)]
		public void TestSuppressSendingSTUOncePaymentAuthorizedSOAndC1()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var declaration = GetDeclaration("71002788");
			declaration.JE_DeclarationReference = "DEC2788";
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = "TRK";
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(2);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;

			var request = new AutoSendStatementDateChangeRequest();
			request.OverrideAllOrByOrganisation = "ALL";
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, request);

			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			SetupStatement("71002788", "7711223377", declaration.JE_DeclarationReference);

			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO101601SV9  71002788 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
					"SO20CR 6NYC00663                                                                " +
					"SO40MACLU6A62S6680206                                                           " +
					"SO40HSSLLCHS222271                                         00000001     00000001" +
					"SO50051116121894BILL DEPARTED                                                   " +
					"SO60052616163799RELEASE SUSPENDED                                               " +
					"SO60052616163790UNDER CBP REVIEW                                                " +
					"SO70FDARAD012216122901DATA UNDER PGA REVIEW         01  002001                  " +
					"SO70NHTOFF012216122901DATA UNDER PGA REVIEW         07  001001                  " +
					"SO70NHTOFF012216122907MAY PROCEED                   07  002001                  " +
					"SO70FDARAD012216122907MAY PROCEED                   07  001001                  " +
					"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 71002788                                                    Y2       Y  3901SV9CQ00001";
			entry.Messages.Add(outgoing);

			var c1message = Factory.New<MQEDIMessage>();
			c1message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message.EM_MessageNum = "HYEDUSCMT_159673";
			c1message.EM_Status = EDIMessage.Status.Queued;
			c1message.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WO101601SV9  71002788 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
									   "WO40MACLU6A62S6680206                                                           " +
									   "WO40HSSLLCHS222271                                         00000001     00000001" +
									   "WO50052716101095BILL ARRIVED                                                    " +
									   "WO60052716101098RELEASED                                06152101                " +
									   "WO60052716101001ONE USG                                                         " +
									   "Y  4701SV9C100006";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status")));
			AssertNotNull(email);
			Assert("Should contains a reason why supress STU message.", email.Body.Contains(AutomaticSTUConditionChecker.NotificationSuppressSTUPaymentAuthorized));
			var emailc1 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo/Manifest/Entry Status Query")));
			AssertNotNull(emailc1);
			Assert("Should contains a reason why supress STU message.", email.Body.Contains(AutomaticSTUConditionChecker.NotificationSuppressSTUPaymentAuthorized));
		}

		public void TestUpdateDeclarationFromSOMessagesSingleQty()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = GetDeclaration("71002057");
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = "TRK";

			var bill1 = declaration.Bills.AddNew();
			bill1.US_UI_NKBillIssuerSCAC = "C2";
			bill1.CU_BillNum = "PI3151202078";
			bill1.CU_BillType = "MB";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_CU_ParentBill = bill1.PK;
			bill2.US_UI_NKBillIssuerSCAC = "C2";
			bill2.CU_BillNum = "PI3151202079";
			bill2.CU_BillType = "HB";

			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.US_EntryDate = ZDateTime.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			var message = CreateStatusMessage(@"
B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                                   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			CombineAssertions(() =>
			{
				AssertEquals("JE_TotalNoOfPacks", 4957, reLoadJob.JE_TotalNoOfPacks);
				AssertEquals("JE_TotalNoOfPacksPackType", "BO", reLoadJob.JE_TotalNoOfPacksPackType);
			});
		}

		public void TestUpdateDeclarationFromSOMessagesMultiSubBills()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = GetDeclaration("71002057");
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = "TRK";

			var bill1 = declaration.Bills.AddNew();
			bill1.US_UI_NKBillIssuerSCAC = "C2";
			bill1.CU_BillNum = "PI3151202078";
			bill1.CU_BillType = "MB";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_CU_ParentBill = bill1.PK;
			bill2.US_UI_NKBillIssuerSCAC = "C2";
			bill2.CU_BillNum = "PI3151202079";
			bill2.CU_BillType = "HB";

			var bill3 = declaration.Bills.AddNew();
			bill3.CU_CU_ParentBill = bill1.PK;
			bill3.US_UI_NKBillIssuerSCAC = "C2";
			bill3.CU_BillNum = "PI3151202080";
			bill3.CU_BillType = "HB";

			var message = CreateStatusMessage(@"
B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                                   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00001111BO   00001111
SO40RC2  PI3151202080                                      00002222PKG  00002222
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000                                                               
".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			CombineAssertions(() =>
			{
				Assert("1111BO", reLoadJob.Packages.OfType<Package>().Any(p => p.CW_PackQty == 1111 && p.CW_PackType == "BO"));
				Assert("2222PKG", reLoadJob.Packages.OfType<Package>().Any(p => p.CW_PackQty == 2222 && p.CW_PackType == "PKG"));
			});
		}

		public void TestUpdateDeclarationFromSOMessages_SO50()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = GetDeclaration("71002058");
			declaration.US_EnableENS = true;
			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var bill = declaration.Bills.AddNew();
			bill.US_UI_NKBillIssuerSCAC = "HLCU";
			bill.CU_BillNum = "PI3151202078";
			bill.CU_BillType = "HB";

			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002058 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO40RHLCUPI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1026141512    
SO40RHLCUPI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var reloadBill = reLoadJob.Bills.Cast<Bill>().FirstOrDefault(x => x.CU_BillNum == "PI3151202078");
			CombineAssertions(() =>
			{
				AssertEquals("bill.CU_NoOfPacks is updated", 4956m, reloadBill.CU_NoOfPacks);
				AssertEquals("bill.CU_PackType is updated", "BO", reloadBill.CU_PackType);
				AssertEquals("JE_DateOfArrival is not updated for the 2 arrival dates not match", ZDateTime.Empty, reLoadJob.JE_DateOfArrival);
				AssertEquals("US_SchDArrival is not updated for the 2 arrival ports not match", ZString.Empty, reLoadJob.US_SchDArrival);
			});
		}

		public void TestCS00487919_PGADispositionUpdateSecondsApart()
		{
			var declaration = GetDeclaration("71002057");
			var message1 = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR 600012531                                                                " +
					"SO40M    01604099093                                                            " +
					"SO40H    033647189                                         00000001     00000000" +
					"SO50121516110191NO BILL MATCH                                                   " +
					"SO70FDAFOO121516110101DATA UNDER PGA REVIEW         01  00011001              02" +
					"SO70FDAFOO121516110101DATA UNDER PGA REVIEW         01  00011002              02" +
					"SO70FDAFOO121516110101DATA UNDER PGA REVIEW         01  00011003              02" +
					"SO70FDAFOO121516110101DATA UNDER PGA REVIEW         01  00021001              02" +
					"SO70FDAFOO121516110101DATA UNDER PGA REVIEW         01  00031001              02" +
					"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(reLoadJob.EntryPGACusDispositions.Count, 1);
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "FDA" && x.CDI_Status == "01"));

			var message2 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO20CR 600012531                                                                " +
				"SO40M    01604099093                                                            " +
				"SO40H    033647189                                         00000001     00000000" +
				"SO50121516110191NO BILL MATCH                                                   " +
				"SO70FDAFOO121516110107MAY PROCEED                   072200011001              02" +
				"SO70FDAFOO121516110107MAY PROCEED                   072200011002              02" +
				"SO70FDAFOO121516110107MAY PROCEED                   072200011003              02" +
				"SO70FDAFOO121516110107MAY PROCEED                   072200021001              02" +
				"SO70FDAFOO121516110107MAY PROCEED                   072200031001              02" +
				"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(reLoadJob.EntryPGACusDispositions.Count, 1);
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "FDA" && x.CDI_Status == "07"));
		}

		public void TestBillStatusMISCC1AndSO_1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, "IMakeBondCloseDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, "IMakeBondCloseDisposition6263", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, "INeutralInBondDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, "IsExamDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName5 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, "IsHoldDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName6 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, "IsHoldExamRemovedDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName7 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1A", "1A DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "52", "52 DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1B", "1B DESC", startDate, endDate);

			var attribute11 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName4.ZXE_Name, "Y");
			var attribute12 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName7.ZXE_Name, code3.ZZD_Code);
			var attribute21 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName2.ZXE_Name, "Y");
			var attribute31 = helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, attributeName6.ZXE_Name, "Y");
			Factory.Save();

			var declaration = GetDeclaration("71002057");
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "00591325206";
			bill1.CU_BillType = "HB";
			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1                            MOLU13700107330                        YY2       Y  3901SV9CQ00001";
			entry.Messages.Add(outgoing);

			var c1message = Factory.New<MQEDIMessage>();
			c1message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message.EM_MessageNum = "HYEDUSCMT_159673";
			c1message.EM_Status = EDIMessage.Status.Queued;
			c1message.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WR12704            01                     APLUAA TEST1            001T 121414   " +
									   "WR2                                                                         A001" +
									   "WR3001HK8438909090                                                              " +
									   "WR4            13700107330 00591325206             00000034KG   MOLUMOLUMN1     " +
									   "WR512121407561A AMS FLIGHT NOT DEPARTED/ARRIVED                  00000049       " +
									   "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("1A", reLoadBill1.CU_MessageStatus);
			AssertEquals("Y", reLoadBill1.HLDOrEXMStatus);

			var message = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO50051116121852MANIFEST HOLD AGRICULTURE                                       " +
				"SO40R    00591325207                                       00000150BL   00000000" +
				"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("1A/52", reLoadBill1.CU_MessageStatus);
			AssertEquals("Y", reLoadBill1.HLDOrEXMStatus);

			var c1message2 = Factory.New<MQEDIMessage>();
			c1message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message2.EM_MessageNum = "HYEDUSCMT_159673";
			c1message2.EM_Status = EDIMessage.Status.Queued;
			c1message2.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WR12704            01                     APLUAA TEST1            001T 121414   " +
									   "WR2                                                                         A001" +
									   "WR3001HK8438909090                                                              " +
									   "WR4            13700107330 00591325206             00000034KG   MOLUMOLUMN1     " +
									   "WR512121407561B AMS FLIGHT NOT DEPARTED/ARRIVED                  00000049       " +
									   "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("52", reLoadBill1.CU_MessageStatus);
			AssertEquals("Y", reLoadBill1.HLDOrEXMStatus);

			var message2 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO50051116141855MANIFEST HOLD AGRICULTURE                                       " +
				"SO40R    00591325207                                       00000150BL   00000000" +
				"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("", reLoadBill1.CU_MessageStatus);
			AssertEquals("N", reLoadBill1.HLDOrEXMStatus);
		}

		public void TestBillStatus72WillBeRemovedAfterReceived56()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, "IsHoldDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code72 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "72", "72 DESC", startDate, endDate);
			var code56 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "56", "56 DESC", startDate, endDate);

			var attribute1 = helper.CreateNewOrGetExistingCusCodeListAttribute(code72.PK, attributeName1.ZXE_Name, "Y");
			var attribute2 = helper.CreateNewOrGetExistingCusCodeListAttribute(code72.PK, attributeName2.ZXE_Name, code56.ZZD_Code);
			Factory.Save();

			var declaration = GetDeclaration("71002057");
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "00591325206";
			bill1.CU_BillType = "HB";
			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1                            MOLU13700107330                        YY2       Y  3901SV9CQ00001";
			entry.Messages.Add(outgoing);

			var c1message = Factory.New<MQEDIMessage>();
			c1message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message.EM_MessageNum = "HYEDUSCMT_159673";
			c1message.EM_Status = EDIMessage.Status.Queued;
			c1message.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WR12704            01                     APLUAA TEST1            001T 121414   " +
									   "WR4            13700107330 00591325206             00000034KG   MOLUMOLUMN1     " +
									   "WR5022924105572 CBPA INSPECTION/DOC REVIEW HOLD                  5000     001   " +
									   "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("72", reLoadBill1.CU_MessageStatus);

			var message = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO50022924105856CBP HOLD REMOVED                                                " +
				"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals(ZString.Empty, reLoadBill1.CU_MessageStatus);
		}

		public void TestBillHoldStatusRemoved()
		{
			var declaration = GetDeclaration("71002057");
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "15702827311";

			var message = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    15702827311                                       00000009     00000011" +
				"SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var message2 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    15702827311                                       00000009     00000011" +
				"SO50120116074752MANIFEST HOLD AGRICULTURE               YQTR 727  1201161108    " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var message3 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    15702827311                                       00000009     00000011" +
				"SO50120116175755AGRICULTURE MANIFEST HOLD REMOVED       YQTR 727  1201161108    " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("", reLoadBill1.CU_MessageStatus);
		}

		public void TestBillStatusIsHLDOrEXM()
		{
			var declaration = GetDeclaration("71002057");
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "00591325206";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillNum = "00591325207";

			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO50051116121852MANIFEST HOLD AGRICULTURE                                       " +
					"SO40R    00591325207                                       00000150BL   00000000" +
					"SO50051116121852MANIFEST HOLD AGRICULTURE                                       " +
					"SO60120915080990UNDER CBP REVIEW                                                " +
					"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var message2 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO50051116131895MANIFEST HOLD AGRICULTURE                                       " +
				"SO40R    00591325207                                       00000150BL   00000000" +
				"SO50051116131895MANIFEST HOLD AGRICULTURE                                       " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var message3 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325207                                       00000150BL   00000000" +
				"SO50051116141855MANIFEST HOLD AGRICULTURE                                       " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("52", reLoadBill1.CU_MessageStatus);
			AssertEquals("Y", reLoadBill1.HLDOrEXMStatus);
			var reLoadBill2 = new BusinessObjectFactory().Load<Bill>(bill2.PK);
			AssertEquals("", reLoadBill2.CU_MessageStatus);
			AssertEquals("N", reLoadBill2.HLDOrEXMStatus);
		}

		public void TestBillStatusIsNotHLDOrEXM()
		{
			var declaration = GetDeclaration("71002057");
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "00591325206";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillNum = "00591325207";

			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO50051116121851MANIFEST HOLD AGRICULTURE                                       " +
					"SO40R    00591325207                                       00000150BL   00000000" +
					"SO50051116121851MANIFEST HOLD AGRICULTURE                                       " +
					"SO60120915080990UNDER CBP REVIEW                                                " +
					"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("51", reLoadBill1.CU_MessageStatus);
			AssertEquals("Y", reLoadBill1.HLDOrEXMStatus);
			var reLoadBill2 = new BusinessObjectFactory().Load<Bill>(bill2.PK);
			AssertEquals("51", reLoadBill2.CU_MessageStatus);
			AssertEquals("Y", reLoadBill2.HLDOrEXMStatus);

			var message2 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO50051116131851MANIFEST HOLD AGRICULTURE                                       " +
				"SO40R    00591325207                                       00000150BL   00000000" +
				"SO50051116131854MANIFEST HOLD AGRICULTURE                                       " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var message3 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO50051116141851MANIFEST HOLD AGRICULTURE                                       " +
				"SO40R    00591325207                                       00000150BL   00000000" +
				"SO50051116141895MANIFEST HOLD AGRICULTURE                                       " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var message4 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO50051116151851MANIFEST HOLD AGRICULTURE                                       " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var message5 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO50051116161854MANIFEST HOLD AGRICULTURE                                       " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var message6 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO50051116171895MANIFEST HOLD AGRICULTURE                                       " +
				"SO60120915080990UNDER CBP REVIEW                                                " +
				"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			reLoadBill1 = new BusinessObjectFactory().Load<Bill>(bill1.PK);
			AssertEquals("", reLoadBill1.CU_MessageStatus);
			AssertEquals("N", reLoadBill1.HLDOrEXMStatus);
			reLoadBill2 = new BusinessObjectFactory().Load<Bill>(bill2.PK);
			AssertEquals("", reLoadBill2.CU_MessageStatus);
			AssertEquals("N", reLoadBill2.HLDOrEXMStatus);
		}

		public void TestRVW_ReleaseStatus()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
					"SO20CR 6NYC00663                                                                " +
					"SO40MACLU6A62S6680206                                                           " +
					"SO40HSSLLCHS222271                                         00000001     00000001" +
					"SO50051116121894BILL DEPARTED                                                   " +
					"SO60052616163799RELEASE SUSPENDED                                               " +
					"SO60052616163790UNDER CBP REVIEW                                                " +
					"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var dispositionDataCount = reLoadJob.DispositionCodes.Count;
			var releaseStatus = reLoadJob.ReleaseStatus;
			AssertEquals("RVW", releaseStatus);
		}

		public void TestDispositionAddWithSOAndC1()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
					"SO20CR 16R190408                                                                " +
					"SO40RCNRU000026480059                                      00005930     00005930" +
					"SO42V3832799547 6136043901100416                                                " +
					"SO50100416160393BILL ON FILE                                                    " +
					"SO60100416160397ADMISSIBLE                                                      " +
					"SO70FDACOS100416160301DATA UNDER PGA REVIEW         01  00011001              02" +
					"SO70FDADRU100416160301DATA UNDER PGA REVIEW         01  00021001              02" +
					"SO70FDADRU100416160301DATA UNDER PGA REVIEW         01  00051001              02" +
					"SO70FDACOS100416160301DATA UNDER PGA REVIEW         01  00061001              02" +
					"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var dispositionDataCount = reLoadJob.OGADispositionCodes.Count;
			AssertEquals(4, dispositionDataCount);

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 71002057                                                    Y2       Y  3901SV9CQ00001";
			entry.Messages.Add(outgoing);

			var c1message = Factory.New<MQEDIMessage>();
			c1message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message.EM_MessageNum = "HYEDUSCMT_159673";
			c1message.EM_Status = EDIMessage.Status.Queued;
			c1message.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WO103901004  71002057 0127-391700700CNRUM3404107            16281100816         " +
									   "WO20CR 16R190408                                                                " +
									   "WO40RCNRU000026480059                                      00005930     00005930" +
									   "WO42V3832799547 6136043901100816                                                " +
									   "WO50100816025695BILL ARRIVED                                                    " +
									   "WO60100816025698RELEASED                                10081601                " +
									   "WO60100816025601ONE USG                                                         " +
									   "WO103901004  01941845 0127-391700700CNRUM3404107            16281100816         " +
									   "WO20CR 16R190408                                                                " +
									   "WO40RCNRU000026480059                                      00005930     00005930" +
									   "WO42V3832799547 6136043901100716                                                " +
									   "WO50100716201193BILL ON FILE                                                    " +
									   "WO60100716201197ADMISSIBLE                                                      " +
									   "WO103901004  01941845 0127-391700700CNRUDAY OF 20160928/360416265092816         " +
									   "WO20CR 16R190408                                                                " +
									   "WO40RCNRU000026480059                                      00005930     00005930" +
									   "WO42V3832799547 6136043901100516                                                " +
									   "WO50100516115793BILL ON FILE                                                    " +
									   "WO60100516115797ADMISSIBLE                                                      " +
									   "WO70FDADRU100516115707MAY PROCEED                   072200051001              02" +
									   "WO103901004  01941845 0127-391700700CNRUDAY OF 20160928/360416265092816         " +
									   "WO20CR 16R190408                                                                " +
									   "WO40RCNRU000026480059                                      00005930     00005930" +
									   "WO42V3832799547 6136043901100516                                                " +
									   "WO50100516115693BILL ON FILE                                                    " +
									   "WO60100516115697ADMISSIBLE                                                      " +
									   "WO70FDADRU100516115601DATA UNDER PGA REVIEW         072200021001              02" +
									   "WO103901004  01941845 0127-391700700CNRUDAY OF 20160928/360416265092816         " +
									   "WO20CR 16R190408                                                                " +
									   "WO40RCNRU000026480059                                      00005930     00005930" +
									   "WO42V3832799547 6136043901100416                                                " +
									   "WO50100416161593BILL ON FILE                                                    " +
									   "WO60100416161597ADMISSIBLE                                                      " +
									   "WO70FDACOS100416161501DATA UNDER PGA REVIEW         072200011001              02" +
									   "WO70FDACOS100416161501DATA UNDER PGA REVIEW         072200061001              02" +
									   "WO103901004  01941845 0127-391700700CNRUDAY OF 20160928/360416265092816         " +
									   "WO20CR 16R190408                                                                " +
									   "WO40RCNRU000026480059                                      00005930     00005930" +
									   "WO42V3832799547 6136043901100416                                                " +
									   "WO50100416160393BILL ON FILE                                                    " +
									   "WO60100416160397ADMISSIBLE                                                      " +
									   "WO103901004  01941845 0127-391700700CNRUDAY OF 20160928/360416265092816         " +
									   "WO20CR 16R190408                                                                " +
									   "WO40RCNRU000026480059                                      00005930     00005930" +
									   "WO42V3832799547 6136043901100416                                                " +
									   "WO50100416160393BILL ON FILE                                                    " +
									   "WO60100416160397ADMISSIBLE                                                      " +
									   "WO70FDACOS100416160301DATA UNDER PGA REVIEW         01  00011001              02" +
									   "WO70FDADRU100416160301DATA UNDER PGA REVIEW         01  00021001              02" +
									   "WO70FDADRU100416160301DATA UNDER PGA REVIEW         01  00051001              02" +
									   "WO70FDACOS100416160301DATA UNDER PGA REVIEW         01  00061001              02" +
									   "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			dispositionDataCount = reLoadJob.OGADispositionCodes.Count;
			AssertEquals("The Count should not be increase", 8, dispositionDataCount);
		}

		public void TestOGADispositionDataCount()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
					"SO20CR 6NYC00663                                                                " +
					"SO40MACLU6A62S6680206                                                           " +
					"SO40HSSLLCHS222271                                         00000001     00000001" +
					"SO50051116121894BILL DEPARTED                                                   " +
					"SO60052616163799RELEASE SUSPENDED                                               " +
					"SO60052616163790UNDER CBP REVIEW                                                " +
					"SO70FDARAD012216122901DATA UNDER PGA REVIEW         01  002001                  " +
					"SO70NHTOFF012216122901DATA UNDER PGA REVIEW         07  001001                  " +
					"SO70NHTOFF012216122907MAY PROCEED                   07  002001                  " +
					"SO70FDARAD012216122907MAY PROCEED                   07  001001                  " +
					"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var dispositionDataCount = reLoadJob.OGADispositionCodes.Count;
			AssertEquals(4, dispositionDataCount);

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 71002057                                                    Y2       Y  3901SV9CQ00001";
			entry.Messages.Add(outgoing);

			var c1message = Factory.New<MQEDIMessage>();
			c1message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message.EM_MessageNum = "HYEDUSCMT_159673";
			c1message.EM_Status = EDIMessage.Status.Queued;
			c1message.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
									   "WO40MACLU6A62S6680206                                                           " +
									   "WO40HSSLLCHS222271                                         00000001     00000001" +
									   "WO50052716101095BILL ARRIVED                                                    " +
									   "WO60052716101098RELEASED                                05271601                " +
									   "WO60052716101001ONE USG                                                         " +
									   "WO70FDARAD012216122901DATA UNDER PGA REVIEW         01  002001                  " +
									   "WO70NHTOFF012216122901DATA UNDER PGA REVIEW         07  001001                  " +
									   "WO70NHTOFF012216122907MAY PROCEED                   07  002001                  " +
									   "WO70FDARAD012216122907MAY PROCEED                   07  001001                  " +
									   "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			dispositionDataCount = reLoadJob.OGADispositionCodes.Count;
			AssertEquals("The Count should not be increase", 4, dispositionDataCount);

			var c1message2 = Factory.New<MQEDIMessage>();
			c1message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message2.EM_MessageNum = "HYEDUSCMT_159673";
			c1message2.EM_Status = EDIMessage.Status.Queued;
			c1message2.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
										"WO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
										"WO40MACLU6A62S6680206                                                           " +
										"WO40HSSLLCHS222271                                         00000001     00000001" +
										"WO50052716101095BILL ARRIVED                                                    " +
										"WO60052716101098RELEASED                                05271601                " +
										"WO60052716101001ONE USG                                                         " +
										"WO70NHTOFF012216123902                              02  002001                  " +
										"WO70FDARAD012216123902                              02  001001                  " +
										"Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			dispositionDataCount = reLoadJob.OGADispositionCodes.Count;
			AssertEquals("DispositionData Count should be increase", 6, dispositionDataCount);
		}

		public void TestPGAHeaderEventsShouldPostWhenStatusUpdate()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
					"SO20CR 6NYC00663                                                                " +
					"SO40MACLU6A62S6680206                                                           " +
					"SO50051116121894BILL DEPARTED                                                   " +
					"SO70FDAFOO072116140001DATA UNDER PGA REVIEW         01  00011001              02" +
					"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			declaration.Logs.GetAllLogs().Load();

			var concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - PGA Line Status FDA 01");
			Assert(declaration.Logs.HasLogWith(concurrenceLogQuery));

			concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - PGA FDA 01");
			Assert(declaration.Logs.HasLogWith(concurrenceLogQuery));

			var message2 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
				"SO20CR 6NYC00663                                                                " +
				"SO40MACLU6A62S6680206                                                           " +
				"SO50051116121894BILL DEPARTED                                                   " +
				"SO70FDAFOO072116140701DATA UNDER PGA REVIEW         01  00011001              02" +
				"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			declaration.Logs.GetAllLogs().Load();

			concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - PGA Line Status FDA 01");
			var pgaEntryLogs = declaration.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageStatusChange.Code && x.SL_Reference == "SO - PGA Line Status FDA 01");
			AssertEquals(2, pgaEntryLogs.Count());

			var pgaLineLogs = declaration.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageStatusChange.Code && x.SL_Reference == "SO - PGA FDA 01");
			AssertEquals(1, pgaLineLogs.Count());

			var message3 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
				"SO20CR 6NYC00663                                                                " +
				"SO40MACLU6A62S6680206                                                           " +
				"SO50051116121894BILL DEPARTED                                                   " +
				"SO70FDAFOO072116141707MAY PROCEED                   072200011001              02" +
				"Y  1601SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			declaration.Logs.GetAllLogs().Load();

			concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - PGA Line Status FDA 07");
			Assert(declaration.Logs.HasLogWith(concurrenceLogQuery));

			concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - PGA FDA 07");
			Assert(declaration.Logs.HasLogWith(concurrenceLogQuery));
		}

		public void TestCS00426144_DispositionDataCount()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
					"SO20CR 6NYC00663                                                                " +
					"SO40MACLU6A62S6680206                                                           " +
					"SO40HSSLLCHS222271                                         00000001     00000001" +
					"SO50051116121894BILL DEPARTED                                                   " +
					"SO60052616163799RELEASE SUSPENDED                                               " +
					"SO60052616163790UNDER CBP REVIEW                                                " +
					"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var dispositionDataCount = reLoadJob.DispositionCodes.Count;
			var releaseStatus = reLoadJob.ReleaseStatus;
			AssertEquals("RVW", releaseStatus);
			AssertEquals(2, dispositionDataCount);

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 70038383                                                    Y2       Y  3901SV9CQ00001";

			entry.Messages.Add(outgoing);

			var c1message = Factory.New<MQEDIMessage>();
			c1message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message.EM_MessageNum = "HYEDUSCMT_159673";
			c1message.EM_Status = EDIMessage.Status.Queued;
			c1message.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
									   "WO40MACLU6A62S6680206                                                           " +
									   "WO40HSSLLCHS222271                                         00000001     00000001" +
									   "WO50052716101095BILL ARRIVED                                                    " +
									   "WO60052716101098RELEASED                                05271601                " +
									   "WO60052716101001ONE USG                                                         " +
									   "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			dispositionDataCount = reLoadJob.DispositionCodes.Count;
			releaseStatus = reLoadJob.ReleaseStatus;
			AssertEquals("REL", releaseStatus);
			AssertEquals(4, dispositionDataCount);

			var c1message2 = Factory.New<MQEDIMessage>();
			c1message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message2.EM_MessageNum = "HYEDUSCMT_159673";
			c1message2.EM_Status = EDIMessage.Status.Queued;
			c1message2.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
									   "WO40MACLU6A62S6680206                                                           " +
									   "WO40HSSLLCHS222271                                         00000001     00000001" +
									   "WO50052716101095BILL ARRIVED                                                    " +
									   "WO60052716101098RELEASED                                05271601                " +
									   "WO60052716101001ONE USG                                                         " +
									   "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			dispositionDataCount = reLoadJob.DispositionCodes.Count;
			releaseStatus = reLoadJob.ReleaseStatus;
			AssertEquals("REL", releaseStatus);
			AssertEquals("DispositionData Count should not be increase", 4, dispositionDataCount);
		}

		public void TestCS00426144_ReleaseStatus()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
					"SO20CR 6NYC00663                                                                " +
					"SO40MACLU6A62S6680206                                                           " +
					"SO40HSSLLCHS222271                                         00000001     00000001" +
					"SO50051116121894BILL DEPARTED                                                   " +
					"SO60051116121890UNDER CBP REVIEW                                                " +
					"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var releaseDate = reLoadJob.JE_EntryAuthorisationDate;
			var releaseStatus = reLoadJob.ReleaseStatus;
			Assert(!releaseDate.IsValid);
			AssertEquals("RVW", releaseStatus);

			var message2 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
				"SO20CR 6NYC00663                                                                " +
				"SO40MACLU6A62S6680206                                                           " +
				"SO40HSSLLCHS222271                                         00000001     00000001" +
				"SO50051316054695BILL ARRIVED                                                    " +
				"SO60051316054690UNDER CBP REVIEW                                                " +
				"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			releaseDate = reLoadJob.JE_EntryAuthorisationDate;
			releaseStatus = reLoadJob.ReleaseStatus;

			Assert(!releaseDate.IsValid);
			AssertEquals("RVW", releaseStatus);

			var message3 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
				"SO20CR 6NYC00663                                                                " +
				"SO40MACLU6A62S6680206                                                           " +
				"SO40HSSLLCHS222271                                         00000001     00000001" +
				"SO50051716141395BILL ARRIVED                                                    " +
				"SO60051716141398RELEASED                                05171601                " +
				"SO60051716141301ONE USG                                                         " +
				"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			releaseDate = reLoadJob.JE_EntryAuthorisationDate;
			releaseStatus = reLoadJob.ReleaseStatus;
			Assert(releaseDate.IsValid);
			AssertEquals("REL", releaseStatus);

			var message4 = CreateStatusMessage(
				"B001601SV9SO                                                                    " +
				"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
				"SO20CR 6NYC00663                                                                " +
				"SO40MACLU6A62S6680206                                                           " +
				"SO40HSSLLCHS222271                                         00000001     00000001" +
				"SO50052616163795BILL ARRIVED                                                    " +
				"SO60052616163799RELEASE SUSPENDED                                               " +
				"SO60052616163790UNDER CBP REVIEW                                                " +
				"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			releaseDate = reLoadJob.JE_EntryAuthorisationDate;
			releaseStatus = reLoadJob.ReleaseStatus;
			Assert(!releaseDate.IsValid);
			AssertEquals("RVW", releaseStatus);

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 70038383                                                    Y2       Y  3901SV9CQ00001";

			entry.Messages.Add(outgoing);

			var c1message = Factory.New<MQEDIMessage>();
			c1message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message.EM_MessageNum = "HYEDUSCMT_159673";
			c1message.EM_Status = EDIMessage.Status.Queued;
			c1message.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
									   "WO40MACLU6A62S6680206                                                           " +
									   "WO40HSSLLCHS222271                                         00000001     00000001" +
									   "WO50052716101095BILL ARRIVED                                                    " +
									   "WO60052716101098RELEASED                                05271601                " +
									   "WO60052716101001ONE USG                                                         " +
									   "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			releaseDate = reLoadJob.JE_EntryAuthorisationDate;
			releaseStatus = reLoadJob.ReleaseStatus;
			Assert(releaseDate.IsValid);
			AssertEquals("REL", releaseStatus);
		}

		public void TestIncludeOneUSGMessage()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60032416163201ONE USG                                                         " +
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(reLoadJob.DispositionCodes.Count, 1);
			Assert(reLoadJob.DispositionCodes.Cast<DispositionData>().Any(x => x.US_Code == "01"));

			declaration.Logs.GetAllLogs().Load();

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - 1USG");
			Assert(declaration.Logs.HasLogWith(logQuery));
		}

		public void TestOnlyOneSoMessage()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDARAD012216122901DATA UNDER PGA REVIEW         01  002001                  " +
					"SO70NHTOFF012216122901DATA UNDER PGA REVIEW         07  001001                  " +
					"SO70NHTOFF012216122907MAY PROCEED                   07  002001                  " +
					"SO70FDARAD012216122907MAY PROCEED                   07  001001                  " +
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(reLoadJob.EntryPGACusDispositions.Count, 2);
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "NHT" && x.CDI_Status == "07"));
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "FDA" && x.CDI_Status == "07"));
		}

		public void TestTwoOrderSoMessages()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDARAD012216122801DATA UNDER PGA REVIEW         01  001001                  " +
					"SO70NHTOFF012216122801DATA UNDER PGA REVIEW         01  001001                  " +

					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var message2 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO20CR B00160703                                                                " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO70FDARAD012216122901DATA UNDER PGA REVIEW         01  001001                  " +
				"SO70NHTOFF012216122907MAY PROCEED                   07  001001                  " +
				"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(reLoadJob.EntryPGACusDispositions.Count, 2);
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "NHT" && x.CDI_Status == "07"));
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "FDA" && x.CDI_Status == "01"));
		}

		public void TestTwoSoMessagesShouldNotUpdate()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDARAD012216122801DATA UNDER PGA REVIEW         01  001001                  " +
					"SO70NHTOFF012216122801DATA UNDER PGA REVIEW         01  001001                  " +

					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var message2 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO20CR B00160703                                                                " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO70FDARAD012216122701DATA UNDER PGA REVIEW         01  001001                  " +
				"SO70NHTOFF012216122707MAY PROCEED                   07  001001                  " +
				"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(reLoadJob.EntryPGACusDispositions.Count, 2);
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "NHT" && x.CDI_Status == "01"));
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "FDA" && x.CDI_Status == "01"));
		}

		public void TestOGADispositionDataLogMSC_01()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60093013012597ADMISSIBLE                                                      " +
					"SO70EPAPST103013143907EPA DATA REVIEW             020112333 777THRU5551215 1    " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var ogaDispositionCodes = reLoadJob.OGADispositionCodes;
			AssertEquals(1, ogaDispositionCodes.Count);

			var concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - PGA Line Status EPA 01");
			Assert(reLoadJob.Logs.HasLogWith(concurrenceLogQuery));
		}

		public void TestOGADispositionDataLogMSC_02()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60093013012597ADMISSIBLE                                                      " +
					"SO70EPAPST103013143907EPA DATA REVIEW             020212333 777THRU5551215 1    " +
					"SO70EPAPST103013143907EPA DATA REVIEW             020212333 777THRU5551215 1    " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var ogaDispositionCodes = reLoadJob.OGADispositionCodes;
			AssertEquals(1, ogaDispositionCodes.Count);

			var concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - PGA Line Status EPA 02");
			Assert(reLoadJob.Logs.HasLogWith(concurrenceLogQuery));
		}

		public void TestOGADispositionDataLogMSC_03()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60093013012597ADMISSIBLE                                                      " +
					"SO70EPAPST103013143901EPA DATA REVIEW             020612333 777THRU5551215 1    " +
					"SO70EPAPST103013143901EPA DATA REVIEW             020712333 777THRU5551215 1    " +
					"SO70EPAPST103013143901EPA DATA REVIEW             021112333 777THRU5551215 1    " +
					"SO70FDA   103013143901FDA DATA REVIEW             020712333 777THRU5551215 1    " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

			var concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - PGA Line Status EPA 06, 07, 11");
			Assert(reLoadJob.Logs.HasLogWith(concurrenceLogQuery));

			concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - PGA Line Status FDA 07");
			Assert(reLoadJob.Logs.HasLogWith(concurrenceLogQuery));
		}

		public void TestCalculateDeclarationReleaseStatusForSO50_1()
		{
			var declaration = GetDeclaration("71002057");
			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "00591325206";
			var entryHeader = declaration.ActiveEntryHeaders[0];
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO50120915080952MANIFEST HOLD AGRICULTURE                                       " +
					"SO60120915080990UNDER CBP REVIEW                                                " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			declaration.Reload();
			AssertEquals(CRLReleaseStatusList.Codes.RVW, declaration.ReleaseStatus);
		}

		public void TestDoNotRelyOnDispositionDateTimeWhenItComesToPostingFromSO70CS00399336()
		{
			var declaration = GetDeclaration("71002057");
			//PGA 01 - Data under review
			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RHLCUPI3151202078                                      00004956     00004956
SO50012016120594BILL DEPARTED                                                   
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory = new BusinessObjectFactory();
			var declarationLoaded = factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(1, declarationLoaded.OGADispositionCodes.Count);
			AssertEquals("01", declarationLoaded.OGADispositionCodes[0].US_Code);

			var message1 = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RHLCUPI3151202078                                      00004956     00004956
SO50012016142094BILL DEPARTED                                                   
SO60012016142098RELEASED                                01201601                
Y  4601267SO00000".Replace("\r\n", ""));

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			//PGA 07 - May Proceed
			var message2 = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RHLCUPI3151202078                                      00004956     00004956
SO50012016135994BILL DEPARTED                                                   
SO60012016135998RELEASED                                01201601                
SO70FDAFOO012016135407MAY PROCEED                   0722001001                  
Y  4601267SO00000".Replace("\r\n", ""));

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			factory = new BusinessObjectFactory();
			declarationLoaded = factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(2, declarationLoaded.OGADispositionCodes.Count);
			AssertNotNull("SO50 in this message has an earlier disposition date, but in terms of PGA disposition, this is the latest message", declarationLoaded.OGADispositionCodes.Cast<OGADispositionData>().FirstOrDefault(x => x.US_Code == "07"));
		}

		public void TestSimplifiedEntryDispositionsChanged()
		{
			var declaration = GetDeclaration("71001299");
			declaration.JE_MasterBill = "ALP31222406";
			AssertEquals("Precondition: declaration has one bill", 1, declaration.Bills.Count);

			var seEntry = declaration.CustomsEntryHeaders.AddNew();
			seEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			seEntry.EntryNumber = "71001299";

			var request = new AutoSendStatementDateChangeRequest();
			request.OverrideAllOrByOrganisation = "ALL";
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, request);

			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.US_EntryDate = ZDateTime.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(2);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;

			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.EntryNumber = "71001299";

			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			dailyStatement.B2_StatementNumber = "11224488";
			dailyStatement.B2_GC = GlbCompany.CurrentCompany.PK;
			dailyStatement.B2_StatementAmount = 5.55m;

			var statementLine = Factory.New<CusStatementLine>();
			statementLine.B3_B2 = dailyStatement.PK;
			statementLine.B3_BrokerReference = declaration.JE_DeclarationReference;
			statementLine.B3_EntryNum = "71001299";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryFilerCode = "SV9";
			statementLine.B3_EntryStatus = "PA";
			Factory.Save();

			var message1 = CreateStatusMessage("B001101SV9SO                                00                                  " +
					"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
					"SO20CR B00159843                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO50022713162694BILL DEPARTED                                                   " +
					"SO60022713162696DOCUMENT REQUIRED                               03              " +
					"Y  3910SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			AssertDispositions(message1, declaration.DispositionCodes, 1);
			var masterBill = declaration.PrimaryMasterBill;
			AssertEquals("Bill dispositions", 1, masterBill.DispositionCodes.Count);

			var message2 = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
				"SO20CR B00159843                                                                " +
				"SO40R    ALP31222406                                       00000600             " +
				"SO50022713165594BILL DEPARTED                                                   " +
				"SO60022713165503PENDING INTENSIVE EXAM                                          " +
				"SO60022713165596DOCUMENT REQUIRED                               03              " +
				"Y  3910SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.DispositionCodes.Load();

			masterBill.DispositionCodes.Load();
			AssertDispositions(message2, declaration.DispositionCodes, 3);
			AssertEquals("New Bill disposition should be added, because time is different", 2, masterBill.DispositionCodes.Count);
			AssertEquals("Source", BillDispositionSourceList.Codes.SO, masterBill.DispositionCodes[0].US_Source);
			AssertEquals("Source", BillDispositionSourceList.Codes.SO, masterBill.DispositionCodes[1].US_Source);

			var message3 = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
				"SO20CR B00159843                                                                " +
				"SO40R    ALP31222406                                       00000600             " +
				"SO50022713172094BILL DEPARTED                                                   " +
				"SO60022713172098RELEASED                                02271301                " +
				"Y  3910SV9SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.DispositionCodes.Load();
			masterBill.DispositionCodes.Load();
			AssertDispositions(message3, declaration.DispositionCodes, 4);
			AssertEquals("New Bill disposition should be added, because time is different", 3, masterBill.DispositionCodes.Count);
			declaration.Logs.GetAllLogs().Load();
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus + " - REL"));

			declaration.DispositionCodes.Sort(DispositionData.Schema.US_Order, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("96", declaration.DispositionCodes[0].US_Code);
			AssertEquals("03", declaration.DispositionCodes[1].US_Code);
			AssertEquals("96", declaration.DispositionCodes[2].US_Code);
			AssertEquals("98", declaration.DispositionCodes[3].US_Code);

			declaration.Reload();
			AssertNotEquals("released", ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);
			AssertEquals(CRLReleaseStatusList.Codes.REL, declaration.ReleaseStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertNotNull(email);
			Assert("Should contains a reason why supress STU message.", email.Body.Contains(AutomaticSTUConditionChecker.NotificationSuppressSTUPaymentAuthorized));

			var message4 = CreateStatusMessage("B005206OHLSO                                                                    " +
				"SO105206SV9  71001299 0123-456789012AL                      2246 021413         " +
				"SO20CR SLHR41889967                                                             " +
				"SO40M    93240203811                                                            " +
				"SO40H    LHR41889967                                       00000008     00000008" +
				"SO50032014115695BILL ARRIVED                                                    " +
				"SO60032014115699RELEASE SUSPENDED                                               " +
				"SO60032014115696DOCUMENT REQUIRED                               02              " +
				"Y  5206OHLSO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			message4.Reload();

			AssertEquals(declaration.ActiveEntryHeaders.SimplifiedEntry.PK, message4.EM_LinkUniqueID);
			declaration.Reload();
			AssertEquals("release suspended", ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);
			declaration.Logs.GetAllLogs().Load();
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.AuthorisationWithdrawn, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus + " - DOC"));

			AssertEquals(CRLReleaseStatusList.Codes.DOC, new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK).ReleaseStatus);
			dailyStatement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentFailed;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(2);
			var message5 = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
				"SO20CR B00159843                                                                " +
				"SO40R    ALP31222406                                       00000600             " +
				"SO50022713172094BILL DEPARTED                                                   " +
				"SO60022713172098RELEASED                                02271301                " +
				"Y  3910SV9SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			declaration.Logs.GetAllLogs().Load();
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertNotNull(email);
			AssertEquals("Should contains a reason why supress STU message.", false, email.Body.Contains(AutomaticSTUConditionChecker.NotificationSuppressSTUPaymentAuthorized));
		}

		public void TestACECargoReleaseStatusEmailReceipients()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "~AA";
			staff1.GS_EmailAddress = "blah@com.au";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "~BB";
			staff2.GS_EmailAddress = "blah2@com.au";
			Factory.Save();

			var declaration = GetDeclaration("71001299");
			declaration.JE_MasterBill = "ALP31222406";
			declaration.JE_GS_NKCusAgent = staff2.GS_Code;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var messageSend = Factory.New<MQEDIMessage>();
			messageSend.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageSend.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			messageSend.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageSend.EM_MessageNum = "HYEDUSCMT_148784";
			messageSend.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148784     " +
										 "SE10USV9  71002958 01EI 23-45678901240800000001201101                           " +
										 "SE15R    00591325210                                                            " +
										 "SE16COA 6020 09091300000450                                                     " +
										 "SE15M    AAA00178975                                                            " +
										 "SE15H    AAAHB343234                                       00000002CU           " +
										 "SE15H    AAAHB000006                                       00000009VY           " +
										 "SE15M    BBB002124564                                                           " +
										 "SE15H    BBB00245454HB                                                          " +
										 "SE15S    BBB00215454SHB                                    00000009AM           " +
										 "SE20CR B00160864                                                                " +
										 "Y  1101SV9SE00010";
			messageSend.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			messageSend.EM_SystemCreateUser = staff1.GS_Code;

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.Messages.Add(messageSend);

			var message = CreateStatusMessage("B001101SV9SO                                00                                  " +
					"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
					"SO20CR B00159843                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO50022713172094BILL DEPARTED                                                   " +
					"SO60022713172098RELEASED                                02271301 16             " +
					"SO70ACE000103013143911EPA DATA REVIEW             020112333 777THRU5551215 1    " +
					"Y  3910SV9SO00000");

			message.EM_SystemCreateUser = staff1.GS_Code;
			message.EM_MessageNum = "HYEDUSCMT_148784";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("ACE Cargo Release Status"));
			AssertNotNull(email);

			AssertEquals(true, email.Recipients.Contains(staff1.GS_EmailAddress));
			AssertEquals(false, email.Recipients.Contains(staff2.GS_EmailAddress));
		}

		public void TestPermitClosedWhenCargoReleaseIsDeletionIsAccepted()
		{
			// - create FTZ declaration

			JobDeclaration declaration = JobDeclarationTest.CreateFTZWeeklyEstimateDeclaration(Factory, "40000007");

			// this line creates permit
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 14);

			Factory.Save();
			// - check that permit created and is not closed

			List<CusPermitHeader> permits = declaration.FindRelatedPermits();
			AssertEquals(1, permits.Count);

			CusPermitHeader permit = permits[0];
			AssertEquals(false, permit.CPH_IsClosed);

			// - send CargoRelease message
			var messageSend = Factory.New<MQEDIMessage>();
			messageSend.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageSend.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			messageSend.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageSend.EM_MessageNum = "HYEDUSCMT_188589";
			messageSend.EM_MessageText = "B  2501SV9SE                                  3901SV9  1   HYEDUSCMT_188589     " +
										 "SE10ASV9  40000007 06EI 58-123456789  800000010002501  2501                     " +
										 "SE11W101617F231    FTZ000123                                                    " +
										 "SE13CARGOWISE SUPPORT                                                           " +
										 "SE20CR B00169535                                                                " +
										 "SE20KIIY                                                                        " +
										 "SE30MF A1 CHEMICALS PTY LTD                                                     " +
										 "SE351519 HOMEDALE ROAD                   15TEST                                 " +
										 "SE36BANKSTOWN                                   2214           AU               " +
										 "SE30CN                                    EI 58-123456789                       " +
										 "SE30BY                                    EI 58-123456789                       " +
										 "SE40001AU                                                                       " +
										 "SE41       00000000                                                             " +
										 "SE6001011000100000001000                                                        " +
										 "Y  2501SV9SE                                                                    ";
			messageSend.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
			messageSend.EM_SystemCreateUser = User.ServiceUserCode;

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.Messages.Add(messageSend);

			// - receive Cargo Release Deletion Accepted messsage
			var message = CreateStatusMessage("B003901SV9SO                                               HYEDUSCMT_188589     " +
												  "SO101101SV9  40000007 0613-147927000                             112315         " +
												  "SO20CR B00169535                                                                " +
												  "SO60112315183023ENTRY CANCELLED                                                 " +
												  "Y  3901SV9SO00000                                                               ");

			message.EM_SystemCreateUser = User.ServiceUserCode;
			message.EM_MessageNum = "HYEDUSCMT_188589";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			// - check that permit is closed

			var newFactory = new BusinessObjectFactory();
			var permitReloaded = newFactory.Load<CusPermitHeader>(permit.PK);

			AssertEquals(true, permitReloaded.CPH_IsClosed);
			Assert("has closed log entry", permitReloaded.Logs.HasLogWith(
				l => l.SL_SE_NKEvent == "PCL" && l.SL_Reference == "Permit closed"
			));
		}

		public void TestDocumentTypeAndPGAEntryHoldType()
		{
			var declaration = GetDeclaration("71001299");
			declaration.JE_MasterBill = "ALP31222406";

			var message = CreateStatusMessage("B001101SV9SO                                00                                  " +
					"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
					"SO20CR B00159843                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO50022713172094BILL DEPARTED                                                   " +
					"SO60022713172098RELEASED                                02271301 16             " +
					"SO70ACE000103013143911EPA DATA REVIEW             020112333 777THRU5551215 1    " +
					"Y  3910SV9SO00000");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("ACE Cargo Release Status"));
			AssertNotNull(email);
			Assert(email.Body.Contains("<td>Document Type</td><td>16 - APTL Foreign Certificate</td>"));
			Assert(email.Body.Contains("PGA Entry Hold Type"));
			Assert(email.Body.Contains("15 - APTL CITES Certificate"));
			Assert(email.Body.Contains("1 " + new PGAEntryHoldTypeCodeList().GetCodeFromDescription("1")));

			Assert(email.Body.Contains("CBP Line Status"));
		}

		public void TestDoNotUpdateReleaseDateIfBelatedReleaseNotificationMessages()
		{
			var declaration = GetDeclaration("71001299");
			declaration.JE_MasterBill = "ALP31222406";
			AssertEquals("Precondition: declaration has one bill", 1, declaration.Bills.Count);

			var message3 = CreateStatusMessage("B001101SV9SO                                00                                  " +
					"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
					"SO20CR B00159843                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO50022713172094BILL DEPARTED                                                   " +
					"SO60022713172098RELEASED                                02271301                " +
					"Y  3910SV9SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			declaration.Reload();
			declaration.DispositionCodes.Load();
			AssertEquals(new ZDate(2013, 2, 27), declaration.JE_EntryAuthorisationDate.Date);
			AssertEquals(1, declaration.DispositionCodes.Count);

			var message4 = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
				"SO20CR B00159843                                                                " +
				"SO40R    ALP31222406                                       00000600             " +
				"SO50022713162094BILL DEPARTED                                                   " +
				"SO60022713162098RELEASED                                02281301                " +
				"Y  3910SV9SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			declaration.Reload();
			declaration.DispositionCodes.Load();
			AssertEquals(new ZDate(2013, 2, 27), declaration.JE_EntryAuthorisationDate.Date);
			AssertEquals(1, declaration.DispositionCodes.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("ACE Cargo Release Status"));
			AssertNotNull(email);
			AssertContains(ACEABIProcessor.BelatedDispositionReleaseNotificationWarning, email.Body);
		}

		[TestDate(2013, 02, 11)]
		public void TestSendAutoEntrySummaryQuery()
		{
			var declaration = GetDeclaration("71001299");
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "SV9";

			declaration.US_PaymentDueDate = new ZDateTime(2013, 02, 05);
			DataRegistry.Business.USCustomsDataRegistry.Instance.AutoQueryEntrySummaries.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, true);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "71001299";

			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry2.EntryNumber = "71001299";

			var message3 = CreateStatusMessage("B001101SV9SO                                00                                  " +
					"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
					"SO20CR B00159843                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO50022713172094BILL DEPARTED                                                   " +
					"SO60022713172098RELEASED                                02271301                " +
					"Y  3910SV9SO00000");
			message3.EM_LinkUniqueID = entry.PK;
			message3.EM_LinkTable = entry.TableName;
			Factory.Save();

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummary;
			entry.Messages.Add(message);

			new ABIIncomingMessageProcessor().ExecuteBatch();

			declaration.Reload();
			AssertEquals("Release date should have been set", new ZDateTime(2013, 02, 27, 00, 00, 0), declaration.JE_EntryAuthorisationDate);
			AssertNotEquals("Payment Due Date should be recalculated", new ZDateTime(2013, 02, 05), declaration.US_PaymentDueDate);
			entry.Messages.Load();
			AssertEquals("New Entry Summary Query should be sent, because Release Date updated via RR message", 1,
				entry.Messages.OfType<MQEDIMessage>().Count(x => x.EM_MessageType == ApplicationIdentifierCodeList.Codes.QueryEntrySummary));
		}

		public void TestProcessCancelledEntryWithPreliminaryStatement()
		{
			var declaration = GetDeclaration("01000048");
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "01000048";
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			Factory.Save();

			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			AssertEquals("Precondition: Release Status should be 'Released'", CRLReleaseStatusList.Codes.REL, declaration.ReleaseStatus);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statement.B2_StatementNumber = "800400560";
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "01000048";
			line.B3_EntryFilerCode = "XJ5";
			line.B3_Status = StatementLineStatusList.Codes.Active;

			var charge = line.Charges.AddNew();
			charge.B4_ChargeAmount = 15.94m;
			charge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;

			var charge2 = line.Charges.AddNew();
			charge2.B4_ChargeAmount = 20m;
			charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.HMF;

			line.B3_CustomsFeesTotal = 35.94m;
			statement.B2_StatementAmount = 35.94m;

			var message4 = CreateStatusMessage("B001101SV9SO                                00                                  " +
					"SO101901XJ5  01000048 0123-456789012AL                      2246 021413         " +
					"SO20CR B00159843                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO60022713162023ENTRY CANCELLED                                                 " +
					"Y  3910SV9SO00000");

			Factory.Save();
			AssertEquals(statement.PK, declaration.RelatedStatement.PK);

			new ABIIncomingMessageProcessor().ExecuteBatch();
			entry.Reload();
			AssertEquals("Entry shouldn't have been canceled", ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, entry.CH_Status);

			var newFactory = new BusinessObjectFactory();
			var declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Declaration Release Date should be cleared", ZDateTime.Empty, declarationReloaded.JE_EntryAuthorisationDate);
			AssertEquals("Declaration Release Status should be empty", CRLReleaseStatusList.Codes.CAN, declarationReloaded.ReleaseStatus);

			line.Reload();
			AssertEquals(StatementLineStatusList.Codes.Deleted, line.B3_Status);
			statement.Reload();
			AssertEquals("If only one statement line, statement should have deleted status", StatementHeaderStatusList.Codes.Deleted, statement.B2_Status);

			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			line.B3_Status = StatementLineStatusList.Codes.Active;

			var line2 = statement.StatementLines.AddNew();
			line2.B3_EntryNum = "00000064";
			line2.B3_EntryFilerCode = "XJ5";
			line2.B3_Status = StatementLineStatusList.Codes.Active;

			var charge3 = line2.Charges.AddNew();
			charge3.B4_ChargeAmount = 20.50m;
			charge3.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;

			var charge4 = line.Charges.AddNew();
			charge4.B4_ChargeAmount = 10.50m;
			charge4.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.HMF;

			line2.B3_CustomsFeesTotal = 31m;
			statement.B2_StatementAmount = 66.94m;

			var message5 = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101901XJ5  01000048 0123-456789012AL                      2246 021413         " +
				"SO20CR B00159843                                                                " +
				"SO40R    ALP31222406                                       00000600             " +
				"SO60022713162023ENTRY CANCELLED                                                 " +
				"Y  3910SV9SO00000");
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			entry.Reload();
			AssertEquals("Entry shouldn't have been canceled", ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, entry.CH_Status);

			line.Reload();
			AssertEquals(StatementLineStatusList.Codes.Deleted, line.B3_Status);
			line2.Reload();
			AssertEquals(StatementLineStatusList.Codes.Active, line2.B3_Status);
			statement.Reload();
			AssertEquals("Statement should not be cancelled if more than one statement line", StatementHeaderStatusList.Codes.Preliminary, statement.B2_Status);
		}

		public void TestRemarksForStatement()
		{
			var declaration = GetDeclaration("01000048");
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "01000048";
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			Factory.Save();

			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			AssertEquals("Precondition: Release Status should be 'Released'", CRLReleaseStatusList.Codes.REL, declaration.ReleaseStatus);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statement.B2_StatementNumber = "800400560";
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "01000048";
			line.B3_EntryFilerCode = "XJ5";
			line.B3_Status = StatementLineStatusList.Codes.Active;

			var charge = line.Charges.AddNew();
			charge.B4_ChargeAmount = 15.94m;
			charge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;

			var charge2 = line.Charges.AddNew();
			charge2.B4_ChargeAmount = 20m;
			charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.HMF;

			line.B3_CustomsFeesTotal = 35.94m;
			statement.B2_StatementAmount = 35.94m;

			line.B3_Status = StatementLineStatusList.Codes.Active;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			var message6 = CreateStatusMessage("B001101SV9SO                                00                                  " +
									"SO101901XJ5  01000048 0123-456789012AL                      2246 021413         " +
									"SO20CR B00159843                                                                " +
									"SO40R    ALP31222406                                       00000600             " +
									"SO60022713162023ENTRY CANCELLED                                                 " +
									"Y  3910SV9SO00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertNotNull(email);
			Assert("Should contains remarks.", email.Body.Contains("This entry has just been canceled, but it is on a statement"));
		}

		public void TestDispositions()
		{
			var declaration = GetDeclaration("71002057");
			declaration.JE_MasterBill = "00591325206";
			declaration.JE_HouseBill = "HAWB001";

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO50093013012593BILL ON FILE                                                    " +
					"SO60093013012597ADMISSIBLE                                                      " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var message1 = CreateStatusMessage("B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      2006 0909131        " +
				"SO20CR B00160702                                                                " +
				"SO40M    00591325206                                                            " +
				"SO40H    HAWB001                                           00000100AT   00000250" +
				"SO427006005     613901295003261403271400000250                                  " +
				"SO50093013015294BILL DEPARTED                                                   " +
				"SO40M    00591325206                                                            " +
				"SO40H    HAWB001                                           00000100AT   00000250" +
				"SO50093013014294BILL DEPARTED                           YCOA 4006 0909133901    " +
				"SO60093013014298RELEASED                                09301301                " +
				"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.DispositionCodes.Load();

			var masterBill = declaration.PrimaryMasterBill;
			masterBill.DispositionCodes.Load();
			AssertEquals("Disposition codes processed", 1, masterBill.DispositionCodes.Count);
			AssertEquals("Disposition codes processed for Master Bill", "93", masterBill.DispositionCodes[0].US_Code);
			AssertEquals("Source", BillDispositionSourceList.Codes.SO, masterBill.DispositionCodes[0].US_Source);

			var houseBill = declaration.PrimaryHouseBill;
			houseBill.DispositionCodes.Load();
			AssertEquals("Disposition codes processed", 2, houseBill.DispositionCodes.Count);
			AssertEquals("Source", BillDispositionSourceList.Codes.SO, houseBill.DispositionCodes[0].US_Source);
			houseBill.Reload();
			AssertEquals("Split Bill", true, houseBill.US_SESplitShip);

			AssertEquals("Declaration disposition codes processed", 2, declaration.DispositionCodes.Count);
			declaration.Logs.GetAllLogs().Load();
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus + " - REL"));

			declaration.DispositionCodes.Sort(DispositionData.Schema.US_Order, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("97", declaration.DispositionCodes[0].US_Code);
			AssertEquals("98", declaration.DispositionCodes[1].US_Code);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertNotNull(email);
		}

		public void TestPGADispositions()
		{
			var declaration = GetDeclaration("71002057");
			declaration.JE_MasterBill = "00591325206";

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60093013012597ADMISSIBLE                                                      " +
					"SO70ACE000123013143913EPA DATA REVIEW             010411222 365THRU3334444100   " +
					"SO711 1234567890  0616150910  100144                                            " +
					"SO712 2222222222  0316150910  122126                                            " +
					"SO72COMMENT1.                                                                   " +
					"SO72COMMENT2.                                                                   " +
					"SO70ACE000103013143911EPA DATA REVIEW             020112333 777THRU5556666100   " +
					"SO711 3333333333  0616150910  102103                                            " +
					"SO712 4444444444  0316150910  110111                                            " +
					"SO72COMMENT3.                                                                   " +
					"SO72COMMENT4.                                                                   " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Declaration disposition codes processed", 1, declarationReloaded.DispositionCodes.Count);
			AssertEquals("97", declarationReloaded.DispositionCodes[0].US_Code);

			AssertEquals("PGA disposition codes processed", 2, declarationReloaded.OGADispositionCodes.Count);
			var disposition1 = declarationReloaded.OGADispositionCodes[0];
			AssertEquals(@"Should always be empty, because FDA message status now depends on OtherAgencyQuotaIdentifier, which is absent in SO70 record.
The same disposition code (for example 01) shared between FWS and FDA, unable to determine what to set", ZString.Empty, declarationReloaded.FDAStatus);
			AssertEquals("no longer set OGA FDA message status", "", declarationReloaded.FDAMsgStatus);

			AssertEquals("04", disposition1.US_Code);
			AssertEquals(new ZDateTime(2013, 12, 30, 14, 39, 00), disposition1.US_DispositionDate);
			AssertEquals("222", disposition1.US_OGADispositionBeginningCBPLine);
			AssertEquals(OGADispositionSourceList.Codes.PGA, disposition1.US_Source);

			AssertEquals(2, disposition1.OGADispositionDetails.Count);
			AssertEquals("100", disposition1.OGADispositionDetails[0].US_LineSubReasonCode1);
			AssertEquals("144", disposition1.OGADispositionDetails[0].US_LineSubReasonCode2);
			AssertEquals("1", disposition1.OGADispositionDetails[0].US_ReferenceIDQualifier);
			AssertEquals("1234567890", disposition1.OGADispositionDetails[0].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), disposition1.OGADispositionDetails[0].US_ReceiptDateTime);

			AssertEquals("122", disposition1.OGADispositionDetails[1].US_LineSubReasonCode1);
			AssertEquals("126", disposition1.OGADispositionDetails[1].US_LineSubReasonCode2);
			AssertEquals("2", disposition1.OGADispositionDetails[1].US_ReferenceIDQualifier);
			AssertEquals("2222222222", disposition1.OGADispositionDetails[1].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition1.OGADispositionDetails[1].US_ReceiptDateTime);

			AssertEquals("COMMENT1. COMMENT2.", disposition1.US_Comment);
			var disposition2 = declarationReloaded.OGADispositionCodes[1];

			AssertEquals(new ZDateTime(2013, 10, 30, 14, 39, 00), disposition2.US_DispositionDate);
			AssertEquals("01", disposition2.US_Code);

			AssertEquals(2, disposition2.OGADispositionDetails.Count);
			AssertEquals("102", disposition2.OGADispositionDetails[0].US_LineSubReasonCode1);
			AssertEquals("103", disposition2.OGADispositionDetails[0].US_LineSubReasonCode2);
			AssertEquals("1", disposition2.OGADispositionDetails[0].US_ReferenceIDQualifier);
			AssertEquals("3333333333", disposition2.OGADispositionDetails[0].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), disposition2.OGADispositionDetails[0].US_ReceiptDateTime);

			AssertEquals("110", disposition2.OGADispositionDetails[1].US_LineSubReasonCode1);
			AssertEquals("111", disposition2.OGADispositionDetails[1].US_LineSubReasonCode2);
			AssertEquals("2", disposition2.OGADispositionDetails[1].US_ReferenceIDQualifier);
			AssertEquals("4444444444", disposition2.OGADispositionDetails[1].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition2.OGADispositionDetails[1].US_ReceiptDateTime);

			AssertEquals("COMMENT3. COMMENT4.", disposition2.US_Comment);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertContains("COMMENT1. COMMENT2.", email.Body);
			AssertContains("COMMENT3. COMMENT4.", email.Body);
			AssertContains("DETAINED, REFUSAL SATISFIED, INVALID EXEMPTION (FME) QUALIFIER, INVALID FILER", email.Body);
			AssertContains("PARTIAL RELEASE AND REFUSE, MISMATCH IN REGISTRATION, CANCELLED MANUFACTURER FACILITY REGISTRATION", email.Body);
		}

		public void TestPGADispositions_Version01()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "01", "DATA UNDER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "02", "HOLD INTACT", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "04", "DATA REJECTED PER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "11", "INTENSIVE – EXAM/SAMPLE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "13", "EXAM- RESOLVED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "100", "DETAINED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "102", "REFUSED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "103", "PARTIAL RELEASE AND REFUSE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "110", "MISMATCH IN REGISTRATION", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "111", "CANCELLED MANUFACTURER FACILITY REGISTRATION", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "122", "INVALID EXEMPTION (FME) QUALIFIER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "126", "INVALID FILER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "144", "REFUSAL SATISFIED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "DIS Form List");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "AMS01", "AMS_FOREIGN_GOVT_EXPORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.USDISFormGroup, "NOGROUP");
			Factory.Save();

			var declaration = GetDeclaration("71002057");
			declaration.JE_MasterBill = "00591325206";

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131Y       " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60093013012597ADMISSIBLE                                                      " +
					"SO70ACE000123013143913EPA DATA REVIEW             01041122223654THRU933301   H01" +
					"SO711 1234567890  0616150910  100144                                            " +
					"SO712 2222222222  0316150910  122126                                            " +
					"SO72COMMENT1.                                                                   " +
					"SO72COMMENT2.                                                                   " +
					"SO70ACE000103013143911EPA DATA REVIEW             020112333B777THRU55566AMS01001" +
					"SO711 3333333333  0616150910  102103                                            " +
					"SO712 4444444444  0316150910  110111                                            " +
					"SO72COMMENT3.                                                                   " +
					"SO72COMMENT4.                                                                   " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Declaration disposition codes processed", 1, declarationReloaded.DispositionCodes.Count);
			AssertEquals("97", declarationReloaded.DispositionCodes[0].US_Code);

			AssertEquals("PGA disposition codes processed", 2, declarationReloaded.OGADispositionCodes.Count);
			var disposition1 = declarationReloaded.OGADispositionCodes[0];
			AssertEquals(@"Should always be empty, because FDA message status now depends on OtherAgencyQuotaIdentifier, which is absent in SO70 record.
The same disposition code (for example 01) shared between FWS and FDA, unable to determine what to set", ZString.Empty, declarationReloaded.FDAStatus);
			AssertEquals("no longer set OGA FDA message status", "", declarationReloaded.FDAMsgStatus);

			AssertEquals("04", disposition1.US_Code);
			AssertEquals(new ZDateTime(2013, 12, 30, 14, 39, 00), disposition1.US_DispositionDate);
			AssertEquals("2222", disposition1.US_OGADispositionBeginningCBPLine);
			AssertEquals(OGADispositionSourceList.Codes.PGA, disposition1.US_Source);
			AssertEquals("01", disposition1.US_DocumentType);
			AssertEquals("Packing List", disposition1.DocumentTypeDesc);

			AssertEquals(2, disposition1.OGADispositionDetails.Count);
			AssertEquals("100", disposition1.OGADispositionDetails[0].US_LineSubReasonCode1);
			AssertEquals("144", disposition1.OGADispositionDetails[0].US_LineSubReasonCode2);
			AssertEquals("1", disposition1.OGADispositionDetails[0].US_ReferenceIDQualifier);
			AssertEquals("1234567890", disposition1.OGADispositionDetails[0].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), disposition1.OGADispositionDetails[0].US_ReceiptDateTime);

			AssertEquals("122", disposition1.OGADispositionDetails[1].US_LineSubReasonCode1);
			AssertEquals("126", disposition1.OGADispositionDetails[1].US_LineSubReasonCode2);
			AssertEquals("2", disposition1.OGADispositionDetails[1].US_ReferenceIDQualifier);
			AssertEquals("2222222222", disposition1.OGADispositionDetails[1].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition1.OGADispositionDetails[1].US_ReceiptDateTime);

			AssertEquals("COMMENT1. COMMENT2.", disposition1.US_Comment);
			var disposition2 = declarationReloaded.OGADispositionCodes[1];

			AssertEquals(new ZDateTime(2013, 10, 30, 14, 39, 00), disposition2.US_DispositionDate);
			AssertEquals("01", disposition2.US_Code);
			AssertEquals("AMS01", disposition2.US_DocumentType);
			AssertEquals("AMS_FOREIGN_GOVT_EXPORT", disposition2.DocumentTypeDesc);

			AssertEquals(2, disposition2.OGADispositionDetails.Count);
			AssertEquals("102", disposition2.OGADispositionDetails[0].US_LineSubReasonCode1);
			AssertEquals("103", disposition2.OGADispositionDetails[0].US_LineSubReasonCode2);
			AssertEquals("1", disposition2.OGADispositionDetails[0].US_ReferenceIDQualifier);
			AssertEquals("3333333333", disposition2.OGADispositionDetails[0].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), disposition2.OGADispositionDetails[0].US_ReceiptDateTime);

			AssertEquals("110", disposition2.OGADispositionDetails[1].US_LineSubReasonCode1);
			AssertEquals("111", disposition2.OGADispositionDetails[1].US_LineSubReasonCode2);
			AssertEquals("2", disposition2.OGADispositionDetails[1].US_ReferenceIDQualifier);
			AssertEquals("4444444444", disposition2.OGADispositionDetails[1].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition2.OGADispositionDetails[1].US_ReceiptDateTime);

			AssertEquals("COMMENT3. COMMENT4.", disposition2.US_Comment);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertContains(@"<tr><td>PGA Correction Response</td><td>Y</td></tr>", email.Body);
			AssertContains(@"<tr class=""tableheadings""><th>Agency/Quota Indicator</th><th>Disposition Date</th><th>PGA Entry Status Desc</th><th>PGA Line Status Desc</th><th>Review Reason Status Desc</th><th>CBP Line Status</th><th>Beg. CBP Line</th><th>Beg. Tariff</th><th>Beg. PGA Line</th><th>End PGA Line</th><th>End Tariff</th><th>End CBP Line</th><th>Document Type</th><th>PGA Entry Hold Type</th><th>Comment</th><th>Reasons</th></tr></thead><tr><td>ACE</td><td>30-Dec-13 14:39</td><td>13 EXAM- RESOLVED</td><td>04 DATA REJECTED PER PGA REVIEW</td><td>&nbsp;</td><td>01 DATA UNDER PGA REVIEW</td><td>2222</td><td>3</td><td>654</td><td>333</td><td>9</td><td>THRU</td><td>01 - Packing List</td><td>H - </td><td>COMMENT1. COMMENT2.</td><td>DETAINED, REFUSAL SATISFIED, INVALID EXEMPTION (FME) QUALIFIER, INVALID FILER</td></tr><tr><td>ACE</td><td>30-Oct-13 14:39</td><td>11 INTENSIVE – EXAM/SAMPLE</td><td>01 DATA UNDER PGA REVIEW</td><td>&nbsp;</td><td>02 HOLD INTACT</td><td>333B</td><td>7</td><td>77T</td><td>566</td><td>5</td><td>HRU5</td><td>AMS01 - AMS_FOREIGN_GOVT_EXPORT</td><td>0 - </td><td>COMMENT3. COMMENT4.</td><td>REFUSED, PARTIAL RELEASE AND REFUSE, MISMATCH IN REGISTRATION, CANCELLED MANUFACTURER FACILITY REGISTRATION</td></tr>", email.Body);
		}

		public void TestPGADispositions_Version02()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "01", "DATA UNDER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "02", "HOLD INTACT", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "08", "MOVE TO SECURE HLDNG FCLTY", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "11", "INTENSIVE – EXAM/SAMPLE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "100", "DETAINED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "102", "REFUSED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "103", "PARTIAL RELEASE AND REFUSE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "110", "MISMATCH IN REGISTRATION", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "111", "CANCELLED MANUFACTURER FACILITY REGISTRATION", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "122", "INVALID EXEMPTION (FME) QUALIFIER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "126", "INVALID FILER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "144", "REFUSAL SATISFIED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "DIS Form List");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "AMS01", "AMS_FOREIGN_GOVT_EXPORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.USDISFormGroup, "NOGROUP");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason, "52", "PN REFUSED INACCURATE PN", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason, "53", "PN REFUSED UNTIMELY PN", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));

			Factory.Save();

			var declaration = GetDeclaration("71002057");
			declaration.JE_MasterBill = "00591325206";

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131Y       " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60093013012597ADMISSIBLE                                                      " +
					"SO70FDAFOO070516162201DATA UNDER PGA REVIEW         085200011001              02" +
					"SO711 1234567890  0616150910  100144                                            " +
					"SO712 2222222222  0316150910  122126                                            " +
					"SO72COMMENT1.                                                                   " +
					"SO72COMMENT2.                                                                   " +
					"SO70ACE000103013143911EPA DATA REVIEW             020853333B777THRU55566AMS01002" +
					"SO711 3333333333  0616150910  102103                                            " +
					"SO712 4444444444  0316150910  110111                                            " +
					"SO72COMMENT3.                                                                   " +
					"SO72COMMENT4.                                                                   " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Declaration disposition codes processed", 1, declarationReloaded.DispositionCodes.Count);
			AssertEquals("97", declarationReloaded.DispositionCodes[0].US_Code);

			AssertEquals("PGA disposition codes processed", 2, declarationReloaded.OGADispositionCodes.Count);
			var disposition1 = declarationReloaded.OGADispositionCodes[0];
			AssertEquals(@"Should always be empty, because FDA message status now depends on OtherAgencyQuotaIdentifier, which is absent in SO70 record.
The same disposition code (for example 01) shared between FWS and FDA, unable to determine what to set", ZString.Empty, declarationReloaded.FDAStatus);
			AssertEquals("no longer set OGA FDA message status", "", declarationReloaded.FDAMsgStatus);

			AssertEquals("08", disposition1.US_Code);
			AssertEquals(new ZDateTime(2016, 07, 5, 16, 22, 00), disposition1.US_DispositionDate);
			AssertEquals("0001", disposition1.US_OGADispositionBeginningCBPLine);
			AssertEquals(OGADispositionSourceList.Codes.PGA, disposition1.US_Source);
			AssertEquals("", disposition1.US_DocumentType);
			AssertEquals("", disposition1.DocumentTypeDesc);

			AssertEquals(2, disposition1.OGADispositionDetails.Count);
			AssertEquals("100", disposition1.OGADispositionDetails[0].US_LineSubReasonCode1);
			AssertEquals("144", disposition1.OGADispositionDetails[0].US_LineSubReasonCode2);
			AssertEquals("1", disposition1.OGADispositionDetails[0].US_ReferenceIDQualifier);
			AssertEquals("1234567890", disposition1.OGADispositionDetails[0].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), disposition1.OGADispositionDetails[0].US_ReceiptDateTime);

			AssertEquals("122", disposition1.OGADispositionDetails[1].US_LineSubReasonCode1);
			AssertEquals("126", disposition1.OGADispositionDetails[1].US_LineSubReasonCode2);
			AssertEquals("2", disposition1.OGADispositionDetails[1].US_ReferenceIDQualifier);
			AssertEquals("2222222222", disposition1.OGADispositionDetails[1].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition1.OGADispositionDetails[1].US_ReceiptDateTime);
			AssertEquals("COMMENT1. COMMENT2.", disposition1.US_Comment);
			AssertEquals("52", disposition1.US_ReviewReasonCode);
			AssertEquals("PN REFUSED INACCURATE PN", disposition1.ReviewReasonCodeDesc);

			var disposition2 = declarationReloaded.OGADispositionCodes[1];
			AssertEquals(new ZDateTime(2013, 10, 30, 14, 39, 00), disposition2.US_DispositionDate);
			AssertEquals("08", disposition2.US_Code);
			AssertEquals("AMS01", disposition2.US_DocumentType);
			AssertEquals("AMS_FOREIGN_GOVT_EXPORT", disposition2.DocumentTypeDesc);

			AssertEquals(2, disposition2.OGADispositionDetails.Count);
			AssertEquals("102", disposition2.OGADispositionDetails[0].US_LineSubReasonCode1);
			AssertEquals("103", disposition2.OGADispositionDetails[0].US_LineSubReasonCode2);
			AssertEquals("1", disposition2.OGADispositionDetails[0].US_ReferenceIDQualifier);
			AssertEquals("3333333333", disposition2.OGADispositionDetails[0].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), disposition2.OGADispositionDetails[0].US_ReceiptDateTime);

			AssertEquals("110", disposition2.OGADispositionDetails[1].US_LineSubReasonCode1);
			AssertEquals("111", disposition2.OGADispositionDetails[1].US_LineSubReasonCode2);
			AssertEquals("2", disposition2.OGADispositionDetails[1].US_ReferenceIDQualifier);
			AssertEquals("4444444444", disposition2.OGADispositionDetails[1].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition2.OGADispositionDetails[1].US_ReceiptDateTime);
			AssertEquals("COMMENT3. COMMENT4.", disposition2.US_Comment);
			AssertEquals("53", disposition2.US_ReviewReasonCode);
			AssertEquals("PN REFUSED UNTIMELY PN", disposition2.ReviewReasonCodeDesc);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertContains(@"<tr><td>PGA Correction Response</td><td>Y</td></tr>", email.Body);
			AssertContains(@"<th>Agency/Quota Indicator</th><th>Disposition Date</th><th>PGA Entry Status Desc</th><th>PGA Line Status Desc</th><th>Review Reason Status Desc</th><th>CBP Line Status</th><th>Beg. CBP Line</th><th>Beg. Tariff</th><th>Beg. PGA Line</th><th>End PGA Line</th><th>End Tariff</th><th>End CBP Line</th><th>Document Type</th><th>PGA Entry Hold Type</th><th>Comment</th><th>Reasons</th></tr></thead><tr><td>FDA</td><td>05-Jul-16 16:22</td><td>01 DATA UNDER PGA REVIEW</td><td>08 MOVE TO SECURE HLDNG FCLTY</td><td>PN REFUSED INACCURATE PN</td><td>&nbsp;</td><td>0001</td><td>1</td><td>001</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>COMMENT1. COMMENT2.</td><td>DETAINED, REFUSAL SATISFIED, INVALID EXEMPTION (FME) QUALIFIER, INVALID FILER</td></tr><tr><td>ACE</td><td>30-Oct-13 14:39</td><td>11 INTENSIVE – EXAM/SAMPLE</td><td>08 MOVE TO SECURE HLDNG FCLTY</td><td>PN REFUSED UNTIMELY PN</td><td>02 HOLD INTACT</td><td>333B</td><td>7</td><td>77T</td><td>566</td><td>5</td><td>HRU5</td><td>AMS01 - AMS_FOREIGN_GOVT_EXPORT</td><td>0 - </td><td>COMMENT3. COMMENT4.</td><td>REFUSED, PARTIAL RELEASE AND REFUSE, MISMATCH IN REGISTRATION, CANCELLED MANUFACTURER FACILITY REGISTRATION</td></tr>", email.Body);
		}

		public void TestReleaseStatusAfterCancel()
		{
			var declaration = GetDeclaration("71002057");

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60093013012523ENTRY CANCELLED                                                 " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			declaration.DispositionCodes.Load();
			AssertEquals("Declaration disposition codes processed", 1, declaration.DispositionCodes.Count);
			declaration.Logs.GetAllLogs().Load();
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus + " - CAN"));

			AssertEquals("23", declaration.DispositionCodes[0].US_Code);
			AssertEquals(CRLReleaseStatusList.Codes.CAN, declaration.ReleaseStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertNotNull(email);
		}

		public void TestEntryNotPermitted()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);

			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO60RecordDispCode, "SO60RecordDispCode", dataGrouping.ZZZ_DataGrouping);
			var code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "79", "ENTRY NOT PERMITTED AT REPORTED PORT", startDate, endDate);
			Factory.Save();

			var declaration = GetDeclaration("71002057");

			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60093013012579ENTRY NOT PERMITTED AT REPORTED PORT                            " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			declaration.DispositionCodes.Load();
			AssertEquals("Declaration disposition codes processed", 1, declaration.DispositionCodes.Count);
			declaration.Logs.GetAllLogs().Load();
			AssertEquals("SO - NRL", declaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange).SL_Reference);

			AssertEquals("79", declaration.DispositionCodes[0].US_Code);
			AssertEquals("ENTRY NOT PERMITTED AT REPORTED PORT", declaration.DispositionCodes[0].DispositionCodeDesc);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertNotNull(email);
			Assert(email.Body.Contains("<td>Disposition Action/Date</td><td>79 ENTRY NOT PERMITTED AT REPORTED PORT/30-Sep-13 01:25</td>"));
		}

		public void TestCertifyFromEntrySummary()
		{
			var declaration = GetDeclaration("71002057");

			declaration.JE_MasterBill = "FSDFS324234";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "71005415";

			Factory.Save();

			var message = CreateStatusMessage("B001101SV9SO                                                                    " +
				"SO101101SV9  71005415 0123-456789012                                   1        " +
				"SO40     APLUFSDFS324234                                   00000015CS   00000000" +
				"SO50040814004691NO BILL MATCH                                                   " +
				"Y  1101SV9SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();

			var cusEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var result = cusEntry.Messages.Find(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			AssertNotNull(result);

			AssertEquals("When certify Cargo Release from entry summary, status notification message also returned. Cargo Release entry may not exists.", cusEntry.PK, message.EM_LinkUniqueID);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertNotNull(email);
		}

		public void TestPrintSplitDetails()
		{
			var declaration = GetDeclaration("71002057");

			declaration.JE_MasterBill = "FSDFS324234";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "71005415";

			Factory.Save();

			var message = CreateStatusMessage(
				"B001101SV9SO                                                                    " +
				"SO101101SV9  71005415 0123-456789012                                   1        " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000425     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1025141512    " +
				"SO40M    16070630070                                                            " +
				"SO40H    BDH40062434                                       00000261     00000686" +
				"SO42257395585   6117041512110314                                                " +
				"SO4216070630070 6117041512110314                                                " +
				"SO50110314151195BILL ARRIVED                            YCPA 90   1026141512    " +
				"SO60093013012597ADMISSIBLE                                                      " +
				"SO60093013014298RELEASED                                09301301                " +
				"SO70ACE000123013143912EPA DATA REVIEW                         2  0101000001     " +
				"SO70ACE000123013143901PGA DATA ACCEPTED                       2  0101000001     " +
				"Y  1101SV9SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertNotNull(email);
			Assert(email.Body.Contains("<td>97 ADMISSIBLE/30-Sep-13 01:25</td></tr><tr><td>Disposition Action/Date</td><td>98 RELEASED/30-Sep-13 01:42</td>"));
			Assert(email.Body.Contains("<td>261 </td></tr>"));
		}

		public void TestCS00374252()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "235";
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.ADM;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "15542365";
			Factory.Save();

			var message446179 = CreateStatusMessage("B  2720235SO                                                                    SO102704235  15542365 0135-219431300OOLUOOCL LONG BEACH     91E  090815         SO20CR S00175196                                                                SO40MOOLU2563624231                                                             SO40HJQSHATWLAX6165                                        00000057     00000057SO50091015194995BILL ARRIVED                                                    SO60091015194998RELEASED                                09081501                Y  2720235SO");
			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_From = "USC";
			interchange.EI_To = "ABC";
			interchange.EI_InterchangeNum = "446179";
			message446179.EM_EI = interchange.PK;

			var message446180 = CreateStatusMessage("B  2720235SO                                                                    SO102704235  15542365 0135-219431300OOLUOOCL LONG BEACH     91E  090815         SO20CR S00175196                                                                SO40MOOLU2563624231                                                             SO40HJQSHATWLAX6165                                        00000057     00000057SO50091015194995BILL ARRIVED                                                    SO60091015194981IN-BOND PORT DISCREPANCY                                        SO60091015194999RELEASE SUSPENDED                                               SO60091015194997ADMISSIBLE                                                      Y  2720235SO");
			var interchange2 = Factory.New<CBPEDIInterchange>();
			interchange2.EI_From = "USC";
			interchange2.EI_To = "ABC";
			interchange2.EI_InterchangeNum = "446180";
			message446180.EM_EI = interchange2.PK;

			var message446181 = CreateStatusMessage("B  2720235SO                                                                    SO102704235  15542365 0135-219431300OOLUOOCL LONG BEACH     91E  090815         SO20CR S00175196                                                                SO40MOOLU2563624231                                                             SO40HJQSHATWLAX6165                                        00000057     00000057SO50091015194995BILL ARRIVED                                                    SO60091015194998RELEASED                                09081501                Y  2720235SO");
			var interchange3 = Factory.New<CBPEDIInterchange>();
			interchange3.EI_From = "USC";
			interchange3.EI_To = "ABC";
			interchange3.EI_InterchangeNum = "446181";
			message446181.EM_EI = interchange3.PK;

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(CRLReleaseStatusList.Codes.REL, decLoaded.ReleaseStatus);
		}

		public void TestPNConfirmationNumberSetToFDALine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "235";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8471704065";
			invoiceLine1.JI_LinePrice = 15000m;
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.US_UC_NKCountryOfOrigin = "AU";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8471704022";
			invoiceLine2.JI_LinePrice = 300m;
			invoiceLine2.JI_CustomsQuantity = 1m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";

			var fda1_line2 = invoiceLine2.ACE_FDALines.AddNew();
			fda1_line2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda1_line2.US_LineNo = 1;

			var fda2_line2 = invoiceLine2.ACE_FDALines.AddNew();
			fda2_line2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda2_line2.US_BrandName = "Food";
			fda2_line2.US_LineNo = 2;

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3771704022";
			invoiceLine3.JI_LinePrice = 1200m;
			invoiceLine3.JI_CustomsQuantity = 100m;
			invoiceLine3.US_UC_NKCountryOfOrigin = "IT";

			var fda_line3 = invoiceLine3.ACE_FDALines.AddNew();
			fda_line3.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda_line3.US_LineNo = 1;

			var invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "2671704022";
			invoiceLine4.JI_LinePrice = 200m;
			invoiceLine4.JI_CustomsQuantity = 105m;
			invoiceLine4.US_UC_NKCountryOfOrigin = "FR";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "50689378";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "50689378";

			fda1_line2.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

			Factory.Save();

			var message = CreateStatusMessage(
				"B001303235SO                                                                    " +
				"SO101303235  50689378 0120-086695300MAEUMSC FLAMINIA        540W 102715         " +
				"SO20CR S00182399                                                                " +
				"SO40MMAEU954607736                                                              " +
				"SO40HMAEU567902246                                         00000010     00000010" +
				"SO50102615084194BILL DEPARTED                                                   " +
				"SO70FDA   102615084101DATA UNDER PGA REVIEW             002001                  " +
				"SO7101156400703561102615084131                                                  " +
				"SO70FDA   102615084101DATA UNDER PGA REVIEW             003001                  " +
				"SO7101156400703561102615084135                                                  " +
				"Y  1303235SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			fda1_line2.Reload();
			AssertEquals("Prior Notice Confirmation Number set", "156400703561", fda1_line2.US_PNC);
			AssertEquals("Suspend Tracking Status Change when Prior Notice Confirmation Number changed", PGATrackingStatusList.Codes.Added, fda1_line2.US_TrackingStatus);

			fda2_line2.Reload();
			AssertEquals("", fda2_line2.US_PNC);

			fda_line3.Reload();
			AssertEquals("Prior Notice Confirmation Number set", "156400703561", fda_line3.US_PNC);

			declaration.Reload();
			AssertEquals("01 - Prior Notice Confirmation Number", declaration.OGADispositionCodes[0].OGADispositionDetails[0].ReferenceQualifierCodeDesc);
		}

		public void TestSO20CMTACECargoReleaseStatusResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "906";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8471704065";
			invoiceLine1.JI_LinePrice = 15000m;
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.US_UC_NKCountryOfOrigin = "AU";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "00941598";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "00941598";

			Factory.Save();

			var message = CreateStatusMessage(
				"B003002906SO                                                                    " +
				"SO103002906  00941598 0113-611944100KKLUYM MATURITY         41E  040217         " +
				"SO20CMTTRANSFER FOR EXAM TO CES MERCER  PLEASE UPLOAD ENT                       " +
				"SO20CMTRY DOCS TO DIS                                                           " +
				"SO20RSN05                                                                       " +
				"SO20RRNRRN123                                                                   " +
				"SO40RKKLUNB3706038                                         00001960     00001960" +
				"SO50040317105895BILL ARRIVED                                                    " +
				"SO60040317105822RELEASE DATE UPDATE                     04031701                " +
				"SO60040317105898RELEASED                                04031701                " +
				"SO60040317105801ONE USG                                                         " +
				"Y  3002906SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();

			var cusEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			cusEntry.Messages.Add(message);
			var result = cusEntry.Messages.Find(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("ACE Cargo Release Status Response"));
			AssertNotNull(email);
			Assert("Comments From CBP", email.Body.Contains("<tr style=\"color:red\"><td style=\"color:red\">Comments</td><td style=\"color:red\">"));
			Assert("RSN", email.Body.Contains($"<tr><td>{ReferenceIdentifierQualifierCodeList.Descriptions.RSN}</td><td>05 Additional information required via DIS</td></tr>"));
			Assert("RRN", email.Body.Contains($"<tr><td>{ReferenceIdentifierQualifierCodeList.Descriptions.RRN}</td><td>RRN123</td></tr>"));

			declaration.Reload();
			var errRecords = declaration.EntryStatusesAndErrors.ACECargoRelReferenceData;

			ErrorsRecord oneCMT = errRecords.OfType<ErrorsRecord>().First(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.CMT);
			AssertNotNull(oneCMT);
			AssertEquals(EM_ActionStatusList.Codes.Incomplete, oneCMT.ReferenceOnAction);
			AssertEquals(ReferenceIdentifierQualifierCodeList.Codes.CMT, oneCMT.ErrorMessageIdentifier);
			AssertEquals("TRANSFER FOR EXAM TO CES MERCER  PLEASE UPLOAD ENTRY DOCS TO DIS", oneCMT.NarrativeMessage);

			ErrorsRecord oneRSN = errRecords.OfType<ErrorsRecord>().First(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.RSN);
			AssertNotNull(oneRSN);
			AssertEquals(ReferenceIdentifierQualifierCodeList.Codes.RSN, oneRSN.ErrorMessageIdentifier);
			AssertEquals("05 Additional information required via DIS", oneRSN.NarrativeMessage);

			ErrorsRecord oneRRN = errRecords.OfType<ErrorsRecord>().First(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.RRN);
			AssertNotNull(oneRRN);
			AssertEquals(ReferenceIdentifierQualifierCodeList.Codes.RRN, oneRRN.ErrorMessageIdentifier);
			AssertEquals("RRN123", oneRRN.NarrativeMessage);
		}

		public void TestSO71ACECargoReleaseStatusResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "235";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8471704065";
			invoiceLine1.JI_LinePrice = 15000m;
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.US_UC_NKCountryOfOrigin = "AU";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8471704022";
			invoiceLine2.JI_LinePrice = 300m;
			invoiceLine2.JI_CustomsQuantity = 1m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";

			var fda1_line2 = invoiceLine2.ACE_FDALines.AddNew();
			fda1_line2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda1_line2.US_LineNo = 1;

			var fda2_line2 = invoiceLine2.ACE_FDALines.AddNew();
			fda2_line2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda2_line2.US_BrandName = "Food";
			fda2_line2.US_LineNo = 2;

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3771704022";
			invoiceLine3.JI_LinePrice = 1200m;
			invoiceLine3.JI_CustomsQuantity = 100m;
			invoiceLine3.US_UC_NKCountryOfOrigin = "IT";

			var fda_line3 = invoiceLine3.ACE_FDALines.AddNew();
			fda_line3.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda_line3.US_LineNo = 1;

			var invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "2671704022";
			invoiceLine4.JI_LinePrice = 200m;
			invoiceLine4.JI_CustomsQuantity = 105m;
			invoiceLine4.US_UC_NKCountryOfOrigin = "FR";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "50689378";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "50689378";

			Factory.Save();

			var message = CreateStatusMessage(
				"B001303235SO                                                                    " +
				"SO101303235  50689378 0120-086695300MAEUMSC FLAMINIA        540W 102715         " +
				"SO20CR S00182399                                                                " +
				"SO40MMAEU954607736                                                              " +
				"SO40HMAEU567902246                                         00000010     00000010" +
				"SO50102615084194BILL DEPARTED                                                   " +
				"SO70FDA   102615084101DATA UNDER PGA REVIEW             002001                  " +
				"SO7101156400703561102615084131                                                  " +
				"SO70FDA   102615084101DATA UNDER PGA REVIEW             003001                  " +
				"SO7101156400703561102615084135                                                  " +
				"Y  1303235SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			fda1_line2.Reload();
			AssertEquals("Prior Notice Confirmation Number set", "156400703561", fda1_line2.US_PNC);

			fda2_line2.Reload();
			AssertEquals("", fda2_line2.US_PNC);

			fda_line3.Reload();
			AssertEquals("Prior Notice Confirmation Number set", "156400703561", fda_line3.US_PNC);

			declaration.Reload();
			AssertEquals("01 - Prior Notice Confirmation Number", declaration.OGADispositionCodes[0].OGADispositionDetails[0].ReferenceQualifierCodeDesc);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("ACE Cargo Release Status Response"));
			AssertNotNull(email);
			Assert(email.Body.Contains(ACEABIProcessor.PNCNumbersReceived));

			var message2 = CreateStatusMessage(
			"B001303235SO                                                                    " +
			"SO101303235  50689378 0120-086695300MAEUMSC FLAMINIA        540W 102715         " +
			"SO20CR S00182399                                                                " +
			"SO40MMAEU954607736                                                              " +
			"SO40HMAEU567902246                                         00000010     00000010" +
			"SO50102615084194BILL DEPARTED                                                   " +
			"SO70FDA   102615084101DATA UNDER PGA REVIEW             002001                  " +
			"SO7102156400703562102615084131                                                  " +
			"SO70FDA   102715084101DATA UNDER PGA REVIEW             003001                  " +
			"SO7102156400222222102715084135                                                  " +
			"SO70FDA   102815084101DATA UNDER PGA REVIEW             004001                  " +
			"Y  1303235SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			declaration.Reload();
			AssertEquals("01 - Prior Notice Confirmation Number", declaration.OGADispositionCodes[0].OGADispositionDetails[0].ReferenceQualifierCodeDesc);

			var email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("ACE Cargo Release Status Response"));
			AssertNotNull(email2);
			Assert("No PNC Number", !email2.Body.Contains(ACEABIProcessor.PNCNumbersReceived));
		}

		public void TestFailed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71016297";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var soMessage = CreateStatusMessage("B001101SV9SO                                                                    SO101101SV9  71016297 0158-123456789APLUTITANIC                  112715         SO40RAPLUMST112715                                         00000100     00000000SO50113015101391NO BILL MATCH                                                   SO70FDAFOO112715110801DATA UNDER PGA REVIEW       0101  001001                  SO70FDAFOO112715110801DATA UNDER PGA REVIEW       0101  002001                  SO70FDAFOO112715111204DATA REJECTED PER PGA REVIEW040414002001                  SO71                          109                                               Y  1101SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			soMessage.Reload();
			AssertEquals("RCV", soMessage.EM_Status);
		}

		public void TestFDATrackingStatusResetAfterSOMessageIsRejected()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71016297";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;

			var aceFDALine0 = invoiceLine.ACE_FDALines.AddNew();
			aceFDALine0.US_LineNo = 1;
			aceFDALine0.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			var aceFDALine1 = invoiceLine.ACE_FDALines.AddNew();
			aceFDALine1.US_LineNo = 2;
			aceFDALine1.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CreateStatusMessage(
				"B001303SV9SO                                                                    " +
				"SO101303SV9  71016297 0120-086695300MAEUMSC FLAMINIA        540W 102715         " +
				"SO20CR B00000001                                                                " +
				"SO40MMAEU954607736                                                              " +
				"SO40HMAEU567902246                                         00000010     00000010" +
				"SO50102615084194BILL DEPARTED                                                   " +
				"SO70FDACOS081916144301DATA UNDER PGA REVIEW         041400011001              02" +
				"SO71                          107                                               " +
				"SO70FDACOS081916144301DATA UNDER PGA REVIEW         041400011002              02" +
				"SO71                          107                                               " +
				"Y  1303SV9SO00000");

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
			aceFDALine0.Reload();
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, aceFDALine0.US_TrackingStatus);
			aceFDALine1.Reload();
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, aceFDALine1.US_TrackingStatus);
		}

		public void TestUpdateReplaceRequestEntryStatus()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = GetDeclaration("71002057");
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = "TRK";
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.US_EntryDate = ZDateTime.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ReplaceRequestPending;

			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO60012016120584DOC REQUIRED FOR CORRECTION REQUEST             CBP03           
SO60012016120586CORRECTION REQUEST REJECTED                                     
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			entry.Reload();
			declaration.Reload();

			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseReplace, entry.CH_Status);
			AssertEquals(CRLReleaseStatusList.Codes.REL, declaration.ReleaseStatus);

			entry.CH_Status = ImportMessageStatusList.Codes.ReplaceRequestPending;
			declaration.ReleaseStatus = ZString.Empty;

			message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO60012016120523ENTRY CANCELLED                                 CBP03           
SO60012016120586CORRECTION REQUEST REJECTED                                     
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			entry.Reload();
			declaration.Reload();

			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseDelete, entry.CH_Status);
			AssertEquals(CRLReleaseStatusList.Codes.CAN, declaration.ReleaseStatus);

			entry.CH_Status = ImportMessageStatusList.Codes.ReplaceRequestPending;

			message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016121596DOCUMENT REQUIRED                               03              
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			entry.Reload();
			declaration.Reload();

			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseReplace, entry.CH_Status);
		}

		public void TestUpdateReleaseStatusPerPreviousDispositionCodesGroup()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = GetDeclaration("71002057");
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = "TRK";
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.US_EntryDate = ZDateTime.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ReplaceRequestPending;

			var message1 = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

			AssertEquals(CRLReleaseStatusList.Codes.REL, decLoaded.ReleaseStatus);

			entry.CH_Status = ImportMessageStatusList.Codes.ReplaceRequestPending;
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.DOC;

			message1 = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120684DOC REQUIRED FOR CORRECTION REQUEST             CBP03           
SO60012016120686CORRECTION REQUEST REJECTED                                     
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var entLoaded = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

			AssertEquals(ImportMessageStatusList.Codes.ReplaceRequestRejected, entLoaded.CH_Status);
			AssertEquals(CRLReleaseStatusList.Codes.REL, decLoaded.ReleaseStatus);
		}

		[TestDate(2022, 01, 21)]
		public void TestSOMessageAttachedToCorrectJobWhenSameEntryNumberAllocatedToDifferentJobs()
		{
			var declaration1 = GetDeclaration("71002057");
			declaration1.JE_DeclarationReference = "BXX00000010";
			var declaration2 = GetDeclaration("71002057");
			declaration2.JE_DeclarationReference = "BYY00000020";

			CreateStatusMessage("B004601267SO                                                                    " +
"SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         " +
"SO20CR Y00000020                                                                " +
"SO40RC2  PI3151202078                                      00004956BO   00004956" +
"SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    " +
"SO40RC2  PI3151202079                                      00004957BO   00004957" +
"SO50120121073851MANIFEST HOLD CBP                       YQTR 727  1201161108    " +
"SO60120121120598RELEASED                                01202101                " +
"Y  4601267SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration1 = newFactory.Load<JobDeclaration>(declaration1.PK);
			AssertEquals("Release Status for first declaration should NOT be changed", ZString.Empty, loadedDeclaration1.ReleaseStatus);
			var loadedDeclaration2 = newFactory.Load<JobDeclaration>(declaration2.PK);
			AssertEquals("Release Status for second declaration should be updated to REL", CRLReleaseStatusList.Codes.REL, loadedDeclaration2.ReleaseStatus);
		}

		public void TestReleaseStatusWithOneUSGInSO()
		{
			var declaration = GetDeclaration("73000414");

			var message = CreateStatusMessage(
					"B001101SV9SO                                                                    " +
					"SO100708SV9  73000414 01030712-00085NISD864X2TR864X2        19030030919         " +
					"SO20CR BCHB00174297                                                             " +
					"SO40RNISD00147479                                          00000001     00000001" +
					"SO50030919130795BILL ARRIVED                                                    " +
					"SO60030919130797ADMISSIBLE                                                      " +
					"Y  1101SV9SO00005                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Declaration disposition codes processed", 1, decLoaded.DispositionCodes.Count);
			AssertEquals(CRLReleaseStatusList.Codes.ADM, decLoaded.ReleaseStatus);

			message = CreateStatusMessage(
				"B001101SV9SO                                                                    " +
				"SO100708SV9  73000414 01030712-00085NISD864X2TR864X2        19030030919         " +
				"SO20CR BCHB00174297                                                             " +
				"SO40RNISD00147479                                          00000001     00000001" +
				"SO50030919130895BILL ARRIVED                                                    " +
				"SO60030919130890UNDER CBP REVIEW                                                " +
				"Y  1101SV9SO00005                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Declaration disposition codes processed", 2, decLoaded.DispositionCodes.Count);
			AssertEquals(CRLReleaseStatusList.Codes.RVW, decLoaded.ReleaseStatus);

			message = CreateStatusMessage(
				"B001101SV9SO                                                                    " +
				"SO100708SV9  73000414 01030712-00085NISD864X2TR864X2        19030030919         " +
				"SO20CR BCHB00174297                                                             " +
				"SO40RNISD00147479                                          00000001     00000001" +
				"SO50030919131795BILL ARRIVED                                                    " +
				"SO60030919131790UNDER CBP REVIEW                                                " +
				"SO60030919131798RELEASED                                03091901                " +
				"SO60030919131701ONE USG                                                         " +
				"Y  1101SV9SO00007                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Declaration disposition codes processed", 5, decLoaded.DispositionCodes.Count);
			AssertEquals(CRLReleaseStatusList.Codes.REL, decLoaded.ReleaseStatus);
			AssertEquals(new ZDateTime(2019, 03, 09), decLoaded.JE_EntryAuthorisationDate);

			message = CreateStatusMessage(
				"B001101SV9SO                                                                    " +
				"SO100708SV9  73000414 01030712-00085NISD864X2TR864X2        19030030919         " +
				"SO20CR BCHB00174297                                                             " +
				"SO40RNISD00147479                                          00000001     00000001" +
				"SO60030919131801ONE USG                                                         " +
				"Y  1101SV9SO00004                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Declaration disposition codes processed", 6, decLoaded.DispositionCodes.Count);
			AssertEquals(CRLReleaseStatusList.Codes.REL, decLoaded.ReleaseStatus);
			AssertEquals(new ZDateTime(2019, 03, 09), decLoaded.JE_EntryAuthorisationDate);
		}

		public void TestSOMessageAttachedToENSEntry()
		{
			var declaration1 = GetDeclaration("73000415");
			var declaration2 = GetDeclaration("73000416");

			var addRef = declaration2.AdditionalReferenceNumbers.AddNew();
			addRef.CE_EntryType = "TES";
			addRef.CE_EntryNum = "73000415";

			var message = CreateStatusMessage("B001101SV9SO                                00                                  " +
					"SO101901SV9  73000415 0123-456789012AL                      2246 021413         " +
					"SO20CR B00159843                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO50022713172094BILL DEPARTED                                                   " +
					"SO60022713172098RELEASED                                02271301 16             " +
					"SO70ACE000103013143911EPA DATA REVIEW             020112333 777THRU5551215 1    " +
					"Y  3910SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration1.PK);
			AssertEquals("Message should attach to ENS Entry Num", 1, decLoaded.ActiveEntryHeaders[0].Messages.Count);
			decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration2.PK);
			AssertEquals("Message should not attach to a non ENS Entry Num", 0, decLoaded.ActiveEntryHeaders[0].Messages.Count);
		}

		public void TestGetEmailGroupRegistryItem_WillReturnLowValueEntriesReleaseMessagesRegistrySettings_WhenErrorStatusTicked()
		{
			var simplifiedEntryProcessor = new SimplifiedEntryStatusNotificationProcessor();
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			message.EM_LinkTable = "something";

			simplifiedEntryProcessor.Message = message;

			MethodInfo dynMethod = simplifiedEntryProcessor.GetType().GetMethod("GetEmailGroupRegistryItem",
			BindingFlags.NonPublic | BindingFlags.Instance);

			var registry = dynMethod.Invoke(simplifiedEntryProcessor, Array.Empty<object>());

			AssertEquals(USCustomsDataRegistry.Instance.ABIMessagesGroup, registry);

			simplifiedEntryProcessor.Message.EM_LinkTable = CusUSLVConsignmentSchema.Constants.TableName;
			registry = dynMethod.Invoke(simplifiedEntryProcessor, Array.Empty<object>());
			AssertEquals("Low Value Entries registry settings should be returned when message link table is CusUSLVConsignment", USCustomsDataRegistry.Instance.LowValueEntriesReleaseMessages, registry);
		}

		public void TestLogSERInMSCEventWhenUseCodeIsHVL()
		{
			var clearance = (BusinessObject)Factory.New<Integration.Customs.US.LVS.ICusUSLVClearance>();
			clearance[CusUSLVClearanceSchema.ULH_EntryFilerCode] = "SV9";
			clearance[CusUSLVClearanceSchema.ULH_UseCode] = "HVL";

			var consignment = (BusinessObject)Factory.New<Integration.Customs.US.LVS.ICusUSLVConsignment>();
			consignment[CusUSLVConsignmentSchema.ULB_ULH] = clearance.PK;

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_ParentTable = CusUSLVConsignmentSchema.Constants.TableName;
			entryNum.CE_ParentID = consignment.PK;
			entryNum.CE_EntryNum = "71002057";
			entryNum.CE_EntryType = "ENS";
			entryNum.CE_Category = "CUS";

			var bill = Factory.NewWithValidTestData<Bill>();
			bill.CU_BillNum = "PI3151202078";
			consignment[CusUSLVConsignmentSchema.ULB_HouseBill] = bill.CU_BillNum;

			var mscEvent = Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_Code, Events.MessageStatusChange.Code)).FirstOrDefault();
			mscEvent.SE_ReferenceFormat = "<EVENT><If(DEP != \"\", \" by <DEP>\", \"\")><If(MST != \"\", \" from <MST>\", \"\")><If(TYP != \"\", \": <TYP>\", \"\")><If(RFN != \"\", \", Reference No. <RFN>,\", \"\")><If(CRF != \"\", \" <CRF>,\", \"\")><If(OLD != \"\", \" from <OLD>\", \"\")><If(OLD == \"\" && NEW != \"\", \" <NEW>\", \"\")><If(OLD != \"\" && NEW != \"\", \" to <NEW>\", \"\")><If(LOC != \"\", \" at <CityCountry(LOC)>\", \"\")><If(EQN != \"\", \", <EQN>\", \"\")><If(FAC != \"\", \" at <FAC>\", \"\")><If(VFL!=\"\",\", <VFL>\",\"\")><If(FDT!=\"\",\", <datetime.Parse(FDT).Format(\"dd-MMM-yy\")>\",\"\")><If(RES != \"\", \" because <RES>\", \"\")><If(REF != \"\", \", <REF>\", \"\")>";

			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var msc = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageStatusChangeCode)).FirstOrDefault();
			AssertNotNull(msc);
			AssertEquals("|CRF=SV971002057|MST=Cargo Release|NEW=REL|RFN=PI3151202078|SER=PCS|TYP=SO", msc.SL_Reference);
			AssertEquals("Message: Status Change from Cargo Release: SO, Reference No. PI3151202078, SV971002057, REL", msc.DisplayEventReference);
		}

		public void TestUseABIMessagesGroupRegistrySetting_WhenSendSimplifiedEntryStatusNotificationEmailForDeclaration()
		{
			var declaration = GetDeclaration("71002057");

			AssertEmailControlledByABIMessageGroupRegistrySetting(new ManifestGroupNotification("NOE", ZGuid.Empty, false), (email) => { AssertNull(email); });
			AssertEmailControlledByABIMessageGroupRegistrySetting(new ManifestGroupNotification("ESG", Core.Constants.Groups.PostMastersGroupPK, false), (email) => { AssertNotNull(email); });

			void AssertEmailControlledByABIMessageGroupRegistrySetting(ManifestGroupNotification manifestGroupNotification, Action<EmailDef> assertAction)
			{
				var message = CreateStatusMessage(
						"B003901SV9SO                                                                    " +
						"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
						"SO40R    00591325206                                       00000150BL   00000000" +
						"SO60093013012523ENTRY CANCELLED                                                 " +
						"Y  3901SV9SO00000                                                               ");

				Factory.Save();

				using (USCustomsDataRegistry.Instance.ABIMessagesGroup.SetTemporaryValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, manifestGroupNotification))
				{
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					new ABIIncomingMessageProcessor().ExecuteBatch();

					var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));

					assertAction(email);
				}
			}
		}

		protected override void EndToEndCore()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new Bitmap(1, 2);
			var image2 = new Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "~AA";
			staff1.GS_EmailAddress = "blah@com.au";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "~BB";
			staff2.GS_EmailAddress = "blah2@com.au";
			Factory.Save();

			var declaration1 = GetDeclaration("71001240");
			declaration1.JE_MasterBill = "ALP31222406";
			declaration1.JE_GS_NKCusAgent = staff2.GS_Code;
			declaration1.JE_GB = newBranch.PK;

			var declaration2 = GetDeclaration("71001257");
			declaration2.JE_MasterBill = "ALP31222402";
			declaration2.JE_GS_NKCusAgent = staff2.GS_Code;
			declaration2.JE_GB = newBranch.PK;

			var declaration3 = GetDeclaration("71001224");
			declaration3.JE_MasterBill = "GFD34523436";
			declaration3.JE_GB = newBranch.PK;

			var declaration4 = GetDeclaration("71001273");
			declaration4.JE_MasterBill = "CMV32434231";
			declaration4.JE_GB = newBranch.PK;
			var mB2 = declaration4.Bills.AddNew();
			mB2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			mB2.CU_BillNum = "45345345";

			var declaration5 = GetDeclaration("70031321");
			declaration5.JE_MasterBill = "08188888881";
			declaration5.JE_GB = newBranch.PK;
			var house1 = declaration5.PrimaryMasterBill.ChildBills.AddNew();
			house1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house1.CU_BillNum = "789423789";
			var house2 = declaration5.PrimaryMasterBill.ChildBills.AddNew();
			house2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house2.CU_BillNum = "890543890";

			var declaration6 = GetDeclaration("71001232");
			declaration6.JE_MasterBill = "ALP31222421";
			declaration6.JE_GB = newBranch.PK;
			house1 = declaration6.PrimaryMasterBill.ChildBills.AddNew();
			house1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house1.CU_BillNum = "HAWB001";

			var declaration7 = GetDeclaration("71001238");
			declaration7.JE_MasterBill = "ALP31200421";
			declaration7.JE_GB = newBranch.PK;
			house1 = declaration7.PrimaryMasterBill.ChildBills.AddNew();
			house1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house1.CU_BillNum = "HAWB0022";

			var declaration1Message = CreateStatusMessage("B001101SV9SO                                00                                  " +
					"SO101901SV9  71001240 1123-456789012AL                      2246 021413         " +
					"SO20CR B00159827                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO50022513144594BILL DEPARTED                                                   " +
					"SO60022513144596DOCUMENT REQUIRED                               03              " +
					"Y  3910SV9SO00000");

			var declaration2Message = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101901SV9  71001257 0123-456789012AL                      2242 021413         " +
				"SO20CR B00159828                                                                " +
				"SO40R    ALP31222402                                       00000600             " +
				"SO50022513144293BILL ON FILE                                                    " +
				"SO60022513144296DOCUMENT REQUIRED                               03              " +
				"Y  3910SV9SO00000                                                               ");

			var declaration3Message = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101101SV9  71001224 0123-456789012                                            " +
				"SO20CR B00159818                                                                " +
				"SO40R    GFD34523436                                       00000006FL           " +
				"SO50022213101891NO BILL MATCH                                                   " +
				"Y  3910SV9SO00000");

			var declaration4Message = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101101SV9  71001273 0123-456789012                                            " +
				"SO20CR B00159836                                                                " +
				"SO40R    45345345                                          00000003AT           " +
				"SO50022713190491NO BILL MATCH                                                   " +
				"SO40R    CMV32434231                                       00000004PK           " +
				"SO50022713190491NO BILL MATCH                                                   " +
				"Y  3910SV9SO00000");

			var declaration5Message = CreateStatusMessage("B003910SV9SO                                00                                  " +
				"SO101101SV9  70031321 0123-456789012                                            " +
				"SO40M    08188888881                                                            " +
				"SO40H    789423789                                         00000001AE           " +
				"SO50022413223791NO BILL MATCH                                                   " +
				"SO40M    08188888881                                                            " +
				"SO40H    890543890                                         00000002AE           " +
				"SO50022413223791NO BILL MATCH                                                   " +
				"Y  3910SV9SO00000");

			var declaration6Message = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101901SV9  71001232 0123-456789012AL                      224L 021413         " +
				"SO20CR B00159823                                                                " +
				"SO40M    ALP31222421                                                            " +
				"SO40H    HAWB001                                           00000150             " +
				"SO50022213101894BILL DEPARTED                                                   " +
				"SO60022213101896DOCUMENT REQUIRED                               03              " +
				"Y  3910SV9SO00000");
			declaration6Message.EM_MessageNum = "HYEDUSCMT_145698";

			var declaration7Message = CreateStatusMessage("B001101SV9SO                                00                                  " +
				"SO101901SV9  71001238 0123-456789012AL                      224L 021413         " +
				"SO20CR B00159824                                                                " +
				"SO40M    ALP31200421                                                            " +
				"SO40H    HAWB0022                                          00000150             " +
				"SO50022213120494BILL DEPARTED                                                   " +
				"SO60022213120498RELEASED                                02221301                " +
				"Y  3910SV9SO00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			AssertDispositions(declaration1Message, declaration1.DispositionCodes, 1);
			var primaryBill = declaration1.PrimaryMasterBill;
			AssertEquals("Bill dispositions", 1, primaryBill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "94", primaryBill.DispositionCodes[0].US_Code);
			AssertEquals("Source", BillDispositionSourceList.Codes.SO, primaryBill.DispositionCodes[0].US_Source);

			AssertDispositions(declaration2Message, declaration2.DispositionCodes, 1);
			primaryBill = declaration2.PrimaryMasterBill;
			AssertEquals("Bill dispositions", 1, primaryBill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "93", primaryBill.DispositionCodes[0].US_Code);
			AssertEquals("Source", BillDispositionSourceList.Codes.SO, primaryBill.DispositionCodes[0].US_Source);

			declaration2.Logs.GetAllLogs().Load();
			AssertNotNull(declaration2.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus + " - DOC"));

			AssertDispositions(declaration3Message, declaration3.DispositionCodes, 0);
			primaryBill = declaration3.PrimaryMasterBill;
			AssertEquals("Bill dispositions", 1, primaryBill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "91", primaryBill.DispositionCodes[0].US_Code);

			AssertDispositions(declaration4Message, declaration4.DispositionCodes, 0);
			var bill = declaration4.Bills.FindByBillNumberAndType("45345345", Customs.Business.BillTypeList.Codes.MasterBill);
			AssertEquals("Bill dispositions", 1, bill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "91", bill.DispositionCodes[0].US_Code);
			bill = declaration4.Bills.FindByBillNumberAndType("CMV32434231", Customs.Business.BillTypeList.Codes.MasterBill);
			AssertEquals("Bill dispositions", 1, bill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "91", bill.DispositionCodes[0].US_Code);

			AssertDispositions(declaration5Message, declaration5.DispositionCodes, 0);
			bill = declaration5.Bills.FindByBillNumberAndType("789423789", Customs.Business.BillTypeList.Codes.HouseBill);
			AssertEquals("Bill dispositions", 1, bill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "91", bill.DispositionCodes[0].US_Code);
			AssertEquals("Source", BillDispositionSourceList.Codes.SO, bill.DispositionCodes[0].US_Source);

			bill = declaration5.Bills.FindByBillNumberAndType("890543890", Customs.Business.BillTypeList.Codes.HouseBill);
			AssertEquals("Bill dispositions", 1, bill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "91", bill.DispositionCodes[0].US_Code);

			AssertDispositions(declaration6Message, declaration6.DispositionCodes, 1);
			AssertEquals(ZString.Empty, declaration6Message.EM_MessageNum);

			bill = declaration6.Bills.FindByBillNumberAndType("HAWB001", Customs.Business.BillTypeList.Codes.HouseBill);
			AssertEquals("Bill dispositions", 1, bill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "94", bill.DispositionCodes[0].US_Code);

			declaration7.Logs.GetAllLogs().Load();
			AssertNotNull(declaration7.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus + " - REL"));
			AssertDispositions(declaration7Message, declaration7.DispositionCodes, 1);
			bill = declaration7.Bills.FindByBillNumberAndType("HAWB0022", Customs.Business.BillTypeList.Codes.HouseBill);
			AssertEquals("Bill dispositions", 1, bill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "94", bill.DispositionCodes[0].US_Code);

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(x => x.Subject.Contains("ACE Cargo Release Status Response"));
			AssertEquals(7, emails.Count);
			var banner = emails[0].Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(2, image.Width);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest => 7;

		void AssertDispositions(MQEDIMessage message, DispositionDataCollection dispositions, int expectedDispositionsCount)
		{
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			AssertEquals(expectedDispositionsCount, dispositions.Count);
		}

		JobDeclaration GetDeclaration(ZString entryNum)
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec.US_EntryFilerCode = "SV9";
			dec.US_EnableENS = false;
			dec.ImportEntryNumber = entryNum;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			return dec;
		}

		MQEDIMessage CreateStatusMessage(ZString msgText)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = msgText;
			return message;
		}

		void SetupStatement(string entryNum, string statementNum, string declarationReferenceNum)
		{
			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			dailyStatement.B2_StatementNumber = statementNum;
			dailyStatement.B2_GC = GlbCompany.CurrentCompany.PK;
			dailyStatement.B2_StatementAmount = 5.55m;

			var statementLine = Factory.New<CusStatementLine>();
			statementLine.B3_B2 = dailyStatement.PK;
			statementLine.B3_BrokerReference = declarationReferenceNum;
			statementLine.B3_EntryNum = entryNum;
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryFilerCode = "SV9";
			statementLine.B3_EntryStatus = "PA";
			Factory.Save();
		}
	}
}
