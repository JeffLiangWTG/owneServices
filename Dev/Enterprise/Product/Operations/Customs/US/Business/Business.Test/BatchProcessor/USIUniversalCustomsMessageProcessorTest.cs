using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USIUniversalCustomsMessageProcessor))]
	sealed class USIUniversalCustomsMessageProcessorTest : UniversalCustomsMessageProcessorTest<USIUniversalCustomsMessageProcessor>
	{
		protected override string ApplicationCode => EDIMessage.ApplicationCodes.USCustomsImport;

		public void TestShouldMessageBeProcessedInASeparateFactory()
		{
			var processor = new USIUniversalCustomsMessageProcessor();
			var message1 = Factory.New<EDIMessage>();
			message1.EM_MessageType = "XXX";
			AssertEquals(false, processor.ShouldMessageBeProcessedInASeparateFactory(message1));

			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement;
			AssertEquals(false, processor.ShouldMessageBeProcessedInASeparateFactory(message1));
		}

		public void TestPreProcessMessage_NotCBP()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var message = Factory.New<EBondEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_LinkedObject = entryHeader;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			AssertGetLinkedBusinessObjectMetaData("Not CBP message", message
				, ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, ResString.GetMultilingualString("9D940781-329E-4098-8C13-9759DC8717FE", "CBP Message Processor cannot process message EBondEDIMessage "))
				, SerializationKeysResult.SerialProcessingInReceivedOrder);
		}

		public void TestPreProcessMessage_CBP()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.GB_Code = "DNZ";

			var declaration = GetMergedDeclaration("00000063");
			declaration.JE_GB = newBranch.PK;
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var outMessage = Factory.New<MQEDIMessage>();
			outMessage.EM_GB = newBranch.PK;
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_LinkedObject = entryHeader;
			outMessage.EM_MessageOwner = Constants.ACE;
			outMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery;
			outMessage.EM_MessageNum = "123456";

			var inMessage = Factory.New<MQEDIMessage>();
			inMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			inMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMessage.EM_MessageOwner = Constants.ACE;
			inMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse;
			inMessage.EM_MessageNum = "123456";
			Factory.Save();
			AssertGetLinkedBusinessObjectMetaData("ERACE", inMessage
				, LinkedBusinessObjectMetaData.New(entryHeader.TableName, entryHeader.PK, newBranch.PK, declaration.JE_DeclarationReference)
				, new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>() { declaration.JE_DeclarationReference.ToString(), $"Entry:XJ5-00000063|{newBranch.PK}" })
			);
		}

		public void TestPreProcessMessage_CBPMissingProvider()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.GB_Code = "DNZ";

			var declaration = GetMergedDeclaration("00000063");
			declaration.JE_GB = newBranch.PK;
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var outMessage = Factory.New<MQEDIMessage>();
			outMessage.EM_GB = newBranch.PK;
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_LinkedObject = entryHeader;
			outMessage.EM_MessageOwner = Constants.ACE;
			outMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQuery;
			outMessage.EM_MessageNum = "123456";

			var inMessage = Factory.New<MQEDIMessage>();
			inMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			inMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMessage.EM_MessageOwner = Constants.ACE;
			inMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQueryResponse;
			inMessage.EM_MessageNum = "123456";
			Factory.Save();
			AssertGetLinkedBusinessObjectMetaData("Message has no processor provider, has origin message, linked object is IMessageAttachee", inMessage
				, LinkedBusinessObjectMetaData.New(entryHeader.TableName, entryHeader.PK, newBranch.PK, declaration.JE_DeclarationReference)
				, new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>() { declaration.JE_DeclarationReference.ToString(), $"Entry:XJ5-00000063|{GlbCompany.CurrentCompany.PK}" })
			);
		}

		public void TestPreProcessMessage_CBPMissingProviderLinkObjectIsIJobNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var protest = new Protest.Protest(declaration);

			var outMessage = protest.Messages.AddNew(typeof(MQEDIMessage));
			outMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_LinkedObject = protest;
			outMessage.EM_MessageOwner = Constants.ACE;
			outMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ProtestAmendment;
			outMessage.EM_MessageNum = "123456";

			var inMessage = protest.Messages.AddNew(typeof(MQEDIMessage));
			inMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			inMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMessage.EM_MessageOwner = Constants.ACE;
			inMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ProtestAmendmentResponse;
			inMessage.EM_MessageNum = "123456";
			Factory.Save();
			AssertGetLinkedBusinessObjectMetaData("Message has no processor provider, has origin message, linked object is not IMessageAttachee", inMessage
				, LinkedBusinessObjectMetaData.New(protest.TableName, protest.PK, GlbBranch.CurrentBranch.PK, declaration.JE_DeclarationReference)
				, new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>() { declaration.JE_DeclarationReference.ToString() })
			);
		}

		public void TestPreProcessMessage_NoOriginalMessage()
		{
			var inMessage = Factory.New<MQEDIMessage>();
			inMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			inMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMessage.EM_MessageOwner = Constants.ACE;
			inMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			inMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse;
			Factory.Save();
			AssertGetLinkedBusinessObjectMetaData("Message has no processor provider, has no origin message", inMessage
				, LinkedBusinessObjectMetaData.New(ZString.Empty, ZGuid.Empty, GlbBranch.CurrentBranch.PK, "InBond")
				, new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>() { "InBond" })
			);
		}

		void AssertGetLinkedBusinessObjectMetaData(string caseID, BaseEDIMessage incomingMessage, ProcessingResult<LinkedBusinessObjectMetaData> expectedMetaData, ProcessingResult<SerializationKeysResult> expectedKeys)
		{
			var logger = new LoggingInformation();
			var processor = new USIUniversalCustomsMessageProcessor();
			var actualMetaData = processor.GetLinkedBusinessObjectMetaData(incomingMessage, logger);
			var actualKeys = processor.GetSerializationKeysResult(incomingMessage, logger, actualMetaData.ReturnValue);
			actualKeys.ReturnValue.Keys.SetEquals(expectedKeys.ReturnValue.Keys);
			CombineAssertions(caseID, () =>
			{
				AssertEquals("Meta data", expectedMetaData, actualMetaData);
				AssertEquals("KeyResult.ResultType", expectedKeys.ReturnValue.ResultType, actualKeys.ReturnValue.ResultType);
				Assert("KeyResult.Keys ", expectedKeys.ReturnValue.Keys.SetEquals(actualKeys.ReturnValue.Keys));
			});
		}

		public void TestProcessMessage_ReferenceFileMessageTypes()
		{
			var inMessage = Factory.New<MQEDIMessage>();
			inMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			inMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMessage.EM_MessageOwner = Constants.ACE;
			inMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse;
			inMessage.EM_MessageNum = "123456";
			var log = new LoggingInformation();
			new USIUniversalCustomsMessageProcessor().ProcessMessage(inMessage, log, default);
			AssertEquals("EM_Status", EDIMessage.Status.Received, inMessage.EM_Status);
		}

		JobDeclaration GetMergedDeclaration(ZString entryNumber)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "XJ5";
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			dec.ImportEntryNumber = entryNumber;

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();
			dec.InvoiceLines.AddNew();
			dec.US_CertReqDate = ZDateTime.Now;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			return dec;
		}
	}
}
