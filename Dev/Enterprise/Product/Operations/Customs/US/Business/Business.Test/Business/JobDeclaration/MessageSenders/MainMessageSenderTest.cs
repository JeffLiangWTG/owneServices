using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class MainMessageSenderTest : MessageSenderTest
	{
		public void TestSendOriginalMessageWithoutMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNull(entry);

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease);
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals("In order to send an Original Cargo Release message, Enable Cargo Release must be ticked. Please tick the Enable Cargo Release tick box on the Declaration tab in order to send an Original Cargo Release message.", ((SendsMessagesToCustomsShutterUpperer)(declaration.MessageInitiator)).Warning);
		}

		public void TestCargoReleaseMessageShouldHaveEnableCROrCertifyCR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			declaration.DoMerge();
			Factory.Save();

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Replacement, ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease);
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals("In order to send a Cargo Release message, please tick either Enable Cargo Release or Certify Cargo Rel from Sum on the Declaration tab.", ((SendsMessagesToCustomsShutterUpperer)(declaration.MessageInitiator)).Warning);
		}

		public void TestSendOriginalMessageForSE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			declaration.DoMerge();

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd;
			Factory.Save();

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease);
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals("In order to send an Original Cargo Release message, Enable Cargo Release must be ticked. Please tick the Enable Cargo Release tick box on the Declaration tab in order to send an Original Cargo Release message.", ((SendsMessagesToCustomsShutterUpperer)(declaration.MessageInitiator)).Warning);

			entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease);
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals("ACE Cargo Release message has already been accepted. You must use the Send Replacement/Update Messages option to make ACE Cargo Release changes.", ((SendsMessagesToCustomsShutterUpperer)(declaration.MessageInitiator)).Warning);
		}

		public void TestSendMessageUseMutex()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decInFactory2 = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals(true, decInFactory2.LockImportEntryNumberAllocationMutex);

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
			SetupMainSender(mainSender);
			AssertEquals(false, mainSender.SendMessage());
			var warning = declaration.GetImportEntryNumberAllocationMutexLockInfo() + " is in the process of allocating Import Entry Number for this job; system cannot merge and send the data as it will result in a different Import Entry Number being allocated.\r\nPlease retry sending when the other user has finished.";
			AssertEquals(warning, initiator.Warning);

			initiator.Warning = "";
			decInFactory2.ImportEntryNumber = "ENT3234";
			factory2.Save();
			AssertEquals(true, decInFactory2.LockImportEntryNumberAllocationMutex);
			AssertEquals(true, mainSender.SendMessage());
			AssertEquals("", initiator.Warning);
			AssertEquals("ENT3234", declaration.ImportEntryNumber);
			decInFactory2.UnlockImportEntryNumberAllocationMutex();
		}

		protected void SetupMainSender(MainMessageSender mainSender)
		{
			mainSender.OnPrepare += new MainMessageSender.PrepareEventHandler(delegate
			{ return true; });
			mainSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate
			{ return true; });
			mainSender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ Factory.Save(); });
		}

		public void TestAddATHEventForACE()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.IseBondAutoSendACEMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = "SE";
			declaration.US_EnableENS = true;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CTS;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;

			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			invoiceLine.JI_CL = ensEntry.MergedLines.AddNew().PK;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.Factory.Save();
			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			mainSender.Actions.Cast<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);

			AssertNotNull(eBondLogger.GetAutoSendEvent(ensEntry.Logs));
		}

		public void TestCensusWarningAndUS_PaidCopiedBeforeSendingMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			declaration.DoMerge();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Replacement);
			var actions = mainSender.Actions;
			actions[0].US_Paid = YesNoDefaultList.Codes.Yes;
			var censusWarning = actions[0].CensusWarningCodes.AddNew();
			censusWarning.ConditionCode = CensusWarningCodeList.Codes.LowValueDividedByQty1;
			censusWarning.OverrideCode = CensusOverrideCodeList.Codes._01;
			censusWarning.EntryLinePK = declaration.InvoiceLines[0].JI_CL;

			actions[0].US_SendMessage = true;
			actions[0].US_AcknowledgeAndSign = true;

			SetupMainSender(mainSender);
			mainSender.SendMessage();

			var message = (MQEDIMessage)entry.Messages[0];

			var ens10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals("Payment detail should not be sent when US_Paid is indicated as Y", "", ens10.PaymentTypeCode);

			var cw02 = message.MessageBlock.MessageBlocks.OfType<AENSCW02>().FirstOrDefault();
			AssertNotNull(cw02);
			AssertEquals(CensusWarningCodeList.Codes.LowValueDividedByQty1, cw02.CensusWarningConditionCode1);
			AssertEquals(CensusOverrideCodeList.Codes._01, cw02.CensusWarningConditionOverrideCode1);
		}

		public void TestCreditOnHold_CreditCheckDoneExternally()
		{
			AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code);

			SetupAndAssertCreditOnHoldTestCase(true);
		}

		public void TestCreditOnHold_CreditCheckDoneWithinCW1()
		{
			using (ZArchitecture.Environment.Globals.SetIsWinzorForTest(true))
			using (ZArchitecture.Environment.Globals.SetIsUserInteractiveForTest(true))
			{
				AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveLocallyInCW1.Code);

				SetupAndAssertCreditOnHoldTestCase(false);
			}
		}

		protected virtual void SetupAndAssertCreditOnHoldTestCase(bool isCreditCheckPerformedExternally)
		{
			var declaration = SetupDeclarationForDisbursementAmount();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.CompanyData.OB_AROnCreditHold = true;
			declaration.Importer.CompanyData.OB_IsDebtor = true;
			declaration.US_EntryFilerCode = "XJ5";

			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			declaration.DoMerge();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.NotSent;
			Factory.Save();

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
			var actions = mainSender.Actions;
			actions[0].US_Paid = YesNoDefaultList.Codes.Yes;
			actions[0].US_SendMessage = true;
			actions[0].US_AcknowledgeAndSign = true;

			SetupMainSender(mainSender);
			AssertEquals(false, mainSender.SendMessage());
			AssertEquals("Credit limiting on -- status should remain unchanged", ImportMessageStatusList.Codes.NotSent, entry.CH_Status);

			var notifier = mainSender.MessageInitiator as SendsMessagesToCustomsShutterUpperer;
			string expectedWarning = null;
			if (!isCreditCheckPerformedExternally)
			{
				expectedWarning = "Submit message with credit restriction canceled.";
				AssertEquals("Canceled due to credit restriction", expectedWarning, notifier.Warning);

				declaration.Importer.CompanyData.OB_AROnCreditHold = false;
				declaration.Importer.CompanyData.OB_IsDebtor = false;
				Factory.Save();

				AssertEquals(true, mainSender.SendMessage());

				AssertEquals("Credit limiting off -- should now be 'lodged' with customs", ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, entry.CH_Status);

				entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
				declaration.Importer.CompanyData.OB_AROnCreditHold = true;
				declaration.Importer.CompanyData.OB_IsDebtor = true;
				declaration.InvoiceLines[0].JI_CustomsQuantity = 900m;
				declaration.DoMerge();
				Factory.Save();
				AssertEquals(true, entry.HasBeenLodgedAtCustoms);
				var notifier2 = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.MessageInitiator = notifier2;
				var mainSender2 = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
				actions = mainSender2.Actions;
				actions[0].US_Paid = YesNoDefaultList.Codes.Yes;
				actions[0].US_SendMessage = true;
				actions[0].US_AcknowledgeAndSign = true;
				SetupMainSender(mainSender2);

				var outgoingMessage = Factory.New<MQEDIMessage>();
				outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
				outgoingMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
				outgoingMessage.EM_MessageText =
	"B  3901JJ8AE                                                                    " +
	"10AJJ8  85451101 3901            06   X           3121720          F            " +
	"1113-14202600013-142026000               120220      022AB01IL                  " +
	"20    3901      HA68                                                            " +
	"318B 913                                                                        " +
	"40  002 SESE092120        0000000005     0000000063    N                        " +
	"41P1016200000000063                                                             " +
	"47MSEUDDAB683HAG                                                                " +
	"47S13-142026000                                                                 " +
	"507220110000 0000000000 0000000369 000000006300KG                               " +
	"5201H6ANBK070                                                                   " +
	"5403STL154698                                                                   " +
	"6249900000128                                                                   " +
	"8800000000000 00000000000 00000000000 00000000000                               " +
	"8949900000052833                                                                " +
	"9000000000000 00000000000 00000007148 00000000000 00000000000 00000042000       " +
	"Y  3901JJ8AE                                                                    ";
				outgoingMessage.EM_MessageNum = "HYEDUSCMT_236622";
				entry.Messages.Add(outgoingMessage);

				var incomingMsg = Factory.New<MQEDIMessage>();
				incomingMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				incomingMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				incomingMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				incomingMsg.EM_MessageNum = "HYEDUSCMT_236622";
				incomingMsg.EM_MessageText =
	"B008888XJ5AX                                               HYEDUSCMT_236622     " +
	"E0 SUMMRY 000001 REF ID: SV9 71017956 B00165528    171                          " +
	"E0 LINITM 000001 REF ID: 001                                                    " +
	"E1 IQ10   LINE SUBJECT TO QUOTA                   SV9  71017956                 " +
	"E0 PSTLIN 000001 REF ID: SV9 71017956 B00165528                                 " +
	"E1 IQ08   QUOTA PROCESS PENDING                   SV9  71017956                 " +
	"E1AI995   SUMMARY HAS BEEN ADDED                  SV9  7101795600100B00165528   " +
	"Y  8888XJ5AX00006                                                               ";

				entry.Messages.Add(incomingMsg);
				Factory.Save();
				AssertEquals(false, mainSender2.SendMessage());

				AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, entry.CH_Status);
				AssertEquals("Canceled due to credit restriction", expectedWarning, notifier2.Warning);
			}
			else
			{
				AssertEquals("No popup with message - Submit message with credit restriction canceled.", null, notifier.Warning);
			}
		}

		[TestDate(2012, 3, 1)]
		public void TestRefreshExchangeRatesBeforeSendingMessages()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			audCurr.ExchangeRates.DeleteAll();

			audCurr.SetUpExchangeRates(ZDateTime.Now, 1.05m);
			audCurr.SetUpExchangeRates(ZDateTime.Now.AddDays(1), 1.06m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(2);
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "CR";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice.JobComInvoiceLines.AddNew();
			declaration.ResumeApportionment();
			AssertEquals("Exchange rate", 1.06m, invoice.JZ_InvoiceCurrExRate);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			audCurr.SetUpExchangeRates(ZDateTime.Now.AddDays(2), 1.07m);
			declaration.ResumeApportionment();
			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
			var actions = mainSender.Actions;
			SetupMainSender(mainSender);
			mainSender.SendMessage();

			AssertEquals("Exchange rate is refreshed", 1.07m, invoice.JZ_InvoiceCurrExRate);
		}

		[TestDate(2017, 12, 17)]
		public void TestMPFAndDutyDatesAreStored()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1020304050";
			tariff1.UE_Unit1 = "KG";
			tariff1.UE_DateFrom = new ZDateTime(2017, 1, 1);
			tariff1.UE_DateTo = new ZDateTime(2017, 12, 31);
			tariff1.UE_Unit1 = Core.Constants.Weight.Kilograms;
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff1.UE_Column1RateAdValorem = 0.162m;
			tariff1.UE_Column2RateAdValorem = 0.370m;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1020304050";
			tariff2.UE_Unit1 = "KG";
			tariff2.UE_DateFrom = new ZDateTime(2018, 1, 1);
			tariff2.UE_DateTo = new ZDateTime(2018, 12, 31);
			tariff2.UE_Unit1 = Core.Constants.Weight.Kilograms;
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff2.UE_Column1RateAdValorem = 0.255m;
			tariff2.UE_Column2RateAdValorem = 0.451m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_ExportDate = new ZDateTime(2017, 12, 11);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2018, 1, 1);
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_Tariff = "1020304050";
			invoiceLine.JI_CustomsQuantity = 10m;
			declaration.DoMerge();

			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			var actions = mainSender.Actions.OfType<EntryHeaderMessageSendingAction>().ToArray();
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals(255m, entrySummaryEntry.TotalDutyAmount);
			AssertEquals(new ZDateTime(2018, 1, 1), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(25.67m, entrySummaryEntry.MPFAmountForEntry);
			AssertEquals(new ZDateTime(2018, 1, 1), entrySummaryEntry.US_MPFCalcDate);
			AssertEquals(25.67m, invoiceLine.US_PayableMPF);
			AssertEquals(255m, invoiceLine.US_Duty);
			declaration.UnlockImportEntryNumberAllocationMutex();
		}

		[TestDate(2017, 12, 17)]
		public void TestMPFAndDutyIsRecalculated()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1020304050";
			tariff1.UE_Unit1 = "KG";
			tariff1.UE_DateFrom = new ZDateTime(2017, 1, 1);
			tariff1.UE_DateTo = new ZDateTime(2017, 12, 31);
			tariff1.UE_Unit1 = Core.Constants.Weight.Kilograms;
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff1.UE_Column1RateAdValorem = 0.162m;
			tariff1.UE_Column2RateAdValorem = 0.370m;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1020304050";
			tariff2.UE_Unit1 = "KG";
			tariff2.UE_DateFrom = new ZDateTime(2018, 1, 1);
			tariff2.UE_DateTo = new ZDateTime(2018, 12, 31);
			tariff2.UE_Unit1 = Core.Constants.Weight.Kilograms;
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff2.UE_Column1RateAdValorem = 0.255m;
			tariff2.UE_Column2RateAdValorem = 0.451m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_ExportDate = new ZDateTime(2017, 12, 11);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2018, 1, 1);
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_Tariff = "1020304050";
			invoiceLine.JI_CustomsQuantity = 10m;
			declaration.DoMerge();

			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(255m, entrySummaryEntry.TotalDutyAmount);
			AssertEquals(ZDateTime.Empty, entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(25.67m, entrySummaryEntry.MPFAmountForEntry);
			AssertEquals(ZDateTime.Empty, entrySummaryEntry.US_MPFCalcDate);
			AssertEquals(25.67m, invoiceLine.US_PayableMPF);
			AssertEquals(255m, invoiceLine.US_Duty);

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
			var actions = mainSender.Actions.OfType<EntryHeaderMessageSendingAction>().ToArray();
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals(255m, entrySummaryEntry.TotalDutyAmount);
			AssertEquals(ZDateTime.Empty, entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(25.67m, entrySummaryEntry.MPFAmountForEntry);
			AssertEquals(ZDateTime.Empty, entrySummaryEntry.US_MPFCalcDate);
			AssertEquals(25.67m, invoiceLine.US_PayableMPF);
			AssertEquals(255m, invoiceLine.US_Duty);

			mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.CargoRelease);
			actions = mainSender.Actions.OfType<EntryHeaderMessageSendingAction>().ToArray();
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals(255m, entrySummaryEntry.TotalDutyAmount);
			AssertEquals(ZDateTime.Empty, entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(25.67m, entrySummaryEntry.MPFAmountForEntry);
			AssertEquals(ZDateTime.Empty, entrySummaryEntry.US_MPFCalcDate);
			AssertEquals(25.67m, invoiceLine.US_PayableMPF);
			AssertEquals(255m, invoiceLine.US_Duty);

			mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.None);
			actions = mainSender.Actions.OfType<EntryHeaderMessageSendingAction>().ToArray();
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals(255m, entrySummaryEntry.TotalDutyAmount);
			AssertEquals(ZDateTime.Empty, entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(25.67m, entrySummaryEntry.MPFAmountForEntry);
			AssertEquals(ZDateTime.Empty, entrySummaryEntry.US_MPFCalcDate);
			AssertEquals(25.67m, invoiceLine.US_PayableMPF);
			AssertEquals(255m, invoiceLine.US_Duty);

			mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			actions = mainSender.Actions.OfType<EntryHeaderMessageSendingAction>().ToArray();
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals(255m, entrySummaryEntry.TotalDutyAmount);
			AssertEquals(new ZDateTime(2018, 1, 1), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(25.67m, entrySummaryEntry.MPFAmountForEntry);
			AssertEquals(new ZDateTime(2018, 1, 1), entrySummaryEntry.US_MPFCalcDate);
			AssertEquals(25.67m, invoiceLine.US_PayableMPF);
			AssertEquals(255m, invoiceLine.US_Duty);

			declaration.US_EstimatedEntryDate = new ZDateTime(2017, 12, 24);
			declaration.DoMerge();
			entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(255m, entrySummaryEntry.TotalDutyAmount);
			AssertEquals(new ZDateTime(2018, 1, 1), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(25.67m, entrySummaryEntry.MPFAmountForEntry);
			AssertEquals(new ZDateTime(2018, 1, 1), entrySummaryEntry.US_MPFCalcDate);
			AssertEquals(25.67m, invoiceLine.US_PayableMPF);
			AssertEquals(255m, invoiceLine.US_Duty);

			mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			actions = mainSender.Actions.OfType<EntryHeaderMessageSendingAction>().ToArray();
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals(162m, entrySummaryEntry.TotalDutyAmount);
			AssertEquals(new ZDateTime(2017, 12, 24), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(25m, entrySummaryEntry.MPFAmountForEntry);
			AssertEquals(new ZDateTime(2017, 12, 24), entrySummaryEntry.US_MPFCalcDate);
			AssertEquals(25m, invoiceLine.US_PayableMPF);
			AssertEquals(162m, invoiceLine.US_Duty);

			declaration.US_EstimatedEntryDate = new ZDateTime(2017, 12, 16);
			mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			actions = mainSender.Actions.OfType<EntryHeaderMessageSendingAction>().ToArray();
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals(162m, entrySummaryEntry.TotalDutyAmount);
			AssertEquals(new ZDateTime(2017, 12, 16), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(25m, entrySummaryEntry.MPFAmountForEntry);
			AssertEquals(new ZDateTime(2017, 12, 16), entrySummaryEntry.US_MPFCalcDate);
			AssertEquals(25m, invoiceLine.US_PayableMPF);
			AssertEquals(162m, invoiceLine.US_Duty);
		}

		[TestDate(2012, 3, 1)]
		public void TestDoNotRefreshExchangeRatesIfOneMessageIsSentAccepted()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			audCurr.ExchangeRates.DeleteAll();

			audCurr.SetUpExchangeRates(ZDateTime.Now, 1.05m);
			audCurr.SetUpExchangeRates(ZDateTime.Now.AddDays(1), 1.06m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(2);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "CR";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice.JobComInvoiceLines.AddNew();
			declaration.ResumeApportionment();
			AssertEquals("Exchange rate", 1.06m, invoice.JZ_InvoiceCurrExRate);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.CargoReleaseEntry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;

			audCurr.SetUpExchangeRates(ZDateTime.Now.AddDays(2), 1.07m);
			declaration.ResumeApportionment();
			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
			var actions = mainSender.Actions;
			SetupMainSender(mainSender);
			mainSender.SendMessage();

			AssertEquals("Exchange rate is NOT refreshed as at this point CRL is lodged", 1.06m, invoice.JZ_InvoiceCurrExRate);
		}

		public void TestSendMessageIfNotSaved()
		{
			shouldSave = false;
			Assert(!Sender.SendMessage());
		}

		public void TestOnPrepareNotSet()
		{
			shouldPrepare = false;
			Assert(!Sender.SendMessage());
		}

		public void TestOnPrepareReturnsFalse()
		{
			shouldReturnsTrueOnPrepare = false;
			Assert(!Sender.SendMessage());
		}

		public void TestOnAllowNotificationsNotSet()
		{
			shouldAllowNotifications = false;
			Assert(!Sender.SendMessage());
		}

		public void TestOnAllowNotificationsReturnsFalse()
		{
			shouldReturnTrueOnAllowNotifications = false;
			Assert(!Sender.SendMessage());
		}

		public void TestCopyPSCReasonsAndExplanation()
		{
			var reasonQuery = new ZQuery();
			reasonQuery.AddToFilter(Enterprise.ZArchitecture.Schema.CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			int existingReasonCount = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), reasonQuery);

			var explanationQuery = new ZQuery();
			explanationQuery.AddToFilter(Enterprise.ZArchitecture.Schema.CusAddInfoSchema.B7_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			int existingExplanationCount = Factory.GetDatabaseCount(typeof(PSCExplanationCusAddInfo), explanationQuery);

			((MainMessageSender)Sender).OnPrepare -= new MainMessageSender.PrepareEventHandler(sender_OnPrepare);
			((MainMessageSender)Sender).OnPrepare -= new MainMessageSender.PrepareEventHandler(sender_OnPrepareForTestCopyPSCReasonsAndExplanation);
			((MainMessageSender)Sender).OnPrepare += new MainMessageSender.PrepareEventHandler(sender_OnPrepareForTestCopyPSCReasonsAndExplanation);
			Sender.SendMessage();
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			((MainMessageSender)Sender).OnPrepare += new MainMessageSender.PrepareEventHandler(sender_OnPrepare);

			int reasonCount = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), reasonQuery);
			AssertEquals("There is 1 more reasons", existingReasonCount + 1, reasonCount);

			int explanationCount = Factory.GetDatabaseCount(typeof(PSCExplanationCusAddInfo), explanationQuery);
			AssertEquals("There is 1 more explanation", existingExplanationCount + 1, explanationCount);
		}

		public void TestSendAmendmentMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			declaration.DoMerge();

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals("Use the Send Replacement/Amendment Messages option to send a Replace Message.", ((SendsMessagesToCustomsShutterUpperer)(declaration.MessageInitiator)).Warning);
		}

		[TestDate(2024, 07, 01)]
		public void TestMPFAndDutyDatesForImmediateDelivery()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 07, 10);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_Tariff = "1020304050";
			invoiceLine.JI_CustomsQuantity = 10m;
			declaration.DoMerge();

			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			var actions = mainSender.Actions.OfType<EntryHeaderMessageSendingAction>().ToArray();
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals("US_DutyCalcDate from JE_EntryAuthorisationDate", new ZDateTime(2024, 7, 10), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals("US_MPFCalcDate from JE_EntryAuthorisationDate", new ZDateTime(2024, 7, 10), entrySummaryEntry.US_MPFCalcDate);

			declaration.US_ImmediateDelivery = true;
			declaration.DoMerge();
			mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals("US_DutyCalcDate from current date", new ZDateTime(2024, 7, 1), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals("US_MPFCalcDate from current date", new ZDateTime(2024, 7, 1), entrySummaryEntry.US_MPFCalcDate);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(2);
			entrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.DoMerge();
			AssertEquals("US_DutyCalcDate shouldn't change since entry has been accepted", new ZDateTime(2024, 7, 1), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals("US_MPFCalcDate shouldn't change since entry has been accepted", new ZDateTime(2024, 7, 1), entrySummaryEntry.US_MPFCalcDate);

			mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			SetupMainSender(mainSender);
			mainSender.SendMessage();
			AssertEquals("US_DutyCalcDate re-calculated from new current date", new ZDateTime(2024, 7, 3), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals("US_MPFCalcDate re-calculated from new current date", new ZDateTime(2024, 7, 3), entrySummaryEntry.US_MPFCalcDate);
		}

		public void TestSaveHandlesConcurrencyError()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			declaration.DoMerge();
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			mainSender.OnPrepare += _ => true;
			mainSender.OnAllowNotifications += _ => true;
			mainSender.OnSave += Factory.Save;
			mainSender.SendMessage();

			entry.US_MPFCalcDate = ZDateTime.Now.AddDays(2);
			Factory.Save();

			AssertEquals("AEO", entry.CH_Status);

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageNum = "EDIEDIDAT_1";
			message.EM_Status = "QUE";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = "B001101SV9AX                                               EDIEDIDAT_1          E0 SUMMRY 000001 REF ID: SV9 71017956 B00165528    171                          E0 LINITM 000001 REF ID: 001                                                    E1 IQ10   LINE SUBJECT TO QUOTA                   SV9  71017956                 E0 PSTLIN 000001 REF ID: SV9 71017956 B00165528                                 E1 IQ08   QUOTA PROCESS PENDING                   SV9  71017956                 E1AI995   SUMMARY HAS BEEN ADDED                  SV9  7101795600100B00165528   Y  1101SV9AX00006                                                               ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			AssertEquals("AEO", entry.CH_Status);
			message.Reload();
			AssertEquals("RCV", message.EM_Status);

			mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Replacement, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			mainSender.OnPrepare += _ => true;
			mainSender.OnAllowNotifications += _ => true;
			mainSender.OnSave += Factory.Save;
			AssertNoExceptionThrown("Concurrency errors are not expected.", () => mainSender.SendMessage());
		}

		public void TestMergeDoesNotGetMarkedDirtyAfterMessageSendingForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			declaration.DoMerge();
			Factory.Save();

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
			var actions = mainSender.Actions;

			actions[0].US_SendMessage = true;
			actions[0].US_AcknowledgeAndSign = true;
			actions[0].US_Paid = YesNoDefaultList.Codes.Yes;
			actions[0].US_SE_MultipleDispositionsIndic = true;

			actions[1].US_SendMessage = true;
			actions[1].US_Paid = YesNoDefaultList.Codes.Yes;

			mainSender.OnPrepare += new MainMessageSender.PrepareEventHandler(delegate
			{ return true; });
			mainSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate
			{ return true; });
			mainSender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ Factory.Save(); });
			mainSender.SendMessage();

			Assert(!declaration.MergeManager.RequiresMerge);
		}

		JobDeclaration SetupDeclarationForDisbursementAmount()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_ShortDescription = "Cofee Fee Test";
			tariff.UE_Unit1 = "KG";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;
			Factory.Save();

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00001111";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_ShortDescription = "Wines Test";
			tariff2.UE_Unit1 = "KG";

			var dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate2.UD_TaxFeeFlag = "1";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate2.UD_TaxFeeSpecificRate = 0.0851m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "TEST NAME", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			newFactory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			declaration.US_EntryFilerCode = "XJ5";
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-Cot";
			invoiceHeader.JZ_InvoiceAmount = 9901.25m;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			invoiceLine1.JI_CustomsQuantity = 840m;
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_Weight = 3285.59m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_CustomsSecondQuantity = 3285.59m;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine2.JI_CustomsQuantity = 840m;
			invoiceLine2.JI_LinePrice = 2000m;
			invoiceLine2.JI_Weight = 3285.59m;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_CustomsSecondQuantity = 3285.59m;

			declaration.US_SchDEntry = "1234";
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}

		public void TestDisbursmentAmountNotChanged()
		{
			var declaration = SetupDeclarationForDisbursementAmount();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var iCusEntryHeader = (MessageBuilders.ICusEntryHeader)entry;
			AssertEquals(420m, iCusEntryHeader.GrandTotalOtherRevenueAmount);
			AssertEquals("TotalAmountPayable should contain coffee fee", 491.480m, entry.TotalAmountPayable);
			AssertEquals("TotalAmountPayable should contain coffee fee", 491.480m, entry.TotalPayableIncludingDeferredTax);

			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageText =
"B  3901JJ8AE                                                                    " +
"10AJJ8  85451101 3901            06   X           3121720          F            " +
"1113-14202600013-142026000               120220      022AB01IL                  " +
"20    3901      HA68                                                            " +
"318B 913                                                                        " +
"40  002 SESE092120        0000000005     0000000063    N                        " +
"41P1016200000000063                                                             " +
"47MSEUDDAB683HAG                                                                " +
"47S13-142026000                                                                 " +
"507220110000 0000000000 0000000369 000000006300KG                               " +
"5201H6ANBK070                                                                   " +
"5403STL154698                                                                   " +
"6249900000128                                                                   " +
"8800000000000 00000000000 00000000000 00000000000                               " +
"8949900000052833                                                                " +
"9000000000000 00000000000 00000007148 00000000000 00000000000 00000042000       " +
"Y  3901JJ8AE                                                                    ";
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_236622";
			entry.Messages.Add(outgoingMessage);

			var incomingMsg = Factory.New<MQEDIMessage>();
			incomingMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incomingMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			incomingMsg.EM_MessageText =
"B008888XJ5AX                                               HYEDUSCMT_236622     " +
"E0 SUMMRY 000001 REF ID: SV9 71017956 B00165528    171                          " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E1 IQ10   LINE SUBJECT TO QUOTA                   SV9  71017956                 " +
"E0 PSTLIN 000001 REF ID: SV9 71017956 B00165528                                 " +
"E1 IQ08   QUOTA PROCESS PENDING                   SV9  71017956                 " +
"E1AI995   SUMMARY HAS BEEN ADDED                  SV9  7101795600100B00165528   " +
"Y  8888XJ5AX00006                                                               ";

			entry.Messages.Add(outgoingMessage);
			Factory.Save();

			AssertEquals("Disbursment", false, declaration.HasDiscrepancyInDisbursementAmount);
		}

		public void TestDisbursmentAmountChanged()
		{
			var declaration = SetupDeclarationForDisbursementAmount();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var iCusEntryHeader = (MessageBuilders.ICusEntryHeader)entry;
			AssertEquals(420m, iCusEntryHeader.GrandTotalOtherRevenueAmount);
			AssertEquals("TotalAmountPayable should contain coffee fee", 491.480m, entry.TotalAmountPayable);
			AssertEquals("TotalAmountPayable should contain coffee fee", 491.480m, entry.TotalPayableIncludingDeferredTax);

			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageText =
"B  3901JJ8AE                                                                    " +
"10AJJ8  85451101 3901            06   X           3121720          F            " +
"1113-14202600013-142026000               120220      022AB01IL                  " +
"20    3901      HA68                                                            " +
"318B 913                                                                        " +
"40  002 SESE092120        0000000005     0000000063    N                        " +
"41P1016200000000063                                                             " +
"47MSEUDDAB683HAG                                                                " +
"47S13-142026000                                                                 " +
"507220110000 0000000000 0000000369 000000006300KG                               " +
"5201H6ANBK070                                                                   " +
"5403STL154698                                                                   " +
"6249900000128                                                                   " +
"8800000000000 00000000000 00000000000 00000000000                               " +
"8949900000052833                                                                " +
"9000000000000 00000000000 00000007348 00000000000 00000000000 00000033000       " +
"Y  3901JJ8AE                                                                    ";
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_236622";
			entry.Messages.Add(outgoingMessage);

			var incomingMsg = Factory.New<MQEDIMessage>();
			incomingMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incomingMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMsg.EM_MessageNum = "HYEDUSCMT_236622";

			incomingMsg.EM_MessageText =
"B008888XJ5AX                                               HYEDUSCMT_236622     " +
"E0 SUMMRY 000001 REF ID: SV9 71017956 B00165528    171                          " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E1 IQ10   LINE SUBJECT TO QUOTA                   SV9  71017956                 " +
"E0 PSTLIN 000001 REF ID: SV9 71017956 B00165528                                 " +
"E1 IQ08   QUOTA PROCESS PENDING                   SV9  71017956                 " +
"E1AI995   SUMMARY HAS BEEN ADDED                  SV9  7101795600100B00165528   " +
"Y  8888XJ5AX00006                                                               ";

			entry.Messages.Add(incomingMsg);
			Factory.Save();

			AssertEquals("Disbursment", true, declaration.HasDiscrepancyInDisbursementAmount);
		}

		public void TestMergeSavedBeforeCreditCheck()
		{
			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;

			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Test Company Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";
			header.OH_Code = "ZABC";

			header.OH_IsCreditor = true;
			header.CompanyData.SetAPTaxApplicable(true);
			header.MiscServ.OM_APWHTApplicable = true;

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var customsCharge = Factory.NewWithValidTestData<AccChargeCode>();
			customsCharge.AC_MarginPercentage = 100m;
			customsCharge.AC_ChargeType = "DSB";
			ObjectFactory.Get<IAccounting>().IncludeUnpostedRevenueInGlobalCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL");
			ObjectFactory.Get<IAccounting>().IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");

			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));

			using (AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			using (CustomsDataRegistry.Instance.EnableAccountingIntegration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option))
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customsCharge.PK.ToGuid()))
			using (RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, header.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				declaration.US_EnableCRL = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;

				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.OH_IsDebtor = true;
				declaration.JE_OH_Importer = importer.PK;
				declaration.Importer.CompanyData.OB_ARCreditLimit = 10m;

				var invoice = declaration.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				line.JI_Tariff = "0123456789";
				line.US_98GoodsValue = 10000m;

				var initiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.MessageInitiator = initiator;

				Factory.Save();

				var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);

				mainSender.OnPrepare += _ => true;
				mainSender.OnAllowNotifications += _ => true;
				mainSender.OnSave += Factory.Save;
				mainSender.SendMessage();

				Assert(!declaration.MergeManager.RequiresMerge);
				AssertEquals("Preparation didn't go through because the credit check failed", Customs.Business.MessageSender.MessageSendingStatus.PreparingJob, mainSender.SendingStatus);

				declaration.Importer.CompanyData.OB_ARCreditLimit = 1000m;
				Factory.Save();
				mainSender.SendMessage();

				Assert(!declaration.MergeManager.RequiresMerge);
				AssertEquals("Preparation didn't go through because the credit check failed", Customs.Business.MessageSender.MessageSendingStatus.GeneratingMessages, mainSender.SendingStatus);
			}
		}

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			sender = null; //on every test sender will be constructed
			Declaration.HasChanges = true; //on every test sender will check "Declaration.HasChanges" property
			shouldSave = true;
			shouldPrepare = true;
			shouldReturnsTrueOnPrepare = true;
			shouldAllowNotifications = true;
			shouldReturnTrueOnAllowNotifications = true;
		}

		protected override void TearDown()
		{
			Declaration.UnlockImportEntryNumberAllocationMutex();
			Declaration.UnlockFTZAdmissionNumberAllocationMutex();
			base.TearDown();
		}

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new MainMessageSender(Declaration, ImportMessageSendingMessageType.Original);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
					if (shouldPrepare)
					{
						sender.OnPrepare += new MainMessageSender.PrepareEventHandler(sender_OnPrepare);
					}
					if (shouldAllowNotifications)
					{
						sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler((MessageSendingNotificationCollection notifications) => shouldReturnTrueOnAllowNotifications);
					}
				}
				return sender;
			}
		}
		MainMessageSender sender;

		bool sender_OnPrepareForTestCopyPSCReasonsAndExplanation(ImportMessageSendingActionCollection actions)
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_PSC = true;
			Declaration.US_EntryFilerCode = "XJ5";
			Declaration.US_EnableENS = true;

			var invoice = Declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, actions);
			action.US_SendMessage = true;
			var pscCode = action.PSCReasonCodes.AddNew();
			pscCode.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			pscCode.Reason1 = PSCHeaderReasonList.Codes.H01;
			action.US_PSCExplanation = "Some explanation";

			actions.Add(action);

			return sender_OnPrepare(actions);
		}

		bool sender_OnPrepare(ImportMessageSendingActionCollection actions)
		{
			actions.SelectAll();
			actions.IsCancelled = !shouldReturnsTrueOnPrepare;
			return !actions.IsCancelled;
		}

		bool shouldSave = true;
		bool shouldPrepare = true;
		bool shouldReturnsTrueOnPrepare = true;
		bool shouldAllowNotifications = true;
		bool shouldReturnTrueOnAllowNotifications = true;

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;

			return settings;
		}

		protected override JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
					declaration.US_EnableCRL = false;
					declaration.MessageInitiator = new SendsMessagesToCustoms();
					declaration.ValidationModes = ValidationModes.EntrySummary;

					JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
