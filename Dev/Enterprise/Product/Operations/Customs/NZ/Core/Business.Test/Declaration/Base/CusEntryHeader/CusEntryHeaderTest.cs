using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System;
	using CargoWise.Data.Testing;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Accounting.Integration;
	using Enterprise.Customs.Common;
	using Enterprise.Customs.Common.NZ;
	using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
	using NUnit.Framework;
	using static Enterprise.Customs.Business.CusEntryHeader;

	sealed class CusEntryHeaderNonInheritedTest : TestCaseWithFactory
	{
		public void TestHumanReadableName()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_BGMReference = "M00001201";

			AssertEquals("ECI Manifesting M00001201", entryHeader.HumanReadableName);
		}

		[TestDate(2018, 6, 1)]
		public void TestTotalAmountPayableIncludingEntryFeeForImport()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			var declaration = SetupDeclarationWithFuelLeviesTariff();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.DoMerge();
			var entryHeader = declaration.CusEntryHeader;
			CombineAssertions(() =>
			{
				AssertEquals("entryHeader.EntryFeeAmount", 42.81m, entryHeader.EntryFeeAmount);
				AssertEquals("entryHeader.TotalEntryFeeGST", 6.43m, entryHeader.EntryFeeGST);
				AssertEquals("entryHeader.TotalEntryFeeAmount", 49.24m, entryHeader.TotalEntryFeeAmount);
				AssertEquals("entryHeader.TotalAmountPayable", 822.39m, entryHeader.TotalAmountPayable);
				AssertEquals("entryHeader.TotalAmountPayableIncludingEntryFee", 871.63m, entryHeader.TotalAmountPayableIncludingEntryFee);
			});
		}

		[TestDate(2018, 6, 1)]
		public void TestTotalAmountPayableIncludingEntryFeeForExport()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			var declaration = SetupDeclarationWithFuelLeviesTariff();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.DoMerge();
			var entryHeader = declaration.CusEntryHeader;
			CombineAssertions(() =>
			{
				AssertEquals("entryHeader.EntryFeeAmount", 15.6m, entryHeader.EntryFeeAmount);
				AssertEquals("entryHeader.TotalEntryFeeGST", 2.34m, entryHeader.EntryFeeGST);
				AssertEquals("entryHeader.TotalEntryFeeAmount", 17.94m, entryHeader.TotalEntryFeeAmount);
				AssertEquals("entryHeader.TotalAmountPayable", 0m, entryHeader.TotalAmountPayable);
				AssertEquals("entryHeader.TotalAmountPayableIncludingEntryFee", 17.94m, entryHeader.TotalAmountPayableIncludingEntryFee);
			});
		}

		[TestDate(2018, 6, 1)]
		public void TestTotalAmountPayableIncludingEntryFeeForDrawback()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			var declaration = SetupDeclarationWithFuelLeviesTariff();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			declaration.DoMerge();
			var entryHeader = declaration.CusEntryHeader;
			CombineAssertions(() =>
			{
				AssertEquals("entryHeader.EntryFeeAmount", 15.6m, entryHeader.EntryFeeAmount);
				AssertEquals("entryHeader.TotalEntryFeeGST", 2.34m, entryHeader.EntryFeeGST);
				AssertEquals("entryHeader.TotalEntryFeeAmount", 17.94m, entryHeader.TotalEntryFeeAmount);
				AssertEquals("entryHeader.TotalAmountPayable", 0m, entryHeader.TotalAmountPayable);
				AssertEquals("entryHeader.TotalAmountPayableIncludingEntryFee", -17.94m, entryHeader.TotalAmountPayableIncludingEntryFee);
			});
		}

		[TestDate(2018, 6, 1)]
		public void TestTotalAmountPayableIncludingEntryFeeForGSTRateChange()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			var declaration = SetupDeclarationWithFuelLeviesTariff();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.JE_DateOfArrival = new ZDateTime(2019, 12, 01);
			declaration.DoMerge();
			var entryHeader = declaration.CusEntryHeader;

			CombineAssertions(() =>
			{
				AssertEquals("entryHeader.EntryFeeAmount", ZDecimal.Zero, entryHeader.EntryFeeAmount);
				AssertEquals("entryHeader.TotalEntryFeeGST", ZDecimal.Zero, entryHeader.EntryFeeGST);
				AssertEquals("entryHeader.TotalEntryFeeAmount", ZDecimal.Zero, entryHeader.TotalEntryFeeAmount);
				AssertEquals("entryHeader.TotalAmountPayable", ZDecimal.Zero, entryHeader.TotalAmountPayable);
				AssertEquals("entryHeader.TotalAmountPayableIncludingEntryFee", ZDecimal.Zero, entryHeader.TotalAmountPayableIncludingEntryFee);
			});
		}

		[TestDate(2018, 6, 1)]
		public void TestDutyAndFeesOnIPIDeclarationAreZero()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			var declaration = SetupDeclarationWithFuelLeviesTariff();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.DoMerge();
			var entryHeader = declaration.CusEntryHeader;

			CombineAssertions(() =>
			{
				AssertEquals("Total Duty", 0.00m, entryHeader.DutyTotalInMergedLines);
				AssertEquals("GST Total", 0.00m, entryHeader.GSTAmount);
				AssertEquals("Levies", 0.00m, entryHeader.TotalMisc);
				AssertEquals("Entry Fee", 0.00m, entryHeader.TotalEntryFeeAmount);
				AssertEquals("Amount Payable", 0.00m, entryHeader.TotalAmountPayableIncludingEntryFee);
			});
		}

		public void TestLogWhenIsActiveIsChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CusEntryHeader;
			entry.EntryNumber = "234324";
			var activelog = entry.Logs.MostRecentLogByEventTime(Events.SetToActive);
			AssertNull("Precondition: No active log", activelog);
			var inactivelog = entry.Logs.MostRecentLogByEventTime(Events.SetToInactive);
			AssertNull("Precondition: No inactive log", inactivelog);
			AssertEquals(true, entry.CH_IsActive);

			Factory.Save();
			activelog = entry.Logs.MostRecentLogByEventTime(Events.SetToActive);
			AssertNull("No active log should be created", activelog);
			inactivelog = entry.Logs.MostRecentLogByEventTime(Events.SetToInactive);
			AssertNull("No inactive log should be created", inactivelog);

			entry.CH_IsActive = false;
			activelog = entry.Logs.MostRecentLogByEventTime(Events.SetToActive);
			AssertNull("Active log should be created only when saved", activelog);
			inactivelog = entry.Logs.MostRecentLogByEventTime(Events.SetToInactive);
			AssertNull("Inactive log should be created only when saved", inactivelog);

			entry.CH_JE = ZGuid.NewZGuid(); // causes save exception
			AssertExceptionThrown(typeof(ZSaveException), () => Factory.Save());
			activelog = entry.Logs.MostRecentLogByEventTime(Events.SetToActive);
			AssertNull("No active log should be created when save failed", activelog);
			inactivelog = entry.Logs.MostRecentLogByEventTime(Events.SetToInactive);
			AssertNull("No inactive log should be created when save failed", inactivelog);

			entry.CH_JE = declaration.PK;
			Factory.Save();
			activelog = entry.Logs.MostRecentLogByEventTime(Events.SetToActive);
			AssertNull("No active log should be created as it was changed to inactive", activelog);
			inactivelog = entry.Logs.MostRecentLogByEventTime(Events.SetToInactive);
			AssertNotNull("Inactive log should be created when changed to inactive and saved", inactivelog);

			entry.CH_IsActive = true;
			activelog = entry.Logs.MostRecentLogByEventTime(Events.SetToActive);
			AssertNull("Active log should be created only when saved", activelog);
			var inactivelog2 = entry.Logs.MostRecentLogByEventTime(Events.SetToInactive);
			AssertEquals("No new inactive log should be created as it was changed to active", inactivelog, inactivelog2);

			Factory.Save();
			activelog = entry.Logs.MostRecentLogByEventTime(Events.SetToActive);
			AssertNotNull("Active log should be created as it was changed to active and saved", activelog);
			inactivelog2 = entry.Logs.MostRecentLogByEventTime(Events.SetToInactive);
			AssertEquals("No new inactive log should be created as it was changed to active", inactivelog, inactivelog2);

			entry.CH_CustomsDeliveryInstructions = "a"; // cause has change
			entry.CH_IsActive = false; // change to false
			entry.CH_IsActive = true; // change back to true
			Factory.Save();
			var activelog2 = entry.Logs.MostRecentLogByEventTime(Events.SetToActive);
			AssertEquals("No new active log should be created as active status has not been changed since saved", activelog, activelog2);
			inactivelog2 = entry.Logs.MostRecentLogByEventTime(Events.SetToInactive);
			AssertEquals("No new inactive log should be created as it was changed to active", inactivelog, inactivelog2);

			entry.CH_IsActive = false;
			Factory.Save();
			activelog2 = entry.Logs.MostRecentLogByEventTime(Events.SetToActive);
			AssertEquals("No new active log should be created as it was changed to inactive", activelog, activelog2);
			inactivelog2 = entry.Logs.MostRecentLogByEventTime(Events.SetToInactive);
			AssertNotEquals("A new inactive log should be created as it was changed to inactive", inactivelog, inactivelog2);
		}

		public void TestDeleteIfContainsNoValuableDataTSWCompletionProcess()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Sight;
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, declaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(declaration));
			var activeEntryHeader = declaration.CusEntryHeader;
			activeEntryHeader.EntryNumber = "1942482";
			activeEntryHeader.CH_EntryStatus = "IAR";

			var completionDummyEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			completionDummyEntryHeader.PopulateCH_BGMReferenceIfNeeded();
			completionDummyEntryHeader.CH_IsActive = false;

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Completion;
			var completionEntryHeader = declaration.CusEntryHeader;
			completionEntryHeader.Messages.AddNew(typeof(TSWMessage));
			completionEntryHeader.PopulateCH_BGMReferenceIfNeeded();

			AssertEquals("activeEntryHeader should not be removed", false, activeEntryHeader.IsDeleted);
			AssertEquals("completionDummyEntryHeader should be deleted", true, completionDummyEntryHeader.IsDeleted);
			AssertEquals("completionEntryHeader should not be removed", false, completionEntryHeader.IsDeleted);
		}

		public void TestDeleteIfContainsNoValuableDataTSWCompletionProcessLongJobNo()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "BAKLCIA00023839";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Sight;
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, declaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(declaration));
			var orginalEntry = declaration.CusEntryHeader;
			orginalEntry.EntryNumber = "1942482";
			orginalEntry.CH_EntryStatus = "IAR";
			orginalEntry.CH_BGMReference = "SIT000000015";
			Factory.Save();

			var completionDummyEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			completionDummyEntryHeader.PopulateCH_BGMReferenceIfNeeded();
			completionDummyEntryHeader.CH_IsActive = false;

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Completion;
			var completionEntryHeader = declaration.CusEntryHeader;
			completionEntryHeader.Messages.AddNew(typeof(TSWMessage));
			completionEntryHeader.PopulateCH_BGMReferenceIfNeeded();

			AssertEquals("orginalEntry should not be removed", false, orginalEntry.IsDeleted);
			AssertEquals("completionDummyEntryHeader should be deleted", true, completionDummyEntryHeader.IsDeleted);
			AssertEquals("completionEntryHeader should not be removed", false, completionEntryHeader.IsDeleted);
		}

		[TestDate(2008, 6, 6)]
		public void TestAutoRatingDoesntPickUpDoubleChargesWhenASecondEntryHeaderIsPresent()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000.00m;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000.00m;
			invoiceLine.JI_Tariff = "4201.00.00.01B";
			invoiceLine.JI_RN_NKCountryOfExport = "US";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;

			CusEntryHeader discardedEntryHeader = declaration.CusEntryHeader;
			discardedEntryHeader.Messages.AddNewTestTransmitMessage();
			ZString mergeErrors = declaration.MergeAndSaveIfNotMergedAlreadyReturningErrors();
			AssertEquals("Precondition: mergeErrors", "", mergeErrors);

			declaration.ResetToOriginal();
			mergeErrors = declaration.MergeAndSaveIfNotMergedAlreadyReturningErrors();
			AssertEquals("Precondition: mergeErrors", "", mergeErrors);
			CusEntryHeader formalEntryHeader = declaration.CusEntryHeader;
			formalEntryHeader.Messages.AddNewTestTransmitMessage();

			AssertEquals("Precondition: discardedEntryHeader.CH_IsActive", false, discardedEntryHeader.CH_IsActive);
			AssertEquals("Precondition: formalEntryHeader.CH_IsActive", true, formalEntryHeader.CH_IsActive);

			AssertEquals("discardedEntryHeader.EntryFeeAmount", 29.00m, discardedEntryHeader.EntryFeeAmount);
			AssertEquals("formalEntryHeader.EntryFeeAmount", 29.00m, formalEntryHeader.EntryFeeAmount);

			ICustomsCharges chargesGetter = CargoWise.Common.ServiceLocator.GetService<ICustomsCharges>(declaration);

			AssertEquals("chargesGetter.IsActive", true, chargesGetter.IsActive);
			CustomsCharge[] charges = chargesGetter.GetCustomsCharges(null);
			ZDecimal totalEntryFee = 0m;
			ZDecimal totalCharges = 0m;
			foreach (CustomsCharge charge in charges)
			{
				if (charge.Description == EntryChargeTypeList.Descriptions.EntryFee)
				{
					totalEntryFee += charge.Amount;
				}
				totalCharges += charge.Amount;
			}
			AssertEquals("totalEntryFee", 29.00m, totalEntryFee);
			AssertEquals("totalCharges", 2066.50m, totalCharges);
		}

		public void TestGetUncancelledDeclarationAmendedPermitEvents()
		{
			var entryHeader = Factory.New<CusEntryHeader>();

			var logs = entryHeader.GetUncancelledDeclarationAmendedPermitEvents();
			AssertEquals("Number of uncancelled DeclarationAmendedPermitApproved events", 0, logs.Length);

			var newLog = entryHeader.Logs.AddNew(Events.DeclarationAmendedPermitApproved);
			logs = entryHeader.GetUncancelledDeclarationAmendedPermitEvents();
			AssertEquals("Number of uncancelled DeclarationAmendedPermitApproved events", 1, logs.Length);
			AssertEquals("Number of uncancelled DeclarationAmendedPermitApproved events", newLog.PK, logs[0].PK);

			newLog.Cancel();
			logs = entryHeader.GetUncancelledDeclarationAmendedPermitEvents();
			AssertEquals("Number of uncancelled DeclarationAmendedPermitApproved events", 0, logs.Length);
		}

		public void TestDeclarationAmendedPermitEventsAreCancelledWhenStatusChangedFromQueuedForSendingToNotSentToCustoms()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;

			var logs = entryHeader.GetUncancelledDeclarationAmendedPermitEvents();
			AssertEquals("Number of uncancelled DeclarationAmendedPermitApproved events", 0, logs.Length);

			entryHeader.Logs.AddNew(Events.DeclarationAmendedPermitApproved);
			entryHeader.Logs.AddNew(Events.DeclarationAmendedPermitApproved);
			logs = entryHeader.GetUncancelledDeclarationAmendedPermitEvents();
			AssertEquals("Number of uncancelled DeclarationAmendedPermitApproved events", 2, logs.Length);

			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			logs = entryHeader.GetUncancelledDeclarationAmendedPermitEvents();
			AssertEquals("Number of uncancelled DeclarationAmendedPermitApproved events", 0, logs.Length);

			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;

			entryHeader.Logs.AddNew(Events.DeclarationAmendedPermitApproved);
			logs = entryHeader.GetUncancelledDeclarationAmendedPermitEvents();
			AssertEquals("Number of uncancelled DeclarationAmendedPermitApproved events", 1, logs.Length);

			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			logs = entryHeader.GetUncancelledDeclarationAmendedPermitEvents();
			AssertEquals("Number of uncancelled DeclarationAmendedPermitApproved events", 1, logs.Length);
		}

		JobDeclaration SetupDeclarationWithFuelLeviesTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1001001";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;

			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;
			line.JI_Tariff = "2710.19.21.10C"; // UNLEADED MOTOR FUEL has fuel levies
			line.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			line.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			line.JI_QualifiesForPreferentialDuty = "N";
			line.JI_CustomsQuantity = 1000m;
			line.JI_CustomsUnitQty = StatisticalUQList.Codes.Litres;

			return declaration;
		}
	}

	public abstract class CusEntryHeaderTest<T> : Customs.Business.Testing.CusEntryHeaderAbstractTest
		where T : CusEntryHeader
	{
		public new void TestWorkflowSupportableBusinessObject()
		{
			Assert("We no longer support workflow on CusEntryHeader", condition: true);
		}

		public void TestDutyTotalInMergedLines()
		{
			DutyCalculatorTestObjects testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.EntryHeader.MergedLines.AddNew().DutyAmount = 12.32m;
			testObjects.EntryHeader.MergedLines.AddNew().DutyAmount = 54.77m;
			AssertEquals("CusEntryLine.DutyAmount", 67.09m, testObjects.EntryHeader.DutyTotalInMergedLines);
		}

		public void TestCH_CustomsDeliveryInstructionsIsAlwaysReadOnly()
		{
			Assert("CustomsDeliveryInstructions should be read-only", EntryHeader.CH_CustomsDeliveryInstructionsInfo.ReadOnly);
		}

		public void TestHasBeenWithdrawn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCancelled;
			AssertEquals(true, entry.HasBeenWithdrawn);

			entry.CH_EntryStatus = FormalEntryStatusList.Codes.EntryInError;
			AssertEquals(false, entry.HasBeenWithdrawn);
		}

		public void TestValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(typeof(CusEntryHeaderValidation), entry.Validation.GetType());
		}

		public void TestIsImport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("entryHeader.IsImport", true, Declaration.CusEntryHeader.IsImport);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("entryHeader.IsImport", false, Declaration.CusEntryHeader.IsImport);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals("entryHeader.IsImport", false, Declaration.CusEntryHeader.IsImport);
			Declaration.JE_MessageType = ZString.Empty;
			AssertEquals("entryHeader.IsImport", false, Declaration.CusEntryHeader.IsImport);
		}

		public void TestIsExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("entryHeader.IsExport", false, Declaration.CusEntryHeader.IsExport);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("entryHeader.IsExport", true, Declaration.CusEntryHeader.IsExport);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals("entryHeader.IsExport", false, Declaration.CusEntryHeader.IsExport);
			Declaration.JE_MessageType = ZString.Empty;
			AssertEquals("entryHeader.IsExport", false, Declaration.CusEntryHeader.IsExport);
		}

		public void TestHasNonCancelledMessages()
		{
			CusEntryHeader entryHeader = GetNewCusEntryHeader();
			AssertEquals(false, entryHeader.HasNonCancelledMessages);

			NZCMessage message1 = entryHeader.Messages.AddNew();
			message1.EM_Status = NZCMessage.Status.Queued;
			AssertEquals(true, entryHeader.HasNonCancelledMessages);

			message1.EM_Status = NZCMessage.Status.Cancelled;
			AssertEquals(false, entryHeader.HasNonCancelledMessages);

			NZCMessage message2 = entryHeader.Messages.AddNew();
			message2.EM_Status = NZCMessage.Status.Queued;
			AssertEquals(true, entryHeader.HasNonCancelledMessages);
		}

		public void TestDateMessageQueuedToBeSentOn()
		{
			var entryHeader = GetNewCusEntryHeader();
			AssertEquals(ZDateTime.Empty, entryHeader.DateMessageQueuedToBeSentOn);

			entryHeader.CH_EDITransmitDate = new ZDateTime(2006, 1, 1);
			AssertEquals(new ZDateTime(2006, 1, 1), entryHeader.DateMessageQueuedToBeSentOn);

			var message1 = entryHeader.Messages.AddNew();
			message1.EM_Status = NZCMessage.Status.Cancelled;
			message1.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message1.EM_HeldUntilDate = new ZDateTime(2006, 2, 3);
			AssertEquals(new ZDateTime(2006, 1, 1), entryHeader.DateMessageQueuedToBeSentOn);

			var message2 = entryHeader.Messages.AddNew();
			message2.EM_Status = NZCMessage.Status.Queued;
			message2.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message2.EM_HeldUntilDate = new ZDateTime(2006, 4, 5);
			var message2LocalHeldUntilDate = message2.EM_HeldUntilDate.ToLocalBranchTime(Factory);
			AssertEquals(message2LocalHeldUntilDate, entryHeader.DateMessageQueuedToBeSentOn);

			var message3 = entryHeader.Messages.AddNew();
			message3.EM_Status = NZCMessage.Status.Sent;
			message3.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			message3.EM_HeldUntilDate = new ZDateTime(2006, 6, 7);
			AssertEquals(message2LocalHeldUntilDate, entryHeader.DateMessageQueuedToBeSentOn);
		}

		public void TestCancelQueuedMessages()
		{
			CusEntryHeader entryHeader = GetNewCusEntryHeader();

			NZCMessage message1 = entryHeader.Messages.AddNew();
			message1.EM_Status = NZCMessage.Status.Cancelled;
			message1.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;

			NZCMessage message2 = entryHeader.Messages.AddNew();
			message2.EM_Status = NZCMessage.Status.Queued;
			message2.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;

			NZCMessage message3 = entryHeader.Messages.AddNew();
			message3.EM_Status = NZCMessage.Status.Sent;
			message3.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;

			NZCMessage message4 = entryHeader.Messages.AddNew();
			message4.EM_Status = NZCMessage.Status.Cancelled;
			message4.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			NZCMessage message5 = entryHeader.Messages.AddNew();
			message5.EM_Status = NZCMessage.Status.Queued;
			message5.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			NZCMessage message6 = entryHeader.Messages.AddNew();
			message6.EM_Status = NZCMessage.Status.Sent;
			message6.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			entryHeader.CancelQueuedMessages();

			AssertEquals("message1.EM_Status", NZCMessage.Status.Cancelled, message1.EM_Status);
			AssertEquals("message2.EM_Status", NZCMessage.Status.Cancelled, message2.EM_Status);
			AssertEquals("message3.EM_Status", NZCMessage.Status.Sent, message3.EM_Status);
			AssertEquals("message4.EM_Status", NZCMessage.Status.Cancelled, message4.EM_Status);
			AssertEquals("message5.EM_Status", NZCMessage.Status.Queued, message5.EM_Status);
			AssertEquals("message6.EM_Status", NZCMessage.Status.Sent, message6.EM_Status);
		}

		public void TestDeleteIfContainsNoValuableData()
		{
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, Declaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(Declaration));
			CusEntryHeader activeEntryHeader = Declaration.CusEntryHeader;
			CusEntryHeader inactiveEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
			inactiveEntryHeader.CH_IsActive = false;
			activeEntryHeader.DeleteIfContainsNoValuableData();
			inactiveEntryHeader.DeleteIfContainsNoValuableData();
			AssertEquals("activeEntryHeader.IsDeleted()", false, activeEntryHeader.IsDeleted);
			AssertEquals("inactiveEntryHeader.IsDeleted()", ShouldBeDeletedIfContainsNoValuableData(inactiveEntryHeader), inactiveEntryHeader.IsDeleted);
		}

		protected virtual bool ShouldBeDeletedIfContainsNoValuableData(CusEntryHeader inactiveEntryHeader) => true;

		[ExpectNoExceptions]
		public void TestDeleteIfContainsNoValuableDataWithMultipleCountryHeaders()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Name = "US Test Company";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Code = "USC";

			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "USA";
			usBranch.GB_RL_NKHomePort = "USLAX";

			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();

			var usDeclaration = Factory.New<JobDeclaration>();
			usDeclaration.JE_JS = shipment.PK;
			usDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			usDeclaration.JE_ApplicationCode = "ACE";
			usDeclaration.JE_GB = usBranch.PK;
			var usEntry = usDeclaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Pre-condition", Core.Constants.CountryCodes.UnitedStates, usEntry.CountryCode);

			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, Declaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(Declaration));
			Declaration.JE_JS = shipment.PK;
			var activeEntryHeader = Declaration.CusEntryHeader;
			var inactiveEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
			inactiveEntryHeader.CH_IsActive = false;
			activeEntryHeader.DeleteIfContainsNoValuableData();
			inactiveEntryHeader.DeleteIfContainsNoValuableData();
			usEntry.DeleteIfContainsNoValuableData();
			AssertEquals("activeEntryHeader.IsDeleted()", false, activeEntryHeader.IsDeleted);
			AssertEquals("inactiveEntryHeader.IsDeleted()", ShouldBeDeletedIfContainsNoValuableData(inactiveEntryHeader), inactiveEntryHeader.IsDeleted);
			AssertEquals("usEntry should not be hit when running through NZ Delete functionality", false, usEntry.IsDeleted);
		}

		public void TestIsFormalEntry()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			Assert("Entry Header is a formal entry", entryHeader.IsFormalEntry);
		}

		public void TestIsIPIEntryHeader()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			Assert("Entry Header is a Primary Industries Import entry", entryHeader.IsIPIEntryHeader);
		}

		public void TestIsCurrentEntryHeaderOnDeclaration()
		{
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, Declaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(Declaration));
			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_IsActive = false;
			AssertEquals(false, entryHeader.IsEntryHeaderCurrentDeclarationType(Declaration));
		}

		public void TestIPIIsCurrentEntryHeaderOn()
		{
			var jobDeclaration = CreateImportSeaJob();
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, jobDeclaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(jobDeclaration));

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "61844737";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.FormalEntry;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			// now change the current declaration to an IPI declaration
			jobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			var ipiEntryHeader = jobDeclaration.CusEntryHeader;
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, jobDeclaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(jobDeclaration));

			var ipiEntryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			ipiEntryNum.CE_EntryNum = "75328491";
			ipiEntryNum.CE_EntryType = CusEntryNumberTypeList.Codes.PrimaryIndustriesEntry;
			ipiEntryNum.CE_ParentID = ipiEntryHeader.PK;
			ipiEntryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			// Change the declaration back to a Formal declaration again
			jobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Normal;
			Factory.Save();
			AssertEquals("Formal entry is now the current entry", entryHeader, jobDeclaration.CusEntryHeader);
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, jobDeclaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(jobDeclaration));

			// Change the declaration back to the Primary Industries declaration again
			jobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			AssertEquals("Primary Industry entry is now the current entry", ipiEntryHeader, jobDeclaration.CusEntryHeader);
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, jobDeclaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(jobDeclaration));

			// Both the Formal entry & IPI entry should be active
			Assert("Both Formal entry & IPI entry should be active", ipiEntryHeader.IsActive);
			Assert("Both Formal entry & IPI entry should be active", entryHeader.IsActive);

			// Change the declaration back to a Formal declaration again
			jobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Normal;
			Factory.Save();

			// Both the Formal entry & IPI entry should still be active
			Assert("Both Formal entry & IPI entry should be active", ipiEntryHeader.IsActive);
			Assert("Both Formal entry & IPI entry should be active", entryHeader.IsActive);

			// Change the declaration back to the IPI entry
			jobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			Factory.Save();

			// Both the Formal entry & IPI entry should still be active
			Assert("Both Formal entry & IPI entry should be active", ipiEntryHeader.IsActive);
			Assert("Both Formal entry & IPI entry should be active", entryHeader.IsActive);
		}

		public void TestCrapObjectsGetDeletedOnSave()
		{
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, Declaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(Declaration));
			CusEntryHeader activeEntryHeader = Declaration.CusEntryHeader;
			CusEntryHeader inactiveEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
			inactiveEntryHeader.CH_IsActive = false;
			Factory.Save();
			AssertEquals("activeEntryHeader.IsDeleted()", false, activeEntryHeader.IsDeleted);
			AssertEquals("inactiveEntryHeader.IsDeleted()", ShouldBeDeletedIfContainsNoValuableData(inactiveEntryHeader), inactiveEntryHeader.IsDeleted);
		}

		/*
		 * This test simulates concurrent creation of entry headers. It ensures no more than one is ultimately created.
		 * Methododology:
		 * 1. Uses 2 factories to simulate concurrent users, another factory to assert the result, to achieve data isolation.
		 * 2. The later created entry header should be deleted on saving without an error
		 * 3. Care was taken as to how query cache is touched to guarentee cross-factory isolation and simulate normal cache snapshot
		 */
		public void TestDuplicateHeaderNotCreatedWithRowInDB()
		{
			var declaration = Declaration;
			var anotherFactory = new BusinessObjectFactory();
			Factory.Save();

			CusEntryHeader firstEntryHeader = declaration.CusEntryHeader;
			firstEntryHeader.CH_IsActive = true;

			var anotherEntryCollection = ((JobDeclaration)anotherFactory.Load(declaration.GetType(), declaration.PK)).CustomsEntryHeaders;
			var secondEntryHeader = (CusEntryHeader)anotherFactory.New(firstEntryHeader.GetType());
			secondEntryHeader.CH_JE = declaration.PK;
			anotherEntryCollection.Add(secondEntryHeader);

			Factory.Save();

			secondEntryHeader.CH_IsActive = true;
			// set up query cache to test if the program will ignore it when trying to poll new entry headers from DB
			anotherEntryCollection.Reload(false);

			anotherFactory.Save();
			CombineAssertions(() =>
			{
				var assertFactory = new BusinessObjectFactory();
				AssertEquals("Only one entry header remains", true, assertFactory.Load<CusEntryHeader>(firstEntryHeader.PK) == null ^ assertFactory.Load<CusEntryHeader>(secondEntryHeader.PK) == null);
			});
		}

		public void TestCH_VersionNumberFromLastResponse()
		{
			AssertEquals("EntryHeader.CH_VersionNumberFromLastResponse", 0, EntryHeader.CH_LastResponseVersionNumber);
			EntryHeader.CH_LastResponseVersionNumber = 12;
			AssertEquals("EntryHeader.CH_VersionNumberFromLastResponse", 12, EntryHeader.CH_LastResponseVersionNumber);
			EntryHeader.CH_LastResponseVersionNumber = 15;
			AssertEquals("EntryHeader.CH_VersionNumberFromLastResponse", 15, EntryHeader.CH_LastResponseVersionNumber);
		}

		public void TestCH_MPIFoodResponseTime()
		{
			AssertEquals("CH_MPIFoodResponseTime", ZDateTime.Empty, EntryHeader.CH_MPIFoodResponseTime);
			EntryHeader.CH_MPIFoodResponseTime = ZDateTime.BrettsBirthday;
			AssertEquals("CH_MPIFoodResponseTime", ZDateTime.BrettsBirthday, EntryHeader.CH_MPIFoodResponseTime);
		}

		public void TestCH_MPIFoodStatus()
		{
			AssertEquals("CH_MPIFoodStatus", ZString.Empty, EntryHeader.CH_MPIFoodStatus);
			EntryHeader.CH_MPIFoodStatus = "F04";
			AssertEquals("CH_MPIFoodStatus", "F04", EntryHeader.CH_MPIFoodStatus);
		}

		public void TestCH_MPIBioResponseTime()
		{
			var testDate = ZDateTime.Now;
			AssertEquals("CH_MPIBioResponseTime", ZDateTime.Empty, EntryHeader.CH_MPIBioResponseTime);
			EntryHeader.CH_MPIBioResponseTime = testDate;
			AssertEquals("CH_MPIBioResponseTime", testDate, EntryHeader.CH_MPIBioResponseTime);
		}

		public void TestCH_MPIBioStatus()
		{
			AssertEquals("CH_MPIBioStatus", ZString.Empty, EntryHeader.CH_MPIBioStatus);
			EntryHeader.CH_MPIBioStatus = "B04";
			AssertEquals("CH_MPIBioStatus", "B04", EntryHeader.CH_MPIBioStatus);
		}

		public void TestCH_NZCSResponseTime()
		{
			var testDate = ZDateTime.Now.AddHours(-2);
			AssertEquals("CH_NZCSResponseTime", ZDateTime.Empty, EntryHeader.CH_NZCSResponseTime);
			EntryHeader.CH_NZCSResponseTime = testDate;
			AssertEquals("CH_NZCSResponseTime", testDate, EntryHeader.CH_NZCSResponseTime);
		}

		public void TestCH_NZCSStatus()
		{
			AssertEquals("CH_NZCSStatus", ZString.Empty, EntryHeader.CH_NZCSStatus);
			EntryHeader.CH_NZCSStatus = "822";
			AssertEquals("CH_NZCSStatus", "822", EntryHeader.CH_NZCSStatus);
		}

		public void TestIsActive()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			CusEntryHeader eCIEntryHeader = Declaration.CusEntryHeader;
			eCIEntryHeader.Messages.AddNew();
			AssertEquals("ECIEntryHeader.IsActive", true, eCIEntryHeader.IsActive);
			eCIEntryHeader.IsActive = false;
			AssertEquals("ECIEntryHeader.IsActive", false, eCIEntryHeader.IsActive);
		}

		public void TestOnSavingSetsBGMReference()
		{
			Assert("Precondition: EntryHeader.CH_BGMReference is empty before saving", EntryHeader.CH_BGMReference.IsEmpty);
			Factory.Save();
			Assert("EntryHeader.CH_BGMReference should be filled in After Saving", !EntryHeader.CH_BGMReference.IsEmpty);
		}

		public void TestPopulateCH_BGMReferenceIfNeeded()
		{
			Db.Connection.BeginTransaction();
			Assert("Precondition: EntryHeader.CH_BGMReference is empty", EntryHeader.CH_BGMReference.IsEmpty);
			EntryHeader.PopulateCH_BGMReferenceIfNeeded();
			Assert("EntryHeader.CH_BGMReference should be filled in", !EntryHeader.CH_BGMReference.IsEmpty);
			Db.Connection.RollbackTransaction();
		}

		public void TestPopulateBGMReferenceForCompletionEntry()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var entryHeader = declaration.CusEntryHeader;
			Db.Connection.BeginTransaction();
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			Assert("CH_BGMReference generated from the standard entry", !entryHeader.CH_BGMReference.EndsWith("C"));
			AssertEquals(EntryHeaderTypes.NZ.FormalEntry, entryHeader.CH_MessageType);

			declaration.DeclarationNumber = "23420862";
			declaration.JE_EntryStatus = "DOR";
			Factory.Save();

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Factory.Save();
			AssertEquals("23420862", declaration.JE_OriginalEntryNumber);

			var completionEntryHeader = declaration.CusEntryHeader;
			AssertEquals(EntryHeaderTypes.NZ.Completion, completionEntryHeader.CH_MessageType);
			Assert("CH_BGMReference generated from the completion entry is suffixed with 'C'", completionEntryHeader.CH_BGMReference.EndsWith("C"));
			Db.Connection.RollbackTransaction();
		}

		public void TestPopulateBGMReferenceForStandAloneCompletionEntry()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_OriginalEntryNumber = "60914656";
			declaration.JE_OriginalEntryType = "TMP";
			var entryHeader = declaration.CusEntryHeader;
			Factory.Save();

			AssertEquals("CustomsEntryHeaders - should be 2, header for this completion & header with manual data from original entry details", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("declaration entryHeader should be the completion entry", EntryHeaderTypes.NZ.Completion, entryHeader.CH_MessageType);
			Assert("CH_BGMReference generated from the completion entry is suffixed with 'C'", entryHeader.CH_BGMReference.EndsWith("C"));

			var manualHeader = declaration.EntryHeaderForOriginalEntryNumber;
			AssertEquals("declaration EntryHeaderForOriginalEntryNumber should be the manual entry for storing original entry number", EntryHeaderTypes.NZ.Original, manualHeader.CH_MessageType);
			AssertEquals("declaration manualHeader should have MAN status", FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms, manualHeader.CH_EntryStatus);
			Assert("CH_BGMReference generated from the manual entry should not be a 'C' type reference", !manualHeader.CH_BGMReference.EndsWith("C"));
			AssertEquals("References should be the same except for the completion entry being suffixed with 'C'", entryHeader.CH_BGMReference.TrimEnd('C'), manualHeader.CH_BGMReference);
		}

		public void TestCH_EDITransmitDate()
		{
			AssertEquals("Default value of CH_EDITransmitDate.", ZDateTime.Empty, EntryHeader.CH_EDITransmitDate);
			EntryHeader.CH_EDITransmitDate = new ZDateTime(2004, 12, 12);
			AssertEquals("CH_EDITransmitDate was just set. Checking Value.", new ZDateTime(2004, 12, 12), EntryHeader.CH_EDITransmitDate);
			EntryHeader.CH_EDITransmitDate = ZDateTime.Empty;
			AssertEquals("CH_EDITransmitDate was just set. Checking Value.", ZDateTime.Empty, EntryHeader.CH_EDITransmitDate);
		}

		public void TestCH_IsActive()
		{
			EntryHeader.CH_IsActive = true;
			AssertEquals("CH_IsActive was just set. Checking Value.", true, EntryHeader.CH_IsActive);
			EntryHeader.CH_IsActive = false;
			AssertEquals("CH_IsActive was just set. Checking Value.", false, EntryHeader.CH_IsActive);
		}

		public void TestCH_RecordAdded()
		{
			ZDateTime testDate = new ZDateTime(2004, 12, 12, 12, 12, 12);
			EntryHeader.CH_RecordAdded = ZDateTime.Empty;
			AssertEquals("CH_RecordAdded was just set. Checking Value.", ZDateTime.Empty, EntryHeader.CH_RecordAdded);
			EntryHeader.CH_RecordAdded = testDate;
			AssertEquals("CH_RecordAdded was just set. Checking Value.", testDate, EntryHeader.CH_RecordAdded);
		}

		public void TestCH_LastNumberOfLinesSentToCustoms()
		{
			AssertEquals("CH_LastNumberOfLinesSentToCustoms Default", 0, EntryHeader.CH_LastNumberOfLinesSentToCustoms);
			EntryHeader.CH_LastNumberOfLinesSentToCustoms = 10;
			AssertEquals("CH_LastNumberOfLinesSentToCustoms was just set. Checking Value.", 10, EntryHeader.CH_LastNumberOfLinesSentToCustoms);
			EntryHeader.CH_LastNumberOfLinesSentToCustoms = 999;
			AssertEquals("CH_LastNumberOfLinesSentToCustoms was just set. Checking Value.", 999, EntryHeader.CH_LastNumberOfLinesSentToCustoms);
		}

		public void TestDeclaration()
		{
			AssertEquals(Declaration, EntryHeader.Declaration);
		}

		public void TestMergedLines()
		{
			CusEntryLine cusEntryLine = EntryHeader.MergedLines.AddNew();
			cusEntryLine.CL_ParentTrailer = "F";
			AssertEquals("F", cusEntryLine.CL_ParentTrailer);
		}

		public void TestMessages()
		{
			NZCMessage message = EntryHeader.Messages.AddNew();
			message.EM_MessageText = "123";
			AssertEquals("Checking EM_MessageText", message.EM_MessageText, "123");
		}

		public void TestChargesCollection()
		{
			AssertNotNull("Charges Collection should not be null", EntryHeader);
			CusEntryHeaderCharge charge = EntryHeader.Charges.AddNew();
			AssertNotNull("Charge should be obtainable from the collection", charge);
		}

		public void TestEntryFee()
		{
			CusEntryHeaderCharge charge = EntryHeader.Charges.AddNew(EntryChargeTypeList.Codes.EntryFee, 23m);
			AssertEquals("EntryHeader.EntryFeeAmount", 23m, EntryHeader.EntryFeeAmount);
			EntryHeader.EntryFeeAmount = 44m;
			AssertEquals("EntryHeader.EntryFeeAmount", 44m, EntryHeader.EntryFeeAmount);
		}

		public void TestEntryFeeGST()
		{
			CusEntryHeaderCharge charge = EntryHeader.Charges.AddNew(EntryChargeTypeList.Codes.EntryFeeGST, 23m);
			AssertEquals("EntryHeader.EntryFeeGST", 23m, EntryHeader.EntryFeeGST);
			EntryHeader.EntryFeeGST = 44m;
			AssertEquals("EntryHeader.EntryFeeAmount", 44m, EntryHeader.EntryFeeGST);
		}

		public void TestIsCurrentEntryHeaderOn()
		{
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, Declaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(Declaration));

			CusEntryHeader formalEntryHeader = Declaration.CusEntryHeader;
			CusEntryHeader industryPrimaryHeader = Declaration.CustomsEntryHeaders.AddNew();
			formalEntryHeader.CH_IsActive = false;
			industryPrimaryHeader.CH_IsActive = true;
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", false, formalEntryHeader.IsEntryHeaderCurrentDeclarationType(Declaration));

			formalEntryHeader.CH_IsActive = true;
			industryPrimaryHeader.CH_IsActive = false;
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, formalEntryHeader.IsEntryHeaderCurrentDeclarationType(Declaration));
		}

		#region Implementation

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetNewJobDeclaration();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		protected virtual JobDeclaration GetNewJobDeclaration()
		{
			return JobDeclaration.New(Factory);
		}
		#endregion

		#region EntryHeader
		protected CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = GetNewCusEntryHeader();
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		protected virtual CusEntryHeader GetNewCusEntryHeader()
		{
			return Declaration.CusEntryHeader;
		}
		#endregion

		protected override Type ExpectedChargeCollectionType => typeof(Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharge>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharge);

		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		protected override BusinessObject GetNewBusinessObject() => EntryHeader;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCancelled;
			return EntryHeader;
		}

		JobDeclaration CreateImportSeaJob()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "ANL SHIPPING";

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			jobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			jobDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			jobDeclaration.JE_TransactionNature = NatureOfTransactionList.Codes.N10;
			jobDeclaration.JE_TotalWeight = 15m;
			jobDeclaration.JE_TotalWeightUnit = "T";
			jobDeclaration.JE_VoyageFlightNo = "227W";
			jobDeclaration.JE_ContainerMode = "FCL";
			jobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			jobDeclaration.JE_DeclarationReference = "BSIS/00002309";
			jobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-14);
			jobDeclaration.JE_GoodsDescription = "CHEMICALS";
			jobDeclaration.JE_GS_NKCusAgent = "JKS";
			jobDeclaration.JE_HouseBill = "HB92027";
			jobDeclaration.JE_MasterBill = "OB293042-24902";
			jobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			jobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			jobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			jobDeclaration.JE_RL_NKOrigin = "SGSIN";
			jobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			jobDeclaration.JE_RL_NKPortOfLoading = "SGSIN";
			jobDeclaration.JE_VesselName = "Hyogo Maru";
			jobDeclaration.JE_TotalNoOfPacks = 15;
			jobDeclaration.JE_TotalNoOfPacksPackType = "CNT";

			NZAddInfo addInfo = ((IHaveNZAddInfo)jobDeclaration).AddInfo;
			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
			var wareHouseOrg = Factory.New<OrgHeader>();
			wareHouseOrg.OH_Code = "WAREHOUSE";
			OrgCusCode supplierCode = wareHouseOrg.CustomsCodes.AddNew();
			supplierCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			supplierCode.OK_CountryDefault = true;
			supplierCode.OK_CustomsRegNo = "2975W";
			supplierCode.OK_RN_NKCodeCountry = "NZ";
			jobDeclaration.WarehouseDocAddress.E2_OA_Address = wareHouseOrg.MainAddress.PK;

			var container1 = jobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "MNHU0029382";
			container1.CO_ContainerSize = ContainerSizeList.Codes.ContainerIc40Ft;
			container1.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C45;
			container1.CO_MPIApprovedSystemNumber = "75128";

			var packLine1 = jobDeclaration.Packages[0];
			packLine1.CW_PackQty = 2;
			packLine1.CW_PackType = "07";

			var container2 = jobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "MNHU0149961";
			container2.CO_ContainerSize = ContainerSizeList.Codes.ContainerIc40Ft;
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			container2.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C45;
			container2.CO_MPIApprovedSystemNumber = "73492";

			var packLine2 = jobDeclaration.Packages[1];
			packLine2.CW_HouseBill = packLine1.CW_HouseBill;
			packLine2.CW_ContainerNoOrEquipmentNo = "MNHU0149961";
			packLine2.CW_PackQty = 1;
			packLine2.CW_PackType = "PK";

			jobDeclaration.JE_SendMCDContainerQuarantineDeclaration = true;
			jobDeclaration.JE_HaveMAFContainerDeclaration = true;
			jobDeclaration.JE_IsContainerClean = true;
			jobDeclaration.JE_IsWoodPackagingUsed = true;

			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);

			var invoiceLine = jobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			return jobDeclaration;
		}

		#endregion
	}

	public class CustomsEntryHeader_NoDuplicatedReferenceTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestBGMReferenceNoDuplicate()
		{
			var factory1 = new BusinessObjectFactory();
			var declaration1 = factory1.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var reference1 = ZString.Empty;
			var connection1 = ((IDbConnected)factory1).Connection;

			try
			{
				connection1.BeginTransaction();
				entry1.OnSaving();
				reference1 = entry1.CH_BGMReference;
			}
			finally
			{
				connection1.RollbackTransaction();
			}

			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			factory2.Save();
			AssertEquals("CH_BGMReference should use the last number from number fountain as the transaction rolled back.", reference1, entry2.CH_BGMReference);
			AssertEquals("Should not refresh CH_BGMReference untill factory saving.", reference1, entry1.CH_BGMReference);

			declaration1.JE_DeclarationReference = "";
			factory1.Save();
			AssertNotEquals("CH_BGMReference get a new number after another saving.", reference1, entry1.CH_BGMReference);
		}
	}
}
