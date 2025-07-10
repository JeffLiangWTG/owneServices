using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Messaging.MessageProcessors.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

[assembly: HostedService("#@1",
	"#@1 Testing",
	"TST",
	typeof(Enterprise.Customs.ServiceTasks.Testing.BranchMessageProcessorServiceTestHelper),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("#@1",
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=#@1"
	},
	"TEST CUSTOMS NUDGING"
	)]

[assembly: HostedService("#T2",
	"#T2 Testing NoAppCode",
	"TST",
	typeof(Enterprise.Customs.ServiceTasks.Testing.BranchMessageProcessorServiceNoAppCodeTestHelper),
	CanRunInAnyBranch = true,
	MinimumPeriod = "15Minutes",
	DefaultScheduleRunEvery = "15minutes"
	)]
[assembly: HostedServiceBusinessObjectBinding("#T2",
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=#T2"
	},
	"TEST CUSTOMS NUDGING 2"
	)]

namespace Enterprise.Customs.ServiceTasks.Testing
{
	[TestsSubclassesOf(typeof(BranchMessageProcessorService))]
	public abstract class BranchMessageProcessorServiceTest<T> : ServiceTaskTestCase<T> where T : BranchMessageProcessorService
	{
		public void TestEndToEnd()
		{
			var testData = SetupDataForTesting();
			Factory.Save();
			ErrorReporter.Clear();
			var serviceTask = CreateServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();
			AssertResult(new BusinessObjectFactory(), testData, serviceTask);
		}

		protected virtual void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, T serviceTask)
		{
			var message = factory.Load<EDIMessage>(testData.MessagePK);
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
		}

		protected abstract T CreateServiceTask();

