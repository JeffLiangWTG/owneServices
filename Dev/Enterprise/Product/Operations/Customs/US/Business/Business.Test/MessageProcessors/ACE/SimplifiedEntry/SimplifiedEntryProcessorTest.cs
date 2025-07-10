using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class SimplifiedEntryProcessorTest : ABIProcessorTest<SimplifiedEntryProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestACECargoReleaseReceipients_EnableCRLOnly()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "~AA";
			staff1.GS_EmailAddress = "blah@com.au";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "~BB";
			staff2.GS_EmailAddress = "blah2@com.au";
			Factory.Save();

			var declaration = GetDeclaration();
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.JE_GS_NKCusAgent = staff2.GS_Code;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148784";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148784     SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE16COA 6020 09091300000450                                                     SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE20CR B00160864                                                                Y  1101SV9SE00010";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			message.EM_SystemCreateUser = staff1.GS_Code;

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.Messages.Add(message);
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148784";
			responseMessage.EM_Status = EDIMessage.Status.Queued;

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00010";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Response for"); }));
			Assert(email.Body.Contains("SE DATA ACCEPTED"));
			AssertEquals(true, email.Recipients.Contains(staff1.GS_EmailAddress));
			AssertEquals(false, email.Recipients.Contains(staff2.GS_EmailAddress));
		}

		public void TestACECargoReleaseReceipients_SendErrorOnly()
		{
			var branch = GlbBranch.CurrentBranch;

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "II";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "I1";
			staff.GS_LoginName = "I1";
			staff.GS_EmailAddress = "II@pretend.email.com";
			Factory.Save();

			var abiGroupNotification = new Registry.Business.Customs.ManifestGroupNotification();
			abiGroupNotification.SendErrorOnly = true;
			abiGroupNotification.SendGroupPK = group.PK;
			abiGroupNotification.SendMode = Enterprise.Core.Constants.EmailTo.StaffMemberAndNominatedGroup;

			var declaration1 = GetDeclaration();
			declaration1.US_EnableENS = true;
			declaration1.US_EnableCRL = false;
			declaration1.JE_GS_NKCusAgent = staff.GS_Code;

			declaration1.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148784";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148784     SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE16COA 6020 09091300000450                                                     SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE20CR B00160864                                                                Y  1101SV9SE00010";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			message.EM_SystemCreateUser = staff.GS_Code;

			var seEntry = declaration1.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.Messages.Add(message);
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148784";
			responseMessage.EM_Status = EDIMessage.Status.Queued;

			using (USCustomsDataRegistry.Instance.ABIMessagesGroup.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, abiGroupNotification))
			{
				var declaration2 = GetDeclaration();
				declaration2.JE_GS_NKCusAgent = staff.GS_Code;

				message2.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_145698     " +
	"SE10ASV9  71001133 01EI 23-45678901240800000100001101                           " +
	"SE9011037MISSING SELLER                                                         " +
	"SE9011035MISSING BUYER                                                          " +
	"SE9011036MISSING CONSIGNEE                                                      " +
	"SE9011038MISSING MANUFACTURER                                                   " +
	"SE15M    00122222222                                                            " +
	"SE15H    HSE021413G                                        00000100CS           " +
	"SE20CR B00159762                                                                " +
	"SE40001                                                                         " +
	"SE9011060MISSING COUNTRY OF ORIGIN CODE                                         " +
	"SE9011038MISSING MANUFACTURER                                                   " +
	"SE9011037MISSING SELLER                                                         " +
	"SE9011036MISSING CONSIGNEE                                                      " +
	"SE9011035MISSING BUYER                                                          " +
	"SE60          0000010000                                                        " +
	"SE9011063MISSING TARIFF NUMBER                                                  " +
	"SE40002                                                                         " +
	"SE9011038MISSING MANUFACTURER                                                   " +
	"SE9011037MISSING SELLER                                                         " +
	"SE40003AU                                                                       " +
	"SE9011037MISSING SELLER                                                         " +
	"SE9001   SE DATA REJECTED                                                       " +
	"Y  1101SV9SX00006";

				Factory.Save();
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIIncomingMessageProcessor().ExecuteBatch();

				message2.Reload();
				AssertEquals("RCV", message2.EM_Status);

				var email1 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject.Contains("ACE Cargo Release Response (Failure)"); }));
				AssertNotNull(email1);

				responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE9011178LOCATION OF GOODS IS REQUIRED                                          SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00010";
				Factory.Save();
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIIncomingMessageProcessor().ExecuteBatch();
				responseMessage.Reload();

				var email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject.Contains("ACE Cargo Release Response for"); }));
				AssertNull(email2);
			}
		}

		public void TestACECargoReleaseReceipients_EnableENSOnly()
		{
			AddRefCusCodeTypeAndRefDataGrouping();
			AddRefCusCodeList("178", @"An Add/Replace/Update transaction is submitted with entry type 06 or entry type 21 and location of goods is not reported.");

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "~AA";
			staff1.GS_EmailAddress = "blah@com.au";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "~BB";
			staff2.GS_EmailAddress = "blah2@com.au";
			Factory.Save();

			var declaration = GetDeclaration();
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.JE_GS_NKCusAgent = staff1.GS_Code;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148784";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148784     SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE16COA 6020 09091300000450                                                     SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE20CR B00160864                                                                Y  1101SV9SE00010";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			message.EM_SystemCreateUser = staff1.GS_Code;

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.Messages.Add(message);
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148784";
			responseMessage.EM_Status = EDIMessage.Status.Queued;

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE9011178LOCATION OF GOODS IS REQUIRED                                          SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00010";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Response for"); }));

			Assert(email.Body.Contains("SE DATA ACCEPTED"));
			Assert("The description of error code 178", email.Body.Contains(@"An Add/Replace/Update transaction is submitted with entry type 06 or entry type 21 and location of goods is not reported."));
			AssertEquals(true, email.Recipients.Contains(staff1.GS_EmailAddress));
			AssertEquals(false, email.Recipients.Contains(staff2.GS_EmailAddress));
		}

		public void TestFailedMessageCorrectlyProcessed()
		{
			AddRefCusCodeTypeAndRefDataGrouping();
			AddRefCusCodeList("036", @"An Add/Replace transaction is submitted without a Consignee identifier.");
			AddRefCusCodeList("037", @"An Add/Replace transaction is submitted without a Seller name / address or identifier.");
			AddRefCusCodeList("038", @"An Add/Replace transaction is submitted without a Manufacturer name and address and entry type is not 86.");
			Factory.Save();

			var declaration = GetDeclaration();

			message2.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_145698     " +
"SE10ASV9  71001133 01EI 23-45678901240800000100001101                           " +
"SE9011037MISSING SELLER                                                         " +
"SE9011035MISSING BUYER                                                          " +
"SE9011036MISSING CONSIGNEE                                                      " +
"SE9011038MISSING MANUFACTURER                                                   " +
"SE15M    00122222222                                                            " +
"SE15H    HSE021413G                                        00000100CS           " +
"SE20CR B00159762                                                                " +
"SE40001                                                                         " +
"SE9011060MISSING COUNTRY OF ORIGIN CODE                                         " +
"SE9011038MISSING MANUFACTURER                                                   " +
"SE9011037MISSING SELLER                                                         " +
"SE9011036MISSING CONSIGNEE                                                      " +
"SE9011035MISSING BUYER                                                          " +
"SE60          0000010000                                                        " +
"SE9011063MISSING TARIFF NUMBER                                                  " +
"SE40002                                                                         " +
"SE9011038MISSING MANUFACTURER                                                   " +
"SE9011037MISSING SELLER                                                         " +
"SE40003AU                                                                       " +
"SE9011037MISSING SELLER                                                         " +
"SE9001   SE DATA REJECTED                                                       " +
"Y  1101SV9SX00006";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			message2.Reload();
			AssertEquals("RCV", message2.EM_Status);

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Reload();
			AssertEquals(entryHeader, message2.EM_LinkedObject);
			AssertEquals(EM_MessageSubTypeList.Codes.ACECargoReleaseAdd, message2.EM_MessageSubType);
			AssertEquals(ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd, entryHeader.CH_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Response (Failure)"); }));
			AssertNotNull(email);
			Assert("The description of error code 038", email.Body.Contains(@"An Add/Replace transaction is submitted without a Manufacturer name and address and entry type is not 86."));
			Assert("The description of error code 037", email.Body.Contains(@"An Add/Replace transaction is submitted without a Seller name / address or identifier."));
			Assert("The description of error code 036", email.Body.Contains(@"An Add/Replace transaction is submitted without a Consignee identifier."));
		}

		void AddRefCusCodeTypeAndRefDataGrouping(string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ACECargoReleaseSEInputValidationRules,
			string description = "ACE Cargo Release (SE) Input Validation Rules")
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			helper.CreateNewOrGetExistingCusCodeType(codeType, description, refDataGrouping.ZZZ_DataGrouping);
		}

		void AddRefCusCodeList(string code, string description, string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ACECargoReleaseSEInputValidationRules)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, codeType, code, description, new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		}

		public void TestWarningMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec.US_EntryFilerCode = "SV9";
			dec.ImportEntryNumber = "71001919";

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3920100000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148191";
			message.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_164692     SE10ASV9  40000349 01EI 58-12345678911800000100001101  1101                     SE15RAPLUMST081515A                                        00000100CS   N       SE20CR B00164172                                                                SE40001HK CYCLOHEXYLDIAZENIUMDIOXY                                              SE6029021100000000010000                                                        OI        CYCLOHEXYLDIAZENIUMDIOXY                                              PG01001EPAPS1   Y                        130.027                                SE6036010000000000000100                                                        PG02PCAS 1234                                                                   PG04YCYCLOHEXYLDIAZENIUMDIOXY                                            1000000PG07ACME                                                                        PG14 EP8241-420                                                                 PG19EPN   012345CHN002                                                          PG19CB                   U.S. DEMO COMPANY               184 TEST STREET        PG20                                     CHICAGO              IL US60056        PG21CB CRAIG SEELIG           2154441234     CRAIG.SEELIG@TEST.COM              PG55NP                                                                          PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21IM CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG55CI                                                                          PG19OV                   MARITIME INDUSTRIAL SHIPPERS    600 PORT STREET        PG20                                     NEWARK               NJ US20034        PG21OV JAY SMITH              14101123456    JAY.SMITH@MARITIMESHIPPERS.COM     PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROADPG20                                     HONG KONG               HK             PG21DEQJOHN SMITH             5555555555     JOHN.SMITH@COMPANY.COM             PG19LG                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21LG CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG22 944         CI EP3                                                         PG261000000100000DR                                                             PG262                                                                           PG29KG 000000100000                                                             Y  1101SV9SX00044                                                               ";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			var entryHeader = dec.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Messages.Add(message);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148191";//SX now returns user data in B block

			responseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_164692     SE10ASV9  71012932 01EI 58-12345678911800000100001101  1101                     SE15RAPLUMST081515A                                        00000100CS   N       SE20CR B00164172                                                                SE40001HK CYCLOHEXYLDIAZENIUMDIOXY                                              SE6029021100000000010000                                                        OI        CYCLOHEXYLDIAZENIUMDIOXY                                              PG01001EPAPS1   Y                        130.027                                SE9013PH6   MISSING DIS DOCUMENTATION                                           PG02PCAS 1234                                                                   PG04YCYCLOHEXYLDIAZENIUMDIOXY                                            1000000PG07ACME                                                                        PG14 EP8241-420                                                                 PG19EPN   012345CHN002                                                          PG19CB                   U.S. DEMO COMPANY               184 TEST STREET        PG20                                     CHICAGO              IL US60056        PG21CB CRAIG SEELIG           2154441234     CRAIG.SEELIG@TEST.COM              PG55NP                                                                          PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21IM CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG55CI                                                                          PG19OV                   MARITIME INDUSTRIAL SHIPPERS    600 PORT STREET        PG20                                     NEWARK               NJ US20034        PG21OV JAY SMITH              14101123456    JAY.SMITH@MARITIMESHIPPERS.COM     PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROADPG20                                     HONG KONG               HK             PG21DEQJOHN SMITH             5555555555     JOHN.SMITH@COMPANY.COM             PG19LG                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21LG CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG22 944         CI EP3                                                         PG261000000100000DR                                                             PG262                                                                           PG29KG 000000100000                                                             SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00044                                                               ";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseAdd, responseMessage.EM_MessageSubType);

			entryHeader.Reload();
			AssertEquals(entryHeader, responseMessage.EM_LinkedObject);
			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseAdd, entryHeader.CH_Status);
		}

		public void TestSetMessageSubType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "SV9";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.ImportEntryNumber = "71005415";
			declaration.JE_MasterBill = "FSDFS324234";
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(declaration.MessageInitiator);
			Factory.Save();
			var entrySummary = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = entrySummary.Messages[0];
			message.EM_MessageNum = "HYEDUSCMT_151199";

			AssertEquals("Precondition - entry should have US_CRLCertStatus = 'P'", CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending, entrySummary.US_CRLCertStatus);

			var ensResponse = Factory.New<MQEDIMessage>();
			ensResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			ensResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ensResponse.EM_Status = EDIMessage.Status.Queued;
			ensResponse.EM_MessageNum = "HYEDUSCMT_151199";
			ensResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			ensResponse.EM_MessageText =
