using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	[TestedType(typeof(TelematicsDataProcessingServiceTask))]
	class TelematicsDataProcessingServiceTaskTests : ServiceTaskTestCase<TelematicsDataProcessingServiceTask>
	{
		public void TestCode()
		{
			AssertEquals("Telematics service task have code of \"TEL\"", "TEL", TelematicsDataProcessingServiceTask.Code);
		}

		public void TestHostedServiceAttributeParameters()
		{
			// Arrange
			var attributes = typeof(TelematicsDataProcessingServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>();

			// Act
			var result = attributes.Single(attribute => attribute.Code == TelematicsDataProcessingServiceTask.Code);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("TEL", result.Category);
				AssertEquals(typeof(TelematicsDataProcessingServiceTask), result.Type);
			});
		}

		public void TestMinimumPeriod()
		{
			var minimumPeriod = typeof(TelematicsDataProcessingServiceTask).Assembly
				.GetCustomAttributes(typeof(HostedServiceAttribute), false)
				.Cast<HostedServiceAttribute>()
				.SingleOrDefault(attribute => attribute.Code == TelematicsDataProcessingServiceTask.Code)
				?.MinimumPeriod;
			AssertEquals("1second", minimumPeriod);
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			var result = thisClassAttribute.CanRunInAnyBranch;
			AssertEquals("This service task should be able to be run in any branch.", true, result);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Protobuf messages inbound",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.Telematics,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + TelematicsMessageList.Codes.ProtobufData,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Telematics XML messages inbound",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.Telematics,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + TelematicsMessageList.Codes.TelematicsXmlData,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y"),
				};
			}
		}
	}
}
