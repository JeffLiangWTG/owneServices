using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DeclarationEntriesStatusesAndErrorCollectionsTest : TestCaseWithFactory
	{
		#region ENS Tests

		public void TestUpdateEntrySummaryStatusNotificationDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E171333   090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";

			AssertEquals(1, declaration.ENSStatusNotifications.Count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			message.EM_LinkUniqueID = declaration.PK;
			message.EM_LinkTable = "JobDeclaration";
			AssertEquals(1, declaration.ENSStatusNotifications.Count);
		}

		public void TestUpdateEntrySummaryStatusNotificationDetailsForRecon()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var recon = new ReconDeclaration(declaration);

			AssertNotNull(recon.ReconEntry);

			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = recon.ReconEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E171333   090110                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                     5555555555  123456789012                    " +
				"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
				"Y  8888XJ5UC00003";

			AssertEquals(1, recon.ReconWrappedJobDeclaration.ENSStatusNotifications.Count);
		}

		public void TestENSERecords_NoErrors()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary, 1));

			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse, 2);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5ER                                               26740                " +
"10A888898-041772300                             8         XJ5 7000508301999     " +
"22            08151561655 WERWERER1               00000000                      " +
"E228888XJ5 70005083   42J01   INVALID IT/AWB/BL QUANTITY               B00150873" +
"E228888XJ5 70005083   EJY01   UNIT OF MEASURE REQUIRED                 B00150873" +
"E228888XJ5 70005083   52401   TRANSACTION DATA REJECTED                B00150873" +
"350000000000000000000000000000000000000000506000                                " +
"E358888XJ5 70005083   64101   INVALID CVD OR ADD ENTRY                 B00150873" +
"Y  8888XJ5ER00007";

			Factory.Save();

			var coll = new DeclarationEntriesStatusesAndErrorCollections(Declaration);
			AssertEquals("Errors should be shown", 4, coll.ENSERecords.Count);

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary, 3));
			Factory.Save();
			coll = new DeclarationEntriesStatusesAndErrorCollections(Declaration);
			AssertEquals("Errors should not be shown as there is a pending message", 0, coll.ENSERecords.Count);
		}

		public void TestENSERecords_Errors()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));
			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5ER                                               26740                " +
"10A888898-041772300                             8         XJ5 7000508301999     " +
"22            08151561655 WERWERER1               00000000                      " +
"E228888XJ5 70005083   42J01   INVALID IT/AWB/BL QUANTITY               B00150873" +
"E228888XJ5 70005083   EJY01   UNIT OF MEASURE REQUIRED                 B00150873" +
"E228888XJ5 70005083   52401   TRANSACTION DATA REJECTED                B00150873" +
"350000000000000000000000000000000000000000506000                                " +
"E358888XJ5 70005083   64101   INVALID CVD OR ADD ENTRY                 B00150873" +
"Y  8888XJ5ER00007";
			Factory.Save();

			AssertEquals(4, EntryStatusesAndErrors.ENSERecords.Count);
			AssertEquals("42J", EntryStatusesAndErrors.ENSERecords[0].ErrorMessageIdentifier);
			AssertEquals("0", EntryStatusesAndErrors.ENSERecords[0].LineNumber);
			AssertEquals("INVALID IT/AWB/BL QUANTITY", EntryStatusesAndErrors.ENSERecords[0].NarrativeMessage);
		}

		public void TestENSERecords()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));
			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B014701OHLER                                  5201OHL  1   441490               " +
"E04701OHL 00192637A10020719PAPERLESS - FILER RETAIN RECORDS        0122357A     " +
"EC4701OHL 00192637A10020719CERT-CONT BOND INSUFFICIENT: STB REQD   01223VJC     " +
"EC4701OHL 00192637A10020719CERT-ENTRY CANNOT BE CERTIFIED          012232A3     " +
"Y  4701OHLER00003000000512450";
			Factory.Save();

			AssertEquals(2, EntryStatusesAndErrors.ENSERecords.Count);
			AssertEquals("VJC", EntryStatusesAndErrors.ENSERecords[0].ErrorMessageIdentifier);
			AssertEquals("2A3", EntryStatusesAndErrors.ENSERecords[1].ErrorMessageIdentifier);
		}

		public void TestENSERecordsCWOMessage()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse));
			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B003902SV9CO                                               6009115              " +
"CW03SV9  10002580  00127C09C01CENSUS WARN OVERRIDE ACCPTD                       " +
"CW03SV9  10002580  00227J05485CENSUS WARNING COND NOT FOUND FOR OVRD            " +
"Y  3902SV9CO00002";
			Factory.Save();

			AssertEquals(1, EntryStatusesAndErrors.ENSERecords.Count);
			AssertEquals("485", EntryStatusesAndErrors.ENSERecords[0].ErrorMessageIdentifier);
		}

		public void TestENSE0RecordsCWOMessage()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse));
			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B003902SV9CO                                               6009115              " +
"CW03SV9  10002580  00127C09C01CENSUS WARN OVERRIDE ACCPTD                       " +
"CW03SV9  10002580  00227J05485CENSUS WARNING COND NOT FOUND FOR OVRD            " +
"Y  3902SV9CO00002";
			Factory.Save();

			AssertEquals(1, EntryStatusesAndErrors.ENSE0Records.Count);
			AssertEquals("(CWO)", EntryStatusesAndErrors.ENSE0Records[0].ErrorMessageIdentifier);
			AssertEquals("1 Line(s) Accepted; 1 Line(s) Rejected", EntryStatusesAndErrors.ENSE0Records[0].NarrativeMessage);
		}

		public void TestENS0Records()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));
			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5ER                                               26740                " +
"E08888XJ5 70005703B00151074ACCEPTED - RECORDS REQUIRED             02808        " +
"E08888XJ5 70005703B00151074CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     " +
"Y  8888XJ5ER00007";
			Factory.Save();

			AssertEquals(1, EntryStatusesAndErrors.ENSE0Records.Count);
			AssertEquals("ACCEPTED - RECORDS REQUIRED", EntryStatusesAndErrors.ENSE0Records[0].NarrativeMessage);

			AssertEquals(1, Declaration.DispositionCodesView.Count);
			AssertEquals("CERT-RELEASE CERTIFIED VIA SUMMARY", Declaration.DispositionCodesView[0].NarrativeMessage);
		}

		public void TestDispositionCodesViewWhenCertifiedInACSWhenReplacementIsSent()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var utcNow = ZDateTime.UtcNow;
			var outgoingOriginal = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			outgoingOriginal.EM_SystemCreateTimeUtc = utcNow.AddHours(-4);
			formalEntry.Messages.Add(outgoingOriginal);

			var incomingMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			outgoingOriginal.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-3);
			formalEntry.Messages.Add(incomingMessage);
			incomingMessage.EM_MessageText =
"B001101SV9AX                                               HYEDUSCMT_162229     " +
"E0 SUMMRY 000001 REF ID: SV9 71009664 B00163350    175                          " +
"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100966400100B00163350   " +
"Y  1101SV9AX00002";

			var incomingHDMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse);
			incomingHDMessage.EM_SystemCreateTimeUtc = utcNow.AddHours(-2);
			formalEntry.Messages.Add(incomingHDMessage);
			incomingHDMessage.EM_MessageText =
"B011101SV9HD                                               429141301            " +
"H1A1101SV9 7100966413-1479270004004291581                   AA  110101037       " +
"H2D284    13-147927000A001  0000005000B00163350                                 " +
"H61101SV9 71009664B0016335013-147927000DATA ADDED AS REQUESTED       2GC        " +
"H61101SV9 71009664B0016335013-147927000CARGO RELEASE DATA CERTIFIED  2A4        " +
"Y  1101SV9HD00004";

			Declaration.DispositionCodesView.RemoveAll();
			var collection = new DeclarationEntriesStatusesAndErrorCollections(Declaration);
			AssertEquals(1, collection.CargoReleaseRecords.Count);
			AssertEquals(1, Declaration.DispositionCodesView.Count);
			AssertEquals("CARGO RELEASE DATA CERTIFIED", Declaration.DispositionCodesView[0].NarrativeMessage);

			var outgoingMessage2 = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			outgoingMessage2.EM_SystemCreateTimeUtc = utcNow.AddHours(-1);
			outgoingMessage2.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_162231     10RSV9  71009664 1101B00163350   0140 XY          2051115                       1113-14792700013-147927000                     042915       NY                  20AA  1101042915D284                                                            21001                                                                           2200000001PC                                                                    23M    00155522213                                                              318B 037                                                                        40  001 CHCH042915        0000000150     0000000100    N                        47MCHHARWIN8PLA                                                                 47C13-147927000                                                                 47S13-147927000                                                                 508466939585 0000023500 0000005000             X                                6249900001732                                                                   8949900000002500                                                                9000000023500 00000002500 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			formalEntry.Messages.Add(outgoingMessage2);

			Declaration.DispositionCodesView.RemoveAll();
			collection = new DeclarationEntriesStatusesAndErrorCollections(Declaration);
			AssertEquals(1, collection.CargoReleaseRecords.Count);
			AssertEquals(1, Declaration.DispositionCodesView.Count);
			AssertEquals("The fact that cargo is certified remains", "CARGO RELEASE DATA CERTIFIED", Declaration.DispositionCodesView[0].NarrativeMessage);
		}

		public void TestENS0RecordsWithStatementUpdateNarrative()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));
			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5ER                                               26740                " +
"E08888XJ5 70005703B00151074ACCEPTED - RECORDS REQUIRED             02808        " +
"Y  8888XJ5ER00007";

			var statementMessage = CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction);
			formalEntry.Messages.Add(statementMessage);
			Factory.Save();

			var statementResponseMessage = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse);
			formalEntry.Messages.Add(statementResponseMessage);
			statementResponseMessage.EM_MessageText =
"B018888XJ5HT                                               30449                " +
"H18888XJ5 7000788157FSUMM REMVD FR STMT, DOCS NOW REQD        1      B00151238  " +
"Y  8888XJ5HT00001";
			Factory.Save();

			var coll = new DeclarationEntriesStatusesAndErrorCollections(Declaration);
			AssertEquals(2, coll.ENSE0Records.Count);
			AssertEquals("ACCEPTED - RECORDS REQUIRED", coll.ENSE0Records[0].NarrativeMessage);
			AssertEquals("SUMM REMVD FR STMT, DOCS NOW REQD", coll.ENSE0Records[1].NarrativeMessage);

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));
			Factory.Save();

			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5ER                                               26740                " +
"E08888XJ5 70005703B00151074ACCEPTED - RECORDS REQUIRED             02808        " +
"Y  8888XJ5ER00007";
			Factory.Save();

			coll = new DeclarationEntriesStatusesAndErrorCollections(Declaration);
			AssertEquals(3, coll.ENSE0Records.Count);
		}

		public void TestENSMessagesTransmittedRejected()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));
			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));
			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
				"B018888XJ5ER                                               35610                " +
