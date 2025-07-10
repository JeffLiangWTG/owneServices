using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(MQEDIMessage))]
	sealed class MQEDIMessageTest : EDIMessageTest
	{
		public void TestEM_MessageTextDetail()
		{
			var messageText = "B011101SV9SX                                               HYEDUSCMT_164692     SE10ASV9  71002057 01EI  123-45-678911800000100001101  1101                     SE11           A002         ***_**_****123W                                     Y  1101SV9SX00044                                                               ";
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDEntrySummary;
			message.EM_MessageText = messageText;
			AssertMultilineASCIIEquals(@"B011101SV9SX                                               HYEDUSCMT_164692     
SE10ASV9  71002057 01EI  123-45-678911800000100001101  1101                     
SE11           A002         ***_**_****123W                                     
Y  1101SV9SX00044", message.EM_MessageTextDetail);

			message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDEntrySummary;
			message.EM_MessageText = messageText;
			Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			var informationText = $"There is personal information in this Message Text and you are not authorized to see it based on security: {Env.Security.OrgDetailsViewPersonalInformation.DisplayTextPathToSecurityRight}.";
			AssertEquals(informationText, message.EM_MessageTextDetail);
		}

		[TestDate(2023, 03, 20, 13, 24, 00)]
		public void TestDispositionDateTime()
		{
			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			message.EM_MessageText = "B013910SV9NF                                               HYEDUSCMT_118672     90A153000112000000702501N                                                       91B2 1  15300011200000070                  2303151330                           20A40CR  CLIENT REP TEST AIR CARCR987          2011110320111104250120111104     4077739100036                        00000HAWB001        0000000100GB41380      950327T*CENSUS*INVALID AIR MOT                                                  9502083 ZONE ADMISSION DATA ACCEPTED                                            Y  3910SV9NF00004";
			AssertEquals("Get from message text", new ZDateTime(2023, 03, 15, 13, 30, 00), message.DispositionDateTime);
			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData);
			message.EM_MessageText = "B013910SV9NF                                               HYEDUSCMT_118672     90A153000112000000702501N                                                       91B2 1  15300011200000070                  2303151330                           20A40CR  CLIENT REP TEST AIR CARCR987          2011110320111104250120111104     4077739100036                        00000HAWB001        0000000100GB41380      950327T*CENSUS*INVALID AIR MOT                                                  9502083 ZONE ADMISSION DATA ACCEPTED                                            Y  3910SV9NF00004";
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Get from EM_SystemCreateTimeUtc", Env.Time.GetLocalTimeFromUtc(ZDateTime.UtcNow.ToDateTime()), message.DispositionDateTime);
		}

		public void TestIControllerIDProviderMembers()
		{
			var message = Factory.New<MQEDIMessage>();
			IControllerIDProvider provider = message;
			AssertEquals("ControllerID", ControllerIDs.Messaging.EDIMessage, provider.ControllerID);
			AssertEquals("BusinessObjectPK", message.PK.ToGuid(), provider.BusinessObjectPK);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			AssertEquals("ControllerID", ControllerIDs.Customs.US.BorderLineReleaseMessage, provider.ControllerID);
			AssertEquals("BusinessObjectPK", message.PK.ToGuid(), provider.BusinessObjectPK);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuota;
			AssertEquals("ControllerID", ControllerIDs.Customs.US.QueryMessages, provider.ControllerID);
			AssertEquals("BusinessObjectPK", message.PK.ToGuid(), provider.BusinessObjectPK);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuotaResponse;
			AssertEquals("ControllerID", ControllerIDs.Customs.US.QueryMessages, provider.ControllerID);
			AssertEquals("BusinessObjectPK", message.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestMessageNumPopulatedForBIRD()
		{
			var declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			AssertNotNull("PreCondition", declaration.ActiveEntryHeaders.EntrySummaryEntry);
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BIRDTransaction;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDEntrySummary;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Assert("PreCondition", message.IsBIRDTransaction);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZString.Empty, message.EM_MessageNum);
			AssertEquals("Status for BRD transactions", EDIMessage.Status.Sent, message.EM_Status);
			var aceBIRDMessage = Factory.New<MQEDIMessage>();
			aceBIRDMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDEntrySummary;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Assert("PreCondition", aceBIRDMessage.IsBIRDTransaction);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZString.Empty, aceBIRDMessage.EM_MessageNum);
			AssertEquals("Status for BRD transactions", EDIMessage.Status.Sent, aceBIRDMessage.EM_Status);
		}

		public void TestCertifyRequestDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration1.US_EnableENS = true;
			declaration1.US_EnableCRL = true;
			declaration1.JE_DeclarationReference = "B000005";
			declaration1.US_EntryFilerCode = "XJ5";
			Assert(declaration1.US_CertReqDate.IsEmpty);
			CusEntryHeader entry = declaration1.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "SE";
			CusEntryHeader entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = "ENS";
			CusEntryHeader entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = "CRL";
			CusEntryHeader entry3 = declaration1.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = "BCR";
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration1, ImportMessageSendingMessageType.Original);
			actions[0].US_CertifyCargoRelease = true;
			actions[0].US_SendMessage = true;
			actions[1].US_SendMessage = true;
			actions[2].US_SendMessage = true;
			actions[3].US_SendMessage = true;
			EntryHeaderSingleMessageManager manager = new EntryHeaderSingleMessageManager(entry, (EntryHeaderMessageSendingAction)actions[0]);
			manager.GenerateOriginalMessages(entry);
			manager = new EntryHeaderSingleMessageManager(entry1, (EntryHeaderMessageSendingAction)actions[1]);
			manager.GenerateOriginalMessages(entry1);
			manager = new EntryHeaderSingleMessageManager(entry2, (EntryHeaderMessageSendingAction)actions[2]);
			manager.GenerateOriginalMessages(entry2);
			actions[3].US_CertifyCargoRelease = true;
			manager = new EntryHeaderSingleMessageManager(entry3, (EntryHeaderMessageSendingAction)actions[3]);
			Factory.Save();
			Assert(!declaration1.US_CertReqDate.IsEmpty);
		}

		public void TestOutgoingBIRDMessageDoNotHaveEntryNumber()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			AssertNotNull("PreCondition", declaration.ActiveEntryHeaders.EntrySummaryEntry);
			MQEDIMessage message = new BIRDEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry).PopulateMessage();
			AssertEquals("PreCondition", ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
			Factory.Save(); //entry number place holder will be replaced by entry number which is empty
							//should be in 80-byte order, If not, it won't be serialised correctly and will throw an exception
			AssertNoExceptionThrown(() => _ = message.MessageBlock.MessageBlocks);
			var ens10 = message.MessageBlock.MessageBlocks.OfType<ENS10>().FirstOrDefault();
			AssertEquals(ZString.Empty, ens10.EntryNumber);
			AssertEquals("A field that appears after entry number", EntryTypeList.Codes.Warehouse, ens10.EntryType);
		}

		[TestDate(2009, 6, 1)]
		public void TestTotalENSAmountPayable()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208.60.2000";
			invoiceLine.JI_CustomsQuantity = 403m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_TaxApply = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Total Amount Payable", 12.5m, entry.TotalAmountPayable);
			var outgoingMessageFor21 = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false).PopulateMessage();
			AssertEquals(12.5m, outgoingMessageFor21.TotalENSAmountDue);
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var outgoingMessageFor21Deferred = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false).PopulateMessage();
			AssertEquals(12.5m, outgoingMessageFor21Deferred.TotalENSAmountDue);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var outgoingMessageFor01Deferred = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false).PopulateMessage();
			AssertEquals(37.5m, outgoingMessageFor01Deferred.TotalENSAmountDue);
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var outgoingMessageFor01NotDeferred = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false).PopulateMessage();
			AssertEquals(1474.73m, outgoingMessageFor01NotDeferred.TotalENSAmountDue);
		}

		[TestDate(2009, 6, 1)]
		public void TestACETotalENSAmountPayable()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208.60.2000";
			invoiceLine.JI_CustomsQuantity = 403m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_TaxApply = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Total Amount Payable", 1474.73m, entry.TotalAmountPayable);
			var outgoingMessageFor21 = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add).PopulateMessage();
			AssertEquals(1474.73m, outgoingMessageFor21.TotalENSAmountDue);
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var outgoingMessageFor21Deferred = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add).PopulateMessage();
			AssertEquals(37.5m, outgoingMessageFor21Deferred.TotalENSAmountDue);
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var outgoingMessageFor01NotDeferred = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add).PopulateMessage();
			AssertEquals(1474.73m, outgoingMessageFor01NotDeferred.TotalENSAmountDue);
		}

		public void TestMessageStatusToShowInQueryModule()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "~~";
			staff.GS_FullName = "Nobody Here";
			MQEDIMessage outgoing = Factory.New<MQEDIMessage>();
			outgoing.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "~122";
			outgoing.EM_SystemCreateTimeUtc = ZDateTime.Today;
			outgoing.EM_SystemCreateUser = "~~";
			outgoing.EM_MessageText = @"Z¿ºB09207100000000020000200002B1                                                
