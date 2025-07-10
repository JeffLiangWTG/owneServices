using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	sealed class UCUProcessorTest : TestCaseWithFactory
	{
		public void TestProcessInBatch_BatchSize()
		{
			using (CustomsDataRegistry.Instance.UCUInterchangesPerBatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				SetupDataForTesting();

				var logger = new LoggingInformation();
				var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker(), CancellationToken.None, "ProcessInBatch");
				processor.Process();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, interchange1, true, outgoingEDIMessage1);
				AssertMessage(newFactory, interchange2, true, outgoingEDIMessage2);
				AssertMessage(newFactory, interchange3, true, outgoingEDIMessage3);
				AssertInterchange(newFactory, interchange1, EDIInterchange.Status.Received, 0);
				AssertInterchange(newFactory, interchange2, EDIInterchange.Status.Received, 0);
				AssertInterchange(newFactory, interchange3, EDIInterchange.Status.Received, 0);

				AssertContainsExactElementsInExactOrder("Batch size is 1 so only 1 KRC interchange processed at each time",
					new[] {
					"\tStart to process 1 KRC interchange(s) for EDI/KR1.",
					"\t1 KRC interchange(s) and unpacking message(s) saved in batch.",
					"\tStart to process 1 KRC interchange(s) for EDI/KR1.",
					"\t1 KRC interchange(s) and unpacking message(s) saved in batch.",
					"\tStart to process 1 KRC interchange(s) for EDI/KR2.",
					"\t1 KRC interchange(s) and unpacking message(s) saved in batch.",
					"\tNo KRC interchange(s) to deal with."
				}, logger.UserLogStrings);
			}
		}

		public void TestProcess_GroupByBranch()
		{
			SetupDataForTesting();

			var logger = new LoggingInformation();
			var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker(), CancellationToken.None, "ProcessInBatch");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			AssertMessage(newFactory, interchange1, true, outgoingEDIMessage1);
			AssertMessage(newFactory, interchange2, true, outgoingEDIMessage2);
			AssertMessage(newFactory, interchange3, true, outgoingEDIMessage3);
			AssertInterchange(newFactory, interchange1, EDIInterchange.Status.Received, 0);
			AssertInterchange(newFactory, interchange2, EDIInterchange.Status.Received, 0);
			AssertInterchange(newFactory, interchange3, EDIInterchange.Status.Received, 0);

			AssertContainsExactElementsInExactOrder("Batch size is 50 by deafult so the KRC interchanges in the same branch will be processed in batch",
				new[] {
					"\tStart to process 2 KRC interchange(s) for EDI/KR1.",
					"\t2 KRC interchange(s) and unpacking message(s) saved in batch.",
					"\tStart to process 1 KRC interchange(s) for EDI/KR2.",
					"\t1 KRC interchange(s) and unpacking message(s) saved in batch.",
					"\tNo KRC interchange(s) to deal with."
			}, logger.UserLogStrings);
		}

		public void TestProcessSeparately()
		{
			SetupDataForTesting();

			var logger = new LoggingInformation();
			var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker(), CancellationToken.None, "ProcessSeparately");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			AssertMessage(newFactory, interchange2, true, outgoingEDIMessage2);
			AssertMessage(newFactory, interchange3, true, outgoingEDIMessage3);
			AssertInterchange(newFactory, interchange2, EDIInterchange.Status.Received, 0);
			AssertInterchange(newFactory, interchange3, EDIInterchange.Status.Received, 0);

			AssertContainsExactElementsInExactOrder("Save Separately",
				new[] {
					"\tStart to process 2 KRC interchange(s) for EDI/KR1.",
					"\tUnable to save 2 KRC interchanges and messages in batch. Each interchange and message(s) bundle will be saved separately with separate factory.",
					"\tThe KRC interchange In001 and unpacking message(s) saved Separately.",
					"\tThe KRC interchange In002 and unpacking message(s) saved Separately.",
					"\tStart to process 1 KRC interchange(s) for EDI/KR2.",
					"\tUnable to save 1 KRC interchanges and messages in batch. Each interchange and message(s) bundle will be saved separately with separate factory.",
					"\tThe KRC interchange In003 and unpacking message(s) saved Separately.",
					"\tNo KRC interchange(s) to deal with."
				}, logger.UserLogStrings);
		}

		public void TestProcessRetryCount()
		{
			SetupDataForTesting();

			using (CustomsDataRegistry.Instance.UCUInterchangeUnpackingMaxRetryCount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var logger = new LoggingInformation();
				var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker(), CancellationToken.None, "ProcessError");
				processor.Process();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, interchange2, false);
				AssertMessage(newFactory, interchange3, false);
				AssertInterchange(newFactory, interchange2, EDIInterchange.Status.Failed, 2);
				AssertInterchange(newFactory, interchange3, EDIInterchange.Status.Failed, 2);

				AssertContainsExactElementsInExactOrder("Retry",
					new[] {
							"\tStart to process 2 KRC interchange(s) for EDI/KR1.",
							"\tUnable to save 2 KRC interchanges and messages in batch. Each interchange and message(s) bundle will be saved separately with separate factory.",
							"\tUnable to save KRC interchange In001 and message(s) Separately. Interchange Retry count 0.",
							"\tUnable to save KRC interchange In002 and message(s) Separately. Interchange Retry count 0.",
							"\tStart to process 1 KRC interchange(s) for EDI/KR2.",
							"\tUnable to save 1 KRC interchanges and messages in batch. Each interchange and message(s) bundle will be saved separately with separate factory.",
							"\tUnable to save KRC interchange In003 and message(s) Separately. Interchange Retry count 0.",
							"\tStart to process 2 KRC interchange(s) for EDI/KR1.",
							"\tUnable to save 2 KRC interchanges and messages in batch. Each interchange and message(s) bundle will be saved separately with separate factory.",
							"\tUnable to save KRC interchange In001 and message(s) Separately. Interchange Retry count 1.",
							"\tUnable to save KRC interchange In002 and message(s) Separately. Interchange Retry count 1.",
							"\tStart to process 1 KRC interchange(s) for EDI/KR2.",
							"\tUnable to save 1 KRC interchanges and messages in batch. Each interchange and message(s) bundle will be saved separately with separate factory.",
							"\tUnable to save KRC interchange In003 and message(s) Separately. Interchange Retry count 1.",
							"\tFailed to unpack KRC Interchange In001. Max retry attempts 1 Reached.",
							"\tFailed to unpack KRC Interchange In002. Max retry attempts 1 Reached.",
							"\tFailed to unpack KRC Interchange In003. Max retry attempts 1 Reached.",
							"\tNo KRC interchange(s) to deal with."
					}, logger.UserLogStrings);
			}
		}

		public void TestProcess_DBHits()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var krBranch1 = company.Branches.AddNew();
			krBranch1.GB_RL_NKHomePort = "KRSOL";
			krBranch1.GB_Code = "KR1";
			krBranch1.GB_BranchName = "KR1 Name";
			Factory.Save();

			for (ulong i = 0; i < 101u; i++)
			{
				(outgoingEDIMessage1, interchange1) = CreateInterchange("KRC", krBranch1.PK, $"In001_{i}", true);
			}
			Factory.Save();

			using (CustomsDataRegistry.Instance.UCUInterchangesPerBatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			{
				var logger = new LoggingInformation();
				var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker(), CancellationToken.None, "RecordFactorySaveCount");

				var expectedDbHits = new Dictionary<string, int>
				{
					{ EDIMessageSchema.Constants.TableName, 3 },
					{ EDIInterchangeSchema.Constants.TableName, 3 }
				};
				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, processor.Factory_Exposed))
				{
					processor.Process();
				}
			}
		}

		public void TestProcess_NoOutgoingEDIMessage()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var krBranch1 = company.Branches.AddNew();
			krBranch1.GB_RL_NKHomePort = "KRSOL";
			krBranch1.GB_Code = "KR1";
			krBranch1.GB_BranchName = "KR1 Name";
			Factory.Save();

			(_, interchange1) = CreateInterchange("KRC", krBranch1.PK, "In001", false);
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker(), CancellationToken.None, "ProcessSeparately");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			AssertMessage(newFactory, interchange1);
			AssertInterchange(newFactory, interchange1, EDIInterchange.Status.Received, 0);
		}

		public void TestProcess_NoOutgoingEDIInterchange()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var krBranch1 = company.Branches.AddNew();
			krBranch1.GB_RL_NKHomePort = "KRSOL";
			krBranch1.GB_Code = "KR1";
			krBranch1.GB_BranchName = "KR1 Name";
			Factory.Save();

			(_, interchange1) = CreateInterchange("KRC", krBranch1.PK, "In001", false, false);
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker(), CancellationToken.None, "ProcessSeparately");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			AssertMessage(newFactory, interchange1);
			AssertInterchange(newFactory, interchange1, EDIInterchange.Status.Received, 0);
		}

		public void TestProcess_EmptyEDIInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "001";
			interchange.EI_ApplicationCode = "KRC";
			interchange.EI_InterchangeType = "ZZZ";
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Receive;
			interchange.EI_Status = EDIMessage.Status.Queued;
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker(), CancellationToken.None, "ProcessSeparately");
			processor.Process();

			var newFactory = new BusinessObjectFactory();
			AssertMessage(newFactory, interchange, false);
			AssertInterchange(newFactory, interchange, EDIInterchange.Status.Discarded, 0);
			var note = interchange.Notes.FindByDescription("Unpack EDIInterchange Error").Single();
			AssertEquals("note.ST_NoteDataAsText", "The interchange has an empty message text, and cannot be unpacked to EdiMessage.", note.ST_NoteDataAsText);
		}

		public void TestUnpackError()
		{
			SetupDataForTesting();

			using (CustomsDataRegistry.Instance.UCUInterchangeUnpackingMaxRetryCount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var logger = new LoggingInformation();
				var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker("KR unpack failed reason."), CancellationToken.None, "ProcessInBatch");
				processor.Process();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, interchange2, false);
				AssertMessage(newFactory, interchange3, false);
				AssertInterchange(newFactory, interchange2, EDIInterchange.Status.Failed, 1);
				AssertInterchange(newFactory, interchange3, EDIInterchange.Status.Failed, 1);

				AssertContainsExactElementsInExactOrder("Unpack log Error",
					new[] {
						"\tStart to process 2 KRC interchange(s) for EDI/KR1.",
						"\tKR inner unpack error: KR unpack failed reason.",
						"\tFailed to unpack KRC Interchange In001. Interchange Retry count 1. ErrorReason: KR unpack failed reason.",
						"\tKR inner unpack error: KR unpack failed reason.",
						"\tFailed to unpack KRC Interchange In002. Interchange Retry count 1. ErrorReason: KR unpack failed reason.",
						"\t2 KRC interchange(s) and unpacking message(s) saved in batch.",
						"\tStart to process 1 KRC interchange(s) for EDI/KR2.",
						"\tKR inner unpack error: KR unpack failed reason.",
						"\tFailed to unpack KRC Interchange In003. Interchange Retry count 1. ErrorReason: KR unpack failed reason.",
						"\t1 KRC interchange(s) and unpacking message(s) saved in batch.",
						"\tFailed to unpack KRC Interchange In001. Max retry attempts 0 Reached.",
						"\tFailed to unpack KRC Interchange In002. Max retry attempts 0 Reached.",
						"\tFailed to unpack KRC Interchange In003. Max retry attempts 0 Reached.",
						"\tNo KRC interchange(s) to deal with."
					}, logger.UserLogStrings);
			}
		}

		public void TestUnpackException()
		{
			SetupDataForTesting();

			using (CustomsDataRegistry.Instance.UCUInterchangeUnpackingMaxRetryCount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var logger = new LoggingInformation();
				var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker("Exception"), CancellationToken.None, "ProcessInBatch");
				processor.Process();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, interchange2, false);
				AssertMessage(newFactory, interchange3, false);
				AssertInterchange(newFactory, interchange2, EDIInterchange.Status.Failed, 1);
				AssertInterchange(newFactory, interchange3, EDIInterchange.Status.Failed, 1);

				AssertContainsExactElementsInExactOrder("Unpack Exception",
					new[] {
						"\tStart to process 2 KRC interchange(s) for EDI/KR1.",
						"\tFailed to unpack KRC Interchange In001. Interchange Retry count 1. ErrorReason: Unexpected exception during Unpacking.",
						"\tFailed to unpack KRC Interchange In002. Interchange Retry count 1. ErrorReason: Unexpected exception during Unpacking.",
						"\t2 KRC interchange(s) and unpacking message(s) saved in batch.",
						"\tStart to process 1 KRC interchange(s) for EDI/KR2.",
						"\tFailed to unpack KRC Interchange In003. Interchange Retry count 1. ErrorReason: Unexpected exception during Unpacking.",
						"\t1 KRC interchange(s) and unpacking message(s) saved in batch.",
						"\tFailed to unpack KRC Interchange In001. Max retry attempts 0 Reached.",
						"\tFailed to unpack KRC Interchange In002. Max retry attempts 0 Reached.",
						"\tFailed to unpack KRC Interchange In003. Max retry attempts 0 Reached.",
						"\tNo KRC interchange(s) to deal with."
					}, logger.UserLogStrings);
			}
		}

		public void TestProcessCancellation()
		{
			SetupDataForTesting();

			var logger = new LoggingInformation();
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();
			var processor = new InterchangeUnpackProcessorForTest(logger, "KRC", new KRCInterchangeUnpacker(), cancellationTokenSource.Token, "ProcessInBatch");

			AssertExceptionThrown<OperationCanceledException>("The operation was canceled.", processor.Process);
		}

		void SetupDataForTesting()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var krBranch1 = company.Branches.AddNew();
			krBranch1.GB_RL_NKHomePort = "KRSOL";
			krBranch1.GB_Code = "KR1";
			krBranch1.GB_BranchName = "KR1 Name";
			var krBranch2 = company.Branches.AddNew();
			krBranch2.GB_RL_NKHomePort = "KRSO2";
			krBranch2.GB_Code = "KR2";
			krBranch1.GB_BranchName = "KR2 Name";
			Factory.Save();

			(outgoingEDIMessage1, interchange1) = CreateInterchange("KRC", krBranch1.PK, "In001");
			(outgoingEDIMessage2, interchange2) = CreateInterchange("KRC", krBranch1.PK, "In002");
			(outgoingEDIMessage3, interchange3) = CreateInterchange("KRC", krBranch2.PK, "In003");
			Factory.Save();
		}
		EDIInterchange interchange1, interchange2, interchange3;
		EDIMessage outgoingEDIMessage1, outgoingEDIMessage2, outgoingEDIMessage3;

		(EDIMessage, EDIInterchange) CreateInterchange(ZString applicationCode, ZGuid branchPK, ZString interchangeNum, bool hasOutgoingEDIMessage = true, bool hasOutgoingEDIInterchange = true)
		{
			EDIInterchange outgoingInterchange = null;
			EDIMessage outgoingEDIMessage = null;
			if (hasOutgoingEDIInterchange)
			{
				outgoingInterchange = Factory.New<EDIInterchange>();
				outgoingInterchange.EI_InterchangeNum = interchangeNum + "_OutInterchange";
				outgoingInterchange.EI_ApplicationCode = applicationCode;
				outgoingInterchange.EI_InterchangeType = "ZZZ";
				outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
				outgoingInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
				outgoingInterchange.EI_SessionGUID = ZGuid.NewZGuid();
				outgoingInterchange.EI_GB = branchPK;
				outgoingInterchange.EI_BodyText = "outgoingInterchange";

				if (hasOutgoingEDIMessage)
				{
					outgoingEDIMessage = Factory.New<EDIMessage>();
					outgoingEDIMessage.EM_ApplicationCode = applicationCode;
					outgoingEDIMessage.EM_MessageType = "ZZZ";
					outgoingEDIMessage.EM_GB = branchPK;
					outgoingEDIMessage.MessageNumberStrategy = new TestMessageNumberStrategy(interchangeNum + "_OutMsg");
					outgoingEDIMessage.EM_LinkTable = interchangeNum + "testLinkTable";
					outgoingEDIMessage.EM_LinkUniqueID = ZGuid.NewZGuid();
					outgoingEDIMessage.EM_EI = outgoingInterchange.PK;
					outgoingEDIMessage.EM_Status = EDIMessage.Status.Sent;
				}
			}

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = interchangeNum;
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_InterchangeType = "ZZZ";
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Receive;
			interchange.EI_Status = EDIMessage.Status.Queued;
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_SessionGUID = hasOutgoingEDIInterchange ? outgoingInterchange.EI_SessionGUID : ZGuid.NewZGuid();
			interchange.EI_GB = branchPK;
			interchange.EI_BodyText = "interchange";
			return (outgoingEDIMessage, interchange);
		}

		void AssertMessage(BusinessObjectFactory factory, EDIInterchange interchange, bool hasMessage = true, EDIMessage outgoingEDIMessage = null)
		{
			var message = factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			if (hasMessage)
			{
				AssertEquals("EM_MessageNum", interchange.EI_InterchangeNum, message.EM_MessageNum);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_ApplicationCode", interchange.EI_ApplicationCode, message.EM_ApplicationCode);
				AssertEquals("EM_GB", interchange.EI_GB, message.EM_GB);
				AssertEquals("EM_LinkTable", outgoingEDIMessage != null ? outgoingEDIMessage.EM_LinkTable : "", message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", outgoingEDIMessage != null ? outgoingEDIMessage.EM_LinkUniqueID : ZGuid.Empty, message.EM_LinkUniqueID);
				AssertEquals("EM_LinkUniqueID", outgoingEDIMessage != null ? outgoingEDIMessage.PK : ZGuid.Empty, message.EM_EM_RequestMessage);
			}
			else
			{
				AssertEquals("No unpacking message", null, message);
			}
		}

		void AssertInterchange(BusinessObjectFactory factory, EDIInterchange interchange, ZString status, int retryCount)
		{
			var result = factory.Load<EDIInterchange>(interchange.PK);
			AssertEquals("EI_Status", status, result.EI_Status);
			AssertEquals("EI_RetryCount", retryCount, result.EI_RetryCount);
		}

		public class InterchangeUnpackProcessorForTest : UCUProcessor
		{
			public InterchangeUnpackProcessorForTest(LoggingInformation logger, string applicationCode, IUniversalCustomsInterchangeUnpacker interchangeUnpacker, CancellationToken token, ZString howToProcess)
							: base(logger, applicationCode, interchangeUnpacker, token)
			{
				this.howToProcess = howToProcess;

				if (howToProcess == "RecordFactorySaveCount")
				{
					this.Factory_Exposed = new BusinessObjectFactory();
				}
			}
			readonly ZString howToProcess;

			public BusinessObjectFactory Factory_Exposed { get; private set; }

			protected override BusinessObjectFactory GetFactory(string description)
			{
				switch (description)
				{
					case "Interchanges and Messages Saving in batch":
						if (howToProcess == "ProcessSeparately" || howToProcess == "ProcessError")
						{
							var factory = new BusinessObjectFactory();
							factory.Saving += (f) => throw new ZSaveException(new ZDataException(new ApplicationException("I am testing for failing in saving in batch"), null, null), f);
							return factory;
						}

						if (howToProcess == "RecordFactorySaveCount")
						{
							return Factory_Exposed;
						}

						return base.GetFactory(description);
					case "Interchange and Message(s) Saving Separately":
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

		public class KRCInterchangeUnpacker(ZString error) : IUniversalCustomsInterchangeUnpacker
		{
			public KRCInterchangeUnpacker() : this(ZString.Empty)
			{ }

			readonly ZString error = error;

			public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger)
			{
				if (error == "Exception")
				{
					throw new Exception("Unexpected exception during Unpacking.");
				}

				if (!error.IsEmpty)
				{
					logger.Log($"KR inner unpack error: {error}");
					return new EDIInterchangeUnpackerResult(error);
				}

				var message = interchange.Factory.New<EDIMessage>();
				message.EM_ApplicationCode = interchange.EI_ApplicationCode;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_Status = EDIMessage.Status.Queued;
				message.EM_MessageNum = interchange.EI_InterchangeNum;
				message.EM_EI = interchange.PK;

				return new EDIInterchangeUnpackerResult(new[] { message });
			}
		}
	}
}