"10A888891-01319900091-013199000                 8100909   XJ5 7002282301891  IL " +
"40001AU00000044000000000220000000000000000000000000000000                       " +
"E408888XJ5 7002282300145201   CHARGES AMOUNT INVALID                   B00152676" +
"E408888XJ5 7002282300102901   INVALID FOREIGN PORT CODE                B00152676" +
"50 8504406012          000000025000NO                               AU092309N   " +
"9000000000000000000000000 00000000000000000000000000000305000000004400          " +
"E908888XJ5 70022823   52401   TRANSACTION DATA REJECTED                B00152676" +
"Y  8888XJ5ER00007";

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));

			//MessageType is set to be EI for response message for this syntax error
			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummary);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
			"B012709XJ5EI                                  8888XJ5011   35606                " +
			"EB A  AND  B  REC DP/FLR/OFFICE CONFLICT                                        " +
			"EBTRANSACTION DATA REJECTED                                                     " +
			"Y  2709XJ5EI00036";
			AssertEquals("4", EntryStatusesAndErrors.ENSTransmitCount);
			AssertEquals("2", EntryStatusesAndErrors.ENSRejectCount);
		}

		public void TestENSMessagesTransmittedRejectedForACE()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary));
			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary));
			
			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
			"B00                                                        B                    " +
			"X0 BLOCK       1 REF ID: 8888 XJ5    AE 6009071                                 " +
			"X1 FX20   FILER NOT AUTHORIZED FOR APPLICATION ID                               " +
			"X1RF999   BATCH REJECTED                                                        " +
			"Y           00003";

			Factory.Save();
			AssertEquals("2", EntryStatusesAndErrors.ENSTransmitCount);
			AssertEquals("1", EntryStatusesAndErrors.ENSRejectCount);
		}

		public void TestENSStatusDateAndTransmitCountForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("ENS status date", ZDateTime.BrettsBirthday, coll.ENSStatusDate);
			AssertEquals("ENSTrasmitCount", "1", coll.ENSTransmitCount);

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("ENS status date", ZDateTime.BrettsBirthday.AddDays(1), coll.ENSStatusDate);
			AssertEquals("ENSTrasmitCount", "1", coll.ENSTransmitCount);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			message = CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(2);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("ENS status date", ZDateTime.BrettsBirthday.AddDays(2), coll.ENSStatusDate);
			AssertEquals("ENSTrasmitCount", "2", coll.ENSTransmitCount);

			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(3);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("ENS status date", ZDateTime.BrettsBirthday.AddDays(3), coll.ENSStatusDate);
			AssertEquals("ENSTrasmitCount", "2", coll.ENSTransmitCount);
		}

		public void TestENSStatusDateForCancelledEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = "ACS";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);

			var cancelledMessage = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification);
			cancelledMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(cancelledMessage);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;

			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("ENS status date should come from ACE UC message", ZDateTime.Today.AddDays(-1), coll.ENSStatusDate);
			AssertEquals("Cargo Release Msg date should come from ACE UC message", ZDateTime.Today.AddDays(-1), coll.CRLMessageStatusDate);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration2.US_EntryFilerCode = "XJ5";
			declaration2.US_EnableENS = true;
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			message = CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			declaration2.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);

			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-4);
			declaration2.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);

			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			declaration2.Messages.Add(message);
			declaration2.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;

			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration2);
			AssertEquals("ENS status date should come from RR message attached to declaration", ZDateTime.Today.AddDays(-3), coll.ENSStatusDate);
			AssertEquals("Cargo Release Msg date should come declaration RR message", ZDateTime.Today.AddDays(-3), coll.CRLMessageStatusDate);
		}

		public void TestENSERecordsForPGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("No response message yet", 0, coll.ENSERecords.Count);

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			message.EM_MessageText =
				"B001101SV9AX                                               HYEDUSCMT_162451     " +
				"E0 SUMMRY 000001 REF ID: SV9 71009920 B00163412                                 " +
				"E0 LINITM 0002   REF ID:2                                                       " +
				"E0 TARIFF 000001 REF ID:2921196010                                              " +
				"E0 OI            REF ID: PESTICIDES                                             " +
				"E0 PG01          REF ID: 001EPAPS1   Y                        130.027           " +
				"E1 PPH6   MISSING DIS DOCUMENTATION                                             " +
				"E0 PG02          REF ID: P125                                                   " +
				"E1 FP61   INVALID PRODUCT CODE QUALIFIER                                        " +
				"E0 PSTLIN 000001 REF ID: SV9 71009920 B00163412                                 " +
				"E1 FPGA   PGA DATA REJECTED                       SV9  71009920     B00163412   " +
				"E1RF998   TRANSACTION DATA REJECTED               SV9  71009920     B00163412   " +
				"Y  1101SV9AX00004";

			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			var ensRecords = coll.ENSERecords;
			AssertEquals(3, ensRecords.Count);

			var record1 = ensRecords[0];
			AssertEquals("LineNumber", "2", record1.LineNumber);
			AssertEquals("TariffNumber", "921196010", record1.TariffNumber);
			AssertEquals("PGAAgencyCode", "EPA", record1.PGAAgencyCode);
			AssertEquals("PGALine", "001", record1.PGALine);

			Assert("Error/Status code", ensRecords.Cast<ErrorsRecord>().Any(x => x.ErrorMessageIdentifier == "PH6"));
			Assert("NarrativeMessage", ensRecords.Cast<ErrorsRecord>().Any(x => x.NarrativeMessage == "MISSING DIS DOCUMENTATION"));

			Assert("Error/Status code", ensRecords.Cast<ErrorsRecord>().Any(x => x.ErrorMessageIdentifier == "P61"));
			Assert("NarrativeMessage", ensRecords.Cast<ErrorsRecord>().Any(x => x.NarrativeMessage == "INVALID PRODUCT CODE QUALIFIER"));
		}

		public void TestENSERecordsForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("No response message yet", 0, coll.ENSERecords.Count);

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			message.EM_MessageText =
					"B003902SV9AX                                               6007798              "
				+ "E0 SUMMRY 000001 REF ID: SV9 10000931 B00005021                                 "
				+ "E0 LINITM 000001 REF ID: 001                                                    "
				+ "E0 TARIFF 000001 REF ID: 99021062                                               "
				+ "E1 F434   HTS NBR NOT ACTIVE                      SV9  10000931     B00005021   "
				+ "E0 TARIFF 000002 REF ID: 9032896015                                             "
				+ "E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  10000931     B00005021   "
				+ "E0 LINITM 000002 REF ID: 002                                                    "
				+ "E0 TARIFF 000001 REF ID: 99021062                                               "
				+ "E1 F434   HTS NBR NOT ACTIVE                      SV9  10000931     B00005021   "
				+ "E1RF998   TRANSACTION DATA REJECTED               SV9  10000931     B00005021   "
				+ "Y  3902SV9AX00010";

			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals(3, coll.ENSERecords.Count);
			AssertEquals("Rejected Count", "1", coll.ENSRejectCount);

			message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(2);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("Another outgoing sent & not responded yet", 0, coll.ENSERecords.Count);
			AssertEquals("Rejected Count", "1", coll.ENSRejectCount);
		}

		public void TestENS0RecordsForCanceledEntry()
		{
			//ACE
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EntryFilerCode = "XJ5";

			var formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			formalEntry.EntryNumber = "00000063";

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary));
			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 00000063 B00155595                                 " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 4703110000                                             " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022270     B00155595   " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   " +
"Y  1101SV9AX00005";

			Factory.Save();

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification);
			formalEntry.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E161333010062314                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";
			formalEntry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;
			Factory.Save();

			var coll = new DeclarationEntriesStatusesAndErrorCollections(Declaration);
			AssertEquals("Should be only one record for canceled entry - if cancelled, do not show status from Entry Summary responses", 1, coll.ENSE0Records.Count);
			AssertEquals("Entry Summary Canceled", coll.ENSE0Records[0].NarrativeMessage);
			AssertEquals(new ZDateTime(2014, 06, 23), coll.ENSE0Records[0].StatusDate);

			//ACS
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_EntryFilerCode = "XJ5";

			formalEntry = Declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			formalEntry.EntryNumber = "00000063";

			formalEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));

			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			formalEntry.Messages.Add(message);
			message.EM_MessageText = "B018888XJ5ER                                               6000790              E08888XJ5 10000657B00001267ACCEPTED - RECORDS REQUIRED             01808        E08888XJ5 10000657B00001267CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002";
			Factory.Save();

			var cargoReleaseProcessingResult = (MQEDIMessage)Declaration.Messages.AddNew(typeof(MQEDIMessage));
			cargoReleaseProcessingResult.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cargoReleaseProcessingResult.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			cargoReleaseProcessingResult.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			cargoReleaseProcessingResult.EM_MessageText =
			"B018888XJ5RR                                                                    " +
			"R18888XJ5 700097210138-285097000B00151484APLU23                  56   070209    " +
			"R4            YUIRE897                            00000001PK   APLU             " +
			"R5062314160523ENTRY CANCELLED                                                   " +
			"Y  8888XJ5RR00003";

			formalEntry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;
			Factory.Save();

			coll = new DeclarationEntriesStatusesAndErrorCollections(Declaration);
			AssertEquals("Should be only one record for canceled entry - if cancelled, do not show status from Entry Summary responses", 1, coll.ENSE0Records.Count);
			AssertEquals("ENTRY CANCELLED", coll.ENSE0Records[0].NarrativeMessage);
			AssertEquals(new ZDateTime(2014, 06, 23), coll.ENSE0Records[0].StatusDate);
		}

		#endregion

		#region Cargo Release Tests

		public void TestCRLRecords()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader cRLEntry = Declaration.ActiveEntryHeaders.AddNew();
			cRLEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;

			cRLEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse);
			cRLEntry.Messages.Add(message);
			message.EM_MessageText =
			"B018888XJ5HR                                               35934                " +
			"H1A8888XJ5 7002309491-0131990004010260981                   LH  888801891       " +
			"H2I299    91-013199000 430  0000001500B00152733                                 " +
			"H68888XJ5 70023094B0015273391-013199000DATA ADDED AS REQUESTED       2GC        " +
			"H68888XJ5 70023094B0015273391-013199000CARGO RELEASE DATA CERTIFIED  2A4        " +
			"Y  8888XJ5HR00004";
			Factory.Save();

			AssertEquals(1, EntryStatusesAndErrors.CargoReleaseRecords.Count);
			AssertEquals("DATA ADDED AS REQUESTED", EntryStatusesAndErrors.CargoReleaseRecords[0].NarrativeMessage);

			AssertEquals(1, Declaration.DispositionCodesView.Count);
			AssertEquals("CARGO RELEASE DATA CERTIFIED", Declaration.DispositionCodesView[0].NarrativeMessage);

			CargoWise.Common.ErrorReporter.Clear();
			Declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			AssertEquals("messages are marked as discarded", EDIMessage.Status.Discarded, message.EM_Status);
			if (CargoWise.Common.ErrorReporter.LastMessageReported == "Deleting CusEntryHeader with messages")
			{
				CargoWise.Common.ErrorReporter.Clear();
			}
			CusEntryHeader bcrEntry = Declaration.ActiveEntryHeaders.AddNew();
			bcrEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			bcrEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BorderCargoRelease));
			Factory.Save();

			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse);
			bcrEntry.Messages.Add(message);
			message.EM_MessageText =
			"B018888XJ5HS                                               31367                " +
			"01A8888XJ5700089133091-013199000857291-01319900006050901 ABFS                   " +
			"02XO3920995000AUABCEXP72ALE               0000010307                            " +
			"E028888XJ57000891379IMANUFACTURER NUMBER NOT ON FIL                             " +
			"E028888XJ570008913524TRANSACTION DATA REJECTED                                  " +
			"Y  8888XJ5HS00004";
			Factory.Save();
			fEntryStatusesAndErrors = null;

			AssertEquals(2, EntryStatusesAndErrors.CargoReleaseRecords.Count);
			AssertEquals("MANUFACTURER NUMBER NOT ON FIL", EntryStatusesAndErrors.CargoReleaseRecords[0].NarrativeMessage);
			AssertEquals(USConstants.NarrativeRejectedMessage, EntryStatusesAndErrors.CargoReleaseRecords[1].NarrativeMessage);
		}

		public void TestCOTariffRecords()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var processingMessage1 = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);
			processingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			Declaration.Messages.Add(processingMessage1);
			processingMessage1.EM_MessageText =
			"B018888XJ5RR                                                                    " +
			"R18888XJ5 700044250298-041772300B00150729AA                      0970 080708    " +
			"R3001NI6110202079                                                               " +
			"R3003AD6110202079                                                               " +
			"R3007NI6110202079                                                               " +
			"R4            00112212944                         00001016CT                    " +
			"R5082708214906ENTRY DOCUMENTS REQUIRED                                          " +
			"R5082708214931CST APPROVAL REQUIRED                                             " +
			"Y018888XJ5RR00007";

			var processingMessage2 = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);
			processingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			Declaration.Messages.Add(processingMessage2);
			processingMessage2.EM_MessageText =
			"B012704OHLRR                                                                    " +
			"R12704OHL 000004340195-455335100X90001042APLUAPL JADE            V137 063009    " +
			"R3001PL1902112030                                                               " +
			"R4            702230386   GDYLA00429              00000953PK   APLUULFM         " +
			"R5063009201906ENTRY DOCUMENTS REQUIRED                                          " +
			"R5063009201931CST APPROVAL REQUIRED                                             " +
			"R6FDA    063009201904FDA EXAM/SAMPLE                                            " +
			"R6FDA    0630092019  FDA EXAM, NOTIFY                 02001 001THRU001 008      " +
			"Y  2704OHLRR00007";

			var processingMessage3 = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);
			processingMessage3.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-8);
			Declaration.Messages.Add(processingMessage3);
			processingMessage3.EM_MessageText =
			"B018888XJ5RR                                                                    " +
			"R18888XJ5 700097210138-285097000B00151484APLU23                  56   070209    " +
			"R4            YUIRE897                            00000001PK   APLU             " +
			"R5070509202716PENDING CUSTOMS REVIEW                                            " +
			"Y  8888XJ5RR00003";

			Factory.Save();

			AssertEquals(1, EntryStatusesAndErrors.COTariffRecords.Count);
			AssertEquals("1", EntryStatusesAndErrors.COTariffRecords[0].LineNumber);
			AssertEquals("PL", EntryStatusesAndErrors.COTariffRecords[0].CountryOfOrigin);
			AssertEquals("1902112030", EntryStatusesAndErrors.COTariffRecords[0].TariffNumber);

			AssertEquals("C/O, Tariff(s) Exist", EntryStatusesAndErrors.COTariffExist);

			fEntryStatusesAndErrors = null;
			var processingMessage4 = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse);
			processingMessage4.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-7);
			Entry.Messages.Add(processingMessage4);
			processingMessage4.EM_MessageText =
				"B018888XJ5IS                                                                    " +
				"R1                                       CTYOXIN CHONG QING      0178E061210    " +
				"R3005IT1000200123                        CTYOXIN CHONG QING      0178E061210    " +
				"R4            NB7101648   1                       00000757CTN  CHNJCTYO         " +
				"R5052010045755CAR AMEND ADD                                            001      " +
				"R1                                       CTYOXIN CHONG QING      0178E061210    " +
				"R4            NB7101648                           00000757CTN  CHNJCTYO         " +
				"R5052010045721CENTER RELEASED                                  00002353001      " +
				"Y  8888XJ5IS00003";
			Factory.Save();

			AssertEquals(1, EntryStatusesAndErrors.COTariffRecords.Count);
			AssertEquals("5", EntryStatusesAndErrors.COTariffRecords[0].LineNumber);
			AssertEquals("IT", EntryStatusesAndErrors.COTariffRecords[0].CountryOfOrigin);
			AssertEquals("1000200123", EntryStatusesAndErrors.COTariffRecords[0].TariffNumber);

			AssertEquals("C/O, Tariff(s) Exist", EntryStatusesAndErrors.COTariffExist);

			fEntryStatusesAndErrors = null;
			var processingMessage5 = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse);
			processingMessage5.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			Entry.Messages.Add(processingMessage5);
			processingMessage5.EM_MessageText =
				"B018888XJ5C1                                                                    " +
				"WR12704SV9 7003838301                     APLUAA TEST1            001T 120414   " +
				"WR2                                                                         A001" +
				"WR3004HK8438909090                                                              " +
				"WR4            MASTER11    HOUSE34                 00000034KG   APLUWERTMN1     " +
				"WR5120214075617 ELECTRONIC INVOICE REQUIRED                      00000049       " +
				"WN1            27042704                27046200011301462000 TEST CON   113014   " +
				"Y  8888XJ5C100003";
			Factory.Save();

			AssertEquals(1, EntryStatusesAndErrors.COTariffRecords.Count);
			AssertEquals("4", EntryStatusesAndErrors.COTariffRecords[0].LineNumber);
			AssertEquals("HK", EntryStatusesAndErrors.COTariffRecords[0].CountryOfOrigin);
			AssertEquals("8438909090", EntryStatusesAndErrors.COTariffRecords[0].TariffNumber);
			AssertEquals("C/O, Tariff(s) Exist", EntryStatusesAndErrors.COTariffExist);
		}

		public void TestBCRRecords()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader bCREntry = Declaration.ActiveEntryHeaders.AddNew();
			bCREntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;

			bCREntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BorderCargoRelease));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse);
			bCREntry.Messages.Add(message);
			message.EM_MessageText =
			"B018888XJ5HS                                               35794                " +
			"01A8888XJ5700230373041-208411000842141-20841100009270901 BAAB                   " +
			"E028888XJ5700230372GCDATA ADDED AS REQUESTED                                    " +
			"E028888XJ5700230372A4CARGO RELEASE DATA CERTIFIED                               " +
			"Y  8888XJ5HS00003";
			Factory.Save();

			AssertEquals(1, EntryStatusesAndErrors.CargoReleaseRecords.Count);
			AssertEquals("DATA ADDED AS REQUESTED", EntryStatusesAndErrors.CargoReleaseRecords[0].NarrativeMessage);

			AssertEquals(1, Declaration.DispositionCodesView.Count);
			AssertEquals("CARGO RELEASE DATA CERTIFIED", Declaration.DispositionCodesView[0].NarrativeMessage);
		}

		public void TestCRLMessagesTransmittedRejected()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader cRLEntry = Declaration.ActiveEntryHeaders.AddNew();
			cRLEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			cRLEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			cRLEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			CusEntryHeader bCREntry = Declaration.ActiveEntryHeaders.AddNew();
			bCREntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			bCREntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BorderCargoRelease));
			bCREntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BorderCargoRelease));

			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse);
			cRLEntry.Messages.Add(message);
			message.EM_MessageText =
			"B018888XJ5HR                                               35934                " +
			"H1A8888XJ5 7002309491-0131990004010260981                   LH  888801891       " +
			"H2I299    91-013199000 430  0000001500B00152733                                 " +
			"H68888XJ5 70023094B0015273391-013199000DATA ADDED AS REQUESTED       2GC        " +
			"H68888XJ5 70023094B0015273391-013199000CARGO RELEASE DATA CERTIFIED  2A4        " +
			"Y  8888XJ5HR00004";

			MQEDIMessage bCRMessage = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse);
			bCREntry.Messages.Add(bCRMessage);
			bCRMessage.EM_MessageText =
				"B018888XJ5HS                                               35794                " +
				"01A8888XJ5700230373041-208411000842141-20841100009270901 BAAB                   " +
				"E028888XJ5700230372GCDATA ADDED AS REQUESTED                                    " +
				"E028888XJ5700230372A4CARGO RELEASE DATA CERTIFIED                               " +
				"Y  8888XJ5HS00003";

			MQEDIMessage failedMessage = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse);
			cRLEntry.Messages.Add(failedMessage);
			failedMessage.EM_MessageText =
			"B018888XJ5HR                                               35935                " +
			"H1A8888XJ5 7002297199-9999999001110170981                   MAEU100101432       " +
			"H2S505    99-999999900 425W 0000000000B00152704            VALDIVIA             " +
			"H68888XJ5 70022971B0015270499-999999900IMPORTER NUMBER NOT ON FILE   646        " +
			"H68888XJ5 70022971B0015270499-999999900TRANSACTION DATA REJECTED     524        " +
			"Y  8888XJ5HR00004";

			MQEDIMessage failedBCRMessage = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse);
			bCREntry.Messages.Add(failedBCRMessage);
			failedBCRMessage.EM_MessageText =
			"B018888XJ5HS                                               31367                " +
			"01A8888XJ5700089133091-013199000857291-01319900006050901 ABFS                   " +
			"02XO3920995000AUABCEXP72ALE               0000010307                            " +
			"E028888XJ57000891379IMANUFACTURER NUMBER NOT ON FIL                             " +
			"E028888XJ570008913524TRANSACTION DATA REJECTED                                  " +
			"Y  8888XJ5HS00004";
			Factory.Save();

			fEntryStatusesAndErrors = null;
			AssertEquals("4", EntryStatusesAndErrors.CRLTransmitCount);
			AssertEquals("2", EntryStatusesAndErrors.CRLRejectCount);
		}

		public void TestSimplifiedEntryMessagesTransmittedRejected()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			var entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.CargoRelease));
			entry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.CargoRelease));
			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			entry.Messages.Add(message);
			message.EM_MessageText =
			"B011101SV9SX                                               HYEDUSCMT_145823     " +