"B001101SV9AX                                               HYEDUSCMT_151199     " +
"E0 SUMMRY 000001 REF ID: SV9 71005415 B00162063                                 " +
"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100541500100B00162063   " +
"Y  1101SV9AX00002";

			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "HYEDUSCMT_151199";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageText =
"B001101SV9SX                                               HYEDUSCMT_151199     " +
"SE10ASV9  71005415 01EI 23-45678901210800000000901101  1101                     " +
"SE11 040714A001    TITANIC             0098L                                    " +
"SE15RAPLUFSDFS324234                                       00000015CS           " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00000";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals("A", EM_MessageSubTypeList.Codes.ACECargoReleaseAdd, message.EM_MessageSubType);

			ensResponse.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageText =
				"B001101SV9SX                                               HYEDUSCMT_151199     " +
				"SE10USV9  71005415 01EI 23-45678901210800000000901101  1101                     " +
				"SE11 040714A001    TITANIC             0098L                                    " +
				"SE15RAPLUFSDFS324234                                       00000015CS           " +
				"SE9002   SE DATA ACCEPTED                                                       " +
				"Y  1101SV9SX00000";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals("U", EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate, message.EM_MessageSubType);

			ensResponse.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageText =
				"B001101SV9SX                                               HYEDUSCMT_151199     " +
				"SE10RSV9  71005415 01EI 23-45678901210800000000901101  1101                     " +
				"SE11 040714A001    TITANIC             0098L                                    " +
				"SE15RAPLUFSDFS324234                                       00000015CS           " +
				"SE9002   SE DATA ACCEPTED                                                       " +
				"Y  1101SV9SX00000";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals("R", EM_MessageSubTypeList.Codes.ACECargoReleaseReplace, message.EM_MessageSubType);

			ensResponse.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageText =
				"B001101SV9SX                                               HYEDUSCMT_151199     " +
				"SE10RSV9  71005415 01EI 23-45678901210800000000901101  1101                     " +
				"SE11 040714A001    TITANIC             0098L                                    " +
				"SE15RAPLUFSDFS324234                                       00000015CS           " +
				"SE9002   SE DATA ACCEPTED                                                       " +
				"Y  1101SV9SX00000";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals("R", EM_MessageSubTypeList.Codes.ACECargoReleaseReplace, message.EM_MessageSubType);

			ensResponse.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageText =
				"B001101SV9SX                                               HYEDUSCMT_151199     " +
				"SE10DSV9  71005415 01EI 23-45678901210800000000901101  1101                     " +
				"SE11 040714A001    TITANIC             0098L                                    " +
				"SE15RAPLUFSDFS324234                                       00000015CS           " +
				"SE9002   SE DATA ACCEPTED                                                       " +
				"Y  1101SV9SX00000";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals("D", EM_MessageSubTypeList.Codes.ACECargoReleaseDelete, message.EM_MessageSubType);
		}

		public void TestProcessFailedMessage()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec.US_EntryFilerCode = "SV9";
			dec.ImportEntryNumber = "71001919";
			dec.JE_GS_NKCusAgent = staffZ1.GS_Code;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3920100000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148191";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148191     SE10ASV9  71001919 01EI 58-12345678940800000001001101                           SE15R    61143545342                                       00000005BL           SE20***CR                                                                       SE30CN                                    EI 58-123456789                       SE30BY ACE TEST IMPORTER 1                                                      SE35155000 TOWERS CRESCENT DRIVE         15NWD ENTERPRISES 12A                  SE36VIENNA                                      221826224      US               SE30SE GABLER MASCHINENBAU GMBH                                                 SE3515NIELS-BOHR-RING 5 A                                                       SE36LUEBECK                                     23568          DE               SE40001FR                                                                       SE50MF ACE SUPPLIER AU OFFICE                                                   SE5515100 BROAD STREET                                                          SE56SYDNEY                                      AA77BB         AU               SE6039201000000000000100                                                        Y  1101SV9SE00015";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			var entryHeader = dec.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Messages.Add(message);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148191";//SX now returns user data in B block

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10ASV9  71001919 01EI 58-12345678940800000001001101                           SE9011018CONTINUOUS BOND NOT ON FILE                                            SE15R    61143545342                                       00000005BL           SE20***CR                                                                       SE9011033INVALID REFERENCE QUALIFIER                                            SE9001   SE DATA REJECTED                                                       Y  1101SV9SX00015                                                               ";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseAdd, responseMessage.EM_MessageSubType);

			entryHeader.Reload();
			AssertEquals(entryHeader, responseMessage.EM_LinkedObject);
			AssertEquals(ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd, entryHeader.CH_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Response (Failure)"); }));
			AssertNotNull(email);
		}

		public void TestProcessSuccessfulResponse()
		{
			var declaration = GetNotMergedDeclaration("71002867");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148588";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148588     SE10ASV9  71002867 01EI 23-45678901240800000001001101                           SE15M    00591325207                                                            SE15H    HAWB001                                           00000100AP           SE20CR B00160854                                                                SE30CN                                    EI 23-456789012                       SE30BY SIMPLIFIED ENTRY TEST IMPORTER                                           SE3515123 MAIN STREET                                                           SE36LOS ANGELES                                 60111          US               SE30SE YACHT BATTERY CO                                                         SE3515ROOM 5F-1, 212 BA-DER RD           15SEC 3                                SE36TAIPEI                                      2018           TW               SE40001GB                                                                       SE50MF 0UANGZHOU FOREIGN TRADE BAIYUN*                                          SE5515LTD                                154/F. NO.218 GUANGYUAN ZHONG 343      SE56ROAD GUANGZHOU CHINA                        21512          GB               SE6036010000000000000100                                                        Y  1101SV9SE00016";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Messages.Add(message);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10ASV9  71002867 01EI 23-45678901240800000001001101                           SE15M    00591325207                                                            SE15H    HAWB001                                           00000100AP           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00016";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseAdd, responseMessage.EM_MessageSubType);
			AssertEquals("Message Number should be set", "HYEDUSCMT_148588", responseMessage.EM_MessageNum);

			entryHeader.Reload();
			AssertEquals(entryHeader, responseMessage.EM_LinkedObject);
			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseAdd, entryHeader.CH_Status);

			var replaceMessage = mock.Object;
			replaceMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			replaceMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			replaceMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			replaceMessage.EM_MessageNum = "HYEDUSCMT_148589";
			replaceMessage.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148589     SE10RSV9  71002867 01EI 23-45678901240800000001001101                           SE13VISHNEVETSKAYA                                             1                SE15M    00591325207                                                            SE15H    HAWB001                                                                SE16COA 4007 09091300000150                                                     SE20CR B00160854                                                                SE30CN                                    EI 23-456789012                       SE30BY SIMPLIFIED ENTRY TEST IMPORTER                                           SE3515123 MAIN STREET                                                           SE36LOS ANGELES                                 60111          US               SE30SE YACHT BATTERY CO                                                         SE3515ROOM 5F-1, 212 BA-DER RD           15SEC 3                                SE36TAIPEI                                      2018           TW               SE40001GB                                                                       SE50MF 0UANGZHOU FOREIGN TRADE BAIYUN*                                          SE5515LTD                                154/F. NO.218 GUANGYUAN ZHONG 343      SE56ROAD GUANGZHOU CHINA                        21512          GB               SE6036010000000000000100                                                        Y  1101SV9SE00018";
			replaceMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseReplace;
			entryHeader.Messages.Add(replaceMessage);

			var responseReplaceMessage = Factory.New<MQEDIMessage>();
			responseReplaceMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseReplaceMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseReplaceMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseReplaceMessage.EM_Status = EDIMessage.Status.Queued;
			responseReplaceMessage.EM_MessageText = "B011101SV9SX                                                                    SE10RSV9  71002867 01EI 23-45678901240800000001001101                           SE15M    00591325207                                                            SE15H    HAWB001                                                                SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00018";
			responseReplaceMessage.EM_MessageNum = "HYEDUSCMT_148589";//sx returns user data in B
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			responseReplaceMessage.Reload();
			AssertEquals("RCV", responseReplaceMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseReplace, responseReplaceMessage.EM_MessageSubType);
			AssertEquals("Message Number should be set", "HYEDUSCMT_148589", responseReplaceMessage.EM_MessageNum);
		}

		public void TestProcessResponseWithoutMessageNum()
		{
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10ASV9  71001919 01EI 58-12345678940800000001001101                           SE9011018CONTINUOUS BOND NOT ON FILE                                            SE15R    61143545342                                       00000005BL           SE20***CR                                                                       SE9011033INVALID REFERENCE QUALIFIER                                            SE9001   SE DATA REJECTED                                                       Y  1101SV9SX00015                                                               ";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();
			AssertContains("", responseMessage.Logs.MostRecentLog.ReferenceFreeText);
		}

		public void TestUpdateTrackingStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = false;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71002867";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var ttb = invoiceLine.TTBLines.AddNew();
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			var lacey = invoiceLine.LaceyActLines.AddNew();
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			var nmfs = invoiceLine.NMFSLines.AddNew();
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var nhtsa = invoiceLine.NHTSALines.AddNew();
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fws = invoiceLine.FWSHeaders.AddNew();
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var fsis = invoiceLine.FSISLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vne = invoiceLine.VehicleLines.AddNew();
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var pst = invoiceLine.PSTLines.AddNew();
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var atf = invoiceLine.ATFLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var ams = invoiceLine.AMSLines.AddNew();
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;

			invoiceLine.US_DDTCExemptionCode = "1";
			invoiceLine.US_TSCACertification = "1";
			invoiceLine.US_FDAContactName = "1";

			ttb.US_PermitNumber = "1";
			lacey.US_PGACommercialDescription = "1";
			nmfs.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfs.US_AMLRPermitNumber = "1";
			nhtsa.US_CertifyingIndividual = "1";
			fws.US_CartonQty = 1;
			fsis.US_ProductID = "ADF";
			fda.US_BrandName = "AAA";
			vne.US_BodyCode = "12";
			pst.US_BrandName = "PST";
			atf.US_AECAExemptionCode = "AAA";
			ams.US_IntendedUseCode = "AAA";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Deleted;
			ttb.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			fws.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			fsis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			fda.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = (EntryHeaderMessageSendingAction)actions[0];

			var message = new SimplifiedEntryMessageBuilder(entryHeader, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action)).PopulateMessage();
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
			declaration.US_PGAReplaceUpdateNeeded = YesNoList.Codes.Yes;
			Factory.Save();
			message.EM_MessageNum = "HYEDUSCMT_148588";

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10ASV9  71002867 01EI 23-45678901240800000001001101                           SE15M    00591325207                                                            SE15H    HAWB001                                           00000100AP           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00016";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			ttb = newFactory.Load<TTBLine>(ttb.PK);
			lacey = newFactory.Load<PGA>(lacey.PK);
			nmfs = newFactory.Load<NMFSLine>(nmfs.PK);
			nhtsa = newFactory.Load<NHTSAHeader>(nhtsa.PK);
			fws = newFactory.Load<FWSHeader>(fws.PK);
			fsis = newFactory.Load<USInvoiceLineFSISLine>(fsis.PK);
			fda = newFactory.Load<ACEFDA>(fda.PK);
			vne = newFactory.Load<Vehicle>(vne.PK);
			pst = newFactory.Load<Pesticide>(pst.PK);
			atf = newFactory.Load<ATF>(atf.PK);
			ams = newFactory.Load<AMS>(ams.PK);

			CombineAssertions(() =>
			{
				AssertEquals("US_PGAReplaceUpdateNeeded", ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
				AssertEquals("TTB", PGATrackingStatusList.Codes.Deleted, ttb.US_TrackingStatus);
				AssertEquals("LACEY", PGATrackingStatusList.Codes.Added, lacey.US_TrackingStatus);
				AssertEquals("NMFS", PGATrackingStatusList.Codes.Added, nmfs.US_TrackingStatus);
				AssertEquals("NHTSA", PGATrackingStatusList.Codes.Added, nhtsa.US_TrackingStatus);
				AssertEquals("FWS", PGATrackingStatusList.Codes.Added, fws.US_TrackingStatus);
				AssertEquals("FSIS", PGATrackingStatusList.Codes.Deleted, fsis.US_TrackingStatus);
				AssertEquals("FDA", PGATrackingStatusList.Codes.Added, fda.US_TrackingStatus);
				AssertEquals("VNE", PGATrackingStatusList.Codes.Added, vne.US_TrackingStatus);
				AssertEquals("PST", PGATrackingStatusList.Codes.Added, pst.US_TrackingStatus);
				AssertEquals("ATF", PGATrackingStatusList.Codes.Added, atf.US_TrackingStatus);
				AssertEquals("AMS", PGATrackingStatusList.Codes.Added, ams.US_TrackingStatus);
				AssertEquals("DDTC", PGATrackingStatusList.Codes.Added, invoiceLine.US_DDTCTrackingStatus);
				AssertEquals("TSCA", ZString.Empty, invoiceLine.US_TSCATrackingStatus);
				AssertEquals("ODS", PGATrackingStatusList.Codes.Deleted, invoiceLine.US_ODSTrackingStatus);
			});
		}

		public void TestUpdateTrackingStatusForDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71002867";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var ttb = invoiceLine.TTBLines.AddNew();
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			var lacey = invoiceLine.LaceyActLines.AddNew();
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			var nmfs = invoiceLine.NMFSLines.AddNew();
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var nhtsa = invoiceLine.NHTSALines.AddNew();
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fws = invoiceLine.FWSHeaders.AddNew();
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var fsis = invoiceLine.FSISLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vne = invoiceLine.VehicleLines.AddNew();
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var pst = invoiceLine.PSTLines.AddNew();
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var atf = invoiceLine.ATFLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var ams = invoiceLine.AMSLines.AddNew();
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;

			invoiceLine.US_DDTCExemptionCode = "1";
			invoiceLine.US_TSCACertification = "1";
			invoiceLine.US_FDAContactName = "1";

			ttb.US_PermitNumber = "1";
			lacey.US_PGACommercialDescription = "1";
			nmfs.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfs.US_AMLRPermitNumber = "1";
			nhtsa.US_CertifyingIndividual = "1";
			fws.US_CartonQty = 1;
			fsis.US_ProductID = "ADF";
			fda.US_BrandName = "AAA";
			vne.US_BodyCode = "12";
			pst.US_BrandName = "PST";
			atf.US_AECAExemptionCode = "AAA";
			ams.US_IntendedUseCode = "AAA";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Deleted;
			ttb.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			fws.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			fsis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			fda.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
			declaration.US_PGAReplaceUpdateNeeded = YesNoList.Codes.Yes;
			Factory.Save();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var action = (EntryHeaderMessageSendingAction)actions[0];

			var message = new SimplifiedEntryMessageBuilder(entryHeader, UpdateActionCode.Delete, ACEEntrySummaryMessageSendingOption.New(action)).PopulateMessage();
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseDelete;
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseDelete;
			Factory.Save();
			message.EM_MessageNum = "HYEDUSCMT_148588";

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";

			responseMessage.EM_MessageText =
					"B011101SV9SX                                                                    " +
					"SE10ASV9  71002867 01EI 23-45678901240800000001001101                           " +
					"SE15M    00591325207                                                            " +
					"SE15H    HAWB001                                           00000100AP           " +
					"SE9002   SE DATA ACCEPTED                                                       " +
					"Y  1101SV9SX00016";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			ttb = newFactory.Load<TTBLine>(ttb.PK);
			lacey = newFactory.Load<PGA>(lacey.PK);
			nmfs = newFactory.Load<NMFSLine>(nmfs.PK);
			nhtsa = newFactory.Load<NHTSAHeader>(nhtsa.PK);
			fws = newFactory.Load<FWSHeader>(fws.PK);
			fsis = newFactory.Load<USInvoiceLineFSISLine>(fsis.PK);
			fda = newFactory.Load<ACEFDA>(fda.PK);
			vne = newFactory.Load<Vehicle>(vne.PK);
			pst = newFactory.Load<Pesticide>(pst.PK);
			atf = newFactory.Load<ATF>(atf.PK);
			ams = newFactory.Load<AMS>(ams.PK);

			CombineAssertions(() =>
			{
				AssertEquals("US_PGAReplaceUpdateNeeded", ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
				AssertEquals("TTB", ZString.Empty, ttb.US_TrackingStatus);
				AssertEquals("LACEY", ZString.Empty, lacey.US_TrackingStatus);
				AssertEquals("NMFS", ZString.Empty, nmfs.US_TrackingStatus);
				AssertEquals("NHTSA", ZString.Empty, nhtsa.US_TrackingStatus);
				AssertEquals("FWS", ZString.Empty, fws.US_TrackingStatus);
				AssertEquals("FSIS", ZString.Empty, fsis.US_TrackingStatus);
				AssertEquals("FDA", ZString.Empty, fda.US_TrackingStatus);
				AssertEquals("VNE", ZString.Empty, vne.US_TrackingStatus);
				AssertEquals("PST", ZString.Empty, pst.US_TrackingStatus);
				AssertEquals("ATF", ZString.Empty, atf.US_TrackingStatus);
				AssertEquals("AMS", ZString.Empty, ams.US_TrackingStatus);
				AssertEquals("DDTC", ZString.Empty, invoiceLine.US_DDTCTrackingStatus);
				AssertEquals("TSCA", ZString.Empty, invoiceLine.US_TSCATrackingStatus);
				AssertEquals("ODS", ZString.Empty, invoiceLine.US_ODSTrackingStatus);
			});
		}

		public void TestProcessWhenCertifiedFromEntrySummary()
		{
			var declaration = GetNotMergedDeclaration("71002958");
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			Assert("Precondition", actions[0].IsEntrySummary);
			actions[0].US_SendMessage = true;
			actions[0].US_CertifyCargoRelease = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0].EM_MessageNum = "HYEDUSCMT_161297";
			Factory.Save();

			Assert("Precondition", declaration.ActiveEntryHeaders.EntrySummaryEntry.IsCargoReleaseBeingCertified);

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_161297";//sx returns user data in b

			responseMessage.EM_MessageText = "B001101SV9SX                                               HYEDUSCMT_161297     SE10ASV9  71008484 01EI 58-12345678910800000050001101  1101                     SE15RAPLU324897324                                         00000001PK           SE20CR B00163009                                                                SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00000";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseAdd, responseMessage.EM_MessageSubType);
			AssertEquals("Attached to SE entry", seEntry.PK, responseMessage.EM_LinkUniqueID);

			declaration.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();
			Assert(declaration.HasCargoReleaseBeenCertified);
		}

		public void TestQuotaLineStoreInCusDisposition_SXMessage()
		{
			var declaration = GetNotMergedDeclaration("71001919");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			Assert("Precondition", actions[0].IsEntrySummary);
			actions[0].US_SendMessage = true;
			actions[0].US_CertifyCargoRelease = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0].EM_MessageNum = "HYEDUSCMT_148191";
			Factory.Save();

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148191";
			responseMessage.EM_MessageText =
