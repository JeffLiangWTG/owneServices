using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryHeaderMessageSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTIBExtensionWhenTariff98130075()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var line = entry.MergedLines.AddNew();
			line.CL_AdValoremTariff = "98130075";
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.ExtendTIB);
			var action = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.TemporaryImportationBond);
			var expectedMessage = "6 month TIB using tariff 9813.00.75 may not be extended.";
			AssertHasMessageErrorContaining(action.US_SendMessageInfo, expectedMessage);
			AssertContains(expectedMessage, action.Notifications.FirstOrDefault().Message);
		}

		public void TestThirdTIBExtensionRequest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var incomingMessage1 = Factory.New<MQEDIMessage>();
			incomingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage1.EM_MessageNum = "~15000";
			incomingMessage1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse;
			incomingMessage1.EM_MessageText = "B018888XJ5TX                                               ~15000               " +
				"E0 SUMMRY 000001 REF ID: XJ5 23456781                                           " +
				"E1RF995 EXT GRANTED SUBJECT TO REVIEW             XJ5  23456781     B00000001   " +
				"Y  8888XJ5X100007";
			incomingMessage1.EM_Status = MQEDIMessage.Status.Received;
			incomingMessage1.EM_LinkedObject = entry;
			var incomingMessage2 = Factory.New<MQEDIMessage>();
			incomingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_MessageNum = "~15000";
			incomingMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse;
			incomingMessage2.EM_MessageText = "B018888XJ5TX                                               ~15000               " +
				"E0 SUMMRY 000001 REF ID: XJ5 23456781                                           " +
				"E1RF995 EXT GRANTED SUBJECT TO REVIEW             XJ5  23456781     B00000001   " +
				"Y  8888XJ5X100007";
			incomingMessage2.EM_Status = MQEDIMessage.Status.Received;
			incomingMessage2.EM_LinkedObject = entry;
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.ExtendTIB);
			var action = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.TemporaryImportationBond);
			action.Validation.ValidateUS_SendMessage();
			var expectedMessage = "Two TIB extensions have already been granted on this entry. No more extensions are allowed.";
			AssertHasMessageErrorContaining(action.US_SendMessageInfo, expectedMessage);
		}

		public void TestFirstTIBExtensionRequest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.ExtendTIB);
			var action = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.TemporaryImportationBond);
			action.Validation.ValidateUS_SendMessage();
			AssertEquals(0, action.Notifications.Count());
		}

		public void TestCheckReplacementForCargoReleaseType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.EXM;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var entrySummaryAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CertifyCargoRelease = true;
			entrySummaryAction.Validation.ValidateUS_CertifyCargoRelease();
			AssertHasWarning(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.ReplacementWarningMessageForExm);
			entrySummaryAction.US_CertifyCargoRelease = false;
			entrySummaryAction.Validation.ValidateUS_CertifyCargoRelease();
			AssertNoWarning(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.ReplacementWarningMessageForExm);
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.ADM;
			var bill = declaration.Bills.AddNew();
			var disp = bill.DispositionCodes.AddNew();
			disp.US_Code = CargoReleaseProcessingResultList.Codes.BillArrived;
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.ADM;
			entrySummaryAction.US_CertifyCargoRelease = true;
			entrySummaryAction.Validation.ValidateUS_CertifyCargoRelease();
			AssertHasWarning(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.ReplacementWarningMessageForNOTREL);
			entrySummaryAction.US_CertifyCargoRelease = false;
			entrySummaryAction.Validation.ValidateUS_CertifyCargoRelease();
			AssertNoWarning(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.ReplacementWarningMessageForNOTREL);
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			entrySummaryAction.US_CertifyCargoRelease = true;
			entrySummaryAction.Validation.ValidateUS_CertifyCargoRelease();
			AssertHasMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.ReplacementErrorMessageForREL);
			entrySummaryAction.US_CertifyCargoRelease = false;
			entrySummaryAction.Validation.ValidateUS_CertifyCargoRelease();
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.ReplacementErrorMessageForREL);
		}

		public void TestCheckSEReplacementOrUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			Declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var simplifiedEntryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			simplifiedEntryAction.US_SendMessage = true;
			simplifiedEntryAction.US_SE_ActionType = ACECargoReleaseActionType.Codes.Replace;
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.EXM;
			simplifiedEntryAction.Validation.ValidateUS_SE_ActionType();
			AssertHasWarning(simplifiedEntryAction.US_SE_ActionTypeInfo, EntryHeaderMessageSendingActionValidation.ReplacementWarningMessageForExm);
			simplifiedEntryAction.US_SE_ActionType = ACECargoReleaseActionType.Codes.Update;
			simplifiedEntryAction.Validation.ValidateUS_SE_ActionType();
			AssertNoWarning(simplifiedEntryAction.US_SE_ActionTypeInfo, EntryHeaderMessageSendingActionValidation.ReplacementWarningMessageForExm);
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.ADM;
			var bill = declaration.Bills.AddNew();
			var disp = bill.DispositionCodes.AddNew();
			disp.US_Code = CargoReleaseProcessingResultList.Codes.BillArrived;
			simplifiedEntryAction.US_SE_ActionType = ACECargoReleaseActionType.Codes.Replace;
			simplifiedEntryAction.Validation.ValidateUS_SE_ActionType();
			AssertHasWarning(simplifiedEntryAction.US_SE_ActionTypeInfo, EntryHeaderMessageSendingActionValidation.ReplacementWarningMessageForNOTREL);
			simplifiedEntryAction.US_SE_ActionType = ACECargoReleaseActionType.Codes.Update;
			simplifiedEntryAction.Validation.ValidateUS_SE_ActionType();
			AssertNoWarning(simplifiedEntryAction.US_SE_ActionTypeInfo, EntryHeaderMessageSendingActionValidation.ReplacementWarningMessageForNOTREL);
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			simplifiedEntryAction.US_SE_ActionType = ACECargoReleaseActionType.Codes.Replace;
			simplifiedEntryAction.Validation.ValidateUS_SE_ActionType();
			AssertHasMessageError(simplifiedEntryAction.US_SE_ActionTypeInfo, EntryHeaderMessageSendingActionValidation.ReplacementErrorMessageForREL);
			simplifiedEntryAction.US_SE_ActionType = ACECargoReleaseActionType.Codes.Update;
			simplifiedEntryAction.Validation.ValidateUS_SE_ActionType();
			AssertNoMessageError(simplifiedEntryAction.US_SE_ActionTypeInfo, EntryHeaderMessageSendingActionValidation.ReplacementErrorMessageForREL);
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			simplifiedEntryAction.US_SE_ActionType = ACECargoReleaseActionType.Codes.Update;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(-16);
			simplifiedEntryAction.Validation.ValidateUS_SE_ActionType();
			AssertHasMessageError(simplifiedEntryAction.US_SE_ActionTypeInfo, EntryHeaderMessageSendingActionValidation.UpdateErrorMessageForREL);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(-10);
			simplifiedEntryAction.Validation.ValidateUS_SE_ActionType();
			AssertNoMessageError(simplifiedEntryAction.US_SE_ActionTypeInfo, EntryHeaderMessageSendingActionValidation.UpdateErrorMessageForREL);
		}

		public void TestCheckUS_CertifyCargoReleaseForOriginalACECRAccepted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd;
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.LogManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearACECargoReleaseAdd, new ImportMessageStatusList());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SendMessage = true;
			seAction.US_CertifyCargoRelease = true;
			seAction.Validation.ValidateUS_CertifyCargoRelease();
			AssertHasMessageError(seAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.OriginalACECRAccepted);
			seAction.US_CertifyCargoRelease = false;
			seAction.Validation.ValidateUS_CertifyCargoRelease();
			AssertNoMessageError(seAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.OriginalACECRAccepted);
		}

		public void TestCertifyCargoReleaseForImmediateDelivery()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_ImmediateDelivery = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var ensAction = (EntryHeaderMessageSendingAction)actions[0];
			ensAction.US_SendMessage = true;
			ensAction.US_CertifyCargoRelease = true;
			AssertHasMessageError(ensAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CannotCertifyCRForImmediateDelivery);
			ensAction.US_CertifyCargoRelease = false;
			AssertNoMessageError(ensAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CannotCertifyCRForImmediateDelivery);
			declaration.US_ImmediateDelivery = false;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensAction = (EntryHeaderMessageSendingAction)actions[0];
			ensAction.US_SendMessage = true;
			ensAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(ensAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CannotCertifyCRForImmediateDelivery);
		}

		public void TestCheckUS_CertifyCargoReleaseOnMessageForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ValidationModes = ValidationModes.EntrySummary;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var laceyActLine = invoiceLine.LaceyActLines.AddNew();
			var constituenteElement = laceyActLine.PG04ConstituentElements.AddNew();
			constituenteElement.US_PGAUnitOfMeasure = "M3";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd;
			var entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CertifyCargoRelease = true;
			declaration.US_CertifyCargoRelease = true;
			constituenteElement.US_PGAQuantityOfConstituentElement = 100m;
			constituenteElement.US_PGAUnitOfMeasure = ZString.Empty;
			entrySummaryAction.Validation.ValidateAll();
			laceyActLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(constituenteElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQRequired);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CertifyCargoRelease = false;
			declaration.US_CertifyCargoRelease = false;
			entrySummaryAction.Validation.ValidateAll();
			laceyActLine.Validation.ValidateAll();
			constituenteElement.AddInfoValidation.ValidateUS_PGAUnitOfMeasure();
