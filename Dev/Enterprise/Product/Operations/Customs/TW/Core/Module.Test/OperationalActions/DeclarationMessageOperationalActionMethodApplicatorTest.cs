using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Module.OperationalActions;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(DeclarationMessageOperationalActionMethodApplicator))]
	sealed class DeclarationMessageOperationalActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestDefaultScheduleActionCode()
		{
			var testApplicator = new DeclarationMessageOperationalActionMethodApplicator();
			AssertEquals(true, testApplicator.UseDaysOfDelayed);
			AssertEquals(false, testApplicator.ReMergeAndCalculate);
			AssertEquals(true, testApplicator.SuppressNotificationPopout);
			AssertEquals(false, testApplicator.IgnoreMessageWarnings);
			AssertEquals("Submit Original Entry to Taiwan Customs", testApplicator.Name);
		}

		public void TestApplyCore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList("TW", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BT", "Nanjing Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
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
			var applicator = new DeclarationMessageOperationalActionMethodApplicatorForTest();
			applicator.UseDaysOfDelayed = true;
			applicator.ReMergeAndCalculate = false;
			applicator.SuppressNotificationPopout = true;
			applicator.IgnoreMessageWarnings = true;
			var ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessageCollection.Length);
			var testDeclaration_IMP = Factory.New<JobDeclaration>();
			testDeclaration_IMP.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDeclaration_IMP.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			testDeclaration_IMP.JE_CustomsOffice = "BT";
			testDeclaration_IMP.JE_DeclarationReference = "TWB000001";
			var entryInstruction = testDeclaration_IMP.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryInstruction.CEI_CustomsOffice = "BT";
			Factory.Save();
			applicator.PerformFunctionOperationalAction(testOperationalLog, new BusinessObject[] { testDeclaration_IMP });
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
			applicator.PerformFunctionOperationalAction(testOperationalLog, new BusinessObject[] { testDeclaration_IMP });
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
			applicator.PerformFunctionOperationalAction(testOperationalLog, new BusinessObject[] { testDeclaration_IMP });
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				AssertEquals(@"ERROR: A valid credential is missing.
INFO: [HL TWB000001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}

			);
			testOperationalLog.messages.Clear();
			applicator.ReMergeAndCalculate = true;
			applicator.PerformFunctionOperationalAction(testOperationalLog, new BusinessObject[] { testDeclaration_IMP });
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
			applicator.PerformFunctionOperationalAction(testOperationalLog, new BusinessObject[] { testDeclaration_IMP });
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
				AssertEquals(@"ERROR: Entry status must be empty or REJ.
INFO: [HL TWB000001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}

			);
			testOperationalLog.messages.Clear();
			entry.CH_EntryStatus = "";
			entry.CH_Status = "BB";
			applicator.PerformFunctionOperationalAction(testOperationalLog, new BusinessObject[] { testDeclaration_IMP });
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
			var testDeclaration_IMP_NoEntry = Factory.New<JobDeclaration>();
			testDeclaration_IMP_NoEntry.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			var testDeclaration_EXP = Factory.New<JobDeclaration>();
			testDeclaration_EXP.JE_MessageType = JobMessageTypeList.Codes.Export;
			testDeclaration_IMP_NoEntry.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			Factory.Save();
			applicator.PerformFunctionOperationalAction(testOperationalLog, new BusinessObject[] { testDeclaration_IMP, testDeclaration_IMP_NoEntry, testDeclaration_EXP });
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 1, ediMessageCollection.Length);
				AssertEquals(1, ediMessageCollection.Count(msg => msg.EM_LinkUniqueID == entry.PK));
				AssertContains(@"INFO: [HL TWB000001]: Submit Succeeded.
ERROR: You can't merge this entry because there are no invoice headers.
ERROR: Please generate entries first.
INFO: [HL B00001000]: Submit Failed.
ERROR: You can't merge this entry because there are no invoice headers.
ERROR: Please generate entries first.
INFO: [HL B00001001]: Submit Failed.", testOperationalLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var logEntries = testDeclaration_IMP.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
				AssertEquals("CCC (Customs Commenced) Log Entry event should have been created", 1, logEntries.Length);
			}

			);
		}

		class DeclarationMessageOperationalActionMethodApplicatorForTest : DeclarationMessageOperationalActionMethodApplicator
		{
			public DeclarationMessageOperationalActionMethodApplicatorForTest() : base()
			{
			}

			public void PerformFunctionOperationalAction(IOperationalActionSectionLog log, BusinessObject[] targets)
			{
				base.ApplyCore(log, targets);
			}
		}
	}
}
