using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(UCUServiceTask))]
	sealed class UCUServiceTaskTest : ServiceTaskTestCase<UCUServiceTask>
	{
		public void TestRunTask_Process()
		{
			SetupDataForTesting();

			using (SetupUniversalCustomsInterchangeUnpackers())
			{
				var logger = new TestServiceLogger();
				var serviceTask = new UCUServiceTaskForTest(logger);
				serviceTask.RunTask();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, interchange1);
				AssertMessage(newFactory, interchange2);
				AssertMessage(newFactory, interchange3);
				AssertMessage(newFactory, interchange4);
				AssertInterchange(newFactory, interchange1, EDIInterchange.Status.Received, 0);
				AssertInterchange(newFactory, interchange2, EDIInterchange.Status.Received, 0);
				AssertInterchange(newFactory, interchange3, EDIInterchange.Status.Received, 0);
				AssertInterchange(newFactory, interchange4, EDIInterchange.Status.Received, 0);

				var logs = logger.ToString();
				Assert(logs.Contains("Information|UCU Service Task start"));
				Assert(logs.Contains("Information|Lock ITH to process."));
				Assert(logs.Contains("Information|Start to process 1 ITH interchange(s) for EDI/IT1."));
				Assert(logs.Contains("Information|1 ITH interchange(s) and unpacking message(s) saved in batch."));
				Assert(logs.Contains("Information|No ITH interchange(s) to deal with."));
				Assert(logs.Contains("Information|Lock KRC to process."));
				Assert(logs.Contains("Information|Start to process 2 KRC interchange(s) for EDI/KR1."));
				Assert(logs.Contains("Information|2 KRC interchange(s) and unpacking message(s) saved in batch."));
				Assert(logs.Contains("Information|Start to process 1 KRC interchange(s) for EDI/KR2."));
				Assert(logs.Contains("Information|1 KRC interchange(s) and unpacking message(s) saved in batch."));
				Assert(logs.Contains("Information|No KRC interchange(s) to deal with."));
			}
		}

		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		public void TestRunTask_MultipleRunners()
		{
			SqlApplicationLock sqlAppLock1 = null;
			SqlApplicationLock sqlAppLock2 = null;
			var connection = Db.Connection;
			using (RunNonTransactioned())
			using (new DisposableAction(() =>
			{
				AssertEquals("Lock ITH", true, connection.TryGetLock("ITH", out sqlAppLock1));
				AssertEquals("Lock KRC", true, connection.TryGetLock("KRC", out sqlAppLock2));
			}, () =>
			{
				sqlAppLock1?.Dispose();
				sqlAppLock2?.Dispose();
			}))
			using (CustomsDataRegistry.Instance.UCUInterchangesPerBatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (SetupUniversalCustomsInterchangeUnpackers())
			{
				SetupDataForTesting();
				var logger1 = new TestServiceLogger();
				var logger2 = new TestServiceLogger();
				var task1 = RunTaskInNewInstance(logger1);
				Thread.Sleep(TimeSpan.FromMilliseconds(10));
				var task2 = RunTaskInNewInstance(logger2);

				task1.Wait();
				task2.Wait();

				var newFactory = new BusinessObjectFactory();
				AssertMessage(newFactory, interchange1);
				AssertMessage(newFactory, interchange2);
				AssertMessage(newFactory, interchange3);
				AssertInterchange(newFactory, interchange1, EDIInterchange.Status.Received, 0);
				AssertInterchange(newFactory, interchange2, EDIInterchange.Status.Received, 0);
				AssertInterchange(newFactory, interchange3, EDIInterchange.Status.Received, 0);

				var logger1Str = logger1.ToString();
				var logger2Str = logger2.ToString();

				if (logger1Str.Contains("Information|Start to process 1 ITH interchange(s) for EDI/IT1."))
				{
					Assert(logger2Str.Contains("Information|Start to process 2 KRC interchange(s) for EDI/KR1."));
					Assert(logger2Str.Contains("Information|Start to process 1 KRC interchange(s) for EDI/KR2."));
				}
				else
				{
					Assert(logger1Str.Contains("Information|Start to process 2 KRC interchange(s) for EDI/KR1."));
					Assert(logger1Str.Contains("Information|Start to process 1 KRC interchange(s) for EDI/KR2."));
					Assert(logger2Str.Contains("Information|Start to process 1 ITH interchange(s) for EDI/IT1."));
				}
			}
		}

		IDisposable SetupUniversalCustomsInterchangeUnpackers()
		{
			return new UCUInterchangeUnpackersRegistrationSubstitute(new (string ApplicationCode, IUniversalCustomsInterchangeUnpacker Unpacker)[]
			{
				("ITH", new ITHInterchangeUnpacker()),
				("KRC", new UCUProcessorTest.KRCInterchangeUnpacker())
			});
		}

		Task RunTaskInNewInstance(TestServiceLogger logger)
		{
			var runner = new UCUServiceTaskForTest(logger);

			return Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					runner.RunTask();
				}
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void SetupDataForTesting()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var krBranch1 = company.Branches.AddNew();
			krBranch1.GB_RL_NKHomePort = "KRSOL";
			krBranch1.GB_Code = "KR1";
			var krBranch2 = company.Branches.AddNew();
			krBranch2.GB_RL_NKHomePort = "KRSO2";
			krBranch2.GB_Code = "KR2";
			var itBranch1 = company.Branches.AddNew();
			itBranch1.GB_RL_NKHomePort = "ITPAR";
			itBranch1.GB_Code = "IT1";
			Factory.Save();

			interchange1 = CreateInterchange("ITH", itBranch1.PK, "In001");
			interchange2 = CreateInterchange("KRC", krBranch1.PK, "In002");
			interchange3 = CreateInterchange("KRC", krBranch1.PK, "In003");
			interchange4 = CreateInterchange("KRC", krBranch2.PK, "In004");
			Factory.Save();
		}
		EDIInterchange interchange1, interchange2, interchange3, interchange4;

		EDIInterchange CreateInterchange(ZString applicationCode, ZGuid branchPK, ZString interchangeNum)
		{
			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = interchangeNum + "_Out";
			outgoingInterchange.EI_ApplicationCode = applicationCode;
			outgoingInterchange.EI_InterchangeType = "ZZZ";
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			outgoingInterchange.EI_SessionGUID = ZGuid.NewZGuid();
			outgoingInterchange.EI_GB = branchPK;
			outgoingInterchange.EI_BodyText = "outgoingInterchange";

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = interchangeNum;
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Receive;
			interchange.EI_Status = EDIMessage.Status.Queued;
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_SessionGUID = outgoingInterchange.EI_SessionGUID;
			interchange.EI_GB = branchPK;
			interchange.EI_BodyText = "interchange";
			return interchange;
		}

		void AssertMessage(BusinessObjectFactory factory, EDIInterchange interchange, bool hasMessage = true)
		{
			var message = factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			if (hasMessage)
			{
				AssertEquals("EM_MessageNum", interchange.EI_InterchangeNum, message.EM_MessageNum);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_ApplicationCode", interchange.EI_ApplicationCode, message.EM_ApplicationCode);
				AssertEquals("EM_GB", interchange.EI_GB, message.EM_GB);
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

		class UCUServiceTaskForTest : UCUServiceTask
		{
			public UCUServiceTaskForTest(TestServiceLogger logger)
			{
				this.ServiceLogger = logger;
			}

			protected override IUniversalCustomsMessagingInterchangeProcessor GetProcessor(LoggingInformation logger, string applicationCode, IUniversalCustomsInterchangeUnpacker interchangeUnpacker, CancellationToken token)
			{
				logger.Log($"Lock {applicationCode} to process.");
				Task.Delay(2000).Wait();
				return new UCUProcessor(logger, applicationCode, interchangeUnpacker, token);
			}
		}

		class ITHInterchangeUnpacker : IUniversalCustomsInterchangeUnpacker
		{
			public ITHInterchangeUnpacker() { }

			public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger)
			{
				var message = interchange.Factory.New<EDIMessage>();
				message.EM_ApplicationCode = interchange.EI_ApplicationCode;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_Status = EDIMessage.Status.Queued;
				message.EM_MessageNum = interchange.EI_InterchangeNum;
				message.EM_EI = interchange.PK;
				message.EM_GB = ZGuid.Invalid;

				return new EDIInterchangeUnpackerResult(new[] { message });
			}
		}
	}
}