"SE10ASV9  71001323 01EI 23-45678901240800000100001101                           " +
"SE15R    ALP31222402                                       00000100CS           " +
"SE20CR B00159868                                                                " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00015";
			Factory.Save();
			AssertEquals("2", EntryStatusesAndErrors.CRLTransmitCount);
			AssertEquals("0", EntryStatusesAndErrors.CRLRejectCount);

			var failedMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			entry.Messages.Add(failedMessage);
			failedMessage.EM_MessageText =
			"B011101SV9SX                                               HYEDUSCMT_145815     " +
"SE10ASV9  71001323 01EI 23-45678901240800000100001101                           " +
"SE9011037MISSING SELLER                                                         " +
"SE15R    ALP31222402                                       00000100CS           " +
"SE20CR B00159868                                                                " +
"SE40001AU                                                                       " +
"SE9011037MISSING SELLER                                                         " +
"SE9001   SE DATA REJECTED                                                       " +
"Y  1101SV9SX00012";
			Factory.Save();

			fEntryStatusesAndErrors = null;
			AssertEquals("2", EntryStatusesAndErrors.CRLTransmitCount);
			AssertEquals("1", EntryStatusesAndErrors.CRLRejectCount);
		}

		void SetUpEmailing()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "~Z";
			Factory.Save();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "X1";
			staff.GS_LoginName = "x1";
			staff.GS_EmailAddress = "brett@pretend.email.com";
		}

		public void TestStatusHierarchyForACECargoRelease()
		{
			SetUpEmailing();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			Entry.EntryNumber = "71001299";
			Entry.Declaration.US_EntryFilerCode = "SV9";
			Factory.Save();

			var block_1 = "SO60022713165531CST APPROVAL REQUIRED                                           ";
			var block_2 = "SO60022713165551MANIFEST HOLD CBP                                               ";
			var block_3 = "SO60022713165596DOCUMENT REQUIRED                               03              ";
			var block_4 = "SO60022713165522RELEASE DATE UPDATE                           99                ";
			var block_5 = "SO60022713165503PENDING INTENSIVE EXAM                                          ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.EXM, block_1, block_2, block_3, block_4, block_5);

			var block2_2 = "SO60022713165631CST APPROVAL REQUIRED                                           ";
			var block2_3 = "SO60022713165696DOCUMENT REQUIRED                               03              ";
			var block2_4 = "SO60022713165622RELEASE DATE UPDATE                           99                ";
			var block2_5 = "SO60022713165651MANIFEST HOLD CBP                                               ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.HLD, block2_2, block2_2, block2_3, block2_4, block2_5);

			var block3_3 = "SO60022713165731CST APPROVAL REQUIRED                                           ";
			var block3_4 = "SO60022713165722RELEASE DATE UPDATE                           99                ";
			var block3_5 = "SO60022713165796DOCUMENT REQUIRED                               03              ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.DOC, block3_3, block3_4, block3_5);

			var block4_4 = "SO60022713165831CST APPROVAL REQUIRED                                           ";
			var block4_5 = "SO60022713165897ADMISSIBLE                                                      ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.ADM, block4_4, block4_5);

			var block5_5 = "SO60022713165931CST APPROVAL REQUIRED                                           ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.ADM, block5_5);

			var block6_6 = "SO60022713165925ENTRY WILL BE CANCELLED IN 7 DAYS                               ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.NRC, block6_6);
		}

		public void TestStatusHierarchyAboutRVW()
		{
			SetUpEmailing();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			Entry.EntryNumber = "71001299";
			Entry.Declaration.US_EntryFilerCode = "SV9";
			Factory.Save();

			var block3_3 = "SO60022713165531CST APPROVAL REQUIRED                                           ";
			var block3_4 = "SO60022713165590UNDER CBP REVIEW                                                ";
			var block3_5 = "SO60022713165596DOCUMENT REQUIRED                               03              ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.DOC, block3_3, block3_4, block3_5);

			var block4_3 = "SO60022713165631CST APPROVAL REQUIRED                                           ";
			var block4_4 = "SO60022713165697ADMISSIBLE                                                      ";
			var block4_5 = "SO60022713165690UNDER CBP REVIEW                                                ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.RVW, block4_3, block4_4, block4_5);

			var block5_4 = "SO60022713165731CST APPROVAL REQUIRED                                           ";
			var block5_5 = "SO60022713165797ADMISSIBLE                                                      ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.ADM, block5_4, block5_5);
		}

		public void TestCalculateReleaseStatusForACECargoRelease()
		{
			SetUpEmailing();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			Entry.Declaration.US_EnableENS = false;
			Entry.Declaration.US_CertifyCargoRelease = false;
			Entry.EntryNumber = "71001299";
			Entry.Declaration.US_EntryFilerCode = "SV9";
			Factory.Save();

			var block_1 = "SO60022713162696DOCUMENT REQUIRED                               03              ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.DOC, block_1);

			var block_2 = "SO60022713165596DOCUMENT REQUIRED                               03              ";
			var block_3 = "SO60022713165503PENDING INTENSIVE EXAM                                          ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.EXM, block_2, block_3);

			var block_4 = "SO50030413161193BILL ON FILE                                                    ";
			var block_5 = "SO60030413161197ADMISSIBLE                                                      ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.ADM, block_4, block_5);

			var block_6 = "SO60030413172098RELEASED                                02271301                ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.REL, block_6);

			Entry.Declaration.ReleaseStatus = ZString.Empty;
			Entry.Declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			Entry.Declaration.US_EntryFilerCode = "SV9";
			Entry.Declaration.ImportEntryNumber = "71002883";
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageText = "B001101SV9SO                                                                    SO101101SV9  71002883 0123-456789012CO                      2009 0909131        SO20CR B00160856                                                                SO40M    00591325209                                                            SO40H    HAWB001                                           00000100     00000250SO50102413011594BILL DEPARTED                           YCOA 2009 0909131101    SO40M    00591325209                                                            SO40H    HAWB001                                           00000150     00000250SO50102413011594BILL DEPARTED                           YCOA 4009 0909131101    SO60102413011598RELEASED                                10241301                Y  1101SV9SO00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(Entry.Declaration.PK);
			AssertEquals("Release Status", CRLReleaseStatusList.Codes.REL, declaration.ReleaseStatus);
			AssertEquals("Release Date", new ZDateTime(2013, 10, 24), declaration.JE_EntryAuthorisationDate);
		}

		public void TestCalculateReleaseStatusWithDispositionOrder()
		{
			SetUpEmailing();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			Entry.EntryNumber = "71001299";
			Entry.Declaration.US_EntryFilerCode = "SV9";
			Factory.Save();

			var block_1 = "SO60042619172098RELEASED                                04301901                ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.REL, block_1);
			var declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(Entry.Declaration.PK);
			AssertEquals(new ZDateTime(2019, 04, 30), declarationLoaded.JE_EntryAuthorisationDate);

			var block_2 = "SO60042619272099RELEASE SUSPENDED                                               ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.NRL, block_2);
			declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(Entry.Declaration.PK);
			AssertEquals(ZDateTime.Empty, declarationLoaded.JE_EntryAuthorisationDate);
			AssertNotNull(declarationLoaded.Logs.MostRecentLogByEventTime(Events.AuthorisationWithdrawn, "SO - NRL"));

			var block_3 = "SO60042619302098RELEASED                                05011901                ";
			var block_4 = "SO60042619302001ONE USG                                                         ";
			var block_5 = "SO60042619302090UNDER CBP REVIEW                                                ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.RVW, block_3, block_4, block_5);
			declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(Entry.Declaration.PK);
			AssertEquals(ZDateTime.Empty, declarationLoaded.JE_EntryAuthorisationDate);

			var block_6 = "SO60042619302101ONE USG                                                         ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.RVW, block_6);
			declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(Entry.Declaration.PK);
			AssertEquals(ZDateTime.Empty, declarationLoaded.JE_EntryAuthorisationDate);

			var block_7 = "SO60042619310022RELEASE DATE UPDATE                     05021901                ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.REL, block_7);
			declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(Entry.Declaration.PK);
			AssertEquals(new ZDateTime(2019, 05, 02), declarationLoaded.JE_EntryAuthorisationDate);

			var block_8 = "SO60042619350005PAPERLESS ENTRY                                                 ";
			var block_9 = "SO60042619350022RELEASE DATE UPDATE                           99                ";
			var block_10 = "SO60042619350052MANIFEST HOLD AGRICULTURE                                       ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.HLD, block_8, block_9, block_10);
			declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(Entry.Declaration.PK);
			AssertEquals(ZDateTime.Empty, declarationLoaded.JE_EntryAuthorisationDate);
			AssertNotNull(declarationLoaded.Logs.MostRecentLogByEventTime(Events.AuthorisationWithdrawn, "SO - HLD"));

			var block_11 = "SO60042619402098RELEASED                                05021901                ";
			var block_12 = "SO60042619402052MANIFEST HOLD AGRICULTURE                                       ";
			ProcessAndAssertSimplifiedEntryReleaseStatus(CRLReleaseStatusList.Codes.HLD, block_11, block_12);
			declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(Entry.Declaration.PK);
			AssertEquals(ZDateTime.Empty, declarationLoaded.JE_EntryAuthorisationDate);
			AssertNotNull(declarationLoaded.Logs.MostRecentLogByEventTime(Events.AuthorisationWithdrawn, "SO - HLD"));
		}

		void ProcessAndAssertSimplifiedEntryReleaseStatus(ZString expectedStatus, params string[] additionalStatusBlocks)
		{
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;

			var builder = new StringBuilder();
			builder.Append("B001101SV9SO                                00                                  ");
			builder.Append("SO101901SV9  71001299 0123-456789012AL                      2246 021413         ");
			builder.Append("SO20CR B00159843                                                                ");
			builder.Append("SO40R    ALP31222406                                       00000600             ");
			builder.Append("SO50022713165594BILL DEPARTED                                                   ");
			foreach (ZString blockText in additionalStatusBlocks)
			{
				builder.Append(blockText);
			}
			builder.Append("Y  3910SV9SO00000");
			incomingMessage.EM_MessageText = builder.ToString();
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(Entry.Declaration.PK);
			AssertEquals("Release Status", expectedStatus, declaration.ReleaseStatus);
		}

		public void TestCargoReleaseDetailsForACEEntrySummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = "ACS";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			//certifying from entry summary
			message.EM_MessageText = "B  3902SV9AE                                               6008834              10ASV9  10001699 3902B00005106   0110 XY          2020211                       1191-01319900091-013199000                     013111       IL                  20APLU3902013111I071ADMIRALENGRACHT                                             2156                                                                            2200000001PK                                                                    23MAPLU792479                                                                   318B 891                                                                        40  001 DEAU013111        0000000006602670000000150    N                        44SNOWMOBILE                                                                    47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 502008200010 0000000053 0000002000 000000015000KG                               OI        SNOWMOBILE                                                            FD0100121UAB07   DESLNSMITH                    THLIATHA191NAK THLIATHA191NAK    FD020000021000KG                                                                FD030000002000                                                                  FD05ADA01312011                                                                 FD05APA3902                                                                     FD05CSHAU                                                                       FD05FMEH                                                                        FD05OFTI                                                                        FD05PFTG                                                                        FD05SA187432 MAIN ROAD                                                          FD05SACCHICAGO                                                                  FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNMR IMPORTER                                                              FD05SCZ60125                                                                    FD05SEMNONE                                                                     FD05SFNJOHN                                                                     FD05SFTI                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT56                                                                       FD05BOLAPLU792479                                                               FD05TEMNONE                                                                     6250100000250                                                                   6249900000420                                                                   40  002 THAU013111        0000000012602670000000210    N                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 509802004040 0000000000 0000001000                                              502008200010 0000000054 0000003000 000000021000KG                               OI        PINEAPPLE                                                             FD0100121UAB07   DESLNSMITH                    THLIATHA191NAK THLIATHA191NAK    FD020000021000KG                                                                FD030000004000                                                                  FD05ADA01312011                                                                 FD05APA3902                                                                     FD05CSHAU                                                                       FD05FMEH                                                                        FD05OFTI                                                                        FD05PFTG                                                                        FD05SA187432 MAIN ROAD                                                          FD05SACCHICAGO                                                                  FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNMR IMPORTER                                                              FD05SCZ60125                                                                    FD05SEMNONE                                                                     FD05SFNJOHN                                                                     FD05SFTI                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT56                                                                       FD05BOLAPLU792479                                                               FD05TEMNONE                                                                     6250100000500                                                                   40  003 THAU013111        0000000032602670000002500    N                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 509802004040 0000000000 0000010000                                              508537109070 0000001350 0000000500 000000001200NO                               OA  FD0                                                                         6250100001313                                                                   895010000000206349900000002500                                                  9000000001457 00000004563 00000000000 00000000000 00000000000                   Y  3902SV9AE";
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("CRL Message Status", ZDateTime.Empty, coll.CRLMessageStatusDate);
			AssertEquals("CargoReleaseError", 0, coll.CargoReleaseRecords.Count);

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			message.EM_MessageText = "B003902SV9AX                                               6008834              E0 SUMMRY 000001 REF ID: SV9 10001699 B00005106                                 E1AI995   SUMMARY HAS BEEN ADDED                  SV9  10001699     B00005106   Y  3902SV9AX00002";
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("CRL Message Status: CR transaction respose has not arrived", ZDateTime.Empty, coll.CRLMessageStatusDate);
			AssertEquals("CargoReleaseError", 0, coll.CargoReleaseRecords.Count);

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse);
			message.EM_MessageText = "B013902SV9HD                                               131160953            H1A3902SV9 1000169991-0131990001001311181                   APLU390201891       H2I071    91-013199000A56   0000016500B00005106            ADMIRALENGRACHT      H5001DE2008200010THLIATHA191NAK             0000002000                          FD030000002000                                                  000000000000    H63902SV9 10001699B0000510691-013199000FD04 CONTACT NME,TELE REQD FDAFGM        FD05TEMNONE                                                                     H63902SV9 10001699B0000510691-013199000FDA PN MANDATORY FLD MISSING  FGG        H5002TH2008200010THLIATHA191NAK             0000003000                          FD030000004000                                                  000000000000    H63902SV9 10001699B0000510691-013199000FD04 CONTACT NME,TELE REQD FDAFGM        FD05TEMNONE                                                                     H63902SV9 10001699B0000510691-013199000FDA PN MANDATORY FLD MISSING  FGG        H63902SV9 10001699B0000510691-013199000TRANSACTION DATA REJECTED     524        Y  3902SV9HD00013";
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(2);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("CRL Message Status: CR transaction respose has arrived", ZDateTime.BrettsBirthday.AddDays(2), coll.CRLMessageStatusDate);
			AssertEquals("CargoReleaseError", 5, coll.CargoReleaseRecords.Count);

			message = CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(3);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("CRL is sent", ZDateTime.BrettsBirthday.AddDays(3), coll.CRLMessageStatusDate);

			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(4);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("CRL is responded", ZDateTime.BrettsBirthday.AddDays(4), coll.CRLMessageStatusDate);
			AssertEquals("CargoReleaseError", 0, coll.CargoReleaseRecords.Count);
		}

		public void TestCargoReleaseRecordsForACEPGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			message.EM_MessageText =
				@"B011101SV9SX                                               HYEDUSCMT_162367     " +
