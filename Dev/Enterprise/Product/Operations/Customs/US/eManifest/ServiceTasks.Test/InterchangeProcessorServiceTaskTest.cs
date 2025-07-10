using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.eManifest.Messaging.Interchange.Testing;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.ServiceTasks.Testing
{
	[TestedType(typeof(InterchangeProcessorServiceTask))]
	sealed class InterchangeProcessorServiceTaskTest : ServiceTaskExtensionsTest<InterchangeProcessorServiceTask>
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
			MessagingTestHelper.GetReceivedInterchange(Factory, InterchangeProcessorTest.ManifestInterchangeText.Replace("\r\n", "'"));
		}

		protected override string ProcessedLog()
		{
			return "Interchange '1' has been processed successfully.";
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"US Customs eManifest interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.USeManifest),
				};
			}
		}
	}
}
