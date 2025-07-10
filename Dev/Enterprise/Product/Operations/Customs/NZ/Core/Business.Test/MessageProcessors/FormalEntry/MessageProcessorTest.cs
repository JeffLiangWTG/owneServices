using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.FormalEntry.Testing
{
	using CargoWise.BrandManager;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
	using Enterprise.Customs.NZ.Business.Declaration.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.DocumentEngine.Scheduler.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Schema;
	using Messaging.Business;
	using NUnit.Framework;

	public class MessageProcessorTest : MessageProcessors.Testing.MessageProcessorForEntryHeaderTest
	{
		public void TestEmailGroupUserSettings()
		{
			var importAckGroup = Guid.NewGuid();
			ZString importAckMode = Constants.EmailTo.NoEmails;
			var importImpGroup = Guid.NewGuid();
			ZString importImpMode = Constants.EmailTo.StaffMember;
			var importErrGroup = Guid.NewGuid();
			ZString importErrMode = Constants.EmailTo.NominatedGroup;

			var exportAckGroup = Guid.NewGuid();
			ZString exportAckMode = Constants.EmailTo.StaffMember;
			var exportImpGroup = Guid.NewGuid();
			ZString exportImpMode = Constants.EmailTo.NominatedGroup;
			var exportErrGroup = Guid.NewGuid();
			ZString exportErrMode = Constants.EmailTo.StaffMemberAndNominatedGroup;

			var unsolicitedDOGroup = Guid.NewGuid();
			ZString unsolicitedDOMode = Constants.EmailTo.NominatedGroup;

			NZCustomsDataRegistry.Instance.ImportDeclarationsSendAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importAckGroup);
			NZCustomsDataRegistry.Instance.ImportDeclarationsSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importAckMode);
			NZCustomsDataRegistry.Instance.ImportDeclarationsSendImpedimentsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importImpGroup);
			NZCustomsDataRegistry.Instance.ImportDeclarationsSendImpediments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importImpMode);
			NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrorsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importErrGroup);
			NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importErrMode);

			NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportAckGroup);
			NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportAckMode);
			NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpedimentsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportImpGroup);
			NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpediments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportImpMode);
			NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrorsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportErrGroup);
			NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportErrMode);

			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOGroup);
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOMode);

			var declaration = Factory.New<JobDeclaration>();
			var message = declaration.CusEntryHeader.Messages.AddNew();
			var processor = new MessageProcessor_ForTesting(new LoggingInformation());
			processor.Call_SetupPropertiesForMessageProcessing(message);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("processor.AcknowledgementEmailGroup", importAckGroup, processor.Get_AcknowledgementEmailGroup());
			AssertEquals("processor.AcknowledgementEmailMode", importAckMode, processor.Get_AcknowledgementEmailMode());
			AssertEquals("processor.ImpedimentEmailGroup", importImpGroup, processor.Get_ImpedimentEmailGroup());
			AssertEquals("processor.ImpedimentEmailMode", importImpMode, processor.Get_ImpedimentEmailMode());
			AssertEquals("processor.ErrorEmailGroup", importErrGroup, processor.Get_ErrorEmailGroup());
			AssertEquals("processor.ErrorEmailMode", importErrMode, processor.Get_ErrorEmailMode());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("processor.AcknowledgementEmailGroup", exportAckGroup, processor.Get_AcknowledgementEmailGroup());
			AssertEquals("processor.AcknowledgementEmailMode", exportAckMode, processor.Get_AcknowledgementEmailMode());
			AssertEquals("processor.ImpedimentEmailGroup", exportImpGroup, processor.Get_ImpedimentEmailGroup());
			AssertEquals("processor.ImpedimentEmailMode", exportImpMode, processor.Get_ImpedimentEmailMode());
			AssertEquals("processor.ErrorEmailGroup", exportErrGroup, processor.Get_ErrorEmailGroup());
			AssertEquals("processor.ErrorEmailMode", exportErrMode, processor.Get_ErrorEmailMode());

			AssertEquals("UnsolicitedDeliveryOrderGroup", unsolicitedDOGroup, processor.Get_UnsolicitedDOGroup());
			AssertEquals("UnsolicitedDeliveryOrderMode", unsolicitedDOMode, processor.Get_UnsolicitedDOMode());
		}

		public void TestDuplicateEntryResponseDoNotChangeDeclarationStatusDueToWebServiceProblem()
		{
			var factory = declaration.Factory;

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			factory.Save();
			declaration.JE_DeclarationReference = "B00031588";
			factory.Save();

			var message = SetupNZCMessage(@"UNH+1+CUSRES:D:96B:UN+B00031588'
BGM+932+19369111:01'
FTX+DIN+++MAF HOLD. 1 FCL(S) SAID TO CONTAIN 2400 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
NAD+AL+00235710A:ZZZ:143'
TAX+4+TOT'
MOA+161:19940.82'
GIS+D:134:143'
UNT+9+1'", factory.New<NZCMessage>());

			factory.Save();
			var processor = new DeclarationDelegator();
			Assert(processor.CanProcess(message));
			processor.Process(new LoggingInformation(), message);
			factory.Save();
			declaration.CusEntryHeader.Reload();
			AssertEquals("DOR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Entry Number", "19369111", declaration.CusEntryHeader.EntryNumber);

			message = SetupNZCMessage(@"UNH+1+CUSRES:D:96B:UN+B00031588'
BGM+963+82847364:01'
GIS+801:120:143'
ERP+001::044'
ERC+487::143'
UNT+6+1'", factory.New<NZCMessage>());

			factory.Save();
			processor = new DeclarationDelegator();
			Assert(processor.CanProcess(message));
			processor.Process(new LoggingInformation(), message);
			factory.Save();
			declaration.CusEntryHeader.Reload();
			AssertEquals("Should not change the status according to the response for a different entry number", "DOR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Entry Number", "19369111", declaration.CusEntryHeader.EntryNumber);
			Assert("still active", declaration.CusEntryHeader.IsActive);

			var inactivatedEntry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => !x.IsActive);
			AssertNotNull(inactivatedEntry);
			AssertEquals("Entry number is saved to the inactivated entry", "82847364", inactivatedEntry.EntryNumber);
		}

		[TestDate(2010, 8, 31)]
		public void TestExportJobsDontReportAmountDiscrepancies()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			AssertEquals("Precondition: Declaration.CusEntryHeader.TotalAmountPayableIncludingEntryFee", 14.25m, declaration.CusEntryHeader.TotalAmountPayableIncludingEntryFee);
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageExportJobsDontReportAmountDiscrepancies, InterpretedMessageExportJobsDontReportAmountDiscrepancies, expectedProcessMessageResult: true);
			AssertEquals("JE_EntryStatus - Legacy procesing", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("JE_EntryStatusDescription - Legacy procesing", FormalEntryStatusList.Descriptions.DeliveryOrderReceived, declaration.JE_EntryStatusDescription);
		}

		#region Messages for TestExportJobsDontReportAmountDiscrepancies

		const string EDIFACTMessageExportJobsDontReportAmountDiscrepancies =