"SE10RSV9  71009656 01EI 13-14792700040800000010001101  1101                     " +
"SE15R    00104291327                                       00000010PC           " +
"SE20CR B00163345                                                                " +
"SE40001CH YUMMY TUNA                                                            " +
"SE6003034200200000001000                                                        " +
"OI        YUMMY TUNA                                                            " +
"PG01001NMF370YFTYY                                                              " +
"SE9011P48   MISSING PG22 RECORD PER PGA                                         " +
"SE9013PH6   MISSING DIS DOCUMENTATION                                           " +
"PG02P                                                                           " +
"SE9001   SE DATA REJECTED                                                       " +
"Y  1101SV9SX00015";

			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			entry.Messages.Add(message);
			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			var cargoReleaseRecords = coll.CargoReleaseRecords;
			AssertEquals("CargoReleaseError", 3, cargoReleaseRecords.Count);

			var record1 = cargoReleaseRecords[0];
			AssertEquals("LineNumber", "1", record1.LineNumber);
			AssertEquals("LineNumber", "0303420020", record1.TariffNumber);
			AssertEquals("PGAAgencyCode", "NMF", record1.PGAAgencyCode);
			AssertEquals("PGALine", "1", record1.PGALine);
			AssertEquals("error/status code", "P48", record1.ErrorMessageIdentifier);
			AssertEquals("NarrativeMessage", "MISSING PG22 RECORD PER PGA", record1.NarrativeMessage);

			var record2 = cargoReleaseRecords[1];
			AssertEquals("LineNumber", "1", record2.LineNumber);
			AssertEquals("LineNumber", "0303420020", record2.TariffNumber);
			AssertEquals("PGAAgencyCode", "NMF", record2.PGAAgencyCode);
			AssertEquals("PGALine", "1", record2.PGALine);
		}

		public void TestCargoReleaseRecordsForMultiplePGAlines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			message.EM_MessageText =
				"B011101SV9SX                                               HYEDUSCMT_162367     " +
				"SE10RSV9  71009656 01EI 13-14792700040800000010001101  1101                     " +
				"SE15R    00104291327                                       00000010PC           " +
				"SE20CR B00163345                                                                " +
				"SE40001CH YUMMY TUNA                                                            " +
				"SE6003034200200000001000                                                        " +
				"OI        YUMMY TUNA                                                            " +
				"PG01001NMF370YFTYY                                                              " +
				"SE9011P48   MISSING PG22 RECORD PER PGA                                         " +
				"SE9013PH6   MISSING DIS DOCUMENTATION                                           " +
				"PG02P                                                                           " +
				"PG01002NMF370YFTYY                                                              " +
				"SE9011P47   WHATEVER                                                            " +
				"PG02P                                                                           " +
				"SE9001   SE DATA REJECTED                                                       " +
				"SE40002CH YUMMY TUNA                                                            " +
				"SE6004034200200000001000                                                        " +
				"OI        YUMMY TUNA                                                            " +
				"PG01001EPA370YFTYY                                                              " +
				"SE9011P46   WHATEVER2                                                           " +
				"PG02P                                                                           " +
				"SE9001   SE DATA REJECTED                                                       " +
				"Y  1101SV9SX00015";
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			entry.Messages.Add(message);
			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			var cargoReleaseRecords = coll.CargoReleaseRecords;
			AssertEquals("CargoReleaseError", 6, cargoReleaseRecords.Count);

			var lineNo1s = cargoReleaseRecords.Cast<ErrorsRecord>().Where(x => x.LineNumber == "1");
			AssertEquals("PGA 0001", 3, lineNo1s.Count());
			AssertEquals(3, lineNo1s.Count(x => x.PGAAgencyCode == "NMF"));
			var line1 = lineNo1s.FirstOrDefault();
			AssertEquals("PGA 0001", "0303420020", line1.TariffNumber.ToString());

			var lineNo2s = cargoReleaseRecords.Cast<ErrorsRecord>().Where(x => x.LineNumber == "2");
			AssertEquals("PGA 0002", 1, lineNo2s.Count());
			AssertEquals(1, lineNo2s.Count(x => x.PGAAgencyCode == "EPA"));
			var line2 = lineNo2s.FirstOrDefault();
			AssertEquals("PGA 0002", "0403420020", line2.TariffNumber.ToString());
		}

		public void TestCargoReleaseDetailsForACECargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.CargoRelease);
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_145823     SE10ASV9  71001323 01EI 23-45678901240800000100001101                           SE15R    ALP31222402                                       00000100CS           SE20CR B00159868                                                                SE30MF ACE TEST SUPPLIER                                                        SE3515P O BOX 288                                                               SE36OSAKA                                       530-01         JP               SE30CN                                    EI 23-456789012                       SE30BY SIMPLIFIED ENTRY TEST IMPORTER                                           SE3515123 MAIN STREET                                                           SE36LOS ANGELES                                 60111          US               SE30SE ACE TEST SUPPLIER                                                        SE3515P O BOX 288                                                               SE36OSAKA                                       530-01         JP               SE40001AU                                                                       SE6039209950000000010000                                                        Y  1101SV9SE00015";

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entry.Messages.Add(message);
			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("CRL Message Status", ZDateTime.Today.AddDays(-10), coll.CRLMessageStatusDate);
			AssertEquals("CargoReleaseError", 0, coll.CargoReleaseRecords.Count);

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			message.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_145823     SE10ASV9  71001323 01EI 23-45678901240800000100001101                           SE15R    ALP31222402                                       00000100CS           SE20CR B00159868                                                                SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00015";
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			entry.Messages.Add(message);
			coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertEquals("CRL Message Status: SE response has arrived", ZDateTime.Today.AddDays(-9), coll.CRLMessageStatusDate);
			AssertEquals("CargoReleaseError", 1, coll.CargoReleaseRecords.Count);
			AssertEquals("CRLErrorsExist", ZString.Empty, coll.CRLErrorsExist);
		}

		public void TestACECargoRelReferenceDataForDeclaration()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = Entry;
			var processingMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse);
			processingMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			Declaration.Messages.Add(processingMessage);
			processingMessage.EM_MessageText =
				"B018888SV9C1                                                                    " +
				"WO103901SV9  70000045 01390123456789APLUVESSEL 110          001T 1203142        " +
				"WO20RSN09                                                                       " +
				"WO40HAPLUHOUSE1                                            00000010             " +
				"WO50120314153157SPLIT BILL DOES NOT QUALIFY FOR RELEASE YAPLU001T 1201142704    " +
				"Y018888SV9CQ00003";

			AssertEquals(1, EntryStatusesAndErrors.ACECargoRelReferenceData.Count);
			AssertEquals("RSN", EntryStatusesAndErrors.ACECargoRelReferenceData[0].ErrorMessageIdentifier);
			AssertEquals("09 Original entry is on hold. Hold must be resolved prior to correction request", EntryStatusesAndErrors.ACECargoRelReferenceData[0].NarrativeMessage);
		}

		public void TestACECargoRelReferenceDataForDeclarationCargoReleaseStatus()
		{
			declaration = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Declaration.US_EntryFilerCode = "SV9";
			var header = Declaration.Invoices.AddNew();
			header.InvoiceLines.AddNew();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(entry);
			Declaration.ImportEntryNumber = "70000045";

			var processingMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			processingMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			Declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(processingMessage);
			processingMessage.EM_MessageText =
				"B018888SV9SO                                                                    " +
				"SO103901SV9  70000045 01390123456789APLUVESSEL 110          001T 1203142        " +
				"SO20RSN09Original entry is on hold. Hold must be resolved prior to correction   " +
				"SO40HAPLUHOUSE1                                            00000010             " +
				"SO50120314153157SPLIT BILL DOES NOT QUALIFY FOR RELEASE YAPLU001T 1201142704    " +
				"Y018888SV9CQ00003";

			AssertEquals(1, EntryStatusesAndErrors.ACECargoRelReferenceData.Count);
			AssertEquals("RSN", EntryStatusesAndErrors.ACECargoRelReferenceData[0].ErrorMessageIdentifier);
			AssertEquals("09ORIGINAL ENTRY IS ON HOLD. HOLD MUST BE RESOLVED", EntryStatusesAndErrors.ACECargoRelReferenceData[0].NarrativeMessage);
		}

		public void TestACECargoRelReferenceDataForMultipleCBPComments()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Declaration.US_EntryFilerCode = "819";
			var header = Declaration.Invoices.AddNew();
			header.InvoiceLines.AddNew();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Delete();
			AssertEquals(true, entry.IsDeleted);
			Declaration.ImportEntryNumber = "00184185";

			var processingMessage1 = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			processingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(processingMessage1);
			processingMessage1.EM_MessageText =
				"B001101SV9SO                                                                    " +
				"SO101101819  00184185 0158-123456789                                   1        " +
				"SO20CR B00161827                                                                " +
				"SO20CMTSOMETHING HERE IN SAMPLE COMMENT                                         " +
				"SO20CMTSOMETHINGELS HERE IN SAMPLE COMMENT                                      " +
				"SO40R    00112312333                                       00000000     00000000" +
				"SO60043014144623ENTRY CANCELLED                                                 " +
				"Y  1101SV9SO00000";

			var processingMessage2 = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			processingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-6);
			declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(processingMessage2);
			processingMessage2.EM_MessageText =
				"B001101SV9SO                                                                    " +
				"SO101101819  00184185 0158-123456789                                   1        " +
				"SO20CR B00161827                                                                " +
				"SO20CMTSOMETHING HERE IN DIMPLE SOMMENT                                         " +
				"SO20CMTSOMETHINGELS HERE IN DIMPLE SOMMENT                                      " +
				"SO40R    00112312333                                       00000000     00000000" +
				"SO60043014144623ENTRY CANCELLED                                                 " +
				"Y  1101SV9SO00000";

			AssertEquals(3, EntryStatusesAndErrors.ACECargoRelReferenceData.Count);
			var errorRecords = EntryStatusesAndErrors.ACECargoRelReferenceData.Cast<ErrorsRecord>();

			AssertEquals(true, errorRecords.Any(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.CR && x.NarrativeMessage == "B00161827" && (x.StatusDate.ToLongTimeString().Length == ZDateTime.Today.AddDays(-5).ToLongTimeString().Length)));
			AssertEquals(1, errorRecords.Count(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.CR && x.NarrativeMessage == "B00161827"));
			AssertEquals(true, errorRecords.Any(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.CMT && x.NarrativeMessage.Contains("SOMETHING HERE IN SAMPLE COMMENT") && (x.StatusDate.ToLongTimeString().Length == ZDateTime.Today.AddDays(-5).ToLongTimeString().Length)));
			AssertEquals(true, errorRecords.Any(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.CMT && x.NarrativeMessage.Contains("SOMETHINGELS HERE IN SAMPLE COMMENT") && (x.StatusDate.ToLongTimeString().Length == ZDateTime.Today.AddDays(-5).ToLongTimeString().Length)));
			AssertEquals(true, errorRecords.Any(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.CMT && x.NarrativeMessage.Contains("SOMETHING HERE IN DIMPLE SOMMENT") && (x.StatusDate.ToLongTimeString().Length == ZDateTime.Today.AddDays(-6).ToLongTimeString().Length)));
			AssertEquals(true, errorRecords.Any(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.CMT && x.NarrativeMessage.Contains("SOMETHINGELS HERE IN DIMPLE SOMMENT") && (x.StatusDate.ToLongTimeString().Length == ZDateTime.Today.AddDays(-6).ToLongTimeString().Length)));
		}

		public void TestCargoReleaseStatusDateWhenEntrySummaryNotCertifying()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			//Not certifying
			message.EM_MessageText = "B  3902SV9AE                                               6008834              10ASV9  10001699 3902B00005106   0110 X           2020211                       1191-01319900091-013199000                     013111       IL                  20APLU3902013111I071ADMIRALENGRACHT                                             2156                                                                            2200000001PK                                                                    23MAPLU792479                                                                   318B 891                                                                        40  001 DEAU013111        0000000006602670000000150    N                        44SNOWMOBILE                                                                    47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 502008200010 0000000053 0000002000 000000015000KG                               OI        SNOWMOBILE                                                            FD0100121UAB07   DESLNSMITH                    THLIATHA191NAK THLIATHA191NAK    FD020000021000KG                                                                FD030000002000                                                                  FD05ADA01312011                                                                 FD05APA3902                                                                     FD05CSHAU                                                                       FD05FMEH                                                                        FD05OFTI                                                                        FD05PFTG                                                                        FD05SA187432 MAIN ROAD                                                          FD05SACCHICAGO                                                                  FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNMR IMPORTER                                                              FD05SCZ60125                                                                    FD05SEMNONE                                                                     FD05SFNJOHN                                                                     FD05SFTI                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT56                                                                       FD05BOLAPLU792479                                                               FD05TEMNONE                                                                     6250100000250                                                                   6249900000420                                                                   40  002 THAU013111        0000000012602670000000210    N                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 509802004040 0000000000 0000001000                                              502008200010 0000000054 0000003000 000000021000KG                               OI        PINEAPPLE                                                             FD0100121UAB07   DESLNSMITH                    THLIATHA191NAK THLIATHA191NAK    FD020000021000KG                                                                FD030000004000                                                                  FD05ADA01312011                                                                 FD05APA3902                                                                     FD05CSHAU                                                                       FD05FMEH                                                                        FD05OFTI                                                                        FD05PFTG                                                                        FD05SA187432 MAIN ROAD                                                          FD05SACCHICAGO                                                                  FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNMR IMPORTER                                                              FD05SCZ60125                                                                    FD05SEMNONE                                                                     FD05SFNJOHN                                                                     FD05SFTI                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT56                                                                       FD05BOLAPLU792479                                                               FD05TEMNONE                                                                     6250100000500                                                                   40  003 THAU013111        0000000032602670000002500    N                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 509802004040 0000000000 0000010000                                              508537109070 0000001350 0000000500 000000001200NO                               OA  FD0                                                                         6250100001313                                                                   895010000000206349900000002500                                                  9000000001457 00000004563 00000000000 00000000000 00000000000                   Y  3902SV9AE";
			message.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			var coll = new DeclarationEntriesStatusesAndErrorCollections(declaration);
			AssertNoExceptionThrown(delegate
			{
				var accessed = coll.CRLMessageStatusDate;
			});
		}

		public void TestBillDispositionsForMultipleBills()
		{
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			Declaration.ImportEntryNumber = "71002057";
			Declaration.US_EntryFilerCode = "SV9";
			Declaration.JE_MasterBill = "00591325209";
			Declaration.JE_HouseBill = "HAWB001";
			var houseBill2 = Declaration.Bills.AddNew();
			houseBill2.CU_BillNum = "HAWB002";

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageText = @"B001101SV9SO                                                                    " +
"SO103901SV9  71002057 0123-456789012CO                      2006 0909131        " +
"SO20CR B00160856                                                                " +
"SO40M    00591325209                                                            " +
"SO40H    HAWB001                                           00000100     00000250" +
"SO50102413011594BILL DEPARTED                           YCOA 2009 0909131101    " +
"SO40M    00591325209                                                            " +
"SO40H    HAWB001                                           00000150     00000250" +
"SO50102413011753CBP HOLD                                YCOA 4009 0909131101    " +
"SO40M    00591325209                                                            " +
"SO40H    HAWB002                                           00000710     00000490" +
"SO50102413011553CBP HOLD                                YCOA 4009 0909131118    " +
"Y  1101SV9SO00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Declaration.DispositionCodes.Load();
			AssertEquals("Bill Dispositions", 2, Declaration.PrimaryHouseBill.DispositionCodes.Count);
			AssertEquals("Bill Dispositions", 1, houseBill2.DispositionCodes.Count);
		}

		public void TestACECargoRelReferenceData()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var processingMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse);
			processingMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			Entry.Messages.Add(processingMessage);
			processingMessage.EM_MessageText =
				"B018888SV9C1                                                                    " +
				"WO103901SV9  70000045 01390123456789APLUVESSEL 110          001T 1203142        " +
				"WO20RSN09Original entry is on hold. Hold must be resolved prior to correction   " +
				"WO40HAPLUHOUSE1                                            00000010             " +
				"WO50120314153157SPLIT BILL DOES NOT QUALIFY FOR RELEASE YAPLU001T 1201142704    " +
				"Y018888SV9CQ00003";

			AssertEquals(1, EntryStatusesAndErrors.ACECargoRelReferenceData.Count);
			AssertEquals("RSN", EntryStatusesAndErrors.ACECargoRelReferenceData[0].ErrorMessageIdentifier);
			AssertEquals("09ORIGINAL ENTRY IS ON HOLD. HOLD MUST BE RESOLVED", EntryStatusesAndErrors.ACECargoRelReferenceData[0].NarrativeMessage);

			processingMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse);
			processingMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			Entry.Messages.Add(processingMessage);
			processingMessage.EM_MessageText =
				"B018888SV9C1                                                                    " +
				"WO103901SV9  70000045 01390123456789APLUVESSEL 110          001T 1203142        " +
				"WO40HAPLUHOUSE1                                            00000010             " +
				"WO50120314153157SPLIT BILL DOES NOT QUALIFY FOR RELEASE YAPLU001T 1201142704    " +
				"Y018888SV9CQ00003";

			fEntryStatusesAndErrors = null;
			AssertEquals(1, EntryStatusesAndErrors.ACECargoRelReferenceData.Count);
			AssertEquals("RSN", EntryStatusesAndErrors.ACECargoRelReferenceData[0].ErrorMessageIdentifier);
			AssertEquals("09ORIGINAL ENTRY IS ON HOLD. HOLD MUST BE RESOLVED", EntryStatusesAndErrors.ACECargoRelReferenceData[0].NarrativeMessage);
		}

		public void TestFailDeserializeACECargoRelReferenceData_NoNotification()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var processingMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse);
			processingMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			Entry.Messages.Add(processingMessage);
			processingMessage.EM_MessageText =
				"B018888SV9C1                                                                    " +
				"WO  0408B8PSO                                                                   " +
				"WO002                                                                           " +
				"WO40HAPLUHOUSE1                                            00000010             " +
				"WO50120314153157SPLIT BILL DOES NOT QUALIFY FOR RELEASE YAPLU001T 1201142704    " +
				"Y018888SV9CQ00003";

			AssertEquals("Deserialization failed", 0, EntryStatusesAndErrors.ACECargoRelReferenceData.Count);
			AssertEquals("No error reported", 0, ErrorReporter.TotalErrorCount);
		}

		#endregion

		#region IT Tests
		public void TestITDepartureStatus()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			MQEDIMessage outMessage = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.InbondTransaction);
			outMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			entry.Messages.Add(outMessage);
			Factory.Save();

			AssertEquals("IT Departure Status", ImportMessageStatusList.Codes.AwaitingDepartureOriginal, EntryStatusesAndErrors.ITDepartureStatus);

			MQEDIMessage responseMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse);
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			responseMessage.EM_MessageText =
"B018888XJ5QT                                               27458                " +
"10A61045439100   APLU2704     0062500011-123456700 N                            " +
"9501109 INVALID BONDED CARRIER ID                                               " +
"9501270 TRANSACTION DATA  REJECTED                                              " +
"Y  8888XJ5QT00003";
			entry.Messages.Add(responseMessage);
			Factory.Save();

			AssertEquals("IT Departure Status", ImportMessageStatusList.Codes.ErrorDepartureOriginal, EntryStatusesAndErrors.ITDepartureStatus);
		}

		public void TestITArrivalStatus()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			MQEDIMessage outMessage = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability);
			outMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondArrival;
			entry.Messages.Add(outMessage);
			Factory.Save();

			AssertEquals("IT Arrival Status", ImportMessageStatusList.Codes.AwaitingArrival, EntryStatusesAndErrors.ITArrivalStatus);

			MQEDIMessage responseMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse);
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondArrival;
			responseMessage.EM_MessageText =
