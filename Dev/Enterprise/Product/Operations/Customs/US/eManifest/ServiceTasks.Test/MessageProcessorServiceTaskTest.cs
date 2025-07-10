using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorServiceTask))]
	sealed class MessageProcessorServiceTaskTest : ServiceTaskExtensionsTest<MessageProcessorServiceTask>
	{
		public void TestCanRunInAnyBranch()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("60Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override void PrepareMessagesToBeProcessed()
		{
			const string InterchangeString = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+1778++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+1778+UN+D:03B
UNH+1778+CUSRES:D:03B:UN
BGM+132:::ST+LOCK2453892GK011
DTM+132:200611302300:203
TDT+11++03+:::TR+LOCK+I++:146::16ABB43764376
TDT+11+++++++:274::11223344
LOC+60+3004
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+10+1778
UNE+1+1778
UNZ+1+1778
";
			MessagingTestHelper.CreateMessage(Factory, InterchangeString);
		}

		protected override string ProcessedLog()
		{
			return "1 message processed";
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs eManifest messages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USeManifest,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
