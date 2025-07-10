using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IDispositionDetailProviderExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestHasEarlierReleaseDispositionDateTime()
		{
			//no R5 with release details
			var message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse;
			message.EM_MessageNum = "35310";
			message.EM_MessageText = @"B012007M34IS                                               2953                 R1                                       EGLVEVER ELITE          0573E073111    R4605507582   003101253812                        00001513CTN  EGLV    MY1      R5091514164005PAPERLESS                                                         R6FDA    091510164004FDA EXAM/SAMPLE                                            Y  2007M34IS00165";

			//Sep-24
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			message2.EM_MessageNum = "35310";
			message2.EM_MessageText = @"B018888XJ5RR                                                                    R18888XJ5 010000480191-013199000B00001004FTRDADMIRALENGRACHT     1356 061307    R4            13465865424                         00001000KG   AAAD             R5092414110322ENTRY DOCUMENTS REQUIRED                10241409                  Y018888XJ5RR00003";

			//Sep-23
			var message3 = Factory.New<MQEDIMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageNum = "~15001";
			message3.EM_Status = EDIMessage.Status.Queued;
			message3.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 010000480191-013199000B00001004FTRDADMIRALENGRACHT     1356 061307    R4            13465865424                         00001000KG   AAAD             R5092314100322ENTRY DOCUMENTS REQUIRED                10201409                  Y018888XJ5RR00003";

			AssertNull(message.GetFirstReleaseDetailBlock());

			var block2 = message2.GetFirstReleaseDetailBlock();
			AssertNotNull(block2);

			var block3 = message3.GetFirstReleaseDetailBlock();
			AssertNotNull(block3);

			Assert(block3.HasEarlierReleaseDispositionDateTime(new MQEDIMessage[] { message2 }));
			Assert(!block2.HasEarlierReleaseDispositionDateTime(new MQEDIMessage[] { message3 }));

			//Sep-22
			var message4 = Factory.New<MQEDIMessage>();
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message4.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message4.EM_MessageNum = "35310";
			message4.EM_MessageText = @"B018888XJ5C1                                                                    WR18888XJ5 010000480191-013199000B00001004FTRDADMIRALENGRACHT    1356           WR4            13465865424                         00001000KG   AAAD            WR5092214110322ENTRY DOCUMENTS REQUIRED                  121514                 Y018888XJ5RR00003";

			var block4 = message4.GetFirstReleaseDetailBlock();
			AssertNull(block4); // WR5 is not a release details provider.
		}

		public void TestCalculateEntryStatusForDeleteReject()
		{
			var declaration = GetDeclaration("71002057");
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_Status = ImportMessageStatusList.Codes.CancellationRequestPending;
			declaration.US_EnableENS = false;
			CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60102715144987CANCELLATION REQUEST REJECTED                                   " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			entryHeader.Reload();
			AssertEquals(entryHeader.CH_Status, ImportMessageStatusList.Codes.CancellationRequestRejected);
		}

		public void TestCalculateEntryStatusForDeleteAccepted()
		{
			var declaration = GetDeclaration("71002057");
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_Status = ImportMessageStatusList.Codes.CancellationRequestPending;
			CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60100915144723ENTRY CANCELLED                                                 " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			entryHeader.Reload();
			declaration.US_EnableENS = true;
			AssertEquals(entryHeader.CH_Status, ImportMessageStatusList.Codes.ClearACECargoReleaseDelete);
		}

		public void TestUpdateQuotaStatus()
		{
			var declaration = GetDeclaration("71002057");
			CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60060418111670QUOTA PENDING                                                   " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			AssertEquals(declaration.US_QuotaStatus, CargoReleaseProcessingResultList.Codes.QuotaPending);
		}

		public void TestOneUSGMessage()
		{
			var declaration = GetDeclaration("71002057");
			CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60032416163201ONE USG                                                         " +
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			declaration.Logs.GetAllLogs().Load();

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "SO - 1USG");
			Assert(declaration.Logs.HasLogWith(logQuery));
		}

		public void TestCancellingRequestPending()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "70004250";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B  1101SV9SE                                               HYEDUSCMT_168117     SE10DSV9  71018202 01EI 58-12345678911800000100001101  1101                     SE13CRAIG SEELIG                            2155551212     03                   SE20EN SV911111111                                                              Y  1101SV9SE";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.Messages.Add(message);
			entry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;

			var incomingMessageSE9004 = Factory.New<MQEDIMessage>();
			incomingMessageSE9004.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessageSE9004.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			incomingMessageSE9004.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageSE9004.EM_MessageNum = "~15000";
			incomingMessageSE9004.EM_Status = EDIMessage.Status.Queued;
			incomingMessageSE9004.EM_MessageText =
				"B018888XJ5SX                                                                    " +
				"SE10DXJ5  70004250 01EI 58-12345678911800000100001101  1101                     " +
				"SE9004   CANCELLATION REQUEST PENDING                                           " +
				"Y  1101XJ5SX00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var factoryForLoad = new BusinessObjectFactory();
			var entryRealoaded = factoryForLoad.Load<CusEntryHeader>(entry.PK);
			AssertEquals(ImportMessageStatusList.Codes.CancellationRequestPending, entryRealoaded.CH_Status);

			var incomingMessageSE60 = Factory.New<MQEDIMessage>();
			incomingMessageSE60.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessageSE60.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			incomingMessageSE60.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageSE60.EM_MessageNum = "~15000";
			incomingMessageSE60.EM_Status = EDIMessage.Status.Queued;
			incomingMessageSE60.EM_MessageText =
				"B018888XJ5SO                                                                    " +
				"SO101101XJ5  70004250 0158-123456789                                            " +
				"SO20CR B00000001                                                                " +
				"SO20CMTTRANSMIT DELETION REQUEST W/ CARRIER LETTER TO THE DIS. RETRANSMIT DELET " +
				"SO20CMTION REQUEST VIA ACR.                                                     " +
				"SO40RAPLUMST020816A                                        00000100     00000000" +
				"SO50020516142195BILL ARRIVED                                                    " +
				"SO60020516142198RELEASED                                02011601                " +
				"SO60020516142187CANCELLATION REQUEST REJECTED                                   " +
				"Y  1101XJ5SO00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			factoryForLoad = new BusinessObjectFactory();

			entryRealoaded = factoryForLoad.Load<CusEntryHeader>(entry.PK);
			AssertEquals(ImportMessageStatusList.Codes.CancellationRequestRejected, entryRealoaded.CH_Status);

			var incomingMessageSE6021 = Factory.New<MQEDIMessage>();
			incomingMessageSE6021.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessageSE6021.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			incomingMessageSE6021.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageSE6021.EM_MessageNum = "~15000";
			incomingMessageSE6021.EM_Status = EDIMessage.Status.Queued;
			incomingMessageSE6021.EM_MessageText =
				"B018888XJ5SO                                                                    " +
				"SO101101XJ5  70004250 0158-123456789                                            " +
				"SO20CR B00000001                                                                " +
				"SO40RAPLUMST020816A                                        00000100     00000000" +
				"SO60020816104521ENTRY DELETED BY CBP                                            " +
				"Y  1101XJ5SO00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			factoryForLoad = new BusinessObjectFactory();

			entryRealoaded = factoryForLoad.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should be marked as deleted, as disposition code is 21.", ImportMessageStatusList.Codes.ClearACECargoReleaseDelete, entryRealoaded.CH_Status);

			var incomingMessageSE6023 = Factory.New<MQEDIMessage>();
			incomingMessageSE6023.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessageSE6023.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			incomingMessageSE6023.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageSE6023.EM_MessageNum = "~15000";
			incomingMessageSE6023.EM_Status = EDIMessage.Status.Queued;
			incomingMessageSE6023.EM_MessageText =
				"B018888XJ5SO                                                                    " +
				"SO101101XJ5  70004250 0158-123456789                                            " +
				"SO20CR B00000001                                                                " +
				"SO40RAPLUMST020816A                                        00000100     00000000" +
				"SO60020816104523ENTRY CANCELLED                                                 " +
				"Y  1101XJ5SO00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			factoryForLoad = new BusinessObjectFactory();

			entryRealoaded = factoryForLoad.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should be marked as deleted, as disposition code is 23.", ImportMessageStatusList.Codes.ClearACECargoReleaseDelete, entryRealoaded.CH_Status);
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
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
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
	}
}