"B018888XJ5WT                                               30291                " +
"101045439122                                                                    " +
"20                                                                              " +
"9501118 INBOND ARR DT INV-YYMMDD                                                " +
"9501144 ARRIVAL TIME IS INVALID                                                 " +
"9501119 INBOND PORT REQUIRED                                                    " +
"9501270 TRANSACTION DATA  REJECTED                                              " +
"Y  8888XJ5WT00006";
			entry.Messages.Add(responseMessage);
			Factory.Save();

			AssertEquals("IT Arrival Status", ImportMessageStatusList.Codes.ErrorArrival, EntryStatusesAndErrors.ITArrivalStatus);
		}

		public void TestITExportStatus()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			MQEDIMessage outMessage = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability);
			outMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondExportation;
			entry.Messages.Add(outMessage);
			Factory.Save();

			AssertEquals("IT Export Status", ImportMessageStatusList.Codes.AwaitingExportation, EntryStatusesAndErrors.ITExportStatus);

			MQEDIMessage responseMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse);
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondExportation;
			responseMessage.EM_MessageText =
"B018888XJ5WT                                               30292                " +
"105045439122                                                                    " +
"20                                                                              " +
"9501118 INBOND ARR DT INV-YYMMDD                                                " +
"9501144 ARRIVAL TIME IS INVALID                                                 " +
"9501119 INBOND PORT REQUIRED                                                    " +
"9501270 TRANSACTION DATA  REJECTED                                              " +
"Y  8888XJ5WT00006";
			entry.Messages.Add(responseMessage);
			Factory.Save();

			AssertEquals("IT Export Status", ImportMessageStatusList.Codes.ErrorExportation, EntryStatusesAndErrors.ITExportStatus);
		}

		public void TestITTOLStatus()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			MQEDIMessage outMessage = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability);
			outMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondTransferOfLiability;
			entry.Messages.Add(outMessage);
			Factory.Save();

			AssertEquals("IT TOL Status", ImportMessageStatusList.Codes.AwaitingTransferOfLiability, EntryStatusesAndErrors.ITTOLStatus);

			MQEDIMessage responseMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse);
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondTransferOfLiability;
			responseMessage.EM_MessageText =