Z¿ºA09197100000000010000100001A1                                                
Z¿ºC09217100000000030000300003C1                                                
Z¿ºZ09237100000000050000500005Z1                                                
Z¿ºY09227100000000040000400004Y1";
			AssertEquals("should be QUE", MQEDIMessage.Status.Queued, outgoing.MessageStatusToShowInQueryModule);
			MQEDIMessage outgoing2 = Factory.New<MQEDIMessage>();
			outgoing2.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			outgoing2.EM_MessageNum = "~123";
			outgoing2.EM_SystemCreateTimeUtc = ZDateTime.Today;
			outgoing2.EM_SystemCreateUser = "~~";
			outgoing2.EM_MessageText = @"Z¿ºB09207100000000020000200002B1                                                
Z¿ºA09197100000000010000100001A1                                                
Z¿ºC09217100000000030000300003C1                                                
Z¿ºZ09237100000000050000500005Z1                                                
Z¿ºY09227100000000040000400004Y1";
			outgoing2.EM_Status = MQEDIMessage.Status.Sent;
			AssertEquals(false, outgoing2.HasRelatedMessage);
			MQEDIMessage response = Factory.New<MQEDIMessage>();
			response.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			response.EM_MessageNum = "~123";
			response.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(1);
			response.EM_SystemCreateUser = "~~";
			response.EM_Status = MQEDIMessage.Status.Received;
			response.EM_MessageText = @"Z¿ºB09207100000000020000200002B1                                                
Z¿ºA09197100000000010000100001A1                                                
Z¿ºC09217100000000030000300003C1                                                
Z¿ºZ09237100000000050000500005Z1                                                
Z¿ºY09227100000000040000400004Y1";
			AssertEquals(true, outgoing2.HasRelatedMessage);
			AssertEquals("should be RCV", MQEDIMessage.Status.Received, outgoing2.MessageStatusToShowInQueryModule);
		}

		public void TestIsENSCleared()
		{
			var accepted = CreateResponseMessage("1", AcceptedER);
			Assert(accepted.IsENSCleared);
			var rejected = CreateResponseMessage("1", RejectedER);
			Assert(!rejected.IsENSCleared);
			var acceptedBN = CreateResponseMessage("1", AcceptedBN);
			acceptedBN.EM_MessageType = "BN";
			Assert(!acceptedBN.IsENSCleared);
			rejected = CreateResponseMessage("1", BatchRejected);
			Assert(!rejected.IsENSCleared);
		}

		public void TestIsBIRDTransaction()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BIRDTransaction;
			Assert(message.IsBIRDTransaction);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			Assert(!message.IsBIRDTransaction);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BIRDEntrySummaryQuery;
			Assert(message.IsBIRDTransaction);
		}

		public void TestEM_MessageInterpretationForBIRD()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDEntrySummary;
			message.EM_MessageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         708802200015           000000000100NO                               0000002500  8950100000012813                                                                90                      0                       0000001281300000102500          ZZ7501000000010                                                                 ";
			AssertNoExceptionThrown(() => _ = message.EM_MessageInterpretation);
		}

		public void TestEM_MessageInterpretation_CargoManifestQuery()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoManifestInBondQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "B013307M34IS                                               5611                 "
				+ "R1                                       ANZ                     0008 081211    "
				+ "SC0860008 08121108657068233 0000100000                       08657068233        "
				+ "Y  3307M34IS00002";
			string expected = @"------------------APLB------------------
 Block Number (2-3)                  :1
 Processing District Port Code (4-7) :3307
 Entry Filer Code (8-10)             :M34
 Application Identifier (11-12)      :IS
 User Data (60-80)                   :5611

-----------------CMQR1------------------
 Carrier Code (42-45)                       :ANZ
 Voyage Flight Trip Manifest Number (66-70) :0008
 Date Of Arrival (71-76)                    :12-Aug-11

-----------------CMQSC------------------
 Importing Carrier Code (3-5)   :086
 Flight Number (6-10)           :0008
 Scheduled Arrival Date (11-16) :12-Aug-11
 Air Waybill Number (17-27)     :08657068233
 Manifest Quantity (29-33)      :1
 Inbond Number (62-72)          :08657068233

