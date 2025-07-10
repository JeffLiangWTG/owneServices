using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.eManifest.ServiceTasks.Testing
{
	[TestedType(typeof(InterchangeComposerServiceTask))]
	sealed class InterchangeComposerServiceTaskTest : ServiceTaskExtensionsTest<InterchangeComposerServiceTask>
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
			MessagingTestHelper.GetTransmitEDIMessage(Factory, EDIMessage.ApplicationCodes.USeManifest);
		}

		protected override string ProcessedLog()
		{
			return "1 message(s) prepared for sending";
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs eManifest messages outbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USeManifest,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
