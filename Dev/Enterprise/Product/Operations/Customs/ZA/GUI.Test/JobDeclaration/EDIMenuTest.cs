using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessagingProcess;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Environment.Testing.UserContextTest;
using LineMerger = Enterprise.Customs.ZA.Business.LineMerger;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		#region SendToCustomsMenuItem

		public void TestSendToCustomsMenuItem()
		{
			Env.Security.ImportMessaging.IsAllowed = true;
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			var testMenu = new EDIMenuForTest();
			AssertEquals(false, testMenu.SendToCustomsMenuItem.Visible);

			AssertNoExceptionThrown("RefreshMenu without Declaration", () => testMenu.RefreshMenu());

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);

			declaration.JE_ApplicationCode = "ITF";
			testMenu.RefreshMenu();
			AssertEquals(false, testMenu.SendToCustomsMenuItem.Visible);

			declaration.JE_ApplicationCode = "BLT";
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);

			Env.Security.ImportMessaging.IsAllowed = false;
			testMenu.RefreshMenu();
			AssertEquals(false, testMenu.SendToCustomsMenuItem.Enabled);

			Env.Security.ImportMessaging.IsAllowed = true;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Enabled);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			Env.Security.ExportMessaging.IsAllowed = false;
			testMenu.RefreshMenu();
			AssertEquals(false, testMenu.SendToCustomsMenuItem.Enabled);

			Env.Security.ExportMessaging.IsAllowed = true;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Enabled);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

			Assert("Pre-condition", declaration.HasChanges);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			testMenu.SendToCustomsMenuItem.PerformClick();
			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(declaration.HasChanges);

			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);
			testMenu.SendToCustomsMenuItem.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			CombineAssertions(() =>
			{
				declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
				Factory.Save();
				var merger = new LineMerger(declaration);
				merger.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				testMenu.SendToCustomsMenuItem.PerformClick();
				AssertEquals("This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = "IMX";
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(false, testMenu.SendToCustomsMenuItem.Visible);

			declaration.JE_MessageType = "IMP";
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);
		}

		public void TestSendToCustomsHavingEntryLineNoMessageError()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var testMenu = new EDIMenuForTest();
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 10000;
			Factory.Save();
			testMenu.SendToCustomsMenuItem.PerformClick();
			AssertContains(ZA.Business.ValidationConstants.Shared.MessageCannotBeSent, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains(ZA.Business.ValidationConstants.Shared.EntryLineNumberExceedMax, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSendToCustomsHavingPreviousEntryLineNoMessageError()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var testMenu = new EDIMenuForTest();
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = Factory.New<DummyJobComInvoiceLine>();
			invoiceLine.JI_PreviousEntryLineNumberReturns = new ZShort("11111");

			invoiceHeader.InvoiceLines.Add(invoiceLine);
			invoiceLine.JI_JZ = invoiceHeader.PK;
			Factory.Save();
			testMenu.SendToCustomsMenuItem.PerformClick();
			AssertContains(Business.ValidationConstants.Shared.MessageCannotBeSent, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains(Business.ValidationConstants.InvoiceLine.PreviousMRNLineNumberMinMaxValue, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		sealed class DummyJobComInvoiceLine : JobComInvoiceLine
		{
			public DummyJobComInvoiceLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZShort JI_PreviousEntryLineNumberReturns { get; set; }

			public override ZShort JI_PreviousEntryLineNumber => JI_PreviousEntryLineNumberReturns;
		}

		public void TestSendToCustomsDeferred_Defer()
		{
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 3 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = CreateDeferrableJob();

				var testMenu = new EDIMenuForTest();
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.SendToCustomsMenuItem.PerformClick();
				AssertEquals("DeferredSubmissionForm is shown when deferable", typeof(DeferredSubmissionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				Factory.Save();
				var cusEntryHeader = (Business.CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
				var message = cusEntryHeader.Messages.First() as EDIMessage;
				AssertNotNull(message);
				AssertEquals("message should have held until date when deferable and deferred", ZDateTime.Today.AddDays(2), message.EM_HeldUntilDate);

				cusEntryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Free;
				cusEntryHeader.Messages.RemoveAndDeleteAllFromTest();
				cusEntryHeader.MessageStatus = "";
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.SendToCustomsMenuItem.PerformClick();
				AssertEquals("DeferredSubmissionForm is not shown when not deferable", typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				Factory.Save();
				var message2 = cusEntryHeader.Messages.First() as EDIMessage;
				AssertNotNull(message2);
				AssertEquals("message should not have a held until date when not deferable and not deferred", ZDateTime.Empty, message2.EM_HeldUntilDate);
			}
		}

		public void TestSendToCustomsDeferred_Cancel()
		{
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 3 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = CreateDeferrableJob();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object formOrDialog) =>
				{
					if (formOrDialog is MessageSendingForm)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
					else if (formOrDialog is DeferredSubmissionForm submissionForm)
					{
						var deferredSubmission = submissionForm.BusinessEntity as DeferredSubmission;
						AssertEquals("Overwritten values do not persist between form useages", false, deferredSubmission.IsOverwritten);
						deferredSubmission.IsOverwritten = true;

						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var testMenu = new EDIMenuForTest();
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);
				testMenu.SendToCustomsMenuItem.PerformClick();
				AssertEquals("DeferredSubmissionForm is shown when deferable", typeof(DeferredSubmissionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				Factory.Save();
				var cusEntryHeader = (Business.CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
				var message = cusEntryHeader.Messages.FirstOrDefault() as EDIMessage;
				AssertNull("message not created when deferable and cancelled", message);

				testMenu.SendToCustomsMenuItem.PerformClick();
				AssertEquals("DeferredSubmissionForm is shown a second time", typeof(DeferredSubmissionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSendToCustomsWhenTransactionStatusIsPending()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Factory.Save();

			var helper = new ZAWhsDataTestHelper(Factory);
			var inwardEntry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			inwardEntry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
			var declaration = inwardEntry.Declaration;
			Factory.Save();

			var testMenu = new EDIMenuForTest();
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertExceptionThrown<ApplicationException>("There is a Warehouse Transaction pending, please close the form and retry when there is a response from Customs. Alternatively, disable the warehouse integration and re-enable it after receiving all response back from Customs.", () => testMenu.SendToCustomsMenuItem.PerformClick());

			inwardEntry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
			Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertExceptionThrown<ApplicationException>("There is a Warehouse Transaction pending, please close the form and retry when there is a response from Customs. Alternatively, disable the warehouse integration and re-enable it after receiving all response back from Customs.", () => testMenu.SendToCustomsMenuItem.PerformClick());
		}

		public void TestSendToCustomsPOC_Visibility()
		{
			var user = new UserForTest();
			user.IsDeveloper = true;
			var context = new UserContextForTest(user, Env.CurrentCompany);

			Env.Security.ImportMessaging.IsAllowed = true;
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			var testMenu = new EDIMenuForTest();
			AssertEquals(false, testMenu.SendToCustomsPOCMenuItem.Visible);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			testMenu.Declaration = declaration;

			using (Env.SetTemporaryUserContext(context))
			{
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.SendToCustomsPOCMenuItem.Visible);

				user.IsDeveloper = false;
				testMenu.RefreshMenu();
				AssertEquals(false, testMenu.SendToCustomsPOCMenuItem.Visible);

				using (MessagingPOCHelper.TemporarilyEnablePOC(enabled: true))
				{
					testMenu.RefreshMenu();
					AssertEquals(true, testMenu.SendToCustomsPOCMenuItem.Visible);
				}
			}
		}

		public void TestSendToCustomsPOC_Click()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var line = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = line.PK;

			using (var mainForm = new ZForm(declaration))
			{
				var testMenu = new EDIMenuForTest();
				testMenu.Declaration = declaration;
				mainForm.Menu.MenuItems.Add(testMenu);
				testMenu.RefreshMenu();

				CombineAssertions("Save Declaration - No", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); // Save?
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // Result
					testMenu.SendToCustomsPOCMenuItem.PerformClick();
					AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertEquals("User canceled, send aborted", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				});

				CombineAssertions("Save Declaration - Yes, SendDialog - Cancel", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save?

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // Result
					testMenu.SendToCustomsPOCMenuItem.PerformClick();
					AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertEquals("User canceled, send aborted", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				});

				CombineAssertions("Already Saved, SendDialog - Send", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Ignore validation errors
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Ignore validation errors
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // Result
					testMenu.SendToCustomsPOCMenuItem.PerformClick();
					AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					AssertEquals("1 Message(s) queued for sending", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				});
			}
		}

		JobDeclaration CreateDeferrableJob()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);

			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("DFM");
			Factory.Save();

			var orgheader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader1.CompanyData.OB_IsCreditor = true;
			orgheader1.OH_FullName = "ORG1";
			orgheader1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
			var orgheader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader2.CompanyData.OB_IsCreditor = true;
			orgheader2.OH_FullName = "ORG2";
			orgheader2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "22334455", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var mapping1 = maps.AddNew();
			mapping1.AccountStartDay = 5;
			mapping1.OrganizationPK = orgheader1.PK;
			mapping1.CreditorPK = orgheader1.PK;
			mapping1.CustomsOfficeCode = "DFM";
			mapping1.Cash = false;
			mapping1.ImporterPays = false;
			mapping1.FinancialAccountNumber = "1111111111";
			mapping1.DutyDefermentAmount = 100;
			var mapping2 = maps.AddNew();
			mapping2.AccountStartDay = 5;
			mapping2.OrganizationPK = orgheader2.PK;
			mapping2.CreditorPK = orgheader2.PK;
			mapping2.CustomsOfficeCode = "DFM";
			mapping2.Cash = false;
			mapping2.ImporterPays = false;
			mapping2.FinancialAccountNumber = "2222222222";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);

			var declaration = GetDeclaration();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "IMP1";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(Enterprise.Customs.ZA.Business.OrgCusAccountProvider.FANCode, "1111111111", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_Importer = importer.PK;
			Factory.Save();

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.AgentCode = "11223344";
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(5);
			declaration.DoMerge();
			Factory.Save();

			var cusEntryHeader = (Business.CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			AssertEquals("ValueAddedTax should not be 0", 22.5m, cusEntryHeader.CustomsDuty);
			AssertEquals("Duty should not be 0", 14.7m, cusEntryHeader.ValueAddedTax);
			AssertContains("CH_BGMReference", "11223344", cusEntryHeader.CH_BGMReference);
			AssertEquals("PaymentMethod should be Defer", PaymentMethodCodeList.Codes.Defer, cusEntryHeader.CH_PaymentMethod);

			return declaration;
		}

		JobDeclaration GetDeclaration()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, Business.UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Enterprise.Customs.Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();

			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			var startDate = ZDateTime.Today.AddYears(0);
			var endDate = ZDateTime.Today.AddYears(1);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99999", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.3 * VFD", preference.PK);
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			helper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var testApplicability1 = helper.CreateCusApplicability(tariff1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_CustomsOffice = "DFM";

			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = "FOB";
			invHeader.JZ_InvoiceAmount = 1m;
			invHeader.JZ_RX_NKInvoice_Currency = "ZAR";
			var invLine1 = invHeader.InvoiceLines.AddNew();
			invLine1.JI_CEI = instruction1.PK;
			invLine1.JI_Procedure = invLine1.EntryInstruction.CEI_Style + "00";
			invLine1.JI_Tariff = "99999";
			invLine1.JI_PrimaryPreference = Business.UniversalReferenceConstants.PrimaryPreference.Standard;
			invLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invLine1.JI_LinePrice = 75m;
			invLine1.JI_ZZF_NKTaxType = "VAT";

			return declaration;
		}

		#endregion

		#region CancelPendingSubmissionsMenuItem

		public void TestCancelPendingSubmissionsMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var normalMessage = Factory.New<CUSDECEDIMessage>();
			normalMessage.EM_LinkedObject = entryHeader;
			normalMessage.EM_Status = EDIMessage.Status.Queued;
			normalMessage.EM_HeldUntilDate = ZDateTime.Empty;

			var testMenu = new EDIMenuForTest();
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals("CancelPendingSubmissions is hidden", false, testMenu.CancelPendingSubmissionsMenuItem.Visible);
			AssertEquals("SendToCustoms is enabled", true, testMenu.SendToCustomsMenuItem.Enabled);
			AssertEquals("GenerateEntries is enabled", true, testMenu.GenerateEntriesMenuItem.Enabled);

			var heldMessage = Factory.New<CUSDECEDIMessage>();
			heldMessage.EM_LinkedObject = entryHeader;
			heldMessage.EM_Status = EDIMessage.Status.Queued;
			heldMessage.EM_HeldUntilDate = ZDateTime.Today.AddDays(1);

			entryHeader.Messages.Reload(true);

			testMenu.RefreshMenu();
			AssertEquals("CancelPendingSubmissions is visible", true, testMenu.CancelPendingSubmissionsMenuItem.Visible);
			AssertEquals("SendToCustoms is disabled", false, testMenu.SendToCustomsMenuItem.Enabled);
			AssertEquals("GenerateEntries is disabled", false, testMenu.GenerateEntriesMenuItem.Enabled);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			testMenu.CancelPendingSubmissionsMenuItem.PerformClick();

			AssertEquals("Held message is cancelled", EDIMessage.Status.Cancelled, heldMessage.EM_Status);
			AssertEquals("Un-held message remains queued", EDIMessage.Status.Queued, normalMessage.EM_Status);
			AssertEquals("1 Pending Submissions were Canceled.", UnitTestUserNotification.Instance.LastMessage.Text);

			testMenu.RefreshMenu();
			AssertEquals("CancelPendingSubmissions is hidden", false, testMenu.CancelPendingSubmissionsMenuItem.Visible);
			AssertEquals("SendToCustoms is enabled", true, testMenu.SendToCustomsMenuItem.Enabled);
			AssertEquals("GenerateEntries is enabled", true, testMenu.GenerateEntriesMenuItem.Enabled);
		}

		#endregion

		public void TestBondedWarehouseMenuItems()
		{
			using (var menu = new EDIMenu())
			{
				var helper = new ZAWhsDataTestHelper(Factory);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = helper.Importer.PK;
				var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_Description = "DESC 1";
				entryInstruction1.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
				helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;
				var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Description = "DESC 2";
				entryInstruction2.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				entry1.CH_CEI_Instruction = entryInstruction1.PK;
				entry1.CH_BGMReference = "BGM1223";
				entry1.EntryNumber = "ENT1231";
				var entry1Line = entry1.MergedLines.AddNew();
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				entry2.CH_CEI_Instruction = entryInstruction2.PK;
				entry2.CH_BGMReference = "BGM5865";
				entry2.EntryNumber = "ENT65644";
				var entry2Line = entry2.MergedLines.AddNew();

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_JZ = invoice.PK;
				invoiceLine1.JI_CEI = entryInstruction1.PK;
				invoiceLine1.JI_CL = entry1Line.PK;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_JZ = invoice.PK;
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine2.JI_CL = entry2Line.PK;

				menu.Declaration = declaration;
				CombineAssertions(() =>
				{
					menu.RefreshMenu();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", false, bondedWarehouseMenuItem.Visible);

					entryInstruction2.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
					entryInstruction2.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
					invoiceLine2.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
					AssertEquals("entry1.IsInwardBondedWarehousingEnabled", false, entry1.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry1.IsOutwardBondedWarehousingEnabled", false, entry1.IsOutwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsInwardBondedWarehousingEnabled", true, entry2.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsOutwardBondedWarehousingEnabled", false, entry2.IsOutwardBondedWarehousingEnabled);
					menu.RefreshMenu();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);

					AssertEquals("bondedWarehouseMenuItem.MenuItems.Count", 10, bondedWarehouseMenuItem.MenuItems.Count);
					var entryMenuItem1 = bondedWarehouseMenuItem.MenuItems[0];
					AssertMenuItem(entryMenuItem1, entry2.EntryHeaderDescriptiveMenuItemText, true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[1], "S&elect Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[2], "&Synchronize with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[3], "Update Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[4], "Cancel Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[5], "&Finalize Stock with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[6], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[7], "&Disable Integration", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[8], "Synchronize With Orders", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[9], "Select &Order Lines", false);

					AssertEquals("entryMenuItem1.Visible", true, entryMenuItem1.Visible);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					entry2.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
					AssertEquals("entry1.IsInwardBondedWarehousingEnabled", false, entry1.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry1.IsOutwardBondedWarehousingEnabled", false, entry1.IsOutwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsInwardBondedWarehousingEnabled", true, entry2.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsOutwardBondedWarehousingEnabled", false, entry2.IsOutwardBondedWarehousingEnabled);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					entry2.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					AssertEquals("entry1.IsInwardBondedWarehousingEnabled", false, entry1.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry1.IsOutwardBondedWarehousingEnabled", false, entry1.IsOutwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsInwardBondedWarehousingEnabled", true, entry2.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsOutwardBondedWarehousingEnabled", false, entry2.IsOutwardBondedWarehousingEnabled);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", false);

					entryInstruction1.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
					entryInstruction1.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
					invoiceLine1.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
					invoiceLine1.JI_PreviousEntryNumber = "ENT1";
					invoiceLine1.JI_PreviousEntryLineNumber = 1;
					entryInstruction2.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
					invoiceLine2.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
					invoiceLine2.JI_PreviousEntryNumber = "ENT1";
					invoiceLine2.JI_PreviousEntryLineNumber = 2;
					entry2.CH_WarehouseTransactionStatus = ZString.Empty;
					AssertEquals("entry1.IsInwardBondedWarehousingEnabled", false, entry1.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry1.IsOutwardBondedWarehousingEnabled", true, entry1.IsOutwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsInwardBondedWarehousingEnabled", false, entry2.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsOutwardBondedWarehousingEnabled", true, entry2.IsOutwardBondedWarehousingEnabled);
					menu.RefreshMenu();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);

					AssertEquals("bondedWarehouseMenuItem.MenuItems.Count", 11, bondedWarehouseMenuItem.MenuItems.Count);
					entryMenuItem1 = bondedWarehouseMenuItem.MenuItems[0];
					AssertMenuItem(entryMenuItem1, entry1.EntryHeaderDescriptiveMenuItemText, true);
					var entryMenuItem2 = bondedWarehouseMenuItem.MenuItems[1];
					AssertMenuItem(entryMenuItem2, entry2.EntryHeaderDescriptiveMenuItemText, true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[2], "S&elect Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[3], "&Synchronize with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[4], "Update Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[5], "Cancel Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[6], "&Finalize Stock with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[7], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[8], "&Disable Integration", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[9], "Synchronize With Orders", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[10], "Select &Order Lines", false);

					AssertEquals("entryMenuItem1.Visible", true, entryMenuItem1.Visible);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					AssertEquals("entryMenuItem2.Visible", true, entryMenuItem2.Visible);
					entryMenuItem2.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem2.MenuItems.Count", 6, entryMenuItem2.MenuItems.Count);
					AssertMenuItem(entryMenuItem2.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem2.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem2.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					entryInstruction1.CEI_Style = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
					invoiceLine1.JI_Procedure = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode + helper.ChangeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
					entry1.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated;
					AssertEquals("entry1.IsInwardBondedWarehousingEnabled", true, entry1.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry1.IsOutwardBondedWarehousingEnabled", true, entry1.IsOutwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsInwardBondedWarehousingEnabled", false, entry2.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsOutwardBondedWarehousingEnabled", true, entry2.IsOutwardBondedWarehousingEnabled);
					AssertEquals("entryMenuItem1.Visible", true, entryMenuItem1.Visible);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", true);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					entryInstruction1.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
					invoiceLine1.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
					entry1.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
					AssertEquals("entry1.IsInwardBondedWarehousingEnabled", false, entry1.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry1.IsOutwardBondedWarehousingEnabled", true, entry1.IsOutwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsInwardBondedWarehousingEnabled", false, entry2.IsInwardBondedWarehousingEnabled);
					AssertEquals("entry2.IsOutwardBondedWarehousingEnabled", true, entry2.IsOutwardBondedWarehousingEnabled);
					AssertEquals("entryMenuItem1.Visible", true, entryMenuItem1.Visible);
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem1.MenuItems.Count", 6, entryMenuItem1.MenuItems.Count);
					AssertMenuItem(entryMenuItem1.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem1.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem1.MenuItems[2], "&Cancel Inventory Stock Release", true);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					AssertEquals("entryMenuItem2.Visible", true, entryMenuItem2.Visible);
					entryMenuItem2.OnPopup(EventArgs.Empty);
					AssertEquals("entryMenuItem2.MenuItems.Count", 6, entryMenuItem2.MenuItems.Count);
					AssertMenuItem(entryMenuItem2.MenuItems[0], "&Update Inventory", true);
					AssertMenuItem(entryMenuItem2.MenuItems[1], "&Cancel Inventory", false);
					AssertMenuItem(entryMenuItem2.MenuItems[2], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(entryMenuItem1.MenuItems[3], "&Cancel Bonded Warehouse Change Of Ownership", false);
					AssertMenuItem(entryMenuItem1.MenuItems[4], "&Cancel Inventory Change Of Regime", false);
					AssertMenuItem(entryMenuItem1.MenuItems[5], "&Disable Integration", true);

					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
					menu.RefreshMenu();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					AssertEquals("bondedWarehouseMenuItem.MenuItems.Count", 11, bondedWarehouseMenuItem.MenuItems.Count);
					entryMenuItem1 = bondedWarehouseMenuItem.MenuItems[0];
					AssertMenuItem(entryMenuItem1, entry1.EntryHeaderDescriptiveMenuItemText, true);
					entryMenuItem2 = bondedWarehouseMenuItem.MenuItems[1];
					AssertMenuItem(entryMenuItem2, entry2.EntryHeaderDescriptiveMenuItemText, true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[2], "S&elect Inventory", true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[3], "&Synchronize with Inventory", true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[4], "Update Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[5], "Cancel Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[6], "&Finalize Stock with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[7], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[8], "&Disable Integration", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[9], "Synchronize With Orders", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[10], "Select &Order Lines", false);

					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					menu.RefreshMenu();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals("bondedWarehouseMenuItem.Visible", true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					AssertEquals("bondedWarehouseMenuItem.MenuItems.Count", 11, bondedWarehouseMenuItem.MenuItems.Count);
					entryMenuItem1 = bondedWarehouseMenuItem.MenuItems[0];
					AssertMenuItem(entryMenuItem1, entry1.EntryHeaderDescriptiveMenuItemText, true);
					entryMenuItem2 = bondedWarehouseMenuItem.MenuItems[1];
					AssertMenuItem(entryMenuItem2, entry2.EntryHeaderDescriptiveMenuItemText, true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[2], "S&elect Inventory", true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[3], "&Synchronize with Inventory", true);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[4], "Update Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[5], "Cancel Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[6], "&Finalize Stock with Inventory", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[7], "&Cancel Inventory Stock Release", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[8], "&Disable Integration", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[9], "Synchronize With Orders", false);
					AssertMenuItem(bondedWarehouseMenuItem.MenuItems[10], "Select &Order Lines", false);
				});
			}
		}

		void AssertMenuItem(MenuItem menuItem, string text, bool visible)
		{
			AssertEquals("menuItem.Text", text, menuItem.Text);
			AssertEquals("menuItem.Visible", visible, menuItem.Visible);
		}

		public void TestAllocateEntryInstructionsMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var menuItemText = EDIMenu.AllocateEntryInstructionsMenuText;
				var item = menu.MenuItems.FindByText(menuItemText);
				AssertNotNull("Allocate Entry Instructions MenuItem", item);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				menu.RefreshMenu();
				AssertEquals("Allocate Entry Instructions MenuItem should be visible for import declaration.", true, item.Visible);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				menu.RefreshMenu();
				AssertEquals("Allocate Entry Instructions MenuItem should not be visible for export declaration.", false, item.Visible);
			}
		}

		#region SendSupportingDocsMenuItem

		public void TestSendSupportingDocsMenuItem()
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);
			var testMenu = new EDIMenuForTest();
			AssertEquals(false, testMenu.SendSupportingDocsMenuItem.Visible);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OH_AgentOverride = ZGuid.Empty;
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendSupportingDocsMenuItem.Visible);

			declaration.JE_ApplicationCode = "ITF";
			testMenu.RefreshMenu();
			AssertEquals(false, testMenu.SendSupportingDocsMenuItem.Visible);

			declaration.JE_ApplicationCode = "BLT";
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendSupportingDocsMenuItem.Visible);

			Assert("Pre-condition", declaration.HasChanges);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			testMenu.SendSupportingDocsMenuItem.PerformClick();
			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(declaration.HasChanges);

			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendToCustomsMenuItem.Visible);

			testMenu.SendSupportingDocsMenuItem.PerformClick();
			AssertEquals(ZA.Business.ValidationConstants.SupportingDocSendingManager.EmptyTradingPartyIDError + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);

			var agentOrg = Factory.New<OrgHeader>();
			agentOrg.OH_Code = "AGENTCODE";
			agentOrg.OH_FullName = "AgentName";
			declaration.JE_OH_AgentOverride = agentOrg.PK;
			agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AGT7634", Core.Constants.CountryCodes.SouthAfrica);
			agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "CDPC", Core.Constants.CountryCodes.SouthAfrica);
			declaration.AgentCode = "AGT7634";

			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.SendToCustomsMenuItem.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			CombineAssertions(() =>
			{
				declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
				Factory.Save();
				var merger = new LineMerger(declaration);
				merger.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				testMenu.SendToCustomsMenuItem.PerformClick();
				AssertEquals("This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = "IMX";
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(false, testMenu.SendSupportingDocsMenuItem.Visible);

			declaration.JE_MessageType = "IMP";
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendSupportingDocsMenuItem.Visible);
		}

		#endregion

		public void TestSetDeclarationResetPortMessagingMenuItems()
		{
			var declaration1 = CreateCommonJobDeclaration();
			var declaration2 = CreateCommonJobDeclaration();
			using (var menu = new EDIMenu())
			{
				AssertNull("No Port Messaging", menu.MenuItems.FindByText("Port Messaging"));
				// assert that there is no port messaging menu items.
				menu.Declaration = declaration1;
				var menuItem = GetAndAssertPortMessagingMenuItem(menu);
				var menuItemIsDisposed = false;
				var action = new EventHandler((sender, e) => { menuItemIsDisposed = true; });
				menuItem.Disposed += action;
				menu.Declaration = declaration1;
				AssertCollectionContains(menuItem, menu.MenuItems);
				AssertEquals(false, menuItemIsDisposed);
				_ = GetAndAssertPortMessagingMenuItem(menu);
				AssertCollectionContains(menuItem, menu.MenuItems);
				AssertEquals(false, menuItemIsDisposed);
				menu.Declaration = declaration2;
				AssertNull("No Port Messaging", menu.MenuItems.FindByText("Port Messaging"));
				AssertCollectionNotContains(menuItem, menu.MenuItems);
				AssertEquals(true, menuItemIsDisposed);
				menuItem.Disposed -= action;
				_ = GetAndAssertPortMessagingMenuItem(menu);
			}
		}

		public void TestDA66DA63MenuItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var menuItemText = EDIMenu.RefundMessagingDA66DA63MenuText;
				var subMenuItemText = EDIMenu.ApplicationForRefundDA66DA63MenuText;

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				menu.RefreshMenu();
				var item = menu.MenuItems.FindByText(menuItemText);
				AssertEquals("no menu for imports", null, item);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				menu.RefreshMenu();
				item = menu.MenuItems.FindByText(menuItemText);
				AssertMenuItem(item, "Refund Messaging (DA63)", true);
				var subItem = item.MenuItems.FindByText(subMenuItemText);
				AssertMenuItem(subItem, "Application for Refund (DA63)", true);

				menu.OnPopup(EventArgs.Empty);
				var menuItem = menu.MenuItems.FindByText("Refund Messaging (DA63)") as ZMenuItem;
				menuItem.OnPopup(EventArgs.Empty);
				AssertEquals(
@"Refund Messaging (DA63)
   Application for Refund (DA63)",
				menuItem.GetVisibleMenuItemsCaptions());
			}
		}

		#region Cargo Dues Menu Items
		public void TestCargoDuesMenuItems_Import()
		{
			var declaration = CreateCommonJobDeclaration();
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var menuItem = GetAndAssertPortMessagingMenuItem(menu);
				menuItem.OnPopup(EventArgs.Empty);
				AssertEquals(
@"Port Messaging
   Cargo Dues Order (ZA)
      Import",
				menuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestCargoDuesMenuItems_Export()
		{
			var declaration = CreateCommonJobDeclaration();
			declaration.JE_RL_NKOrigin = "ZAJNB";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var menuItem = GetAndAssertPortMessagingMenuItem(menu);
				menuItem.OnPopup(EventArgs.Empty);
				AssertEquals(
@"Port Messaging
   Cargo Dues Order (ZA)
      Export",
				menuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestCargoDuesMenuItems_Coastwise()
		{
			var declaration = CreateCommonJobDeclaration();
			declaration.JE_RL_NKOrigin = "ZAJNB";
			declaration.JE_RL_NKFinalDestination = "ZAJSV";
			declaration.Transports.AddNew("ZAJNB", "ZAJUP");
			declaration.Transports.AddNew("ZAJUP", "ZAJSV");
			foreach (Transport transport in declaration.Transports)
			{
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			}
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var menuItem = GetAndAssertPortMessagingMenuItem(menu);
				menuItem.OnPopup(EventArgs.Empty);
				AssertEquals(
@"Port Messaging
   Cargo Dues Order (ZA)
      Load Coastwise
      Discharge Coastwise",
				menuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestCargoDuesMenuItems_RemovePortMessagingMenuItem()
		{
			using (var menu = new EDIMenu())
			{
				var declaration1 = CreateCommonJobDeclaration();
				declaration1.JE_RL_NKOrigin = "ZAJNB";
				declaration1.JE_RL_NKFinalDestination = "AUSYD";
				menu.Declaration = declaration1;
				var menuItem = GetAndAssertPortMessagingMenuItem(menu);
				menuItem.OnPopup(EventArgs.Empty);
				AssertEquals(
@"Port Messaging
   Cargo Dues Order (ZA)
      Export",
				menuItem.GetVisibleMenuItemsCaptions());

				var declaration2 = CreateCommonJobDeclaration();
				declaration2.JE_RL_NKOrigin = "ZAJNB";
				declaration2.JE_RL_NKFinalDestination = "AUSYD";
				var shipment = Factory.New<ForwardingShipment>();
				declaration2.JE_JS = shipment.PK;
				menu.Declaration = declaration2;
				menu.OnPopup(EventArgs.Empty);
				var item = menu.MenuItems.FindByText("Port Messaging");
				AssertNull("Sub menu item `Port Messaging` should NOT exists", item);
			}
		}

		ZMenuItem GetAndAssertPortMessagingMenuItem(EDIMenu menu)
		{
			menu.OnPopup(EventArgs.Empty);
			var item = menu.MenuItems.FindByText("Port Messaging") as ZMenuItem;
			AssertNotNull("Sub menu item `Port Messaging` should exists", item);
			return item;
		}

		JobDeclaration CreateCommonJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			return declaration;
		}
		#endregion
	}

	sealed class EDIMenuForTest : EDIMenu
	{
		public MenuItem CancelPendingSubmissionsMenuItem => base.cancelPendingSubmissionsMenuItem;
		public new MenuItem GenerateEntriesMenuItem => base.GenerateEntriesMenuItem;
		public MenuItem SendSupportingDocsMenuItem => base.sendSupportingDocsMenuItem;
		public MenuItem SendToCustomsMenuItem => base.sendToCustomsMenuItem;
		public MenuItem SendToCustomsPOCMenuItem => base.sendToCustomsPOCMenuItem;
	}
}