"B011101SV9SX                                               HYEDUSCMT_148191     " +
"SE10ASV9  71001919 01EI 58-12345678911800000100001101  1101                     " +
"SE15RAPLUMST081515A                                        00000100CS   N       " +
"SE20CR B00386204                                                                " +
"SE40001IN CREAMY PEANUT BUTTER                                                  " +
"SE9013Q10LINE SUBJECT TO QUOTA                                                  " +
"SE9013Q08QUOTA PROCESS PENDING                                                  " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00010                                                               ";

			AddRefCusCodeTypeAndRefDataGrouping("USQTA", "US Quota Dispositions");
			AddRefCusCodeList("Q10", "Line Subject Quota", "USQTA");
			AddRefCusCodeList("Q08", "Quota Process Pending", "USQTA");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			responseMessage.Reload();
			declaration.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();
			var expectedValue = new Dictionary<ZString, ZString[]>();
			expectedValue.Add("Q10", new ZString[] { "1", "Q10", ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse, "LINE SUBJECT TO QUOTA" });
			expectedValue.Add("Q08", new ZString[] { "2", "Q08", ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse, "QUOTA PROCESS PENDING" });
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("CusDisposition count", 2, entryHeader.MergedLines[0].QuotaDispositions.Count);
			AssertQuotaStoreInCusDisposition(entryHeader.MergedLines[0], responseMessage, expectedValue);
		}

		public void TestProcessBLUResponseForACECargoReleaseType()
		{
			var declaration = GetNotMergedDeclaration("71002958");
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148784";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148784     SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE16COA 6020 09091300000450                                                     SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE20CR B00160864                                                                Y  1101SV9SE00010";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.Messages.Add(message);
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148784";//sx returns user data in b

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00010";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate, responseMessage.EM_MessageSubType);
			AssertEquals("Message Number should be set", "HYEDUSCMT_148784", responseMessage.EM_MessageNum);
			AssertEquals("Should be linked to declaration", seEntry, responseMessage.EM_LinkedObject);
			AssertEquals(seEntry.PK, responseMessage.EM_LinkUniqueID);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Response for"); }));
			Assert(email.Body.Contains("SE DATA ACCEPTED"));
		}

		public void TestProcessBLUResponseWhenENSEntryExists()
		{
			var declaration = GetNotMergedDeclaration("71002958");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148784";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148784     SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE16COA 6020 09091300000450                                                     SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE20CR B00160864                                                                Y  1101SV9SE00010";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.Messages.Add(message);
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148784";
			responseMessage.EM_Status = EDIMessage.Status.Queued;

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00010";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate, responseMessage.EM_MessageSubType);
			AssertEquals("Message Number should be set", "HYEDUSCMT_148784", responseMessage.EM_MessageNum);
			AssertEquals("Should be linked to declaration", seEntry, responseMessage.EM_LinkedObject);
			AssertEquals(seEntry.PK, responseMessage.EM_LinkUniqueID);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Response for"); }));
			Assert(email.Body.Contains("SE DATA ACCEPTED"));
		}

		public void TestProcessBLUResponseWhenOnlyCLREntryExists()
		{
			var declaration = GetNotMergedDeclaration("71002958");
			declaration.US_EnableENS = false;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			AssertNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(seEntry);

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_148784";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148784     SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE16COA 6020 09091300000450                                                     SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE20CR B00160864                                                                Y  1101SV9SE00010";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;

			seEntry.Messages.Add(message);
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148784";

			responseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00010";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate, responseMessage.EM_MessageSubType);
			AssertEquals("Message Number should be set", "HYEDUSCMT_148784", responseMessage.EM_MessageNum);
			AssertEquals("Should be linked to declaration", seEntry, responseMessage.EM_LinkedObject);
			AssertEquals(seEntry.PK, responseMessage.EM_LinkUniqueID);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Response for"); }));
			Assert(email.Body.Contains("SE DATA ACCEPTED"));
		}

		public void TestProcessWaitingReviewMessage()
		{
			var declaration = GetNotMergedDeclaration("71004798");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_150158";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_150158     SE10RSV9  71004798 01EI 58-12345678940800000100001101                           SE11       B815                        132                                      SE13CRAIG SEELIG                            215-555-1212       1                SE15M    0011212123                                                             SE15H    HOUSE1                                            00001000CS           SE20CR B00161503                                                                SE30MF ACE TEST SUPPLIER                                                        SE3515P O BOX 288                                                               SE36OSAKA                                       530-01         JP               SE30CN                                    EI 58-123456789                       SE30BY ACE TEST IMPORTER 1                                                      SE35155000 TOWERS CRESCENT DRIVE         15NWD ENTERPRISES 12A                  SE36PHILLY                                      19444          US               SE30SE ACE TEST SUPPLIER                                                        SE3515P O BOX 288                                                               SE36OSAKA                                       530-01         JP               SE40001AU                                                                       SE6039209950000000010000                                                        Y  1101SV9SE00018";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Messages.Add(message);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_150158";

			responseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_150158     SE10RSV9  71004798 01EI 58-12345678940800000100001101                           SE11       B815                        132                                      SE15M    0011212123                                                             SE15H    HOUSE1                                            00001000CS           SE9004   REPLACE REQUEST PENDING                                                Y  1101SV9SX00018";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseAdd, responseMessage.EM_MessageSubType);

			entryHeader.Reload();
			AssertEquals(entryHeader, responseMessage.EM_LinkedObject);
			AssertEquals("Entry Status", ImportMessageStatusList.Codes.CancellationRequestPending, entryHeader.CH_Status);
		}

		public void TestProcessWaitingReviewMessageWithRRP()
		{
			var declaration = GetNotMergedDeclaration("71004798");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_150158";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_150158     SE10RSV9  71004798 01EI 58-12345678940800000100001101                           SE11       B815                        132                                      SE13CRAIG SEELIG                            215-555-1212       1                SE15M    0011212123                                                             SE15H    HOUSE1                                            00001000CS           SE20CR B00161503                                                                SE30MF ACE TEST SUPPLIER                                                        SE3515P O BOX 288                                                               SE36OSAKA                                       530-01         JP               SE30CN                                    EI 58-123456789                       SE30BY ACE TEST IMPORTER 1                                                      SE35155000 TOWERS CRESCENT DRIVE         15NWD ENTERPRISES 12A                  SE36PHILLY                                      19444          US               SE30SE ACE TEST SUPPLIER                                                        SE3515P O BOX 288                                                               SE36OSAKA                                       530-01         JP               SE40001AU                                                                       SE6039209950000000010000                                                        Y  1101SV9SE00018";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseReplace;

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Messages.Add(message);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_150158";

			responseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_150158     SE10RSV9  71004798 01EI 58-12345678940800000100001101                           SE11       B815                        132                                      SE15M    0011212123                                                             SE15H    HOUSE1                                            00001000CS           SE9004   REPLACE REQUEST PENDING                                                Y  1101SV9SX00018";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseReplace, responseMessage.EM_MessageSubType);

			entryHeader.Reload();
			AssertEquals(entryHeader, responseMessage.EM_LinkedObject);
			AssertEquals("Entry Status", ImportMessageStatusList.Codes.ReplaceRequestPending, entryHeader.CH_Status);
		}

		public void TestProcessWaitingReviewMessageWithCPR()
		{
			var declaration = GetNotMergedDeclaration("71004798");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "HYEDUSCMT_150158";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_150158     SE10RSV9  71004798 01EI 58-12345678940800000100001101                           SE11       B815                        132                                      SE13CRAIG SEELIG                            215-555-1212       1                SE15M    0011212123                                                             SE15H    HOUSE1                                            00001000CS           SE20CR B00161503                                                                SE30MF ACE TEST SUPPLIER                                                        SE3515P O BOX 288                                                               SE36OSAKA                                       530-01         JP               SE30CN                                    EI 58-123456789                       SE30BY ACE TEST IMPORTER 1                                                      SE35155000 TOWERS CRESCENT DRIVE         15NWD ENTERPRISES 12A                  SE36PHILLY                                      19444          US               SE30SE ACE TEST SUPPLIER                                                        SE3515P O BOX 288                                                               SE36OSAKA                                       530-01         JP               SE40001AU                                                                       SE6039209950000000010000                                                        Y  1101SV9SE00018";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseDelete;

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Messages.Add(message);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_150158";

			responseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_150158     SE10RSV9  71004798 01EI 58-12345678940800000100001101                           SE11       B815                        132                                      SE15M    0011212123                                                             SE15H    HOUSE1                                            00001000CS           SE9004   CANCELLATION REQUEST PENDING                                           Y  1101SV9SX00018";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals("RCV", responseMessage.EM_Status);
			AssertEquals("Message Sub Type should be set", EM_MessageSubTypeList.Codes.ACECargoReleaseDelete, responseMessage.EM_MessageSubType);

			entryHeader.Reload();
			AssertEquals(entryHeader, responseMessage.EM_LinkedObject);
			AssertEquals("Entry Status", ImportMessageStatusList.Codes.CancellationRequestPending, entryHeader.CH_Status);
		}

		public void TestCRLCertStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.US_EntryFilerCode = "SV9";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.ImportEntryNumber = "71005415";
			declaration.JE_MasterBill = "FSDFS324234";
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(declaration.MessageInitiator);
			Factory.Save();
			var entrySummary = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = entrySummary.Messages[0];
			message.EM_MessageNum = "HYEDUSCMT_151199";

			AssertEquals("Precondition - entry should have US_CRLCertStatus = 'P'", CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending, entrySummary.US_CRLCertStatus);

			var ensResponse = Factory.New<MQEDIMessage>();
			ensResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			ensResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ensResponse.EM_Status = EDIMessage.Status.Queued;
			ensResponse.EM_MessageNum = "HYEDUSCMT_151199";
			ensResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			ensResponse.EM_MessageText =
