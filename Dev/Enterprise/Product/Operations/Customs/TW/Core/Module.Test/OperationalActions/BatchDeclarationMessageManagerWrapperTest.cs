using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Module.OperationalActions;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	public sealed class BatchDeclarationMessageManagerWrapperTest : TestCaseWithFactory
	{
		[TestDate(2020, 05, 20)]
		public void TestPerformFunctionOperationalAction()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeList("TW", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BT", "Nanjing Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var customsOffice = TWRefCusCodeListLoader.GetCustomsOffice(Factory, "BT", ZDateTime.Now);
			customsOffice.ZZD_IsSea = true;
			customsOffice.ZZD_IsAir = false;
			Factory.Save();
			var company = GlbCompany.CurrentCompany;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var extPassword1 = Factory.New<Business.GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();
			var testOperationalLog = new DummyOperationalActionSectionLog();
			var testDeclaration_IMP = Factory.New<JobDeclaration>();
			testDeclaration_IMP.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDeclaration_IMP.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			testDeclaration_IMP.JE_CustomsOffice = "BT";
			testDeclaration_IMP.JE_DeclarationReference = "TWB000001";
			var entryInstruction = testDeclaration_IMP.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryInstruction.CEI_CustomsOffice = "BT";
			entryInstruction.CEI_DateForDuty = TestDateAttribute.Date.AddDays(-2);
			Factory.Save();
			var useDaysOfDelayed = true;
			var reMergeAndCalculate = false;
			var suppressNotificationPopout = true;
			var ignoreMessegeWarnings = true;
			var notificationCollector = new MessageNotificationCollector();
			var sendsMessagesToCustomsGUI = new OperationalActionMessageNotificationCollector(testOperationalLog)
			{ SuppressUserInteraction = suppressNotificationPopout };
			testDeclaration_IMP.MessageInitiator = sendsMessagesToCustomsGUI;
			var ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessageCollection.Length);
			testOperationalLog.messages.Clear();
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				AssertEquals(@"ERROR: The job does not have 'Submit Type: BLT - Submit entry using built-in messaging system' selected.
INFO: [HL TWB000001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}

			);
			testOperationalLog.messages.Clear();
			testDeclaration_IMP.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				AssertEquals(@"ERROR: You can't merge this entry because there are no invoice headers.
ERROR: Please generate entries first.
INFO: [HL TWB000001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}

			);
			testOperationalLog.messages.Clear();
			var invoiceHeader = testDeclaration_IMP.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			var newInvoiceLine = invoiceHeader.InvoiceLines.AddNew();
			entryInstruction.CEI_BoxNumber = "123";
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				AssertEquals(@"ERROR: Declaration date is not today.
INFO: [HL TWB000001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}

			);
			testOperationalLog.messages.Clear();
			entryInstruction.CEI_DateForDuty = TestDateAttribute.Date;
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				AssertEquals(@"ERROR: A valid credential is missing.
INFO: [HL TWB000001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}

			);
			testOperationalLog.messages.Clear();
			reMergeAndCalculate = true;
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				AssertEquals(@"ERROR: A valid credential is missing.
INFO: [HL TWB000001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}

			);
			testOperationalLog.messages.Clear();
			testDeclaration_IMP.JE_GS_NKCusAgent = "TT";
			testDeclaration_IMP.JE_CustomsProfile = "123-3";
			var entry = testDeclaration_IMP.ActiveEntryHeaders[0];
			entry.CH_EntryStatus = "CC";
			entry.CH_Status = "";
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				AssertEquals(@"ERROR: Entry status must be empty or REJ.
INFO: [HL TWB000001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}

			);
			ignoreMessegeWarnings = false;
			testOperationalLog.messages.Clear();
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				var messagesString = testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r");
				AssertContains(@"Do you want to send the message(s) despite these errors?", messagesString);
				AssertContains(@"INFO: [Automatic Answer]:No", messagesString);
				AssertContains(@"INFO: [HL TWB000001]: Submit Failed.", messagesString);
			}

			);
			ignoreMessegeWarnings = true;
			testOperationalLog.messages.Clear();
			entry.CH_EntryStatus = "";
			entry.CH_Status = "BB";
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				AssertEquals(@"ERROR: Message status must be empty or a code starting with ER.
INFO: [HL TWB000001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}

			);
			testOperationalLog.messages.Clear();
			entry.CH_EntryStatus = "";
			entry.CH_Status = "";
			AssertNullOrEmpty(entry.EntryNumber);
			Factory.Save();
			useDaysOfDelayed = true;
			entryInstruction.CEI_DateForDuty = TestDateAttribute.Date;
			testDeclaration_IMP.JE_DateOfArrival = TestDateAttribute.Date.AddDays(-30);
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 1, ediMessageCollection.Length);
				AssertEquals(1, ediMessageCollection.Count(msg => msg.EM_LinkUniqueID == entry.PK));
				var messagesString = testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r");
				AssertContains(@"INFO: [HL TWB000001]: Submit Succeeded.", messagesString);
				AssertContains(@"INFO: [User's Answer]:UPD", messagesString);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			);
			AssertNotNullOrEmpty(entry.EntryNumber);
			testOperationalLog.messages.Clear();
			useDaysOfDelayed = false;
			entry.CH_EntryStatus = "";
			entry.CH_Status = "";
			entryInstruction.CEI_DaysOfDelayedDeclaration = 0;
			entryInstruction.CEI_DateForDuty = TestDateAttribute.Date;
			testDeclaration_IMP.JE_DateOfArrival = TestDateAttribute.Date.AddDays(-30);
			PerformFunctionOperationalAction(testDeclaration_IMP, useDaysOfDelayed, reMergeAndCalculate, suppressNotificationPopout, ignoreMessegeWarnings, notificationCollector, testOperationalLog, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 2, ediMessageCollection.Length);
				AssertEquals(2, ediMessageCollection.Count(msg => msg.EM_LinkUniqueID == entry.PK));
				var messagesString = testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r");
				AssertContains(@"INFO: [HL TWB000001]: Submit Succeeded.", messagesString);
				AssertContains(@"INFO: [User's Answer]:CNT", messagesString);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			);
		}

		void PerformFunctionOperationalAction(JobDeclaration testDeclaration, bool useDaysOfDelayed, bool reMergeAndCalculate, bool suppressNotificationPopout, bool ignoreMessegeWarnings, MessageNotificationCollector notificationCollector, IOperationalActionSectionLog log, OperationalActionMessageNotificationCollector sendsMessagesToCustomsGUI)
		{
			var messageType = testDeclaration.CustomsMessageType;
			var wrapper = new JobDeclarationMessageSendingObjectParent(testDeclaration, messageType);
			var messageManagerWrapper = new BatchDeclarationMessageManagerWrapper(testDeclaration, wrapper, log, notificationCollector, sendsMessagesToCustomsGUI, messageType, ignoreMessegeWarnings, suppressNotificationPopout, reMergeAndCalculate);
			messageManagerWrapper.PerformFunctionOperationalAction(useDaysOfDelayed);
		}
	}
}
