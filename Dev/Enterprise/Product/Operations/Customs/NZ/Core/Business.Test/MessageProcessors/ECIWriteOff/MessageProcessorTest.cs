using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Registry;
using Constants = Enterprise.Core.Constants;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;
using NZCMessage = Enterprise.Customs.NZ.Business.Declaration.NZCMessage;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.ECIWriteOff.Testing
{
	class MessageProcessorTest : MessageProcessors.Testing.MessageProcessorForEntryHeaderTest
	{
		public void TestEmailGroupUserSettings()
		{
			Guid importAckGroup = Guid.NewGuid();
			ZString importAckMode = Constants.EmailTo.NoEmails;
			Guid importImpGroup = Guid.NewGuid();
			ZString importImpMode = Constants.EmailTo.StaffMember;
			Guid importErrGroup = Guid.NewGuid();
			ZString importErrMode = Constants.EmailTo.NominatedGroup;

			Guid exportAckGroup = Guid.NewGuid();
			ZString exportAckMode = Constants.EmailTo.StaffMember;
			Guid exportImpGroup = Guid.NewGuid();
			ZString exportImpMode = Constants.EmailTo.NominatedGroup;
			Guid exportErrGroup = Guid.NewGuid();
			ZString exportErrMode = Constants.EmailTo.StaffMemberAndNominatedGroup;

			NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importAckGroup);
			NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importAckMode);
			NZCustomsDataRegistry.Instance.ImportEciSendImpedimentsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importImpGroup);
			NZCustomsDataRegistry.Instance.ImportEciSendImpediments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importImpMode);
			NZCustomsDataRegistry.Instance.ImportEciSendErrorsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importErrGroup);
			NZCustomsDataRegistry.Instance.ImportEciSendErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importErrMode);

			NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportAckGroup);
			NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportAckMode);
			NZCustomsDataRegistry.Instance.ExportEciSendImpedimentsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportImpGroup);
			NZCustomsDataRegistry.Instance.ExportEciSendImpediments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportImpMode);
			NZCustomsDataRegistry.Instance.ExportEciSendErrorsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportErrGroup);
			NZCustomsDataRegistry.Instance.ExportEciSendErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportErrMode);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			NZCMessage message = declaration.CusEntryHeader.Messages.AddNew();
			MessageProcessor_ForTesting processor = new MessageProcessor_ForTesting(new LoggingInformation());
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
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			decCreator.MergeDeclaration();
			factory.Save();
			declaration.JE_DeclarationReference = "B00001189";
			factory.Save();

			var message = SetupNZCMessage(@"UNH+1+CUSRES:D:96B:UN+B00001189'
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
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Entry Number", "19369111", declaration.CusEntryHeader.EntryNumber);

			message = SetupNZCMessage(@"UNH+1+CUSRES:D:98A:UN+B00001189'BGM+963'GIS+841:120:143'ERP+001::044'ERC+487::143'UNT+6+1'", factory.New<NZCMessage>());

			factory.Save();
			processor = new DeclarationDelegator();
			Assert(processor.CanProcess(message));
			processor.Process(new LoggingInformation(), message);
			factory.Save();
			declaration.CusEntryHeader.Reload();
			AssertEquals("Should not change the status according to the response for a different entry number", "CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Entry Number", "19369111", declaration.CusEntryHeader.EntryNumber);
			Assert("still active", declaration.CusEntryHeader.IsActive);

			var inactivatedEntry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => !x.IsActive);
			AssertNotNull(inactivatedEntry);
			AssertEquals("Entry number is saved to the inactivated entry", "", inactivatedEntry.EntryNumber);
			AssertEquals("status error", "REJ", inactivatedEntry.CH_EntryStatus);
		}

		public void TestErrorInLineGetsReflectedBackToEmailSubject()
		{
			NZCMessage cancelMessage = declaration.CusEntryHeader.Messages.AddNew();
			cancelMessage.EM_MessageSubType = NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Cancellation;
			cancelMessage.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			cancelMessage.EM_MessageText = NZCMessage.SendersReferencePlaceHolder + NZCMessage.MessageNumberPlaceHolder;
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageLineErrorNoConsignmentsWrittenOff, InterpretedMessageLineErrorNoConsignmentsWrittenOff, expectedProcessMessageResult: true);
			AssertEquals("Resulting Last Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Resulting Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_EntryStatus);
			AssertEquals("Resulting Manifest Status", LowValueManifestStatusList.Codes.ManifestInError, declaration.CusEntryHeader.CH_EntryStatus);
		}

		public void TestCancellationResponsePutsOnACancellationStatus()
		{
			Declaration.CusEntryHeader entryHeader = declaration.CusEntryHeader;
			NZCMessage cancelMessage = entryHeader.Messages.AddNew();
			cancelMessage.EM_MessageSubType = NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Cancellation;
			cancelMessage.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			cancelMessage.EM_MessageText = NZCMessage.SendersReferencePlaceHolder + NZCMessage.MessageNumberPlaceHolder;
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageCancellationResponse, InterpretedMessageCancellationResponse, expectedProcessMessageResult: true);
			AssertEquals("Declaration.JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.ConsignmentCancelled, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Declaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentCancelled, declaration.JE_EntryStatus);
			AssertEquals("entryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.ManifestCancelled, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.CH_IsActive", false, entryHeader.CH_IsActive);
		}

		public void TestProcessWriteOffResponseAndDeclarationNumber()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageSingleWriteOff, InterpretedMessageSingleWriteOff, expectedProcessMessageResult: true);
			AssertEquals("Resulting Last Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Resulting Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("Resulting Manifest Status", LowValueManifestStatusList.Codes.ManifestAccepted, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("98279024", declaration.DeclarationNumber);
			AssertEquals(1, messageProcessor.AcknowledgementEmailSendCount);
			AssertEquals("Delivery Instructions Should be there", "Response Status: WOF-Consignment Written Off/Cleared", declaration.CustomsDeliveryInstructions);
		}

		public void TestProcessErrorResponse()
		{
			AssertProcessErrorResponse(EDIFACTMessageHeaderError, InterpretedMessageHeaderError);
		}

		void AssertProcessErrorResponse(string eDIFACTMessage, string interpretedMessage)
		{
			ProcessMessageAndCompareAgainstExpectedResult(eDIFACTMessage, interpretedMessage, expectedProcessMessageResult: true);
			AssertEquals("Resulting Last Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Resulting Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_EntryStatus);
			AssertEquals("Resulting Manifest Status", LowValueManifestStatusList.Codes.ManifestRejected, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("", declaration.DeclarationNumber);
			AssertEquals("Emails Sent", 1, messageProcessor.StatusEmailSendCount);
			AssertEquals("Delivery Instructions Should be there", "Response Status: ERR-Consignment In Error", declaration.CustomsDeliveryInstructions);
		}

		public void TestProcessErrorResponse_248_249_250_252_697_701()
		{
			var codes = new Dictionary<string, string>(6);
			codes.Add("248", @"**Error** in Header, Unknown Field Number: ZZZ:-
  LOU - single use LOU has already been used.");
			codes.Add("249", @"**Error** in Header, Unknown Field Number: ZZZ:-
  Tariff Item incompatible with LOU quoted.");
			codes.Add("250", @"**Error** in Header, Unknown Field Number: ZZZ:-
  Value exceeds limit for LOU quoted.");
			codes.Add("252", @"**Error** in Header, Unknown Field Number: ZZZ:-
  Other Information code not valid for this entry type.");
			codes.Add("697", @"**Error** in Header, Unknown Field Number: ZZZ:-
  Other Information Code specified not for this client code.");
			codes.Add("701", @"**Error** in Header, Unknown Field Number: ZZZ:-
  Entry requires verification by a Customs Officer.");
			var eDIFACTMessageBuilder = new ZStringBuilder(@"UNH+2447+CUSRES:D:98A:UN+B01001001'
BGM+963+00000000'
GIS+841:120:143'");
			var interpretedMessageBuilder = new ZStringBuilder(@"[ICR/CRE Rejected] Response for ECI Write-Off: B01001001

ECI Rejection Report
----------------------------------------------------------------------
ECI Write-Off  : B01001001
Entry Number   : 00000000
Message No     : 2447

Message Status : (841) ECI Rejected.
                 Error Report Attached.

Message Errors
----------------------------------------------------------------------");

			foreach (var pair in codes)
			{
				eDIFACTMessageBuilder.Append("ERP+001::ZZZ'");
				eDIFACTMessageBuilder.Append(string.Format("ERC+{0}::143'", pair.Key));
				interpretedMessageBuilder.Append(pair.Value);
			}
			eDIFACTMessageBuilder.Append(string.Format("UNT+{0}+2447'", codes.Count * 2));
			AssertProcessErrorResponse(eDIFACTMessageBuilder.ToStringWithNewLineBetweenAppends(), interpretedMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		public void TestProcessLineErrorResponse()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageLineErrorSomeConsignmentsMayBeWrittenOff, InterpretedMessageLineErrorSomeConsignmentsMayBeWrittenOff, expectedProcessMessageResult: true);
			AssertEquals("Resulting Last Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Resulting Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_EntryStatus);
			AssertEquals("Resulting Manifest Status", LowValueManifestStatusList.Codes.ManifestInError, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Emails Sent", 1, messageProcessor.StatusEmailSendCount);
			AssertEquals("Delivery Instructions Should be there", "Response Status: ERR-Consignment In Error", declaration.CustomsDeliveryInstructions);
		}

		public void TestProcessFormalEntryRequiredResponse()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageFormalEntryRequired, InterpretedMessageFormalEntryRequired, expectedProcessMessageResult: true);
			AssertEquals("Resulting Last Consignment Status", LowValueConsignmentStatusList.Codes.FormalDeclarationRequired, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Resulting Consignment Status", LowValueConsignmentStatusList.Codes.FormalDeclarationRequired, declaration.JE_EntryStatus);
			AssertEquals("Resulting Manifest Status", LowValueManifestStatusList.Codes.ManifestAccepted, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Emails Sent", 1, messageProcessor.StatusEmailSendCount);
			AssertEquals("Delivery Instructions Should be there", "Response Status: FEQ-Formal Declaration Required", declaration.CustomsDeliveryInstructions);
		}

		public void TestProcessFreeTextResponse()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageWithFreeTextInformation, InterpretedMessageWithFreeTextInformation, expectedProcessMessageResult: true);
			AssertEquals("Resulting Last Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Resulting Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_EntryStatus);
			AssertEquals("Resulting Manifest Status", LowValueManifestStatusList.Codes.ManifestInError, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Emails Sent", 1, messageProcessor.StatusEmailSendCount);
			AssertEquals("Delivery Instructions Should be there", "Response Status: ERR-Consignment In Error\r\nTHIS IS A TEST FOR LINE 1\r\nTHIS IS A TEST FOR LINE 3", declaration.CustomsDeliveryInstructions);
		}

		public void TestNoConsignmentsWrittenOffAfterErrorResponse()
		{
			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageLineErrorSomeConsignmentsMayBeWrittenOff, InterpretedMessageLineErrorSomeConsignmentsMayBeWrittenOff, expectedProcessMessageResult: true);
			AssertEquals("Resulting Last Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Resulting Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_EntryStatus);
			AssertEquals("Resulting Manifest Status", LowValueManifestStatusList.Codes.ManifestInError, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Emails Sent", 1, messageProcessor.StatusEmailSendCount);
			AssertEquals("Delivery Instructions Should be there", "Response Status: ERR-Consignment In Error", declaration.CustomsDeliveryInstructions);

			ProcessMessageAndCompareAgainstExpectedResult(EDIFACTMessageFormalEntryRequired, InterpretedMessageFormalEntryRequired, expectedProcessMessageResult: true);
			AssertEquals("Resulting Last Consignment Status", LowValueConsignmentStatusList.Codes.FormalDeclarationRequired, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Resulting Consignment Status", LowValueConsignmentStatusList.Codes.FormalDeclarationRequired, declaration.JE_EntryStatus);
			AssertEquals("Resulting Manifest Status", LowValueManifestStatusList.Codes.ManifestAccepted, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Emails Sent", 2, messageProcessor.StatusEmailSendCount);
			AssertEquals("Delivery Instructions Should be there", "Response Status: FEQ-Formal Declaration Required", declaration.CustomsDeliveryInstructions);
		}

		public void TestFEQAfterFormalEntryDORDoesNotOverrideStatus()
		{
			Declaration.CusEntryHeader entryHeader = declaration.CusEntryHeader;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			declaration.JE_DeclarationReference = "M00064718";
			NZCMessage eciMessage = entryHeader.Messages.AddNew();
			eciMessage.EM_MessageSubType = NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original;
			eciMessage.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			eciMessage.EM_MessageText = EciManifest;

			var factory = declaration.Factory;
			var response1 = SetupNZCMessage(EciManifestResponse, factory.New<NZCMessage>());

			var processor = new DeclarationDelegator();
			Assert(processor.CanProcess(response1));
			processor.Process(new LoggingInformation(), response1);
			factory.Save();

			AssertEquals("Resulting Last Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Resulting Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("Resulting Manifest Status", LowValueManifestStatusList.Codes.ManifestAccepted, declaration.CusEntryHeader.CH_EntryStatus);

			var response2 = SetupNZCMessage(EciManifestResponse2, factory.New<NZCMessage>());
			Assert(processor.CanProcess(response2));
			processor.Process(new LoggingInformation(), response2);
			factory.Save();

			declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Formal entry CusEntryHeader should have been created - CH_EntryStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			entryHeader = declaration.CusEntryHeader;
			NZCMessage cusdecMessage = entryHeader.Messages.AddNew();
			cusdecMessage.EM_MessageSubType = NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original;
			cusdecMessage.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			cusdecMessage.EM_MessageText = formalMessage;
			factory.Save();

			var response3 = SetupNZCMessage(@"UNH+1+CUSRES:D:96B:UN+M00064718'
BGM+932+89975802:01'
FTX+DIN+++1 LOOSE PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:625.89'
GIS+B:134:143'
UNT+8+1'", factory.New<NZCMessage>());

			factory.Save();
			processor = new DeclarationDelegator();
			Assert(processor.CanProcess(response3));
			processor.Process(new LoggingInformation(), response3);
			factory.Save();
			declaration.CusEntryHeader.Reload();
			AssertEquals("DOR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Entry Number", "89975802", declaration.CusEntryHeader.EntryNumber);
			AssertEquals("Resulting Entry Status", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("Formal (Active entry header) Status", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.CusEntryHeader.CH_EntryStatus);

			var response4 = SetupNZCMessage(@"UNH+1+CUSRES:D:98A:UN+M00064718'
BGM+932+93345527'
GIS+842:120:143'
DOC+WOF:148:143+14::170935973'
CNT+10:1'
UNT+6+1'", factory.New<NZCMessage>());   // Late ECI Write Off message received after Formal declaration made

			factory.Save();
			processor = new DeclarationDelegator();
			Assert(processor.CanProcess(response4));
			processor.Process(new LoggingInformation(), response4);
			factory.Save();
			declaration.CusEntryHeader.Reload();

			AssertEquals("Formal Entry Status should not be overridden by processing of a late write off message", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("Formal (Active entry header) Status should remain unchanged", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.CusEntryHeader.CH_EntryStatus);
		}

		#region MessageFormalEntryRequired
		const string EDIFACTMessageFormalEntryRequired =
@"UNH+2449+CUSRES:D:98A:UN+B01001001'
BGM+932+98279024'
GIS+844:120:143'
CNT+10:0'
UNT+5+2449'
";
		const string InterpretedMessageFormalEntryRequired =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B01001001

ECI Write-Off Report
----------------------------------------------------------------------
ECI Write-Off  : B01001001
Entry Number   : 98279024
Message No     : 2449

Message Status : (844) ECI Received OK.
                 No Consignments were Written Off.

Job Responses
----------------------------------------------------------------------
Job Number: B01001001   House Bill: HOUSETEST123
--- Clearance Status: FEQ-Formal Declaration Required ---";
		#endregion
		#region MessageHeaderError
		const string EDIFACTMessageHeaderError =
@"UNH+2447+CUSRES:D:98A:UN+B01001001'
BGM+963+00000000'
GIS+841:120:143'
ERP+001::80'
ERC+677::143'
ERP+001::505'
ERC+672::143'
UNT+8+2447'
";
		const string InterpretedMessageHeaderError =
@"[ICR/CRE Rejected] Response for ECI Write-Off: B01001001

ECI Rejection Report
----------------------------------------------------------------------
ECI Write-Off  : B01001001
Entry Number   : 00000000
Message No     : 2447

Message Status : (841) ECI Rejected.
                 Error Report Attached.

Message Errors
----------------------------------------------------------------------
**Error** in Header, Craft/Flight No.:-
  Flight No : Not of file
     (https://www.customs.govt.nz/business/import/lodge-your-import-entry/craft-names-and-flight-numbers/
     for a full list of valid Flight Numbers and Vessel Names).
**Error** in Header, Date of Arrival/Departure:-
  Date of Arrival : ECI lodged too many days after import.";
		#endregion
		#region MessageLineErrorSomeConsignmentsMayBeWrittenOff
		const string EDIFACTMessageLineErrorSomeConsignmentsMayBeWrittenOff =
@"UNH+2360+CUSRES:D:98A:UN+B01001001'
BGM+932+79021367'
GIS+843:120:143'
DOC+ERR:148:143+1::HOUSETEST123'
ERP+2:1:400'
ERC+458::143'
CNT+10:0'
UNT+8+2360'
";
		const string InterpretedMessageLineErrorSomeConsignmentsMayBeWrittenOff =
@"[ICR/CRE in Error, Check Consignments for Status] Response for ECI Write-Off: B01001001

ECI Write-Off Report
----------------------------------------------------------------------
ECI Write-Off  : B01001001
Entry Number   : 79021367
Message No     : 2360

Message Status : (843) ECI Received With Errors.
                 Some Consignments May have been Written Off.

Job Responses
----------------------------------------------------------------------
Job Number: B01001001   House Bill: HOUSETEST123
--- Clearance Status: ERR-Consignment In Error ---
**Error** in Goods Details, Currency Code:-
  Currency Code : Not specified or invalid.
";
		#endregion
		#region MessageLineErrorNoConsignmentsWrittenOff
		const string EDIFACTMessageLineErrorNoConsignmentsWrittenOff =
@"UNH+3177+CUSRES:D:96B:UN+B01001001'
BGM+932+98279024'
GIS+844:120:143'
DOC+ERR:148:143+1::KLJHSFKJS'
ERP+2:1:355'
ERC+556::143'
CNT+10:0'
UNT+8+3177'
";
		const string InterpretedMessageLineErrorNoConsignmentsWrittenOff =
@"[ICR/CRE in Error, Check Consignments for Status] Response for ECI Write-Off: B01001001

ECI Write-Off Report
----------------------------------------------------------------------
ECI Write-Off  : B01001001
Entry Number   : 98279024
Message No     : 3177

Message Status : (844) ECI Received OK.
                 No Consignments were Written Off.

Job Responses
----------------------------------------------------------------------
Job Number: B01001001   House Bill: KLJHSFKJS
--- Clearance Status: ERR-Consignment In Error ---
**Error** in Goods Details, Goods Description:-
  Goods Description : Not specified.
";
		#endregion
		#region MessageCancellationResponse
		const string EDIFACTMessageCancellationResponse = @"UNH+2449+CUSRES:D:98A:UN+B01001001'
BGM+965+98279024'
GIS+830:120:143'
UNT+4+2449'
";
		const string InterpretedMessageCancellationResponse = @"[ICR/CRE Cancelled] Response for ECI Write-Off: B01001001

Confirmation Of Adjustment
----------------------------------------------------------------------
ECI Write-Off  : B01001001
Entry Number   : 98279024
Message No     : 2449

Message Status : (830) Adjustment Accepted. Entry is now CANCELLED.
";
		#endregion
		#region MessageSingleWriteOff
		public const string EDIFACTMessageSingleWriteOff =
@"UNH+2449+CUSRES:D:98A:UN+B01001001'
BGM+932+98279024'
GIS+842:120:143'
DOC+WOF:148:143+1::HOUSETEST123'
CNT+10:1'
UNT+6+2449'
";
		const string InterpretedMessageSingleWriteOff =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B01001001

ECI Write-Off Report
----------------------------------------------------------------------
ECI Write-Off  : B01001001
Entry Number   : 98279024
Message No     : 2449

Message Status : (842) ECI Received OK.
                 Consignments have been Written Off.

Job Responses
----------------------------------------------------------------------
Job Number: B01001001   House Bill: HOUSETEST123
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";
		#endregion
		#region MessageWithFreeTextInformation
		const string EDIFACTMessageWithFreeTextInformation =
@"UNH+2360+CUSRES:D:98A:UN+B01001001'
BGM+932+79021367'
FTX+ICN+++THIS IS A TEST FOR LINE 1::THIS IS A TEST FOR LINE 3'
GIS+843:120:143'
DOC+ERR:148:143+1::HOUSETEST123'
ERP+2:1:400'
ERC+458::143'
CNT+10:0'
UNT+8+2360'
";
		const string InterpretedMessageWithFreeTextInformation =
@"[ICR/CRE in Error, Check Consignments for Status] Response for ECI Write-Off: B01001001

ECI Write-Off Report
----------------------------------------------------------------------
ECI Write-Off  : B01001001
Entry Number   : 79021367
Message No     : 2360

Message Status : (843) ECI Received With Errors.
                 Some Consignments May have been Written Off.

Customs Instructions
----------------------------------------------------------------------
THIS IS A TEST FOR LINE 1
THIS IS A TEST FOR LINE 3

Job Responses
----------------------------------------------------------------------
Job Number: B01001001   House Bill: HOUSETEST123
--- Clearance Status: ERR-Consignment In Error ---
**Error** in Goods Details, Currency Code:-
  Currency Code : Not specified or invalid.";
		#endregion

		#region ECIMessageReceivedAfterFormalDeclaration

		const string EciManifest = @"UNH+441174+CUSCAR:D:98A:UN'
BGM+785+M00064718+9'
NAD+CA++AIR NEW ZEALAND'
TDT+20++4+++++:::NZ124'
LOC+22+NZAKL'
DTM+132:20140812:102'
GIS+10:105:143'
GIS+N:109:143'
CNT+10:86'
CNI+1'
RFF+HWB:108218275'
LOC+9+AUMEL'
LOC+11+NZAKL'
LOC+27+AU'
NAD+CN++IFF (NZ) LTD.:128 STODDARD ROAD:MT ROSKILL:MOUNT ROSKILL AUK 1004'
NAD+CZ++INTL FLAVOURS & FRAGRANCESAUST:310 FRANKSTON DANDENONG RD:DANDENONG VIC 3175'
GID+1+1:PK'
FTX+AAA+++DOCUMENTS'
MEA+WT+AAG+KGM:0.150'
MOA+14:0.00:NZD'
LOC+4+AUHLM'
CNI+2'
RFF+HWB:170887183'
LOC+9+AUMEL'
LOC+11+NZAKL'
LOC+27+AU'
LOC+7+NZHLZ'
NAD+CN++ATL BALANCING & DRIVELINE:32 COMMERCE STREET:HAMILTON WKO 3200'
NAD+CZ++HARDY SPICER - INTERNATIONAL:17-31 DISCOVERY RD:DANDENONG SOUTH VIC 3175'
GID+1+1:PK'
FTX+AAA+++UNIJOINTS'
MEA+WT+AAG+KGM:0.350'
MOA+14:29.42:AUD'
LOC+4+AUHLM'
CNI+14'
RFF+HWB:170935973'
LOC+9+AUMEL'
LOC+11+NZAKL'
LOC+27+AU'
NAD+CN++TOYWORLD:16 RURU STREET:CNR NIKAUST ST:EDEN TERRACE 1010'
NAD+CZ++TRANSTAR INTERNATIONAL FREIGHT:SUITE 1 14 WOODRUFF ST:PORT MELBOURNE VIC 3207'
GID+1+3:PK'
FTX+AAA+++SAMPLE TOYS /TRACTOR WAGON VEHICLEPLASTIC TOYS'
MEA+WT+AAG+KGM:22.200'
MOA+14:72.51:AUD'
LOC+4+AUHLM'
UNT+1074+441174'";

		const string EciManifestResponse = @"UNH+1+CUSRES:D:98A:UN+M00064718'
BGM+932+93345527'
GIS+842:120:143'
DOC+WOF:148:143+1::108218275'
DOC+WOF:148:143+2::170887183'
DOC+WOF:148:143+3::170896600'
DOC+HLD:148:143+14::170935973'
CNT+10:57'
UNT+63+1'";

		const string EciManifestResponse2 = @"UNH+1+CUSRES:D:98A:UN+M00064718'
BGM+962+93345527'
FTX+ICN+++LINE14 - AWB170935973 - AKE1414A7D - EXAM REQUIRED'
GIS+805:120:143'
UNT+5+1'";

		const string formalMessage = @"UNH+441289+CUSDEC:D:96B:UN'
BGM+929+M00064718-19+9'
CST++10:105:143'
LOC+9+AUMEL'
LOC+11+NZAKL'
LOC+41+NZAKL'
DTM+151:20140812:102'
MEA+WT+AAD+KGM:9'
RFF+MB:08612547091'
RFF+HWB:170950813'
PAC+1++PK'
TDT+20++4+++++:::NZ124'
NAD+AL+40017610J:ZZZ:143'
NAD+CB+40229586E:ZZZ:143'
UNS+D'
DMS+429043, 247, 522+935'
TOD+++CFR:106:143'
CST+1+9503000949G:169:143+640550K:184:143'
FTX+AAA+++REMOTE CONTROL TOYS AND WORKING MODELS, INCLUDING PARTS THEREOF,:EXCLUDING REMOT'
LOC+27+AU'
LOC+35+AU'
NAD+SU+00928005W:ZZZ:143'
MOA+14:3166.00:AUD'
CUX+2++0.91'
MOA+40:3479'
MOA+64:102'
MOA+70:9'
GIS+N:109:143'
TAX+1+GST'
MOA+161:538.50'
CST+2+8504400109D:169:143+997689C:184:143'
FTX+AAA+++NICAD, NICKEL METAL HYDRIDE OR LITHIUM BATTERY CHARGERS'
LOC+27+AU'
LOC+35+AU'
MEA+AAR++NMB:6.000'
NAD+SU+00928005W:ZZZ:143'
MOA+14:188.73:AUD'
CUX+2++0.91'
MOA+40:207'
MOA+64:6'
MOA+70:1'
GIS+N:109:143'
TAX+1+GST'
MOA+161:32.10'
CST+3+8507500000B:169:143+300840B:184:143'
FTX+AAA+++NICKEL-METAL HYDRIDE STORAGE BATTERY PACK'
LOC+27+AU'
LOC+35+AU'
MEA+AAR++NMB:1.000'
NAD+SU+00928005W:ZZZ:143'
MOA+14:49.00:AUD'
CUX+2++0.91'
MOA+40:54'
MOA+64:2'
MOA+70:0'
GIS+N:109:143'
TAX+1+GST'
MOA+161:8.40'
UNS+S'
CNT+4:1'
CNT+5:3'
CNT+11:1'
TAX+3+CUD++3740'
MOA+161:0.00'
TAX+3+GST'
MOA+161:579.00'
TAX+4+TOT'
MOA+161:579.00'
GIS+B:134:143'
AUT+LAACNNGIOEFJGD@H+40070045B'
UNT+71+441289'";

		#endregion

		#region Implementation
		protected override void SetupMessageProcessor(LoggingInformation logger)
		{
			messageProcessor = new MessageProcessor_ForTesting(logger);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BusinessObjectFactory jobDecFactory = new BusinessObjectFactory();
			declaration = JobDeclaration.New(jobDecFactory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_DeclarationReference = "B01001001";
			declaration.JE_HouseBill = "HOUSETEST123";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			jobDecFactory.Save();
		}

		protected new MessageProcessor_ForTesting messageProcessor
		{
			get { return (MessageProcessor_ForTesting)base.messageProcessor; }
			set { base.messageProcessor = value; }
		}

		public class MessageProcessor_ForTesting : MessageProcessor
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
		}
		#endregion
	}
}
