using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;
using ECBM = Enterprise.Customs.Business.MessageManagers;

namespace Enterprise.Customs.ZA.Business.MessageManagers.Testing
{
	[TestedType(typeof(REQDOCMessageManager))]
	sealed class REQDOCMessageManagerTest : EDIFACTMessageManagerTestCase
	{
		public override void TestShouldSendMessagesInTestMode()
		{
			AssertShouldSendMessagesInTestMode(messageManager, ((CusEntryHeader)dataWrapper).Declaration.Branch);

			var statacParent = new STATACREQDOCSendingObjectParent(Factory);
			var statacSendingObject = new STATACREQDOCSendingObject(statacParent, new FinancialAccountNumberPortMap());
			var statacMessageManager = new REQDOCMessageManager(statacSendingObject, new MessageNotificationCollector_ForTest());

			AssertShouldSendMessagesInTestMode(statacMessageManager, ((IREQDOCMessageDataProvider)statacSendingObject).Branch);
		}

		public override void SetTestMode(bool testMode)
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, testMode);
		}

		public override void TestCanSendThisMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._00;

			new LineMerger(declaration).DoMerge();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			var testWrapper = new MessageSendingObjectForREQDOC(entryHeader);
			testWrapper.MessageType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Original;

			var manager = new REQDOCMessageManagerForTest(testWrapper, notification);
			var reason = ZString.Empty;
			AssertEquals(false, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals("Job not yet saved, Please save before sending.", reason);

			Factory.Save();
			reason = ZString.Empty;
			manager = new REQDOCMessageManagerForTest(testWrapper, notification);
			AssertEquals(true, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals(string.Empty, reason);
		}

		public override void TestGetMessageBuilder()
		{
			AssertType(typeof(REQDOCMessageBuilder), (messageManager as REQDOCMessageManagerForTest).GetMessageBuilder_Exposed(MessageSubTypes.Create));
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("REQDOC", messageManager.MessageFriendlyName);
		}

		public override void TestPopulateMessages()
		{
			var date = ZDateTime.Today.ToCCYYMMDD();

			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew("CDP", "TST", "ZA");
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._00;

			new LineMerger(declaration).DoMerge();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00505655KFN20160602000001";
			entryHeader.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
			entryHeader.CH_EntryStatus = "1";
			var message = Factory.New<ZAMessageForTest>();
			entryHeader.Messages.Add(message);
			message.EM_ApplicationCode = SARSEDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			message.MessageNumForTesting = "MOTHERBEN";

			Factory.Save();

			var testWrapper = new MessageSendingObjectForREQDOC(entryHeader);
			testWrapper.MovementReferenceNumber = "KFN201607115000003";
			testWrapper.DocumentMessageSource = "358359155008f49bfnode1";

			var manager = new REQDOCMessageManagerForTest(testWrapper, notification);
			var result = manager.PopulateMessages_Exposed();
			AssertEquals(1, result.Length);
			AssertEquals(ZAMessageStatusList.Codes.Acknowledged, entryHeader.CH_Status);
			AssertEquals("1", entryHeader.CH_EntryStatus);
			AssertMultilineASCIIEquals("MessageBody with Reference value", @"UNH+<<MSGNO PLACEHOLDER>>+REQDOC:D:99B:UN:ZZZ01
BGM+929+00505655KFN20160602000001+9
DOC+929+KFN201607115000003::358359155008f49bfnode1
DTM+318:" + date + @":102
NAD+MS+51051342TST
LIN+1
UNT+7+<<MSGNO PLACEHOLDER>>
", result[0].EM_MessageText.Replace("'", "\r\n"));

			instruction.CEI_DateForDuty = ZDateTime.Now;
			testWrapper = new MessageSendingObjectForREQDOC(entryHeader);

			manager = new REQDOCMessageManagerForTest(testWrapper, notification);
			result = manager.PopulateMessages_Exposed();
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", @"UNH+<<MSGNO PLACEHOLDER>>+REQDOC:D:99B:UN:ZZZ01
BGM+929+00505655KFN20160602000001+9
DOC+929
DTM+318:" + date + @":102
NAD+MS+51051342TST
LIN+1
UNT+7+<<MSGNO PLACEHOLDER>>
", result[0].EM_MessageText.Replace("'", "\r\n"));

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_ParentTable = "CusEntryHeader";
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_EntryNum = "KFN201607115000007";
			entryNum.CE_EntryType = "MRN";
			entryNum.CE_Category = "CUS";
			entryNum.CE_IssueDate = ZDateTime.Today;
			entryNum.CE_EntryIsSystemGenerated = true;

			Factory.Save();

			testWrapper = new MessageSendingObjectForREQDOC(entryHeader);
			testWrapper.MessageType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Original;

			manager = new REQDOCMessageManagerForTest(testWrapper, notification);
			result = manager.PopulateMessages_Exposed();
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", @"UNH+<<MSGNO PLACEHOLDER>>+REQDOC:D:99B:UN:ZZZ01
BGM+929+00505655KFN20160602000001+9
DOC+929+KFN201607115000007
DTM+318:" + date + @":102
NAD+MS+51051342TST
LIN+1
UNT+7+<<MSGNO PLACEHOLDER>>
", result[0].EM_MessageText.Replace("'", "\r\n"));
			AssertEquals(Env.CurrentBranchPK, result[0].EM_GB);
		}

		public override void TestIsWaitingForResponse()
		{
			var newFactory = new BusinessObjectFactory();
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			CombineAssertions("Send REQDOC - send straight away", () =>
			{
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				AssertEquals(false, messageManager.IsWaitingForResponse);
			});

			CombineAssertions("With CUSDEC only, Send REQDOC - show and warn with new Text", () =>
			{
				var cusdec1PK = CreateTestMessageInAnotherFactory(newFactory, "0001", "DEC", "ORG", "");
				entryHeader.Messages.Add(Factory.Load<CUSDECEDIMessage>(cusdec1PK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				AssertEquals(true, messageManager.IsWaitingForResponse);
			});
			CombineAssertions("CUSDEC with rejection CONTROL, Send REQDOC - send straight away", () =>
			{
				var contrl1PK = CreateTestMessageInAnotherFactory(newFactory, "1", "CTL", "XXX", GetCONTRLBody("0001", "4"));
				entryHeader.Messages.Add(Factory.Load<CONTRLEDIMessage>(contrl1PK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				AssertEquals(false, messageManager.IsWaitingForResponse);
			});
			CombineAssertions("CUSDEC with accepted CONTROL, Send REQDOC - show and warn with new Text", () =>
			{
				var cusdec2PK = CreateTestMessageInAnotherFactory(newFactory, "0003", "DEC", "ORG", "");
				entryHeader.Messages.Add(Factory.Load<CUSDECEDIMessage>(cusdec2PK));
				Factory.Save();
				var contrl2PK = CreateTestMessageInAnotherFactory(newFactory, "1", "CTL", "XXX", GetCONTRLBody("0003", "7"));
				entryHeader.Messages.Add(Factory.Load<CONTRLEDIMessage>(contrl2PK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				AssertEquals(true, messageManager.IsWaitingForResponse);
			});
			CombineAssertions("CUSDEC with accepted CONTROL and CUSRES irrelevant, Send REQDOC - show and warn with new Text", () =>
			{
				var cusresPK = CreateTestMessageInAnotherFactory(newFactory, "1", "RES", "XXX", GetCUSRESBody("0001"));
				entryHeader.Messages.Add(Factory.Load<CUSRESEDIMessage>(cusresPK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				AssertEquals(true, messageManager.IsWaitingForResponse);
			});
			CombineAssertions("CUSDEC with accepted CONTROL and proper CUSRES, Send REQDOC - send straight away", () =>
			{
				var cusresPK = CreateTestMessageInAnotherFactory(newFactory, "1", "RES", "XXX", GetCUSRESBody("0003"));
				entryHeader.Messages.Add(Factory.Load<CUSRESEDIMessage>(cusresPK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				AssertEquals(false, messageManager.IsWaitingForResponse);
			});
			CombineAssertions("With CUSDEC only, Send Specific REQDOC - by pass status check and send straight away", () =>
			{
				var cusdec3PK = CreateTestMessageInAnotherFactory(newFactory, "0005", "DEC", "ORG", "");
				entryHeader.Messages.Add(Factory.Load<CUSDECEDIMessage>(cusdec3PK));
				Factory.Save();
				var sendingObject = new MessageSendingObjectForREQDOC(entryHeader);
				sendingObject.DocumentMessageSource = "specific";
				var messageManager = new REQDOCMessageManager(sendingObject, notification);
				AssertEquals(false, messageManager.IsWaitingForResponse);
			});
		}

		public void TestNotificationOfMessageInAwaitingStatus()
		{
			var awaitingMessage = @"This job is waiting for a response from Customs.
Would you like to request Customs to resend the latest response that they have on their system for this entry?
If Customs have not issued a response on their system then this request will not receive a response from Customs.";
			var newFactory = new BusinessObjectFactory();
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			CombineAssertions("Send REQDOC - send straight away", () =>
			{
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				notification.ShowInformation(ZString.Empty, ZString.Empty);
				notification.NextAnswer = false;
				Factory.Save();
				AssertEquals(true, messageManager.Send());
				AssertEquals("notification.LastMessage", "Create REQDOC message queued for sending.", notification.LastMessage);
			});

			CombineAssertions("With CUSDEC only, Send REQDOC - show and warn with new Text", () =>
			{
				var cusdec1PK = CreateTestMessageInAnotherFactory(newFactory, "0001", "DEC", "ORG", "");
				entryHeader.Messages.Add(Factory.Load<CUSDECEDIMessage>(cusdec1PK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				notification.ShowInformation(ZString.Empty, ZString.Empty);
				notification.NextAnswer = false;
				AssertEquals(false, messageManager.Send());
				AssertEquals("notification.LastMessage", awaitingMessage, notification.LastMessage);
			});
			CombineAssertions("CUSDEC with rejection CONTROL, Send REQDOC - send straight away", () =>
			{
				var contrl1PK = CreateTestMessageInAnotherFactory(newFactory, "1", "CTL", "XXX", GetCONTRLBody("0001", "4"));
				entryHeader.Messages.Add(Factory.Load<CONTRLEDIMessage>(contrl1PK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				notification.ShowInformation(ZString.Empty, ZString.Empty);
				notification.NextAnswer = false;
				AssertEquals(true, messageManager.Send());
				AssertEquals("notification.LastMessage", "Create REQDOC message queued for sending.", notification.LastMessage);
			});
			CombineAssertions("CUSDEC with accepted CONTROL, Send REQDOC - show and warn with new Text", () =>
			{
				var cusdec2PK = CreateTestMessageInAnotherFactory(newFactory, "0003", "DEC", "ORG", "");
				entryHeader.Messages.Add(Factory.Load<CUSDECEDIMessage>(cusdec2PK));
				Factory.Save();
				var contrl2PK = CreateTestMessageInAnotherFactory(newFactory, "1", "CTL", "XXX", GetCONTRLBody("0003", "7"));
				entryHeader.Messages.Add(Factory.Load<CONTRLEDIMessage>(contrl2PK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				notification.ShowInformation(ZString.Empty, ZString.Empty);
				notification.NextAnswer = false;
				AssertEquals(false, messageManager.Send());
				AssertEquals("notification.LastMessage", awaitingMessage, notification.LastMessage);
			});
			CombineAssertions("CUSDEC with accepted CONTROL and CUSRES irrelevant, Send REQDOC - show and warn with new Text", () =>
			{
				var cusresPK = CreateTestMessageInAnotherFactory(newFactory, "1", "RES", "XXX", GetCUSRESBody("0001"));
				entryHeader.Messages.Add(Factory.Load<CUSRESEDIMessage>(cusresPK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				notification.ShowInformation(ZString.Empty, ZString.Empty);
				notification.NextAnswer = false;
				AssertEquals(false, messageManager.Send());
				AssertEquals("notification.LastMessage", awaitingMessage, notification.LastMessage);
			});
			CombineAssertions("CUSDEC with accepted CONTROL and proper CUSRES, Send REQDOC - send straight away", () =>
			{
				var cusresPK = CreateTestMessageInAnotherFactory(newFactory, "1", "RES", "XXX", GetCUSRESBody("0003"));
				entryHeader.Messages.Add(Factory.Load<CUSRESEDIMessage>(cusresPK));
				Factory.Save();
				var messageManager = new REQDOCMessageManager(new MessageSendingObjectForREQDOC(entryHeader), notification);
				notification.ShowInformation(ZString.Empty, ZString.Empty);
				notification.NextAnswer = false;
				AssertEquals(true, messageManager.Send());
				AssertEquals("notification.LastMessage", "Create REQDOC message queued for sending.", notification.LastMessage);
			});

			CombineAssertions("With CUSDEC only, Send Specific REQDOC - by pass status check and send straight away", () =>
			{
				var cusdec3PK = CreateTestMessageInAnotherFactory(newFactory, "0005", "DEC", "ORG", "");
				entryHeader.Messages.Add(Factory.Load<CUSDECEDIMessage>(cusdec3PK));
				Factory.Save();
				var sendingObject = new MessageSendingObjectForREQDOC(entryHeader);
				sendingObject.DocumentMessageSource = "specific";
				var messageManager = new REQDOCMessageManager(sendingObject, notification);
				notification.ShowInformation(ZString.Empty, ZString.Empty);
				notification.NextAnswer = false;
				AssertEquals(true, messageManager.Send());
				AssertEquals("notification.LastMessage", "Create REQDOC message queued for sending.", notification.LastMessage);
			});
		}

		public void TestMessageContentEscapedWithCorrectCharacterSet()
		{
			ZString date = ZDateTime.Today.ToCCYYMMDD();

			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew("CDP", "TST", "ZA");
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._00;

			new LineMerger(declaration).DoMerge();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00505655KFN20160602000001";
			entryHeader.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
			entryHeader.CH_EntryStatus = "1";
			var message = Factory.New<ZAMessageForTest>();
			entryHeader.Messages.Add(message);
			message.EM_ApplicationCode = SARSEDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			message.MessageNumForTesting = "MOTHERBEN";

			Factory.Save();

			var testWrapper = new MessageSendingObjectForREQDOC(entryHeader);
			testWrapper.MovementReferenceNumber = "KFN201607115000003";
			testWrapper.DocumentMessageSource = "123-+:'?-789";

			var manager = new REQDOCMessageManagerForTest(testWrapper, notification);
			var result = manager.PopulateMessages_Exposed();

			var msgText = result[0].EM_MessageText;

			AssertContains("Escaped", "DOC+929+KFN201607115000003::123-?+?:?'??-789'", msgText);
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._00;

			new LineMerger(declaration).DoMerge();
			return declaration.ActiveEntryHeaders[0];
		}

		protected override ECBM.EDIFACTMessageManager GetMessageManager()
		{
			return new REQDOCMessageManagerForTest(new MessageSendingObjectForREQDOC(dataWrapper as CusEntryHeader), new TestUserNotification());
		}

		ZGuid CreateTestMessageInAnotherFactory(BusinessObjectFactory factory, ZString messageNumber, ZString messageType, ZString messageSubType, ZString messageText)
		{
			var message = factory.New<ZAMessageForTest>();
			message.MessageNumForTesting = messageNumber;
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = messageText;
			factory.Save();
			return message.PK;
		}

		string GetCONTRLBody(string sourceMsg, string actionCode)
		{
			return ZString.Format(@"UNH+1+CONTRL:D:3:UN:CONTRL'
UCI+851+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+4'
UCM+{0}+CUSDEC:D:96B:UN:ZZZ01+{1}'
UNT+4+1'", sourceMsg, actionCode).Replace("\r\n", "");
		}

		string GetCUSRESBody(string sourceMsg)
		{
			return ZString.Format(@"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00505655CLP20160514000245:0'
DTM+178:20160511:102'
DTM+202:20160511:102'
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
RFF+ACD:{0}'
TAX+3+CUS:107:ZZZ'
MOA+161:490688'
CNT+7:226.79'
CNT+11:5'
UNT+21+1'", sourceMsg).Replace("\r\n", "");
		}

		new readonly TestUserNotification notification = new TestUserNotification();

		void AssertShouldSendMessagesInTestMode(ECBM.EDIFACTMessageManager edifactMessageManager, IBranch reqdocBranch)
		{
			Env.Registry.ZACustoms.SetIsTestMode(reqdocBranch, true);
			Assert(edifactMessageManager.ShouldSendMessagesInTestModeForTesting);
			Env.Registry.ZACustoms.SetIsTestMode(reqdocBranch, false);
			Assert(!edifactMessageManager.ShouldSendMessagesInTestModeForTesting);
		}
	}

	sealed class REQDOCMessageManagerForTest : REQDOCMessageManager
	{
		public REQDOCMessageManagerForTest(MessageSendingObjectForREQDOC sendingObject, ECBM.IUserNotification notification) : base(sendingObject, notification) { }

		public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode) => GetMessageBuilder(actionCode);

		public Messaging.Business.EDIMessage[] PopulateMessages_Exposed()
		{
			return PopulateMessage(MessageSubTypes.Create);
		}

		public bool CanSendThisMessage_Exposed(out ZString messageText)
		{
			return CanSendThisMessage(MessageSubTypes.Undefined, out messageText);
		}
	}
}