"B001101SV9AX                                               HYEDUSCMT_151199     " +
"E0 SUMMRY 000001 REF ID: SV9 71005415 B00162063                                 " +
"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100541500100B00162063   " +
"Y  1101SV9AX00002";

			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "HYEDUSCMT_151199";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageText =
"B001101SV9SX                                               HYEDUSCMT_151199     " +
"SE10ASV9  71005415 01EI 23-45678901210800000000901101  1101                     " +
"SE11 040714A001    TITANIC             0098L                                    " +
"SE15RAPLUFSDFS324234                                       00000015CS           " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00000";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals("Entry Summary Header is a linked object", entrySummary.PK, message.EM_LinkUniqueID);
			entrySummary.Reload();
			AssertEquals("CertStatus should be 'certified', because accepted SX message received", CargoReleaseCertificationStatusList.Codes.Certified, entrySummary.US_CRLCertStatus);
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

			var declaration = GetDeclaration();
			declaration.JE_GB = newBranch.PK;
			declaration.JE_GS_NKCusAgent = staffZ1.GS_Code;
			declaration.Logs.GetAllLogs().Load();
			AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse));

			message2.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_145704     " +
"SE10ASV9  71001141 11EI 23-45678901240800000001501101                           " +
"SE15R    31734243425                                       00000003AT           " +
"SE20CR B00159765                                                                " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00022";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message2.Reload();
			AssertEquals("RCV", message2.EM_Status);
			AssertEquals(EM_MessageSubTypeList.Codes.ACECargoReleaseAdd, message2.EM_MessageSubType);

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertEquals(entryHeader.PK, message2.EM_LinkedObject.PK);
			entryHeader.Reload();
			((ILogsInternals)entryHeader.Logs).ReloadFromDB();
			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseAdd, entryHeader.CH_Status);
			AssertEquals(true, entryHeader.HasBeenLodgedAtCustoms);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Response for B00001000 / XJ5-0000001-4"); }));
			Assert(email.Body.Contains("SE DATA ACCEPTED"));
			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(2, image.Width);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoingMessage2 = mock.Object;
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageNum = "~15003";
			outgoingMessage2.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_145629     SE10ASV9 7100102601EI 58-1234567894158-123456789000000000001101                 SE15M31  DKT00003006                                               CS           SE20CR B00159692                                                                SE40001GB                                                                       SE30MF CENTURA FOODS                                                            SE3515DROYLSDEN                          15PO BOX 4 FITZROY STREET              SE36DROYLSDEN                                   M4             GB               SE30CN                                                                          SE30BY KAAL AUSTRALIA PTY LTD                                                   SE3515KIORA CRES                                                                SE36YENNORA                                     2161           AU               SE6036010000000000000100                                                        Y  1101SV9SE00012";
			outgoingMessage2.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseReplace;
			entryHeader.Messages.Add(outgoingMessage2);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseReplace;

			var responseMessage2 = Factory.New<MQEDIMessage>();
			responseMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage2.EM_Status = EDIMessage.Status.Queued;
			responseMessage2.EM_MessageNum = "~15003";

			responseMessage2.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_145704     " +