@"UNH+3611+CUSRES:D:96B:UN+B00001000'
BGM+932+43173202:01'
FTX+DIN+++12 LOOSE PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:0'
GIS+D:134:143'
UNT+8+3611'";

		const string InterpretedMessageExportJobsDontReportAmountDiscrepancies =
@"[Delivery Order Received] Response for Customs Declaration: B00001000

Delivery Order
----------------------------------------------------------------------
Job Number     : B00001000
Master Bill    : 081-11111111
Entry Type     : Export (Normal)
Entry Number   : 43173202
Message No     : 3611

Message Status : (819) Delivery Order Herewith.
                 Method of Payment as Specified.

Amount Returned: $0.00
Terms          : Client Deferred

Delivery Instructions
----------------------------------------------------------------------
12 LOOSE PACKAGE(S) OR ITEM(S)";

		#endregion

		public void TestDisplayWarningMessageIfTotalDoesNotMatch()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.CusEntryHeader.CH_TotalPaid = 1234.65m;
			AssertNotEquals("Precondition: Declaration.CusEntryHeader.TotalAmountPayable", 5000.25m, declaration.CusEntryHeader.TotalAmountPayable);
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageDeliveryOrderWithDifferentTotalPayable, interpretedMessageDeliveryOrderWithDifferentTotalPayable, expectedProcessMessageResult: true);

			declaration.CusEntryHeader.CH_TotalPaid = 4985.32m;
			declaration.CusEntryHeader.EntryFeeAmount = 12.33;
			declaration.CusEntryHeader.EntryFeeGST = 2.35;
			AssertEquals("Precondition: Declaration.CusEntryHeader.TotalAmountPayable", 5000.0m, declaration.CusEntryHeader.TotalAmountPayableIncludingEntryFee);
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageDeliveryOrderWithSameTotalPayableButDifferentDecimalPlaces, InterpretedMessageDeliveryOrderWithSameTotalPayableButDifferentDecimalPlaces, expectedProcessMessageResult: true);
		}

		public void TestReturnedTotalAmountIsStoredInEntryHeader()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			messageProcessor.ProcessMessage(GetNZCMessage(EDIFACTMessageDeliveryOrderWithSameTotalPayableButDifferentDecimalPlaces));
			AssertEquals("Export CH_TotalAmountReturned", 5000.00m, declaration.CusEntryHeader.CH_TotalAmountReturned);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			messageProcessor.ProcessMessage(GetNZCMessage(EDIFACTMessageDeliveryOrderWithDifferentTotalPayable));
			AssertEquals("Export CH_TotalAmountReturned", 5000.25m, declaration.CusEntryHeader.CH_TotalAmountReturned);

			messageProcessor.ProcessMessage(GetNZCMessage(EDIFACTMessageDeliveryOrderForHeaderCountTest));
			AssertEquals("Export CH_TotalAmountReturned", 3924.25m, declaration.CusEntryHeader.CH_TotalAmountReturned);
		}

		public void TestCorrectNumberofHeaders()
		{
			var originalHeader = declaration.CusEntryHeader;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			AssertEquals("Precondition: Declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageEntryAcceptedForHeaderCountTest, InterpretedMessageEntryAcceptedForHeaderCountTest, expectedProcessMessageResult: true);
			AssertEquals("originalHeader.EntryNumber", "77013452", originalHeader.EntryNumber);
			AssertEquals("originalHeader.CH_EntryStatus", FormalEntryStatusList.Codes.InspectionsAuditRequirements, originalHeader.CH_EntryStatus);
			AssertEquals("originalHeader.CH_IsActive", true, originalHeader.CH_IsActive);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageEntryCancelledForHeaderCountTest, InterpretedMessageEntryCancelledForHeaderCountTest, expectedProcessMessageResult: true);
			AssertEquals("originalHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, originalHeader.CH_EntryStatus);
			AssertEquals("originalHeader.CH_IsActive", false, originalHeader.CH_IsActive);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageEntryRestoredForHeaderCountTest, InterpretedEntryRestoredForHeaderCountTest, expectedProcessMessageResult: true);
			var restoredHeader = declaration.CusEntryHeader;
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
			AssertNotEquals("restoredHeader", originalHeader, restoredHeader);
			AssertEquals("restoredHeader.EntryNumber", "77013452", restoredHeader.EntryNumber);
			AssertEquals("restoredHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, restoredHeader.CH_EntryStatus);
			AssertEquals("restoredHeader.CH_IsActive", true, restoredHeader.CH_IsActive);

			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 3924.25m;
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageDeliveryOrderForHeaderCountTest, InterpretedMessageDeliveryOrderForHeaderCountTest, expectedProcessMessageResult: true);
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Declaration.CusEntryHeader", restoredHeader, declaration.CusEntryHeader);
			AssertEquals("restoredHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, restoredHeader.CH_EntryStatus);
			AssertEquals("restoredHeader.CH_IsActive", true, restoredHeader.CH_IsActive);
		}

		#region Messages and Response Text for TestCorrectNumberofHeaders()

		const string EDIFACTMessageEntryAcceptedForHeaderCountTest =
@"UNH+82995+CUSRES:D:96B:UN+B00001000'
BGM+962+77013452:02'
FTX+ICN+++ADJUSTMENT RECEIVED, CLEARANCE / APPROVAL BY CUSTOMS OFFICER REQUIRED.'
GIS+805:120:143'
UNT+5+82995'";

		const string InterpretedMessageEntryAcceptedForHeaderCountTest =
		@"[Inspections/Audit Requirements] Response for Customs Declaration: B00001000

Inspections/Audit Requirements
----------------------------------------------------------------------
Job Number     : B00001000
Entry Type     : Import (Normal)
Entry Number   : 77013452
Message No     : 82995

Message Status : (805) Entry Held.
                 Instructions as Specified.

Customs Instructions
----------------------------------------------------------------------
ADJUSTMENT RECEIVED, CLEARANCE / APPROVAL BY CUSTOMS OFFICER REQUIRED.";

		const string EDIFACTMessageEntryCancelledForHeaderCountTest =
@"UNH+83031+CUSRES:D:96B:UN+B00001000'
BGM+962+77013452:03'
GIS+814:120:143'
UNT+4+83031'";

		const string InterpretedMessageEntryCancelledForHeaderCountTest =
@"[Entry Cancelled] Response for Customs Declaration: B00001000

Inspections/Audit Requirements
----------------------------------------------------------------------
Job Number     : B00001000
Entry Type     : Import (Normal)
Entry Number   : 77013452
Message No     : 83031

Message Status : (814) Entry Cancelled.";

		const string EDIFACTMessageEntryRestoredForHeaderCountTest =
@"UNH+83032+CUSRES:D:96B:UN+B00001000'
BGM+962+77013452:04'
GIS+815:120:143'
UNT+4+83032'";

		const string InterpretedEntryRestoredForHeaderCountTest =
@"[Entry Restored] Response for Customs Declaration: B00001000

Inspections/Audit Requirements
----------------------------------------------------------------------
Job Number     : B00001000
Entry Type     : Import (Normal)
Entry Number   : 77013452
Message No     : 83032

Message Status : (815) Entry Restored.";

		const string EDIFACTMessageDeliveryOrderForHeaderCountTest =
@"UNH+83033+CUSRES:D:96B:UN+B00001000'
BGM+932+77013452:04'
FTX+DIN+++1 LOOSE PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:3924.25'
GIS+D:134:143'
UNT+8+83033'";

		const string InterpretedMessageDeliveryOrderForHeaderCountTest =
@"[Delivery Order Received] Response for Customs Declaration: B00001000

Delivery Order
----------------------------------------------------------------------
Job Number     : B00001000
Entry Type     : Import (Normal)
Entry Number   : 77013452
Message No     : 83033

Message Status : (819) Delivery Order Herewith.
                 Method of Payment as Specified.

Amount Returned: $3,924.25
Terms          : Client Deferred

Delivery Instructions
----------------------------------------------------------------------
1 LOOSE PACKAGE(S) OR ITEM(S)";

		const string EDIFACTMessageDeliveryOrderWithDifferentTotalPayable =
@"UNH+83033+CUSRES:D:96B:UN+B00001000'
BGM+932+77013452:04'
FTX+DIN+++1 LOOSE PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:5000.25'
GIS+D:134:143'
UNT+8+83033'";

		readonly string interpretedMessageDeliveryOrderWithDifferentTotalPayable =
@$"[Delivery Order Received] Response for Customs Declaration: B00001000

Delivery Order
----------------------------------------------------------------------
Job Number     : B00001000
Entry Type     : Import (Normal)
Entry Number   : 77013452
Message No     : 83033

Message Status : (819) Delivery Order Herewith.
                 Method of Payment as Specified.

Amount Returned: $5,000.25
** WARNING - Total Amount Payable returned by Customs does not **
** match the amount calculated by {BrandingFactory.Instance.ProductName}. ($1,234.65) **
Terms          : Client Deferred

Delivery Instructions
----------------------------------------------------------------------
1 LOOSE PACKAGE(S) OR ITEM(S)";

		const string EDIFACTMessageDeliveryOrderWithSameTotalPayableButDifferentDecimalPlaces =
		@"UNH+83033+CUSRES:D:96B:UN+B00001000'
BGM+932+77013452:04'
FTX+DIN+++1 LOOSE PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:5000.00'
GIS+D:134:143'
UNT+8+83033'";

		const string InterpretedMessageDeliveryOrderWithSameTotalPayableButDifferentDecimalPlaces =
@"[Delivery Order Received] Response for Customs Declaration: B00001000

Delivery Order
----------------------------------------------------------------------
Job Number     : B00001000
Entry Type     : Import (Normal)
Entry Number   : 77013452
Message No     : 83033

Message Status : (819) Delivery Order Herewith.
                 Method of Payment as Specified.

Amount Returned: $5,000.00
Terms          : Client Deferred

Delivery Instructions
----------------------------------------------------------------------
1 LOOSE PACKAGE(S) OR ITEM(S)";

		#endregion

		public void TestDontUpdateDeclarationStatusIfEntryHeaderIsNotActive()
		{
			var rightEntryHeader = declaration.CusEntryHeader;
			rightEntryHeader.EntryNumber = "47975057";
			rightEntryHeader.CH_IsActive = false;
			var wrongEntryHeader = declaration.CusEntryHeader;
			wrongEntryHeader.EntryNumber = "12344321";
			AssertNotEquals("2 entry headers got from CusEntryNubmer should be different as the first one was set inactive", rightEntryHeader, wrongEntryHeader);

			rightEntryHeader.MergedLines.AddNew().DutyAmount = 11319.5m;
			ProcessMessageAndCompareAgainstExpectedResult(rightEntryHeader.Messages.AddNew(), EDIFACTMessageDeliveryOrder, InterpretedMessageDeliveryOrder, expectedProcessMessageResult: true);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("Declaration.Logs.MostRecentLogByEventTime(Events.ClearedCustoms)", null, declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertEquals("rightEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, rightEntryHeader.CH_EntryStatus);
			AssertEquals("wrongEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, wrongEntryHeader.CH_EntryStatus);
		}

		public void TestOutOfOrderResponsesByVersionNumberGetIgnored()
		{
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");

			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.50m;
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageDOBrokerDeferred, InterpretedMessageDOBrokerDeferred, expectedProcessMessageResult: true);
			var originalEntryHeader = declaration.CusEntryHeader;
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("originalEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, originalEntryHeader.CH_EntryStatus);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageEntryRestored, InterpretedMessageEntryRestored, expectedProcessMessageResult: true);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, declaration.JE_EntryStatus);
			AssertEquals("originalEntryHeader.CH_IsActive", true, originalEntryHeader.CH_IsActive);
			AssertEquals("Declaration.CusEntryHeader", originalEntryHeader, declaration.CusEntryHeader);
			AssertEquals("originalEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, originalEntryHeader.CH_EntryStatus);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageEntryCancelled, InterpretedMessageEntryCancelled, expectedProcessMessageResult: true);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, declaration.JE_EntryStatus);
			AssertEquals("originalEntryHeader.CH_IsActive", true, originalEntryHeader.CH_IsActive);
			AssertEquals("Declaration.CusEntryHeader", originalEntryHeader, declaration.CusEntryHeader);
			AssertEquals("originalEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, originalEntryHeader.CH_EntryStatus);
		}

		public void TestEntryRestoredResponseReActivatesCorrectEntryHeader_DeletesRedundantOne()
		{
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");

			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.50m;
			declaration.JE_EDITransmitDate = new ZDateTime(10, 10, 10);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageDOBrokerDeferred, InterpretedMessageDOBrokerDeferred, expectedProcessMessageResult: true);
			var originalEntryHeader = declaration.CusEntryHeader;
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("originalEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, originalEntryHeader.CH_EntryStatus);
			AssertEquals("originalEntryHeader.EntryNumber", "47975057", originalEntryHeader.EntryNumber);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageEntryCancelled, InterpretedMessageEntryCancelled, expectedProcessMessageResult: true);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, declaration.JE_EntryStatus);
			AssertEquals("originalEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, originalEntryHeader.CH_EntryStatus);
			AssertEquals("originalEntryHeader.CH_IsActive", false, originalEntryHeader.CH_IsActive);
			AssertEquals("originalEntryHeader.EntryNumber", "47975057", originalEntryHeader.EntryNumber);
			var secondEntryHeader = declaration.CusEntryHeader;
			AssertEquals("secondEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, secondEntryHeader.CH_EntryStatus);
			AssertEquals("secondEntryHeader.EntryNumber", "", secondEntryHeader.EntryNumber);

			ProcessMessageAndCompareAgainstExpectedResult(originalEntryHeader.Messages.AddNew(), EDIFACTMessageEntryRestored, InterpretedMessageEntryRestored, expectedProcessMessageResult: true);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader", originalEntryHeader, declaration.CusEntryHeader);
			AssertEquals("originalEntryHeader.CH_IsActive", true, originalEntryHeader.CH_IsActive);
			AssertEquals("originalEntryHeader.EntryNumber", "47975057", originalEntryHeader.EntryNumber);
			AssertEquals("originalEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, originalEntryHeader.CH_EntryStatus);
			AssertEquals("secondEntryHeader.IsDeleted", true, secondEntryHeader.IsDeleted);
			AssertEquals(new ZDateTime(10, 10, 10), declaration.JE_EDITransmitDate);
		}

		public void TestEntryRestoredResponseReActivatesCorrectEntryHeader_SetsTransmitDate()
		{
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");

			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.50m;
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageDOBrokerDeferred, InterpretedMessageDOBrokerDeferred, expectedProcessMessageResult: true);
			var originalEntryHeader = declaration.CusEntryHeader;
			declaration.JE_EDITransmitDate = new ZDateTime(10, 10, 10);

			AssertEquals("originalEntryHeader.CH_EDITransmitDate", new ZDateTime(10, 10, 10), originalEntryHeader.CH_EDITransmitDate);
			AssertEquals("Declaration.JE_EDITransmitDate", new ZDateTime(10, 10, 10), declaration.JE_EDITransmitDate);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("originalEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, originalEntryHeader.CH_EntryStatus);
			AssertEquals("originalEntryHeader.EntryNumber", "47975057", originalEntryHeader.EntryNumber);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageEntryCancelled, InterpretedMessageEntryCancelled, expectedProcessMessageResult: true);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, declaration.JE_EntryStatus);
			AssertEquals("originalEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, originalEntryHeader.CH_EntryStatus);
			AssertEquals("originalEntryHeader.CH_IsActive", false, originalEntryHeader.CH_IsActive);
			AssertEquals("originalEntryHeader.EntryNumber", "47975057", originalEntryHeader.EntryNumber);
			AssertEquals(ZDateTime.Empty, declaration.JE_EDITransmitDate);

			var secondEntryHeader = declaration.CusEntryHeader;
			AssertEquals("secondEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, secondEntryHeader.CH_EntryStatus);
			AssertEquals("secondEntryHeader.EntryNumber", "", secondEntryHeader.EntryNumber);

			ProcessMessageAndCompareAgainstExpectedResult(secondEntryHeader.Messages.AddNew(), EDIFACTMessageEntryRestored, InterpretedMessageEntryRestored, expectedProcessMessageResult: true);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader", secondEntryHeader, declaration.CusEntryHeader);
			AssertEquals("originalEntryHeader.CH_IsActive", true, secondEntryHeader.CH_IsActive);
			AssertEquals("originalEntryHeader.EntryNumber", "47975057", secondEntryHeader.EntryNumber);
			AssertEquals("originalEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, secondEntryHeader.CH_EntryStatus);
			AssertEquals("secondEntryHeader.IsDeleted", false, originalEntryHeader.CH_IsActive);
			AssertEquals(new ZDateTime(10, 10, 10), declaration.JE_EDITransmitDate);
		}

		public void TestTransmitDateIsResetOnEntryFail()
		{
			declaration.JE_DeclarationReference = "B00002249";
			declaration.JE_EDITransmitDate = ZDateTime.Today;
			var originalEntryHeader = declaration.CusEntryHeader;
			var outgoingMessage = originalEntryHeader.Messages.AddNew();
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageText = "UNH+2184+CUSDEC:D:96B:UN'BGM+929+B00002249+9'CST++10:105:143'LOC+9+AUSYD'LOC+11+NZAKL'LOC+41+NZAKL'DTM+151:20151210:102'GIS+ATF:110:143:25001'GIS+PDO:110:143'MEA+WT+AAD+KGM:1'RFF+MB:66767676767'RFF+HWB:GGDG'PAC+1++PK'TDT+20++4+++++:::NZ1'NAD+AL+51352368J:ZZZ:143'NAD+CB+00009908C:ZZZ:143'UNS+D'DMS+21+935'TOD+++FOB:106:143'CST+1+3926906969J:169:143'FTX+AAA+++ARTICLES OF PLASTICS & OF OTHER MATERIALS OF 3901 TO 3914 N.E.C. IN:CHPT 39'LOC+27+AU'LOC+35+AU'NAD+SU+00710841Y:ZZZ:143'MOA+14:3000.00:NZD'CUX+2++1.00'MOA+40:3000'MOA+64:23'MOA+70:1'GIS+N:109:143'TAX+1+CUD'MOA+161:150.00'TAX+1+GST'MOA+161:476.10'UNS+S'CNT+4:1'CNT+5:1'CNT+11:1'TAX+3+CUD++3000'MOA+161:150.00'TAX+3+GST'MOA+161:476.10'TAX+4+TOT'MOA+161:626.10'GIS+B:134:143'AUT+IIENHO@GFGC@@DGM+40006206E'UNT+47+2184'";
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00002249";
			outgoingMessage.EM_LinkedObject = originalEntryHeader;

			var responseMessage = GetNZCMessage(EntryRejectionWithEntryNumberOf00000000);
			var processor = new MessageProcessor(new LoggingInformation());
			processor.ProcessMessage(responseMessage);
			AssertEquals("Declaration.DeclarationNumber", "", declaration.DeclarationNumber);
			AssertEquals("JE_EDITransmitDate should have been reset on original entry fail", ZDateTime.Empty, declaration.JE_EDITransmitDate);
		}

		public void TestMessageProcessorSetsTheAppropriateLinesIntoError()
		{
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				decCreator.MergeLineSetDutyAndTax(0, 325.00m, 728.75m);
				decCreator.MergeLineSetDutyAndTax(1, 325.00m, 728.75m);

				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					invoiceLine.JI_HadErrorInLastResponse = true;
				}
			}

			var entryLine1 = declaration.CusEntryHeader.MergedLines.FindByLineNumber(1);
			var invoiceLine1 = (JobComInvoiceLine)entryLine1.InvoiceLines[0];
			var entryLine2 = declaration.CusEntryHeader.MergedLines.FindByLineNumber(2);
			var invoiceLine2 = (JobComInvoiceLine)entryLine2.InvoiceLines[0];

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageWithErrorOnLine, InterpretedMessageWithErrorOnLine, expectedProcessMessageResult: true);
			AssertEquals("invoiceLine1.JI_HadErrorInLastResponse", true, invoiceLine1.JI_HadErrorInLastResponse);
			AssertEquals("invoiceLine2.JI_HadErrorInLastResponse", true, invoiceLine2.JI_HadErrorInLastResponse);

			using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.InvoiceLines.ResetHadErrorInLastResponse();
			}
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageWithErrorOnLine, InterpretedMessageWithErrorOnLine, expectedProcessMessageResult: true);
			AssertEquals("invoiceLine1.JI_HadErrorInLastResponse", true, invoiceLine1.JI_HadErrorInLastResponse);
			AssertEquals("invoiceLine2.JI_HadErrorInLastResponse", false, invoiceLine2.JI_HadErrorInLastResponse);
		}

		public void TestCashResponseDefaultsProperly()
		{
			declaration.JE_DeclarationReference = "B00001000";
			var messageCashPayment = GetNZCMessage(EDIFACTMessageDOCashPayment);
			var processor = new MessageProcessor(new LoggingInformation());

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			processor.ProcessMessage(messageCashPayment);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.CashPaidByBroker, declaration.JE_PaymentMethod);

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;
			processor.ProcessMessage(messageCashPayment);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.CashPaidByBroker, declaration.JE_PaymentMethod);

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByClient;
			processor.ProcessMessage(messageCashPayment);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.CashPaidByClient, declaration.JE_PaymentMethod);

			declaration.JE_PaymentMethod = PaymentMethodList.Codes.ClientDeferred;
			processor.ProcessMessage(messageCashPayment);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.CashPaidByClient, declaration.JE_PaymentMethod);

			var messageBrokerDeferred = GetNZCMessage(EDIFACTMessageDOBrokerDeferred);
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.ClientDeferred;
			processor.ProcessMessage(messageBrokerDeferred);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.BrokerDeferred, declaration.JE_PaymentMethod);

			var messageClientDeferred = GetNZCMessage(EDIFACTMessageDOClientDeferred);
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			processor.ProcessMessage(messageClientDeferred);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.ClientDeferred, declaration.JE_PaymentMethod);
		}

		public void TestPropertiesAreProperlyResetBetweenMessages()
		{
			declaration.JE_DeclarationReference = "BB0001109";
			var message1 = GetNZCMessage(EDIFACTMessageHasEntryNumberOf11111111);
			var processor = new MessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message1);
			AssertEquals("Declaration.DeclarationNumber", "11111111", declaration.DeclarationNumber);
			declaration.DeclarationNumber = "22222222";
			var message2 = GetNZCMessage(EDIFACTMessageHasEntryNumberOf00000000);
			processor.ProcessMessage(message2);
			AssertEquals("Declaration.DeclarationNumber", "22222222", declaration.DeclarationNumber);
		}

		public void TestICNFreeTextSavedToNotes()
		{
			declaration.JE_DeclarationReference = "BB0001109";
			var message = GetNZCMessage(EDIFACTMessageICNFreeText);
			var processor = new MessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals("MAF HOLD saved", "MAF HOLD.", declaration.CustomsDeliveryInstructions);
		}

		public void TestAdjustmentReceived()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageAdjustmentReceived, InterpretedMessageAdjustmentReceived, expectedProcessMessageResult: true);
		}

		public void TestAdjustmentReceived2()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageAdjustmentReceived2, InterpretedMessageAdjustmentReceived2, expectedProcessMessageResult: true);
		}

		public void TestDeliveryOrder()
		{
			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.5m;
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageDeliveryOrder, InterpretedMessageDeliveryOrder, expectedProcessMessageResult: true);
			AssertEquals("Delivery Instructions Should be there", "2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)", declaration.CustomsDeliveryInstructions);
		}

		public void TestDeliveryOrderLCL()
		{
			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 1460.5m;
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageDeliveryOrderLCL, InterpretedMessageDeliveryOrderLCL, expectedProcessMessageResult: true);
		}

		public void TestErrorState()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageErrorState, InterpretedMessageErrorState, expectedProcessMessageResult: true);
		}

		public void TestRejection1()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageRejection1, InterpretedMessageRejection1, expectedProcessMessageResult: true);
		}

		public void TestPoliceApproval()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessagePoliceApproval, InterpretedMessagePoliceApproval, expectedProcessMessageResult: true);
		}

		public void TestDeliveryOrderResponseTriggersAutoPrinting()
		{
			var entryQueue = GetNewPrintQueue("Entry Printer");
			var cusCertQueue = GetNewPrintQueue("CusCert Printer");
			var dOrderQueue = GetNewPrintQueue("DOrder Printer");

			var otherBranch = Factory.New<GlbBranch>();
			otherBranch.FillWithValidTestData();
			otherBranch.GB_Code = "D!2";
			otherBranch.GB_RL_NKHomePort = "NZAKL";
			otherBranch.GB_BranchName = "DUMMY Branch 2";
			otherBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			declaration.JE_GB = otherBranch.PK;
			Factory.Save();

			NZCustomsDataRegistry.Instance.CustomsCertificatePrinter.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, cusCertQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.CustomsCertificateCopies.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, 1);

			NZCustomsDataRegistry.Instance.EntryPrintPrinter.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, entryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.EntryPrintCopies.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, 2);

			NZCustomsDataRegistry.Instance.DeliveryOrderPrinter.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, dOrderQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.DeliveryOrderCopies.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, 4);

			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.5m;
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageDeliveryOrder, InterpretedMessageDeliveryOrder, expectedProcessMessageResult: true);

			declaration.Factory.Save();

			var cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 1, cusCertJobs.Count);
			AssertEquals("CusCertJobs[0].SP_Copies", 1, cusCertJobs[0].SP_Copies.ToZInt());
			var emailSubject = GlbCompany.CurrentCompany.GC_Name.ToUpper() + " - DUMMY Branch 2 - {0}";
			AssertEquals("Auto Print should use Declaration's branch", string.Format(emailSubject, "Customs Certificate for " + declaration.JE_DeclarationReference), cusCertJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, cusCertJobs[0].SP_GB);

			var entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 1, entryJobs.Count);
			AssertEquals("EntryJobs[0].SP_Copies", 2, entryJobs[0].SP_Copies.ToZInt());
			AssertEquals("Auto Print should use Declaration's branch", string.Format(emailSubject, "Customs Entry for " + declaration.JE_DeclarationReference), entryJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, entryJobs[0].SP_GB);

			var dOrderJobs = GetPrintJobs(dOrderQueue);
			AssertEquals("DOrderJobs.Count", 1, dOrderJobs.Count);
			AssertEquals("DOrderJobs[0].SP_Copies", 4, dOrderJobs[0].SP_Copies.ToZInt());
			AssertEquals("Auto Print should use Declaration's branch", string.Format(emailSubject, "Delivery Order - 47975057"), dOrderJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, dOrderJobs[0].SP_GB);
		}

		public void TestNonDeliveryOrderResponseDoesNotTriggerAutoPrinting()
		{
			var entryQueue = GetNewPrintQueue("Entry Printer");
			var cusCertQueue = GetNewPrintQueue("CusCert Printer");
			var dOrderQueue = GetNewPrintQueue("DOrder Printer");
			Factory.Save();

			NZCustomsDataRegistry.Instance.CustomsCertificatePrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, cusCertQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.CustomsCertificateCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 1);

			NZCustomsDataRegistry.Instance.EntryPrintPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, entryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.EntryPrintCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 2);

			NZCustomsDataRegistry.Instance.DeliveryOrderPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dOrderQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.DeliveryOrderCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 4);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageErrorState, InterpretedMessageErrorState, expectedProcessMessageResult: true);

			var cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 0, cusCertJobs.Count);

			var entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 0, entryJobs.Count);

			var dOrderJobs = GetPrintJobs(dOrderQueue);
			AssertEquals("DOrderJobs.Count", 0, dOrderJobs.Count);
		}

		public void TestLoggingEntryCancelled()
		{
			declaration.JE_DeclarationReference = "B00001000";
			var message = GetNZCMessage(EDIFACTMessageEntryCancelled);
			var processor = new MessageProcessor(new LoggingInformation());
			var originalHeader = declaration.CusEntryHeader;
			AssertEquals("CH_IsActive", true, originalHeader.CH_IsActive);
			AssertEquals("CH_IsEntryCancelled", false, originalHeader.CH_IsEntryCancelled);
			AssertEquals("CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, originalHeader.CH_EntryStatus);
			AssertNull("Should not have cancelledEvent Log", originalHeader.Logs.MostRecentLogByEventTime(Events.Cancelled));
			processor.ProcessMessage(message);
			AssertEquals("CH_IsActive", false, originalHeader.CH_IsActive);
			AssertEquals("CH_IsEntryCancelled", true, originalHeader.CH_IsEntryCancelled);
			AssertEquals("CH_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, originalHeader.CH_EntryStatus);
			var cancelledEvent = originalHeader.Logs.MostRecentLogByEventTime(Events.Cancelled);
			AssertNotNull("Should have cancelledEvent Log", cancelledEvent);
			AssertEquals("SL_Reference", "47975057", cancelledEvent.SL_Reference);
		}

		public void TestCH_IsRestored()
		{
			string message1 =
@"UNH+1+CUSRES:D:96B:UN+S00006834'
BGM+932+31737885:01'
FTX+DIN+++MAF CLEARANCE GIVEN TO MOVE FROM WHARF TO AN ATF. 1 FCL(S) SAID TO CON:TAIN 619 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:11522.33'
GIS+D:134:143'
UNT+8+1'
";

			string message2 =
@"UNH+1+CUSRES:D:96B:UN+S00006834'
BGM+962+31737885:02'
FTX+ACD+++MAF CLEARANCE GIVEN TO MOVE FROM WHARF TO AN ATF. ADJUSTMENT RECEIVED,: CLEARANCE / APPROVAL BY CUSTOMS OFFICER REQUIRED.'
GIS+805:120:143'
UNT+5+1'
";

			string message3 =
@"UNH+1+CUSRES:D:96B:UN+S00006834'
BGM+962+31737885:03'
GIS+814:120:143'
UNT+4+1'
";

			string message4 =
@"
UNH+1+CUSRES:D:96B:UN+S00006834'
BGM+962+31737885:04'
GIS+815:120:143'
UNT+4+1'
";

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			ProcessMessage(message1);
			ProcessMessage(message2);
			ProcessMessage(message3);
			AssertEquals(false, declaration.CusEntryHeader.CH_IsRestored);
			ProcessMessage(message4);
			AssertEquals(true, declaration.CusEntryHeader.CH_IsRestored);
		}

		void ProcessMessage(string messageText)
		{
			var message = GetNZCMessage(messageText);

			messageProcessor.ProcessMessage(message);
		}

		#region Messages and Interpreted Messages
		#region MessageEntryCancelled
		const string EDIFACTMessageEntryCancelled =
@"UNH+16781+CUSRES:D:96B:UN+B00001000'
BGM+962+47975057:04'
GIS+814:120:143'
UNT+4+16781'
";
		const string InterpretedMessageEntryCancelled =
@"[Entry Cancelled] Response for Customs Declaration: B00001000

Inspections/Audit Requirements
----------------------------------------------------------------------
Job Number     : B00001000
Master Bill    : 081-11111111
Entry Type     : Import (Normal)
Entry Number   : 47975057
Message No     : 16781

Message Status : (814) Entry Cancelled.
";
		#endregion
		#region MessageEntryRestored
		const string EDIFACTMessageEntryRestored =
@"UNH+16779+CUSRES:D:96B:UN+B00001000'
BGM+962+47975057:05'
GIS+815:120:143'
UNT+4+16779'
";
		const string InterpretedMessageEntryRestored =
@"[Entry Restored] Response for Customs Declaration: B00001000

Inspections/Audit Requirements
----------------------------------------------------------------------
Job Number     : B00001000
Master Bill    : 081-11111111
Entry Type     : Import (Normal)
Entry Number   : 47975057
Message No     : 16779

Message Status : (815) Entry Restored.
";
		#endregion
		#region MessageWithErrorOnLine
		const string EDIFACTMessageWithErrorOnLine =
@"UNH+2400+CUSRES:D:96B:UN+B00001000'
BGM+963+04481317:02'
GIS+801:120:143'
ERP+002:1:420'
ERC+378::143'
ERP+002:1:450'
ERC+405::143'
UNT+8+2400'
";
		const string InterpretedMessageWithErrorOnLine =
@"[Entry Rejected] Response for Customs Declaration: B00001000

Error Report
----------------------------------------------------------------------
Job Number     : B00001000
Master Bill    : 081-11111111
Entry Type     : Import (Normal)
Entry Number   : 04481317
Message No     : 2400

Message Status : (801) Entry Rejected.

Message Errors
----------------------------------------------------------------------
**Error** in Line, Merged Line 1, Import Duty (applicable to import tariff items):-
  Duty Payable : Error in calculation.
**Error** in Line, Merged Line 1, GST Payable:-
  GST Payable : Error in calculation.
";
		#endregion
		#region MessageDOCashPayment
		const string EDIFACTMessageDOCashPayment = @"UNH+293602+CUSRES:D:96B:UN+B00001000'
BGM+932+47975057:01'
FTX+DIN+++2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:11319.5'
GIS+C:134:143'
UNT+8+293602'
";
		#endregion
		#region MessageDOBrokerDeferred
		const string EDIFACTMessageDOBrokerDeferred = @"UNH+293602+CUSRES:D:96B:UN+B00001000'
BGM+932+47975057:01'
FTX+DIN+++2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:11319.5'
GIS+B:134:143'
UNT+8+293602'
";
		const string InterpretedMessageDOBrokerDeferred =
@"[Delivery Order Received] Response for Customs Declaration: B00001000

Delivery Order
----------------------------------------------------------------------
Job Number     : B00001000
Master Bill    : 081-11111111
Entry Type     : Import (Normal)
Entry Number   : 47975057
Message No     : 293602

Message Status : (819) Delivery Order Herewith.
                 Method of Payment as Specified.

Amount Returned: $11,319.50
Terms          : Broker Deferred

Delivery Instructions
----------------------------------------------------------------------
2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)
";
		#endregion
		#region MessageDOClientDeferred
		const string EDIFACTMessageDOClientDeferred = @"UNH+293602+CUSRES:D:96B:UN+B00001000'
BGM+932+47975057:01'
FTX+DIN+++2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:11319.5'
GIS+D:134:143'
UNT+8+293602'
";
		#endregion
		#region MessageHasEntryNumberOf11111111
		const string EDIFACTMessageHasEntryNumberOf11111111 =
@"UNH+77913+CUSRES:D:96B:UN+BB0001109'
BGM+965+11111111:04'
FTX+ICN+++MAF HOLD.'
GIS+830:120:143'
UNT+5+77913'
";
		#endregion
		#region MessageHasEntryNumberOf00000000
		const string EDIFACTMessageHasEntryNumberOf00000000 =
@"UNH+77913+CUSRES:D:96B:UN+BB0001109'
BGM+965+00000000:05'
FTX+ICN+++MAF HOLD.'
GIS+830:120:143'
UNT+5+77913'
";
		#endregion
		#region EntryRejectionWithEntryNumberOf00000000
		const string EntryRejectionWithEntryNumberOf00000000 =
@"UNH+293874+CUSRES:D:96B:UN+B00002249'
BGM+962+00000000:01'
FTX+ICN+++FOR BEEF PRODUCTS ENTER BEF AND YOUR MHF PERMIT NUMBER. FOR NON-BEEF P:RODUCTS ENTER NBF.'
GIS+840:120:143'
UNT+5+293874'";
		#endregion
		#region MessageICNFreeText
		const string EDIFACTMessageICNFreeText =
@"UNH+77913+CUSRES:D:96B:UN+BB0001109'
BGM+965+35409333:04'
FTX+ICN+++MAF HOLD.'
GIS+830:120:143'
UNT+5+77913'
";

		#endregion
		#region MessageAdjustmentReceived
		const string EDIFACTMessageAdjustmentReceived =
@"UNH+293870+CUSRES:D:96B:UN+207115'
BGM+962+50294349:04'
FTX+ICN+++ADJUSTMENT RECEIVED, CLEARANCE / APPROVAL BY CUSTOMS OFFICER REQUIRED.'
GIS+805:120:143'
UNT+5+293870'
";
		const string InterpretedMessageAdjustmentReceived =
@"[Inspections/Audit Requirements] Response for Customs Declaration: B00001000

Inspections/Audit Requirements
----------------------------------------------------------------------
Job Number     : 207115
Entry Type     : Export (Normal)
Entry Number   : 50294349
Message No     : 293870

Message Status : (805) Entry Held.
                 Instructions as Specified.

Customs Instructions
----------------------------------------------------------------------
ADJUSTMENT RECEIVED, CLEARANCE / APPROVAL BY CUSTOMS OFFICER REQUIRED.
";
		#endregion
		#region MessageAdjustmentReceived2
		const string EDIFACTMessageAdjustmentReceived2 =
@"UNH+293859+CUSRES:D:96B:UN+207115'
BGM+965+50294349:02'
GIS+830:120:143'
UNT+4+293859'
";
		const string InterpretedMessageAdjustmentReceived2 =
@"[Adjustment Accepted] Response for Customs Declaration: B00001000

Confirmation of Adjustment
----------------------------------------------------------------------
Job Number     : 207115
Entry Type     : Export (Normal)
Entry Number   : 50294349
Message No     : 293859

Message Status : (830) Adjustment Accepted.
";
		#endregion
		#region MessageDeliveryOrder
		public const string EDIFACTMessageDeliveryOrder =
@"UNH+293602+CUSRES:D:96B:UN+B01001001'
BGM+932+47975057:01'
FTX+DIN+++2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:11319.5'
GIS+D:134:143'
UNT+8+293602'
";
		const string InterpretedMessageDeliveryOrder =
@"[Delivery Order Received] Response for Customs Declaration: B00001000

Delivery Order
----------------------------------------------------------------------
Job Number     : B01001001
Entry Type     : Export (Normal)
Entry Number   : 47975057
Message No     : 293602

Message Status : (819) Delivery Order Herewith.
                 Method of Payment as Specified.

Amount Returned: $11,319.50
Terms          : Client Deferred

Delivery Instructions
----------------------------------------------------------------------
2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)
";
		#endregion
		#region MessageDeliveryOrderLCL
		const string EDIFACTMessageDeliveryOrderLCL =