------------------APLY------------------
 Processing District Port Code (4-7)                       :3307
 Entry Filer Code (8-10)                                   :M34
 Application Identifier (11-12)                            :IS
 Number Of Transaction Detail Records In The Block (13-17) :2";
			AssertMultilineASCIIEquals("", expected, message.EM_MessageInterpretation);
		}

		public void TestHasFDADetails()
		{
			MQEDIMessage messageWithFD01 = Factory.New<MQEDIMessage>();
			messageWithFD01.EM_MessageText = "B018888XJ5HI                                               " + EDIMessage.MessageNumberPlaceHolder
				+ "H1A8888XJ5 0000467791-0131990004006040881                   AAAD888802891       "
				+ "H2L363    91-013199000 001  0000000100B00001035                                 "
				+ "HA            12345678914                         00000120BG                    "
				+ "H5001AD0101100010                           0000000100                          "
				+ "OA  FD0                                                                         "
				+ "H5002AU0101100010                           0000000000                          "
				+ "OI        MALE HORSES, PUREBRED BREE                                            "
				+ "FD0100112AAB01   ADADA01012008                                                  "
				+ "FD02                                                                            "
				+ "FD03                                                                            "
				+ "FD04                                                                            "
				+ "FD05ATA1200                                                                     "
				+ "Y  8888XJ5HI00012";
			AssertEquals("HasFDADetails", true, messageWithFD01.HasFDADetails);
			MQEDIMessage messageWithOutFD01 = Factory.New<MQEDIMessage>();
			messageWithOutFD01.EM_MessageText = "B018888XJ5ER                                               6000748              "
				+ "10R888891-01319900091-013199000                 8         XJ5 1000084801891  IL "
				+ "40001SG0000010000000000015000000000000000000000000000050055976                  "
				+ "50 19011045000000093000            KG                               SG123106NSG "
				+ "Y  8888XJ5ER00009000000093000";
			AssertEquals("HasFDADetails", false, messageWithOutFD01.HasFDADetails);
		}

		public void TestStatementLinkableObject()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_LinkUniqueID = statement.PK;
			message.EM_LinkTable = CusStatementHeaderSchema.Constants.TableName;
			AssertEquals(statement, message.EM_LinkedObject);
		}

		public void TestIVisaQuery()
		{
			var visaQueryMessage = Factory.New<MQEDIMessage>();
			visaQueryMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			visaQueryMessage.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			visaQueryMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuota;
			visaQueryMessage.EM_MessageText = "B018888XJ5UI                                               6001382              U198191127  6304920000ZMV                                                       Y  8888XJ5UI00001";
			AssertEquals("Tariff Number", "98191127", ((IVisaQuery)visaQueryMessage).TariffNumber);
			AssertEquals("Second Tariff Number", "6304920000", ((IVisaQuery)visaQueryMessage).SecondTariffNumber);
			AssertEquals("C/O", "ZM", ((IVisaQuery)visaQueryMessage).OriginCountry);
		}

		public void TestGetEntryStatusFromMessages_ER_RR_IS()
		{
			var entrySummaryMessage = Factory.New<MQEDIMessage>();
			entrySummaryMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryMessage.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			entrySummaryMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			entrySummaryMessage.EM_MessageText = "B018888XJ5ER                                               6000790              E08888XJ5 10000657B00001267ACCEPTED - RECORDS REQUIRED             01808        E08888XJ5 10000657B00001267CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002";
			AssertEquals("EntryStatus", ImportEntryStatusList.Codes.CRL, ((IEntryStatusProvider)entrySummaryMessage).EntryStatus);
			var cargoReleaseProcessingResult = Factory.New<MQEDIMessage>();
			cargoReleaseProcessingResult.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cargoReleaseProcessingResult.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			cargoReleaseProcessingResult.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			cargoReleaseProcessingResult.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 100006570191-013199000B00001267APLUAPL EMERALD         365  080807    R4            6985445                             00000150PK   APLU             R5100807012306ENTRY DOCUMENTS REQUIRED                                          Y018888XJ5RR00003";
			AssertEquals("EntryStatus", ImportEntryStatusList.Codes._06, ((IEntryStatusProvider)cargoReleaseProcessingResult).EntryStatus);
			var queryEntryStatusResult = Factory.New<MQEDIMessage>();
			queryEntryStatusResult.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			queryEntryStatusResult.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			queryEntryStatusResult.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse;
			queryEntryStatusResult.EM_MessageText = "B018888XJ5IS                                               6000795              R18888XJ5 100006570191-013199000B00001267APLUAPL EMERALD         365  080807Q999R4            6985445                             00000150PK   APLU             R5100807012307Override to Intensive                                             Y  8888XJ5IS00003";
			AssertEquals("EntryStatus", ImportEntryStatusList.Codes._07, ((IEntryStatusProvider)queryEntryStatusResult).EntryStatus);
		}

		public void TestEM_LinkedObjectRetrievesCorrectly()
		{
			var master = Factory.New<OrgHeader>();
			var address = master.MainAddress;
			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkTable = OrgHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = master.PK;
			AssertEquals(master, message.EM_LinkedObject);
			message = Factory.New<MQEDIMessage>();
			message.EM_LinkTable = OrgAddressSchema.Constants.TableName;
			message.EM_LinkUniqueID = address.PK;
			AssertEquals(address, message.EM_LinkedObject);
		}

		public void TestFillInEntryNumberAndReplaceWithEntryNumberPlaceHolder()
		{
			DeclarationTestHelper.SetupForSendMessage();
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			companyStmNums.SetNextNumber(100212);
			var inBondSetting = InBondNumberSetting.New(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK));
			inBondSetting.NextNumber = 100212;
			inBondSetting.PostNextNumber();
			var nextInBondNumber = inBondSetting.CurrentNextNumber.ToString().PadLeft(8, '0');
			var expectedEntryNumber = companyStmNums.GenerateCustomsNumber(100212);
			var expectedInBondNumber = nextInBondNumber + InBondNumberCheckDigitCalculator.GetCheckDigit(nextInBondNumber);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var inBondEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.InBondDeparture);
			var ensEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			AssertEquals(1, inBondEntry.Length);
			AssertEquals(1, ensEntry.Length);
			var ensMessage = Factory.New<MQEDIMessage>();
			ensMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ensMessage.EM_LinkedObject = ensEntry[0];
			ensMessage.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			var inBondMessage = Factory.New<MQEDIMessage>();
			inBondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			inBondMessage.EM_LinkedObject = inBondEntry[0];
			inBondMessage.EM_MessageText = MQEDIMessage.InBondNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			Factory.Save();
			AssertEquals("Entry number for the entry", expectedEntryNumber, ensEntry[0].EntryNumber);
			AssertEquals("Message text replaced with entry number", true, ensMessage.EM_MessageText.Contains(expectedEntryNumber));
			AssertEquals("entry number for the inbond entry", expectedInBondNumber, inBondEntry[0].EntryNumber);
			AssertEquals("Message text replaced with entry number", true, inBondMessage.EM_MessageText.Contains(expectedInBondNumber));
		}

		public void TestDefaultValues()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			AssertEquals(EDIMessage.ApplicationCodes.USCustomsImport, message.EM_ApplicationCode);
		}

		public void TestActionStatusWithApplicationReference()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationReference = string.Empty;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			AssertEquals(EM_ActionStatusList.Codes.Complete, message.EM_ActionStatus);
		}

		public void TestActionStatus()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ENSStatusDispositionCode, "UCDSP", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INoFurtherActionRequired, "INoFurtherActionRequired", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IActionRequiredDespiteActionID, "IActionRequiredDespiteActionID", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1", "1 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "2", "2 DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3", "3 DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "4", "4 DESC", startDate, endDate);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "5", "5 DESC", startDate, endDate);
			var code6 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "6", "6 DESC", startDate, endDate);
			var code7 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "7", "7 DESC", startDate, endDate);
			var code8 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "8", "8 DESC", startDate, endDate);
			var code9 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "9", "9 DESC", startDate, endDate);
			var codeE = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "E", "E DESC", startDate, endDate);
			var codeP = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "P", "P DESC", startDate, endDate);
			var codeQ = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Q", "Q DESC", startDate, endDate);
			var codeR = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "R", "R DESC", startDate, endDate);
			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName2.ZXE_Name, "Y");
			var attributeE1 = helper.CreateNewOrGetExistingCusCodeListAttribute(codeE.PK, attributeName2.ZXE_Name, "Y");
			var attribute61 = helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, attributeName1.ZXE_Name, "Y");
			var attribute71 = helper.CreateNewOrGetExistingCusCodeListAttribute(code7.PK, attributeName1.ZXE_Name, "Y");
			var attribute81 = helper.CreateNewOrGetExistingCusCodeListAttribute(code8.PK, attributeName1.ZXE_Name, "Y");
			var attributeQ1 = helper.CreateNewOrGetExistingCusCodeListAttribute(codeQ.PK, attributeName1.ZXE_Name, "Y");
			Factory.Save();
			var message = Factory.New<MQEDIMessage>();
			AssertEquals(EM_ActionStatusList.Codes.Incomplete, message.EM_ActionStatus);
			Assert(!message.ActionAuthorised);
			message.Logs.AddNew(Events.Authorised, "Blah");
			AssertEquals("Blah", message.EM_ActionStatus);
			Assert(message.ActionAuthorised);
			message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_ApplicationReference = ENSStatusDispositionCodeList._7;
			Assert("PreCondition", !ENSStatusDispositionCodeListLoader.IsFurtherActionRequired(Factory, message.EM_ApplicationReference));
			AssertEquals("Action status when no further action is required", EM_ActionStatusList.Descriptions.Complete, message.EM_ActionStatus);
		}

		public void TestIsComplete()
		{
			var message = Factory.New<MQEDIMessage>();
			Assert(!message.IsComplete);
			message.Logs.AddNew(Events.Acknowledged, "Blah");
			Assert(!message.IsComplete);
			var log = message.Logs.AddNew(Events.Authorised, "Blah");
			Assert(message.IsComplete);
			log.Cancel();
			Assert(!message.IsComplete);
			message.Logs.AddNew(Events.Authorised, "Blah");
			Assert(message.IsComplete);
		}

		public void TestSetToComplete()
		{
			var message = Factory.New<MQEDIMessage>();
			message.SetToComplete();
			var logs = message.Logs.Find(x => x.SL_Reference == EM_ActionStatusList.Codes.Complete && !x.SL_IsCancelled);
			AssertEquals(1, logs.Count());
			message.SetToComplete();
			logs = message.Logs.Find(x => x.SL_Reference == EM_ActionStatusList.Codes.Complete && !x.SL_IsCancelled);
			AssertEquals(1, logs.Count());
		}

		public void TestIsAIICleared()
		{
			var entrySummaryClearedMessage1 = Factory.New<MQEDIMessage>();
			entrySummaryClearedMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryClearedMessage1.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			entrySummaryClearedMessage1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse;
			entrySummaryClearedMessage1.EM_MessageText = "B018888XJ5CR                                               631                  E00AUABCEXP6390ALEINV08011520          EJT  ERROR-FREE INVOICE ACKNOWLEDGED     Y  8888XJ5CR00001";
			AssertEquals(true, entrySummaryClearedMessage1.IsAIICleared);
			var entrySummaryRejectedMessage1 = Factory.New<MQEDIMessage>();
			entrySummaryRejectedMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryRejectedMessage1.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			entrySummaryRejectedMessage1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse;
			entrySummaryRejectedMessage1.EM_MessageText = "B018888XJ5CR                                               624                  C01AAU1234123123   INV08011135      011308INUSD                                 C08UC12-45687945AB                                                              E08AU1234123123   INV08011135          EIJ  INVALID PARTY NUMBER                C08IM12-45687945AB                                                              E08AU1234123123   INV08011135          EIJ  INVALID PARTY NUMBER                C08IM12-45687945AB                                                              E08AU1234123123   INV08011135          EIJ  INVALID PARTY NUMBER                E95AU1234123123   INV08011135          524  TRANSACTION DATA REJECTED           Y  8888XJ5CR00008";
			AssertEquals(false, entrySummaryRejectedMessage1.IsAIICleared);
			var entrySummaryClearedMessage2 = Factory.New<MQEDIMessage>();
			entrySummaryClearedMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryClearedMessage2.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			entrySummaryClearedMessage2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse;
			entrySummaryClearedMessage2.EM_MessageText = "B018888XJ5CR                                               11483                E00AUABCEXP6390ALE345                  EJT  ERROR-FREE INVOICE ACKNOWLEDGED     Y  8888XJ5CR00001";
			AssertEquals(true, entrySummaryClearedMessage2.IsAIICleared);
			var entrySummaryRejectedMessage2 = Factory.New<MQEDIMessage>();
			entrySummaryRejectedMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryRejectedMessage2.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			entrySummaryRejectedMessage2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse;
			entrySummaryRejectedMessage2.EM_MessageText = "B018888XJ5CR                                               646                  C01AAUABCEXP6390ALEINV0801151038    011408INAUD10750000                         C84000000017960607000000000000000 000000000000000000000000000000                E84AUABCEXP6390ALEINV0801151038        EJK  INVALID TOTAL INVOICE VALUE         E84AUABCEXP6390ALEINV0801151038        EJL  TOTAL SUBJECT TO US DUTY WRONG      E84AUABCEXP6390ALEINV0801151038        EJ1  TOTAL AMT OF INVOICE LINES WRONG    E95AUABCEXP6390ALEINV0801151038        524  TRANSACTION DATA REJECTED           Y  8888XJ5CR00006";
			AssertEquals(false, entrySummaryRejectedMessage2.IsAIICleared);
		}

		public void TestGetNumberFountainNumbersAndFillInPlaceHoldersForReconEntry()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertNotNull(reconDeclaration.ReconEntry);
			new ReconMessageManager(new ReconDeclarationIReconciliation(reconDeclaration), UpdateActionCode.Add).PopulateMessage();
			AssertEquals("One message should have been generated", 1, reconDeclaration.Messages.Count);
			MQEDIMessage message = (MQEDIMessage)reconDeclaration.Messages[0];
			AssertEquals("PlaceHolder is replaced", false, message.EM_MessageText.Contains(MQEDIMessage.USEntryFilerEntryNumberPlaceHolder));
			AssertEquals("Replaced with EntryFilerCode + EntryNumber", true, reconDeclaration.Messages[0].EM_MessageText.Contains(reconDeclaration.ReconEntryNumberWithEntryFilerCode));
		}

		public void TestGetNumberFountainNumbersAndFillInPlaceHoldersForDrawbackSummary()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			new DrawbackSummaryMessageManager(new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add)).PopulateMessage(UpdateActionCode.Add);
			AssertEquals("One message should have been generated", 1, declaration.Messages.Count);
			MQEDIMessage message = (MQEDIMessage)declaration.Messages[0];
			AssertEquals(true, message.EM_MessageText.Contains(MQEDIMessage.USEntryFilerEntryNumberPlaceHolder));
			Factory.Save();
			AssertEquals("PlaceHolder is replaced", false, message.EM_MessageText.Contains(MQEDIMessage.USEntryFilerEntryNumberPlaceHolder));
			AssertEquals("Replaced with EntryFilerCode + EntryNumber", true, declaration.Messages[0].EM_MessageText.Contains(declaration.US_EntryFilerCode + declaration.DeclarationNumber));
		}

		public void TestImporterShouldBeRegisteredInCustomsMessageIsExcluded()
		{
			var eIN = "123";
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, eIN);
			var addInfo = (OrgImpAddInfo)importer.CountryData.ImpAddInfo;
			addInfo.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableCRL = true;
			AssertNoMessageError(declaration.IOROrgPKInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			declaration.IOROrgPK = importer.PK;
			AssertHasMessageError(declaration.IOROrgPKInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			var message = Factory.New<EDIMessage>();
			declaration.Messages.Add(message);
			AssertEquals(typeof(EDIMessage), message.GetType());
			message = Factory.NewWithValidTestData<MQEDIMessage>();
			declaration.Messages.Add(message);
			AssertEquals(false, message.EM_SendWithMessageErrors);
			AssertEquals(typeof(MQEDIMessage), message.GetType());
		}

		public void TestCountOfReconOriginalEntries()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(1999, 9, 15);
			ReconOriginalEntryHeader entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4300m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4600m);
			entry1.US_PaymentDate = new ZDate(1999, 1, 5);
			ReconOriginalEntryHeader entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry, 4000m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry, 4670m);
			entry2.US_PaymentDate = new ZDate(1999, 4, 12);
			ReconOriginalEntryHeader entry3 = reconDeclaration.OriginalEntries.AddNew();
			entry3.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, 4000m);
			entry3.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, 4600m);
			entry3.US_PaymentDate = new ZDate(1999, 5, 28);
			ReconDeclarationIReconciliation iReconciliation = new ReconDeclarationIReconciliation(reconDeclaration);
			MQEDIMessage message = new ReconciliationMessageBuilder("A", iReconciliation).Generate();
			AssertEquals("3", message.CountOfReconOriginalEntries);
		}

		public void TestOnSavingStandAlonePriorNoticeMessageAllocateEntryNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableSPN = true;
			MQEDIMessage rcvMessage = (MQEDIMessage)declaration.Messages.AddNew(typeof(MQEDIMessage));
			rcvMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			rcvMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability;
			rcvMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.FDAPriorNotice;
			rcvMessage.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder;
			MQEDIMessage nonPriorNoticeMessage = (MQEDIMessage)declaration.Messages.AddNew(typeof(MQEDIMessage));
			nonPriorNoticeMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			nonPriorNoticeMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability;
			nonPriorNoticeMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondArrival;
			nonPriorNoticeMessage.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + MQEDIMessage.MessageNumberPlaceHolder + " B";
			Factory.Save();
			AssertEquals(MQEDIMessage.USEntryNumberPlaceHolder, rcvMessage.EM_MessageText);
			AssertEquals(MQEDIMessage.USEntryNumberPlaceHolder + nonPriorNoticeMessage.EM_MessageNum.PadRight(MQEDIMessage.MessageNumberPlaceHolder.Length) + " B", nonPriorNoticeMessage.EM_MessageText);
			AssertEquals(ZString.Empty, declaration.ImportEntryNumber);
			MQEDIMessage message = (MQEDIMessage)declaration.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.FDAPriorNotice;
			message.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + MQEDIMessage.MessageNumberPlaceHolder + " B";
			Factory.Save();
			AssertEquals(MQEDIMessage.USEntryNumberPlaceHolder, rcvMessage.EM_MessageText);
			AssertEquals(MQEDIMessage.USEntryNumberPlaceHolder + nonPriorNoticeMessage.EM_MessageNum.PadRight(MQEDIMessage.MessageNumberPlaceHolder.Length) + " B", nonPriorNoticeMessage.EM_MessageText);
			AssertEquals(declaration.ImportEntryNumber + message.EM_MessageNum.PadRight(MQEDIMessage.MessageNumberPlaceHolder.Length) + " B", message.EM_MessageText);
			AssertNotEquals(ZString.Empty, declaration.ImportEntryNumber);
			message.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + MQEDIMessage.MessageNumberPlaceHolder + " B";
			declaration.ENSEntryNumber.Delete();
			Factory.Save();
			AssertEquals(MQEDIMessage.USEntryNumberPlaceHolder, rcvMessage.EM_MessageText);
			AssertEquals(MQEDIMessage.USEntryNumberPlaceHolder + nonPriorNoticeMessage.EM_MessageNum.PadRight(MQEDIMessage.MessageNumberPlaceHolder.Length) + " B", nonPriorNoticeMessage.EM_MessageText);
			AssertEquals("message already in database", MQEDIMessage.USEntryNumberPlaceHolder + MQEDIMessage.MessageNumberPlaceHolder + " B", message.EM_MessageText);
			AssertEquals(ZString.Empty, declaration.ImportEntryNumber);
		}

		public void TestOnSavingInbondRelatedRecoredsRebuilded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			BlockControlGenerator generator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			MQEDIMessage message = generator.CreateMessage<MQEDIMessage>(Factory);
			bill.Messages.Add(message);
			Factory.Save();
			AssertEquals(1, declaration.InBondRelatedRecords.Count);
			bill = declaration.Bills.AddNew();
			bill.Messages.Add(generator.CreateMessage<MQEDIMessage>(Factory));
			Factory.Save();
			AssertEquals(2, declaration.InBondRelatedRecords.Count);
		}

		public void TestStatusesErrorsVisibility()
		{
			MQEDIMessage message = (MQEDIMessage)GetNewBusinessObject();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			Assert(message.StatusesAndErrorsVisible);
			AssertEquals("No Statuses/Errors available on outgoing messages", message.StatusesErrorsExist);
		}

		public void TestStatusesAndErrors()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cRLEntry = declaration.ActiveEntryHeaders.AddNew();
			cRLEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			cRLEntry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			AssertEquals(0, ((MQEDIMessage)cRLEntry.Messages[0]).StatusesAndErrors.Count);
			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse);
			cRLEntry.Messages.Add(message);
			message.EM_MessageText = "B018888XJ5HR                                               35934                "
				+ "H1A8888XJ5 7002309491-0131990004010260981                   LH  888801891       "
				+ "H2I299    91-013199000 430  0000001500B00152733                                 "
				+ "H68888XJ5 70023094B0015273391-013199000DATA ADDED AS REQUESTED       2GC        "
				+ "H68888XJ5 70023094B0015273391-013199000CARGO RELEASE DATA CERTIFIED  2A4        "
				+ "Y  8888XJ5HR00004";
			AssertEquals(2, ((MQEDIMessage)cRLEntry.Messages[1]).StatusesAndErrors.Count);
			var failedMessage = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse);
			cRLEntry.Messages.Add(failedMessage);
			failedMessage.EM_MessageText = "B018888XJ5HR                                               35935                "
				+ "H1A8888XJ5 7002297199-9999999001110170981                   MAEU100101432       "
				+ "H2S505    99-999999900 425W 0000000000B00152704            VALDIVIA             "
				+ "H68888XJ5 70022971B0015270499-999999900IMPORTER NUMBER NOT ON FILE   646        "
				+ "H68888XJ5 70022971B0015270499-999999900TRANSACTION DATA REJECTED     524        "
				+ "Y  8888XJ5HR00004";
			AssertEquals(2, ((MQEDIMessage)cRLEntry.Messages[2]).StatusesAndErrors.Count);
		}

		public void TestStatusesAndErrorsForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.Messages.Add(CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary));
			AssertEquals(0, ((MQEDIMessage)entry.Messages[0]).StatusesAndErrors.Count);
			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			entry.Messages.Add(message);
			message.EM_MessageText = "B003902SV9AX                                               651                  "
				+ "E0 SUMMRY 000001 REF ID: SV9 00045011 B00001216                                 "
				+ "E0 LINITM 000002 REF ID: 002                                                    "
				+ "E0 ARPART 000003 REF ID: S 98-456321400                                         "

				+ "E1 F578   SOLD TO PARTY UNKNOWN                   SV9  00045011     B00001216   "
				+ "E0 LINITM 000003 REF ID: 003                                                    "
				+ "E0 ARPART 000003 REF ID: S 98-456321400                                         "
				+ "E1 F578   SOLD TO PARTY UNKNOWN                   SV9  00045011     B00001216   "
				+ "E1RF998   TRANSACTION DATA REJECTED               SV9  00045011     B00001216   "
				+ "Y  3902SV9AX00008";
			message = ((MQEDIMessage)entry.Messages[1]);
			AssertEquals(3, message.StatusesAndErrors.Count);
			var statusesAndErrors = message.StatusesAndErrors;
			AssertEquals("2", statusesAndErrors[0].LineNumber);
			AssertEquals("3", statusesAndErrors[1].LineNumber);
			AssertEquals("", statusesAndErrors[2].LineNumber);
			var message2 = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			entry.Messages.Add(message2);
			message2.EM_MessageText = "B003902SV9AX                                               554                  "
				+ "E0 SUMMRY 000001 REF ID: SV9 00000040 B00001206                                 "
				+ "E1AI996   SUMMARY HAS BEEN REPLACED               SV9  00000040     B00001206   "
				+ "Y  3902SV9AX00002";
			message = ((MQEDIMessage)entry.Messages[2]);
			AssertEquals(1, message.StatusesAndErrors.Count);
			var statusData = message.StatusesAndErrors[0];
			AssertEquals("", statusData.LineNumber);
			AssertEquals("SUMMARY HAS BEEN REPLACED", statusData.NarrativeMessage);
			var message3 = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			entry.Messages.Add(message3);
			message3.EM_MessageText = "B003902SV9AX                                               539                  "
				+ "E0 SUMMRY 000001 REF ID: SV9 00000024 B00001204                                 "
				+ "E1 F201   STMT CLIENT BRANCH NOT ALLOWED          SV9  00000024     B00001204   "
				+ "E0 LINITM 000001 REF ID: 001                                                    "
				+ "E1 F577   SOLD TO PARTY MISSING-REQ'D FOR TYPE    SV9  00000024     B00001204   "
				+ "E0 TARIFF 000001 REF ID: 3506990000                                             "
				+ "E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  00000024     B00001204   "
				+ "E1RF998   TRANSACTION DATA REJECTED               SV9  00000024     B00001204   "
				+ "Y  3902SV9AX00007";
			message = ((MQEDIMessage)entry.Messages[3]);
			AssertEquals(4, message.StatusesAndErrors.Count);
			statusData = message.StatusesAndErrors[0];
			AssertEquals("", statusData.LineNumber);
			AssertEquals("STMT CLIENT BRANCH NOT ALLOWED", statusData.NarrativeMessage);
			statusData = message.StatusesAndErrors[1];
			AssertEquals("1", statusData.LineNumber);
			AssertEquals("SOLD TO PARTY MISSING-REQ'D FOR TYPE", statusData.NarrativeMessage);
			AssertEquals("", statusData.TariffNumber);
			statusData = message.StatusesAndErrors[2];
			AssertEquals("1", statusData.LineNumber);
			AssertEquals("*CENSUS* OR-LO VAL/QTY (1)", statusData.NarrativeMessage);
			AssertEquals("3506990000", statusData.TariffNumber);
			statusData = message.StatusesAndErrors[3];
			AssertEquals("", statusData.LineNumber);
			AssertEquals("TRANSACTION DATA REJECTED", statusData.NarrativeMessage);
			AssertEquals("", statusData.TariffNumber);
		}

		public void TestENSERecordsForPGAMembers()
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
			message.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_162451     "
				+ "E0 SUMMRY 000001 REF ID: SV9 71009920 B00163412                                 "
				+ "E0 LINITM 0002   REF ID:2                                                       "
				+ "E0 TARIFF 000001 REF ID:2921196010                                              "
				+ "E0 OI            REF ID: PESTICIDES                                             "
				+ "E0 PG01          REF ID: 001EPAPS1   Y                        130.027           "
				+ "E1 PPH6   MISSING DIS DOCUMENTATION                                             "
				+ "E1RF998   TRANSACTION DATA REJECTED               SV9  71009920     B00163412   "
				+ "Y  1101SV9AX00004";
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(message);
			var message2 = ((MQEDIMessage)declaration.ActiveEntryHeaders[0].Messages[1]);
			Assert(message2.StatusesAndErrors.Count > 0);
			var statusErrorsData = message2.StatusesAndErrors[0];
			AssertEquals("PGAAgencyCode", "EPA", statusErrorsData.PGAAgencyCode);
			AssertEquals("PGALine", "001", statusErrorsData.PGALine);
		}

		public void TestENSERecordsForCargoReleaseResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			message.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_162367     "
				+ "SE10RSV9  71009656 01EI 13-14792700040800000010001101  1101                     "
				+ "SE15R    00104291327                                       00000010PC           "
				+ "SE20CR B00163345                                                                "
				+ "SE40001CH YUMMY TUNA                                                            "
				+ "SE6003034200200000001000                                                        "
				+ "OI        YUMMY TUNA                                                            "
				+ "PG01001NMF370YFTYY                                                              "
				+ "SE9011P48   MISSING PG22 RECORD PER PGA                                         "
				+ "SE9013PH6   MISSING DIS DOCUMENTATION                                           "
				+ "PG02P                                                                           "
				+ "PG01002NMF370YFTYY                                                              "
				+ "SE9011P47   WHATEVER                                                            "
				+ "PG02P                                                                           "
				+ "SE9001   SE DATA REJECTED                                                       "
				+ "SE40002CH YUMMY TUNA                                                            "
				+ "SE6004034200200000001000                                                        "
				+ "OI        YUMMY TUNA                                                            "
				+ "PG01001EPA370YFTYY                                                              "
				+ "SE9011P46   WHATEVER2                                                           "
				+ "PG02P                                                                           "
				+ "SE9001   SE DATA REJECTED                                                       "
				+ "Y  1101SV9SX00015";
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			entry.Messages.Add(message);
			var message2 = ((MQEDIMessage)declaration.ActiveEntryHeaders[0].Messages[0]);
			Assert(message2.StatusesAndErrors.Count > 0);
			var statusErrorsData = message2.StatusesAndErrors[0];
			AssertEquals("Should has LineNumber", "1", statusErrorsData.LineNumber);
			AssertEquals("Should has TariffNumber", "0303420020", statusErrorsData.TariffNumber);
			AssertEquals("Should has PGAAgencyCode", "NMF", statusErrorsData.PGAAgencyCode);
			AssertEquals("Should has PGALine", "1", statusErrorsData.PGALine);
		}

		public void TestIsProtestCleared()
		{
			var message = CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.ProtestInitialFiling);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ProtestInitialFiling;
			AssertEquals(false, message.IsProtestCleared);
			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse;
			AssertEquals(false, message.IsProtestCleared);
			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse;
			message.EM_MessageText = "B015501125PL                                               29064                P01550111150027B11O004724     Protest filing accepted error free                Y  5501125PL00001";
			AssertEquals(true, message.IsProtestCleared);
			var responseMessage2 = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse);
			responseMessage2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse;
			responseMessage2.EM_MessageText = "B013901SV9PL                                               115279               P10            B00156780   X R                                                  P11S        S        S20111121PR0                                               PERAQ3TEAM CODE INVALID                                                         P1511011123456456456123456789123201111211                                       PERAQGASSOC. CLAIM NBR NOT ON FILE                                              PERAQLASSOC. PROT. NBR NOT APPLIC                                               PERAQOFILING PERIOD BASE DATE N/A                                               PERAQRBASE DATE QUALIFIER N/A                                                   P16NNYINTER LEAD        *********                                               PERAQVINT. ADVICE NBR NOT ON FILE                                               PERAQXLEAD PROTEST NBR NOT ON FILE.                                             PERAQZTEST CASE NUMBER NOT ON FILE                                              P50  1101X2901                                                                  PERARYNOTIFY DP/FLR/OFFICE INVALID                                              P600001 SV970023849                                                             PERAS1WRN: ENTRY NOT LIQ/CANCELLED                                              PERASZPRT:ENTRY NOT ON DEN 520/ASSOC514                                         PERAP3HEADER/DETAIL DATA REJECTED                                               PERAQ0NO FILING PERIOD BASE DATE                                                PERAR7NOTIFY SUB. DATA REJECTED                                                 PERASPENTRY LIST REJECTED                                                       PERASXPROTEST FILING INCOMPLETE                                                 P99            B00156780   REJPROTEST FILING REJECTED WITH ERROR(S)             Y  3901SV9PL00023";
			AssertEquals(false, responseMessage2.IsProtestCleared);
		}

		public void TestIsFTZCleared()
		{
			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData);
			AssertEquals(false, message.IsFTZCleared);
			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			message.EM_MessageText = "B013910SV9NF                                               HYEDUSCMT_118673     90A153000112000000702501N                                                       91B6 2  7773910003600000HAWB001            1205251335                           9502119 PHYSICAL ARRIVAL DATA ACCEPTED                                          Y  3910SV9NF00001";
			AssertEquals(true, message.IsFTZCleared);
			var responseMessage2 = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			responseMessage2.EM_MessageText = "B013910SV9NF                                               HYEDUSCMT_118674     90A153000112000000702501N                                                       91B7 2  7773910003600000HAWB001            1205251342                           1027773910003600000HAWB001            B 0000000100FI                            9501152 ADMSN MUST BE AUTH PRIOR TO CONCURRENCE                                 9502120 TRANSACTION DATA REJECTED                                               Y  3910SV9NF00003";
			AssertEquals(false, responseMessage2.IsFTZCleared);
			var responseMessage3 = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			responseMessage3.EM_MessageText = "B013910SV9NF                                               HYEDUSCMT_118672     90A153000112000000702501N                                                       91B2 1  15300011200000070                  1205251330                           20A40CR  CLIENT REP TEST AIR CARCR987          2011110320111104250120111104     4077739100036                        00000HAWB001        0000000100GB41380      950327T*CENSUS*INVALID AIR MOT                                                  9502083 ZONE ADMISSION DATA ACCEPTED                                            Y  3910SV9NF00004";
			AssertEquals(true, responseMessage3.IsFTZCleared);
		}

		public void TestIsFTZAcceptedWithCensusWarning()
		{
			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData);
			AssertEquals(false, message.IsFTZAcceptedWithCensusWarning);
			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			AssertEquals(false, message.IsFTZAcceptedWithCensusWarning);
			message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			message.EM_MessageText = "B013910SV9NF                                               HYEDUSCMT_118672     90A153000112000000702501N                                                       91B2 1  15300011200000070                  1205251330                           20A40CR  CLIENT REP TEST AIR CARCR987          2011110320111104250120111104     4077739100036                        00000HAWB001        0000000100GB41380      950327T*CENSUS*INVALID AIR MOT                                                  9502083 ZONE ADMISSION DATA ACCEPTED                                            Y  3910SV9NF00004";
			AssertEquals(true, message.IsFTZAcceptedWithCensusWarning);
			var responseMessage2 = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			responseMessage2.EM_MessageText = "B003910SV9NF                                               HYEDUSCMT_118552     90A153000112000000652501N                                                       91BF 1  15300011200000065                  1205201741                           95     FTZ ADMISSION AUTHORIZED                                                 Y  3910SV9NF00003";
			AssertEquals(false, responseMessage2.IsFTZAcceptedWithCensusWarning);
		}

		public void TestHasCensusWarnings()
		{
			var message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse);
			AssertEquals(false, message.HasCensusWarnings);
			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			AssertEquals(false, message.HasCensusWarnings);
			message.EM_MessageText = "B012507175ER                                               20397                10R250713-4063812CX13-4063812CX                 8         175 1916203001098  CA E102507175 19162030   54601   EXISTING ENTRY IN CUSTOMS STATUS         OOI207090E102507175 19162030   52401   TRANSACTION DATA REJECTED                OOI207090Y  2507175ER00003000000014157";
			AssertEquals(false, message.HasCensusWarnings);
			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			message.EM_MessageText = "B012507175ER                                               17611                "
				+ "10A250713-4063812CX13-4063812CX                 8         175 1916203001098  CA "
				+ "40002US00000000000000000012000000000000000000000000000000                       "
				+ "50 9801001095                      X                                MX011812Y   "
				+ "E502507175 1916203000228E01   *CENSUS* OR-LO VAL/QTY (2)TARIFF1        OOI207090"
				+ "9000000014157000000000000 00000000000000000000000000000250000000007891          "
				+ "E902507175 19162030   58401761ENT-SUM ACCEPTED WITH WARNINGS           OOI207090"
				+ "Y  2507175ER00006000000014157";
			AssertEquals(true, message.HasCensusWarnings);
		}

		public void TestIsEntrySummaryQuerySuccessful()
		{
			var message = CreateResponseMessage("1", "B018888XJ5JR                                               58                   "
				+ "J18888XJ5 0000011391-013199000010000000040000000000000                  001   B "
				+ "J2XJ5 00000113      0000000000000000000000                               808001B"
				+ "J3XJ5 00000113112607071126070112546821485211210789130010715000000002400B00001011"
				+ "J5XJ5 00000113071120ESP                                                         "
				+ "J98888XJ5 00000113BILLING DATA NOT ON FILE                                      "
				+ "J98888XJ5 00000113COLLECTION DATA NOT ON FILE                                   "
				+ "Y  8888XJ5JR00006");
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse;
			AssertEquals(true, message.IsEntrySummaryQuerySuccessful);
			message = CreateResponseMessage("2", "B013901610JR                                               2336                 "
				+ "J15206610 8100007913-557351300010000000000000000000000101000101900      216     "
				+ "J2610 8100007910190000000000000000000000000824010000000000000000000000   474001 "
				+ "J3610 81000079                            10190003210002839000000000000008100007"
				+ "J5610 81000079001208N8YL012015                                                  "
				+ "Y  3901610JR00004");
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse;
			AssertEquals(true, message.IsEntrySummaryQuerySuccessful);
			message = CreateResponseMessage("3", "B018888XJ5JR                                               58                   "
				+ "J18888XJ5 0000011391-013199000010000000040000000000000                  001   B "
				+ "J2XJ5 00000113      0000000000000000000000                               808001B"
				+ "J3XJ5 00000113112607071126070112546821485211210789130010715000000002400B00001011"
				+ "J5XJ5 00000113071120ESP                                                         "
				+ "J98888XJ5 00000113                                                              "
				+ "Y  8888XJ5JR00006");
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse;
			AssertEquals(false, message.IsEntrySummaryQuerySuccessful);
			message = CreateResponseMessage("3", "B001101SV9JD                                               58319                "
				+ "JA EES 061611012200PM061311115959PM                                             "
				+ "JZ011   REQUESTED TO DATE < REQUESTED FROM DATE                                 "
				+ "Y  1101SV9JD00000");
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse;
			AssertEquals(false, message.IsEntrySummaryQuerySuccessful);
			message = CreateResponseMessage("3", "B001101SV9JD                                               58315                "
				+ "JBSV9  7002293200100061911093801PM       Y2                                     "
				+ "JC1106191100      10      0            01                                       "
				+ "10ASV9  70022932 1101B00155968   0140 YY          2062911                       "
				+ "20BA  1101061911A001                                                            "
				+ "40  001 CHCH061911                 0            120    N  N                     "
				+ "47S13-147927000                                                                 "
				+ "894992500                                                                       "
				+ "90        530        2500         000         000         000                   "
				+ "Y  1101SV9JD00000");
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse;
			AssertEquals(true, message.IsEntrySummaryQuerySuccessful);
			message = CreateResponseMessage("3", "B001101SV9ER                                               58319                "
				+ "JA EES 061611012200PM061311115959PM                                             "
				+ "JZ011   REQUESTED TO DATE < REQUESTED FROM DATE                                 "
				+ "Y  1101SV9ER00000");
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse;
			AssertEquals(false, message.IsEntrySummaryQuerySuccessful);
			message = CreateResponseMessage("3", "B001101SV9ER                                               HYEDUSCMT_175303     "
				+ "JBSV9  7102388900100091316032828PM       Y2                                     "
				+ "JC1109131600      10      0            1 NO                                     "
				+ "JD2      000000000000000000000000000000000000000000089012000000009012           "
				+ "JE00001000000000000000000000000000000000000000000000000000000000000             "
				+ "Y  1101SV9ER00003");
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse;
			AssertEquals(true, message.IsEntrySummaryQuerySuccessful);
		}

		public void TestEM_MessageInterpretationForNewACEPGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var message = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			message.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_148046     10ASV9  71001752 1101B00160597   0110 XY          2092013                       1158-12345678958-123456789                     091013       IL                  20APLU1101091013A002TITANIC                                                     21ER456                                                                         2200000100CS                                                                    23MAPLUMAS3456TER                                                               318B 891                                                                        40  001 JPHK091013                  582010000000010    N                        47MJPACETAC288OSA                                                               47C58-123456789                                                                 47S58-123456789                                                                 508703105000 0000000000 0000001000 000000001000KG                               OI        TEST                                                                  DT010012AY                     895  T                                           OI        OZONE DEPLETING SUBSTANCES TEST                                       PG01001EPAODS     SRV 12345678901234     130.025         OZONE DEPLETING SUBSTA PG02PACC HU890                                                                  PG19IM    32141234                                                              PG20                                                                            PG21IM LANA                   0206456454     LANA.V@DOT.COM                     PG29KG 000000030000                                                             PG01002EPAVNE     AI  ANY0012            .               V E TEST               PG02PFAI GT001                                                                  PG07TOYOTA                             PRIUS          062012AKGJTFGTREW345GT002 PG10           V06     TEST                                                     PG143EP44534545                          2091320130000000000120000M3   M        PG19DEQ                  HAARBY BOLIGMONTERING           ALGADE 23              PG20                                     HAARBY               HH DEDK 5683      PG24EP4B    TEST REMARKS                                                        PG30R09132013    2  3019                                                        6250100000125                                                                   6249900000346                                                                   895010000000012549900000002500                                                  9000000000000 00000002625 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			AssertNoExceptionThrown(() => message.MessageBlock.Serialise());
		}

		public void TestIsBLU()
		{
			var message = (MQEDIMessage)GetNewBusinessObject();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdate;
			Assert(message.IsBLU);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			Assert(message.IsBLU);
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingProcessingResults;
			Assert(message.IsBLU);
			Assert(!message.IsACEBLU);
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			Assert(!message.IsBLU);
			Assert(!message.IsACEBLU);
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			Assert(!message.IsACEBLU);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseStatusNotification;
			Assert(!message.IsACEBLU);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			Assert(message.IsACEBLU);
		}

		public void TestACECargoReleaseResponseMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "SV9";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var aceCargoReleaseEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var outgoingMessage = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.CargoRelease);
			outgoingMessage.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148662     SE10USV9  71002677 01EI 23-45678901240800000000101101                           SE15R    ALP61325205                                       00000200BL           SE20CR B00160830                                                                Y  1101SV9SE00003";
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-4);
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_148662";
			aceCargoReleaseEntry.Messages.Add(outgoingMessage);
			var aceCargoReleaseResponse = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			aceCargoReleaseResponse.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002677 01EI 23-45678901240800000000101101                           SE15R    ALP61325205                                       00000200BL           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00003";
			aceCargoReleaseResponse.EM_MessageNum = "HYEDUSCMT_148662";
			aceCargoReleaseResponse.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-3);
			aceCargoReleaseEntry.Messages.Add(aceCargoReleaseResponse);
			var statusNotification = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			statusNotification.EM_MessageText = "B001101SV9SO                                              HYEDUSCMT_148662      SO101101SV9  71002677 0123-456789012                                   1        SO40     APLUFSDFS324234                                   00000015CS   00000000SO50040814004691NO BILL MATCH                                                   Y  1101SV9SO00000";
			statusNotification.EM_MessageNum = "HYEDUSCMT_148662";
			statusNotification.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-2);
			aceCargoReleaseEntry.Messages.Add(statusNotification);
			AssertEquals("We've seen the case when Customs sent Status Notification Message with the same message number as ACE Cargo Release Update. However, Message.ResponseMessage should be Cargo Release Response 'SX' message", ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse, outgoingMessage.ResponseMessage.EM_MessageType);
		}

		public void TestDeclaration_WhenLinkedObjectHasEmptyDeclarationPK_ShouldSkipLoadDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var message = newFactory.New<MQEDIMessage>();
			var declarationProperty = message.GetType().GetProperty("Declaration", BindingFlags.NonPublic | BindingFlags.Instance);
			var getterMethod = declarationProperty.GetGetMethod(true);
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = entryHeader.PK;
			var currentDeclarationTableHitCount = newFactory.GetTableHitCount(JobDeclarationSchema.Constants.TableName);
			var currentEntryHeaderTableHitCount = newFactory.GetTableHitCount(CusEntryHeaderSchema.Constants.TableName);
			var expectedDbHits = new Dictionary<string, int>()
			{ { JobDeclarationSchema.Constants.TableName, currentDeclarationTableHitCount + 1 }, { CusEntryHeaderSchema.Constants.TableName, currentEntryHeaderTableHitCount + 1 } };
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				getterMethod.Invoke(message, null);
			}

			var entryHeaderWithEmptyDeclarationPK = newFactory.New<CusEntryHeader>();
			entryHeaderWithEmptyDeclarationPK.CH_JE = ZGuid.Empty;
			message.EM_LinkUniqueID = entryHeaderWithEmptyDeclarationPK.PK;
			currentDeclarationTableHitCount = newFactory.GetTableHitCount(JobDeclarationSchema.Constants.TableName);
			currentEntryHeaderTableHitCount = newFactory.GetTableHitCount(CusEntryHeaderSchema.Constants.TableName);
			expectedDbHits = new Dictionary<string, int>()
			{ { JobDeclarationSchema.Constants.TableName, currentDeclarationTableHitCount }, { CusEntryHeaderSchema.Constants.TableName, currentEntryHeaderTableHitCount } };
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				getterMethod.Invoke(message, null);
			}
		}

		public void TestMessageBlock()
		{
			// Output
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			BlockControlGenerator messageBlock = message.MessageBlock;
			AssertEquals(typeof(ABIOutputBlockControlGenerator), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ABIOutputBlockControlGenerator<AABIOutputB, AABIOutputY>), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsExport;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = "B".PadRight(79) + "1Y";
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ExportOutputBlockControlGenerator<AESCommShipBXT, AESCommShipYXT>), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BIRDTransaction;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ABIOutputBlockControlGenerator<BRDAA, BRDZZ>), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ABIOutputBlockControlGenerator<AABIOutputB, AABIOutputY>), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsExport;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ExportOutputBlockControlGenerator<AESCommShipBXT, AESCommShipYXT>), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsExport;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ExportOutputBlockControlGenerator<AESCommWarnBXN, AESCommWarnYXN>), messageBlock.GetType());
			// Input
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ABIInputBlockControlGenerator), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ABIInputBlockControlGenerator<AABIInputB, AABIInputY>), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsExport;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message.EM_MessageText = "B".PadRight(79) + "1Y";
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(AESInputBlockControlGenerator), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BIRDTransaction;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ABIInputBlockControlGenerator<BRDAA, BRDZZ>), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ABIInputBlockControlGenerator<AABIInputB, AABIInputY>), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsExport;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(AESInputBlockControlGenerator), messageBlock.GetType());
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.TemporaryImportationBondEntrySummaries;
			messageBlock = message.MessageBlock;
			AssertEquals(typeof(ABIInputBlockControlGenerator), messageBlock.GetType());
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var result = Factory.New<MQEDIMessage>();
			result.EM_MessageText = "B018888XJ5                                  89             <<MSGNO PLACEHOLDER>>".PadRight(80) + "Y  8888XJ5".PadRight(80);
			return result;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<MQEDIMessage>();

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		MQEDIMessage CreateResponseMessage(ZString messageNum, ZString messageText)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incomingMessage.EM_MessageNum = messageNum;
			incomingMessage.EM_MessageText = messageText;
			incomingMessage.EM_LinkedObject = declaration;
			return incomingMessage;
		}

		MQEDIMessage CreateMessage(string receiveTransmit, string applicationIdentifier)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var result = mock.Object;
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = receiveTransmit;
			result.EM_MessageType = applicationIdentifier;
			result.EM_MessageText = @"B018888XJ5CI                                               ~15000               Y  8888XJ5CI00054";
			result.EM_LinkedObject = declaration;
			return result;
		}

		const string AcceptedER = "B018888XJ5ER                                               53972                E08888XJ5 70033754B00154240ACCEPTED - RECORDS REQUIRED             21808        E08888XJ5 70033754B00154240CERT-RELEASE CERTIFIED VIA SUMMARY      218082A5     Y  8888XJ5ER00002            000000143723";
		internal const string RejectedER = "B018888XJ5ER                                               27428                10A888891-01319900091-013199000                 9112408   XJ5 7000575221089  IL E108888XJ5 70005752   61821   SURETY REVOKED                           B001510819000000000000000000356630 00000000000000000000000000000250000000010000          E908888XJ5 70005752   52421   TRANSACTION DATA REJECTED                B00151081Y  8888XJ5ER00004            000000035663";
		const string AcceptedBN = "B018888XJ5BN                                                                    BN01EXJ570033754                                                                BN02001001 105609248581FDA PRIOR NOTICE RECEIVED 09302010001805                 Y  8888XJ5BN00002";
		const string BatchRejected = "B00                                                        B                    X0 BLOCK       1 REF ID: 8888 XJ5    AE 6009071                                 X1 FX20   FILER NOT AUTHORIZED FOR APPLICATION ID                               X1RF999   BATCH REJECTED                                                        Y           00003";
	}
}
