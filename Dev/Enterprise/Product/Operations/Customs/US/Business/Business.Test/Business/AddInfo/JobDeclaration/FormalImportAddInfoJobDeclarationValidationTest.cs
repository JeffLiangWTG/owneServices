using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FormalImportAddInfoJobDeclarationValidationTest : CommonImportAddInfoJobDeclarationValidationTest
	{
		public void TestFlags()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			Bill bill = declaration.Bills.AddNew();
			AddInfoJobDeclaration addInfo = new AddInfoJobDeclaration(declaration.JE_AddInfoInfo);
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;
			AssertEquals(true, ((FormalImportAddInfoJobDeclarationValidation)addInfo.Validation).IsEntrySummaryValidationMode);
			AssertEquals(true, ((FormalImportAddInfoJobDeclarationValidation)addInfo.Validation).IsCargoReleaseValidationMode);
			declaration.US_EnableINB = true;
			AssertEquals(true, ((FormalImportAddInfoJobDeclarationValidation)addInfo.Validation).IsEntrySummaryValidationMode);
			AssertEquals(true, ((FormalImportAddInfoJobDeclarationValidation)addInfo.Validation).IsCargoReleaseValidationMode);
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
			AssertEquals(true, ((FormalImportAddInfoJobDeclarationValidation)addInfo.Validation).IsEntrySummaryValidationMode);
			AssertEquals(true, ((FormalImportAddInfoJobDeclarationValidation)addInfo.Validation).IsCargoReleaseValidationMode);
		}

		public void TestIsEntrySummaryOrCargoReleaseValidationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			var addInfo = new AddInfoJobDeclaration(declaration.JE_AddInfoInfo);
			var validation = (FormalImportAddInfoJobDeclarationValidation)addInfo.Validation;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableENS = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableCRL = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableENS = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableCRL = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_CertifyCargoRelease = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
		}

		public void TestUS_FTZNoForImportFTZ()
		{
			Assert("Precondition: is not sea", !declaration.IsSea);
			declaration.US_FTZNo = WrongFTZNumber;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Assert("Precondition: is not ConsumptionFTZ", !declaration.IsConsumptionFTZ);
			declaration.US_FTZNo = WrongFTZNumber;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Assert("Precondition: is ConsumptionFTZ", declaration.IsConsumptionFTZ);
			declaration.US_FTZNo = WrongFTZNumber;
			AssertHasMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = WrongFTZNumberWith000;
			AssertHasMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = WrongFTZNumberWith301;
			AssertHasMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = WrongFTZNumberWith001;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = WrongFTZNumberWith300;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = CorrectFTZNumber;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = ZString.Empty;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertHasMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.AddInfoValidation.ValidateUS_FTZNo();
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
		}

		public void TestUS_FTZNoForImportFTZNumberValidationAppliesToCRLAsWell()
		{
			declaration.US_EnableENS = true;
			Assert("Precondition: is not sea", !declaration.IsSea);
			declaration.US_FTZNo = WrongFTZNumber;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Assert("Precondition: is not ConsumptionFTZ", !declaration.IsConsumptionFTZ);
			declaration.US_FTZNo = WrongFTZNumber;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Assert("Precondition: is ConsumptionFTZ", declaration.IsConsumptionFTZ);
			declaration.US_FTZNo = WrongFTZNumber;
			AssertHasMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = WrongFTZNumberWith000;
			AssertHasMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = WrongFTZNumberWith301;
			AssertHasMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = WrongFTZNumberWith001;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = WrongFTZNumberWith300;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = CorrectFTZNumber;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertNoMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
			declaration.US_FTZNo = ZString.Empty;
			AssertNoMessageError(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberHasWrongFormat);
			AssertHasMessageErrorContaining(declaration.US_FTZNoInfo, FormalImportAddInfoJobDeclarationValidation.ImportFTZNumberIsMandatory);
		}

		public void TestCheckUS_ExpressConsignmentIndicator()
		{
			declaration.US_ExpConsign = "~";
			AssertHasMessageError(declaration.US_ExpConsignInfo, FormalImportAddInfoJobDeclarationValidation.ExpressConsignmentIndicatorShouldBeInList);
			declaration.US_ExpConsign = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(declaration.US_ExpConsignInfo, FormalImportAddInfoJobDeclarationValidation.ExpressConsignmentIndicatorShouldBeInList);
		}

		public void TestCheckUS_CargoReleaseTypeCannotBeChangeAfterCustomsTransaction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var cargoReleaseEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull("PreCondition", cargoReleaseEntry);
			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original);
			var actions = mainSender.Actions;
			actions[0].US_SendMessage = true;
			actions[0].US_CertifyCargoRelease = true;
			actions[0].US_AcknowledgeAndSign = true;
			Assert("precondition", actions[0].IsEntrySummary);
			mainSender.OnPrepare += new MainMessageSender.PrepareEventHandler(delegate
			{
				return true;
			});
			mainSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate
			{
				return true;
			});
			mainSender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{
				Factory.Save();
			});
			mainSender.SendMessage();
			Assert("ACE Cargo release entry should say 'waiting for response' while cargo release being ceritified from entry summary", cargoReleaseEntry.IsWaitingForResponse);
			Assert("When certify cargo release is done from entry summary, we should mark the fact on the SE entry so that users can no longer change US_CargoReleaseType", cargoReleaseEntry.HasTransactionsWithCustoms);
			declaration.AddInfoValidation.ValidateUS_CargoReleaseType();
			Assert(!declaration.US_CargoReleaseTypeInfo.HasError(FormalImportAddInfoJobDeclarationValidation.CargoReleaseTypeCannotBeChanged));
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertHasErrorContaining(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CargoReleaseTypeCannotBeChanged);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertNoErrorContaining(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CargoReleaseTypeCannotBeChanged);
			declaration.AddInfoValidation.ValidateUS_EnableCRL();
			AssertNoErrors("This is a valid situation. Should not issue an error", declaration.US_EnableCRLInfo);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertNoErrorContaining(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CargoReleaseTypeCannotBeChanged);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertHasErrorContaining(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CargoReleaseTypeCannotBeChanged);
		}

		public void TestCheckUS_CargoReleaseTypeFromACS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.EntrySummaryEntry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			Assert(declaration.HasCargoReleaseBeenCertified);
			Factory.Save();
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertHasMessageErrorContaining(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CargoReleaseTypeCannotBeChangedAfterCertified);
		}

		public void TestCheckUS_PayableMPF()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_MonthlyFiling = true;
			declaration.US_PayableMPF = ZDecimal.Zero;
			declaration.US_EnableENS = true;
			AssertHasMessageErrorContaining(declaration.US_PayableMPFInfo, FormalImportAddInfoJobDeclarationValidation.MPFMayNotBeZero);
			declaration.US_MonthlyFiling = true;
			declaration.US_PayableMPF = -1;
			AssertNoMessageErrorContaining(declaration.US_PayableMPFInfo, FormalImportAddInfoJobDeclarationValidation.MPFMayNotBeZero);
			AssertHasMessageErrorContaining(declaration.US_PayableMPFInfo, FormalImportAddInfoJobDeclarationValidation.MPFMayNotBeNegative);
			declaration.US_PayableMPF = 1;
			AssertNoMessageErrorContaining(declaration.US_PayableMPFInfo, FormalImportAddInfoJobDeclarationValidation.MPFMayNotBeNegative);
		}

		public void TestCheckUS_BRDRefNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			declaration.US_BRDRefNo = "123456";
			AssertNoErrors(declaration.US_BRDRefNoInfo);
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			dec2.US_BRDRefNo = "123456";
			AssertHasError(dec2.US_BRDRefNoInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.DuplicateDeclarationWithSameRefNo, declaration.JE_DeclarationReference));
			dec2.US_BRDRefNo = "123457";
			AssertNoError(dec2.US_BRDRefNoInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.DuplicateDeclarationWithSameRefNo, declaration.JE_DeclarationReference));
			declaration.US_EnableENS = true;
			declaration.US_BRDRefNo = "B123456789";
			AssertHasWarning(declaration.US_BRDRefNoInfo, FormalImportAddInfoJobDeclarationValidation.LastNineLettersOfBrokerReferenceWillBeSent);
			declaration.US_BRDRefNo = "123456789";
			AssertNoWarning(declaration.US_BRDRefNoInfo, FormalImportAddInfoJobDeclarationValidation.LastNineLettersOfBrokerReferenceWillBeSent);
		}

		public void TestCargoReleaseTypeFromACEtoSE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();
			declaration.US_EnableCRL = true;
			declaration.AddInfoValidation.ValidateUS_CargoReleaseType();
			Assert(!declaration.US_CargoReleaseTypeInfo.HasError(FormalImportAddInfoJobDeclarationValidation.CargoReleaseTypeCannotBeChanged));
		}

		public void TestCheckUS_EnableCRL()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertForUS_EnableCRL(declaration);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertForUS_EnableCRL(declaration);
			//ACE Cargo Release
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entry);
			entry.Messages.AddNew(typeof(EDIMessage));
			AssertEquals("precondition", false, entry.HasTransactionsWithCustoms);
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
			AssertEquals("precondition", true, entry.HasTransactionsWithCustoms);
			declaration.US_EnableCRL = false;
			AssertHasErrorContaining(declaration.US_EnableCRLInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
			declaration.US_EnableCRL = true;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseDelete;
			Assert(entry.HasBeenWithdrawn);
			declaration.US_EnableCRL = false;
			AssertNoErrorContaining(declaration.US_EnableCRLInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
		}

		public void TestCheckUS_EnableENS()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(entry);
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			declaration.US_EnableENS = false;
			AssertHasErrorContaining(declaration.US_EnableENSInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
			entry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
			declaration.US_EnableENS = false;
			AssertHasErrorContaining(declaration.US_EnableENSInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.US_EnableENS = true;
			AssertNoErrorContaining(declaration.US_EnableENSInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			declaration.US_EnableENS = false;
			AssertHasErrorContaining(declaration.US_EnableENSInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
			declaration.US_EnableENS = true;
			AssertNoErrorContaining(declaration.US_EnableENSInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
		}

		public void TestITNumberWithNoITDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_MasterBill = "OBL";
			declaration.JE_DateOfArrival = ZDateTime.Today.AddHours(6);
			Bill masterBill = declaration.PrimaryMasterBill;
			masterBill.ITNumber = "123";
			declaration.US_ITDate = ZDateTime.Empty;
			AssertHasMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenITNumberIsEntered);
			AssertNoMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateCannotBeforeArrivalDate);
			declaration.US_ITDate = ZDateTime.Today.AddHours(1);
			AssertNoMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateCannotBeforeArrivalDate);
			declaration.US_ITDate = ZDateTime.Today.AddDays(-1);
			AssertNoMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenITNumberIsEntered);
			AssertHasMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateCannotBeforeArrivalDate);
			declaration.US_ITDate = ZDateTime.Today.AddDays(+1);
			AssertNoMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenITNumberIsEntered);
			AssertNoMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateCannotBeforeArrivalDate);
			//export date validation depend on IT date
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(+2);
			declaration.US_ITDate = ZDateTime.Today.AddDays(+1);
			AssertHasMessageError(declaration.JE_ExportDateInfo, FormalImportJobDeclarationValidation.ExportDateShouldBeLessThanITDate);
			declaration.US_ITDate = ZDateTime.Today.AddDays(+2);
			AssertNoMessageError(declaration.JE_ExportDateInfo, FormalImportJobDeclarationValidation.ExportDateShouldBeLessThanITDate);
		}

		public void TestITDateWhenEntryAndDischargeDiffer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.JE_MasterBill = "OBL";
			declaration.US_SchDArrival = "2904";
			declaration.US_SchDEntry = "2907";
			declaration.US_ITDate = ZDateTime.Empty;
			declaration.AddInfoValidation.ValidateUS_ITDate();
			AssertHasMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenEntryAndDischargePortDifferBCR);
			declaration.US_SchDEntry = "2904";
			declaration.US_ITDate = ZDateTime.Empty;
			declaration.AddInfoValidation.ValidateUS_ITDate();
			AssertNoMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenEntryAndDischargePortDifferBCR);
			declaration.US_SchDEntry = "2907";
			declaration.US_ITDate = ZDateTime.Now;
			AssertNoMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenEntryAndDischargePortDifferBCR);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			declaration.US_SchDArrival = "2904";
			declaration.US_SchDEntry = "1101";
			declaration.US_ITDate = ZDateTime.Empty;
			declaration.AddInfoValidation.ValidateUS_ITDate();
			AssertHasMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenEntryAndDischargeDistrictDiffer);
			declaration.US_ITDate = ZDateTime.Now;
			AssertNoMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenEntryAndDischargeDistrictDiffer);
			declaration.US_SchDEntry = "1101";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.US_ITDate = ZDateTime.Empty;
			AssertHasMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenEntryAndDischargeDistrictDiffer);
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_ITDate = ZDateTime.Empty;
			AssertNoMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateIsRequiredWhenEntryAndDischargeDistrictDiffer);
			declaration.US_ITDate = ZDateTime.BrettsBirthday;
			AssertHasMessageError(declaration.US_ITDateInfo, FormalImportAddInfoJobDeclarationValidation.ITDateCannotBeMoreThan2YearsInThePast);
		}

		public void TestCheckUS_PreparerDistrictPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			const string CorrectPreparerDistrictPort = "8888";
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_EnableCRL = false;
			declaration.US_EnableENS = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_PreparerDistrictPort = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortIsMandatory);
			declaration.US_EnableCRL = true;
			declaration.AddInfoValidation.ValidateUS_PreparerDistrictPort();
			AssertHasMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortIsMandatory);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.AddInfoValidation.ValidateUS_PreparerDistrictPort();
			AssertHasMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortIsMandatory);
			declaration.US_EntryMode = "";
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortIsMandatory);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = CorrectPreparerDistrictPort;
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortIsMandatory);
			declaration.US_PreparerDistrictPort = CorrectPreparerDistrictPort;
			AssertNoMessageError(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortShouldBeInList);
			declaration.US_PreparerDistrictPort = "~";
			AssertHasMessageError(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortShouldBeInList);
		}

		public void TestCheckUS_WHSEntryNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_WHSEntryNumber = "";
			AssertHasMessageErrorContaining(declaration.US_WHSEntryNumberInfo, FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberRequired);
			declaration.US_WHSEntryNumber = "A";
			AssertNoMessageErrorContaining(declaration.US_WHSEntryNumberInfo, FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberRequired);
			AssertHasMessageErrors(FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberFormat, declaration.US_WHSEntryNumberInfo);
			declaration.US_WHSEntryNumber = "12345678";
			AssertNoMessageErrors(FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberFormat, declaration.US_WHSEntryNumberInfo);
		}

		[TestDate(2015, 07, 29)]
		public void TestCheckUS_EntryMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.US_EntryDate = new ZDateTime(2011, 01, 28);
			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			AssertHasMessageError(declaration.US_EntryModeInfo, ValidationConstants.PAIIsInvalidAfter_28_01_2011);
			declaration.US_EntryDate = new ZDateTime(2011, 01, 27);
			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			AssertNoMessageError(declaration.US_EntryModeInfo, ValidationConstants.PAIIsInvalidAfter_28_01_2011);
		}

		public void TestCheckUS_WHSDistrictPortCode()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "9987", "Test Port", startDate, endDate);
			newFactory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			declaration.US_WHSDistrictPortCode = "ZZZZ";
			AssertNoMessageErrorContaining(declaration.US_WHSDistrictPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_WHSDistrictPortCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_WHSDistrictPortCode = "";
			AssertHasMessageErrorContaining(declaration.US_WHSDistrictPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_WHSDistrictPortCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_WHSDistrictPortCode = "9987";
			AssertNoMessageErrorContaining(declaration.US_WHSDistrictPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_WHSDistrictPortCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_WHSEntryFilerCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			declaration.US_WHSEntryFilerCode = "";
			AssertHasMessageErrors(FormalImportAddInfoJobDeclarationValidation.WHSFilerCode, declaration.US_WHSEntryFilerCodeInfo);
			declaration.US_WHSEntryFilerCode = "888";
			AssertNoMessageErrors(FormalImportAddInfoJobDeclarationValidation.WHSFilerCode, declaration.US_WHSEntryFilerCodeInfo);
			declaration.US_WHSEntryFilerCode = "A8";
			AssertHasMessageErrors(FormalImportAddInfoJobDeclarationValidation.WHSFilerCode, declaration.US_WHSEntryFilerCodeInfo);
		}

		public void TestCheckUS_EntryTypeFor2500InformalLimit()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = UsualTariffForCheckUS_EntryTypeTest;
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValueAfter2013Jan7 + 1;
			declaration.US_EstimatedEntryDate = ValidationConstants.InformalEntryLimit2500StartDate;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Consolidated;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertHasMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsInformalFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValueAfter2013Jan7));
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValue + 1;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsInformalFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValueAfter2013Jan7));
		}

		public void TestCheckUS_EntryType_WithWHSTransaction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			Factory.Save();
			declaration.US_EntryType = ZString.Empty;
			AssertHasError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			foreach (var type in new[] { EntryTypeList.Codes.ReWarehouse, EntryTypeList.Codes.Warehouse, EntryTypeList.Codes.WarehouseFTZ })
			{
				declaration.US_EntryType = type;
				AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
				AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			}

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertHasError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			AssertHasError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCanceled;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			Factory.Save();
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCanceled;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			Factory.Save();
			declaration.US_EntryType = ZString.Empty;
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			AssertHasError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			foreach (var type in new[] {//EntryTypeList.Codes.ConsumptionFTZ, DN: Todo remove this when we know more info
			EntryTypeList.Codes.WarehouseWithdrawalADDCVD, EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa, //EntryTypeList.Codes.AircraftVesselSupplyIE, DN: Todo remove this when we know more info
			EntryTypeList.Codes.WarehouseWithdrawalConsumption, EntryTypeList.Codes.WarehouseWithdrawalQuota })
			{
				declaration.US_EntryType = type;
				AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
				AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			}

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			AssertHasError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertNoError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			AssertHasError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForOutward);
		}

		public void TestCheckUS_EntryType()
		{
			const string GoodsReturnedTariffForCheckUS_EntryTypeTest = FormalImportAddInfoJobDeclarationValidation.USGoodsReturnedTariffPrefix + "10"; //9801001010
			var helper = new DeclarationTestHelper(Factory);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = UsualTariffForCheckUS_EntryTypeTest;
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValue + 1;
			declaration.US_EstimatedEntryDate = ValidationConstants.InformalEntryLimit2500StartDate.AddDays(-1);
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Consolidated;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertHasMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsInformalFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValue));
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Personal;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsInformalFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValue));
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Consolidated;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertNoMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsInformalFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValue));
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValue;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertNoMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsInformalFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValue));
			invoiceLine.JI_Tariff = GoodsReturnedTariffForCheckUS_EntryTypeTest;
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Consolidated;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsAmericanGoodsReturnedFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue));
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue + 1;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertHasMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsAmericanGoodsReturnedFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue));
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Personal;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsAmericanGoodsReturnedFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue));
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Consolidated;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertNoMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsAmericanGoodsReturnedFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue));
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertNoMessageError(declaration.US_EntryTypeInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsAmericanGoodsReturnedFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue));
			AssertNoMessageError(declaration.US_EntryTypeInfo, ValidationConstants.CargoRelease.EntryType);
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = "";
			AssertHasMessageError(declaration.US_EntryTypeInfo, ValidationConstants.CargoRelease.EntryType);
			declaration.US_EnableCRL = false;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertNoMessageError(declaration.US_EntryTypeInfo, ValidationConstants.EntrySummary.EntryType);
			declaration.US_EntryType = "";
			AssertHasMessageError(declaration.US_EntryTypeInfo, ValidationConstants.EntrySummary.EntryType);
			declaration.ValidationModes = ValidationModes.StandAlonePriorNotice;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertNoMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PriorNotice.EntryType);
			declaration.US_EntryType = "";
			AssertHasMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PriorNotice.EntryType);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertNoMessageError(declaration.US_EntryTypeInfo, ValidationConstants.CargoRelease.EntryType);
			declaration.US_EntryType = "";
			AssertHasMessageError(declaration.US_EntryTypeInfo, ValidationConstants.CargoRelease.EntryType);
		}

		public void TestCheckUS_UI_NKCarrierSCAC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = "";
			declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertNoMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertUS_UI_NKCarrierSCACRequired(declaration);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertUS_UI_NKCarrierSCACRequired(declaration);
			AssertUS_UI_NKCarrierSCACValidity(declaration);
			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_ModeOfTransportation, "10"));
			declaration.ValidationModes = ValidationModes.StandAlonePriorNotice;
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			AssertHasMessageError(declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.PriorNotice.CarrierSCAC);
			declaration.US_UI_NKCarrierSCAC = "ZZ";
			AssertNoMessageError(declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.PriorNotice.CarrierSCAC);
			AssertNoMessageErrors(declaration.US_UI_NKCarrierSCACInfo);
		}

		public void TestCheckUS_UI_NKCarrierSCACForBorderMovement()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_UI_NKCarrierSCAC = "AAAD";
			AssertNoMessageErrorContaining(declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestDesignatedExamPort()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_SchDExam = "3902";
			AssertHasMessageErrorContaining(declaration.US_SchDExamInfo, FormalImportAddInfoJobDeclarationValidation.ExamPortForRLFOnly);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			Assert("Precondition: declaration.IsRemoteLocationFiling", declaration.IsRemoteLocationFiling);
			declaration.AddInfoValidation.ValidateUS_SchDExam();
			AssertNoMessageErrorContaining(declaration.US_SchDExamInfo, FormalImportAddInfoJobDeclarationValidation.ExamPortForRLFOnly);
			declaration.US_DES = "A001";
			declaration.US_SchDExam = "";
			AssertHasMessageError(declaration.US_SchDExamInfo, FormalImportAddInfoJobDeclarationValidation.ExamPortRequired);
			declaration.US_SchDExam = "3901";
			AssertNoMessageError(declaration.US_SchDExamInfo, FormalImportAddInfoJobDeclarationValidation.ExamPortRequired);
		}

		public void TestUS_PreparerDistrictPort()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_PreparerDistrictPort = "3901";
			AssertHasMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortRLFOnly);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.AddInfoValidation.ValidateUS_PreparerDistrictPort();
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortRLFOnly);
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_EntryMode = "";
			declaration.US_PreparerDistrictPort = "0901";
			AssertHasMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortRLFOnly);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.AddInfoValidation.ValidateUS_PreparerDistrictPort();
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortRLFOnly);
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EntryMode = "";
			declaration.US_PreparerDistrictPort = "3901";
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, FormalImportAddInfoJobDeclarationValidation.PreparerDistrictPortRLFOnly);
		}

		public void TestUS_PreparerOfficeCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_PreparerOfficeCode = "39";
			AssertHasMessageErrorContaining(declaration.US_PreparerOfficeCodeInfo, FormalImportAddInfoJobDeclarationValidation.PreparerOfficeCodeRLFOnly);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.AddInfoValidation.ValidateUS_PreparerOfficeCode();
			AssertNoMessageErrorContaining(declaration.US_PreparerOfficeCodeInfo, FormalImportAddInfoJobDeclarationValidation.PreparerOfficeCodeRLFOnly);
		}

		public void TestCheckUS_TaxDeferIndicator()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_TaxDeferIndicator = "";
			AssertHasMessageErrorContaining(declaration.US_TaxDeferIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_TaxDeferIndicator = "~";
			AssertNoMessageErrorContaining(declaration.US_TaxDeferIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_TaxDeferIndicatorInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			AssertNoMessageErrorContaining(declaration.US_TaxDeferIndicatorInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageErrorContaining(declaration.US_TaxDeferIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.TaxIsToBeDeferredOnlyOnFormalEntries);
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax;
			AssertNoMessageErrorContaining(declaration.US_TaxDeferIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.TaxIsToBeDeferredOnlyOnFormalEntries);
		}

		public void TestCheckJE_MessageTypeForEntryFilerCode()
		{
			DeclarationTestHelper.SetEntryFilerCode("");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var messageText = string.Format(CommonImportJobDeclarationValidation.EntryFilerCodeIsEmptyAndEntryNumberCannotBeGenerated, CommonImportJobDeclarationValidation.WhyNeedEntryFilerCode);
			AssertHasErrorContaining(declaration.JE_MessageTypeInfo, messageText);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoErrorContaining(declaration.JE_MessageTypeInfo, messageText);
			DeclarationTestHelper.SetEntryFilerCode("");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertHasErrorContaining(declaration.JE_MessageTypeInfo, messageText);
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			AssertNoErrorContaining(declaration.JE_MessageTypeInfo, messageText);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertNoErrors(declaration.JE_MessageTypeInfo);
		}

		public void TestTotalPayableExceedingBondAmount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			CusEntryHeader entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_TotalPaid = 10000m;
			declaration.US_BondAmount = 5000m;
			AssertNoMessageError(declaration.US_BondAmountInfo, FormalImportAddInfoJobDeclarationValidation.TotalPayableGreaterThanBondAmount);
			declaration.US_BondAmount = 10000m;
			AssertNoMessageError(declaration.US_BondAmountInfo, FormalImportAddInfoJobDeclarationValidation.TotalPayableGreaterThanBondAmount);
			declaration.US_BondAmount = 0m;
			AssertNoMessageError(declaration.US_BondAmountInfo, FormalImportAddInfoJobDeclarationValidation.TotalPayableGreaterThanBondAmount);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount = 5000m;
			AssertNoMessageError("Should not error now as auto calculation being done", declaration.US_BondAmountInfo, FormalImportAddInfoJobDeclarationValidation.TotalPayableGreaterThanBondAmount);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.US_BondAmount = 5000m;
			AssertHasMessageError("Should error again as Manual override chosen", declaration.US_BondAmountInfo, FormalImportAddInfoJobDeclarationValidation.TotalPayableGreaterThanBondAmount);
		}

		public void TestMinAndMaxValidationForMANCalcCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			Mock<CusEntryHeader> entry1 = Factory.NewMoq<CusEntryHeader>();
			entry1.Setup(m => m.CustomsValue).Returns(10000m);
			entry1.Object.CH_JE = declaration.PK;
			entry1.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.CustomsEntryHeaders.Add(entry1.Object);
			declaration.ActiveEntryHeaders.Rebuild();
			declaration.US_BondAmount = 95m;
			AssertHasWarning(declaration.US_BondAmountInfo, FormalImportAddInfoJobDeclarationValidation.BelowMinimumWithManualValue);
			declaration.US_BondAmount = 950m;
			AssertNoWarning(declaration.US_BondAmountInfo, FormalImportAddInfoJobDeclarationValidation.BelowMinimumWithManualValue);
		}

		public void TestCheckIOROrgPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.US_TTBInd = "D";
			line.TTBLines.AddNew();
			AssertIOROrgPK(declaration);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			AssertIOROrgPK(declaration);
		}

		public void TestCheckIOROrgPKForBorderCargoRelease()
		{
			declaration.ValidationModes = ValidationModes.CargoRelease;
			Organization.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			var iorWrapper = OrgHeaderWrapper.New(Organization);
			iorWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;
			Organization.CustomsCodes.RemoveAll();
			declaration.IOROrgPK = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.IOROrgPK = Organization.PK;
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJobdeclarationValidateCharactorsForAddressDescription()
		{
			var addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			var addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";
			var party2 = Factory.New<OrgHeader>();
			var orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";
			declaration.JE_OA_InvoicerAddress = orgAddress1.PK;
			AssertHasWarning(declaration.JE_OA_InvoicerAddressInfo, addressDescriptionWarning);
			AssertHasWarning(declaration.JE_OA_InvoicerAddressInfo, addressCodeWarning);
			declaration.JE_OA_InvoicerAddress = orgAddress2.PK;
			AssertNoWarning(declaration.JE_OA_InvoicerAddressInfo, addressDescriptionWarning);
			AssertNoWarning(declaration.JE_OA_InvoicerAddressInfo, addressCodeWarning);
			declaration.JE_OA_ConsigneeAddress = orgAddress1.PK;
			AssertHasWarning(declaration.JE_OA_ConsigneeAddressInfo, addressDescriptionWarning);
			AssertHasWarning(declaration.JE_OA_ConsigneeAddressInfo, addressCodeWarning);
			declaration.JE_OA_ConsigneeAddress = orgAddress2.PK;
			AssertNoWarning(declaration.JE_OA_ConsigneeAddressInfo, addressDescriptionWarning);
			AssertNoWarning(declaration.JE_OA_ConsigneeAddressInfo, addressCodeWarning);
		}

		public void TestCheckUS_BondCalcCode()
		{
			declaration.US_BondCalcCode = ZString.Empty;
			AssertNoMessageErrors(declaration.US_BondCalcCodeInfo);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = "";
			AssertHasMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.CalcCodeRequired);
			declaration.US_BondCalcCode = "AAA";
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.CalcCodeRequired);
			AssertHasMessageErrors("List Validation error", declaration.US_BondCalcCodeInfo);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.EXH;
			AssertNoMessageErrors(declaration.US_BondCalcCodeInfo);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;
			AssertHasMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TemporaryImportationBond);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TemporaryImportationBond);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TemporaryImportationBond);
			declaration.US_EntryType = EntryTypeList.Codes.TradeFair;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.EXH;
			AssertHasMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TradFairBond);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TradFairBond);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			AssertHasMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.NotTIBEntry);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.NotTIBEntry);
			declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;
			AssertHasMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.ExhibitionBond);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.EXH;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.ExhibitionBond);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TemporaryImportationBond);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TemporaryImportationBond);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MSC;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TemporaryImportationBond);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.UFM;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TemporaryImportationBond);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			AssertNoMessageError(declaration.US_BondCalcCodeInfo, FormalImportAddInfoJobDeclarationValidation.TemporaryImportationBond);
		}

		public void TestCheckUS_US_NKLocationOfGoodsForEntrySummary()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			this.declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(true, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_US_NKLocationOfGoodsInfo, "Location of Goods (FIRMS code) is required for Entry Summary (where Entry Type is ");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertEquals(false, EntryTypeList.IsWarehouseLocationNeededFor(EntryTypeList.Codes.ConsumptionFreeDutiable));
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_US_NKLocationOfGoodsInfo, "Location of Goods (FIRMS code) is required for Entry Summary (where Entry Type is ");
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_US_NKLocationOfGoodsInfo, "Location of Goods (FIRMS code) is required for Entry Summary (where Entry Type is ");
		}

		public void TestCheckUS_US_NKLocationOfGoodsForCargoRelease()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			this.declaration.US_EnableENS = true;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "Z2Z2", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			declaration.US_EnableCRL = true;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsNotOnFile);
			declaration.US_US_NKLocationOfGoods = "Z1Z1";
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsNotOnFile);
			declaration.US_US_NKLocationOfGoods = "Z2Z2";
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsNotOnFile);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsNotOnFile);
			declaration.US_US_NKLocationOfGoods = "Z1Z1";
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsNotOnFile);
			this.declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			this.declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageError(this.declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
			this.declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			this.declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertNoMessageError(this.declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
		}

		public void TestCheckUS_US_NKLocationOfGoodsFacilityType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.FirmsCodeFacilityType07or08);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "E1E2", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, "07");
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "A0A7", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, "08");
			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "L8L8", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, "01");
			Factory.Save();

			declaration.US_US_NKLocationOfGoods = "E1E2"; // Code with Facility Type 07
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.FirmsCodeFacilityType07or08);
			declaration.US_US_NKLocationOfGoods = "A0A7"; // Code with Facility Type 08
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.FirmsCodeFacilityType07or08);
			declaration.US_US_NKLocationOfGoods = "L8L8";
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.FirmsCodeFacilityType07or08);
		}

		public void TestCheckUS_US_NKLocationOfGoodsFacilityIsActive()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "GAZ1", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode, "2702");
			Factory.Save();

			declaration.US_SchDEntry = "5201";
			declaration.US_US_NKLocationOfGoods = "GAZ1";
			AssertHasWarning(declaration.US_US_NKLocationOfGoodsInfo, ValidationConstants.Declaration.FirmsNotOnTheSameDistrict);
			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			declaration.AddInfoValidation.ValidateUS_US_NKLocationOfGoods();
			AssertNoWarning("No warning with port is a paired port", declaration.US_US_NKLocationOfGoodsInfo, ValidationConstants.Declaration.FirmsNotOnTheSameDistrict);
			declaration.US_EntryMode = "";
			declaration.AddInfoValidation.ValidateUS_US_NKLocationOfGoods();
			AssertHasWarning("Only show the warning when port is not a paired port", declaration.US_US_NKLocationOfGoodsInfo, ValidationConstants.Declaration.FirmsNotOnTheSameDistrict);
			declaration.US_SchDEntry = "2702";
			declaration.US_US_NKLocationOfGoods = "GAZ1";
			AssertNoWarning(declaration.US_US_NKLocationOfGoodsInfo, ValidationConstants.Declaration.FirmsNotOnTheSameDistrict);
		}

		public void TestFieldsNotRequiredForExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_SchDArrival = "";
			declaration.US_SchDLoading = "";
			AssertEquals(false, declaration.US_SchDArrivalInfo.HasMessageErrors());
			AssertEquals(false, declaration.US_SchDLoadingInfo.HasMessageErrors());
		}

		public void TestCheckUS_ConsolidatedInformalIndicator()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ImmediateTransportation;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Samples;
			AssertHasMessageErrorContaining(declaration.US_ConsolidatedInformalIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.MustBeInformalEntryForSamples);
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Personal;
			AssertNoMessageErrorContaining(declaration.US_ConsolidatedInformalIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.MustBeInformalEntryForSamples);
			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Samples;
			AssertNoMessageErrorContaining(declaration.US_ConsolidatedInformalIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.MustBeInformalEntryForSamples);
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Samples;
			AssertNoMessageErrorContaining(declaration.US_ConsolidatedInformalIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.MustBeInformalEntryForSamples);
			declaration.US_ConsolidatedInformalIndicator = "~";
			AssertHasMessageError(declaration.US_ConsolidatedInformalIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ConsolidatedInformalIndicatorShouldBeInList);
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Consolidated;
			AssertNoMessageError(declaration.US_ConsolidatedInformalIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ConsolidatedInformalIndicatorShouldBeInList);
		}

		public void TestUS_EstimatedEntryDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.US_EstimatedEntryDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EstimatedEntryDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(declaration.US_EstimatedEntryDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_EstimatedEntryDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.US_EstimatedEntryDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2012, 07, 04); // hard coded Independance Day holiday
			AssertNoMessageErrors("There should be no restriction on entering a public holiday in Est. Entry Date", declaration.US_EstimatedEntryDateInfo);
		}

		[TestDate(2008, 4, 1)]
		public void TestEstEntryDate()
		{
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.US_EstimatedEntryDate = ZDateTime.Today;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			AssertNoMessageErrors(declaration.US_EstimatedEntryDateInfo);
			AssertNoMessageErrors(declaration.US_PreliminaryStatementPrintDateInfo);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_EstimatedEntryDate = ZDateTime.Today.AddDays(20);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(10);
			AssertHasMessageError(declaration.US_EstimatedEntryDateInfo, FormalImportAddInfoJobDeclarationValidation.EstEntryDateAfterPaymentDate);
			declaration.US_EstimatedEntryDate = ZDateTime.Today.AddDays(1);
			AssertNoMessageError(declaration.US_PreliminaryStatementPrintDateInfo, FormalImportAddInfoJobDeclarationValidation.EstEntryDateAfterPaymentDate);
			declaration.US_ITDate = ZDateTime.Today.AddDays(10);
			declaration.US_EstimatedEntryDate = ZDateTime.Today;
			AssertHasMessageErrorContaining(declaration.US_EstimatedEntryDateInfo, FormalImportAddInfoJobDeclarationValidation.EstEntryDateBeforeITDate);
		}

		[TestDate(2009, 3, 1)]
		public void TestCheckUS_PeriodicStatementMMWhenPaid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 12, 21);
			declaration.US_PeriodicStatementMM = "01";
			AssertHasMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextTwo);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = declaration.ImportEntryNumber;
			AssertNotNull(statementLine.Declaration);
			Factory.Save();
			AssertNotNull(declaration.RelatedStatement);
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			Assert(declaration.RelatedStatement.IsPaid);
			declaration.US_PeriodicStatementMM = "01";
			AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextTwo);
		}

		[TestDate(2008, 01, 17)]
		public void TestCheckUS_PeriodicStatementMM1()
		{
			SetBranchHolidays();
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 02, 21)]
		public void TestCheckUS_PeriodicStatementMM2()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 03, 23)]
		public void TestCheckUS_PeriodicStatementMM3()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 06, 23)]
		public void TestCheckUS_PeriodicStatementMM4()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 05, 23)]
		public void TestCheckUS_PeriodicStatementMM5()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 06, 23)]
		public void TestCheckUS_PeriodicStatementMM6()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 07, 23)]
		public void TestCheckUS_PeriodicStatementMM7()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 08, 23)]
		public void TestCheckUS_PeriodicStatementMM8()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 09, 23)]
		public void TestCheckUS_PeriodicStatementMM9()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 10, 21)]
		public void TestCheckUS_PeriodicStatementMM10()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 11, 20)]
		public void TestCheckUS_PeriodicStatementMM11()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2008, 12, 21)]
		public void TestCheckUS_PeriodicStatementMM12()
		{
			CheckUS_PeriodicStatementMM();
		}

		[TestDate(2013, 03, 05)]
		public void TestCheckUS_PeriodicStatementAgainstReleaseDate()
		{
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 03, 01);
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2013, 03, 13);
			declaration.US_PeriodicStatementMM = "3";
			AssertHasWarning(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.MonthTheSameAsReleaseDateMonth);
			declaration.US_PeriodicStatementMM = "4";
			AssertNoWarning(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.MonthTheSameAsReleaseDateMonth);
		}

		[TestDate(2013, 04, 24)]
		public void TestCheckUS_PeriodicStatementAgainstReleaseDate2()
		{
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 05, 01);
			declaration.US_PeriodicStatementMM = "5";
			AssertHasWarning(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.MonthTheSameAsReleaseDateMonth);
			declaration.US_PeriodicStatementMM = "6";
			AssertNoWarning(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.MonthTheSameAsReleaseDateMonth);
		}

		[TestDate(2023, 6, 26)]
		public void TestCheckUS_PeriodicStatementAgainstReleaseDate_MustSameOrNext()
		{
			declaration.ValidationModes = ValidationModes.EntrySummary;
			for (var monthIndex = 1; monthIndex < 13; monthIndex++)
			{
				var nextMonth = monthIndex == 12 ? 1 : monthIndex + 1;
				TestCheckUS_PeriodicStatementAgainstReleaseDate_MustSameOrNextCore(monthIndex, new int[] { monthIndex, nextMonth });
			}
		}

		void TestCheckUS_PeriodicStatementAgainstReleaseDate_MustSameOrNextCore(int month, int[] validMonth)
		{
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2023, month, 16);
			for (var i = 1; i < 13; i++)
			{
				declaration.US_PeriodicStatementMM = i.ToString("D2");
				if (validMonth.Contains(i))
				{
					AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextOneToRealeaseDate);
				}
				else
				{
					AssertHasMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextOneToRealeaseDate);
				}
			}
		}

		public void TestPeriodicMonthRequiredForSelectedPaymentType()
		{
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_PeriodicStatementMM = ZString.Empty;
			AssertHasMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.MonthRequiredForPeriodicPaymentType);
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_PeriodicStatementMM = "01";
			AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.MonthRequiredForPeriodicPaymentType);
			AssertHasMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.MonthNotRequiredForSelectedPaymentType);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes;
			AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.MonthRequiredForPeriodicPaymentType);
			AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.MonthNotRequiredForSelectedPaymentType);
		}

		public void TestDisableMessagingModeWhenActiveEntriesExist()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("There should be two active entries", 2, declaration.ActiveEntryHeaders.Count);
			declaration.US_EnableCRL = false;
			AssertEquals("Should be able to disable now", false, declaration.US_EnableCRLInfo.HasError(FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist));
			declaration.DoMerge();
			declaration.RunPreSaveValidation();
			AssertEquals("There should be one active entry", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("No error should be there", false, declaration.HasErrors);
			declaration.US_EnableCRL = true;
			declaration.DoMerge();
			AssertEquals("There should be two active entries", 2, declaration.ActiveEntryHeaders.Count);
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var crlAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.CargoRelease);
			crlAction.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(declaration.MessageInitiator);
			var crlEntry = (CusEntryHeader)declaration.ActiveEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.CargoRelease))[0];
			AssertEquals("preCondition:isWaitingForResponse", true, crlEntry.IsWaitingForResponse);
			declaration.US_EnableCRL = false;
			AssertHasError(declaration.US_EnableCRLInfo, "Cargo Release" + FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertNoError(declaration.US_EnableCRLInfo, "Cargo Release" + FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
			declaration.DoMerge();
			AssertEquals("There should be two active entries", 2, declaration.ActiveEntryHeaders.Count);
			AssertEquals("No error should be there", false, declaration.HasErrors);
		}

		public void TestCheckUS_BondAmountAndBondProducerAccNo()
		{
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			var error = "value cannot be zero.";
			AssertHasMessageErrorContaining(declaration.US_BondAmountInfo, error);
			AssertHasMessageErrorContaining(declaration.US_BondProducerAccNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;
			AssertNoWarningContaining(declaration.US_BondAmountInfo, error);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertNoWarningContaining(declaration.US_BondAmountInfo, error);
			AssertNoWarningContaining(declaration.US_BondProducerAccNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.US_BondAmount = 0m;
			AssertHasMessageErrorContaining(declaration.US_BondAmountInfo, error);
			error = "value cannot be negative.";
			declaration.US_BondAmount = -1m;
			AssertHasMessageErrorContaining(declaration.US_BondAmountInfo, error);
			declaration.US_BondProducerAccNo = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_BondProducerAccNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_BondAmount = 100m;
			AssertNoWarningContaining(declaration.US_BondAmountInfo, error);
			declaration.US_BondProducerAccNo = "1";
			AssertNoMessageErrorContaining(declaration.US_BondProducerAccNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_BondDesignationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_ITDate = ZDateTime.Today;
			declaration.US_EstimatedEntryDate = ZDateTime.Today.AddDays(-15);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = "";
			declaration.AddInfoValidation.ValidateUS_BondDesignationCode();
			AssertHasMessageError(declaration.US_BondDesignationCodeInfo, FormalImportAddInfoJobDeclarationValidation.ShouldEntryDesignationCode);
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
			declaration.AddInfoValidation.ValidateUS_BondDesignationCode();
			AssertNoMessageError(declaration.US_BondDesignationCodeInfo, FormalImportAddInfoJobDeclarationValidation.ShouldEntryDesignationCode);
		}

		public void TestCheckBondProducerAccNoAndBondDates()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			CusBondDetailCollection bondData = new CusBondDetailCollection(importer);
			CusBondDetail oneBondData = bondData.AddNew();
			oneBondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			oneBondData.PW_BondAmount = 50000m;
			oneBondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			oneBondData.PW_BondNumber = "123456";
			oneBondData.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			oneBondData.PW_SuretyCode = "891";
			oneBondData.PW_BondFiledPort = "3901";
			oneBondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(-2);
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.IOROrgPK = importer.PK;
			declaration.US_ITDate = ZDateTime.Today;
			declaration.US_EstimatedEntryDate = ZDateTime.Today.AddDays(-15);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			declaration.US_SuretyCode = "891";
			string message = "The Bond is not effective as it is later than the declaration bond date, ";
			declaration.AddInfoValidation.ValidateUS_BondProducerAccNo();
			AssertNoMessageErrorContaining(declaration.US_BondProducerAccNoInfo, message);
			declaration.US_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondProducerAccNo = "123456";
			AssertHasMessageErrorContaining(declaration.US_BondProducerAccNoInfo, message);
			declaration.US_EstimatedEntryDate = ZDateTime.Today.AddDays(-5);
			declaration.US_BondProducerAccNo = "123456";
			AssertNoMessageErrorContaining(declaration.US_BondProducerAccNoInfo, message);
			AssertNoMessageError(declaration.US_BondProducerAccNoInfo, FormalImportAddInfoJobDeclarationValidation.BondExpired);
			declaration.US_EstimatedEntryDate = ZDateTime.Today;
			declaration.US_BondProducerAccNo = "123456";
			AssertHasMessageError(declaration.US_BondProducerAccNoInfo, FormalImportAddInfoJobDeclarationValidation.BondExpired);
			declaration.US_EstimatedEntryDate = ZDateTime.Empty;
			declaration.US_BondProducerAccNo = "123456";
			AssertHasMessageError(declaration.US_BondProducerAccNoInfo, FormalImportAddInfoJobDeclarationValidation.BondExpired);
			oneBondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(3);
			declaration.US_BondProducerAccNo = "123456";
			AssertNoMessageError(declaration.US_BondProducerAccNoInfo, FormalImportAddInfoJobDeclarationValidation.BondExpired);
			oneBondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(12);
			oneBondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(10);
			declaration.US_BondProducerAccNo = "123456";
			AssertHasMessageErrorContaining(declaration.US_BondProducerAccNoInfo, message);
			oneBondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			declaration.US_BondProducerAccNo = "123456";
			AssertNoMessageErrorContaining(declaration.US_BondProducerAccNoInfo, message);
		}

		public void TestCheckUS_BondProducerAccNo_CS00159470()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerBonds = new CusBondDetailCollection(importer);
			var bond = importerBonds.AddNew();
			bond.PW_ActivityCode = ActivityCodeList.Codes._1;
			bond.PW_BondAmount = 50000m;
			bond.PW_BondEffectiveDate = new ZDateTime(2009, 6, 1);
			bond.PW_BondNumber = "2233";
			bond.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bond.PW_SuretyCode = "891";
			bond.PW_BondFiledPort = "9900";
			bond = importerBonds.AddNew();
			bond.PW_ActivityCode = ActivityCodeList.Codes._1;
			bond.PW_BondAmount = 50000m;
			bond.PW_BondEffectiveDate = new ZDateTime(2001, 1, 1);
			bond.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bond.PW_SuretyCode = "089";
			bond.PW_BondFiledPort = "9900";
			bond.PW_BondExpiryDate = new ZDateTime(2009, 6, 1);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.IOROrgPK = importer.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("pre-condition", true, !declaration.US_SuretyCode.IsEmpty);
			AssertEquals(false, declaration.US_BondProducerAccNoInfo.HasMessageErrors());
		}

		public void TestCheckUS_SchDArrival()
		{
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			var bill = declaration.Bills.AddNew();
			declaration.ValidationModes = ValidationModes.CargoRelease;
			declaration.US_SchDArrival = "";
			AssertHasMessageError(declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_SchDArrival = "";
			AssertNoMessageError(declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDArrival = "7777";
			AssertNoMessageError(declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
			declaration.US_SchDArrival = "";
			AssertHasMessageError(declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.US_SchDArrival = "7777";
			AssertNoMessageError(declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.US_SchDArrival = "";
			AssertHasMessageErrorContaining(declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.US_SchDArrival = "73477";
			AssertNoMessageError(declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.US_SchDArrival = "~";
			AssertHasMessageError("Port Of Discharge", declaration.US_SchDArrivalInfo, ListValidation.InvalidCodeMessageError);
			AssertCheckUS_SchDArrivalAgainstTransportMode(declaration);
			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertNoMessageError("Port Of Discharge", declaration.US_SchDArrivalInfo, ValidationConstants.Declaration.InvalidPortForTransportMode(TransportTypeList.Codes.Sea));
			declaration.JE_PrimaryITNumber = ZString.Empty;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableINB = false;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_SchDArrival = ZString.Empty;
			AssertNoMessageError("Port Of Discharge", declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.JE_PrimaryITNumber = "12345678";
			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertHasMessageError("Port Of Discharge", declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.JE_PrimaryITNumber = ZString.Empty;
			var invLine = declaration.InvoiceLines.AddNew();
			invLine.FSISLines.AddNew();
			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertHasMessageError("Port Of Discharge", declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertHasMessageError("Port Of Discharge", declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.JE_TransportMode = Core.Constants.TransportModes.PassengerHandCarried;
			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertHasMessageError("Port Of Discharge", declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
			declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertHasMessageError("Port Of Discharge", declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
		}

		public void TestCheckUS_SchDEntry()
		{
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_PrimaryITNumber = "125468";
			declaration.JE_PrimaryITNumber = "";
			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			declaration.US_SchDArrival = "2904";
			declaration.US_SchDEntry = "8888";
			AssertNoMessageError(declaration.US_SchDEntryInfo, FormalImportAddInfoJobDeclarationValidation.PairedPortOfEntrySameAsPortOfDischarge);
			declaration.US_SchDEntry = "2904";
			AssertHasMessageError(declaration.US_SchDEntryInfo, FormalImportAddInfoJobDeclarationValidation.PairedPortOfEntrySameAsPortOfDischarge);
			declaration.US_EntryDate = new ZDateTime(2011, 01, 27);
			declaration.US_SchDEntry = "2904";
			AssertHasMessageError(declaration.US_SchDEntryInfo, FormalImportAddInfoJobDeclarationValidation.PairedPortOfEntrySameAsPortOfDischarge);
			declaration.US_EntryDate = new ZDateTime(2011, 01, 29);
			declaration.US_SchDEntry = "2904";
			AssertNoMessageError(declaration.US_SchDEntryInfo, FormalImportAddInfoJobDeclarationValidation.PairedPortOfEntrySameAsPortOfDischarge);

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "###3", "Test PR Port", startDate, endDate);
			newFactory.Save();

			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = "1101";
			declaration.US_SchDEntry = "1101";
			AssertHasWarning(declaration.US_SchDEntryInfo, FormalImportAddInfoJobDeclarationValidation.RLFPortOfEntrySameAsPreparerPort);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = "1101";
			declaration.US_SchDEntry = "2704";
			AssertNoWarning(declaration.US_SchDEntryInfo, FormalImportAddInfoJobDeclarationValidation.RLFPortOfEntrySameAsPreparerPort);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_SchDEntry = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(declaration.US_SchDEntryInfo, FormalImportAddInfoJobDeclarationValidation.SubmitPortOfEntryIsRecommend);
			declaration.JE_PrimaryITNumber = "1234567";
			declaration.AddInfoValidation.ValidateUS_SchDEntry();
			AssertHasMessageErrorContaining(declaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_PrimaryITNumber = ZString.Empty;
			var invLine = declaration.InvoiceLines.AddNew();
			invLine.FSISLines.AddNew();
			declaration.AddInfoValidation.ValidateUS_SchDEntry();
			AssertHasMessageErrorContaining(declaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.Bills[0].ITAndSplitDetails.RemoveAndDeleteAll();
			invLine.FSISLines.RemoveAndDeleteAll();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.AddInfoValidation.ValidateUS_SchDEntry();
			AssertNoMessageErrorContaining(declaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.AddInfoValidation.ValidateUS_SchDEntry();
			AssertHasMessageErrorContaining(declaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_NonAMS = true;
			declaration.AddInfoValidation.ValidateUS_SchDEntry();
			AssertHasMessageErrorContaining(declaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_SchDEntry = "2704";
			AssertNoMessageErrorContaining(declaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_SchDEntryWithITDetails()
		{
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_SchDArrival = "2205";
			declaration.US_SchDEntry = "2205";
			AssertNoMessageError(declaration.US_SchDEntryInfo, ValidationConstants.Declaration.PortOfEntrySameAsDischargeWhenITPresent);
			declaration.US_ITDate = ZDateTime.Now;
			declaration.AddInfoValidation.ValidateUS_SchDEntry();
			AssertHasMessageError(declaration.US_SchDEntryInfo, ValidationConstants.Declaration.PortOfEntrySameAsDischargeWhenITPresent);
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_SchDArrival = "2205";
			declaration.US_SchDEntry = "2205";
			AssertNoMessageError(declaration.US_SchDEntryInfo, ValidationConstants.Declaration.PortOfEntrySameAsDischargeWhenITPresent);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDEntry = "1101";
			AssertNoMessageError(declaration.US_SchDEntryInfo, ValidationConstants.Declaration.PortOfEntrySameAsDischargeWhenITPresent);
		}

		public void TestUS_EntryType()
		{
			declaration.US_EntryType = "~~";
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, "Please enter a valid Entry Type Code");
			declaration.US_EntryType = declaration.AddInfoLookups.US_EntryTypeList[0].Code;
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, "Please enter a valid Entry Type Code");
		}

		public void TestUS_BondType()
		{
			AssertUS_BondType(declaration);
		}

		public void TestUS_BondTypeForBorderCargoRelease()
		{
			declaration.ValidationModes = ValidationModes.CargoRelease;
			AssertUS_BondType(declaration);
		}

		public void TestUS_BondTypeForACECargoRelease()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.ValidationModes = ValidationModes.CargoRelease;
			AssertUS_BondType(declaration);
		}

		public void TestCheckReconciliationIndicators()
		{
			declaration.US_OtherReconIndicator = "~";
			AssertHasMessageError(declaration.US_OtherReconIndicatorInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			AssertNoMessageError(declaration.US_OtherReconIndicatorInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_EntryType = EntryTypeList.Codes.AircraftVesselSupplyIE;
			declaration.US_OtherReconIndicator = ZString.Empty;
			declaration.US_NAFTAReconIndicator = false;
			AssertNoMessageError(declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.JobNotFlaggedForReconciliation);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			declaration.US_NAFTAReconIndicator = true;
			AssertHasMessageError(declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.JobNotFlaggedForReconciliation);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.NotApplicable;
			AssertNoMessageError("NA means not filing a recon. This code should not cause a validation error.", declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.JobNotFlaggedForReconciliation);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			declaration.US_NAFTAReconIndicator = true;
			AssertNoMessageError(declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.JobNotFlaggedForReconciliation);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			AssertHasMessageError(declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ReconIssueInvalidForConsumptionQuotaVisa);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			AssertNoMessageError(declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.ReconIssueInvalidForConsumptionQuotaVisa);
			declaration.US_OtherReconIndicator = ZString.Empty;
			AssertHasMessageError(declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.SelectReconFor7501Entry);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.NotApplicable;
			AssertNoMessageError(declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.SelectReconFor7501Entry);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.Value9802Recon;
			AssertHasWarning(declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.No9802Tariff);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3290991000";
			invoiceLine.US_SupTariff = "9802008040";
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.Value9802Recon;
			AssertNoWarning(declaration.US_OtherReconIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.No9802Tariff);
		}

		public void TestUS_PeriodicStatementMM()
		{
			declaration.US_PeriodicStatementMM = MonthList.Codes._01;
			AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_PeriodicStatementMM = "~~";
			AssertHasMessageError(declaration.US_PeriodicStatementMMInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestUS_PaymentType()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";
			Factory.Save();
			declaration.US_PaymentType = "";
			AssertHasMessageError(declaration.US_PaymentTypeInfo, FormalImportAddInfoJobDeclarationValidation.PaymentTypeRequiredWarning);
			declaration.US_PaymentType = "~";
			AssertNoMessageError(declaration.US_PaymentTypeInfo, FormalImportAddInfoJobDeclarationValidation.PaymentTypeRequiredWarning);
			AssertHasMessageErrorContaining(declaration.US_PaymentTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_PaymentType = "1";
			AssertNoMessageErrorContaining(declaration.US_PaymentTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_TaxRate = 0.1m;
			invoiceLine.JI_CustomsQuantity = 10m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			Assert(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEstimatedTax > 0);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertHasMessageError(declaration.US_PaymentTypeInfo, FormalImportAddInfoJobDeclarationValidation.IRTaxNotAllowedToBePaidOnAMonthlyStatement);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertNoMessageError(declaration.US_PaymentTypeInfo, FormalImportAddInfoJobDeclarationValidation.IRTaxNotAllowedToBePaidOnAMonthlyStatement);
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertHasMessageError(declaration.US_PaymentTypeInfo, FormalImportAddInfoJobDeclarationValidation.IndividualBasisPaymentInvalidForRLFEntry);
			declaration.FormalEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.AddInfoValidation.ValidateUS_PaymentType();
			AssertNoMessageError(declaration.US_PaymentTypeInfo, FormalImportAddInfoJobDeclarationValidation.IndividualBasisPaymentInvalidForRLFEntry);
		}

		public void TestUS_ClientBranchDesignation()
		{
			declaration.US_PaymentType = "";
			declaration.US_ClientBranchDesignation = "XX";
			AssertEquals(true, declaration.US_ClientBranchDesignationInfo.HasMessageErrors());
			declaration.US_PaymentType = "";
			declaration.US_ClientBranchDesignation = "";
			AssertEquals(false, declaration.US_ClientBranchDesignationInfo.HasMessageErrors());
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_ClientBranchDesignation = "XX";
			AssertEquals(true, declaration.US_ClientBranchDesignationInfo.HasMessageErrors());
			declaration.US_ClientBranchDesignation = "";
			AssertEquals(false, declaration.US_ClientBranchDesignationInfo.HasMessageErrors());
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			declaration.US_ClientBranchDesignation = "XX";
			AssertEquals(false, declaration.US_ClientBranchDesignationInfo.HasMessageErrors());
			var branchPK = declaration.Branch != null ? declaration.Branch.PK.ToGuid() : GlbBranch.CurrentBranch.PK.ToGuid();
			var clientRegistryBranchDesignation = USCustomsDataRegistry.Instance.ClientBranchDesignation.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, GlbDepartment.CurrentDepartment.PK.ToGuid());
			var newClientRegistryBranchDesignation = "11";
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), newClientRegistryBranchDesignation);
			declaration.US_ClientBranchDesignation = "~";
			AssertHasMessageErrorContaining("Client Branch Designation should have message error because value differs from registry.", declaration.US_ClientBranchDesignationInfo, string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, newClientRegistryBranchDesignation));
			declaration.US_ClientBranchDesignation = newClientRegistryBranchDesignation;
			AssertNoErrorContaining("Client Branch Designation should not have message error because value from registry.", declaration.US_ClientBranchDesignationInfo, string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, newClientRegistryBranchDesignation));
			declaration.US_ClientBranchDesignation = "";
			AssertNoErrorContaining("Client Branch Designation should not have message error because value of US_ClientBranchDesignation is empty.", declaration.US_ClientBranchDesignationInfo, string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, newClientRegistryBranchDesignation));
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), "");
			declaration.US_ClientBranchDesignation = "~";
			AssertNoErrorContaining("Client Branch Designation should not have message error because value from registry is empty.", declaration.US_ClientBranchDesignationInfo, string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, newClientRegistryBranchDesignation));
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), clientRegistryBranchDesignation);
		}

		public void TestCheckUS_SchDArrivalForEntrySummary()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2205", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var port2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4103", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTransportModeForCusCodeList(port.PK, TransportTypeList.Codes.Sea);
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(port2.PK, attributeNameUnlading.ZXE_Name, "Y");

			Factory.Save();

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_SchDArrival = "2205"; // Sea Port
			AssertHasMessageError(declaration.US_SchDArrivalInfo, ValidationConstants.Declaration.InvalidPortForTransportMode(TransportTypeList.Codes.Air));
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.US_SchDArrival = "4103"; // Air Port
			AssertHasMessageError(declaration.US_SchDArrivalInfo, ValidationConstants.Declaration.InvalidPortForTransportMode(TransportTypeList.Codes.Sea));
			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertNoMessageErrors(declaration.US_SchDArrivalInfo);
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			declaration.US_SchDArrival = "2205"; // Sea Port
			AssertNoMessageErrors(declaration.US_SchDArrivalInfo);
		}

		[TestDate(2008, 8, 26)]
		public void TestUS_PreliminaryStatementPrintDate()
		{
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			AssertNoMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateRequired);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			AssertHasMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateRequired);
			declaration.US_PaymentType = "";
			declaration.US_PresentationDate = ZDateTime.Today;
			AssertNoMessageError("Date should not be defaulted yet", declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintNotRequired);
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);
			AssertHasMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintNotRequired);
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			AssertNoMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateRequired);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertDatesOnHolidaysAndWeekends(declaration.US_PreliminaryStatementPrintDateInfo);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			AssertHasMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateShouldBeInTheFuture);
			declaration.US_PaymentDate = ZDateTime.Today;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			AssertNoMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateShouldBeInTheFuture);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_PeriodicStatementMM = "09";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 09, 10);
			AssertNoMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PSDMustBeLessThanOrEqual11BusinessDay);
			declaration.US_PeriodicStatementMM = "01";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 12, 20);
			AssertNoMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PSDMustBeLessThanOrEqual11BusinessDay);
			declaration.US_PeriodicStatementMM = "09";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 09, 25);
			AssertHasMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PSDMustBeLessThanOrEqual11BusinessDay);
		}

		[TestDate(2008, 8, 26)]
		public void TestUS_PreliminaryStatementPrintDateDefaulting()
		{
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			AssertNoMessageErrors(declaration.US_PreliminaryStatementPrintDateInfo);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);
			AssertHasMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintNotRequired);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertNoMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintNotRequired);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(91);
			AssertHasMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateMoreThan90DaysInTheFuture);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(85);
			AssertNoMessageError(declaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateMoreThan90DaysInTheFuture);
			DefaultStatementPrintDate statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(2);
			AssertHasWarningContaining(declaration.US_PreliminaryStatementPrintDateInfo, "The Preliminary Statement Print Date differs from the calculated default value, (05-Sep-08).");
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 05, 01);
			AssertEquals("US_PreliminaryStatementPrintDate should have generated date plus 8 working days date", new ZDateTime(2008, 05, 13), declaration.US_PreliminaryStatementPrintDate);
			AssertNoWarningContaining(declaration.US_PreliminaryStatementPrintDateInfo, "The Preliminary Statement Print Date differs from the calculated default value, (13-May-08).");
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(10);
			AssertHasWarningContaining(declaration.US_PreliminaryStatementPrintDateInfo, "The Preliminary Statement Print Date differs from the calculated default value, (13-May-08).");
			statementData.NumberOfDays = 5;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			ZDateTime entryDate = new ZDateTime(2008, 06, 16);
			declaration.US_EstimatedEntryDate = entryDate;
			AssertEquals("US_PreliminaryStatementPrintDate should have generated date plus 5 Working days date", new ZDateTime(2008, 06, 23), declaration.US_PreliminaryStatementPrintDate);
			AssertNoWarningContaining(declaration.US_PreliminaryStatementPrintDateInfo, "The Preliminary Statement Print Date differs from the calculated default value, (23-Jun-08).");
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(10);
			AssertHasWarningContaining(declaration.US_PreliminaryStatementPrintDateInfo, "The Preliminary Statement Print Date differs from the calculated default value, (23-Jun-08).");
			declaration.US_PresentationDate = new ZDateTime(2008, 08, 30);
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 09, 05);
			AssertNoNotifications(declaration.US_PreliminaryStatementPrintDateInfo);
			OrgHeader iOR = Factory.NewWithValidTestData<OrgHeader>();
			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)iOR.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_SPDNumberOfDays = 5;
			Factory.Save();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			declaration.IOROrgPK = iOR.PK;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(2);
			AssertHasWarning(declaration.US_PreliminaryStatementPrintDateInfo, "The Preliminary Statement Print Date differs from the calculated default value, (05-Sep-08). The Preliminary Statement Print Date default value is calculated based on the following precedence of date values, Release Date, Presentation Date or Estimated Entry Date, (if entered, in precedence), or the later of Estimated Date of Arrival (ETA) and Current Date plus the number of working days, (5 working days, for this specific Importer of Record), based upon their Organization setting. \r\nTo alter this setting, (if required), adjust the 'Statement Print Date working days to be added' field on the Importer of Record details in Organization > Details > Config > US Defaults tab.\r\n\r\nTo alter the underlying system registry if necessary, please adjust the registry setting: Maintain > System > Registry > Customs > United States of America > Import > ABI > Statement > Default Statement Print Date?");
		}

		public void TestCheckUS_FixPSD()
		{
			AssertNoErrors(declaration.US_FixPSDInfo);
			declaration.US_FixPSD = true;
			AssertHasErrors(declaration.US_FixPSDInfo);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);
			AssertNoErrors(declaration.US_FixPSDInfo);
		}

		public void TestCheckUS_PresentationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.PresentationDate;
			AssertDatesOnHolidaysAndWeekends(declaration.US_PresentationDateInfo);
			declaration.US_EnableCRL = true;
			declaration.US_PresentationDate = ZDateTime.Empty;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.ArrivalDate;
			AssertNoMessageErrors(declaration.US_PresentationDateInfo);
			declaration.US_PresentationDate = new ZDateTime(2008, 02, 06);
			AssertHasMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNotRequired);
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.PresentationDate;
			declaration.US_PresentationDate = new ZDateTime(2008, 02, 06);
			AssertNoMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNotRequired);
			declaration.US_PresentationDate = ZDateTime.Empty;
			AssertHasMessageErrors(declaration.US_PresentationDateInfo);
			declaration.US_PresentationDate = new ZDateTime(2008, 02, 06);
			AssertNoMessageErrors(declaration.US_PresentationDateInfo);
			declaration.US_EntryDate = new ZDateTime(2008, 02, 07);
			declaration.US_PresentationDate = new ZDateTime(2008, 02, 06);
			AssertHasMessageError(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateRequiredPriorETA);
			declaration.US_PresentationDate = declaration.US_PresentationDate.AddDays(+1);
			AssertNoMessageErrors(declaration.US_PresentationDateInfo);
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.ArrivalDate;
			declaration.AddInfoValidation.ValidateUS_PresentationDate();
			AssertNoMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNotRequired);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.AddInfoValidation.ValidateUS_PresentationDate();
			AssertNoMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNotRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_PresentationDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNotRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertHasMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNotRequired);
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.PresentationDate;
			declaration.US_PresentationDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNotRequired);
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EntryDate = new ZDateTime(2008, 02, 07);
			declaration.US_PresentationDate = new ZDateTime(2008, 02, 06);
			AssertNoMessageError(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateRequiredPriorETA);
		}

		public void TestCheckUS_PresentationDateLaterThanReleaseDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 8, 10);
			declaration.US_PresentationDate = new ZDateTime(2011, 8, 11);
			AssertHasMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNoLaterThanReleaseDate);
			AssertHasMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNotRequired);
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.PresentationDate;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNotRequired);
			declaration.US_PresentationDate = new ZDateTime(2011, 8, 9);
			AssertNoMessageErrorContaining(declaration.US_PresentationDateInfo, FormalImportAddInfoJobDeclarationValidation.PresentationDateNoLaterThanReleaseDate);
		}

		public void TestUS_EnableENSHasWarningWhenEntryRangeIsReachedForCompany()
		{
			declaration.US_EnableENS = false;
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			companyStmNums.FirstNumberRange.SNR_ThresholdRunOutWarning = 60000;
			var factory = companyStmNums.Factory;
			factory.Save();
			companyStmNums.SetNextNumber(companyStmNums.SN_MaximumValue - 60000);
			factory.InvalidateCachedProperties();
			AssertEquals(false, companyStmNums.HasReachedLimit);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertEquals(false, declaration.US_EntryTypeInfo.HasWarnings());
			declaration.US_EntryType = "";
			companyStmNums.SetNextNumber(companyStmNums.SN_MaximumValue - 59999);
			factory.InvalidateCachedProperties();
			AssertEquals(true, companyStmNums.HasReachedLimit);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var message = string.Format(FormalImportAddInfoJobDeclarationValidation.CompanyEntryNumberLimitReachWarning, companyStmNums.TotalAvailableNumbers);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals(true, declaration.US_EntryTypeInfo.HasWarning(message));
		}

		public void TestUS_EnableENSHasWarningWhenEntryRangeIsReachedForBranch()
		{
			var branchStmNums = DeclarationTestHelper.SetupBranchSpecificFormalEntryNumber("XJ5");
			branchStmNums.FirstNumberRange.SNR_ThresholdRunOutWarning = 60000;
			var factory = branchStmNums.Factory;
			factory.Save();
			branchStmNums.SetNextNumber(branchStmNums.SN_MaximumValue - 60000);
			factory.InvalidateCachedProperties();
			AssertEquals(false, branchStmNums.HasReachedLimit);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertEquals(false, declaration.US_EntryTypeInfo.HasWarnings());
			declaration.US_EntryType = "";
			branchStmNums.SetNextNumber(branchStmNums.SN_MaximumValue - 59999);
			factory.InvalidateCachedProperties();
			AssertEquals(true, branchStmNums.HasReachedLimit);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var message = string.Format(FormalImportAddInfoJobDeclarationValidation.BranchEntryNumberLimitReachWarning, branchStmNums.TotalAvailableNumbers);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals(true, declaration.US_EntryTypeInfo.HasWarning(message));
		}

		public void TestCheckUS_OGALineReleaseIndicator()
		{
			declaration.US_OGALineReleaseIndicator = "~";
			AssertHasMessageError(declaration.US_OGALineReleaseIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.OGALineReleaseIndicatorShouldBeInList);
			declaration.US_OGALineReleaseIndicator = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(declaration.US_OGALineReleaseIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.OGALineReleaseIndicatorShouldBeInList);
		}

		public void TestCheckUS_EntryDateElectionCode()
		{
			declaration.US_EntryDateElectionCode = "~";
			AssertHasMessageError(declaration.US_EntryDateElectionCodeInfo, FormalImportAddInfoJobDeclarationValidation.EntryDateElectionCodeShouldBeInList);
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.ArrivalDate;
			AssertNoMessageError(declaration.US_EntryDateElectionCodeInfo, FormalImportAddInfoJobDeclarationValidation.EntryDateElectionCodeShouldBeInList);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableCRL = true;
			declaration.US_EntryDateElectionCode = "~";
			AssertNoMessageErrorContaining(declaration.US_EntryDateElectionCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(declaration.US_EntryDateElectionCodeInfo, FormalImportAddInfoJobDeclarationValidation.EntryDateElectionCodeShouldBeInList);
			AssertHasMessageErrorContaining(declaration.US_EntryDateElectionCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_EntryDateElectionCode = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_EntryDateElectionCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(declaration.US_EntryDateElectionCodeInfo, FormalImportAddInfoJobDeclarationValidation.EntryDateElectionCodeShouldBeInList);
			AssertNoMessageErrorContaining(declaration.US_EntryDateElectionCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertNoMessageErrorContaining(declaration.US_EntryDateElectionCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_CertifyCargoRelease()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_PreparerDistrictPort = "3901";
			declaration.US_EntryType = "";
			declaration.US_EnableCRL = false;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_CertifyCargoRelease = false;
			AssertHasMessageError(declaration.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			declaration.US_CertifyCargoRelease = true;
			AssertNoMessageError(declaration.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF);
			declaration.US_EnableENS = false;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			declaration.US_CertifyCargoRelease = true;
			AssertHasMessageError(declaration.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.ShouldNotCertifyForExWarehouse);
			declaration.US_CertifyCargoRelease = false;
			AssertNoMessageError(declaration.US_CertifyCargoReleaseInfo, FormalImportAddInfoJobDeclarationValidation.ShouldNotCertifyForExWarehouse);
		}

		public void TestUS_CertifyCargoReleaseForRLFEntriesPost28Feb()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = false;
			Assert(!declaration.US_CertifyCargoReleaseInfo.HasMessageError(FormalImportAddInfoJobDeclarationValidation.MustCertifyForRLF));
		}

		public void TestCheckUS_ADDCVDSuretyCode()
		{
			declaration.US_ADDCVDSuretyCode = "7A9";
			AssertHasMessageError(declaration.US_ADDCVDSuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_ADDCVDSuretyCode = "789";
			AssertNoMessageError(declaration.US_ADDCVDSuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_ADDCVDSuretyCode = "";
			AssertNoMessageError(declaration.US_ADDCVDSuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
		}

		public void TestUS_SuretyCode()
		{
			AssertUS_SuretyCode(declaration);
		}

		public void TestUS_SuretyCodeForBorderCargoRelease()
		{
			declaration.ValidationModes = ValidationModes.CargoRelease;
			AssertUS_SuretyCode(declaration);
		}

		public void TestCheckUS_IsHMFApplicable()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_IsHMFApplicable = "K";
			AssertHasMessageErrorContaining(declaration.US_IsHMFApplicableInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(declaration.US_IsHMFApplicableInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_IsHMFApplicable = "";
			AssertHasMessageErrorContaining(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForSea);
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForSea);
			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			AssertHasMessageError(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForBWBMOT);
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForBWBMOT);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.NAFTADutyDeferral;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			AssertHasMessageError(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForSeaOnlyWhenNotCertainEntryType + "NAFTA Duty Deferral");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertNoMessageError(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForSeaOnlyWhenNotCertainEntryType + "NAFTA Duty Deferral");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			AssertHasMessageError(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForSeaOnlyWhenNotCertainEntryType + "Consumption Foreign Trade Zone (FTZ)");
		}

		public void TestCheckUS_LiveEntryIndicator()
		{
			declaration.US_LiveEntryIndicator = "!";
			AssertHasMessageError(declaration.US_LiveEntryIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.LiveEntryIndicShouldBeInList);
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(declaration.US_LiveEntryIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.LiveEntryIndicShouldBeInList);
			var entryTypeList = new EntryTypeList();
			for (int i = 0; i < entryTypeList.Count; i++)
			{
				if (entryTypeList[i].Code != EntryTypeList.Codes.ConsumptionFTZ && EntryTypeList.IsQuotaVisa(entryTypeList[i].Code))
				{
					AssertLiveEntryIndicatorInfoWhenEntryTypeIsQuotaVisa(entryTypeList[i].Code);
				}
			}
		}

		public void AssertLiveEntryIndicatorInfoWhenEntryTypeIsQuotaVisa(string entryType)
		{
			declaration.US_EntryType = entryType;
			Assert(declaration.US_LiveEntryIndicator.IsEmpty);
			AssertHasMessageError(declaration.US_LiveEntryIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.LiveEntryIndicatorShouldSelectedWhenEntryTypeIsQuotaVisa);
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(declaration.US_LiveEntryIndicatorInfo, FormalImportAddInfoJobDeclarationValidation.LiveEntryIndicatorShouldSelectedWhenEntryTypeIsQuotaVisa);
		}

		public void TestCheckUS_SchDArrival_APA()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertHasMessageError(declaration.US_SchDArrivalInfo, ValidationConstants.PriorNotice.PortOfArrival);
			declaration.US_SchDArrival = "2904";
			AssertNoMessageError(declaration.US_SchDArrivalInfo, ValidationConstants.PriorNotice.PortOfArrival);
		}

		public void TestCheckUS_FDAATA()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			line.FDAs.AddNew().US_PND = false;
			declaration.AddInfoValidation.ValidateUS_FDAADTA();
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateTimeOfArrivalMandatory);
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(-11);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateTimeOfArrivalMandatory);
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(-9);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(5);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			AssertHasWarning(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalEntry);
			declaration.US_FDAADTA = ZDateTime.Today;
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			AssertNoWarning(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalEntry);
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			declaration.US_FDAADTA = ZDate.Today.AddHours(1);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
		}

		public void TestCheckUS_FDAContactName()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertNoMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FDA.Contact);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			FDA fdaLine1 = invoiceLine.FDAs.AddNew();
			declaration.AddInfoValidation.ValidateUS_FDAContactName();
			AssertHasMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FDA.Contact);
			declaration.US_FDAContactName = "John";
			AssertNoMessageError(declaration.US_FDAContactNameInfo, ValidationConstants.FDA.Contact);
		}

		public void TestCheckUS_FDAContactPhoneNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			FDA fdaLine1 = invoiceLine.FDAs.AddNew();
			declaration.AddInfoValidation.ValidateUS_FDAContactPhoneNo();
			AssertHasMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			declaration.US_FDAContactPhoneNo = "+1 82 9384";
			AssertHasMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			declaration.US_FDAContactPhoneNo = "1829384";
			AssertHasMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			declaration.US_FDAContactPhoneNo = "8293845001";
			AssertNoMessageError(declaration.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
		}

		public void TestCheckUS_FDAContactEmail()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			FDA fdaLine1 = invoiceLine.FDAs.AddNew();
			declaration.US_FDAContactEmail = "r";
			AssertHasWarning(declaration.US_FDAContactEmailInfo, "Invalid email format");
			declaration.US_FDAContactEmail = "johan.anderson@tpg.com";
			AssertNoWarning(declaration.US_FDAContactEmailInfo, "Invalid email format");
		}

		public void TestCheckUS_FDAContactEmailPostFinalRule()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			FDA fdaLine1 = invoiceLine.FDAs.AddNew();
			declaration.AddInfoValidation.ValidateUS_FDAContactEmail();
			AssertHasMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FDA.ContactEmail);
			declaration.US_FDAContactEmail = "william.smith@somelongcompanyname.org";
			AssertNoMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FDA.ContactEmail);
			declaration.US_FDAContactEmail = "bill.smith@company.org";
			AssertNoMessageError(declaration.US_FDAContactEmailInfo, ValidationConstants.FDA.ContactEmail);
			declaration.US_FDAContactEmail = "r";
			AssertHasWarning(declaration.US_FDAContactEmailInfo, "Invalid email format");
			declaration.US_FDAContactEmail = "johan.anderson@tpg.com";
			AssertNoWarning(declaration.US_FDAContactEmailInfo, "Invalid email format");
		}

		public void TestCheckUS_GeneralOrderNo()
		{
			declaration.AddInfoValidation.ValidateUS_GeneralOrderNo();
			AssertNoMessageErrors(declaration.US_GeneralOrderNoInfo);
			declaration.US_EntryDate = ZDateTime.Invalid;
			declaration.AddInfoValidation.ValidateUS_GeneralOrderNo();
			AssertNoWarning(declaration.US_GeneralOrderNoInfo, FormalImportAddInfoJobDeclarationValidation.GONoRequired);
			ErrorReporter.Clear();
			declaration.US_EntryDate = ZDateTime.Today.AddDays(-20);
			declaration.AddInfoValidation.ValidateUS_GeneralOrderNo();
			AssertHasWarning(declaration.US_GeneralOrderNoInfo, FormalImportAddInfoJobDeclarationValidation.GONoRequired);
			declaration.US_GeneralOrderNo = "1111";
			declaration.AddInfoValidation.ValidateUS_GeneralOrderNo();
			AssertNoWarning(declaration.US_GeneralOrderNoInfo, FormalImportAddInfoJobDeclarationValidation.GONoRequired);
			AssertHasWarning(declaration.US_GeneralOrderNoInfo, FormalImportAddInfoJobDeclarationValidation.GONoInvalidFormat);
			declaration.US_GeneralOrderNo = "123456789012";
			declaration.AddInfoValidation.ValidateUS_GeneralOrderNo();
			AssertNoWarning(declaration.US_GeneralOrderNoInfo, FormalImportAddInfoJobDeclarationValidation.GONoInvalidFormat);
			declaration.US_GeneralOrderNo = "2012270490001";
			declaration.AddInfoValidation.ValidateUS_GeneralOrderNo();
			AssertNoWarning("13 digit GO Number should be allowed as well.", declaration.US_GeneralOrderNoInfo, FormalImportAddInfoJobDeclarationValidation.GONoInvalidFormat);
			declaration.US_EntryDate = ZDateTime.Today.AddDays(-20);
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-10);
			declaration.US_GeneralOrderNo = "";
			declaration.AddInfoValidation.ValidateUS_GeneralOrderNo();
			AssertNoWarning(declaration.US_GeneralOrderNoInfo, FormalImportAddInfoJobDeclarationValidation.GONoRequired);
		}

		public void TestCheckUS_EntryFilerCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			AssertEquals("PreCondition", true, declaration.IsImportByExternalBroker);
			declaration.DecEntryNumber = "123456";
			declaration.US_EntryFilerCode = "";
			AssertHasError(declaration.US_EntryFilerCodeInfo, FormalImportAddInfoJobDeclarationValidation.EntryNumberEnteredWithoutEntryFilerCode);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.US_EntryFilerCode = "XJ5";
			AssertNoError(declaration.US_EntryFilerCodeInfo, FormalImportAddInfoJobDeclarationValidation.EntryNumberEnteredWithoutEntryFilerCode);
			AssertHasError(declaration.US_EntryFilerCodeInfo, FormalImportAddInfoJobDeclarationValidation.EntryFilerCodeSameAsCurrentCompany);
			declaration.US_EntryFilerCode = "XJ4";
			AssertNoError(declaration.US_EntryFilerCodeInfo, FormalImportAddInfoJobDeclarationValidation.EntryFilerCodeSameAsCurrentCompany);
		}

		public void TestCheckUS_FDACANType()
		{
			declaration.US_UI_NKCarrierSCAC = "";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			declaration.AddInfoValidation.ValidateUS_FDACANType();
			AssertHasMessageError(declaration.US_FDACANTypeInfo, ValidationConstants.PriorNotice.CarrierType);
			declaration.US_UI_NKCarrierSCAC = "APLU";
			AssertNoMessageError(declaration.US_SchDArrivalInfo, ValidationConstants.PriorNotice.CarrierType);
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_FDACANType = "B";
			AssertHasMessageError(declaration.US_FDACANTypeInfo, "The code you have selected is not in the list.");
			declaration.US_FDACANType = FDACarrierTypeList.Codes.Carrier;
			AssertNoMessageError(declaration.US_FDACANTypeInfo, "The code you have selected is not in the list.");
			declaration.ValidationModes = ValidationModes.StandAlonePriorNotice;
			declaration.US_FDACANType = "B";
			AssertNoMessageError(declaration.US_FDACANTypeInfo, "The code you have selected is not in the list.");
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_FDACANType = "";
			AssertNoMessageError(declaration.US_SchDArrivalInfo, ValidationConstants.PriorNotice.CarrierType);
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_FDACANType = "";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertNoMessageError(declaration.US_FDACANTypeInfo, ValidationConstants.PriorNotice.CarrierType);
			declaration.ValidationModes = ValidationModes.None;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Assert("Precondition: is ConsumptionFTZ", declaration.IsConsumptionFTZ);
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_FDACANType = "";
			AssertNoMessageError(declaration.US_FDACANTypeInfo, ValidationConstants.PriorNotice.CarrierType);
		}

		public void TestCheckUS_FDACAN()
		{
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_FDACANType = FDACarrierTypeList.Codes.Carrier;
			declaration.US_FDACAN = "";
			AssertNoMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.CarrierName);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			declaration.US_FDACAN = "";
			AssertHasMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.CarrierName);
			declaration.US_FDACAN = "AD";
			AssertNoMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.CarrierName);
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle;
			AssertNoMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.PrivatelyOwnedUSVehicleLicense);
			declaration.US_FDACAN = "";
			AssertHasMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.PrivatelyOwnedUSVehicleLicense);
			declaration.US_FDACAN = "AD";
			AssertNoMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.PrivatelyOwnedUSVehicleLicense);
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle;
			AssertNoMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.PrivatelyOwnedForeignVehicleStateProvinceName);
			declaration.US_FDACAN = "";
			AssertHasMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.PrivatelyOwnedForeignVehicleStateProvinceName);
			declaration.US_FDACAN = "AD";
			AssertNoMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.PrivatelyOwnedForeignVehicleStateProvinceName);
			declaration.ValidationModes = ValidationModes.StandAlonePriorNotice;
			declaration.US_FDACAN = "";
			declaration.US_FDACANType = FDACarrierTypeList.Codes.Carrier;
			AssertNoMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.CarrierName);
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle;
			AssertNoMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.PrivatelyOwnedUSVehicleLicense);
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle;
			AssertNoMessageError(declaration.US_FDACANInfo, ValidationConstants.PriorNotice.PrivatelyOwnedForeignVehicleStateProvinceName);
		}

		public void TestCheckUS_FDACCN()
		{
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_FDACANType = FDACarrierTypeList.Codes.Carrier;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			declaration.AddInfoValidation.ValidateUS_FDACCN();
			AssertHasMessageError(declaration.US_FDACCNInfo, ValidationConstants.PriorNotice.CarrierCountry);
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle;
			declaration.AddInfoValidation.ValidateUS_FDACCN();
			AssertNoMessageError(declaration.US_FDACCNInfo, ValidationConstants.PriorNotice.CarrierCountry);
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle;
			declaration.AddInfoValidation.ValidateUS_FDACCN();
			AssertHasMessageError(declaration.US_FDACCNInfo, ValidationConstants.PriorNotice.PrivatelyOwnedUSVehicleStateCode);
			declaration.US_FDACCN = USStatesList.Codes.California;
			AssertNoMessageError(declaration.US_FDACCNInfo, ValidationConstants.PriorNotice.PrivatelyOwnedUSVehicleStateCode);
			declaration.US_FDACCN = "AA";
			AssertHasMessageError("List validation against US State codes", declaration.US_FDACCNInfo, "The code you have selected is not in the list.");
			declaration.US_FDACCN = USStatesList.Codes.Florida;
			AssertNoMessageError(declaration.US_FDACCNInfo, "The code you have selected is not in the list.");
			declaration.US_FDACANType = FDACarrierTypeList.Codes.Carrier;
			declaration.US_FDACCN = "AA";
			AssertHasMessageError("List validation now against Country Codes", declaration.US_FDACCNInfo, "The code you have selected is not in the list.");
			declaration.US_FDACCN = "FR"; //France
			AssertNoMessageError(declaration.US_FDACCNInfo, "The code you have selected is not in the list.");
			declaration.ValidationModes = ValidationModes.StandAlonePriorNotice;
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_FDACANType = FDACarrierTypeList.Codes.Carrier;
			AssertNoMessageError(declaration.US_FDACCNInfo, ValidationConstants.PriorNotice.CarrierCountry);
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle;
			AssertNoMessageError(declaration.US_FDACCNInfo, ValidationConstants.PriorNotice.PrivatelyOwnedUSVehicleStateCode);
			declaration.US_FDACCN = "AA";
			AssertNoMessageError("List validation against US State codes", declaration.US_FDACCNInfo, "The code you have selected is not in the list.");
			declaration.US_FDACANType = FDACarrierTypeList.Codes.Carrier;
			declaration.US_FDACCN = "AA";
			AssertNoMessageError("List validation now against Country Codes", declaration.US_FDACCNInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckUS_FDACCNPostFinalRuleImplementation()
		{
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_FDACANType = FDACarrierTypeList.Codes.Carrier;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			declaration.AddInfoValidation.ValidateUS_FDACCN();
			AssertHasMessageError(declaration.US_FDACCNInfo, ValidationConstants.PriorNotice.CarrierCountry);
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle;
			declaration.AddInfoValidation.ValidateUS_FDACCN();
			AssertNoMessageError(declaration.US_FDACCNInfo, ValidationConstants.PriorNotice.CarrierCountry);
		}

		public void TestCheckUS_DestinationState()
		{
			declaration.US_DestinationState = "~";
			AssertHasMessageError(declaration.US_DestinationStateInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);
			declaration.US_DestinationState = USStatesList.Codes.Alabama;
			AssertNoMessageError(declaration.US_DestinationStateInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}

		public void TestCheckUS_IsFinalWHS()
		{
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_QtyInWHBeforeWithdrawal = 10m;
			declaration.US_QtyBeingWithdrawn = 5m;
			AssertEquals("Pre-condition", 5m, declaration.US_QtyInWHAfterWithdrawal);
			AssertNoWarning(declaration.US_IsFinalWHSInfo, FormalImportAddInfoJobDeclarationValidation.FinalWithdrawalRequired);
			declaration.US_QtyBeingWithdrawn = 10m;
			AssertEquals("Pre-condition", 0m, declaration.US_QtyInWHAfterWithdrawal);
			AssertHasWarning(declaration.US_IsFinalWHSInfo, FormalImportAddInfoJobDeclarationValidation.FinalWithdrawalRequired);
			declaration.US_IsFinalWHS = true;
			AssertNoWarning(declaration.US_IsFinalWHSInfo, FormalImportAddInfoJobDeclarationValidation.FinalWithdrawalRequired);
		}

		public void TestCheckUS_QtyBeingWithdrawn()
		{
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_QtyInWHBeforeWithdrawal = 10m;
			declaration.US_QtyBeingWithdrawn = 5m;
			AssertNoWarning(declaration.US_QtyBeingWithdrawnInfo, FormalImportAddInfoJobDeclarationValidation.WithdrawalConflict);
			declaration.US_QtyBeingWithdrawn = 15m;
			AssertHasWarning(declaration.US_QtyBeingWithdrawnInfo, FormalImportAddInfoJobDeclarationValidation.WithdrawalConflict);
		}

		public void TestCheckUS_QtyInWHAfterWithdrawal()
		{
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_IsFinalWHS = true;
			declaration.US_QtyInWHBeforeWithdrawal = 10m;
			declaration.US_QtyBeingWithdrawn = 5m;
			AssertEquals("Pre-condition", 5m, declaration.US_QtyInWHAfterWithdrawal);
			AssertHasWarning(declaration.US_QtyInWHAfterWithdrawalInfo, FormalImportAddInfoJobDeclarationValidation.FinalWithdrawalConflict);
			declaration.US_QtyBeingWithdrawn = 10m;
			AssertNoWarning(declaration.US_QtyInWHAfterWithdrawalInfo, FormalImportAddInfoJobDeclarationValidation.FinalWithdrawalConflict);
			declaration.US_IsFinalWHS = false;
			declaration.US_QtyInWHBeforeWithdrawal = 50m;
			declaration.US_QtyBeingWithdrawn = 30m;
			declaration.US_QtyInWHAfterWithdrawal = 30m;
			AssertHasWarning(declaration.US_QtyInWHAfterWithdrawalInfo, FormalImportAddInfoJobDeclarationValidation.BalanceError);
			declaration.US_QtyBeingWithdrawn = 13m; // will auto calculate correct balance in Qty after w/d again
			AssertNoWarning(declaration.US_QtyInWHAfterWithdrawalInfo, FormalImportAddInfoJobDeclarationValidation.BalanceError);
		}

		public void TestCheckUS_DES()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "A001", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "125H", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var list1 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode, "1245");
			Factory.Save();

			declaration.US_DES = "~";
			AssertHasMessageError(declaration.US_DESInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_DES = "A001";
			AssertNoMessageError(declaration.US_DESInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_DES = "125H";
			AssertNoWarning(declaration.US_DESInfo, FormalImportAddInfoJobDeclarationValidation.DesExamSiteNotOnTheSameDistrict);
			declaration.US_SchDExam = "1246";
			declaration.AddInfoValidation.ValidateUS_DES();
			AssertHasWarning(declaration.US_DESInfo, FormalImportAddInfoJobDeclarationValidation.DesExamSiteNotOnTheSameDistrict);
			declaration.US_SchDExam = "1245";
			declaration.AddInfoValidation.ValidateUS_DES();
			AssertNoWarning(declaration.US_DESInfo, FormalImportAddInfoJobDeclarationValidation.DesExamSiteNotOnTheSameDistrict);
		}

		public void TestCheckUS_CargoReleaseType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2902", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_CargoReleaseType = "";
			AssertNoError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory);
			AssertNoMessageErrorContaining(declaration.US_CargoReleaseTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "";
			AssertHasError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory);
			declaration.US_CargoReleaseType = "~";
			AssertNoError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory);
			AssertHasMessageErrorContaining(declaration.US_CargoReleaseTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			AssertNoMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BRCIsInvalid);
			declaration.US_ITDate = ZDateTime.Today;
			declaration.AddInfoValidation.ValidateUS_CargoReleaseType();
			AssertHasMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BRCIsInvalid);
			declaration.US_ITDate = ZDateTime.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.AddInfoValidation.ValidateUS_CargoReleaseType();
			AssertHasMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BRCIsInvalid);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			AssertNoMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BRCIsInvalid);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertHasMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BRCIsInvalid);
			var coll = new BorderCargoPortCollection();
			var port = coll.AddNew();
			port.PortCode = "2902";
			port.CRProcess = CRProcessList.Codes.OneStep;
			port.Location = "S";
			DataRegistry.Business.USCustomsDataRegistry.Instance.BorderCargoReleasePorts.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, coll);
			declaration.US_SchDEntry = "3901";
			declaration.AddInfoValidation.ValidateUS_CargoReleaseType();
			AssertHasMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BRCIsInvalidForPortOfEntry);
			coll = new BorderCargoPortCollection();
			port = coll.AddNew();
			port.PortCode = "3901";
			port.CRProcess = CRProcessList.Codes.OneStep;
			port.Location = "S";
			DataRegistry.Business.USCustomsDataRegistry.Instance.BorderCargoReleasePorts.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, coll);
			declaration.AddInfoValidation.ValidateUS_CargoReleaseType();
			AssertNoMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BRCIsInvalidForPortOfEntry);
			Factory.Save();
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			var errorText = "Cargo Release Type cannot be changed ";
			AssertNoError(declaration.US_CargoReleaseTypeInfo, errorText);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			var crlEntry = declaration.CustomsEntryHeaders.AddNew();
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			crlEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseOriginal;
			Factory.Save();
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			AssertHasErrorContaining(declaration.US_CargoReleaseTypeInfo, errorText);
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			Factory.Save();
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			AssertHasErrorContaining(declaration.US_CargoReleaseTypeInfo, errorText);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			AssertHasWarning(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BCRMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertNoWarning(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BCRMode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_CargoReleaseType = "";
			AssertHasError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertNoError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_CargoReleaseType = "";
			AssertHasError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertNoError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory);
		}

		public void TestACSCargoReleaseNotAcceptable()
		{
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertHasMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.UnacceptableCargoReleaseType);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertNoMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.UnacceptableCargoReleaseType);
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalQuota;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertNoMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.UnacceptableCargoReleaseType);
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = ZString.Empty;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var formalEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertHasMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.UnacceptableCargoReleaseType);
			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertNoMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.UnacceptableCargoReleaseType);
			formalEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertNoMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.UnacceptableCargoReleaseType);
		}

		public void TestACSCargoReleaseNotAcceptableAfterAdditionalACEEntryTypesEffectiveDate()
		{
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertHasMessageError(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.UnacceptableCargoReleaseType);
		}

		public void TestUS_CargoReleaseTypeForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_CargoReleaseType = ZString.Empty;
			Assert("no messaging is enabled yet. No error should be issued", !declaration.US_CargoReleaseTypeInfo.HasError(FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory));
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = ZString.Empty;
			AssertHasError("no messaging is enabled yet. No error should be issued", declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory);
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			AssertNoError("no messaging is enabled yet. No error should be issued", declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.CRLTypeIsMandatory);
		}

		public void TestUS_EntryDate()
		{
			declaration.AddInfoValidation.ValidateUS_EntryDate();
			AssertHasMessageErrorContaining(declaration.US_EntryDateInfo, ValidationConstants.Declaration.DateAtEntryPortRequired);
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EntryDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(declaration.US_EntryDateInfo, ValidationConstants.Declaration.DateAtEntryPortRequired);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.AddInfoValidation.ValidateUS_EntryDate();
			AssertNoMessageErrorContaining(declaration.US_EntryDateInfo, ValidationConstants.Declaration.DateAtEntryPortRequired);

			var today = ZDateTime.Today;
			declaration.JE_DateOfArrival = today.AddDays(1);
			declaration.US_EntryDate = today;
			AssertHasMessageError(declaration.US_EntryDateInfo, FormalImportAddInfoJobDeclarationValidation.DateAtEntryMustGreaterThanDateArrival);

			declaration.JE_DateOfArrival = today.AddHours(3);
			declaration.US_EntryDate = today;
			AssertNoMessageErrorContaining(declaration.US_EntryDateInfo, FormalImportAddInfoJobDeclarationValidation.DateAtEntryMustGreaterThanDateArrival);

			declaration.JE_DateOfArrival = today.AddDays(-1);
			declaration.AddInfoValidation.ValidateUS_EntryDate();
			AssertNoMessageErrorContaining(declaration.US_EntryDateInfo, FormalImportAddInfoJobDeclarationValidation.DateAtEntryMustGreaterThanDateArrival);

			var bill = declaration.Bills.AddNew();
			bill.US_SESplitShip = true;
			declaration.US_EntryDate = ZDateTime.Empty;
			declaration.AddInfoValidation.ValidateUS_EntryDate();
			AssertHasMessageErrorContaining(declaration.US_EntryDateInfo, ValidationConstants.Declaration.DateAtEntryPortRequired);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			declaration.AddInfoValidation.ValidateUS_EntryDate();
			AssertNoMessageError("This is legacy cargo release", declaration.US_EntryDateInfo, ValidationConstants.Declaration.DateAtEntryPortRequired);
		}

		public void TestUS_EntryDateLimits()
		{
			declaration.ValidationModes = ValidationModes.CargoRelease;
			declaration.US_EntryDate = ZDateTime.Today.AddDays(-100);
			AssertHasMessageErrorContaining(declaration.US_EntryDateInfo, FormalImportAddInfoJobDeclarationValidation.DateAtEntryPortPastLimit);
			declaration.US_EntryDate = ZDateTime.Today.AddDays(-89);
			AssertNoMessageErrorContaining(declaration.US_EntryDateInfo, FormalImportAddInfoJobDeclarationValidation.DateAtEntryPortPastLimit);
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.US_EntryDate = ZDateTime.Today.AddDays(-100);
			AssertNoMessageErrorContaining(declaration.US_EntryDateInfo, FormalImportAddInfoJobDeclarationValidation.DateAtEntryPortPastLimit);
			declaration.US_EntryDate = ZDateTime.Today.AddDays(65);
			AssertNoMessageErrorContaining(declaration.US_EntryDateInfo, FormalImportAddInfoJobDeclarationValidation.DateAtEntryPortFutureLimit);
			declaration.ValidationModes = ValidationModes.CargoRelease;
			declaration.AddInfoValidation.ValidateUS_EntryDate();
			AssertHasMessageErrorContaining(declaration.US_EntryDateInfo, FormalImportAddInfoJobDeclarationValidation.DateAtEntryPortFutureLimit);
			declaration.US_EntryDate = ZDateTime.Today.AddDays(59);
			AssertNoMessageErrorContaining(declaration.US_EntryDateInfo, FormalImportAddInfoJobDeclarationValidation.DateAtEntryPortFutureLimit);
		}

		public void TestWhenDataIsEnteredOnDifferentFactory()
		{
			var factory2 = new BusinessObjectFactory();
			var submitter = factory2.NewWithValidTestData<OrgHeader>();
			var accessed = submitter.CountryData; //This happens when a form is saved and each plugin is touched
			factory2.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine.FDAs.AddNew();
			declaration.JE_OH_FDASubmitter = submitter.PK;
			AssertHasMessageError(declaration.JE_OH_FDASubmitterInfo, ValidationConstants.PriorNotice.SubmitterFirmType);
			var wrapper = OrgHeaderWrapper.New(submitter); //using factory2
			wrapper.ZO_SubmitterFirmType = SubmitterFirmTypeList.Codes.F;
			factory2.Save();
			declaration.JE_OH_FDASubmitter = submitter.PK;
			AssertNoMessageError(declaration.JE_OH_FDASubmitterInfo, ValidationConstants.PriorNotice.SubmitterFirmType);
		}

		public void TestCheckUS_TIBPurpose()
		{
			declaration.AddInfoValidation.ValidateUS_TIBPurpose();
			AssertNoWarningContaining(declaration.US_TIBPurposeInfo, FormalImportAddInfoJobDeclarationValidation.TIBPurposeRequired);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.AddInfoValidation.ValidateUS_TIBPurpose();
			AssertHasWarningContaining(declaration.US_TIBPurposeInfo, FormalImportAddInfoJobDeclarationValidation.TIBPurposeRequired);
		}

		public void TestValidationSuppressedWhenPSC()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";
			Factory.Save();
			declaration.US_PSC = true;
			declaration.US_PaymentType = "~";
			AssertHasMessageErrorContaining(declaration.US_PaymentTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_PaymentType = "";
			AssertNoMessageErrors(declaration.US_PaymentTypeInfo);
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_TaxRate = 0.1m;
			invoiceLine.JI_CustomsQuantity = 10m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			Assert(declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEstimatedTax > 0);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertNoMessageErrors(declaration.US_PaymentTypeInfo);
			declaration.US_PeriodicStatementMM = ZString.Empty;
			AssertNoMessageErrors(declaration.US_PeriodicStatementMMInfo);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			AssertNoMessageErrors(declaration.US_PreliminaryStatementPrintDateInfo);
		}

		[TestDate(2013, 12, 09)]
		public void TestUS_DeferredTaxDueDate()
		{
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.US_EstimatedEntryDate = new ZDateTime(2013, 12, 02);
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			declaration.US_DeferredTaxDueDate = ZDateTime.Empty;
			AssertNoMessageError(declaration.US_DeferredTaxDueDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
			AssertNoMessageError(declaration.US_DeferredTaxDueDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			declaration.US_DeferredTaxDueDate = ZDateTime.Today;
			AssertNoMessageError(declaration.US_DeferredTaxDueDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
			AssertNoMessageError(declaration.US_DeferredTaxDueDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			declaration.US_DeferredTaxDueDate = new ZDateTime(2013, 12, 14);
			AssertNoMessageError(declaration.US_DeferredTaxDueDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
			AssertHasMessageError(declaration.US_DeferredTaxDueDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			declaration.US_EstimatedEntryDate = new ZDateTime(2014, 01, 01);
			declaration.US_DeferredTaxDueDate = new ZDateTime(2014, 01, 01);
			AssertHasMessageError(declaration.US_DeferredTaxDueDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
			AssertNoMessageError(declaration.US_DeferredTaxDueDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			var calculatedDate = new AddInfoJobDeclarationWorkingDate().GenerateDeferredTaxDueDate(declaration);
			var notificationText = string.Format(FormalImportAddInfoJobDeclarationValidation.DeferredTaxDueDateNotDefaulted, calculatedDate.ToShortDateString());
			AssertHasWarning(declaration.US_DeferredTaxDueDateInfo, notificationText);
			declaration.US_DeferredTaxDueDate = calculatedDate;
			AssertNoWarning(declaration.US_DeferredTaxDueDateInfo, notificationText);
		}

		public void TestCheckUS_FixDefTaxDueDate()
		{
			AssertNoErrors(declaration.US_FixPSDInfo);
			declaration.US_FixDefTaxDueDate = true;
			AssertHasError(declaration.US_FixDefTaxDueDateInfo, FormalImportAddInfoJobDeclarationValidation.FixDefTaxDueDate);
			declaration.US_DeferredTaxDueDate = ZDateTime.Today.AddDays(1);
			AssertNoError(declaration.US_FixDefTaxDueDateInfo, FormalImportAddInfoJobDeclarationValidation.FixDefTaxDueDate);
		}

		public void TestCheckUS_EstEnteredValue()
		{
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.AddInfoValidation.ValidateUS_EstEnteredValue();
			AssertHasMessageErrorContaining(declaration.US_EstEnteredValueInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EstEnteredValue = 120m;
			AssertNoMessageErrorContaining(declaration.US_EstEnteredValueInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EnableENS = true;
			declaration.US_EstEnteredValue = ZDecimal.Zero;
			AssertNoMessageErrorContaining(declaration.US_EstEnteredValueInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EnableENS = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.AddInfoValidation.ValidateUS_EstEnteredValue();
			AssertNoMessageErrorContaining(declaration.US_EstEnteredValueInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_EnableSPN()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableSPN = true;
			AssertHasMessageErrorContaining(declaration.US_EnableSPNInfo, ValidationConstants.PriorNotice.ACSPriorNoticeTurnedOff);
		}

		public void TestCheckJE_OH_IOR_POADateExpired()
		{
			var a2av = new AuthorityToActValidator();
			var ior = Factory.New<OrgHeader>();
			string pOAWillExpireSoon = Enterprise.Customs.Business.AuthorityToActValidator.GetPOAWillExpireSoonString(ZDate.Today.ToString(), "organization (eDocs > Document Tracking)", a2av.CountrySpecificNameForPOA);
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.IOROrgPK = ior.PK;
			AssertHasWarning(declaration.IOROrgPKInfo, Enterprise.Customs.Business.AuthorityToActValidator.GetNoPOADocumentForImporterString(a2av.CountrySpecificNameForPOA));
			AssertNoWarning(declaration.IOROrgPKInfo, pOAWillExpireSoon);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, Enterprise.Customs.Business.AuthorityToActValidator.GetNoPOADocumentForImporterString(a2av.CountrySpecificNameForPOA));
			AssertNoWarning(declaration.IOROrgPKInfo, pOAWillExpireSoon);
			var poaDocument = ior.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poaDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_ValidToDate = ZDateTime.Empty;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			var wrapper = OrgHeaderWrapper.New(ior);
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarning(declaration.IOROrgPKInfo, pOAWillExpireSoon);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarning(declaration.IOROrgPKInfo, pOAWillExpireSoon);
			poaDocument.EQ_ValidToDate = ZDateTime.Today;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, pOAWillExpireSoon);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, pOAWillExpireSoon);
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(60);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarning(declaration.IOROrgPKInfo, pOAWillExpireSoon);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarning(declaration.IOROrgPKInfo, pOAWillExpireSoon);
		}

		public void TestCheckJE_OH_IOR_POAWithAttribute()
		{
			var a2av = new AuthorityToActValidator();
			var ior = Factory.New<OrgHeader>();
			var poaDocument1 = ior.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poaDocument1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDocument1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument1.EQ_DocDescription = "Power of Attorney";
			poaDocument1.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
			poaDocument1.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-1);
			var attribDummy = poaDocument1.Attributes.AddNew();
			attribDummy.D0_AttribName = "DUMMY";
			attribDummy.D0_AttribValue = "WHO";
			var poaDocument2 = ior.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poaDocument2.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDocument2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument2.EQ_DocDescription = "Power of Attorney";
			poaDocument2.EQ_ValidToDate = ZDateTime.Today.AddMonths(6);
			poaDocument2.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-6);
			var attrib1 = poaDocument2.Attributes.AddNew();
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			attrib1.D0_AttribValue = "2710";
			var attrib2 = poaDocument2.Attributes.AddNew();
			attrib2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			attrib2.D0_AttribValue = ImportExportCodeList.Codes.Export;
			var wrapper = OrgHeaderWrapper.New(ior);
			var noPOA = Enterprise.Customs.Business.AuthorityToActValidator.GetPOANotValidForConditions(a2av.CountrySpecificNameForPOA, "organization (eDocs > Document Tracking)", "'2705' Port of Entry and 'IMP' Direction");
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.IOROrgPK = ior.PK;
			declaration.US_SchDEntry = "2705";
			AssertNoWarning(declaration.IOROrgPKInfo, noPOA);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.IOROrgPK = ior.PK;
			AssertNoMessageError(declaration.IOROrgPKInfo, noPOA);
			var attrib3 = poaDocument1.Attributes.AddNew();
			attrib3.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			attrib3.D0_AttribValue = "2711";
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, noPOA);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, noPOA);
			noPOA = Enterprise.Customs.Business.AuthorityToActValidator.GetPOANotValidForConditions(a2av.CountrySpecificNameForPOA, "organization (eDocs > Document Tracking)", "'IMP' Direction and '2705' Port of Entry");
			attrib3.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			attrib3.D0_AttribValue = ImportExportCodeList.Codes.Export;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, noPOA);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, noPOA);
			var attrib4 = poaDocument1.Attributes.AddNew();
			attrib4.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			attrib4.D0_AttribValue = "2705";
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, noPOA);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, noPOA);
			attrib3.D0_AttribValue = ImportExportCodeList.Codes.Import;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarning(declaration.IOROrgPKInfo, noPOA);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(declaration.IOROrgPKInfo, noPOA);

			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_Code = "~CA";
			var attrib5 = poaDocument1.Attributes.AddNew();
			attrib5.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			attrib5.D0_AttribDisplayValue = caCompany.GC_Code;
			noPOA = Enterprise.Customs.Business.AuthorityToActValidator.GetPOANotValidForConditions(a2av.CountrySpecificNameForPOA, "organization (eDocs > Document Tracking)", "'EDI' Company, 'IMP' Direction and '2705' Port of Entry");
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, noPOA);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, noPOA);
			attrib5.D0_AttribDisplayValue = GlbCompany.CurrentCompany.GC_Code;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarning(declaration.IOROrgPKInfo, noPOA);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(declaration.IOROrgPKInfo, noPOA);

			poaDocument2.Delete();
			attrib5.D0_AttribDisplayValue = caCompany.GC_Code;
			noPOA = Enterprise.Customs.Business.AuthorityToActValidator.GetPOANotValidForConditions(a2av.CountrySpecificNameForPOA, "organization (eDocs > Document Tracking)", "'EDI' Company");
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, noPOA);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, noPOA);
			attrib5.D0_AttribDisplayValue = GlbCompany.CurrentCompany.GC_Code;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarning(declaration.IOROrgPKInfo, noPOA);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(declaration.IOROrgPKInfo, noPOA);
		}

		public void TestValidatePOAForIOR()
		{
			var a2av = new AuthorityToActValidator();
			declaration.IOROrgPK = ZGuid.Empty;
			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			string noPOADocument = Enterprise.Customs.Business.AuthorityToActValidator.GetNoPOADocumentForImporterString(a2av.CountrySpecificNameForPOA);
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, noPOADocument);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, noPOADocument);
			var poaDocument = ior.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poaDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-1);
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(1);
			var orgDocumentAttrib = poaDocument.Attributes.AddNew();
			orgDocumentAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			orgDocumentAttrib.D0_AttribDisplayValue = GlbCompany.CurrentCompany.GC_Code;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarning(declaration.IOROrgPKInfo, noPOADocument);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(declaration.IOROrgPKInfo, noPOADocument);
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(+2);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarning(declaration.IOROrgPKInfo, noPOADocument);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(declaration.IOROrgPKInfo, noPOADocument);
			ior.RequiredDocuments.RemoveAll();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, noPOADocument);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, noPOADocument);
			var pofDocument = ior.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorneyForwarding);
			pofDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-1);
			pofDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(-2);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, noPOADocument);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, noPOADocument);
			pofDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(+2);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.IOROrgPKInfo, noPOADocument);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.IOROrgPKInfo, noPOADocument);
		}

		public void TestDBAddInfo()
		{
			Factory.Save();
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.AddInfoValidation.ValidateUS_CargoReleaseType();
			Factory.Save();
			using (((IBusinessObjectInternals)declaration).ResumeValidationForAllDescendantsTemporarily())
			{
				declaration.US_PaymentType = "1";
				declaration.US_ClientBranchDesignation = "AA";
				Factory.Save();
			}

			AssertEquals(0, declaration.GetAddInfo().DbAddInfoExposed.Notifications.Count());
		}

		public void TestCheckUS_EntryType_AddCvdCaseNumbers()
		{
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_CVDCaseNo = "";
			invoiceLine1.US_ADDCaseNo = "";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_CVDCaseNo = "";
			invoiceLine2.US_ADDCaseNo = "";
			declaration.US_EntryType = "XX";
			AssertNoMessageError(declaration.US_EntryTypeInfo, "Entry Type not allowed when no lines have ADD or CVD information");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertHasMessageError(declaration.US_EntryTypeInfo, "Entry Type not allowed when no lines have ADD or CVD information");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertNoMessageError(declaration.US_EntryTypeInfo, "Entry Type not allowed when no lines have ADD or CVD information");
			invoiceLine1.US_CVDCaseNo = "AAA";
			declaration.US_EntryType = "";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertNoMessageError(declaration.US_EntryTypeInfo, "Entry Type not allowed when no lines have ADD or CVD information");
			invoiceLine1.US_CVDCaseNo = "";
			invoiceLine2.US_ADDCaseNo = "AAA";
			declaration.US_EntryType = "";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertNoMessageError(declaration.US_EntryTypeInfo, "Entry Type not allowed when no lines have ADD or CVD information");
		}

		public void TestCheckUS_NAFTAReconIndicator()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "8401100000";
			tariff1.UE_DateFrom = ZDateTime.MinSmallDateTimeValue;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff1.UE_SPICode = "D R P AUBHCACLILJ+JOMAMXSG";
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateFrom = ZDateTime.MinSmallDateTimeValue;
			tariffRule.U1_RuleCode = TariffRuleList.Codes.NoSPIRequired;
			tariffRule.U1_Tariff = "00000000";
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.MinSmallDateTimeValue;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8401100000";
			invoiceLine1.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine1.US_UC_NKCountryOfExport = "MX";
			invoiceLine1.US_SPI = "N/A";
			declaration.US_NAFTAReconIndicator = ZBool.True;
			AssertNoWarning(declaration.US_NAFTAReconIndicatorInfo, "There are no invoice lines with a FTA SPI that warrant FTA Recon.");
			invoiceLine1.US_SPI = "";
			declaration.US_NAFTAReconIndicator = ZBool.True;
			AssertNoWarning(declaration.US_NAFTAReconIndicatorInfo, "There are no invoice lines with a FTA SPI that warrant FTA Recon.");
			invoiceLine1.JI_Tariff = "8401100000";
			invoiceLine1.US_SPI = "MX";
			declaration.US_NAFTAReconIndicator = ZBool.True;
			AssertHasWarning(declaration.US_NAFTAReconIndicatorInfo, "There are no invoice lines with a FTA SPI that warrant FTA Recon.");
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "00000000";
			invoiceLine2.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine2.US_UC_NKCountryOfExport = "MX";
			invoiceLine2.US_SPI = "";
			declaration.US_NAFTAReconIndicator = ZBool.True;
			AssertHasWarning(declaration.US_NAFTAReconIndicatorInfo, "There are no invoice lines with a FTA SPI that warrant FTA Recon.");
			declaration.US_NAFTAReconIndicator = ZBool.False;
			AssertNoWarning(declaration.US_NAFTAReconIndicatorInfo, "There are no invoice lines with a FTA SPI that warrant FTA Recon.");
			invoiceLine1.US_SPI = "";
			declaration.US_NAFTAReconIndicator = ZBool.True;
			AssertNoWarning(declaration.US_NAFTAReconIndicatorInfo, "There are no invoice lines with a FTA SPI that warrant FTA Recon.");
		}

		public void TestCheckUS_UC_NKCountryOfExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_UC_NKCountryOfExport = "-";
			AssertHasWarning(declaration.US_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessage);
			declaration.US_UC_NKCountryOfExport = "CA";
			AssertNoWarning(declaration.US_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.US_EnableCRL = true;
		}

		JobDeclaration declaration;

		OrgHeader organization;
		OrgHeader Organization => organization ?? (organization = Factory.New<OrgHeader>());

		void SetBranchHolidays()
		{
			string commandText = ZString.Format(
				@"INSERT {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})
				VALUES ('{10}', 'New Years Day', 1, 'DAT', '2008-01-01 00:00:00', '', '', '{11}', '{12}')",
				/*0*/GlbHolidaySchema.Constants.TableName,
				/*1*/GlbHolidaySchema.Constants.PK,
				/*2*/GlbHolidaySchema.Constants.GH_HolidayName,
				/*3*/GlbHolidaySchema.Constants.GH_Recurring,
				/*4*/GlbHolidaySchema.Constants.GH_RecurrType,
				/*5*/GlbHolidaySchema.Constants.GH_Date,
				/*6*/GlbHolidaySchema.Constants.GH_RecurrMonth,
				/*7*/GlbHolidaySchema.Constants.GH_RecurrDay,
				/*8*/GlbHolidaySchema.Constants.GH_ParentID,
				/*9*/GlbHolidaySchema.Constants.GH_ParentTableCode,
				/*10*/Guid.NewGuid(),
				/*11*/GlbBranch.CurrentBranch.PK,
				/*12*/GlbBranchSchema.Constants.Prefix);

			using (DbCommand command = Db.Connection.Command(commandText)) // Used for test case set up
			{
				command.ExecuteScalar();
			}
		}

		void AssertForUS_EnableCRL(JobDeclaration declaration)
		{
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertNotNull(entry);
			entry.Messages.AddNew(typeof(EDIMessage));
			AssertEquals("precondition", false, entry.HasTransactionsWithCustoms);
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingCargoReleaseOriginal;
			AssertEquals("precondition", true, entry.HasTransactionsWithCustoms);
			declaration.US_EnableCRL = false;
			AssertHasErrorContaining(declaration.US_EnableCRLInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
			declaration.US_EnableCRL = true;
			AssertNoErrorContaining(declaration.US_EnableCRLInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
			entry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseDelete;
			Assert(entry.HasBeenWithdrawn);
			declaration.US_EnableCRL = false;
			AssertNoErrorContaining(declaration.US_EnableCRLInfo, FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
		}

		void AssertIOROrgPK(JobDeclaration declaration)
		{
			Organization.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			var iorWrapper = OrgHeaderWrapper.New(Organization);
			iorWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;
			Organization.CustomsCodes.RemoveAll();
			declaration.IOROrgPK = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.IOROrgPK = Organization.PK;
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(declaration.IOROrgPKInfo, FormalImportJobDeclarationValidation.IORCustomsRegNoRequired);
			Organization.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123");
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(declaration.IOROrgPKInfo, FormalImportJobDeclarationValidation.IORCustomsRegNoRequired);
			AssertHasMessageError(declaration.IOROrgPKInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			iorWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.Yes;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(declaration.IOROrgPKInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
		}

		void CheckUS_PeriodicStatementMM()
		{
			declaration.ValidationModes = ValidationModes.EntrySummary;
			int currentMonth = ZDateTime.Today.Month;
			AssertNoNotifications(declaration.US_PeriodicStatementMMInfo);
			declaration.US_PeriodicStatementMM = "01";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "02";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "03";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "04";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "05";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "06";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "07";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "08";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "09";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "10";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "11";
			AssertStatementMonthHandledCorrectly(currentMonth);
			declaration.US_PeriodicStatementMM = "12";
			AssertStatementMonthHandledCorrectly(currentMonth);
		}

		void AssertStatementMonthHandledCorrectly(int currentMonth)
		{
			ZDateTime testDate = ZDateTime.Today;
			if (declaration.US_PeriodicStatementMM == "01")
			{
				if (currentMonth == 11 || currentMonth == 12 || currentMonth == 1)
				{
					AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextTwo);
				}
				else
				{
					AssertHasMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextTwo);
				}
			}
			else if (declaration.US_PeriodicStatementMM == "02")
			{
				if (currentMonth == 12 || currentMonth == 1 || currentMonth == 2)
				{
					AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextTwo);
				}
				else
				{
					AssertHasMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextTwo);
				}
			}
			else
			{
				int maxMonth = ZInt.ParseSafe(declaration.US_PeriodicStatementMM, 0);
				int minMonth = maxMonth - 2;
				if (minMonth <= currentMonth && currentMonth <= maxMonth)
				{
					AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextTwo);
				}
				else
				{
					AssertHasMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthOrNextTwo);
				}
			}

			if (ZInt.ParseSafe(declaration.US_PeriodicStatementMM, 0) == currentMonth && testDate.Date.Day > 15)
			{
				AssertHasMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthInvalid);
			}
			else
			{
				AssertNoMessageError(declaration.US_PeriodicStatementMMInfo, PeriodicStatementMMValidator.CurrentMonthInvalid);
			}
		}

		void AssertUS_BondType(JobDeclaration declaration)
		{
			declaration.US_BondType = "~";
			AssertHasMessageError(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_BondType = "";
			AssertHasMessageError(declaration.US_BondTypeInfo, FormalImportAddInfoJobDeclarationValidation.BondTypeRequired);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertNoMessageError(declaration.US_BondTypeInfo, FormalImportAddInfoJobDeclarationValidation.BondTypeRequired);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_BondType = "";
			AssertNoMessageError(declaration.US_BondTypeInfo, FormalImportAddInfoJobDeclarationValidation.BondTypeRequired);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			declaration.US_BondType = "";
			if (declaration.IsACECargoReleaseValidationMode)
			{
				AssertHasMessageError(declaration.US_BondTypeInfo, FormalImportAddInfoJobDeclarationValidation.BondTypeRequired);
			}
			else
			{
				AssertNoMessageError(declaration.US_BondTypeInfo, FormalImportAddInfoJobDeclarationValidation.BondTypeRequired);
			}

			OrgHeader ior = Factory.New<OrgHeader>();
			CusBondDetailCollection coll = new CusBondDetailCollection(ior);
			CusBondDetail bondData = coll.AddNew();
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = new ZDateTime(2007, 1, 1);
			bondData.PW_BondExpiryDate = new ZDateTime(2007, 1, 1);
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			declaration.IOROrgPK = ior.PK;
			declaration.US_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			AssertHasWarning(declaration.US_BondTypeInfo, FormalImportAddInfoJobDeclarationValidation.ContinousBondExistsButNotActive);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(3);
			declaration.US_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			AssertNoWarning(declaration.US_BondTypeInfo, FormalImportAddInfoJobDeclarationValidation.ContinousBondExistsButNotActive);
		}

		void AssertDatesOnHolidaysAndWeekends(ZPropertyInfo dateInfo)
		{
			dateInfo.Value = ZDateTime.Empty;
			AssertNoMessageError(dateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			dateInfo.Value = new ZDateTime(2006, 1, 14); // Saturday
			AssertHasMessageError(dateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			dateInfo.Value = new ZDateTime(2006, 1, 10); // Tuesday
			AssertNoMessageError(dateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			//holidays (2009, 07, 03)
			dateInfo.Value = new ZDateTime(2009, 07, 03); // Friday Independance Day holiday 2009
			AssertHasMessageError(dateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
			dateInfo.Value = new ZDateTime(2009, 07, 06); // following Monday
			AssertNoMessageError(dateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
		}

		void AssertUS_SuretyCode(JobDeclaration declaration)
		{
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SuretyCode = "";
			AssertHasMessageErrors(declaration.US_SuretyCodeInfo);
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_SuretyCode = "";
			AssertHasMessageErrors(declaration.US_SuretyCodeInfo);
			var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMP3232!@");
			if (org == null)
			{
				org = Factory.New<OrgHeader>();
				org.OH_Code = "IMP3232!@";
				org.OH_FullName = "BOB THE BUILDER";
				org.MainAddress.OA_Address1 = "ADDRESS 1";
			}

			declaration.IOROrgPK = org.PK;
			var bondDetail = declaration.IORWrapper.BondDetails.OfType<CusBondDetail>().FirstOrDefault(x => x.PW_SuretyCode == "819");
			if (bondDetail == null)
			{
				bondDetail = declaration.IORWrapper.BondDetails.AddNew();
				bondDetail.PW_SuretyCode = "819";
			}

			bondDetail.PW_ActivityCode = ActivityCodeList.Codes._1a1;
			bondDetail.HasSufficientFund = ZBool.False;
			bondDetail.PW_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SuretyCode = "819";
			AssertHasMessageError(declaration.US_SuretyCodeInfo, FormalImportAddInfoJobDeclarationValidation.BondIsInsufficient);
			bondDetail.HasSufficientFund = ZBool.True;
			declaration.US_SuretyCode = "819";
			AssertNoMessageError(declaration.US_SuretyCodeInfo, FormalImportAddInfoJobDeclarationValidation.BondIsInsufficient);
			declaration.US_SuretyCode = "8Z1";
			AssertHasMessageError(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_SuretyCode = "891";
			AssertNoMessageError(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			AssertNoMessageErrors(declaration.US_SuretyCodeInfo);
			declaration.US_BondType = ZString.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_SuretyCode = "89";
			AssertHasMessageError(declaration.US_SuretyCodeInfo, FormalImportAddInfoJobDeclarationValidation.InformalEntriesWithoutBondTypeDoesntRequireSuretyCode);
			declaration.US_SuretyCode = "";
			AssertNoMessageError(declaration.US_SuretyCodeInfo, FormalImportAddInfoJobDeclarationValidation.InformalEntriesWithoutBondTypeDoesntRequireSuretyCode);
		}

		const string WrongFTZNumber = "FTZ523G";
		const string WrongFTZNumberWith000 = "FTZ000G";
		const string WrongFTZNumberWith001 = "FTZ001G";
		const string WrongFTZNumberWith300 = "FTZ300G";
		const string WrongFTZNumberWith301 = "FTZ301G";
		const string CorrectFTZNumber = "FTZ123G";
		const string UsualTariffForCheckUS_EntryTypeTest = "1003002000";
	}
}