@"UNH+293873+CUSRES:D:96B:UN+206140'
BGM+932+42766464:01'
FTX+DIN+++1 LOOSE PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:1460.5'
GIS+D:134:143'
UNT+8+293873'
";
		const string InterpretedMessageDeliveryOrderLCL =
@"[Delivery Order Received] Response for Customs Declaration: B00001000

Delivery Order
----------------------------------------------------------------------
Job Number     : 206140
Entry Type     : Export (Normal)
Entry Number   : 42766464
Message No     : 293873

Message Status : (819) Delivery Order Herewith.
                 Method of Payment as Specified.

Amount Returned: $1,460.50
Terms          : Client Deferred

Delivery Instructions
----------------------------------------------------------------------
1 LOOSE PACKAGE(S) OR ITEM(S)
";
		#endregion
		#region MessageErrorState
		const string EDIFACTMessageErrorState =
@"UNH+293860+CUSRES:D:96B:UN+207115'
BGM+963+50294349:03'
GIS+837:120:143'
ERP+001::115'
ERC+107::143'
ERP+003::120'
ERC+381::143'
ERP+003::135'
ERC+401::143'
ERP+003::145'
ERC+289::143'
ERP+001::2'
ERC+591::143'
UNT+14+293860'
";
		const string InterpretedMessageErrorState =