"B018888XJ5WT                                               30278                " +
"10A045439122                                                                    " +
"20                                                                              " +
"9501118 INBOND ARR DT INV-YYMMDD                                                " +
"9501144 ARRIVAL TIME IS INVALID                                                 " +
"9501108 BONDED CARRIER ID REQUIRED                                              " +
"9501192 MISSING CITY NAME                                                       " +
"9501193 MISSING OR INVALID STATE OR PROVINCE                                    " +
"9501270 TRANSACTION DATA  REJECTED                                              " +
"Y  8888XJ5WT00008";
			entry.Messages.Add(responseMessage);
			Factory.Save();

			AssertEquals("IT TOL Status", ImportMessageStatusList.Codes.ErrorTransferOfLiability, EntryStatusesAndErrors.ITTOLStatus);
		}

		public void TestITErrorsExist()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse);
			entry.Messages.Add(message);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			message.EM_MessageText =
"B018888XJ5QT                                               27458                " +
"10A61045439100   APLU2704     0062500011-123456700 N                            " +
"9501109 INVALID BONDED CARRIER ID                                               " +
"9501270 TRANSACTION DATA  REJECTED                                              " +
"Y  8888XJ5QT00003";

			Factory.Save();
			AssertEquals("IT Errors Exist - true", "IT Errors Exist", Declaration.ITErrorsExist);
		}

		public void TestITTOLNarrative()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondTransferOfLiability;
			entry.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5WT                                               28934                " +
"101045439041                                                                    " +
"200810171938001001                                                              " +
"9502271 DATA ADDED AS REQUESTED                                                 " +
"Y  8888XJ5WT00003";

			Factory.Save();
			AssertEquals("IT Errors Exist - false", "", Declaration.ITErrorsExist);
			AssertEquals("TOL Records", 1, EntryStatusesAndErrors.ITWT95TOLRecords.Count);
			AssertEquals("TOL Narrative", "DATA ADDED AS REQUESTED", EntryStatusesAndErrors.ITWT95TOLRecords[0].NarrativeMessage);
		}

		public void TestITErrorsExist2_TOL()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse);
			entry.Messages.Add(message);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondTransferOfLiability;
			message.EM_MessageText =
"B018888XJ5WT                                               27458                " +
"10A61045439100   APLU2704     0062500011-123456700 N                            " +
"9501109 INVALID BONDED CARRIER ID                                               " +
"9501270 TRANSACTION DATA  REJECTED                                              " +
"Y  8888XJ5WT00003";

			Factory.Save();
			AssertEquals("IT Errors Exist (TOL) - true", "IT Errors Exist", Declaration.ITErrorsExist);

			AssertEquals("TOL Records", 2, Declaration.ITWT95TOLRecords.Count);
			AssertEquals("TOL Narrative", "INVALID BONDED CARRIER ID", EntryStatusesAndErrors.ITWT95TOLRecords[0].NarrativeMessage);
			AssertEquals("TOL Error Message Identifier", "109", EntryStatusesAndErrors.ITWT95TOLRecords[0].ErrorMessageIdentifier);
			AssertEquals("TOL Status Date", message.EM_SystemCreateTimeUtc, EntryStatusesAndErrors.ITWT95TOLRecords[0].StatusDate);

			AssertEquals("TOL Narrative", "TRANSACTION DATA  REJECTED", EntryStatusesAndErrors.ITWT95TOLRecords[1].NarrativeMessage);
			AssertEquals("TOL Error Message Identifier", "270", EntryStatusesAndErrors.ITWT95TOLRecords[1].ErrorMessageIdentifier);
			AssertEquals("TOL Status Date", message.EM_SystemCreateTimeUtc, EntryStatusesAndErrors.ITWT95TOLRecords[1].StatusDate);
		}

		#endregion

		#region Dispositions Tests

		public void TestLoadBillErrorsRecord_NarrativeMessage()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1 DESC", startDate, endDate);

			var codeType1 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode, "SO50RecordDispCode", dataGrouping.ZZZ_DataGrouping);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "95", "Bill Arrived", startDate, endDate);
			Factory.Save();

			var bill = Declaration.Bills.AddNew();
			var disposition1 = bill.DispositionCodes.AddNewIfNotExist("95", ZDateTime.Today, BillDispositionSourceList.Codes.SO);
			var disposition2 = bill.DispositionCodes.AddNewIfNotExist("Z1", ZDateTime.Today.AddDays(1), BillDispositionSourceList.Codes.CQ);

			var entryStatusesAndErrors = Declaration.EntryStatusesAndErrors;
			var errorsRecords = entryStatusesAndErrors.LoadBillErrorsRecord(bill);

			AssertEquals(2, errorsRecords.Count());
			AssertEquals("Bill Arrived", errorsRecords.First(x => x.StatusDate == ZDateTime.Today).NarrativeMessage);
			AssertEquals("Z1 DESC", errorsRecords.First(x => x.StatusDate == ZDateTime.Today.AddDays(1)).NarrativeMessage);
		}

		public void TestUpdateOGADispositionCodesFromMessage()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);
			Declaration.Messages.Add(message);
			message.EM_MessageText =
"B011102113RR                                                            27444284" +
"R14701113 457149080111-35250130025143832 CMDUXIN DAN DONG        492  110708    " +
"R4            SZ1459194                           00000216CTNS CMDU             " +
"R5110308000805PAPERLESS                                                         " +
"R6FDA    110308000806FDA MAY PROCEED                                            " +
"R6FDA    110308000806FDA MAY PROCEED                  070011001THRU0011001      " +
"R6FDA    110308000806FDA MAY PROCEED                  070021001THRU0031001      " +
"Y  1102113LS00002";
			Factory.Save();

			AssertEquals("OGA/PGA Dispositions Exist", Declaration.OGADispositionsExist);
			AssertEquals("Should be 3 OGA Dispositions", 3, Declaration.OGADispositionCodes.Count);
			AssertEquals(new ZDateTime(2008, 11, 3, 0, 08, 00), Declaration.OGADispositionCodes[1].US_DispositionDate);
			AssertEquals("06", Declaration.OGADispositionCodes[1].US_OGADispositionStatusCode);
			AssertEquals("FDA MAY PROCEED", Declaration.OGADispositionCodes[1].US_OGADispositionStatusMessage);
			AssertEquals("07", Declaration.OGADispositionCodes[1].US_Code);
			AssertEquals("1", Declaration.OGADispositionCodes[1].US_OGADispositionBeginningCBPLine);
			AssertEquals("1", Declaration.OGADispositionCodes[1].US_OGADispositionBeginningOGALine);
			AssertEquals("THRU", Declaration.OGADispositionCodes[1].US_OGADispositionRangeIndicator);
			AssertEquals("1", Declaration.OGADispositionCodes[1].US_OGADispositionEndCBPLine);
			AssertEquals("1", Declaration.OGADispositionCodes[1].US_OGADispositionEndOGALine);
			AssertEquals(ZString.Empty, Declaration.OGADispositionCodes[1].US_Source);

			AssertEquals("07", Declaration.OGADispositionCodes[2].US_Code);
			AssertEquals("2", Declaration.OGADispositionCodes[2].US_OGADispositionBeginningCBPLine);
			AssertEquals("3", Declaration.OGADispositionCodes[2].US_OGADispositionEndCBPLine);
			AssertEquals(ZString.Empty, Declaration.OGADispositionCodes[2].US_Source);
		}

		public void TestUpdateOGADispositionDetailRecordFromMessage()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);
			Declaration.Messages.Add(message);
			message.EM_MessageText =
