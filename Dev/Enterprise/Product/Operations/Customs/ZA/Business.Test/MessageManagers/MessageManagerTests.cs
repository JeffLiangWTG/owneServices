using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageManagers.Testing
{
	sealed class MessageManagerTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSendMessages()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst2.CEI_Style = "13";
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine1 = testInvHeader.InvoiceLines.AddNew();
			testInvLine1.JI_CEI = testInst1.PK;
			testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";
			var testInvLine2 = testInvHeader.InvoiceLines.AddNew();
			testInvLine2.JI_CEI = testInst2.PK;
			testInvLine2.JI_Procedure = testInvLine2.EntryInstruction.CEI_Style + "00";
			Factory.Save();

			new LineMerger(declaration).DoMerge();
			var allEntries = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(2, allEntries.Length);
			var entryHeader1 = testInvLine1.CusEntryLine.Header;
			entryHeader1.CH_Status = "AWA";
			entryHeader1.CH_BGMReference = "BGM11";
			entryHeader1.Charges.AddNew("VAT", 200m);
			var entryHeader2 = testInvLine2.CusEntryLine.Header;
			entryHeader2.CH_BGMReference = "BGM13";

			CombineAssertions("Catch Error in case pre-send check were missed", () =>
			{
				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var notification = new MessageNotificationCollector_ForTest();
				var messageManager = new MessageManager(objectParent, notification);
				messageManager.SendMessages();
				AssertMultilineASCIIEquals("Error of Sending", @"Can not send Create for BGM11 due to error:
Job not yet saved, Please save before sending.
Can not send Create for BGM13 due to error:
Job not yet saved, Please save before sending.", notification.LastMessage);
				AssertEquals(@"Error in Message Sending", notification.LastCaption);
				Assert(allEntries.All(x => x.Messages.Count == 0));
				if (ErrorReporter.LastMessageReported == "SendMessageWithBondedWarehouseAutomation should not be called with a supporter that has changes.")
				{
					ErrorReporter.Clear();
				}
			});

			CombineAssertions("Checking is test or is waiting for response", () =>
			{
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var notification = new MessageNotificationCollector_ForTest();
				var messageManager = new MessageManager(objectParent, notification);
				notification.NextAnswer = false;
				messageManager.SendMessages();
				AssertEquals(@"This job is waiting for a Customs response.
Are you sure that you want to resend to Customs?
This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs.", notification.LastMessage);
				AssertEquals("Continue sending with warning?", notification.LastCaption);
				Assert(allEntries.All(x => x.Messages.Count == 0));
			});

			CombineAssertions("Send Attempt to AutoRate Failed", () =>
			{
				declaration.JE_TransportMode = ZString.Empty;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var notification = new MessageNotificationCollector_ForTest();
				var messageManager = new MessageManager(objectParent, notification);
				notification.NextAnswer = true;
				messageManager.SendMessages();
				AssertStartsWith("Auto-rating Error", @"Auto-rating of Customs Disbursement failed:

Failed to load/create Invoicing Job, please manually create a valid Invoicing Job in 'Billing' tab.", notification.LastMessage);
				AssertEquals("Error in Message Sending", notification.LastCaption);
				Assert(allEntries.All(x => x.Messages.Count == 0));
				ErrorReporter.Clear();
			});

			CombineAssertions("Send Message will now be successful for IMP FIX", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var notification = new MessageNotificationCollector_ForTest();
				var messageManager = new MessageManager(objectParent, notification);
				notification.NextAnswer = true;
				messageManager.SendMessages();
				AssertEquals(@"Create CUSDEC message for BGM11 queued for sending
Create CUSDEC message for BGM13 queued for sending", notification.LastMessage);
				AssertEquals("Message Sending Result", notification.LastCaption);
				Assert(allEntries.All(x => x.Messages.Count == 1));
			});

			CombineAssertions("Send Message will now be successful for EXP FIX", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var notification = new MessageNotificationCollector_ForTest();
				var messageManager = new MessageManager(objectParent, notification);
				notification.NextAnswer = true;
				messageManager.SendMessages();
				AssertEquals(@"Create CUSDEC message for BGM11 queued for sending
Create CUSDEC message for BGM13 queued for sending", notification.LastMessage);
				AssertEquals("Message Sending Result", notification.LastCaption);
				Assert(allEntries.All(x => x.Messages.Count == 2));

				declaration.JE_TransportMode = "";
			});

			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Factory.Save();

			CombineAssertions("Successful Send - Skipping Credit Check", () =>
			{
				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var notification = new MessageNotificationCollector_ForTest();
				var messageManager = new MessageManager(objectParent, notification);
				notification.NextAnswer = true;
				messageManager.SendMessages();
				AssertEquals(@"Create CUSDEC message for BGM11 queued for sending
Create CUSDEC message for BGM13 queued for sending", notification.LastMessage);
				AssertEquals("Message Sending Result", notification.LastCaption);
				Assert(allEntries.All(x => x.Messages.Count == 3));
			});

			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Factory.Save();

			CombineAssertions("Successful Send", () =>
			{
				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var notification = new MessageNotificationCollector_ForTest();
				var messageManager = new MessageManager(objectParent, notification);
				notification.NextAnswer = true;
				messageManager.SendMessages();
				AssertEquals(@"Create CUSDEC message for BGM11 queued for sending
Create CUSDEC message for BGM13 queued for sending", notification.LastMessage);
				AssertEquals("Message Sending Result", notification.LastCaption);
				Assert(allEntries.All(x => x.Messages.Count == 4));
			});

			CombineAssertions("Successful Send - only selected BGM11", () =>
			{
				entryHeader2.Charges.AddNew("VAT", 300m);
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				objectParent.SendingObjectsCollection.Where(m => m.LocalReferenceNumber == "BGM13").Single().ShouldSend = false;
				var notification = new MessageNotificationCollector_ForTest();
				var messageManager = new MessageManager(objectParent, notification);
				notification.NextAnswer = true;
				messageManager.SendMessages();
				AssertEquals("Create CUSDEC message for BGM11 queued for sending", notification.LastMessage);
				AssertEquals("Message Sending Result", notification.LastCaption);
				AssertEquals("BGM11 has new message sent", 5, entryHeader1.Messages.Count);
				AssertEquals("BGM13 has no new message sent", 4, entryHeader2.Messages.Count);
				var charges = ((Accounting.Business.JobInvoicing.Job)declaration.Job).Charges;
				AssertEquals(1, charges.Count);
				AssertEquals("Only BGM11 has charge added by auto-rating", "BGM11", charges[0].JR_APInvoiceNum);
				AssertEquals("Only BGM11 has charge added by auto-rating", 200m, charges[0].CostAmount);
			});
		}

		public void TestSendMessages_ROOCertificatesMissing()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			SetupDefaultPreferenceAndTradeAgreement();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;

			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine1 = testInvHeader.InvoiceLines.AddNew();
			testInvLine1.JI_CEI = testInst1.PK;
			testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";

			testInvLine1.JI_Tariff = "1020304050";
			testInvLine1.JI_PrimaryPreference = "200";
			testInvLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Netherlands;

			Factory.Save();
			new LineMerger(declaration).DoMerge();

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManager(objectParent, notification);

			initiator.AnswerToContinueWithAction = false;
			messageManager.SendMessages();
			AssertEquals("entry.Messages.Count", 0, declaration.CustomsEntryHeaders[0].Messages.Count);

			initiator.AnswerToContinueWithAction = true;
			messageManager.SendMessages();
			AssertNotNull(declaration.Logs.Find(x => x.ReferenceFreeText == "User acknowledged send some ROO certificates as blank").FirstOrDefault());
		}

		[TestDate(2024, 11, 10)]
		public void TestRooCertificateMissingWarnings()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			SetupDefaultPreferenceAndTradeAgreement();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine1 = testInvHeader.InvoiceLines.AddNew();
			testInvLine1.JI_CEI = testInst1.PK;
			testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";

			testInvLine1.JI_Tariff = "1020304050";
			testInvLine1.JI_PrimaryPreference = "200";
			testInvLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Netherlands;

			Factory.Save();
			new LineMerger(declaration).DoMerge();
			declaration.CustomsEntryHeaders[0].CH_BGMReference = "LRN010101";

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManagerForTest(objectParent, notification);

			var cusdecMessageManager = new CUSDECMessageManager(
				objectParent.SendingObjectsCollection.Cast<MessageSendingObject>().First(),
				notification);
			var entry = cusdecMessageManager.EntryHeader;
			AssertEquals(1, messageManager.GetRooCertificateMissingWarningsExposed(entry).Count());
			AssertEquals("LRN010101",
				messageManager.GetRooCertificateMissingWarningsExposed(entry).Select(x => x.Header.CH_BGMReference).First());

			testInvLine1.JI_ROOCert = "X";
			AssertEquals(0, messageManager.GetRooCertificateMissingWarningsExposed(entry).Count());
		}

		void SetupDefaultPreferenceAndTradeAgreement()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var preference200 = helper.CreatePreferenceForCountryAndGrouping("200", "Preferential Rate", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			Factory.Save();

			var startDate = ZDateTime.BrettsBirthday;
			var endDate = ZDateTime.Today.AddDays(1);
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304050", new ZDateTime(2024, 1, 1), new ZDateTime(2079, 12, 1));
			var tariff1P1Rate = helper.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.1 * VFD", preference200.PK);
			Factory.Save();

			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "EU1TEST", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.Netherlands, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(tariff1P1Rate, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
		}

		[TestDate(2017, 11, 2, 14, 8, 0)]
		public void TestAddPendingEntryPayInfosAfterMessagesSent()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst2.CEI_Style = "13";
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine1 = testInvHeader.InvoiceLines.AddNew();
			testInvLine1.JI_CEI = testInst1.PK;
			testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";
			var testInvLine2 = testInvHeader.InvoiceLines.AddNew();
			testInvLine2.JI_CEI = testInst2.PK;
			testInvLine2.JI_Procedure = testInvLine2.EntryInstruction.CEI_Style + "00";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			var entry11 = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().First(x => x.CustomsProcedureCode == "11");
			entry11.CH_Status = "AWA";
			entry11.CH_BGMReference = "BGM11";
			var entry13 = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().First(x => x.CustomsProcedureCode == "13");
			entry13.CH_BGMReference = "BGM13";

			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Factory.Save();
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManager(objectParent, notification);
			notification.NextAnswer = true;
			messageManager.SendMessages();

			AssertEntryPayInfo(entry11, "1");
			AssertEntryPayInfo(entry13, "2");
		}

		static void AssertEntryPayInfo(CusEntryHeader testHeader, ZString cusdecMessageNum)
		{
			AssertEquals(4, testHeader.EntryPayInfos.Count);

			CombineAssertions(() =>
			{
				var testPayInfoDty = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().First(h => h.C9_TransactionType == "DTY");
				AssertEquals("DTY_C9_PaymentDate", new ZDateTime(2017, 11, 2, 14, 8, 0), testPayInfoDty.C9_PaymentDate);
				AssertEquals("DTY_C9_CusResReceived", true, testPayInfoDty.C9_CusResReceived);
				AssertEquals("DTY_C9_RemAdvReceived", false, testPayInfoDty.C9_RemAdvReceived);
				AssertEquals("DTY_C9_PaymentParty", "F", testPayInfoDty.C9_PaymentParty);
				AssertEquals("DTY_C9_PaymentAmount", 0m, testPayInfoDty.C9_PaymentAmount);
				AssertEquals("DTY_C9_IsValid", true, ((ILightValidationInternals)testPayInfoDty).IsValid);
				AssertEquals("DTY_C9_IncommingPayResponseNo", cusdecMessageNum, testPayInfoDty.C9_IncomingPayResponseNo);
				AssertEquals("DTY_C9_PaymentStatus", "PEN", testPayInfoDty.C9_PaymentStatus);

				var testPayInfoVat = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().First(h => h.C9_TransactionType == "VAT");
				AssertEquals("VAT_C9_PaymentDate", new ZDateTime(2017, 11, 2, 14, 8, 0), testPayInfoVat.C9_PaymentDate);
				AssertEquals("VAT_C9_CusResReceived", true, testPayInfoVat.C9_CusResReceived);
				AssertEquals("VAT_C9_RemAdvReceived", false, testPayInfoVat.C9_RemAdvReceived);
				AssertEquals("VAT_C9_PaymentParty", "F", testPayInfoVat.C9_PaymentParty);
				AssertEquals("VAT_C9_PaymentAmount", 0m, testPayInfoVat.C9_PaymentAmount);
				AssertEquals("VAT_C9_IsValid", true, ((ILightValidationInternals)testPayInfoVat).IsValid);
				AssertEquals("VAT_C9_IncommingPayResponseNo", cusdecMessageNum, testPayInfoVat.C9_IncomingPayResponseNo);
				AssertEquals("VAT_C9_PaymentStatus", "PEN", testPayInfoVat.C9_PaymentStatus);

				var testPayInfoOth = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().First(h => h.C9_TransactionType == "OTH");
				AssertEquals("OTH_C9_PaymentDate", new ZDateTime(2017, 11, 2, 14, 8, 0), testPayInfoOth.C9_PaymentDate);
				AssertEquals("OTH_C9_CusResReceived", true, testPayInfoOth.C9_CusResReceived);
				AssertEquals("OTH_C9_RemAdvReceived", false, testPayInfoOth.C9_RemAdvReceived);
				AssertEquals("OTH_C9_PaymentParty", "C", testPayInfoOth.C9_PaymentParty);
				AssertEquals("OTH_C9_PaymentAmount", 0m, testPayInfoOth.C9_PaymentAmount);
				AssertEquals("OTH_C9_IsValid", true, ((ILightValidationInternals)testPayInfoOth).IsValid);
				AssertEquals("OTH_C9_IncommingPayResponseNo", cusdecMessageNum, testPayInfoOth.C9_IncomingPayResponseNo);
				AssertEquals("OTH_C9_PaymentStatus", "PEN", testPayInfoOth.C9_PaymentStatus);

				var testPayInfoPen = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().First(h => h.C9_TransactionType == "PEN");
				AssertEquals("PEN_C9_PaymentDate", new ZDateTime(2017, 11, 2, 14, 8, 0), testPayInfoPen.C9_PaymentDate);
				AssertEquals("PEN_C9_CusResReceived", true, testPayInfoPen.C9_CusResReceived);
				AssertEquals("PEN_C9_RemAdvReceived", false, testPayInfoPen.C9_RemAdvReceived);
				AssertEquals("PEN_C9_PaymentParty", "C", testPayInfoPen.C9_PaymentParty);
				AssertEquals("PEN_C9_PaymentAmount", 0m, testPayInfoPen.C9_PaymentAmount);
				AssertEquals("PEN_C9_IsValid", true, ((ILightValidationInternals)testPayInfoPen).IsValid);
				AssertEquals("PEN_C9_IncommingPayResponseNo", cusdecMessageNum, testPayInfoPen.C9_IncomingPayResponseNo);
				AssertEquals("PEN_C9_PaymentStatus", "PEN", testPayInfoPen.C9_PaymentStatus);
			});
		}

		public void TestAgentCodePersisted()
		{
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ABCDEFG", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_AgentOverride = agent.PK;

			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInvHeader = declaration.Invoices.AddNew();
			testInvHeader.JZ_RelatedIndicator = "N";
			var testInvLine1 = testInvHeader.InvoiceLines.AddNew();
			testInvLine1.JI_CEI = testInst1.PK;
			testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";

			var testInvHeader2 = declaration.Invoices.AddNew();
			testInvHeader2.JZ_RelatedIndicator = "Y";
			var testInvLine2 = testInvHeader2.InvoiceLines.AddNew();
			testInvLine2.JI_CEI = testInst1.PK;
			testInvLine2.JI_Procedure = testInvLine2.EntryInstruction.CEI_Style + "00";

			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			testInvLine2.CusEntryLine.Header.MovementReferenceNumberSetter("DBN201608241234567", ZDateTime.Now);
			Factory.Save();

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();

			var messageManager = new MessageManager(objectParent, notification);
			notification.NextAnswer = true;
			messageManager.SendMessages();

			AssertEquals("JE_AGTCode not persisted as an MRN exists on testInvHeader2", ZString.Empty, declaration.JE_AGTCode);
			AssertEquals("Returns non-persisted AgentCode", "ABCDEFG", declaration.AgentCode);

			testInvLine2.CusEntryLine.Header.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Empty);
			Factory.Save();

			notification.NextAnswer = true;
			messageManager.SendMessages();

			AssertEquals("JE_AGTCode is persisted", "ABCDEFG", declaration.JE_AGTCode);
			AssertEquals("Returns persisted AgentCode", "ABCDEFG", declaration.AgentCode);

			agent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "12345678", Core.Constants.CountryCodes.SouthAfrica);

			notification.NextAnswer = true;
			messageManager.SendMessages();

			AssertEquals("JE_AGTCode is persisted", "ABCDEFG", declaration.JE_AGTCode);
			AssertEquals("Returns persisted AgentCode", "ABCDEFG", declaration.AgentCode);
		}

		public void TestConcurrencyException()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.NameForDebugging = "FACTORY_1";

			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var declaration = factory1.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var agent = factory1.NewWithValidTestData<OrgHeader>();
			agent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ABCDEFG", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_AgentOverride = agent.PK;

			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInvHeader = declaration.Invoices.AddNew();
			testInvHeader.JZ_RelatedIndicator = "N";
			var testInvLine1 = testInvHeader.InvoiceLines.AddNew();
			testInvLine1.JI_CEI = testInst1.PK;
			testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";

			var testInvHeader2 = declaration.Invoices.AddNew();
			testInvHeader2.JZ_RelatedIndicator = "Y";
			var testInvLine2 = testInvHeader2.InvoiceLines.AddNew();
			testInvLine2.JI_CEI = testInst1.PK;
			testInvLine2.JI_Procedure = testInvLine2.EntryInstruction.CEI_Style + "00";

			factory1.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);

			testInvLine2.CusEntryLine.Header.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Empty);
			factory1.Save();

			factory1.RefreshEnabled = false;

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManager(objectParent, notification);

			var factory2 = new BusinessObjectFactory();
			factory2.NameForDebugging = "FACTORY_2";
			factory2.RefreshEnabled = false;
			var dec2 = factory2.Load<JobDeclaration>(declaration.PK);
			dec2.JE_AGTCode = "CONFLICT";
			factory2.Save();

			notification.NextAnswer = true;
			messageManager.SendMessages();

			AssertContains("Message sending failed.\nAnother user/process has modified this declaration and your changes cannot be saved. Please reload this declaration and try sending the messages again.", notification.ErrorNotificationsAsString);
		}

		public void TestErrorPromptSkippedIfExternalCreditApprovalSystemUsed()
		{
			AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code);
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = org.PK;
			declaration.JE_OH_Importer = org.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = ZString.Empty;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			var testInvLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			testInvLine.JI_CEI = testInst.PK;
			testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";

			Factory.Save();

			new LineMerger(declaration).DoMerge();
			testInvLine.CusEntryLine.Header.Charges.AddNew("VAT", 200);
			declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => x.CustomsProcedureCode == "11").CH_BGMReference = "BGM11";

			CombineAssertions("Error popup suppressed only for EXT credit checks", () =>
			{
				declaration.JE_TransportMode = ZString.Empty;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var notification = new MessageNotificationCollector_ForTest();
				var messageManager = new MessageManager(objectParent, notification);
				notification.NextAnswer = true;
				messageManager.SendMessages();
				AssertStartsWith("Auto-rating Error - error notification still present", @"Auto-rating of Customs Disbursement failed:

Failed to load/create Invoicing Job, please manually create a valid Invoicing Job in 'Billing' tab.", notification.LastMessage);
				AssertEquals("Error in Message Sending", notification.LastCaption);

				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				Factory.Save();
				objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				notification = new MessageNotificationCollector_ForTest();
				messageManager = new MessageManager(objectParent, notification);
				notification.NextAnswer = true;
				messageManager.SendMessages();
				var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);
				AssertNotNull(invoiceJob.Logs.Find(x => x.SL_SE_NKEvent == Events.CreditApprovalRequested.Code));
				AssertNotEquals("Error notification not present for external credit check", @"Submit message with credit restriction canceled.", notification.LastMessage);
				AssertNotEquals("Error in Message Sending", notification.LastCaption);
			});
		}

		sealed class MessageManagerForTest : MessageManager
		{
			public MessageManagerForTest(JobDeclarationMessageSendingObjectParent declarationWrapper, IMessageNotificationCollector notification) : base(declarationWrapper, notification) { }

			public IEnumerable<CusEntryLine> GetRooCertificateMissingWarningsExposed(CusEntryHeader entryHeader)
			{
				return GetRooCertificateMissingWarnings(entryHeader);
			}
		}
	}
}