"SE10ASV9  71001141 11EI 23-45678901240800000001501101                           " +
"SE15R    31734243425                                       00000003AT           " +
"SE20CR B00159765                                                                " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00022";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage2.Reload();
			AssertEquals("RCV", responseMessage2.EM_Status);
			AssertEquals(EM_MessageSubTypeList.Codes.ACECargoReleaseReplace, responseMessage2.EM_MessageSubType);
			entryHeader.Reload();
			((ILogsInternals)entryHeader.Logs).ReloadFromDB();
			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseReplace, entryHeader.CH_Status);

			declaration.Logs.GetAllLogs().Load();
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse));
		}

		[TestDate(2024, 06, 10, 13, 30, 00)]
		public void TestAutoGenerateCargoManifestStatusQueryForAirSetsHeldUntilDate()
		{
			var now = ZDateTime.Now;
			var timespanNow = new TimeSpan(now.Hour, now.Minute, 0);

			var declaration = CreateDeclaration();
			SetUpOriginalAndResponseMessage(declaration);
			var registryItem = new AutoQueryBillOfLadingCargoManifestStatus();
			registryItem.SendBasedOnETA = true;
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, registryItem);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			AssertResponseMessageLoaded(declaration, 4);
			AssertEquals("Generated messages HeldUntilDate is the arrival date", 4, declaration.Messages.Cast<MQEDIMessage>().Count(m => m.EM_HeldUntilDate == declaration.JE_DateOfArrival.Add(timespanNow)));
		}

		[TestDate(2024, 06, 10, 13, 30, 00)]
		public void TestAutoGenerateCargoManifestStatusQueryForOceanRailTruckSetsHeldUntilDate()
		{
			var now = ZDateTime.Now;
			var timespanNow = new TimeSpan(now.Hour, now.Minute, 0);

			var declaration = CreateDeclaration();
			SetUpOriginalAndResponseMessage(declaration);
			var registryItem = new AutoQueryBillOfLadingCargoManifestStatus();
			registryItem.SendBasedOnETA = true;
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, registryItem);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea; // Ocean/Rail/Truck all use the same query filter and only Ocean (Sea) is available at this level

			AssertResponseMessageLoaded(declaration, 2);
			AssertEquals("Generated messages HeldUntilDate is arrival date -1", 2, declaration.Messages.Cast<MQEDIMessage>().Count(m => m.EM_HeldUntilDate == declaration.JE_DateOfArrival.AddDays(-1).Add(timespanNow)));
		}

		[TestDate(2024, 06, 10, 13, 30, 00)]
		public void TestAutoGenerateCargoManifestStatusQueryHeldUntilDateUpdatesWithNewETA()
		{
			var now = ZDateTime.Now;
			var timespanNow = new TimeSpan(now.Hour, now.Minute, 0);

			var declaration = CreateDeclaration();
			SetUpOriginalAndResponseMessage(declaration);
			var registryItem = new AutoQueryBillOfLadingCargoManifestStatus();
			registryItem.SendBasedOnETA = true;
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, registryItem);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			AssertResponseMessageLoaded(declaration, 4);

			var initialArrivalDate = declaration.JE_DateOfArrival;
			AssertEquals("Generated messages HeldUntilDate is the arrival date", 4, declaration.Messages.Cast<MQEDIMessage>().Count(m => m.EM_HeldUntilDate == initialArrivalDate.Add(timespanNow)));

			var updatedArrivalDate = initialArrivalDate.AddDays(5);
			declaration.JE_DateOfArrival = updatedArrivalDate;
			AssertEquals("Generated messages HeldUntilDate is the original arrival date until saved", 4, declaration.Messages.Cast<MQEDIMessage>().Count(m => m.EM_HeldUntilDate == initialArrivalDate.Add(timespanNow)));

			Factory.Save();
			AssertEquals("Generated messages HeldUntilDate is updated to be the new arrival date", 4, declaration.Messages.Cast<MQEDIMessage>().Count(m => m.EM_HeldUntilDate == updatedArrivalDate.Add(timespanNow)));
		}

		public void TestAutoGenerateCargoManifestStatusQueryOnlyIfRegistryIsTrue()
		{
			var declaration = CreateDeclaration();
			SetUpOriginalAndResponseMessage(declaration);
			var registryItem = new AutoQueryBillOfLadingCargoManifestStatus();
			registryItem.SendBasedOnETA = false;
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(Guid.Empty, declaration.RegistryBranchPK, System.Guid.Empty, registryItem);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			AssertResponseMessageLoaded(declaration, 0);
		}

		void AssertResponseMessageLoaded(JobDeclaration declaration, int messageCount)
		{
			cargoReleaseResponseMessage.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00010";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			cargoReleaseResponseMessage.Reload();
			AssertEquals("RCV", cargoReleaseResponseMessage.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Response for"); }));
			Assert(email.Body.Contains("SE DATA ACCEPTED"));

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Reload();
			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseAdd, entryHeader.CH_Status);

			declaration.Messages.Load();
			var generatedMessages = declaration.Messages.Cast<MQEDIMessage>().Count(m => m.IsCargoManifestQuery && m.EM_Status == MQEDIMessage.Status.Queued);
			AssertEquals("Declaration has " + messageCount + " generated messages", messageCount, generatedMessages);
		}

		public void TestGetEmailGroupRegistryItem_WillReturnLowValueEntriesReleaseMessagesRegistrySettings_WhenErrorStatusTicked()
		{
			var simplifiedEntryProcessor = new SimplifiedEntryProcessor();
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

		#region Implementation

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		JobDeclaration CreateDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71001919";
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(30);
			declaration.JE_GS_NKCusAgent = staffZ1.GS_Code;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3920100000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			var billMB1 = declaration.Bills.AddNew();
			billMB1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			billMB1.CU_BillNum = "0018210009999";

			var billMB2 = declaration.Bills.AddNew();
			billMB2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			billMB2.CU_BillNum = "0018210009988";

			var billHB1 = declaration.Bills.AddNew();
			billHB1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			billHB1.CU_BillNum = "0018210009977";

			var billHB2 = declaration.Bills.AddNew();
			billHB2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			billHB2.CU_BillNum = "0018210009966";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			return declaration;
		}

		void SetUpOriginalAndResponseMessage(JobDeclaration declaration)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var cargoReleaseMessage = mock.Object;
			cargoReleaseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cargoReleaseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			cargoReleaseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			cargoReleaseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
			cargoReleaseMessage.EM_MessageNum = "HYEDUSCMT_148191";
			cargoReleaseMessage.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148784     SE10USV9  71002958 01EI 23-45678901240800000001201101                           SE15R    00591325210                                                            SE16COA 6020 09091300000450                                                     SE15M    AAA00178975                                                            SE15H    AAAHB343234                                       00000002CU           SE15H    AAAHB000006                                       00000009VY           SE15M    BBB002124564                                                           SE15H    BBB00245454HB                                                          SE15S    BBB00215454SHB                                    00000009AM           SE20CR B00160864                                                                Y  1101SV9SE00010";

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Messages.Add(cargoReleaseMessage);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			cargoReleaseResponseMessage = Factory.New<MQEDIMessage>();
			cargoReleaseResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cargoReleaseResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cargoReleaseResponseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			cargoReleaseResponseMessage.EM_Status = EDIMessage.Status.Queued;
			cargoReleaseResponseMessage.EM_MessageNum = "HYEDUSCMT_148191";

			Factory.Save();
		}

		MQEDIMessage cargoReleaseResponseMessage;

		JobDeclaration GetDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.JE_GS_NKCusAgent = staffZ1.GS_Code;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = true;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_145629     SE10ASV9 7100102601EI 58-1234567894158-123456789000000000001101                 SE15M31  DKT00003006                                               CS           SE20CR B00159692                                                                SE40001GB                                                                       SE30MF CENTURA FOODS                                                            SE3515DROYLSDEN                          15PO BOX 4 FITZROY STREET              SE36DROYLSDEN                                   M4             GB               SE30CN                                                                          SE30BY KAAL AUSTRALIA PTY LTD                                                   SE3515KIORA CRES                                                                SE36YENNORA                                     2161           AU               Y  1101SV9SE00012";
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;

			var entryHeader = dec.ActiveEntryHeaders.SimplifiedEntry;
			entryHeader.Messages.Add(message);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_MessageNum = "~15000";

			Factory.Save();
			return dec;
		}
		MQEDIMessage message2;

		JobDeclaration GetNotMergedDeclaration(ZString entryNumber)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EntryFilerCode = "SV9";
			dec.ImportEntryNumber = entryNumber;
			dec.JE_GS_NKCusAgent = staffZ1.GS_Code;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			return dec;
		}
		#endregion
	}
}