		protected abstract BranchMessageProcessorServiceTestHelperData SetupDataForTesting();
	}

	[TestedType(typeof(BranchMessageProcessorServiceTestHelper))]
	class BranchMessageProcessorServiceTest : BranchMessageProcessorServiceTest<BranchMessageProcessorServiceTestHelper>
	{
		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var message1 = CreateMessage(BranchMessageProcessorServiceTestHelper.ApplicationCodeForTesting1, BranchMessageProcessorServiceTestHelper.MessageTypeForTesting2, auBranch1.PK);
			var message2 = CreateMessage(BranchMessageProcessorServiceTestHelper.ApplicationCodeForTesting2, BranchMessageProcessorServiceTestHelper.MessageTypeForTesting1, auBranch3.PK);
			var message3 = CreateMessage("K#$", BranchMessageProcessorServiceTestHelper.MessageTypeForTesting2, nzBranch1.PK);
			var message4 = CreateMessage(BranchMessageProcessorServiceTestHelper.ApplicationCodeForTesting1, "KD$", auBranch2.PK);
			return new TestData() { MessagePK = message1.PK, Message2PK = message2.PK, Message3PK = message3.PK, Message4PK = message4.PK };
		}

		class TestData : BranchMessageProcessorServiceTestHelperData
		{
			public ZGuid Message2PK;
			public ZGuid Message3PK;
			public ZGuid Message4PK;
		}

		protected override BranchMessageProcessorServiceTestHelper CreateServiceTask() => new BranchMessageProcessorServiceTestHelper(nzBranch2.PK);

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testDataBase, BranchMessageProcessorServiceTestHelper serviceTask)
		{
			var testData = (TestData)testDataBase;
			AssertMessage(factory.Load<EDIMessage>(testData.MessagePK), EDIMessage.Status.Received, "PROCESSED", nzBranch2.PK);
			AssertMessage(factory.Load<EDIMessage>(testData.Message2PK), EDIMessage.Status.Received, "PROCESSED", auBranch3.PK);
			AssertMessage(factory.Load<EDIMessage>(testData.Message3PK), EDIMessage.Status.Queued, "", nzBranch1.PK);
			AssertMessage(factory.Load<EDIMessage>(testData.Message4PK), EDIMessage.Status.Queued, "", auBranch2.PK);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"TEST CUSTOMS NUDGING",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=#@1"),
				};
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			nzCompany = Factory.New<GlbCompany>();
			nzCompany.FillWithValidTestData();
			nzCompany.GC_Code = "NZ1";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			nzBranch1 = nzCompany.Branches.AddNew();
			nzBranch1.GB_RL_NKHomePort = "NZAKL";
			nzBranch1.GB_Code = "NZ1";
			nzBranch2 = nzCompany.Branches.AddNew();
			nzBranch2.GB_RL_NKHomePort = "NZAKL";
			nzBranch2.GB_Code = "NZ2";
			auCompany = Factory.New<GlbCompany>();
			auCompany.FillWithValidTestData();
			auCompany.GC_Code = "AU2";
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			auBranch1 = auCompany.Branches.AddNew();
			auBranch1.GB_RL_NKHomePort = "AUSYD";
			auBranch1.GB_Code = "AU1";
			auBranch2 = auCompany.Branches.AddNew();
			auBranch2.GB_RL_NKHomePort = "AUSYD";
			auBranch2.GB_Code = "AU2";
			auBranch3 = auCompany.Branches.AddNew();
			auBranch3.GB_RL_NKHomePort = "AUSYD";
			auBranch3.GB_Code = "AU3";
			Factory.Save();
		}
		GlbCompany nzCompany;
		GlbBranch nzBranch1;
		GlbBranch nzBranch2;
		GlbCompany auCompany;
		GlbBranch auBranch1;
		GlbBranch auBranch2;
		GlbBranch auBranch3;

		void AssertMessage(EDIMessage message, ZString status, ZString applicationReference, ZGuid branchPK)
		{
			AssertEquals("EM_Status", status, message.EM_Status);
			AssertEquals("EM_ApplicationReference", applicationReference, message.EM_ApplicationReference);
			AssertEquals("EM_GB", branchPK, message.EM_GB);
		}

		EDIMessage CreateMessage(ZString applicationCode, ZString messageType, ZGuid branchPK)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = messageType;
			message.EM_GB = branchPK;
			return message;
		}
	}

	public class BranchMessageProcessorServiceTestHelperData
	{
		public ZGuid MessagePK;
	}

	class BranchMessageProcessorServiceTestHelper : BranchMessageProcessorService
	{
		public BranchMessageProcessorServiceTestHelper()
			: this(GlbBranch.CurrentBranch?.PK ?? ZGuid.Empty)
		{
		}

		public BranchMessageProcessorServiceTestHelper(ZGuid branchPK)
		{
			this.branchPK = branchPK;
		}
		readonly ZGuid branchPK;

		public const string MessageTypeForTesting1 = "#$1";
		public const string MessageTypeForTesting2 = "#$2";

		public const string ApplicationCodeForTesting1 = "#@1";
		public const string ApplicationCodeForTesting2 = "#@2";

		protected override IEnumerable<ZString> MessageTypes => new ZString[] { MessageTypeForTesting1, MessageTypeForTesting2 };

		protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { ApplicationCodeForTesting1, ApplicationCodeForTesting2 };

		public List<ZGuid> ProcessedBranchPKs => processedBranchPKs ?? (processedBranchPKs = new List<ZGuid>());
		List<ZGuid> processedBranchPKs;

		protected override void Process(CancellationToken token)
		{
			ProcessedBranchPKs.Add(GlbBranch.CurrentBranch.PK);
			base.Process(token);
		}

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new BranchCustomsMessageProcessorTestHelper(branchPK, ApplicationCodes, MessageTypes);
	}

	[TestedType(typeof(BranchMessageProcessorServiceNoAppCodeTestHelper))]
	class BranchMessageProcessorServiceNoAppCodeTest : BranchMessageProcessorServiceTest<BranchMessageProcessorServiceNoAppCodeTestHelper>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes =>
		[
			new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"TEST CUSTOMS NUDGING 2",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=#T2"),
		];

		protected override BranchMessageProcessorServiceNoAppCodeTestHelper CreateServiceTask() => new();
		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting() => new();
		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, BranchMessageProcessorServiceNoAppCodeTestHelper serviceTask)
		{
			Assert("Succeeded without error", true);
		}
	}
	class BranchMessageProcessorServiceNoAppCodeTestHelper : BranchMessageProcessorService
	{
		protected override IEnumerable<ZString> MessageTypes => [];

		protected override IEnumerable<ZString> ApplicationCodes => [];

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new BranchCustomsMessageProcessorTestHelper(GlbBranch.CurrentBranch.PK, ApplicationCodes, MessageTypes);
	}

	class BranchCustomsMessageProcessorTestHelper : BranchCustomsMessageProcessor
	{
		public BranchCustomsMessageProcessorTestHelper(ZGuid branchPK, IEnumerable<ZString> applicationCodes, IEnumerable<ZString> messageTypes)
			: base(applicationCodes, messageTypes)
		{
			this.branchPK = branchPK;
		}
		readonly ZGuid branchPK;

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message) => new ApplicationTypeMessageProcessorTestHelper(GlbBranch.CurrentBranch.GB_Code.EndsWith("1"), branchPK, Logger, message.EM_ApplicationCode);
	}

	class ApplicationTypeMessageProcessorTestHelper : BranchCustomsApplicationTypeMessageProcessorTestHelper
	{
		public ApplicationTypeMessageProcessorTestHelper(bool requiresPreProcessing, ZGuid branchPK, LoggingInformation logger, ZString applicationCode)
			: base(logger)
		{
			this.requiresPreProcessing = requiresPreProcessing;
			this.branchPK = branchPK;
			this.applicationCode = applicationCode;
		}
		readonly bool requiresPreProcessing;
		readonly ZGuid branchPK;
		readonly ZString applicationCode;

		protected override bool RequiresPreProcessingCore => requiresPreProcessing;

		protected override string MessageFriendlyNameCore => "TEST";

		protected override string ApplicationCodeCore => applicationCode;

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			base.PreProcessMessageCore(message);
			message.EM_GB = branchPK;
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			message.EM_ApplicationReference = "PROCESSED";
			message.EM_Status = EDIMessage.Status.Received;
		}
	}
}