"B003901SV9SO                                                                    " +
"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
"SO20CR B00160703                                                                " +
"SO40R    00591325206                                       00000150BL   00000000" +
"SO60093013012597ADMISSIBLE                                                      " +
"SO70ACE000123013143913EPA DATA REVIEW             010411222A365THRU3334444100   " +
"SO711 3333333333  0616150910     102103                                         " +
"SO712 4444444444  0316150910     110111                                         " +
"SO72COMMENT3.                                                                   " +
"SO72COMMENT4.                                                                   " +
"Y  3901SV9SO00000                                                               ";
			Factory.Save();

			AssertEquals(1, Declaration.OGADispositionCodes.Count);
			AssertEquals(new ZDateTime(2013, 12, 30, 14, 39, 0), Declaration.OGADispositionCodes[0].US_DispositionDate);
			var disposition1 = Declaration.OGADispositionCodes[0];
			AssertEquals("04", disposition1.US_Code);
			AssertEquals("222", disposition1.US_OGADispositionBeginningCBPLine);
			AssertEquals("", disposition1.US_BeginningTariffPosition);
			AssertEquals("", disposition1.US_EndingTariffPosition);
			AssertEquals("102", disposition1.OGADispositionDetails[0].US_LineSubReasonCode1);
			AssertEquals("103", disposition1.OGADispositionDetails[0].US_LineSubReasonCode2);
			AssertEquals("1", disposition1.OGADispositionDetails[0].US_ReferenceIDQualifier);
			AssertEquals("3333333333", disposition1.OGADispositionDetails[0].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), disposition1.OGADispositionDetails[0].US_ReceiptDateTime);

			AssertEquals("110", disposition1.OGADispositionDetails[1].US_LineSubReasonCode1);
			AssertEquals("111", disposition1.OGADispositionDetails[1].US_LineSubReasonCode2);
			AssertEquals("2", disposition1.OGADispositionDetails[1].US_ReferenceIDQualifier);
			AssertEquals("4444444444", disposition1.OGADispositionDetails[1].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition1.OGADispositionDetails[1].US_ReceiptDateTime);

			AssertEquals("COMMENT3. COMMENT4.", disposition1.US_Comment);
		}

		public void TestUpdateOGADispositionDetailRecordFromMessage_Version01()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);
			Declaration.Messages.Add(message);
			message.EM_MessageText =
"B003901SV9SO                                                                    " +
"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
"SO20CR B00160703                                                                " +
"SO40R    00591325206                                       00000150BL   00000000" +
"SO60093013012597ADMISSIBLE                                                      " +
"SO70ACE000123013143913EPA DATA REVIEW             01041122224365THRU35554441AB01" +
"SO711 3333333333  0616150910     102103                                         " +
"SO712 4444444444  0316150910     110111                                         " +
"SO72COMMENT3.                                                                   " +
"SO72COMMENT4.                                                                   " +
"Y  3901SV9SO00000                                                               ";
			Factory.Save();

			AssertEquals(1, Declaration.OGADispositionCodes.Count);
			AssertEquals(new ZDateTime(2013, 12, 30, 14, 39, 0), Declaration.OGADispositionCodes[0].US_DispositionDate);
			var disposition1 = Declaration.OGADispositionCodes[0];
			AssertEquals("04", disposition1.US_Code);
			AssertEquals("2222", disposition1.US_OGADispositionBeginningCBPLine);
			AssertEquals("4", disposition1.US_BeginningTariffPosition);
			AssertEquals("3", disposition1.US_EndingTariffPosition);
			AssertEquals("102", disposition1.OGADispositionDetails[0].US_LineSubReasonCode1);
			AssertEquals("103", disposition1.OGADispositionDetails[0].US_LineSubReasonCode2);
			AssertEquals("1", disposition1.OGADispositionDetails[0].US_ReferenceIDQualifier);
			AssertEquals("3333333333", disposition1.OGADispositionDetails[0].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), disposition1.OGADispositionDetails[0].US_ReceiptDateTime);

			AssertEquals("110", disposition1.OGADispositionDetails[1].US_LineSubReasonCode1);
			AssertEquals("111", disposition1.OGADispositionDetails[1].US_LineSubReasonCode2);
			AssertEquals("2", disposition1.OGADispositionDetails[1].US_ReferenceIDQualifier);
			AssertEquals("4444444444", disposition1.OGADispositionDetails[1].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition1.OGADispositionDetails[1].US_ReceiptDateTime);

			AssertEquals("COMMENT3. COMMENT4.", disposition1.US_Comment);
		}

		public void TestUpdateOGADispositionDetailRecordFromMessage_NoSO70Block()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);
			Declaration.Messages.Add(message);
			message.EM_MessageText =
"B003901SV9SO                                                                    " +
"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
"SO20CR B00160703                                                                " +
"SO40R    00591325206                                       00000150BL   00000000" +
"SO60093013012597ADMISSIBLE                                                      " +
"SO711 3333333333  061615  0910  102103                                          " +
"SO712 4444444444  031615  0910  110111                                          " +
"SO72COMMENT3.                                                                   " +
"SO72COMMENT4.                                                                   " +
"Y  3901SV9SO00000                                                               ";
			Factory.Save();

			AssertEquals(0, Declaration.OGADispositionCodes.Count);
		}

		public void TestUpdateDispositionCodesFromMessage()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults);
			Declaration.Messages.Add(message);
			message.EM_MessageText =
"B011102113RR                                                            27444284" +
"R14701113 457149080111-35250130025143832 CMDUXIN DAN DONG        492  110708    " +
"R4            SZ1459194                           00000216CTNS CMDU             " +
"R5110308000805PAPERLESS                               06130905                  " +
"Y  1102113LS00002";
			Factory.Save();

			AssertEquals("Dispositions Exist", 1, Declaration.DispositionCodes.Count);
			DispositionData data = Declaration.DispositionCodes[0];
			AssertEquals(new ZDateTime(2008, 11, 3, 0, 08, 00), data.US_DispositionDate);
			AssertEquals("05", data.US_Code);
			AssertEquals((ZShort)1, data.US_Order);
			AssertEquals(new ZDateTime(2009, 6, 13, 0, 00, 00), data.US_ReleaseDate);
			AssertEquals("05", data.US_ReleaseOrigin);
		}

		public void TestAddOGADispositionDataAndCalculateDeclarationFDAStatus()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var block = new CMQR6();
			block.Deserialise("R6FDA    110308000806FDA MAY PROCEED                  070021001THRU0031001      ");
			Declaration.EntryStatusesAndErrors.AddOGADispositionDataAndCalculateDeclarationFDAStatus(block);

			AssertEquals("OGA/PGA Dispositions Exist", Declaration.OGADispositionsExist);
			AssertEquals("Should be 1 OGA Disposition", 1, Declaration.OGADispositionCodes.Count);
			AssertEquals("06", Declaration.FDAStatus);
			AssertEquals(FDAStatusList.Codes.ACC, Declaration.FDAMsgStatus);

			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableSPN = true;
			block = new CMQR6();
			block.Deserialise("R6FDA    110308000806FDA MAY PROCEED                  070021001THRU0031001      ");
			Declaration.EntryStatusesAndErrors.AddOGADispositionDataAndCalculateDeclarationFDAStatus(block);

			AssertEquals("OGA/PGA Dispositions Exist", Declaration.OGADispositionsExist);
			AssertEquals("Should be 1 OGA Disposition", 1, Declaration.OGADispositionCodes.Count);
			AssertEquals(ZString.Empty, Declaration.FDAStatus);
			AssertEquals(ZString.Empty, Declaration.FDAMsgStatus);
		}

		#endregion

		#region BLU Tests

		public void TestBLUL7RecordsWithENSAndCRLEntriesAndBLUDoneOnCRL()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			ensEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));
			Factory.Save();

			ensEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse));
			Factory.Save();

			CusEntryHeader crlEntry = Declaration.ActiveEntryHeaders.AddNew();
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			crlEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			Factory.Save();

			crlEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse));
			Factory.Save();

			crlEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BillofLadingUpdate));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse);
			crlEntry.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5LS                                               33103                " +
"L78888XJ5 700188478VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700188478VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";
			Factory.Save();

			AssertEquals(2, Declaration.BLUL7Records.Count);
			AssertEquals("8VZ", Declaration.BLUL7Records[0].ErrorMessageIdentifier);
			AssertEquals("BILL DATA UPDATED AS REQUESTED", Declaration.BLUL7Records[0].NarrativeMessage);
			AssertEquals("8VX", Declaration.BLUL7Records[1].ErrorMessageIdentifier);
			AssertEquals("BILL RESULTS REQUEST ACCEPTED", Declaration.BLUL7Records[1].NarrativeMessage);
		}

		public void TestBLUL7RecordsWhenOnDec()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			ensEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.EntrySummary));
			Factory.Save();

			ensEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse));
			Factory.Save();

			Declaration.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BillofLadingUpdate));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse);
			Declaration.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5LS                                               33103                " +
"L78888XJ5 700188478VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700188478VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";
			Factory.Save();

			AssertEquals(2, Declaration.BLUL7Records.Count);
			AssertEquals("8VZ", Declaration.BLUL7Records[0].ErrorMessageIdentifier);
			AssertEquals("BILL DATA UPDATED AS REQUESTED", Declaration.BLUL7Records[0].NarrativeMessage);
			AssertEquals("8VX", Declaration.BLUL7Records[1].ErrorMessageIdentifier);
			AssertEquals("BILL RESULTS REQUEST ACCEPTED", Declaration.BLUL7Records[1].NarrativeMessage);
		}

		public void TestBLUMessageStatusDate()
		{
			//status date is only for BLU messages that are attached to Declarations. Old messages attached to entries will be ignored by design.
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var ensEntry = Declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			Declaration.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BillofLadingUpdate));
			Factory.Save();

			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse, 1);
			Declaration.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5LS                                               33103                " +
"L78888XJ5 700188478VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700188478VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";
			Factory.Save();

			AssertEquals("BLU Status Date", message.EM_SystemCreateTimeUtc.Date, EntryStatusesAndErrors.BLUMessageStatusDate.Date);

			message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.CargoRelease, 2);
			Declaration.Messages.Add(message);
			Factory.Save();
			fEntryStatusesAndErrors = null;
			AssertEquals("BLU Status Date", ZDateTime.Empty, EntryStatusesAndErrors.BLUMessageStatusDate.Date);

			message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.CargoRelease, 3);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			Declaration.Messages.Add(message);
			Factory.Save();
			fEntryStatusesAndErrors = null;
			AssertEquals("BLU Status Date", message.EM_SystemCreateTimeUtc.Date, EntryStatusesAndErrors.BLUMessageStatusDate.Date);

			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse, 4);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			Declaration.Messages.Add(message);
			Factory.Save();
			fEntryStatusesAndErrors = null;
			AssertEquals("BLU Status Date", message.EM_SystemCreateTimeUtc.Date, EntryStatusesAndErrors.BLUMessageStatusDate.Date);
		}

		public void TestBLUMessageStatusDateWhenIsProcessingResultsMsg()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			Declaration.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BillofLadingUpdate));
			Factory.Save();

			MQEDIMessage message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BillofLadingProcessingResults, 1);
			Declaration.Messages.Add(message);
			message.EM_MessageText =
