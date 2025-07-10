using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ServiceTasks.Testing.UCPSubscribersTest;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	sealed class UCPProcessorTest : TestCaseWithFactory
	{
		public void TestProcessInBatch_BatchSize()
		{
			using (CustomsDataRegistry.Instance.UCPInterchangesPerBatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				SetupDataForTesting();

				var logger = new LoggingInformation();
				var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker(), CancellationToken.None, "ProcessInBatch");
				processor.Process();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, message1, EDIMessage.Status.ProcessedOK, 0);
				AssertMessage(newFactory, message2, EDIMessage.Status.ProcessedOK, 0);
				AssertMessage(newFactory, message3, EDIMessage.Status.ProcessedOK, 0);
				AssertInterchange(newFactory, message1);
				AssertInterchange(newFactory, message2);
				AssertInterchange(newFactory, message3);

				AssertContainsExactElementsInExactOrder("Batch size is 1 so only 1 ITH message processed at each time",
					new[] {
					"\tStart to process 1 ITH message(s) for EDI/IT1.",
					"\t1 ITH message(s) and packed Interchanges saved in batch.",
					"\tStart to process 1 ITH message(s) for EDI/IT2.",
					"\t1 ITH message(s) and packed Interchanges saved in batch.",
					"\tStart to process 1 ITH message(s) for EDI/IT2.",
					"\t1 ITH message(s) and packed Interchanges saved in batch.",
					"\tNo ITH message(s) to deal with."
				}, logger.UserLogStrings);
			}
		}

		public void TestProcess_GroupByBranch()
		{
			SetupDataForTesting();

			var logger = new LoggingInformation();
			var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker(), CancellationToken.None, "ProcessInBatch");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			AssertMessage(newFactory, message1, EDIMessage.Status.ProcessedOK, 0);
			AssertMessage(newFactory, message2, EDIMessage.Status.ProcessedOK, 0);
			AssertMessage(newFactory, message3, EDIMessage.Status.ProcessedOK, 0);
			AssertInterchange(newFactory, message1);
			AssertInterchange(newFactory, message2);
			AssertInterchange(newFactory, message3);

			AssertContainsExactElementsInExactOrder("Batch size is 50 by deafault so the ITH messages in the same branch will be processed in batch",
				new[] {
					"\tStart to process 1 ITH message(s) for EDI/IT1.",
					"\t1 ITH message(s) and packed Interchanges saved in batch.",
					"\tStart to process 2 ITH message(s) for EDI/IT2.",
					"\t2 ITH message(s) and packed Interchanges saved in batch.",
					"\tNo ITH message(s) to deal with."
			}, logger.UserLogStrings);
		}

		public void TestProcessSeparately()
		{
			SetupDataForTesting();

			var logger = new LoggingInformation();
			var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker(), CancellationToken.None, "ProcessSeparately");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			var t = logger.UserLogStrings;
			AssertMessage(newFactory, message1, EDIMessage.Status.ProcessedOK, 0);
			AssertMessage(newFactory, message2, EDIMessage.Status.ProcessedOK, 0);
			AssertMessage(newFactory, message3, EDIMessage.Status.ProcessedOK, 0);
			AssertInterchange(newFactory, message1);
			AssertInterchange(newFactory, message2);
			AssertInterchange(newFactory, message3);

			AssertContainsExactElementsInExactOrder("Save Separately",
				new[] {
					"\tStart to process 1 ITH message(s) for EDI/IT1.",
					"\tUnable to save 1 ITH message(s) and interchange(s) in batch. Each message and interchange bundle will be saved separately with separate factory.",
					"\tThe ITH EDIMessage In001 EM_Statue updated and packed Interchange In001 saved.",
					"\tStart to process 2 ITH message(s) for EDI/IT2.",
					"\tUnable to save 2 ITH message(s) and interchange(s) in batch. Each message and interchange bundle will be saved separately with separate factory.",
					"\tThe ITH EDIMessage In002 EM_Statue updated and packed Interchange In002 saved.",
					"\tThe ITH EDIMessage In003 EM_Statue updated and packed Interchange In003 saved.",
					"\tNo ITH message(s) to deal with."
				}, logger.UserLogStrings);
		}

		public void TestProcessRetryCount()
		{
			SetupDataForTesting();

			using (CustomsDataRegistry.Instance.UCPInterchangePackingMaxRetryCount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var logger = new LoggingInformation();
				var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker(), CancellationToken.None, "ProcessError");
				processor.Process();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, message1, EDIInterchange.Status.Failed, 2);
				AssertMessage(newFactory, message2, EDIInterchange.Status.Failed, 2);
				AssertMessage(newFactory, message3, EDIInterchange.Status.Failed, 2);

				AssertContainsExactElementsInExactOrder("Retry",
					new[] {
							"\tStart to process 1 ITH message(s) for EDI/IT1.",
							"\tUnable to save 1 ITH message(s) and interchange(s) in batch. Each message and interchange bundle will be saved separately with separate factory.",
							"\tUnable to save ITH message In001 and interchange Separately. Message Retry count 0.",
							"\tStart to process 2 ITH message(s) for EDI/IT2.",
							"\tUnable to save 2 ITH message(s) and interchange(s) in batch. Each message and interchange bundle will be saved separately with separate factory.",
							"\tUnable to save ITH message In002 and interchange Separately. Message Retry count 0.",
							"\tUnable to save ITH message In003 and interchange Separately. Message Retry count 0.",
							"\tStart to process 1 ITH message(s) for EDI/IT1.",
							"\tUnable to save 1 ITH message(s) and interchange(s) in batch. Each message and interchange bundle will be saved separately with separate factory.",
							"\tUnable to save ITH message In001 and interchange Separately. Message Retry count 1.",
							"\tStart to process 2 ITH message(s) for EDI/IT2.",
							"\tUnable to save 2 ITH message(s) and interchange(s) in batch. Each message and interchange bundle will be saved separately with separate factory.",
							"\tUnable to save ITH message In002 and interchange Separately. Message Retry count 1.",
							"\tUnable to save ITH message In003 and interchange Separately. Message Retry count 1.",
							"\tFailed to pack ITH EDIMessage In001. Max retry attempts 1 Reached.",
							"\tFailed to pack ITH EDIMessage In002. Max retry attempts 1 Reached.",
							"\tFailed to pack ITH EDIMessage In003. Max retry attempts 1 Reached.",
							"\tNo ITH message(s) to deal with."
					}, logger.UserLogStrings);
			}
		}

		public void TestProcess_AllowEmptyBody()
		{
			var message = CreateEDIMessage("ITC", GlbBranch.CurrentBranch.PK, "123", "");
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessagePackProcessorForTest(logger, "ITC", new MessagePackerForTest(), CancellationToken.None, "ProcessInBatch");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			AssertMessage(newFactory, message, EDIMessage.Status.ProcessedOK, 0);
			var notes = message.Notes.FindByDescription("Pack EDIMessage Error");
			AssertEquals("no note", 0, notes.Length);
		}

		public void TestProcess_EmptyBody_Error()
		{
			var message = CreateEDIMessage("ITH", GlbBranch.CurrentBranch.PK, "123", "");
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker(), CancellationToken.None, "ProcessSeparately");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			AssertMessage(newFactory, message, EDIMessage.Status.Discarded, 0);
			AssertInterchange(newFactory, message, false);
			var note = message.Notes.FindByDescription("Pack EDIMessage Error").Single();
			AssertEquals("note.ST_NoteDataAsText", "Message Body is required to generate an EDIInterchange.", note.ST_NoteDataAsText);
		}

		public void TestProcess_EM_HeldUntilDate()
		{
			var message = CreateEDIMessage("ITH", GlbBranch.CurrentBranch.PK, "123", "test");
			message.EM_HeldUntilDate = DateTime.UtcNow.AddDays(1);
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker(), CancellationToken.None, "ProcessSeparately");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			AssertMessage(newFactory, message, EDIMessage.Status.Queued, 0);
		}

		public void TestPackError()
		{
			SetupDataForTesting();

			using (CustomsDataRegistry.Instance.UCPInterchangePackingMaxRetryCount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var logger = new LoggingInformation();
				var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker("IT Pack failed reason"), CancellationToken.None, "ProcessInBatch");
				processor.Process();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, message1, EDIInterchange.Status.Failed, 1);
				AssertMessage(newFactory, message2, EDIInterchange.Status.Failed, 1);
				AssertMessage(newFactory, message3, EDIInterchange.Status.Failed, 1);

				AssertContainsExactElementsInExactOrder("Pack log Error",
					new[] {
						"\tStart to process 1 ITH message(s) for EDI/IT1.",
						"\tIT inner pack error: IT Pack failed reason",
						"\tFailed to pack ITH EDIMessage In001. EDIMessage Retry count 1. ErrorReason: IT Pack failed reason",
						"\t1 ITH message(s) and packed Interchanges saved in batch.",
						"\tStart to process 2 ITH message(s) for EDI/IT2.",
						"\tIT inner pack error: IT Pack failed reason",
						"\tFailed to pack ITH EDIMessage In002. EDIMessage Retry count 1. ErrorReason: IT Pack failed reason",
						"\tIT inner pack error: IT Pack failed reason",
						"\tFailed to pack ITH EDIMessage In003. EDIMessage Retry count 1. ErrorReason: IT Pack failed reason",
						"\t2 ITH message(s) and packed Interchanges saved in batch.",
						"\tFailed to pack ITH EDIMessage In001. Max retry attempts 0 Reached.",
						"\tFailed to pack ITH EDIMessage In002. Max retry attempts 0 Reached.",
						"\tFailed to pack ITH EDIMessage In003. Max retry attempts 0 Reached.",
						"\tNo ITH message(s) to deal with."
					}, logger.UserLogStrings);
			}
		}

		public void TestPackException()
		{
			SetupDataForTesting();

			using (CustomsDataRegistry.Instance.UCPInterchangePackingMaxRetryCount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var logger = new LoggingInformation();
				var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker("Exception"), CancellationToken.None, "ProcessInBatch");
				processor.Process();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, message1, EDIInterchange.Status.Failed, 1);
				AssertMessage(newFactory, message2, EDIInterchange.Status.Failed, 1);
				AssertMessage(newFactory, message3, EDIInterchange.Status.Failed, 1);

				AssertContainsExactElementsInExactOrder("Unpack Exception",
					new[] {
						"\tStart to process 1 ITH message(s) for EDI/IT1.",
						"\tFailed to pack ITH EDIMessage In001. EDIMessage Retry count 1. ErrorReason: Unexpected exception during packing.",
						"\t1 ITH message(s) and packed Interchanges saved in batch.",
						"\tStart to process 2 ITH message(s) for EDI/IT2.",
						"\tFailed to pack ITH EDIMessage In002. EDIMessage Retry count 1. ErrorReason: Unexpected exception during packing.",
						"\tFailed to pack ITH EDIMessage In003. EDIMessage Retry count 1. ErrorReason: Unexpected exception during packing.",
						"\t2 ITH message(s) and packed Interchanges saved in batch.",
						"\tFailed to pack ITH EDIMessage In001. Max retry attempts 0 Reached.",
						"\tFailed to pack ITH EDIMessage In002. Max retry attempts 0 Reached.",
						"\tFailed to pack ITH EDIMessage In003. Max retry attempts 0 Reached.",
						"\tNo ITH message(s) to deal with."
					}, logger.UserLogStrings);
			}
		}

		public void TestPackErrorInterchangeDelete()
		{
			SetupDataForTesting();

			using (CustomsDataRegistry.Instance.UCPInterchangePackingMaxRetryCount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var logger = new LoggingInformation();
				var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker("IT Pack Message Added To Interchange"), CancellationToken.None, "ProcessInBatch");

				AssertNoExceptionThrown("Deleting the new interchange should not cause errors", () => processor.Process());

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, message1, EDIInterchange.Status.Failed, 1);
				AssertMessage(newFactory, message2, EDIInterchange.Status.Failed, 1);
				AssertMessage(newFactory, message3, EDIInterchange.Status.Failed, 1);
			}
		}

		public void TestProcessCancellation()
		{
			SetupDataForTesting();

			var logger = new LoggingInformation();
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();
			var processor = new MessagePackProcessorForTest(logger, "ITH", new ITHMessagePacker(), cancellationTokenSource.Token, "ProcessInBatch");

			AssertExceptionThrown<OperationCanceledException>("The operation was canceled.", processor.Process);
		}

		void SetupDataForTesting()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var itBranch1 = company.Branches.AddNew();
			itBranch1.GB_RL_NKHomePort = "ITPAR";
			itBranch1.GB_Code = "IT1";
			itBranch1.GB_BranchName = "IT1 Name";
			var itBranch2 = company.Branches.AddNew();
			itBranch2.GB_RL_NKHomePort = "ITPAR";
			itBranch2.GB_Code = "IT2";
			itBranch2.GB_BranchName = "IT2 Name";
			Factory.Save();

			message1 = CreateEDIMessage("ITH", itBranch1.PK, "In001");
			message2 = CreateEDIMessage("ITH", itBranch2.PK, "In002");
			message3 = CreateEDIMessage("ITH", itBranch2.PK, "In003");
			Factory.Save();
		}
		EDIMessage message1, message2, message3;

		EDIMessage CreateEDIMessage(ZString applicationCode, ZGuid branchPK, ZString messageNum, string messageBody = "test")
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.MessageNumberStrategy = new TestMessageNumberStrategy(messageNum);
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = "IMP";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = branchPK;
			message.EM_MessageText = messageBody;
			return message;
		}

		void AssertInterchange(BusinessObjectFactory factory, EDIMessage message, bool hasInterchange = true)
		{
			message = factory.Load<EDIMessage>(message.PK);
			var interchange = factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.PK, message.EM_EI));
			if (hasInterchange)
			{
				AssertEquals("EI_InterchangeNum", message.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_Status", "QUE", interchange.EI_Status);
				AssertEquals("EI_ApplicationCode", message.EM_ApplicationCode, interchange.EI_ApplicationCode);
				AssertEquals("EI_RecieveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_TransportType", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
				AssertEquals("EI_GB", message.EM_GB, interchange.EI_GB);
			}
			else
			{
				AssertEquals("No packing interchange", null, interchange);
			}
		}

		void AssertMessage(BusinessObjectFactory factory, EDIMessage message, ZString status, int retryCount)
		{
			message = factory.Load<EDIMessage>(message.PK);
			AssertEquals("EM_Status", status, message.EM_Status);
			AssertEquals("EM_RetryCount", retryCount, message.EM_RetryCount);

			if (status == EDIInterchange.Status.Failed || status == EDIInterchange.Status.Discarded || status == EDIInterchange.Status.Queued)
			{
				AssertEquals("EM_EI should be empty", ZGuid.Empty, message.EM_EI);
			}
			else
			{
				AssertNotEquals("EM_EI should be filled", ZGuid.Empty, message.EM_EI);
			}
		}

		public class MessagePackProcessorForTest : UCPProcessor
		{
			public MessagePackProcessorForTest(LoggingInformation logger, string applicationCode, IUniversalCustomsEDIMessagePacker messagePacker, CancellationToken token, ZString howToProcess)
							: base(logger, applicationCode, messagePacker, token)
			{
				this.howToProcess = howToProcess;
			}
			readonly ZString howToProcess;

			protected override BusinessObjectFactory GetFactory(string description)
			{
				switch (description)
				{
					case "Messages and Interchanges Saving in batch":
						if (howToProcess == "ProcessSeparately" || howToProcess == "ProcessError")
						{
							var factory = new BusinessObjectFactory();
							factory.Saving += (f) => throw new ZSaveException(new ZDataException(new ApplicationException("I am testing for failing in saving in batch"), null, null), f);
							return factory;
						}
						return base.GetFactory(description);
					case "Message and Interchange Saving Separately":
						if (howToProcess == "ProcessError")
						{
							var factory = new BusinessObjectFactory();
							factory.Saving += (f) => throw new ZSaveException(new ZDataException(new ApplicationException("I am testing for failing in saving in Separately"), null, null), f);
							return factory;
						}
						return base.GetFactory(description);
					default:
						return base.GetFactory(description);
				}
			}
		}

		public class ITHMessagePacker : IUniversalCustomsEDIMessagePacker
		{
			public ITHMessagePacker() : this(ZString.Empty)
			{ }

			public ITHMessagePacker(ZString error)
			{
				this.error = error;
			}

			readonly ZString error;

			public bool AllowEmptyMessageBody => false;

			public ZString Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
			{
				if (error == "Exception")
				{
					throw new Exception("Unexpected exception during packing.");
				}

				if (error == "IT Pack Message Added To Interchange")
				{
					interchange.ContainedMessages.Add(message);
				}

				if (!error.IsEmpty)
				{
					logger.Log($"IT inner pack error: {error}");
					return error;
				}

				interchange.EI_ApplicationCode = message.EM_ApplicationCode;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_InterchangeNum = message.EM_MessageNum;
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				interchange.EI_TransportType = EDIInterchange.TransportType.xT;

				return ZString.Empty;
			}
		}
	}
}