#pragma warning disable CA2021 // Do not call Enumerable.Cast<T> or Enumerable.OfType<T> with incompatible types
			AssertEquals(false, entrySummaryAction.GetMessageErrors().OfType<string>().Contains("UQ: Unit Of Measure is mandatory."));
#pragma warning restore CA2021 // Do not call Enumerable.Cast<T> or Enumerable.OfType<T> with incompatible types
		}

		public void TestCheckUS_SendMessage_PortOfEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ValidationModes = ValidationModes.EntrySummary;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			EntryHeaderMessageSendingAction ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			ensAction.US_CertifyCargoRelease = true;
			ensAction.US_SendMessage = true;
			AssertHasMessageError(ensAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.PortOfEntryCannotBeEmpty);
		}

		public void TestCheckSendMessageWithNoOriginalAcceptForENSMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ValidationModes = ValidationModes.EntrySummary;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SendMessage = true;
			AssertHasWarning(seAction.US_SendMessageInfo, "There is no ACE Entry Summary acceptance message on file. A delete will be rejected if it was never accepted at ABI.");
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SendMessage = true;
			AssertHasMessageError(seAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.NoEntrySummaryAccepted);
			declaration.US_BRDRefNo = "TESTNUM";
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SendMessage = true;
			AssertNoMessageError(seAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.NoEntrySummaryAccepted);
			declaration.US_BRDRefNo = ZString.Empty;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SendMessage = true;
			AssertHasMessageError(seAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.NoEntrySummaryAccepted);
		}

		public void TestCheckUS_SendMessage_ACS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			AssertHasError(ensAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.NotSupportedByCBP);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			AssertNoError(ensAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.NotSupportedByCBP);
		}

		public void TestCheckUS_SendMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ValidationModes = ValidationModes.CargoRelease;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SendMessage = true;
			AssertHasWarning(seAction.US_SendMessageInfo, "There is no ACE Cargo Release acceptance message on file. A delete will be rejected if it was never accepted at ABI.");
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			seAction = (EntryHeaderMessageSendingAction)actions[0];
			AssertHasWarning(seAction.US_SendMessageInfo, "There is no ACE Cargo Release acceptance message on file. An update or replace will be rejected if it was never accepted at ABI.");
			for (int i = 1; i < 1000; i++)
			{
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "1000000" + i.ToString().PadLeft(3, '0');
				tariff.UE_DateFrom = ZDate.Today.AddMonths(-1);
				tariff.UE_DateTo = ZDate.Today.AddMonths(1);
				invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			seAction = (EntryHeaderMessageSendingAction)actions[0];
			AssertEquals(1000, seAction.entry.EntryLines.Count());
			AssertHasMessageError(seAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.EntryLinesNumberExceeds);
		}

		public void TestCheckUS_SE_DISIDRefNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SE_DISIndicator = false;
			seAction.US_SE_DISIDRefNo = string.Empty;
			seAction.Validation.ValidateUS_SE_DISIDRefNo();
			AssertNoMessageError(seAction.US_SE_DISIDRefNoInfo, EntryHeaderMessageSendingActionValidation.DISIDRefNoRequired);
			seAction.US_SE_DISIndicator = true;
			seAction.US_SE_DISIDRefNo = string.Empty;
			seAction.Validation.ValidateUS_SE_DISIDRefNo();
			AssertHasMessageError(seAction.US_SE_DISIDRefNoInfo, EntryHeaderMessageSendingActionValidation.DISIDRefNoRequired);
			seAction.US_SE_DISIndicator = true;
			seAction.US_SE_DISIDRefNo = "KNZTest.pdf";
			seAction.Validation.ValidateUS_SE_DISIDRefNo();
			AssertNoMessageError(seAction.US_SE_DISIDRefNoInfo, EntryHeaderMessageSendingActionValidation.DISIDRefNoRequired);
		}

		public void TestValidateUS_JobReadyForPosting()
		{
			Declaration.US_EntryFilerCode = "XJ5";
			EntryHeaderMessageSendingAction entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			options.PreApprovalBillingJob = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, options);
			EntryHeaderMessageSendingActionValidation validation = new EntryHeaderMessageSendingActionValidation(entrySummaryAction);
			TestQuery query = new TestQuery();
			query.Result.NumberOfARInvoicesToBeIssued = 2;
			query.Result.NumberOfUnpostedAPCharges = 3;
			query.Result.NumberOfUnpostedAPChargesReadyForPosting = 1;
			query.Result.NumberOfUnpostedARCharges = 5;
			query.Result.NumberOfUnpostedARChargesReadyForPosting = 4;
			validation.AP_ARInvoiceQueryForTesting = query;
			entrySummaryAction.US_JobReadyForPosting = true;
			validation.ValidateUS_JobReadyForPosting();
			AssertHasWarning(entrySummaryAction.US_JobReadyForPostingInfo, string.Format(EntryHeaderMessageSendingActionValidation.MoreThanOneARInvoicesWillBeIssued, 2));
			AssertHasWarning(entrySummaryAction.US_JobReadyForPostingInfo, string.Format(EntryHeaderMessageSendingActionValidation.UnmatchedAPCharges, 3, 1));
			AssertHasWarning(entrySummaryAction.US_JobReadyForPostingInfo, string.Format(EntryHeaderMessageSendingActionValidation.UnmatchedARCharges, 5, 4));
			query.Result.NumberOfARInvoicesToBeIssued = 1;
			query.Result.NumberOfUnpostedAPCharges = 0;
			query.Result.NumberOfUnpostedAPChargesReadyForPosting = 0;
			query.Result.NumberOfUnpostedARCharges = 0;
			query.Result.NumberOfUnpostedARChargesReadyForPosting = 0;
			validation.ValidateUS_JobReadyForPosting();
			AssertNoWarning(entrySummaryAction.US_JobReadyForPostingInfo, string.Format(EntryHeaderMessageSendingActionValidation.MoreThanOneARInvoicesWillBeIssued, 2));
			AssertNoWarning(entrySummaryAction.US_JobReadyForPostingInfo, string.Format(EntryHeaderMessageSendingActionValidation.UnmatchedAPCharges, 3, 1));
			AssertNoWarning(entrySummaryAction.US_JobReadyForPostingInfo, string.Format(EntryHeaderMessageSendingActionValidation.UnmatchedARCharges, 5, 4));
			AssertHasWarning(entrySummaryAction.US_JobReadyForPostingInfo, EntryHeaderMessageSendingActionValidation.NoChargesToBePosted);
			query.Result.NumberOfUnpostedAPCharges = 1;
			query.Result.NumberOfUnpostedAPChargesReadyForPosting = 1;
			query.Result.NumberOfUnpostedARCharges = 1;
			query.Result.NumberOfUnpostedARChargesReadyForPosting = 1;
			validation.ValidateUS_JobReadyForPosting();
			AssertNoWarning(entrySummaryAction.US_JobReadyForPostingInfo, EntryHeaderMessageSendingActionValidation.NoChargesToBePosted);
		}

		public void TestCheckUS_Paid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableCRL = true;
			declaration.ImportEntryNumber = "1";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entrySummary = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entrySummary.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			AssertEquals(2, actions.Count);
			var action = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			action.US_Paid = ZString.Empty;
			AssertHasMessageError(action.US_PaidInfo, EntryHeaderMessageSendingActionValidation.PaidIndicatorToBeSpecified);
			declaration.US_PSC = true;
			action.Validation.ValidateUS_Paid();
			AssertNoMessageError(action.US_PaidInfo, EntryHeaderMessageSendingActionValidation.PaidIndicatorToBeSpecified);
			declaration.US_PSC = false;
			action.US_Paid = YesNoDefaultList.Codes.No;
			AssertNoMessageError(action.US_PaidInfo, EntryHeaderMessageSendingActionValidation.PaidIndicatorToBeSpecified);
			declaration.US_PaymentDueDate = ZDateTime.BrettsBirthday;
			action.US_Paid = YesNoDefaultList.Codes.No;
			AssertHasWarning(action.US_PaidInfo, EntryHeaderMessageSendingActionValidation.PaymentDueDateHasPassedButIsIndicatedAsNotPaid);
			action.US_Paid = YesNoDefaultList.Codes.Yes;
			AssertNoWarning(action.US_PaidInfo, EntryHeaderMessageSendingActionValidation.PaymentDueDateHasPassedButIsIndicatedAsNotPaid);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			action = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			action.US_Paid = ZString.Empty;
			AssertNoMessageErrors("Should not be any notifications, because Deletion message", action.US_PaidInfo);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.ExtendTIB);
			actions[0].US_Paid = ZString.Empty;
			AssertNoMessageErrors("Should not be any notifications, because ExtendTIB message", actions[0].US_PaidInfo);
		}

		public void TestValidateUS_JobReadyWhenRegistryIsOff()
		{
			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			options.PreApprovalBillingJob = false;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, options);
			Declaration.US_EntryFilerCode = "XJ5";
			Declaration.US_JobReadyForPost = true; //Users have set before turning off registry item
			EntryHeaderMessageSendingAction entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			Assert("PreCondition", entrySummaryAction.US_JobReadyForPosting);
			EntryHeaderMessageSendingActionValidation validation = new EntryHeaderMessageSendingActionValidation(entrySummaryAction);
			TestQuery query = new TestQuery();
			validation.AP_ARInvoiceQueryForTesting = query;
			entrySummaryAction.US_JobReadyForPosting = true;
			validation.ValidateUS_JobReadyForPosting();
			Assert("Should not validate when registry is off. Users cannot see the field", !entrySummaryAction.US_JobReadyForPostingInfo.HasWarning(EntryHeaderMessageSendingActionValidation.NoChargesToBePosted));
		}

		public void TestNoSendingOfEntrySummaryIfOnStatement()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EntryFilerCode = "XJ5";
			EntryHeaderMessageSendingAction entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			var messageErrorText = ValidationConstants.EntrySummary.AlreadyOnStatement("entry summary");
			AssertNoMessageError(entrySummaryAction.US_SendMessageInfo, messageErrorText);
			Declaration.AllocateEntryNumber("TEST");
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			CusStatementLine statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "TEST";
			Factory.Save();
			entrySummaryAction.Validation.ValidateUS_SendMessage();
			AssertHasMessageError(entrySummaryAction.US_SendMessageInfo, messageErrorText);
			ImportMessageSendingActionCollection collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.EntrySummaryQuery);
			entrySummaryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNotNull(Declaration.RelatedStatement);
			entrySummaryAction.US_SendMessage = true;
			AssertNoMessageError(entrySummaryAction.US_SendMessageInfo, messageErrorText);
			ENSEntryMock.Setup(m => m.HasBeenLodgedAtCustoms).Returns(true);
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Replacement);
			entrySummaryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNotNull(Declaration.RelatedStatement);
			entrySummaryAction.US_SendMessage = true;
			AssertHasMessageError(entrySummaryAction.US_SendMessageInfo, messageErrorText);
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.ExtendTIB);
			entrySummaryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.TemporaryImportationBond);
			AssertNotNull(Declaration.RelatedStatement);
			entrySummaryAction.US_SendMessage = true;
			AssertNoMessageError(entrySummaryAction.US_SendMessageInfo, messageErrorText);
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Deletion);
			entrySummaryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNotNull(Declaration.RelatedStatement);
			entrySummaryAction.US_SendMessage = true;
			AssertHasMessageError(entrySummaryAction.US_SendMessageInfo, messageErrorText);
		}

		public void TestPSCSubmissionDoesNotGiveEntryOnStatementWarning()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EntryFilerCode = "XJ5";
			var entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			var messageErrorText = ValidationConstants.EntrySummary.AlreadyOnStatement("entry summary");
			entrySummaryAction.US_SendMessage = true;
			AssertNoMessageError(entrySummaryAction.US_SendMessageInfo, messageErrorText);
			Declaration.AllocateEntryNumber("TESTACE");
			var statementHeaderACE = Factory.New<CusStatementHeader>();
			var statementLineACE = statementHeaderACE.StatementLines.AddNew();
			statementLineACE.B3_Status = StatementLineStatusList.Codes.Active;
			statementLineACE.B3_EntryFilerCode = "XJ5";
			statementLineACE.B3_EntryNum = "TESTACE";
			Factory.Save();
			entrySummaryAction.Validation.ValidateUS_SendMessage();
			AssertHasMessageError(entrySummaryAction.US_SendMessageInfo, messageErrorText);
			Declaration.US_PSC = true;
			Factory.Save();
			entrySummaryAction.Validation.ValidateUS_SendMessage();
			AssertNoMessageError("WI00033017: need to remove this error when filing a PSC", entrySummaryAction.US_SendMessageInfo, messageErrorText);
		}

		public void TestValidateUS_AcknowledgeAndSign()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			ensAction.US_AcknowledgeAndSign = false;
			AssertHasMessageError(ensAction.US_AcknowledgeAndSignInfo, EntryHeaderMessageSendingActionValidation.CannotSendUnlessAcknowledgedAndSigned);
			ensAction.US_AcknowledgeAndSign = true;
			AssertNoMessageError(ensAction.US_AcknowledgeAndSignInfo, EntryHeaderMessageSendingActionValidation.CannotSendUnlessAcknowledgedAndSigned);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			Assert(!declaration.IsACE);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			ensAction.US_AcknowledgeAndSign = false;
			AssertNoMessageError(ensAction.US_AcknowledgeAndSignInfo, EntryHeaderMessageSendingActionValidation.CannotSendUnlessAcknowledgedAndSigned);
		}

		public void TestValidateUS_AcknowledgeAndSignForWithdrawal()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			ensAction.US_AcknowledgeAndSign = false;
			bool hasTheMessageError = false;
			foreach (var notification in ensAction.US_AcknowledgeAndSignInfo.GetMessageErrors())
			{
				if (notification.Message == EntryHeaderMessageSendingActionValidation.CannotSendUnlessAcknowledgedAndSigned)
				{
					hasTheMessageError = true;
					break;
				}
			}

			Assert("Should not require to sign for deletion", !hasTheMessageError);
		}

		public void TestValidateUS_SendMessage()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ensEntry = Factory.NewMoq<CusEntryHeader>();
			ensEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.Object.CH_JE = declaration.PK;
			ensEntry.Object.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.CustomsEntryHeaders.Add(ensEntry.Object);
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(ensEntry.Object.MergedLines.AddNew());
			AssertEquals("HasBeenLodgedAtCustoms", true, ensEntry.Object.HasBeenLodgedAtCustoms);
			var crlEntry = Factory.NewMoq<CusEntryHeader>();
			crlEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			crlEntry.Object.CH_CH_PrimeEntry = ensEntry.Object.PK;
			crlEntry.Object.CH_JE = declaration.PK;
			crlEntry.Object.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			AssertEquals("HasBeenLodgedAtCustoms", true, crlEntry.Object.HasBeenLodgedAtCustoms);
			declaration.CustomsEntryHeaders.Add(crlEntry.Object);
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(crlEntry.Object.MergedLines.AddNew());
		}

		public void TestValidateUS_SendMessageForCancelledEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;
			Assert("PreCondition", entry.HasBeenCancelled);
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = (EntryHeaderMessageSendingAction)collection[0];
			AssertHasMessageError(action.US_SendMessageInfo, ValidationConstants.EntrySummary.HasBeenCancelled);
		}

		public void TestValidateUS_SendMessageForRLFEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_SchDEntry = "0901";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ensEntry = Factory.NewMoq<CusEntryHeader>();
			ensEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.Object.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(ensEntry.Object);
			string messageSendingType = "Entry Summary";
			string branchIsNotConfiguredUnableToSendMsg = string.Format(ValidationConstants.Declaration.BranchIsNotConfiguredUnableToSendMsg, messageSendingType);
			string notRLFJob = string.Format(ValidationConstants.Declaration.NotRLFJob, messageSendingType);
			string rLFJob = string.Format(ValidationConstants.Declaration.RLFJob, messageSendingType);
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNoError(ensAction.US_SendMessageInfo, branchIsNotConfiguredUnableToSendMsg);
			AssertNoError(ensAction.US_SendMessageInfo, notRLFJob);
			AssertNoError(ensAction.US_SendMessageInfo, rLFJob);
			var registryItemsCollection = new BranchDistrictPortCollection(new ZArchitecture.Environment.FallbackLevel(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var item1 = registryItemsCollection.AddNew();
			item1.PortCode = "39";
			item1.BranchPK = GlbBranch.CurrentBranch.PK;
			DataRegistry.Business.USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, registryItemsCollection);
			GlbStaff.CurrentUser.GS_LoginName = "TestUser";
			GlbStaff.CurrentUser.GS_GB_HomeBranch = ZGuid.Empty;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertHasError(ensAction.US_SendMessageInfo, branchIsNotConfiguredUnableToSendMsg);
			AssertNoError(ensAction.US_SendMessageInfo, notRLFJob);
			AssertNoError(ensAction.US_SendMessageInfo, rLFJob);
			GlbStaff.CurrentUser.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNoError(ensAction.US_SendMessageInfo, branchIsNotConfiguredUnableToSendMsg);
			AssertHasError(ensAction.US_SendMessageInfo, notRLFJob);
			AssertNoError(ensAction.US_SendMessageInfo, rLFJob);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNoError(ensAction.US_SendMessageInfo, branchIsNotConfiguredUnableToSendMsg);
			AssertNoError(ensAction.US_SendMessageInfo, notRLFJob);
			AssertNoError(ensAction.US_SendMessageInfo, rLFJob);
			declaration.US_SchDEntry = "3902";
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNoError(ensAction.US_SendMessageInfo, branchIsNotConfiguredUnableToSendMsg);
			AssertNoError(ensAction.US_SendMessageInfo, notRLFJob);
			AssertNoError(ensAction.US_SendMessageInfo, rLFJob);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNoError(ensAction.US_SendMessageInfo, branchIsNotConfiguredUnableToSendMsg);
			AssertNoError(ensAction.US_SendMessageInfo, notRLFJob);
			AssertHasError(ensAction.US_SendMessageInfo, rLFJob);
			declaration.US_EntryMode = ZString.Empty;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNoError(ensAction.US_SendMessageInfo, branchIsNotConfiguredUnableToSendMsg);
			AssertNoError(ensAction.US_SendMessageInfo, notRLFJob);
			AssertNoError(ensAction.US_SendMessageInfo, rLFJob);
		}

		public void TestExWarehouseEntryForCertifyForCargoRelease()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var exWarehouseAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			exWarehouseAction.US_SendMessage = true;
			Declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			exWarehouseAction.US_CertifyCargoRelease = true;
			AssertHasMessageError(exWarehouseAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.ShouldNotCertifyForExWarehouse);
		}

		public void TestCertifyCargoReleaseForRLF()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryMode = EntryModeList.Codes.RLF;
			Assert("Precondition: Declaration.IsRemoteLocationFiling", Declaration.IsRemoteLocationFiling);
			EntryHeaderMessageSendingAction cargoReleaseAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.CargoRelease);
			cargoReleaseAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(cargoReleaseAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			cargoReleaseAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			cargoReleaseAction.US_SendMessage = true;
			cargoReleaseAction.US_CertifyCargoRelease = false;
			AssertHasMessageError(cargoReleaseAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			EntryHeaderMessageSendingAction entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = false;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			entrySummaryAction.US_CertifyCargoRelease = false;
			AssertHasMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			entrySummaryAction.entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			entrySummaryAction.US_CertifyCargoRelease = false;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			entrySummaryAction.entry.US_CRLCertStatus = ZString.Empty;
			Declaration.US_EnableCRL = true;
			entrySummaryAction.US_CertifyCargoRelease = false;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
		}

		public void TestUS_SendMessageNotRLFCargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_SchDEntry = "0901";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var crlEntry = Factory.NewMoq<CusEntryHeader>();
			crlEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			crlEntry.Object.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(crlEntry.Object);
			declaration.US_EnableCRL = true;
			var registryItemsCollection = new BranchDistrictPortCollection(new ZArchitecture.Environment.FallbackLevel(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var item1 = registryItemsCollection.AddNew();
			item1.PortCode = "39";
			item1.BranchPK = GlbBranch.CurrentBranch.PK;
			DataRegistry.Business.USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, registryItemsCollection);
			GlbStaff.CurrentUser.GS_LoginName = "TestUser";
			GlbStaff.CurrentUser.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var cargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			string messageSendingType = "Cargo Release";
			AssertHasError(cargoReleaseAction.US_SendMessageInfo, string.Format(ValidationConstants.Declaration.NotRLFJob, messageSendingType));
			declaration.US_SchDEntry = "3902";
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			cargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			AssertNoError(cargoReleaseAction.US_SendMessageInfo, string.Format(ValidationConstants.Declaration.NotRLFJob, messageSendingType));
		}

		public void TestUS_SendMessageRLFCargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_SchDEntry = "3902";
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			Assert("Precondition: Declaration.IsRemoteLocationFiling", declaration.IsRemoteLocationFiling);
			declaration.US_EnableCRL = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var crlEntry = Factory.NewMoq<CusEntryHeader>();
			crlEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			crlEntry.Object.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(crlEntry.Object);
			declaration.US_EnableCRL = true;
			var registryItemsCollection = new BranchDistrictPortCollection(new ZArchitecture.Environment.FallbackLevel(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var item1 = registryItemsCollection.AddNew();
			item1.PortCode = "39";
			item1.BranchPK = GlbBranch.CurrentBranch.PK;
			DataRegistry.Business.USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, registryItemsCollection);
			GlbStaff.CurrentUser.GS_LoginName = "TestUser";
			GlbStaff.CurrentUser.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var cargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			string messageSendingType = "Cargo Release";
			AssertHasError(cargoReleaseAction.US_SendMessageInfo, string.Format(ValidationConstants.Declaration.RLFJob, messageSendingType));
			declaration.US_SchDEntry = "0902";
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			cargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			AssertNoError(cargoReleaseAction.US_SendMessageInfo, string.Format(ValidationConstants.Declaration.RLFJob, messageSendingType));
		}

		public void TestCertifyCargoReleaseForRLFWhenAutoSendENSAfterAIIActivated()
		{
			USCustomsDataRegistry.Instance.AutoSendEntrySummaryOnAcceptedAII.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryMode = EntryModeList.Codes.RLF;
			Assert("Precondition: Declaration.IsRemoteLocationFiling", Declaration.IsRemoteLocationFiling);
			var cargoReleaseAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.CargoRelease);
			cargoReleaseAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(cargoReleaseAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			cargoReleaseAction.US_CertifyCargoRelease = false;
			AssertNoMessageError("This validation should only apply with this registry on RLF jobs when Entry Summary send is actively chosen", cargoReleaseAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			var entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = false;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			entrySummaryAction.US_CertifyCargoRelease = false;
			AssertHasMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			entrySummaryAction.entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			entrySummaryAction.US_CertifyCargoRelease = false;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
		}

		public void TestValidateUS_CertifyCargoReleaseWhenAlreadyCertified()
		{
			var disposition = Declaration.DispositionCodes.AddNew(); // Disposition codes are added by "RR" messages which indicate certification
			EntryHeaderMessageSendingAction cargoReleaseAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.CargoRelease);
			cargoReleaseAction.US_SendMessage = true;
			cargoReleaseAction.US_CertifyCargoRelease = true;
			AssertHasMessageError(cargoReleaseAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CargoReleaseHasAlreadyBeenCertified);
			cargoReleaseAction.US_SendMessage = false;
			cargoReleaseAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(cargoReleaseAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CargoReleaseHasAlreadyBeenCertified);
			EntryHeaderMessageSendingAction entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = false;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CargoReleaseHasAlreadyBeenCertified);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertHasMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CargoReleaseHasAlreadyBeenCertified);
			disposition.Delete();
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CargoReleaseHasAlreadyBeenCertified);
			cargoReleaseAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(cargoReleaseAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CargoReleaseHasAlreadyBeenCertified);
		}

		public void TestValidateUS_CertifyCargoReleaseWhenAlreadyCertifiedForACERLF()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_EntryFilerCode = "AAA";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = "ACE";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var disposition = declaration.DispositionCodes.AddNew(); // Disposition codes are added by "RR" messages which indicate certification
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = actions[0];
			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = true;
			AssertHasMessageError(action.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CargoReleaseHasAlreadyBeenCertified);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			action.US_CertifyCargoRelease = true;
			AssertNoMessageError(action.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CargoReleaseHasAlreadyBeenCertified);
			declaration.US_EntryMode = ZString.Empty;
			declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
			action.US_CertifyCargoRelease = true;
			AssertHasMessageError(action.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CannotCertifyCRForReleasedEntry);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			action.US_CertifyCargoRelease = true;
			AssertNoMessageError(action.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CannotCertifyCRForReleasedEntry);
		}

		public void TestValidateUS_CertifyCargoReleaseWhenTickedOnCargoReleaseAndEntrySummary()
		{
			var cargoReleaseAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.CargoRelease);
			var entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			cargoReleaseAction.US_SendMessage = true;
			entrySummaryAction.US_SendMessage = true;
			cargoReleaseAction.US_CertifyCargoRelease = true;
			AssertEquals("Want to certify through CargoRelease", false, cargoReleaseAction.US_CertifyCargoReleaseInfo.HasMessageErrors());
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertHasMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.YouMayCertifyAtOneTypeOfEntry);
			entrySummaryAction.US_CertifyCargoRelease = false;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.YouMayCertifyAtOneTypeOfEntry);
			entrySummaryAction.US_SendMessage = false;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.YouMayCertifyAtOneTypeOfEntry);
			var seEntryMock = Factory.NewMoq<CusEntryHeader>();
			var seEntry = seEntryMock.Object;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableCRL = true;
			Declaration.CustomsEntryHeaders.Add(seEntry);
			seEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			seEntry.CH_CH_PrimeEntry = ENSEntry.PK;
			var collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			var simplifiedEntryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			simplifiedEntryAction.US_SendMessage = true;
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertHasMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.YouMayCertifyAtOneTypeOfEntry);
			entrySummaryAction.US_CertifyCargoRelease = false;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.YouMayCertifyAtOneTypeOfEntry);
		}

		public void TestValidateUS_CertifyCargoReleaseForEntryReleased()
		{
			ENSEntry.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-1);
			var entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertHasMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CannotCertifyCRForReleasedEntry);
			entrySummaryAction.US_CertifyCargoRelease = false;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, EntryHeaderMessageSendingActionValidation.CannotCertifyCRForReleasedEntry);
		}

		public void TestValidateUS_SendMessageForEntryQuery()
		{
			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ImportMessageSendingActionCollection collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.EntrySummaryQuery);
			ImportMessageSendingAction entrySummaryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CollectionBillInformationCode = CollectionBillInformationCodesList.Codes._1;
			AssertEquals(false, entrySummaryAction.US_SendMessageInfo.Notifications.HasMessageErrors());
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			entrySummaryAction.US_SendMessage = true;
			AssertEquals(false, entrySummaryAction.US_SendMessageInfo.Notifications.HasMessageErrors());
		}

		public void TestValidateUS_SendMessageForPSCReasonCodes()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_PSC = true;
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			var entrySummaryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			AssertHasMessageError(entrySummaryAction.US_SendMessageInfo, ValidationConstants.PSC.NoReasonCodeEntered);
			entrySummaryAction.PSCReasonCodes.AddNew();
			entrySummaryAction.US_SendMessage = true;
			AssertNoMessageError(entrySummaryAction.US_SendMessageInfo, ValidationConstants.PSC.NoReasonCodeEntered);
		}

		public void TestCheckUS_CollectionBillInformationCode()
		{
			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ImportMessageSendingActionCollection collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.EntrySummaryQuery);
			ImportMessageSendingAction entrySummaryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CollectionBillInformationCode = ZString.Empty;
			AssertHasMessageErrorContaining(entrySummaryAction.US_CollectionBillInformationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			entrySummaryAction.US_CollectionBillInformationCode = ZString.Empty;
			AssertNoMessageErrorContaining(entrySummaryAction.US_CollectionBillInformationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			entrySummaryAction.US_CollectionBillInformationCode = "5";
			AssertHasErrorContaining(entrySummaryAction.US_CollectionBillInformationCodeInfo, "Enter a valid selection.");
			entrySummaryAction.US_CollectionBillInformationCode = CollectionBillInformationCodesList.Codes._3;
			AssertNoErrorContaining(entrySummaryAction.US_CollectionBillInformationCodeInfo, "Enter a valid selection.");
		}

		public void TestCertifyCargoReleaseForPSC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.US_PSC = true;
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var entrySummaryAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			entrySummaryAction.US_SendMessage = true;
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertHasMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, ValidationConstants.PSC.CannotCertifyForPSC);
			entrySummaryAction.US_CertifyCargoRelease = false;
			AssertNoMessageError(entrySummaryAction.US_CertifyCargoReleaseInfo, ValidationConstants.PSC.CannotCertifyForPSC);
		}

		public void TestBrokerReferenceNumberWhenSendingOriginal()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var message = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, US.Messaging.Business.UpdateActionCode.Add).PopulateMessage();
			message.EM_LinkedObject = entry;
			Factory.Save();
			message.EM_MessageNum = "~1";
			var response = PSCEntrySummaryDataTest.CreateCustomsResponseSuccess(Factory, "~1");
			Factory.Save();
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions[0].US_SendMessage = true;
			Assert(!actions[0].US_SendMessageInfo.HasMessageError(ValidationConstants.EntrySummary.BrokerReferenceNumberDifferentAndReplacementShouldBeSent(declaration.JE_DeclarationReference)));
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_BRDRefNo = "TESTNUM1";
			var message2 = new EntrySummaryMessageBuilder(entry, US.Messaging.Business.UpdateActionCode.Replace, false).PopulateMessage();
			message2.EM_LinkedObject = entry;
			Factory.Save();
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			message2.EM_MessageNum = "~2";
			var response2 = PSCEntrySummaryDataTest.CreateCustomsResponseSuccess(Factory, "~2");
			Factory.Save();
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions[0].US_SendMessage = true;
			Assert(actions[0].US_SendMessageInfo.HasError(EntryHeaderMessageSendingActionValidation.NotSupportedByCBP));
		}

		public void TestSendingOriginalForPSCFiledByOtherBroker()
		{
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler()
			{ EntryFilerCode = "XJ5" });
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_PSC = true;
			declaration.US_EntryFilerCode = "EEB";
			declaration.US_EnableENS = true;
			Assert(declaration.IsPSCFilingOfEntriesByOtherFiler);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions[0].US_SendMessage = true;
			AssertHasMessageError(actions[0].US_SendMessageInfo, ValidationConstants.EntrySummary.PSCFilingOfEntriesFiledByOtherBroker);
			declaration.US_BRDRefNo = "342980";
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions[0].US_SendMessage = true;
			AssertNoMessageError(actions[0].US_SendMessageInfo, ValidationConstants.EntrySummary.PSCFilingOfEntriesFiledByOtherBroker);
		}

		public void TestCheckSimplifiedEntryFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SendMessage = true;
			seAction.US_SE_ContactName = ZString.Empty;
			AssertHasMessageErrorContaining(seAction.US_SE_ContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			seAction.US_SE_ContactPhone = ZString.Empty;
			AssertHasMessageErrorContaining(seAction.US_SE_ContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			seAction.US_SE_ContactName = "Johnny B. Broker";
			AssertNoMessageErrorContaining(seAction.US_SE_ContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			seAction.US_SE_ContactPhone = "555-0100";
			AssertNoMessageErrorContaining(seAction.US_SE_ContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			seAction.US_SendMessage = false;
			seAction.US_SE_ContactName = ZString.Empty;
			seAction.US_SE_ContactPhone = ZString.Empty;
			AssertNoMessageErrorContaining(seAction.US_SE_ContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(seAction.US_SE_ContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SendMessage = true;
			seAction.US_SE_ContactName = ZString.Empty;
			AssertHasMessageErrorContaining(seAction.US_SE_ContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			seAction.US_SE_ContactPhone = ZString.Empty;
			AssertHasMessageErrorContaining(seAction.US_SE_ContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			seAction.US_SE_ContactName = "Johnny B. Broker";
			AssertNoMessageErrorContaining(seAction.US_SE_ContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			seAction.US_SE_ContactPhone = "555-0100";
			AssertNoMessageErrorContaining(seAction.US_SE_ContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			seAction.US_SendMessage = false;
			seAction.US_SE_ContactName = ZString.Empty;
			seAction.US_SE_ContactPhone = ZString.Empty;
			AssertNoMessageErrorContaining(seAction.US_SE_ContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(seAction.US_SE_ContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			seAction = (EntryHeaderMessageSendingAction)actions[0];
			seAction.US_SendMessage = true;
			seAction.US_SE_MultipleDispositionsIndic = true;
			seAction.US_SE_ReasonCode = "~";
			AssertHasMessageErrorContaining(seAction.US_SE_ReasonCodeInfo, ListValidation.InvalidCodeMessageError);
			seAction.US_SE_ReasonCode = ReasonCodeList.Codes.EntryReplacedBy7512;
			AssertNoMessageErrorContaining(seAction.US_SE_ReasonCodeInfo, ListValidation.InvalidCodeMessageError);
			seAction.Validation.ValidateUS_SE_ReferenceNo();
			AssertHasMessageErrorContaining(seAction.US_SE_ReferenceNoInfo, EntryHeaderMessageSendingActionValidation.RefNoRequired);
			seAction.US_SE_ReferenceNo = "03212456";
			AssertNoMessageErrorContaining(seAction.US_SE_ReferenceNoInfo, EntryHeaderMessageSendingActionValidation.RefNoRequired);
			seAction.US_SE_ReasonCode = ZString.Empty;
			seAction.US_SE_ReferenceNo = ZString.Empty;
			AssertNoMessageErrorContaining(seAction.US_SE_ReferenceNoInfo, EntryHeaderMessageSendingActionValidation.RefNoRequired);
			seAction.US_SE_ReasonCode = ReasonCodeList.Codes.MerchandiseClearedByAnother;
			seAction.Validation.ValidateUS_SE_ReferenceNo();
			AssertHasMessageErrorContaining(seAction.US_SE_ReferenceNoInfo, EntryHeaderMessageSendingActionValidation.RefNoRequired);
			seAction.US_SE_ReferenceNo = "SV9-12345678";
			seAction.Validation.ValidateUS_SE_ReferenceNo();
			AssertHasMessageErrorContaining(seAction.US_SE_ReferenceNoInfo, EntryHeaderMessageSendingActionValidation.EntryNumberRightFormat);
			seAction.US_SE_ReferenceNo = "SV912345678";
			seAction.Validation.ValidateUS_SE_ReferenceNo();
			AssertNoMessageErrorContaining(seAction.US_SE_ReferenceNoInfo, EntryHeaderMessageSendingActionValidation.EntryNumberRightFormat);
		}

		public void TestCheckUS_SE_ActionType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement, (x) => x.IsACECargoRelease);
			var action = actions[0];
			action.US_SendMessage = true;
			action.US_SE_ActionType = "";
			AssertHasMessageErrorContaining(action.US_SE_ActionTypeInfo, MandatoryValidation.YouHaveNotEntered);
			action.US_SE_ActionType = "~";
			AssertNoMessageErrorContaining(action.US_SE_ActionTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(action.US_SE_ActionTypeInfo, ListValidation.InvalidCodeMessageError);
			action.US_SE_ActionType = ACECargoReleaseActionType.Codes.Replace;
			AssertNoMessageErrorContaining(action.US_SE_ActionTypeInfo, ListValidation.InvalidCodeMessageError);
			action.US_SendMessage = false;
			action.US_SE_ActionType = "";
			AssertNoMessageErrorContaining(action.US_SE_ActionTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestSendDeleteMessageWhenGoodsWithdrawals()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EnableENS = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entryHeader);
			entryHeader.EntryNumber = "00003877";
			Factory.Save();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Assert(declaration.IsBondedWarehousePermitEnabled);
			var startDate = new ZDateTime(2017, 07, 01);
			var endDate = new ZDateTime(2017, 12, 31);
			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 1000m, 1000m);
			var transaction1 = permit1.CusPermitLineTransactions[0];
			transaction1.CPL_Reference = "XJ5-00003877";
			Factory.Save();
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var aceCargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			aceCargoReleaseAction.US_SendMessage = true;
			AssertNoMessageErrorContaining(aceCargoReleaseAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.ShouldNotDeleteWhenGoodsWithdrawals);
			permitHelper.CreatePermitLineTransaction(permit1, "REF1", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			Factory.Save();
			var actions2 = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var aceCargoReleaseAction2 = actions2.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			aceCargoReleaseAction2.US_SendMessage = true;
			AssertHasMessageErrorContaining(aceCargoReleaseAction2.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.ShouldNotDeleteWhenGoodsWithdrawals);
		}

		public void TestDeleteACECargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var aceCargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			aceCargoReleaseAction.US_SendMessage = true;
			AssertNoMessageErrorContaining(aceCargoReleaseAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.ShouldNotDeleteCargoReleaseBeforeEntrySummary);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			aceCargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			aceCargoReleaseAction.US_SendMessage = true;
			AssertHasMessageErrorContaining(aceCargoReleaseAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.ShouldNotDeleteCargoReleaseBeforeEntrySummary);
		}

		public void TestShouldNotDeleteCargoReleaseBeforeEntrySummaryMessageWithEntrySummaryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			entrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var aceCargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			aceCargoReleaseAction.US_SendMessage = true;
			AssertNoMessageErrorContaining(aceCargoReleaseAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.ShouldNotDeleteCargoReleaseBeforeEntrySummary);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			aceCargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			aceCargoReleaseAction.US_SendMessage = true;
			AssertHasMessageErrorContaining(aceCargoReleaseAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.ShouldNotDeleteCargoReleaseBeforeEntrySummary);
			entrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			aceCargoReleaseAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			aceCargoReleaseAction.US_SendMessage = true;
			AssertNoMessageErrorContaining(aceCargoReleaseAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.ShouldNotDeleteCargoReleaseBeforeEntrySummary);
		}

		public void TestCheckUS_SendMessageWhenWaitingForResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var aceEntrySummaryAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			aceEntrySummaryAction.US_SendMessage = true;
			AssertNoMessageError(aceEntrySummaryAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.ShouldNotOriginalReplacementWhenWaitingForResponse);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			aceEntrySummaryAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			aceEntrySummaryAction.US_SendMessage = true;
			AssertHasMessageError(aceEntrySummaryAction.US_SendMessageInfo, EntryHeaderMessageSendingActionValidation.ShouldNotOriginalReplacementWhenWaitingForResponse);
		}

		public void TestCheckUS_SendMessageWhenWaitForSUResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var aceEntrySummaryAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			aceEntrySummaryAction.US_SendMessage = true;
			AssertNoMessageError(aceEntrySummaryAction.US_SendMessageInfo, ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);
			CreateSTUMessage(declaration, true);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			aceEntrySummaryAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			aceEntrySummaryAction.US_SendMessage = true;
			AssertHasMessageError(aceEntrySummaryAction.US_SendMessageInfo, ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			aceEntrySummaryAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			aceEntrySummaryAction.US_SendMessage = true;
			AssertHasMessageError(aceEntrySummaryAction.US_SendMessageInfo, ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);
			CreateSTUMessage(declaration, false);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			aceEntrySummaryAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			aceEntrySummaryAction.US_SendMessage = true;
			AssertNoMessageError(aceEntrySummaryAction.US_SendMessageInfo, ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			aceEntrySummaryAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			aceEntrySummaryAction.US_SendMessage = true;
			AssertNoMessageError(aceEntrySummaryAction.US_SendMessageInfo, ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);
		}

		void CreateSTUMessage(JobDeclaration declaration, bool isTransmit)
		{
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			message.EM_MessageType = isTransmit ? ACEApplicationIdentifierCodeList.Codes.StatementUpdate : ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			message.EM_ReceiveTransmit = isTransmit ? EDIMessage.Direction.Transmit : EDIMessage.Direction.Receive;
			message.EM_Status = isTransmit ? EDIMessage.Status.Sent : EDIMessage.Status.Received;
			message.EM_MessageNum = "HYEDUSCMT_000001";
			declaration.Messages.Add(message);
		}

		JobDeclaration declaration;
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

		CusEntryHeader ENSEntry => ENSEntryMock.Object;

		Mock<CusEntryHeader> ensEntryMock;
		Mock<CusEntryHeader> ENSEntryMock
		{
			get
			{
				if (ensEntryMock == null)
				{
					ensEntryMock = Factory.NewMoq<CusEntryHeader>();
					var entry = ensEntryMock.Object;
					Declaration.CustomsEntryHeaders.Add(entry);
					entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
					entry.EntryNumber = "11111";
					CusEntryLine entryline = entry.MergedLines.AddNew();
					InvoiceLine.JI_CL = entryline.PK;
				}

				return ensEntryMock;
			}
		}

		CusEntryHeader CRLEntry => CRLEntryMock.Object;

		Mock<CusEntryHeader> crlEntryMock;
		Mock<CusEntryHeader> CRLEntryMock
		{
			get
			{
				if (crlEntryMock == null)
				{
					crlEntryMock = Factory.NewMoq<CusEntryHeader>();
					var entry = crlEntryMock.Object;
					Declaration.CustomsEntryHeaders.Add(entry);
					entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
					entry.CH_CH_PrimeEntry = ENSEntry.PK;
					CusEntryLine entryline = entry.MergedLines.AddNew();
					InvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryline);
				}

				return crlEntryMock;
			}
		}

		CusEntryHeader INBEntry => INBEntryMock.Object;

		Mock<CusEntryHeader> inbEntryMock;
		Mock<CusEntryHeader> INBEntryMock
		{
			get
			{
				if (inbEntryMock == null)
				{
					inbEntryMock = Factory.NewMoq<CusEntryHeader>();
					var entry = inbEntryMock.Object;
					Declaration.CustomsEntryHeaders.Add(entry);
					entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
					CusEntryLine entryline = entry.MergedLines.AddNew();
					InvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryline);
				}

				return inbEntryMock;
			}
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var invoice = Declaration.Invoices.AddNew();
					invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}

		ImportMessageSendingActionCollection sendingActionCollection;
		ImportMessageSendingActionCollection SendingActionCollection
		{
			get
			{
				if (sendingActionCollection == null)
				{
					CusEntryHeader ensEntry = ENSEntry; //need to be touched
					CusEntryHeader inbEntry = INBEntry;
					CusEntryHeader crlEntry = CRLEntry;
					sendingActionCollection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
				}

				return sendingActionCollection;
			}
		}
	}
}
