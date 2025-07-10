using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class AllACEMessageFailureProcessorTest : MessageFailureProcessorTest<AllACEMessageFailureProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var declaration = GetMergedDeclaration("00000063");

			var outgoingmessage = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add).PopulateMessage();
			Factory.Save();

			outgoingmessage.EM_MessageNum = "YASYUSPRD_70018";

			var message = GetMessageToProcess();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageText =
"B00                                                        B                    " +
"X0 BLOCK       1 REF ID:      286    AE YASYUSPRD_70018                         " +
"X1 FX18   PROC PORT/FLR NOT AUTHRZD FOR SENDR/RCVR                              " +
"X1RF999   BATCH REJECTED                                                        " +
"Y           00003";

			message.EM_MessageNum = "B";

			Factory.Save();

			Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);
			AssertEquals("Status changed", "EEO", declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status);
			AssertEquals("EM_MessageSubType is copied", EM_MessageSubTypeList.Codes.EntrySummaryAdd, message.EM_MessageSubType);

			AssertEquals("Subject contains well-formatted entry number", "Entry Summary Response (Failure) for B00001000 / XJ5-0000006-3", Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
		}

		public void TestProcessEntrySummaryQueryAuthorisation()
		{
			var declaration = GetMergedDeclaration("00000063");
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryQuery;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "56561";

			var message = GetMessageToProcess();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageText =
"B00                                                        B                    " +
"X0 BLOCK       1 REF ID: 3902 SV9    JC 56561                                   " +
"X1 FX18   PROC PORT/FLR NOT AUTHRZD FOR SENDR/RCVR                              " +
"X1RF999   BATCH REJECTED                                                        " +
"Y           00003";

			message.EM_MessageNum = "B";//B block User Data has this

			Factory.Save();

			Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);
			AssertEquals("This failure should not change CH_Status", ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status);
			AssertEquals("EM_MessageSubType is copied", EM_MessageSubTypeList.Codes.EntrySummaryQuery, message.EM_MessageSubType);
		}

		public void TestProcessMessageWithMessageTypeNF()
		{
			var message = GetMessageToProcess();
			message.EM_ApplicationCode = CBPEDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message.EM_Status = CBPEDIMessage.Status.Queued;
			message.EM_MessageNum = "M13ILLORD_2244913";
			message.EM_MessageText = "B003901NLRNF                                               M13ILLORD_2244913    X0 BLOCK  000001 REF ID: 3901 NLR    FT M13ILLORD_2244913                       X1 FX20   FILER NOT AUTHORIZED FOR APPLICATION ID                               X0                                                                              X1RF999   BATCH REJECTED                                                        Y  3901NLRNF00000";
			Factory.Save();

			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();
			Assert(!processor.Logger.Logs.Any(log => log.Message.Contains($"Unable to cast object of type '{typeof(AABIOutputB).FullName}' to type '{typeof(APLB).FullName}'")));
		}

		public void TestProcessMessageWithoutMessageType()
		{
			var message = GetMessageToProcess();
			message.EM_ApplicationCode = CBPEDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message.EM_MessageText = "B00                                                        B                    X0 BLOCK       1 REF ID: 3902 SV9    AE 53797                                   X1 FX18   PROC PORT/FLR NOT AUTHRZD FOR SENDR/RCVR                              X1 FX20   FILER NOT AUTHORIZED FOR APPLICATION ID                               X1RF999   BATCH REJECTED                                                        Y           00004";
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				new ABIIncomingMessageProcessor().ExecuteBatch();
			});

			message.Reload();
			AssertEquals("53797", message.EM_MessageNum);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.EntrySummary, message.EM_MessageType);
		}

		public void TestManufacturerNameandAddressAddFailure()
		{
			ProcessAndAssertForOrg(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAdd, ACEApplicationIdentifierCodeList.Descriptions.ManufacturerNameandAddressAdd);
		}

		[ExpectNoExceptions]
		void ProcessAndAssertForOrg(string applicationIdentifier, string subject)
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);
			ProcessAndAssert(applicationIdentifier, subject, (MQEDIMessage sendingMessage) =>
			{
				sendingMessage.EM_LinkedObject = org;
			}, true);
		}

		JobDeclaration GetMergedDeclaration(ZString entryNumber)
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "XJ5";
			dec.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			dec.ImportEntryNumber = entryNumber;

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();
			dec.InvoiceLines.AddNew();

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			return dec;
		}

		MQEDIMessage GetMessageToProcess()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}
	}
}