@"[Entry in Error] Response for Customs Declaration: B00001000

Error Report
----------------------------------------------------------------------
Job Number     : 207115
Entry Type     : Export (Normal)
Entry Number   : 50294349
Message No     : 293860

Message Status : (837) Adjustment has Placed Entry in Error State.

Message Errors
----------------------------------------------------------------------
**Error** in Header, Total Gross Weight:-
  Total Gross Weight : Should be greater than aggregate of Stats Quantity.
**Error** in Summary, Total Value in NZ Dollars:-
  Total Value in NZ Dollars : Error in calculation.
**Error** in Summary, Total GST:-
  Total GST : Error in calculation.
**Error** in Summary, Total Payable (Total Amount on Import and Excise entries):-
  Total Amount : Error in calculation.
**Error** in Header, Entry Number:-
  EDI Adjustment - Assessment must be verified by Customs.";
		#endregion
		#region MessageRejection1
		const string EDIFACTMessageRejection1 =
@"UNH+293874+CUSRES:D:96B:UN+B00001000'
BGM+962+41739019:01'
FTX+ICN+++FOR BEEF PRODUCTS ENTER BEF AND YOUR MHF PERMIT NUMBER. FOR NON-BEEF P:RODUCTS ENTER NBF.'
GIS+840:120:143'
UNT+5+293874'";
		const string InterpretedMessageRejection1 =