"B018888XJ5PS                                               33103                " +
"L78888XJ5 700188478VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700188478VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";
			Factory.Save();

			AssertEquals("BUL Status Date from Response Results Msg.", message.EM_SystemCreateTimeUtc, EntryStatusesAndErrors.BLUMessageStatusDate);
		}

		public void TestBLUL7RecordsForSimplifiedEntryStatus()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EnableCRL = true;

			var masterBill = Declaration.Bills.CreatePrimaryBill(Customs.Business.BillTypeList.Codes.MasterBill);
			masterBill.CU_BillNum = "MB001";
			var disposition1 = masterBill.DispositionCodes.AddNewIfNotExist("91", new ZDateTime(2013, 03, 11, 14, 32, 0));
			var disposition2 = masterBill.DispositionCodes.AddNewIfNotExist("94", new ZDateTime(2013, 03, 11, 14, 58, 1));

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillNum = "House1";
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			var disposition3 = houseBill.DispositionCodes.AddNewIfNotExist("91", new ZDateTime(2013, 03, 11, 14, 32, 0));
			var disposition4 = houseBill.DispositionCodes.AddNewIfNotExist("93", new ZDateTime(2013, 03, 11, 14, 58, 1));
			Factory.Save();

			AssertEquals(4, Declaration.BLUL7Records.Count);
			var record = Declaration.BLUL7Records.OfType<ErrorsRecord>().FirstOrDefault(x => x.ActionIDNumber == "MB001" && x.ErrorMessageIdentifier == "91");
			AssertEquals(new ZDateTime(2013, 03, 11, 14, 32, 0), record.StatusDate);

			record = Declaration.BLUL7Records.OfType<ErrorsRecord>().FirstOrDefault(x => x.ActionIDNumber == "MB001" && x.ErrorMessageIdentifier == "94");
			AssertEquals(new ZDateTime(2013, 03, 11, 14, 58, 1), record.StatusDate);

			record = Declaration.BLUL7Records.OfType<ErrorsRecord>().FirstOrDefault(x => x.ActionIDNumber == "House1" && x.ErrorMessageIdentifier == "91");
			AssertEquals(new ZDateTime(2013, 03, 11, 14, 32, 0), record.StatusDate);

			record = Declaration.BLUL7Records.OfType<ErrorsRecord>().FirstOrDefault(x => x.ActionIDNumber == "House1" && x.ErrorMessageIdentifier == "93");
			AssertEquals(new ZDateTime(2013, 03, 11, 14, 58, 1), record.StatusDate);
		}

		#endregion

		public void TestBLUL7RecordsForDispositionNarrativeMessage()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EnableCRL = true;
			DispositionList dispositionlist = new DispositionList();

			var masterBill = Declaration.Bills.CreatePrimaryBill(Customs.Business.BillTypeList.Codes.MasterBill);
			masterBill.CU_BillNum = "MB001";
			var disposition = masterBill.DispositionCodes.AddNewIfNotExist("91", new ZDateTime(2013, 03, 11, 14, 32, 0), BillDispositionSourceList.Codes.IS);
			Factory.Save();

			var record = Declaration.BLUL7Records.OfType<ErrorsRecord>().FirstOrDefault(x => x.ActionIDNumber == "MB001" && x.ErrorMessageIdentifier == "91");
			AssertEquals(dispositionlist.GetDescriptionFromCode(record.ErrorMessageIdentifier), record.NarrativeMessage);
		}

		public void TestMarkElectronicInvoicingActionCompleted()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableAII = true;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_ApplicationReference = "1:123456789012";
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E111333   090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";

			invoice1.JZ_MessageStatus = ImportMessageStatusList.Codes.AwaitingElectronicInvoiceOriginal;
			Factory.Save();

			invoice1.JZ_MessageStatus = ImportMessageStatusList.Codes.ClearElectronicInvoiceOriginal;
			Factory.Save();

			AssertEquals("Still not actioned", "7501 Requires Actions", declaration.ENSStatusNotificationsReqFurtherActions);

			invoice2.JZ_MessageStatus = ImportMessageStatusList.Codes.AwaitingElectronicInvoiceOriginal;
			Factory.Save();

			AssertEquals("Still not actioned", "7501 Requires Actions", declaration.ENSStatusNotificationsReqFurtherActions);

			invoice2.JZ_MessageStatus = ImportMessageStatusList.Codes.ClearElectronicInvoiceOriginal;
			Factory.Save();

			AssertEquals("Actioned", "", declaration.ENSStatusNotificationsReqFurtherActions);
		}

		public void TestLatestNonPSCEntryData()
		{
			var declaration = GetMergibleDeclaration();

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			MergeAndSendDeclarationWithSuccessResponse(declaration, "~1");

			//This is the latest 7501 transmission that is accepted before PSC
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			MergeAndSendDeclarationWithSuccessResponse(declaration, "~2");

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, US.Messaging.Business.UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			entry.Messages.Add(message);
			message.EM_MessageNum = "~3";
			entry.Messages.Add(PSCEntrySummaryDataTest.CreateCustomsResponseFailed(Factory, "~3"));
			entry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
			Factory.Save();

			declaration.US_PSC = true;
			MergeAndSendDeclarationWithSuccessResponse(declaration, "~4");

			declaration.EntryStatusesAndErrors.RefreshPSCEntryDataForTesting();
			var latestNONPSCData = declaration.EntryStatusesAndErrors.LatestNonPSCEntryData;
			AssertEquals(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, latestNONPSCData.PaymentType);
			AssertEquals(ZString.Empty, latestNONPSCData.ClientBranchDesig);
			AssertEquals(ZDateTime.Empty, latestNONPSCData.PSD);
			AssertEquals(ZString.Empty, latestNONPSCData.PSCMonth);

			var sTUMessage = DeclarationTestHelper.CreateTransmittedSTUMsg(Factory, "26384", message.EM_SystemCreateTimeUtc.AddMinutes(16), "B011001267HP                                               26384                H4601267 0200744970817110209                                                    Y  1001267HP00001");
			declaration.Messages.Add(sTUMessage);

			var responseMessage = DeclarationTestHelper.CreateIncomingSTUMsg(Factory, "26384", sTUMessage.EM_SystemCreateTimeUtc.AddMinutes(4), "B011001267HT                                               26384                H14601267 020074492GBDATA REPLACED AS REQUESTED               708171110001320802Y  1001267HT00001");
			declaration.Messages.Add(responseMessage);

			declaration.EntryStatusesAndErrors.RefreshPSCEntryDataForTesting();
			latestNONPSCData = declaration.EntryStatusesAndErrors.LatestNonPSCEntryData;

			AssertEquals(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, latestNONPSCData.PaymentType);
			AssertEquals("02", latestNONPSCData.ClientBranchDesig);
			AssertEquals(new ZDateTime(2011, 08, 17), latestNONPSCData.PSD);
			AssertEquals("09", latestNONPSCData.PSCMonth);

			var sTUMessage2 = DeclarationTestHelper.CreateTransmittedSTUMsg(Factory, "26385", message.EM_SystemCreateTimeUtc.AddMinutes(21), "B011001267HP                                               26384                H4601267 0200744950817110209                                                    Y  1001267HP00001");
			declaration.Messages.Add(sTUMessage2);

			var responseMessage2 = DeclarationTestHelper.CreateIncomingSTUMsg(Factory, "26385", sTUMessage2.EM_SystemCreateTimeUtc.AddMinutes(5), "B011001267HT                                               26384                H14601267 020074492GBDATA REPLACED AS REQUESTED               508171110001320802Y  1001267HT00001");
			declaration.Messages.Add(responseMessage2);

			declaration.EntryStatusesAndErrors.RefreshPSCEntryDataForTesting();
			latestNONPSCData = declaration.EntryStatusesAndErrors.LatestNonPSCEntryData;

			AssertEquals(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes, latestNONPSCData.PaymentType);
			AssertEquals("02", latestNONPSCData.ClientBranchDesig);
			AssertEquals(new ZDateTime(2011, 08, 17), latestNONPSCData.PSD);
			AssertEquals("09", latestNONPSCData.PSCMonth);
		}

		public void TestOGADispositionData()
		{
			var orgImporter = Factory.NewWithValidTestData<OrgHeader>();
			orgImporter.OH_Code = "IMP3232!@";
			orgImporter.OH_FullName = "TEST Importer";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = orgImporter.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "71002057";

			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(message);
			message.EM_MessageText =
"B003901SV9SO                                                                    " +
"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
"SO20CR B00160703                                                                " +
"SO40R    00591325206                                       00000150BL   00000000" +
"SO60093013012597ADMISSIBLE                                                      " +
"SO70ACE123013143912EPA DATA REVIEW                         2  0123456789 07     " +
"Y  3901SV9SO00000                                                               ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			declaration.DispositionCodes.Load();
			var ogaDispositionCodes = declaration.OGADispositionCodes;
			AssertEquals(1, ogaDispositionCodes.Count);

			var dispositionCode = ogaDispositionCodes[0];
			AssertEquals("07", dispositionCode.US_DocumentType);
		}

		public void TestUpdateOGADispositionUSCommentWithSubstring()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Declaration.US_EntryFilerCode = "SV9";
			Declaration.US_EnableENS = true;

			Declaration.Invoices.AddNew();
			Declaration.InvoiceLines.AddNew();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			Declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(message);
			message.EM_MessageText = string.Format(
"B003901SV9SO                                                                    " +
"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
"SO20CR B00160703                                                                " +
"SO40R    00591325206                                       00000150BL   00000000" +
"SO60093013012597ADMISSIBLE                                                      " +
"SO70ACE000123013143913EPA DATA REVIEW             010411222 365THRU3334444100   " +
"SO711 3333333333  0616150910     102103                                         " +
"SO712 4444444444  0316150910     110111                                         " +
"SO72{0}" +
"SO72{1}" +
"SO72{2}" +
"Y  3901SV9SO00000                                                               ", "COMMENT3".PadRight(76, '0'), "COMMENT4".PadRight(76, '0'), "COMMENT5".PadRight(76, '0'));
			Declaration.ImportEntryNumber = "71002057";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Declaration.DispositionCodes.Load();

			AssertEquals("COMMENT3".PadRight(76, '0') + " " + "COMMENT4".PadRight(76, '0') + " " + "COMMENT5".PadRight(AutoUSOGADispositionDataAddInfo.Schema.US_CommentMaxLength - 154, '0'), Declaration.OGADispositionCodes[0].US_Comment);
			AssertEquals(AutoUSOGADispositionDataAddInfo.Schema.US_CommentMaxLength, Declaration.OGADispositionCodes[0].US_Comment.Length);
		}

		#region FTZ Tests

		public void TestUpdateFTZDispositionCodes()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			Declaration.Messages.Add(message);
			message.EM_MessageText =
"B003910SV9NF                                               HYEDUSCMT_118672     " +
"90A153000112000000702501N                                                       " +
"91B5 1  15300011200000070                  1205251330                           " +
"91BF 1  15300011200000070                                                       " +
"95     FTZ ADMISSION DOCS REQD                                                  " +
"Y  3910SV9NF00003";
			Factory.Save();

			AssertEquals("Dispositions Exist", 2, Declaration.FTZDispositionCodes.Count);
			var data = Declaration.FTZDispositionCodes[0];
			AssertEquals(new ZDateTime(2012, 05, 25, 13, 30, 00), data.US_DispositionDate);
			AssertEquals(DispositionList.Codes.B5, data.US_Code);
			AssertEquals((ZShort)1, data.US_Order);
			AssertEquals(ZDateTime.Empty, data.US_ReleaseDate);
			AssertEquals(ZString.Empty, data.US_ReleaseOrigin);

			data = Declaration.FTZDispositionCodes[1];
			AssertEquals(ZDateTime.Empty, data.US_DispositionDate);
			AssertEquals(DispositionList.Codes.BF, data.US_Code);
			AssertEquals((ZShort)2, data.US_Order);
			AssertEquals(ZDateTime.Empty, data.US_ReleaseDate);
			AssertEquals(ZString.Empty, data.US_ReleaseOrigin);
		}

		#endregion

		JobDeclaration GetMergibleDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			return declaration;
		}

		void MergeAndSendDeclarationWithSuccessResponse(JobDeclaration declaration, ZString messageNum)
		{
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, US.Messaging.Business.UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			entry.Messages.Add(message);
			entry.Messages.Add(PSCEntrySummaryDataTest.CreateCustomsResponseSuccess(Factory, messageNum));
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;

			Factory.Save();
			message.EM_MessageNum = messageNum;
			Factory.Save();
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					Declaration.US_EntryFilerCode = "XJ5";

					entry = Declaration.CustomsEntryHeaders.AddNew();
					entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				}
				return entry;
			}
		}
		CusEntryHeader entry;

		DeclarationEntriesStatusesAndErrorCollections EntryStatusesAndErrors
		{
			get { return fEntryStatusesAndErrors ?? (fEntryStatusesAndErrors = new DeclarationEntriesStatusesAndErrorCollections(Declaration)); }
		}
		DeclarationEntriesStatusesAndErrorCollections fEntryStatusesAndErrors;

		MQEDIMessage CreateMessage(string receiveTransmit, string applicationIdentifier, ZInt offTimeForTest = default)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var result = mock.Object;
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = receiveTransmit;
			result.EM_MessageType = applicationIdentifier;
			result.EM_MessageText = @"B018888XJ5CI                                               ~15000               Y  8888XJ5CI00054";
			result.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(offTimeForTest);

			return result;
		}

		#endregion
	}
}
