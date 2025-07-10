using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class TemporaryImportBondExpiryNotificationProcessorTest : TestCaseWithFactory
	{
		public void TestGetKeysForBlockingParallelProcessing()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B018888XJ5TS                                               15                   " +
"X18888XJ5 23456789            0501072".PadRight(80) +
"Y  8888XJ5X100007000000000000000000000000";
			message.EM_Status = MQEDIMessage.Status.Queued;

			var logger = new LoggingInformation();
			var processor = new TemporaryImportBondExpiryNotificationProcessor();
			var provider = processor as IKeysForBlockingParallelProcessingProvider;
			var actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			var actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions("Single TIBX1 block", () =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(ZString.Empty, ZGuid.Empty, ZGuid.Empty, $"Entry:XJ5-23456789|{GlbCompany.CurrentCompany.PK}")), actualMetaData);
				AssertEquals(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder, actualKeys.ReturnValue.ResultType);
			});

			message.EM_MessageText = "B018888XJ5TS                                               15                   " +
"X18888XJ5 23456789            0501072".PadRight(80) +
"X18888XJ5 98765432            0602083".PadRight(80) +
"Y  8888XJ5X100007000000000000000000000000";
			actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions("Multiple TIBX1 blocks", () =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(ZString.Empty, ZGuid.Empty, ZGuid.Empty, $"Entry:XJ5-23456789|{GlbCompany.CurrentCompany.PK}")), actualMetaData);
				AssertArrayEqualsByElements("Keys", new[] { $"Entry:XJ5-98765432|{GlbCompany.CurrentCompany.PK}" }, actualKeys.ReturnValue.Keys.ToArray());
			});
		}

		public void TestProcess()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "23456789";
			dec.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText = "B018888XJ5TS                                               15                   " +
"X18888XJ5 23456789            0501072".PadRight(80) +
"Y  8888XJ5X100007000000000000000000000000";
			incomingMessage.EM_Status = MQEDIMessage.Status.Queued;

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();

			CusEntryHeader entryLoaded = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("message is processed", "RCV", incomingMessage.EM_Status);
			AssertEquals("MessageSubType is set", EM_MessageSubTypeList.Codes.TemporaryImportationBondDueToExpire, incomingMessage.EM_MessageSubType);
			AssertEquals("message is linked to the entry", entryLoaded.PK, incomingMessage.EM_LinkedObject.PK);

			AssertEquals("Expiry Date is set", new ZDate(2007, 05, 01), entryLoaded.US_TIBExpiryDate);

			//The date should be serialised in sql format so that JobDeclarationFilterObject can string-compare the dates
			AssertEquals(true, entryLoaded.CH_AddInfo.Contains("TIBExpiryDate=2007-05-01"));
			AssertEquals("No of request is set", 2, entryLoaded.US_TIBNumOfExtensions);
			AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("B00001000 / XJ5-2345678-9"); })));
		}

		public void TestProcess_Unknown()
		{
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText = "B018888XJ5TS                                               15                   " +
"X18888XJ5 23456789            0501072".PadRight(80) +
"Y  8888XJ5X100007000000000000000000000000";
			incomingMessage.EM_Status = MQEDIMessage.Status.Queued;

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Unknown / XJ5-2345678-9"); })));
		}

		public void TestProcessMultipleX1Blocks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryHeader01 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader01.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader01.EntryNumber = "23456789";

			var shipment = Factory.New<ForwardingShipment>();
			var shipDec = Factory.New<JobDeclaration>();
			shipDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			shipDec.JE_JS = shipment.PK;

			var entryHeader02 = shipDec.CustomsEntryHeaders.AddNew();
			entryHeader02.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader02.EntryNumber = "98765432";

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			shipDec.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText = "B018888XJ5TS                                               15                   " +
"X18888XJ5 23456789            0501072".PadRight(80) +
"X18888XJ5 98765432            0602083".PadRight(80) +
"Y  8888XJ5X100007000000000000000000000000";
			incomingMessage.EM_Status = MQEDIMessage.Status.Queued;

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();

			var entryLoaded01 = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader01.PK);
			var entryLoaded02 = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader02.PK);
			CombineAssertions(() =>
			{
				AssertEquals("message is processed", "RCV", incomingMessage.EM_Status);
				AssertEquals("MessageSubType is set", EM_MessageSubTypeList.Codes.TemporaryImportationBondDueToExpire, incomingMessage.EM_MessageSubType);
				AssertEquals("message is linked to the entry", entryLoaded01.PK, incomingMessage.EM_LinkedObject.PK);

				AssertEquals("Entry01 Expiry Date is set", new ZDate(2007, 05, 01), entryLoaded01.US_TIBExpiryDate);
				AssertEquals("Entry01 Expiry Date", true, entryLoaded01.CH_AddInfo.Contains("TIBExpiryDate=2007-05-01"));
				AssertEquals("Entry01 No of request is set", 2, entryLoaded01.US_TIBNumOfExtensions);

				AssertEquals("Entry02 Expiry Date is set", new ZDate(2008, 06, 02), entryLoaded02.US_TIBExpiryDate);
				AssertEquals("Entry02 Expiry Date", true, entryLoaded02.CH_AddInfo.Contains("TIBExpiryDate=2008-06-02"));
				AssertEquals("Entry02 No of request is set", 3, entryLoaded02.US_TIBNumOfExtensions);

				AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
				{ return emailToMatched.Subject.Contains("B00001000 / XJ5-2345678-9"); })));
				AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
				{ return emailToMatched.Subject.Contains("S00001000 / XJ5-9876543-2"); })));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetupForSendMessage();
		}
	}
}