@"[Entry Rejected] Response for Customs Declaration: B00001000

Inspections/Audit Requirements
----------------------------------------------------------------------
Job Number     : B00001000
Entry Type     : Export (Normal)
Entry Number   : 41739019
Message No     : 293874

Message Status : (840) Entry Rejected.
                 Reasons as Specified.

Customs Instructions
----------------------------------------------------------------------
FOR BEEF PRODUCTS ENTER BEF AND YOUR MHF PERMIT NUMBER. FOR NON-BEEF P
RODUCTS ENTER NBF.";
		#endregion
		#region MessagePoliceApproval
		const string EDIFACTMessagePoliceApproval =
@"UNH+293712+CUSRES:D:96B:UN+B00001000'
BGM+962+05909310:01'
FTX+ICN+++POLICE APPROVAL IS REQUIRED TO IMPORT THESE GOODS. PLEASE PRESENT YOUR: APPROVAL TO CUSTOMS.'
GIS+806:120:143'
UNT+5+293712'
";
		const string InterpretedMessagePoliceApproval =
@"[Inspections/Audit Requirements] Response for Customs Declaration: B00001000

Inspections/Audit Requirements
----------------------------------------------------------------------
Job Number     : B00001000
Entry Type     : Export (Normal)
Entry Number   : 05909310
Message No     : 293712

