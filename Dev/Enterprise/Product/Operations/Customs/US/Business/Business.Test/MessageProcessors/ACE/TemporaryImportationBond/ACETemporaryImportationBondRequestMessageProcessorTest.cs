using System;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ACETemporaryImportationBondRequestMessageProcessorTest : ABIProcessorTest<ACETemporaryImportationBondRequestMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestTIBExpiryDateExtention()
		{
			declaration.JE_DateOfArrival = new DateTime(2020, 02, 10);
			entryHeader.US_TIBExpiryDate = entryHeader.JE_DateOfArrival.AddYears(1);
			incomingMessage.EM_MessageText =
"B018888XJ5TX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: XJ5 23456789                                           " +
"E1RF995 EXT GRANTED SUBJECT TO REVIEW             XJ5  23456789     B00000001   " +
"Y  8888XJ5X100007";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			entryHeader.Reload();
			AssertEquals("Update TIB Expiry Date and Add 1 Year.", "10-Feb-22 00:00:00", entryHeader.US_TIBExpiryDate.ToString());

			var incomingMessage2 = Factory.New<MQEDIMessage>();
			incomingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_MessageNum = "~15000";
			incomingMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse;
			incomingMessage2.EM_MessageText = incomingMessage.EM_MessageText;
			incomingMessage2.EM_Status = MQEDIMessage.Status.Queued;
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			entryHeader.Reload();
			AssertEquals("Second Extension.", "10-Feb-23 00:00:00", entryHeader.US_TIBExpiryDate.ToString());

			var incomingMessage3 = Factory.New<MQEDIMessage>();
			incomingMessage3.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage3.EM_MessageNum = "~15000";
			incomingMessage3.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse;
			incomingMessage3.EM_MessageText = incomingMessage.EM_MessageText;
			incomingMessage3.EM_Status = MQEDIMessage.Status.Queued;
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			entryHeader.Reload();
			AssertEquals("No More Extension.", "10-Feb-23 00:00:00", entryHeader.US_TIBExpiryDate.ToString());
		}

		public void TestTIBExpiryDateExtentionWithTariff98130075()
		{
			declaration.JE_DateOfArrival = new DateTime(2020, 02, 10);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 100m;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "98130075";
			invoiceLine.JI_LinePrice = 100m;
			incomingMessage.EM_MessageText =
"B018888XJ5TX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: XJ5 23456781                                           " +
"E1RF995 EXT GRANTED SUBJECT TO REVIEW             XJ5  23456781     B00000001   " +
"Y  8888XJ5X100007";
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var header = (CusEntryHeader)(invoice.Entries[0]);
			header.Messages.Add(outgoingMessage);
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			header.EntryNumber = "23456781";
			header.SetTIBExpiryDate();
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			header.Reload();
			AssertEquals("No Update for Tariff 98130075.", "10-Aug-20 00:00:00", header.US_TIBExpiryDate.ToString());
		}

		protected override void EndToEndCore()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals("message is processed", "RCV", incomingMessage.EM_Status);
			AssertEquals("MessageSubType is set", EM_MessageSubTypeList.Codes.TemporaryImportationBondRequestToExtend, incomingMessage.EM_MessageSubType);
			AssertEquals("message is linked to the entry", entryHeader.PK, incomingMessage.EM_LinkedObject.PK);
			AssertNotNull("An email with subject containing 'Request for ACE TIB Extension/Closure' should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Request for ACE TIB Extension/Closure"); })));
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "23456789";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosure;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.TemporaryImportationBondRequestToExtend;
			entryHeader.Messages.Add(outgoingMessage);

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse;
			incomingMessage.EM_MessageText =
"B018888XJ5TX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: XJ5 23456789                                           " +
"E1RF998 TRANSACTION DATA REJECTED                 XJ5  23456789     B00000001   " +
"Y  8888XJ5X100007";
			incomingMessage.EM_Status = MQEDIMessage.Status.Queued;
			Factory.Save();
		}
		MQEDIMessage incomingMessage;
		MQEDIMessage outgoingMessage;
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
	}
}