Message Status : (806) Entry Routed to Client Service - Documents Required as Specified.

Customs Instructions
----------------------------------------------------------------------
POLICE APPROVAL IS REQUIRED TO IMPORT THESE GOODS. PLEASE PRESENT YOUR
 APPROVAL TO CUSTOMS.
";
		#endregion
		#endregion

		#region Implementation

		StmPrintJobCollection GetPrintJobs(StmPrintQueue printQueue)
		{
			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK);
			var entryPrintJobs = new StmPrintJobCollection(Factory, filter);
			entryPrintJobs.Load();
			return entryPrintJobs;
		}

		StmPrintQueue GetNewPrintQueue(ZString displayName)
		{
			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_AllowPrinting = true;
			printQueue.SQ_DisplayName = displayName;
			printQueue.SQ_QueueName = @"\\PrintServer\" + displayName;
			printQueue.SQ_PrintLanguage = "ESP";
			printQueue.SQ_Scale = 100m;
			printQueue.SQ_RowScale = 100m;
			printQueue.SQ_ColumnScale = 100m;
			printQueue.SQ_ServerName = "PRINTSERVER";
			return printQueue;
		}

		protected override void SetupMessageProcessor(LoggingInformation logger)
		{
			messageProcessor = new MessageProcessor(logger);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var jobDecFactory = new BusinessObjectFactory();
			declaration = Business.Declaration.JobDeclaration.New(jobDecFactory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
		}

		protected new MessageProcessor messageProcessor
		{
			get { return (MessageProcessor)base.messageProcessor; }
			set { base.messageProcessor = value; }
		}

		class MessageProcessor_ForTesting : MessageProcessor
		{
			public MessageProcessor_ForTesting(LoggingInformation logger) : base(logger)
			{
			}

			public void Call_SetupPropertiesForMessageProcessing(NZCMessage message)
			{
				base.SetupPropertiesForMessageProcessing(message);
			}

			public ZGuid Get_AcknowledgementEmailGroup() => AcknowledgementEmailGroup;
			public ZString Get_AcknowledgementEmailMode() => AcknowledgementEmailMode;
			public ZGuid Get_ImpedimentEmailGroup() => ImpedimentEmailGroup;
			public ZString Get_ImpedimentEmailMode() => ImpedimentEmailMode;
			public ZGuid Get_ErrorEmailGroup() => ErrorEmailGroup;
			public ZString Get_ErrorEmailMode() => ErrorEmailMode;

			public ZGuid Get_UnsolicitedDOGroup() => UnsolicitedDOGroup;

			public ZString Get_UnsolicitedDOMode() => UnsolicitedDOMode;
		}
		#endregion
	}
}
